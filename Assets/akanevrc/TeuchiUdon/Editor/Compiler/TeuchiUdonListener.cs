using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    using static TeuchiUdonParser;

    public class TeuchiUdonListener : TeuchiUdonParserBaseListener
    {
        private TeuchiUdonParser Parser { get; }

        public TeuchiUdonListener(TeuchiUdonParser parser)
        {
            Parser = parser;
        }

        public override void ExitTarget([NotNull] TargetContext context)
        {
            TeuchiUdonAssemblyWriter.Instance.PushDataPart(context.body().result.GetAssemblyDataPart(), TeuchiUdonTables.Instance.GetAssemblyDataPart());
            TeuchiUdonAssemblyWriter.Instance.PushCodePart(TeuchiUdonTables.Instance.GetAssemblyCodePart(), context.body().result.GetAssemblyCodePart());
            TeuchiUdonAssemblyWriter.Instance.Prepare();
            TeuchiUdonAssemblyWriter.Instance.WriteAll(Parser.Output);
        }

        public override void EnterBody([NotNull] BodyContext context)
        {
            TeuchiUdonQualifierStack.Instance.Push(TeuchiUdonQualifier.Top);
        }

        public override void ExitBody([NotNull] BodyContext context)
        {
            var topStatements = context.topStatement().Select(x => x.result).ToArray();
            context.result    = new BodyResult(context.Start, topStatements);
        }

        public override void ExitVarBindTopStatement([NotNull] VarBindTopStatementContext context)
        {
            var varBind              = context.varBind().result;
            var attrs                = context.varAttr().Select(x => x.result);
            var (init, export, sync) = ExtractFromVarAttrs(attrs);

            if (varBind.Vars.Length != 1 && (init != null || export || sync != TeuchiUdonSyncMode.Disable))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"tuple cannot be specified with any attributes");
            }
            else if (varBind.Vars.Length == 1 && varBind.Vars[0].Type.TypeNameEquals(TeuchiUdonType.Func))
            {
                if (init != null)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"function cannot be specified with @init");
                }
                if (sync != TeuchiUdonSyncMode.Disable)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"function cannot be specified with @sync, @linear, or @smooth");
                }
            }
            else
            {
                if (init == null)
                {
                    init = "_start";
                }
            }

            context.result = new TopBindResult(context.Start, varBind, init, export, sync);
        }

        private (string init, bool export, TeuchiUdonSyncMode sync) ExtractFromVarAttrs(IEnumerable<VarAttrResult> attrs)
        {
            var init   = (string)null;
            var export = false;
            var sync   = TeuchiUdonSyncMode.Disable;

            foreach (var attr in attrs)
            {
                if (attr is InitVarAttrResult initAttr)
                {
                    if (init == null)
                    {
                        init = initAttr.Identifier.Name;
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(initAttr.Token, $"multiple @init detected");
                    }
                }
                else if (attr is ExportVarAttrResult exportAttr)
                {
                    if (!export)
                    {
                        export = true;
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(exportAttr.Token, $"multiple @export detected");
                    }
                }
                else if (attr is SyncVarAttrResult syncAttr)
                {
                    if (sync == TeuchiUdonSyncMode.Disable)
                    {
                        sync = syncAttr.Mode;
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(syncAttr.Token, $"multiple @sync, @linear, or @smooth detected");
                    }
                }
            }

            return (init, export, sync);
        }

        public override void ExitExprTopStatement([NotNull] ExprTopStatementContext context)
        {
            var expr  = context.expr().result;
            var attrs = context.exprAttr().Select(x => x.result);
            var init  = ExtractFromExprAttrs(attrs);

            if (init == null)
            {
                init = "_start";
            }

            context.result = new TopExprResult(context.Start, expr, init);
        }

        private string ExtractFromExprAttrs(IEnumerable<ExprAttrResult> attrs)
        {
            var init = (string)null;

            foreach (var attr in attrs)
            {
                if (attr is InitExprAttrResult initAttr)
                {
                    if (init == null)
                    {
                        init = initAttr.Identifier.Name;
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(initAttr.Token, $"multiple @init detected");
                    }
                }
            }

            return init;
        }

        public override void ExitInitVarAttr([NotNull] InitVarAttrContext context)
        {
            var identifier = context.identifier().result;
            context.result = new InitVarAttrResult(context.Start, identifier);
        }

        public override void ExitExportVarAttr([NotNull] ExportVarAttrContext context)
        {
            context.result = new ExportVarAttrResult(context.Start);
        }

        public override void ExitSyncVarAttr([NotNull] SyncVarAttrContext context)
        {
            context.result = new SyncVarAttrResult(context.Start, TeuchiUdonSyncMode.Sync);
        }

        public override void ExitLinearVarAttr([NotNull] LinearVarAttrContext context)
        {
            context.result = new SyncVarAttrResult(context.Start, TeuchiUdonSyncMode.Linear);
        }

        public override void ExitSmoothVarAttr([NotNull] SmoothVarAttrContext context)
        {
            context.result = new SyncVarAttrResult(context.Start, TeuchiUdonSyncMode.Smooth);
        }

        public override void ExitInitExprAttr([NotNull] InitExprAttrContext context)
        {
            var identifier = context.identifier().result;
            context.result = new InitExprAttrResult(context.Start, identifier);
        }

        public override void EnterVarBind([NotNull] VarBindContext context)
        {
            var scope = new TeuchiUdonScope(((SingleVarDeclContext)context.varDecl()).identifier().GetText(), TeuchiUdonScopeMode.Var);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitVarBind([NotNull] VarBindContext context)
        {
            TeuchiUdonQualifierStack.Instance.Pop();

            var varDecl = context.varDecl().result;
            var expr    = context.expr   ().result;

            var vars = (TeuchiUdonVar[])null;
            if (varDecl.Vars.Length == 1)
            {
                var v = varDecl.Vars[0];
                var t = expr.Inner.Type;
                if (v.Type.IsAssignableFrom(t))
                {
                    vars = new TeuchiUdonVar[] { new TeuchiUdonVar(v.Qualifier, v.Name, v.Type.TypeNameEquals(TeuchiUdonType.Unknown) ? t : v.Type) };
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variable");
                    vars = new TeuchiUdonVar[0];
                }
            }
            else if (varDecl.Vars.Length >= 2)
            {
                if (expr.Inner.Type.TypeNameEquals(TeuchiUdonType.Tuple))
                {
                    var vs = varDecl.Vars;
                    var ts = expr.Inner.Type.GetArgsAsTuple().ToArray();
                    if (vs.Length == ts.Length && varDecl.Types.Zip(ts, (v, t) => (v, t)).All(x => x.v.IsAssignableFrom(x.t)))
                    {
                        vars = varDecl.Vars
                            .Zip(ts, (v, t) => (v, t))
                            .Select(x => new TeuchiUdonVar(x.v.Qualifier, x.v.Name, x.v.Type.TypeNameEquals(TeuchiUdonType.Unknown) ? x.t : x.v.Type))
                            .ToArray();
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variables");
                        vars = new TeuchiUdonVar[0];
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variables");
                    vars = new TeuchiUdonVar[0];
                }
            }
            
            context.result = new VarBindResult(context.Start, vars, varDecl, expr);
        }

        public override void ExitUnitVarDecl([NotNull] UnitVarDeclContext context)
        {
            var identifiers = new IdentifierResult[0];
            var qualifieds  = new QualifiedResult [0];
            context.result  = ExitVarDecl(context.Start, identifiers, qualifieds, context.isActual);
        }

        public override void ExitSingleVarDecl([NotNull] SingleVarDeclContext context)
        {
            var identifiers = new IdentifierResult[] { context.identifier().result };
            var qualifieds  = new QualifiedResult []
            {
                context.qualified()?.result ?? new QualifiedResult(context.Start, new IdentifierResult[0], TeuchiUdonType.Unknown)
            };
            context.result = ExitVarDecl(context.Start, identifiers, qualifieds, context.isActual);
        }

        public override void ExitTupleVarDecl([NotNull] TupleVarDeclContext context)
        {
            var identifiers = context.identifier().Select(x => x .result);
            var qualifieds  = context.qualified ().Select(x => x?.result ?? new QualifiedResult(context.Start, new IdentifierResult[0], TeuchiUdonType.Unknown));
            context.result = ExitVarDecl(context.Start, identifiers, qualifieds, context.isActual);
        }

        private VarDeclResult ExitVarDecl(IToken token, IEnumerable<IdentifierResult> identifiers, IEnumerable<QualifiedResult> qualifieds, bool isActual)
        {
            var oldQual = (TeuchiUdonQualifier)null;
            if (isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var qualifier = TeuchiUdonQualifierStack.Instance.Peek();

            if (isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);

            return new VarDeclResult(token, qualifier, identifiers, qualifieds);
        }

        public override void ExitQualified([NotNull] QualifiedContext context)
        {
            var identifiers = context.identifier().Select(x => x.result).ToArray();
            var qual        = new TeuchiUdonQualifier(identifiers.Take(identifiers.Length - 1).Select(x => new TeuchiUdonScope(x.Name, TeuchiUdonScopeMode.Type)));
            var t           = new TeuchiUdonType(qual, identifiers[identifiers.Length - 1].Name);
            var type        = (TeuchiUdonType)null;
            if (TeuchiUdonTables.Instance.Types.ContainsKey(t))
            {
                type = TeuchiUdonTables.Instance.Types[t];
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"'{t}' is not defined");
                type = TeuchiUdonType.Bottom;
            }

            context.result = new QualifiedResult(context.Start, identifiers, type);
        }

        public override void ExitIdentifier([NotNull] IdentifierContext context)
        {
            var name       = context.GetText().Replace("@", "");
            context.result = new IdentifierResult(context.Start, name);
        }

        public override void ExitReturnUnitStatement([NotNull] ReturnUnitStatementContext context)
        {
            var value      = new ExprResult(context.Start, new UnitResult(context.Start));
            context.result = ExitReturnStatement(context.Start, value);
        }

        public override void ExitReturnValueStatement([NotNull] ReturnValueStatementContext context)
        {
            var value      = context.expr().result;
            context.result = ExitReturnStatement(context.Start, value);
        }

        private JumpResult ExitReturnStatement(IToken token, ExprResult value)
        {
            var scope = TeuchiUdonQualifierStack.Instance.Peek().LastScope(TeuchiUdonScopeMode.Func);
            if (scope == null)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, "no func exists for return");
            }

            var label = scope == null ? TeuchiUdonLabel.InvalidLabel : ((TeuchiUdonFunc)scope.Label).ReturnAddress;
            return new JumpResult(token, value, label);
        }

        public override void ExitContinueUnitStatement([NotNull] ContinueUnitStatementContext context)
        {
        }

        public override void ExitContinueValueStatement([NotNull] ContinueValueStatementContext context)
        {
        }

        public override void ExitBreakUnitStatement([NotNull] BreakUnitStatementContext context)
        {
        }

        public override void ExitBreakValueStatement([NotNull] BreakValueStatementContext context)
        {
        }

        public override void ExitLetBindStatement([NotNull] LetBindStatementContext context)
        {
            var varBind    = context.varBind().result;
            context.result = new LetBindResult(context.Start, varBind);
        }

        public override void ExitExprStatement([NotNull] ExprStatementContext context)
        {
            context.result = context.expr().result;
        }

        public override void EnterUnitBlockExpr([NotNull] UnitBlockExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetBlockIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var scope          = new TeuchiUdonScope(new TeuchiUdonBlock(index, qual), TeuchiUdonScopeMode.Block);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitUnitBlockExpr([NotNull] UnitBlockExprContext context)
        {
            TeuchiUdonQualifierStack.Instance.Pop();

            var type       = TeuchiUdonType.Unit;
            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var statements = context.statement().Select(x => x.result);
            var expr       = new ExprResult(context.Start, new UnitResult(context.Start));
            var block      = new BlockResult(context.Start, type, index, qual, statements, expr);
            context.result = new ExprResult(block.Token, block);
        }

        public override void EnterValueBlockExpr([NotNull] ValueBlockExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetBlockIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var scope          = new TeuchiUdonScope(new TeuchiUdonBlock(index, qual), TeuchiUdonScopeMode.Block);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitValueBlockExpr([NotNull] ValueBlockExprContext context)
        {
            TeuchiUdonQualifierStack.Instance.Pop();

            var type       = context.expr().result.Inner.Type;
            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var statements = context.statement().Select(x => x.result);
            var expr       = context.expr().result;
            var block      = new BlockResult(context.Start, type, index, qual, statements, expr);
            context.result = new ExprResult(block.Token, block);
        }

        public override void ExitParenExpr([NotNull] ParenExprContext context)
        {
            var expr       = context.expr().result;
            var type       = expr.Inner.Type;
            var paren      = new ParenResult(context.Start, type, expr);
            context.result = new ExprResult(paren.Token, paren);
        }

        public override void EnterLiteralExpr([NotNull] LiteralExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetLiteralIndex();
            context.tableIndex = index;
            var scope          = new TeuchiUdonScope(new TeuchiUdonLiteral(index), TeuchiUdonScopeMode.Literal);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitLiteralExpr([NotNull] LiteralExprContext context)
        {
            TeuchiUdonQualifierStack.Instance.Pop();

            var literal = context.literal().result;
            context.result = new ExprResult(literal.Token, literal);
        }

        public override void ExitEvalVarExpr([NotNull] EvalVarExprContext context)
        {
            var identifier = context.identifier().result;
            var evalVar = (TypedResult)null;

            if (context.Parent is AccessExprContext)
            {
                evalVar = new EvalQualifierCandidateResult(context.Start, identifier);
            }
            else
            {
                foreach (var qual in TeuchiUdonQualifierStack.Instance.Qualifiers)
                {
                    var qv = new TeuchiUdonVar(qual, identifier.Name);
                    if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                    {
                        var v    = TeuchiUdonTables.Instance.Vars[qv];
                        var type = v.Type;
                        evalVar  = new EvalVarResult(context.Start, type, v, identifier);
                    }
                    break;
                }

                if (evalVar == null)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"'{identifier.Name}' is not defined");
                    evalVar = new BottomResult(context.Start);
                }
            }

            context.result = new ExprResult(evalVar.Token, evalVar);
        }

        public override void ExitEvalUnitFuncExpr([NotNull] EvalUnitFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = new ExprResult[] { new ExprResult(context.Start, new UnitResult(context.Start)) };
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(context.Start, identifier, args, parent);
        }

        public override void ExitEvalSingleFuncExpr([NotNull] EvalSingleFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = new ExprResult[] { context.expr().result };
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(context.Start, identifier, args, parent);
        }

        public override void ExitEvalTupleFuncExpr([NotNull] EvalTupleFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = context.expr().Select(x => x.result).ToArray();
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(context.Start, identifier, args, parent);
        }

        private ExprResult ExitEvalFuncExpr(IToken token, IdentifierResult identifier, IEnumerable<ExprResult> args, RuleContext parent)
        {
            var index    = TeuchiUdonTables.Instance.GetEvalFuncIndex();
            var evalFunc = (TypedResult)null;

            if (parent is AccessExprContext)
            {
                evalFunc = new EvalMethodCandidateResult(token, identifier, args);
            }
            else
            {
                foreach (var qual in TeuchiUdonQualifierStack.Instance.Qualifiers)
                {
                    var qv = new TeuchiUdonVar(qual, identifier.Name);
                    if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                    {
                        var v         = TeuchiUdonTables.Instance.Vars[qv];
                        var type      = v.Type;
                        var argTypes  = args.Select(x => x.Inner.Type).ToArray();
                        var iType     =
                            argTypes.Length == 0 ? TeuchiUdonType.Unit :
                            argTypes.Length >= 2 ? TeuchiUdonType.Tuple.ApplyArgsAsTuple(argTypes) :
                            argTypes[0];
                        var oType     = TeuchiUdonType.Unknown;
                        if (type.IsAssignableFromFunc(TeuchiUdonType.Func.ApplyArgsAsFunc(iType, oType)))
                        {
                            var outType = type.GetArgAsFuncOutType();
                            evalFunc    = new EvalFuncResult(token, outType, index, qual, v, identifier, args);
                        }
                        else
                        {
                            if (type.TypeNameEquals(TeuchiUdonType.Func))
                            {
                                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"arguments of '{identifier.Name}' is not compatible");
                            }
                            else
                            {
                                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"'{identifier.Name}' is not a function");
                            }
                            evalFunc = new BottomResult(token);
                        }
                        break;
                    }
                }

                if (evalFunc == null)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"'{identifier.Name}' is not a function");
                    evalFunc = new BottomResult(token);
                }
            }

            return new ExprResult(evalFunc.Token, evalFunc);
        }

        public override void ExitAccessExpr([NotNull] AccessExprContext context)
        {
            var op    = ".";
            var expr1 = context.expr()[0].result;
            var expr2 = context.expr()[1].result;

            if (expr1.Inner is EvalQualifierCandidateResult qualCandidate1)
            {
                var eval = (TypedResult)null;
                foreach (var qual in TeuchiUdonQualifierStack.Instance.Qualifiers)
                {
                    var qt = new TeuchiUdonType(qual, qualCandidate1.Identifier.Name);
                    if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                    {
                        var type  = TeuchiUdonTables.Instance.Types[qt];
                        var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                        eval      = new EvalTypeResult(qualCandidate1.Token, outer, type, qualCandidate1.Identifier);
                        break;
                    }
                }
                
                var scopes  = new TeuchiUdonScope[] { new TeuchiUdonScope(new TextLabel(qualCandidate1.Identifier.Name)) };
                var newQual = new TeuchiUdonQualifier(scopes);
                if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(newQual))
                {
                    var q    = TeuchiUdonTables.Instance.Qualifiers[newQual];
                    var type = TeuchiUdonType.Qual.ApplyArgAsQual(q);
                    eval     = new EvalQualifierResult(qualCandidate1.Token, type, q, qualCandidate1.Identifier);
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(qualCandidate1.Token, $"'{qualCandidate1.Identifier.Name}' is not defined");
                    eval = new BottomResult(qualCandidate1.Token);
                }

                expr1 = new ExprResult(eval.Token, eval);
            }
            else if (expr1.Inner is EvalMethodCandidateResult methodCandidate1)
            {
                var eval = (TypedResult)null;
                foreach (var qual in TeuchiUdonQualifierStack.Instance.Qualifiers)
                {
                    var qv   = new TeuchiUdonVar(qual, methodCandidate1.Identifier.Name);
                    if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                    {
                        var v         = TeuchiUdonTables.Instance.Vars[qv];
                        var type      = v.Type;
                        var argTypes  = methodCandidate1.Args.Select(x => x.Inner.Type).ToArray();
                        var iType     =
                            argTypes.Length == 0 ? TeuchiUdonType.Unit :
                            argTypes.Length >= 2 ? TeuchiUdonType.Tuple.ApplyArgsAsTuple(argTypes) :
                            argTypes[0];
                        var oType     = TeuchiUdonType.Unknown;
                        if (type.IsAssignableFromFunc(TeuchiUdonType.Func.ApplyArgsAsFunc(iType, oType)))
                        {
                            var outType = type.GetArgAsFuncOutType();
                            var index   = TeuchiUdonTables.Instance.GetEvalFuncIndex();
                            eval        = new EvalFuncResult(methodCandidate1.Token, outType, index, qual, v, methodCandidate1.Identifier, methodCandidate1.Args);
                        }
                        else
                        {
                            if (type.TypeNameEquals(TeuchiUdonType.Func))
                            {
                                TeuchiUdonLogicalErrorHandler.Instance.ReportError(methodCandidate1.Token, $"arguments of '{methodCandidate1.Identifier.Name}' is not compatible");
                            }
                            else
                            {
                                TeuchiUdonLogicalErrorHandler.Instance.ReportError(methodCandidate1.Token, $"'{methodCandidate1.Identifier.Name}' is not a function");
                            }
                            eval = new BottomResult(methodCandidate1.Token);
                        }
                    }
                }

                expr1 = new ExprResult(eval.Token, eval);
            }

            var type1 = expr1.Inner.Type;

            if (expr2.Inner is EvalQualifierCandidateResult qualCandidate2)
            {
                var eval = (TypedResult)null;
                do
                {
                    if (type1.TypeNameEquals(TeuchiUdonType.Qual))
                    {
                        var qual = type1.GetArgAsQual();
                        var qv   = new TeuchiUdonVar(qual, qualCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                        {
                            var v    = TeuchiUdonTables.Instance.Vars[qv];
                            var type = v.Type;
                            eval     = new EvalVarResult(qualCandidate2.Token, type, v, qualCandidate2.Identifier);
                            break;
                        }

                        var qt = new TeuchiUdonType(qual, qualCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(qualCandidate2.Token, outer, type, qualCandidate2.Identifier);
                            break;
                        }

                        var scope    = new TeuchiUdonScope(new TextLabel(qualCandidate2.Identifier.Name));
                        var appended = qual.Append(scope);
                        if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(appended))
                        {
                            var q    = TeuchiUdonTables.Instance.Qualifiers[appended];
                            var type = TeuchiUdonType.Qual.ApplyArgAsQual(q);
                            eval     = new EvalQualifierResult(qualCandidate2.Token, type, q, qualCandidate2.Identifier);
                            break;
                        }
                    }
                    else if (type1.TypeNameEquals(TeuchiUdonType.Type))
                    {
                        var t  = type1.GetArgAsType();
                        var s  = new TeuchiUdonScope(new TextLabel(t.Name));
                        var q  = t.Qualifier.Append(s);
                        var qt = new TeuchiUdonType(q, qualCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(qualCandidate2.Token, outer, type, qualCandidate2.Identifier);
                            break;
                        }

                        var qm = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetGetterName(qualCandidate2.Identifier.Name), new TeuchiUdonType[0]);
                        var m  = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
                        if (m.Length == 1)
                        {
                            throw new NotImplementedException();
                            //break;
                        }
                        else if (m.Length >= 2)
                        {
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(qualCandidate2.Token, $"arguments of method '{qualCandidate2.Identifier.Name}' is nondeterministic");
                            break;
                        }
                    }
                    else
                    {
                        var qm = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetGetterName(qualCandidate2.Identifier.Name), new TeuchiUdonType[0]);
                        var m  = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
                        if (m.Length == 1)
                        {
                            throw new NotImplementedException();
                            //break;
                        }
                        else if (m.Length >= 2)
                        {
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(qualCandidate2.Token, $"arguments of method '{qualCandidate2.Identifier.Name}' is nondeterministic");
                            break;
                        }
                    }

                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(qualCandidate2.Token, $"'{qualCandidate2.Identifier.Name}' is not defined");
                    eval = new BottomResult(qualCandidate2.Token);
                } while (false);

                expr2 = new ExprResult(eval.Token, eval);
            }
            else if (expr2.Inner is EvalMethodCandidateResult methodCandidate2)
            {
                var eval = (TypedResult)null;
                do
                {
                    if (type1.TypeNameEquals(TeuchiUdonType.Qual))
                    {
                    }
                    else if (type1.TypeNameEquals(TeuchiUdonType.Type))
                    {
                        var argTypes = methodCandidate2.Args.Select(x => x.Inner.Type);
                        var qm       = new TeuchiUdonMethod(type1, methodCandidate2.Identifier.Name, argTypes);
                        var m        = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
                        if (m.Length == 1)
                        {
                            var type = m[0].OutTypes.Length == 0 ? TeuchiUdonType.Unit : m[0].OutTypes[0];
                            eval     = new EvalMethodResult(methodCandidate2.Token, type, m[0], methodCandidate2.Identifier, methodCandidate2.Args);
                            break;
                        }
                        else if (m.Length >= 2)
                        {
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(methodCandidate2.Token, $"arguments of method '{methodCandidate2.Identifier.Name}' is nondeterministic");
                            break;
                        }
                    }
                    else
                    {
                        var argTypes = methodCandidate2.Args.Select(x => x.Inner.Type);
                        var qm       = new TeuchiUdonMethod(type1, methodCandidate2.Identifier.Name, argTypes);
                        var m        = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
                        if (m.Length == 1)
                        {
                            var type = m[0].OutTypes.Length == 0 ? TeuchiUdonType.Unit : m[0].OutTypes[0];
                            eval     = new EvalMethodResult(methodCandidate2.Token, type, m[0], methodCandidate2.Identifier, methodCandidate2.Args);
                            break;
                        }
                        else if (m.Length >= 2)
                        {
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(methodCandidate2.Token, $"arguments of method '{methodCandidate2.Identifier.Name}' is nondeterministic");
                            break;
                        }
                    }

                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(methodCandidate2.Token, $"'{methodCandidate2.Identifier.Name}' is not defined");
                    eval = new BottomResult(methodCandidate2.Token);
                } while (false);

                expr2 = new ExprResult(eval.Token, eval);
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(expr1.Token, $"invalid '.' operator");
            }

            var infix = new InfixResult(expr1.Token, expr2.Inner.Type, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void EnterFuncExpr([NotNull] FuncExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetFuncIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var scope          = new TeuchiUdonScope(new TeuchiUdonFunc(index, qual), TeuchiUdonScopeMode.Func);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitFuncExpr([NotNull] FuncExprContext context)
        {
            TeuchiUdonQualifierStack.Instance.Pop();

            var varDecl   = context.varDecl().result;
            var expr      = context.expr   ().result;
            var argTypes  = varDecl.Types.Any() ? varDecl.Types : new TeuchiUdonType[] { TeuchiUdonType.Unit };
            var funcTypes = argTypes.Concat(new TeuchiUdonType[] { expr.Inner.Type });
            var inType    =
                argTypes.Length == 0 ? TeuchiUdonType.Unit :
                argTypes.Length >= 2 ? TeuchiUdonType.Tuple.ApplyArgsAsTuple(argTypes) :
                argTypes[0];
            var outType   = expr.Inner.Type;
            var type      = TeuchiUdonType.Func.ApplyArgsAsFunc(inType, outType);

            if (!(context.Parent is VarBindContext varBind))
            {
                throw new NotImplementedException();
            }

            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var func       = new FuncResult(context.Start, type, index, qual, varDecl, expr);
            context.result = new ExprResult(func.Token, func);
        }

        public override void EnterLetInBindExpr([NotNull] LetInBindExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetLetIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var scope          = new TeuchiUdonScope(new TeuchiUdonLet(index, qual), TeuchiUdonScopeMode.Let);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitLetInBindExpr([NotNull] LetInBindExprContext context)
        {
            TeuchiUdonQualifierStack.Instance.Pop();

            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var varBind    = context.varBind().result;
            var expr       = context.expr   ().result;
            var letInBind  = new LetInBindResult(context.Start, expr.Inner.Type, index, qual, varBind, expr);
            context.result = new ExprResult(letInBind.Token, letInBind);
        }

        public override void ExitIntegerLiteral([NotNull] IntegerLiteralContext context)
        {
            var text = context.GetText().Replace("_", "").ToLower();

            var index = 0;
            var count = text.Length;
            var basis = 10;
            var type  = TeuchiUdonType.Int;

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = TeuchiUdonType.ULong;
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = TeuchiUdonType.Long;
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = TeuchiUdonType.UInt;
            }

            if (count >= 2 && text.StartsWith("0"))
            {
                index++;
                count--;
                basis = 8;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue   (context.Start, type, text.Substring(index, count), basis);
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        public override void ExitHexIntegerLiteral([NotNull] HexIntegerLiteralContext context)
        {
            var text = context.GetText().Replace("_", "").ToLower();

            var index = 2;
            var count = text.Length - 2;
            var basis = 16;
            var type  = TeuchiUdonType.Int;

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = TeuchiUdonType.ULong;
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = TeuchiUdonType.Long;
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = TeuchiUdonType.UInt;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue   (context.Start, type, text.Substring(index, count), basis);
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        public override void ExitBinIntegerLiteral([NotNull] BinIntegerLiteralContext context)
        {
            var text = context.GetText().Replace("_", "").ToLower();

            var index = 2;
            var count = text.Length - 2;
            var basis = 2;
            var type  = TeuchiUdonType.Int;

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = TeuchiUdonType.ULong;
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = TeuchiUdonType.Long;
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = TeuchiUdonType.UInt;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue   (context.Start, type, text.Substring(index, count), basis);
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        public override void ExitRealLiteral([NotNull] RealLiteralContext context)
        {
        }

        public override void ExitCharacterLiteral([NotNull] CharacterLiteralContext context)
        {
        }

        public override void ExitRegularString([NotNull] RegularStringContext context)
        {
            var text       = context.GetText();
            var type       = TeuchiUdonType.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToRegularStringValue(text.Substring(1, text.Length - 2));
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        public override void ExitVervatiumString([NotNull] VervatiumStringContext context)
        {
            var text       = context.GetText();
            var type       = TeuchiUdonType.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToVervatiumStringValue(text.Substring(2, text.Length - 3));
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        private object Eval(ExprResult expr)
        {
            return
                expr.Inner is LiteralResult  literal  ? $"<literal>{literal.Literal.Value}" :
                expr.Inner is EvalVarResult  evalVar  ? $"<evalVar>{evalVar.Identifier.Name}" :
                expr.Inner is EvalFuncResult evalFunc ? $"<evalFunc>{evalFunc.Identifier.Name}({string.Join(", ", evalFunc.Args.Select(x => Eval(x)))})" :
                expr.Inner is InfixResult    infix    ? $"<infix>{Eval(infix.Expr1)}{(infix.Op == "." ? infix.Op : $" {infix.Op} ")}{Eval(infix.Expr2)}" :
                expr.Inner is FuncResult     func     ? $"<func>({string.Join(", ", func.VarDecl.Identifiers.Select(x => x.Name))}) -> {Eval(func.Expr)}" :
                "<expr>";
        }

        private object ToIntegerValue(IToken token, TeuchiUdonType type, string text, int basis)
        {
            if (type == TeuchiUdonType.Int)
            {
                try
                {
                    return Convert.ToInt32(text, basis);
                }
                catch
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to int");
                    return null;
                }
            }
            else if (type == TeuchiUdonType.UInt)
            {
                try
                {
                    return Convert.ToUInt32(text, basis);
                }
                catch
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to uint");
                    return null;
                }
            }
            else if (type == TeuchiUdonType.Long)
            {
                try
                {
                    return Convert.ToInt64(text, basis);
                }
                catch
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to long");
                    return null;
                }
            }
            else if (type == TeuchiUdonType.ULong)
            {
                try
                {
                    return Convert.ToUInt64(text, basis);
                }
                catch
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to ulong");
                    return null;
                }
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to unknown");
                return null;
            }
        }

        private object ToRegularStringValue(string text)
        {
            return text;
        }

        private object ToVervatiumStringValue(string text)
        {
            return text;
        }
    }
}
