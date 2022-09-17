using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace akanevrc.TeuchiUdon.Server
{
    public class SemanticTokensHandler : SemanticTokensHandlerBase
    {
        private record SemanticTokenAttribute
        (
            Range Range,
            SemanticTokenType Type,
            SemanticTokenModifier Modifier
        )
        {
            public static SemanticTokenAttribute? Create
            (
                IToken start,
                IToken stop,
                SemanticTokenType type,
                SemanticTokenModifier modifier
            )
            {
                if (start == null || stop == null) return null;

                return new SemanticTokenAttribute
                (
                    new Range
                    (
                        start.Line - 1,
                        start.Column,
                        stop .Line - 1,
                        stop .Column + stop.Text.Length
                    ),
                    type,
                    modifier
                );
            }
        };

        private IServiceProvider Services { get; }
        private DocumentManager DocumentManager { get; }
        private PublishDiagnosticsHandler PublishDiagnosticsHandler { get; }

        public SemanticTokensHandler
        (
            IServiceProvider services,
            DocumentManager documentManager,
            PublishDiagnosticsHandler publishDiagnosticsHandler
        )
        {
            Services                  = services;
            DocumentManager           = documentManager;
            PublishDiagnosticsHandler = publishDiagnosticsHandler;
        }

        protected override SemanticTokensRegistrationOptions CreateRegistrationOptions
        (
            SemanticTokensCapability capability,
            ClientCapabilities clientCapabilities
        )
        {
            return new()
            {
                DocumentSelector = DocumentSelector.ForLanguage("teuchiudon"),
                Legend           = new SemanticTokensLegend()
                {
                    TokenModifiers = capability.TokenModifiers,
                    TokenTypes     = capability.TokenTypes
                },
                Full = new SemanticTokensCapabilityRequestFull
                {
                    Delta = true
                },
                Range = true
            };
        }

        protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(ITextDocumentIdentifierParams request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SemanticTokensDocument(RegistrationOptions.Legend));
        }

        protected override async Task Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams request, CancellationToken cancellationToken)
        {
            var documentPath = request.TextDocument.Uri.ToString();
            var text         = DocumentManager.Get(documentPath);

            await Task.Yield();

            var (results, errors) = TeuchiUdonParserRunner.ParseFromString(Services, text);
            var attributeList = new List<SemanticTokenAttribute>();

            foreach (var res in results)
            {
                AddRangeToAttributeList(attributeList, ResultToAttribute(res));
            }

            foreach (var attr in attributeList)
            {
                builder.Push(attr.Range, attr.Type, attr.Modifier);
            }

            await PublishDiagnosticsHandler.Handle
            (
                new PublishDiagnosticsParams()
                {
                    Uri         = request.TextDocument.Uri,
                    Diagnostics = new Container<Diagnostic>(errors.Select(x => ErrorToDiagnostic(x)!).Where(x => x != null))
                },
                cancellationToken
            )
            .ConfigureAwait(false);
        }

        private IEnumerable<SemanticTokenAttribute?> ResultToAttribute(TeuchiUdonParserResult result)
        {
            switch (result)
            {
                case TargetResult:
                case BodyResult:
                case TopBindResult:
                case TopExprResult:
                    yield break;
                case PublicVarAttrResult publicVarAttr:
                    if (publicVarAttr.Keyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            publicVarAttr.Keyword.Start,
                            publicVarAttr.Keyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case SyncVarAttrResult syncVarAttr:
                    if (syncVarAttr.Keyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            syncVarAttr.Keyword.Start,
                            syncVarAttr.Keyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case KeywordResult:
                    yield break;
                case VarBindResult varBind:
                    yield break;
                case VarDeclResult:
                    yield break;
                case QualifiedVarResult qualifiedVar:
                    if (qualifiedVar.MutKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            qualifiedVar.MutKeyword.Start,
                            qualifiedVar.MutKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    if (qualifiedVar.Identifier != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            qualifiedVar.Identifier.Start,
                            qualifiedVar.Identifier.Stop,
                            SemanticTokenType.Variable,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case IdentifierResult:
                    yield break;
                case JumpResult jump:
                    if (jump.JumpKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            jump.JumpKeyword.Start,
                            jump.JumpKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case LetBindResult let:
                    if (let.LetKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            let.LetKeyword.Start,
                            let.LetKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case ExprResult:
                case InvalidResult:
                case UnknownTypeResult:
                case UnitResult:
                case BlockResult:
                case ParenResult:
                case TupleResult:
                case ArrayCtorResult:
                    yield break;
                case LiteralResult literal:
                    switch (literal.Type.LogicalName)
                    {
                        case "unit":
                        case "nulltype":
                        case "SystemBoolean":
                            yield return SemanticTokenAttribute.Create
                            (
                                literal.Start,
                                literal.Stop,
                                SemanticTokenType.Keyword,
                                SemanticTokenModifier.Definition
                            );
                            yield break;
                        case "SystemInt32":
                        case "SystemUInt32":
                        case "SystemInt64":
                        case "SystemUInt64":
                        case "SystemSingle":
                        case "SystemDouble":
                        case "SystemDecimal":
                            yield return SemanticTokenAttribute.Create
                            (
                                literal.Start,
                                literal.Stop,
                                SemanticTokenType.Number,
                                SemanticTokenModifier.Definition
                            );
                            yield break;
                        case "SystemChar":
                        case "SystemString":
                            yield return SemanticTokenAttribute.Create
                            (
                                literal.Start,
                                literal.Stop,
                                SemanticTokenType.String,
                                SemanticTokenModifier.Definition
                            );
                            yield break;
                        default:
                            yield break;
                    }
                case ThisResult this_:
                    yield return SemanticTokenAttribute.Create
                    (
                        this_.Start,
                        this_.Stop,
                        SemanticTokenType.Keyword,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case InterpolatedStringResult:
                    yield break;
                case RegularStringInterpolatedStringPartResult regularStringInterpolatedStringPart:
                    yield return SemanticTokenAttribute.Create
                    (
                        regularStringInterpolatedStringPart.Start,
                        regularStringInterpolatedStringPart.Stop,
                        SemanticTokenType.String,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case ExprInterpolatedStringPartResult:
                    yield break;
                case EvalVarResult evalVar:
                    yield return SemanticTokenAttribute.Create
                    (
                        evalVar.Start,
                        evalVar.Stop,
                        SemanticTokenType.Variable,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case EvalTypeResult evalType:
                    yield return SemanticTokenAttribute.Create
                    (
                        evalType.Start,
                        evalType.Stop,
                        SemanticTokenType.Type,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case EvalQualifierResult evalQualifier:
                    yield return SemanticTokenAttribute.Create
                    (
                        evalQualifier.Start,
                        evalQualifier.Stop,
                        SemanticTokenType.Namespace,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case EvalGetterResult evalGetter:
                    yield return SemanticTokenAttribute.Create
                    (
                        evalGetter.Start,
                        evalGetter.Stop,
                        SemanticTokenType.Property,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case EvalSetterResult evalSetter:
                    yield return SemanticTokenAttribute.Create
                    (
                        evalSetter.Start,
                        evalSetter.Stop,
                        SemanticTokenType.Property,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case EvalGetterSetterResult evalGetterSetter:
                    yield return SemanticTokenAttribute.Create
                    (
                        evalGetterSetter.Start,
                        evalGetterSetter.Stop,
                        SemanticTokenType.Property,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case EvalFuncResult evalFunc:
                    if (evalFunc.Expr?.Inner is EvalVarResult)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            evalFunc.Expr.Start,
                            evalFunc.Expr.Stop,
                            SemanticTokenType.Function,
                            SemanticTokenModifier.Definition
                        );
                    }
                    else if (evalFunc.Expr?.Inner is InfixResult childInfix && childInfix.Expr2?.Inner is EvalVarResult)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            childInfix.Expr2.Start,
                            childInfix.Expr2.Stop,
                            SemanticTokenType.Function,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case EvalSpreadFuncResult evalSpreadFunc:
                    if (evalSpreadFunc.Expr?.Inner is EvalVarResult)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            evalSpreadFunc.Expr.Start,
                            evalSpreadFunc.Expr.Stop,
                            SemanticTokenType.Function,
                            SemanticTokenModifier.Definition
                        );
                    }
                    else if (evalSpreadFunc.Expr?.Inner is InfixResult childInfix && childInfix.Expr2?.Inner is EvalVarResult)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            childInfix.Expr2.Start,
                            childInfix.Expr2.Stop,
                            SemanticTokenType.Function,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case EvalMethodResult:
                case EvalSpreadMethodResult:
                case EvalCoalescingMethodResult:
                case EvalCoalescingSpreadMethodResult:
                    yield break;
                case EvalCastResult evalCast:
                    if (evalCast.CastKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            evalCast.CastKeyword.Start,
                            evalCast.CastKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case EvalTypeOfResult evalTypeOf:
                    yield return SemanticTokenAttribute.Create
                    (
                        evalTypeOf.Start,
                        evalTypeOf.Stop,
                        SemanticTokenType.Keyword,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case EvalVarCandidateResult:
                case EvalArrayIndexerResult:
                case TypeCastResult:
                case ConvertCastResult:
                case TypeOfResult:
                case PrefixResult:
                case InfixResult:
                    yield break;
                case LetInBindResult letIn:
                    if (letIn.LetKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            letIn.LetKeyword.Start,
                            letIn.LetKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    if (letIn.InKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            letIn.InKeyword.Start,
                            letIn.InKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case IfResult if_:
                    if (if_.IfKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            if_.IfKeyword.Start,
                            if_.IfKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    foreach (var then in if_.ThenKeywords)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            then.Start,
                            then.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    foreach (var elif in if_.ElifKeywords)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            elif.Start,
                            elif.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case IfElseResult ifElse:
                    if (ifElse.IfKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            ifElse.IfKeyword.Start,
                            ifElse.IfKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    if (ifElse.ElseKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            ifElse.ElseKeyword.Start,
                            ifElse.ElseKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    foreach (var then in ifElse.ThenKeywords)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            then.Start,
                            then.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    foreach (var elif in ifElse.ElifKeywords)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            elif.Start,
                            elif.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case WhileResult while_:
                    if (while_.WhileKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            while_.WhileKeyword.Start,
                            while_.WhileKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    if (while_.DoKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            while_.DoKeyword.Start,
                            while_.DoKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case ForResult for_:
                    foreach (var f in for_.ForKeywords)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            f.Start,
                            f.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    if (for_.DoKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            for_.DoKeyword.Start,
                            for_.DoKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case LoopResult loop:
                    if (loop.LoopKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            loop.LoopKeyword.Start,
                            loop.LoopKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case FuncResult:
                    yield break;
                case MethodResult method:
                    yield return SemanticTokenAttribute.Create
                    (
                        method.Start,
                        method.Stop,
                        SemanticTokenType.Method,
                        SemanticTokenModifier.Definition
                    );
                    yield break;
                case ElementsIterExprResult:
                case RangeIterExprResult:
                case SteppedRangeIterExprResult:
                case SpreadIterExprResult:
                case IsoExprResult:
                    yield break;
                case ArgExprResult argExpr:
                    if (argExpr.RefKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            argExpr.RefKeyword.Start,
                            argExpr.RefKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case LetForBindResult letForBind:
                    if (letForBind.LetKeyword != null)
                    {
                        yield return SemanticTokenAttribute.Create
                        (
                            letForBind.LetKeyword.Start,
                            letForBind.LetKeyword.Stop,
                            SemanticTokenType.Keyword,
                            SemanticTokenModifier.Definition
                        );
                    }
                    yield break;
                case AssignForBindResult:
                    yield break;
            }
        }

        private Diagnostic? ErrorToDiagnostic(TeuchiUdonParserError error)
        {
            return error.Start == null && error.Stop == null ? null
            : error.Start != null ?
            new Diagnostic()
            {
                Range = new Range
                (
                    error.Start.Line - 1,
                    error.Start.Column,
                    error.Start.Line - 1,
                    error.Start.Column + error.Start.Text.Length
                ),
                Message  = error.Message,
                Severity = DiagnosticSeverity.Error
            }
            : error.Stop != null ?
            new Diagnostic()
            {
                Range = new Range
                (
                    error.Stop.Line - 1,
                    error.Stop.Column,
                    error.Stop.Line - 1,
                    error.Stop.Column + error.Stop.Text.Length
                ),
                Message  = error.Message,
                Severity = DiagnosticSeverity.Error
            }
            :
            new Diagnostic()
            {
                Range = new Range
                (
                    error.Start!.Line - 1,
                    error.Start!.Column,
                    error.Stop !.Line - 1,
                    error.Stop !.Column + error.Stop!.Text.Length
                ),
                Message  = error.Message,
                Severity = DiagnosticSeverity.Error
            };
        }

        private void AddRangeToAttributeList(List<SemanticTokenAttribute> list, IEnumerable<SemanticTokenAttribute?> attributes)
        {
            foreach (var attr in attributes)
            {
                if (attr == null) continue;
                
                var added = false;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (attr.Range.IsAfter(list[i].Range))
                    {
                        added = true;
                        list.Insert(i + 1, attr);
                        break;
                    }
                    else if (attr.Range.Intersects(list[i].Range))
                    {
                        list.RemoveAt(i);
                    }
                }

                if (!added)
                {
                    list.Insert(0, attr);
                }
            }
        }
    }
}
