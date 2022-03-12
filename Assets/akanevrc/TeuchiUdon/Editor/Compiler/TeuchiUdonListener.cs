using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            if (context is TopStatementContext || context is StatementContext || context is ExprContext && context.Parent is FuncExprContext)
            {
                TeuchiUdonOutValuePool.Instance.PushScope(TeuchiUdonQualifierStack.Instance.Peek().GetFuncQualifier());
            }
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            if (context is TopStatementContext || context is StatementContext || context is ExprContext && context.Parent is FuncExprContext)
            {
                TeuchiUdonOutValuePool.Instance.PopScope(TeuchiUdonQualifierStack.Instance.Peek().GetFuncQualifier());
            }
        }

        public override void ExitTarget([NotNull] TargetContext context)
        {
            var body = context.body()?.result;

            TeuchiUdonAssemblyWriter.Instance.PushDataPart
            (
                TeuchiUdonCompilerStrategy.Instance.GetDataPartFromTables(),
                TeuchiUdonCompilerStrategy.Instance.GetDataPartFromOutValuePool()
            );
            TeuchiUdonAssemblyWriter.Instance.PushCodePart
            (
                TeuchiUdonCompilerStrategy.Instance.GetCodePartFromTables(),
                TeuchiUdonCompilerStrategy.Instance.GetCodePartFromResult(body == null ? (TeuchiUdonParserResult)new UnitResult(null) : (TeuchiUdonParserResult)body)
            );
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
            if (topStatements.Any(x => x == null)) return;

            context.result = new BodyResult(context.Start, topStatements);
        }

        public override void ExitVarBindTopStatement([NotNull] VarBindTopStatementContext context)
        {
            var varBind = context.varBind()?.result;
            var attrs   = context.varAttr().Select(x => x.result);
            if (varBind == null || attrs.Any(x => x == null)) return;

            var (pub, sync) = ExtractFromVarAttrs(attrs);

            if (varBind.Vars.Length != 1 && (pub || sync != TeuchiUdonSyncMode.Disable))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"tuple cannot be specified with any attributes");
            }
            else if (varBind.Vars.Length == 1 && varBind.Vars[0].Type.LogicalTypeNameEquals(TeuchiUdonType.Func))
            {
                if (sync != TeuchiUdonSyncMode.Disable)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"function cannot be specified with @sync, @linear, or @smooth");
                }
            }
            else
            {
                if (varBind.Vars.Length == 1 && TeuchiUdonTables.Instance.Events.ContainsKey(varBind.Vars[0].Name))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"event must be function");
                }

                if (pub)
                {
                    if (varBind.Expr.Inner is LiteralResult literal)
                    {
                        TeuchiUdonTables.Instance.PublicVars.Add(varBind.Vars[0], literal.Literal);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"public valiable cannot be bound non-literal expression");
                    }
                }
                
                if (sync != TeuchiUdonSyncMode.Disable)
                {
                    TeuchiUdonTables.Instance.SyncedVars.Add(varBind.Vars[0], sync);
                }
                if (pub || sync != TeuchiUdonSyncMode.Disable)
                {
                    TeuchiUdonTables.Instance.UnbufferedVars.Add(varBind.Vars[0], varBind.Vars[0]);
                }
            }

            context.result = new TopBindResult(context.Start, varBind, pub, sync);
        }

        private (bool pub, TeuchiUdonSyncMode sync) ExtractFromVarAttrs(IEnumerable<VarAttrResult> attrs)
        {
            var pub  = false;
            var sync = TeuchiUdonSyncMode.Disable;

            foreach (var attr in attrs)
            {
                if (attr is PublicVarAttrResult publicAttr)
                {
                    if (!pub)
                    {
                        pub = true;
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(publicAttr.Token, $"multiple @public detected");
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

            return (pub, sync);
        }

        public override void ExitExprTopStatement([NotNull] ExprTopStatementContext context)
        {
            var expr  = context.expr()?.result;
            if (expr == null) return;

            expr.ReturnsValue = false;
            context.result = new TopExprResult(context.Start, expr);
        }

        public override void ExitPublicVarAttr([NotNull] PublicVarAttrContext context)
        {
            context.result = new PublicVarAttrResult(context.Start);
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

        public override void EnterVarBind([NotNull] VarBindContext context)
        {
            var varDecl = context.varDecl();
            if
            (
                varDecl == null ||
                varDecl is SingleVarDeclContext s && s.qualifiedVar()?.identifier() == null ||
                varDecl is TupleVarDeclContext  t && t.qualifiedVar().Any(x => x?.identifier() == null)
            ) return;

            var index          = TeuchiUdonTables.Instance.GetVarBindIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var varNames       =
                varDecl is SingleVarDeclContext sv ? new string[] { sv.qualifiedVar()?.identifier()?.GetText() ?? "" } :
                varDecl is TupleVarDeclContext  tv ? tv.qualifiedVar().Select(x => x?.identifier()?.GetText() ?? "").ToArray() : new string[0];
            var scope   = new TeuchiUdonScope(new TeuchiUdonVarBind(index, qual, varNames), TeuchiUdonScopeMode.VarBind);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitVarBind([NotNull] VarBindContext context)
        {
            var varDecl = context.varDecl()?.result;
            var expr    = context.expr   ()?.result;
            if (varDecl == null || expr == null) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var mut   = context.MUT() != null;
            var index = context.tableIndex;
            var qual  = TeuchiUdonQualifierStack.Instance.Peek();

            var vars = (TeuchiUdonVar[])null;
            if (varDecl.Vars.Length == 1)
            {
                var v = varDecl.Vars[0];
                var t = expr.Inner.Type;
                if (v.Type.IsAssignableFrom(t))
                {
                    vars = new TeuchiUdonVar[] { new TeuchiUdonVar(TeuchiUdonTables.Instance.GetVarIndex(), v.Qualifier, v.Name, v.Type.LogicalTypeNameEquals(TeuchiUdonType.Unknown) ? t : v.Type, mut, false) };
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variable");
                    vars = new TeuchiUdonVar[0];
                }
            }
            else if (varDecl.Vars.Length >= 2)
            {
                if (expr.Inner.Type.LogicalTypeNameEquals(TeuchiUdonType.Tuple))
                {
                    var vs = varDecl.Vars;
                    var ts = expr.Inner.Type.GetArgsAsTuple().ToArray();
                    if (vs.Length == ts.Length && varDecl.Types.Zip(ts, (v, t) => (v, t)).All(x => x.v.IsAssignableFrom(x.t)))
                    {
                        vars = varDecl.Vars
                            .Zip(ts, (v, t) => (v, t))
                            .Select(x => new TeuchiUdonVar(TeuchiUdonTables.Instance.GetVarIndex(), x.v.Qualifier, x.v.Name, x.v.Type.LogicalTypeNameEquals(TeuchiUdonType.Unknown) ? x.t : x.v.Type, mut, false))
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

            if (mut && vars.Any(x => x.Type.LogicalTypeNameEquals(TeuchiUdonType.Func)))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"function variable cannot be mutable");
            }

            context.result = new VarBindResult(context.Start, index, qual, vars, varDecl, expr);
        }

        public override void ExitUnitVarDecl([NotNull] UnitVarDeclContext context)
        {
            var qualifiedVars = new QualifiedVarResult[0];
            context.result    = ExitVarDecl(context.Start, qualifiedVars, context.isActual);
        }

        public override void ExitSingleVarDecl([NotNull] SingleVarDeclContext context)
        {
            var qualifiedVar = context.qualifiedVar()?.result;
            if (qualifiedVar == null) return;

            var qualifiedVars = new QualifiedVarResult[] { qualifiedVar };
            context.result    = ExitVarDecl(context.Start, qualifiedVars, context.isActual);
        }

        public override void ExitTupleVarDecl([NotNull] TupleVarDeclContext context)
        {
            var qualifiedVars = context.qualifiedVar().Select(x => x.result);
            if (qualifiedVars.Any(x => x == null)) return;

            context.result = ExitVarDecl(context.Start, qualifiedVars, context.isActual);
        }

        private VarDeclResult ExitVarDecl(IToken token, IEnumerable<QualifiedVarResult> qualifiedVars, bool isActual)
        {
            var oldQual = (TeuchiUdonQualifier)null;
            if (isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var qualifier = TeuchiUdonQualifierStack.Instance.Peek();

            if (isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);

            return new VarDeclResult(token, qualifier, qualifiedVars);
        }

        public override void ExitQualifiedVar([NotNull] QualifiedVarContext context)
        {
            var identifier = context.identifier()?.result;
            var qualified  = context.expr      ()?.result;
            if (identifier == null) return;

            if (qualified == null)
            {
                qualified = new ExprResult(context.Start, new UnknownTypeResult(context.Start)) { ReturnsValue = false };
            }
            else
            {
                if (!qualified.Inner.Type.LogicalTypeNameEquals(TeuchiUdonType.Type))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"qualified type {qualified.Inner.Type} is not a type");
                }
                qualified.ReturnsValue = false;
            }

            context.result = new QualifiedVarResult(context.Start, identifier, qualified);
        }

        public override void ExitIdentifier([NotNull] IdentifierContext context)
        {
            if (context.ChildCount == 0) return;

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
            var value = context.expr()?.result;
            if (value == null) return;

            context.result = ExitReturnStatement(context.Start, value);
        }

        private JumpResult ExitReturnStatement(IToken token, ExprResult value)
        {
            var scope = TeuchiUdonQualifierStack.Instance.Peek().LastScope(TeuchiUdonScopeMode.Func);
            if (scope == null)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, "no func exists for return");
            }

            var label = scope == null ? (ITypedLabel)InvalidLabel.Instance : (ITypedLabel)((TeuchiUdonFunc)scope.Label).Return;
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
            var varBind = context.varBind()?.result;
            if (varBind == null) return;

            context.result = new LetBindResult(context.Start, varBind);
        }

        public override void ExitExprStatement([NotNull] ExprStatementContext context)
        {
            var expr = context.expr()?.result;
            if (expr == null) return;

            expr.ReturnsValue = false;
            context.result    = expr;
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
            var statements = context.statement().Select(x => x.result);
            if (statements.Any(x => x == null)) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var type       = TeuchiUdonType.Unit;
            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
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
            var expr       = context.expr()?.result;
            var statements = context.statement().Select(x => x.result);
            if (expr == null || statements.Any(x => x == null)) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var type       = expr.Inner.Type;
            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var block      = new BlockResult(context.Start, type, index, qual, statements, expr);
            context.result = new ExprResult(block.Token, block);
        }

        public override void ExitParenExpr([NotNull] ParenExprContext context)
        {
            var expr       = context.expr()?.result;
            if (expr == null) return;

            var type       = expr.Inner.Type;
            var paren      = new ParenResult(context.Start, type, expr);
            context.result = new ExprResult(paren.Token, paren);
        }

        public override void ExitUnitListCtorExpr([NotNull] UnitListCtorExprContext context)
        {
        }

        public override void ExitSingleListCtorExpr([NotNull] SingleListCtorExprContext context)
        {
        }

        public override void ExitTupleListCtorExpr([NotNull] TupleListCtorExprContext context)
        {
        }

        public override void ExitElementListExpr([NotNull] ElementListExprContext context)
        {
        }

        public override void ExitRangeListExpr([NotNull] RangeListExprContext context)
        {
        }

        public override void ExitSteppedRangeListExpr([NotNull] SteppedRangeListExprContext context)
        {
        }

        public override void ExitSpreadListExpr([NotNull] SpreadListExprContext context)
        {
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
            var literal = context.literal()?.result;
            if (literal == null) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            context.result = new ExprResult(literal.Token, literal);
        }

        public override void EnterThisLiteralExpr([NotNull] ThisLiteralExprContext context)
        {
            var scope = new TeuchiUdonScope(new TeuchiUdonThis(), TeuchiUdonScopeMode.This);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitThisLiteralExpr([NotNull] ThisLiteralExprContext context)
        {
            var thisLiteral = context.thisLiteral()?.result;
            if (thisLiteral == null) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            context.result = new ExprResult(thisLiteral.Token, thisLiteral);
        }

        public override void ExitEvalVarExpr([NotNull] EvalVarExprContext context)
        {
            var identifier = context.identifier()?.result;
            if (identifier == null) return;

            var evalVar = (TypedResult)null;
            if (context.Parent is AccessExprContext)
            {
                evalVar = new EvalVarCandidateResult(context.Start, identifier);
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
                        evalVar  = new EvalVarResult(context.Start, type, v);
                        break;
                    }

                    var qt = new TeuchiUdonType(qual, identifier.Name);
                    if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                    {
                        var type  = TeuchiUdonTables.Instance.Types[qt];
                        var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                        evalVar   = new EvalTypeResult(context.Start, outer, type);
                        break;
                    }

                    if
                    (
                        TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(qt) &&
                        (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                    )
                    {
                        var type  = TeuchiUdonTables.Instance.GenericRootTypes[qt];
                        var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                        evalVar   = new EvalTypeResult(context.Start, outer, type);
                        break;
                    }
                }

                if (evalVar == null)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"'{identifier.Name}' is not defined");
                    evalVar = new BottomResult(context.Start);
                }
            }

            context.result = new ExprResult(evalVar.Token, evalVar);
        }

        public override void ExitAccessExpr([NotNull] AccessExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            var qualifier = TeuchiUdonQualifierStack.Instance.Peek();

            if (expr1.Inner is EvalVarCandidateResult varCandidate1)
            {
                var eval = (TypedResult)null;
                foreach (var qual in TeuchiUdonQualifierStack.Instance.Qualifiers)
                {
                    var qv = new TeuchiUdonVar(qual, varCandidate1.Identifier.Name);
                    if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                    {
                        var v    = TeuchiUdonTables.Instance.Vars[qv];
                        var type = v.Type;
                        eval     = new EvalVarResult(varCandidate1.Token, type, v);
                        break;
                    }

                    var qt = new TeuchiUdonType(qual, varCandidate1.Identifier.Name);
                    if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                    {
                        var type  = TeuchiUdonTables.Instance.Types[qt];
                        var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                        eval      = new EvalTypeResult(varCandidate1.Token, outer, type);
                        break;
                    }

                    if
                    (
                        TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(qt) &&
                        (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                    )
                    {
                        var type  = TeuchiUdonTables.Instance.GenericRootTypes[qt];
                        var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                        eval      = new EvalTypeResult(varCandidate1.Token, outer, type);
                        break;
                    }
                }

                var scopes  = new TeuchiUdonScope[] { new TeuchiUdonScope(new TextLabel(varCandidate1.Identifier.Name)) };
                var newQual = new TeuchiUdonQualifier(scopes);
                if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(newQual))
                {
                    var q    = TeuchiUdonTables.Instance.Qualifiers[newQual];
                    var type = TeuchiUdonType.Qual.ApplyArgAsQual(q);
                    eval     = new EvalQualifierResult(varCandidate1.Token, type, q);
                }

                if (eval == null)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate1.Token, $"'{varCandidate1.Identifier.Name}' is not defined");
                    eval = new BottomResult(varCandidate1.Token);
                }
                
                expr1 = new ExprResult(eval.Token, eval);
            }
            var type1 = expr1.Inner.Type;

            if (expr2.Inner is EvalVarCandidateResult varCandidate2)
            {
                var eval = (TypedResult)null;
                do
                {
                    if (type1.LogicalTypeNameEquals(TeuchiUdonType.Qual))
                    {
                        var qual = type1.GetArgAsQual();
                        var qv   = new TeuchiUdonVar(qual, varCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                        {
                            var v    = TeuchiUdonTables.Instance.Vars[qv];
                            var type = v.Type;
                            eval     = new EvalVarResult(varCandidate2.Token, type, v);
                            break;
                        }

                        var qt = new TeuchiUdonType(qual, varCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate2.Token, outer, type);
                            break;
                        }

                        if
                        (
                            TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(qt) &&
                            (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                        )
                        {
                            var type  = TeuchiUdonTables.Instance.GenericRootTypes[qt];
                            var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate2.Token, outer, type);
                            break;
                        }

                        var scope    = new TeuchiUdonScope(new TextLabel(varCandidate2.Identifier.Name));
                        var appended = qual.Append(scope);
                        if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(appended))
                        {
                            var q    = TeuchiUdonTables.Instance.Qualifiers[appended];
                            var type = TeuchiUdonType.Qual.ApplyArgAsQual(q);
                            eval     = new EvalQualifierResult(varCandidate2.Token, type, q);
                            break;
                        }
                    }
                    else if (type1.LogicalTypeNameEquals(TeuchiUdonType.Type))
                    {
                        var t  = type1.GetArgAsType();
                        var sc = new TeuchiUdonScope(new TextLabel(t.LogicalName));
                        var q  = t.Qualifier.Append(sc);
                        var qt = new TeuchiUdonType(q, varCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate2.Token, outer, type);
                            break;
                        }

                        if
                        (
                            TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(qt) &&
                            (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                        )
                        {
                            var type  = TeuchiUdonTables.Instance.GenericRootTypes[qt];
                            var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate2.Token, outer, type);
                            break;
                        }

                        if
                        (
                            TeuchiUdonTables.Instance.TypeToMethods.ContainsKey(type1) &&
                            TeuchiUdonTables.Instance.TypeToMethods[type1].ContainsKey(varCandidate2.Identifier.Name)
                        )
                        {
                            var ms   = TeuchiUdonTables.Instance.TypeToMethods[type1][varCandidate2.Identifier.Name];
                            var type = TeuchiUdonType.Method.ApplyArgsAsMethod(ms.Values.SelectMany(x => x));
                            eval     = new MethodResult(varCandidate2.Token, type, varCandidate2.Identifier);
                            break;
                        }

                        var qg = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetGetterName(varCandidate2.Identifier.Name));
                        var g  = TeuchiUdonTables.Instance.GetMostCompatibleMethodsWithoutInTypes(qg, 0).ToArray();
                        var qs = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetSetterName(varCandidate2.Identifier.Name));
                        var s  = TeuchiUdonTables.Instance.GetMostCompatibleMethodsWithoutInTypes(qs, 1).ToArray();
                        if (g.Length == 1 && s.Length == 1 && g[0].OutTypes.Length == 1)
                        {
                            eval = new EvalGetterSetterResult(varCandidate2.Token, g[0].OutTypes[0], qualifier, g[0], s[0]);
                            break;
                        }
                        else if (g.Length == 1 && g[0].OutTypes.Length == 1)
                        {
                            eval = new EvalGetterResult(varCandidate2.Token, g[0].OutTypes[0], qualifier, g[0]);
                            break;
                        }
                        else if (s.Length == 1)
                        {
                            eval = new EvalSetterResult(varCandidate2.Token, s[0]);
                            break;
                        }
                        else if (g.Length >= 2 || s.Length >= 2)
                        {
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate2.Token, $"arguments of method '{varCandidate2.Identifier.Name}' is nondeterministic");
                            eval = new BottomResult(varCandidate2.Token);
                            break;
                        }

                        if (t.RealType.IsEnum)
                        {
                            var name  = varCandidate2.Identifier.Name;
                            var index = Array.IndexOf(t.RealType.GetEnumNames(), name);
                            if (index >= 0)
                            {
                                var value = t.RealType.GetEnumValues().GetValue(index);
                                eval = new LiteralResult(varCandidate2.Token, t, TeuchiUdonTables.Instance.GetLiteralIndex(), name, value);
                                break;
                            }
                            else
                            {
                                TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate2.Token, $"'{name}' is not enum value");
                                eval = new BottomResult(varCandidate2.Token);
                                break;
                            }
                        }
                    }
                    else
                    {
                        if
                        (
                            TeuchiUdonTables.Instance.TypeToMethods.ContainsKey(type1) &&
                            TeuchiUdonTables.Instance.TypeToMethods[type1].ContainsKey(varCandidate2.Identifier.Name)
                        )
                        {
                            var ms   = TeuchiUdonTables.Instance.TypeToMethods[type1][varCandidate2.Identifier.Name];
                            var type = TeuchiUdonType.Method.ApplyArgsAsMethod(ms.Values.SelectMany(x => x));
                            eval     = new MethodResult(varCandidate2.Token, type, varCandidate2.Identifier);
                            break;
                        }

                        var qg = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetGetterName(varCandidate2.Identifier.Name));
                        var g  = TeuchiUdonTables.Instance.GetMostCompatibleMethodsWithoutInTypes(qg, 1).ToArray();
                        var qs = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetSetterName(varCandidate2.Identifier.Name));
                        var s  = TeuchiUdonTables.Instance.GetMostCompatibleMethodsWithoutInTypes(qs, 2).ToArray();
                        if (g.Length == 1 && s.Length == 1 && g[0].OutTypes.Length == 1)
                        {
                            eval = new EvalGetterSetterResult(varCandidate2.Token, g[0].OutTypes[0], qualifier, g[0], s[0]);
                            break;
                        }
                        else if (g.Length == 1 && g[0].OutTypes.Length == 1)
                        {
                            eval = new EvalGetterResult(varCandidate2.Token, g[0].OutTypes[0], qualifier, g[0]);
                            break;
                        }
                        else if (s.Length == 1)
                        {
                            eval = new EvalSetterResult(varCandidate2.Token, s[0]);
                            break;
                        }
                        else if (g.Length >= 2 || s.Length >= 2)
                        {
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate2.Token, $"arguments of method '{varCandidate2.Identifier.Name}' is nondeterministic");
                            eval = new BottomResult(varCandidate2.Token);
                            break;
                        }
                    }

                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate2.Token, $"'{varCandidate2.Identifier.Name}' is not defined");
                    eval = new BottomResult(varCandidate2.Token);
                } while (false);

                expr2 = new ExprResult(eval.Token, eval);
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(expr1.Token, $"invalid '{op}' operator");
            }

            var infix      = new InfixResult(expr1.Token, expr2.Inner.Type, qualifier, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitCastExpr([NotNull] CastExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => x == null)) return;

            var type = exprs[0].Inner.Type;
            if (!type.LogicalTypeNameEquals(TeuchiUdonType.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression is not a type");
                return;
            }

            var type1 = type.GetArgAsType();
            var type2 = exprs[1].Inner.Type;
            if (type1.IsAssignableFrom(type2) || type2.IsAssignableFrom(type1))
            {
                var cast       = new TypeCastResult(context.Start, type1, exprs[0], exprs[1]);
                context.result = new ExprResult(cast.Token, cast);
            }
            else if (IsValidConvertType(type1) && IsValidConvertType(type2))
            {
                var qual       = TeuchiUdonQualifierStack.Instance.Peek();
                var cast       = new ConvertCastResult(context.Start, type1, qual, exprs[0], exprs[1]);
                context.result = new ExprResult(cast.Token, cast);
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"specified type cannot be cast");
                return;
            }
        }

        private bool IsValidConvertType(TeuchiUdonType type)
        {
            return new TeuchiUdonType[]
            {
                TeuchiUdonType.Bool,
                TeuchiUdonType.Byte,
                TeuchiUdonType.Char,
                TeuchiUdonType.DateTime,
                TeuchiUdonType.Decimal,
                TeuchiUdonType.Double,
                TeuchiUdonType.Short,
                TeuchiUdonType.Int,
                TeuchiUdonType.Long,
                TeuchiUdonType.SByte,
                TeuchiUdonType.Float,
                TeuchiUdonType.String,
                TeuchiUdonType.UShort,
                TeuchiUdonType.UInt,
                TeuchiUdonType.ULong
            }
            .Any(x => type.LogicalTypeEquals(x));
        }

        public override void ExitEvalUnitFuncExpr([NotNull] EvalUnitFuncExprContext context)
        {
            var expr = context.expr()?.result;
            if (expr == null) return;

            var args       = new ArgExprResult[0];
            context.result = ExitEvalFuncExpr(context.Start, expr, args);
        }

        public override void ExitEvalSingleFuncExpr([NotNull] EvalSingleFuncExprContext context)
        {
            var expr = context.expr   ()?.result;
            var arg  = context.argExpr()?.result;
            if (expr == null || arg == null) return;

            var args = new ArgExprResult[] { arg };
            context.result = ExitEvalFuncExpr(context.Start, expr, args);
        }

        public override void ExitEvalTupleFuncExpr([NotNull] EvalTupleFuncExprContext context)
        {
            var expr = context.expr   ()?.result;
            var args = context.argExpr().Select(x => x?.result).ToArray();
            if (expr == null || args.Length < 2) return;

            context.result = ExitEvalFuncExpr(context.Start, expr, args);
        }

        private ExprResult ExitEvalFuncExpr(IToken token, ExprResult expr, IEnumerable<ArgExprResult> argExprs)
        {
            var type     = expr.Inner.Type;
            var args     = argExprs.Select(x => x.Expr);
            var argRefs  = argExprs.Select(x => x.Ref);
            var qual     = TeuchiUdonQualifierStack.Instance.Peek();
            var evalFunc = (TypedResult)null;

            if (type.LogicalTypeNameEquals(TeuchiUdonType.Func))
            {
                if (argRefs.Any(x => x))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"arguments of func cannot be ref");
                    evalFunc = new BottomResult(token);
                }
                else
                {
                    var argTypes = argExprs.Select(x => x.Expr.Inner.Type).ToArray();
                    var iType    = TeuchiUdonType.ToOneType(argTypes);
                    var oType    = TeuchiUdonType.Unknown;
                    if (type.IsAssignableFrom(TeuchiUdonType.Func.ApplyArgsAsFunc(iType, oType)))
                    {
                        var outType = type.GetArgAsFuncOutType();
                        var index   = TeuchiUdonTables.Instance.GetEvalFuncIndex();
                        evalFunc    = new EvalFuncResult(token, outType, index, qual, expr, args);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"arguments of func is not compatible");
                        evalFunc = new BottomResult(token);
                    }
                }
            }
            else if (type.LogicalTypeNameEquals(TeuchiUdonType.Method) && expr.Inner.Instance != null)
            {
                var instanceType = expr.Inner.Instance.Inner.Type.RealType == null ? new TeuchiUdonType[0] : new TeuchiUdonType[] { expr.Inner.Instance.Inner.Type };
                var inTypes      = instanceType.Concat(args.Select(x => x.Inner.Type));
                var inRefs       = instanceType.Select(_ => false).Concat(argRefs);
                var ms           = type.GetMostCompatibleMethods(inTypes).ToArray();
                if (ms.Length == 0)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"method is not defined");
                    evalFunc = new BottomResult(token);
                }
                else if (ms.Length == 1)
                {
                    var method = ms[0];
                    var inMuts = instanceType.Select(_ => false).Concat(args.Select(x => x.Inner.LeftValues.Length == 1));
                    if
                    (
                        method.InParamInOuts.Select(x => x == TeuchiUdonMethodParamInOut.InOut).SequenceEqual(inRefs) &&
                        inMuts.Zip(inRefs, (m, r) => (m, r)).Select(x => x.m && x.r).SequenceEqual(inRefs)
                    )
                    {
                        var outType = TeuchiUdonType.ToOneType(method.OutTypes);
                        evalFunc = new EvalMethodResult(token, outType, qual, method, expr, args);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"ref mark of method is not compatible");
                        evalFunc = new BottomResult(token);
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"arguments of method is nondeterministic");
                    evalFunc = new BottomResult(token);
                }
            }
            else if (type.LogicalTypeNameEquals(TeuchiUdonType.Type))
            {
                var inTypes = args.Select(x => x.Inner.Type);
                var qm      = new TeuchiUdonMethod(type, "ctor", inTypes);
                var ms      = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
                if (ms.Length == 0)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"ctor is not defined");
                    evalFunc = new BottomResult(token);
                }
                else if (ms.Length == 1)
                {
                    var method = ms[0];
                    var inMuts = args.Select(x => x.Inner.LeftValues.Length == 1);
                    if
                    (
                        method.InParamInOuts.Select(x => x == TeuchiUdonMethodParamInOut.InOut).SequenceEqual(argRefs) &&
                        inMuts.Zip(argRefs, (m, r) => (m, r)).Select(x => x.m && x.r).SequenceEqual(argRefs)
                    )
                    {
                        var outType = TeuchiUdonType.ToOneType(method.OutTypes);
                        evalFunc = new EvalMethodResult(token, outType, qual, method, expr, args);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"ref mark of ctor is not compatible");
                        evalFunc = new BottomResult(token);
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"arguments of ctor is nondeterministic");
                    evalFunc = new BottomResult(token);
                }
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"expression is not a function or method");
                evalFunc = new BottomResult(token);
            }

            return new ExprResult(evalFunc.Token, evalFunc);
        }

        public override void ExitArgExpr([NotNull] ArgExprContext context)
        {
            var expr = context.expr()?.result;
            var rf   = context.REF() != null;
            if (expr == null) return;

            context.result = new ArgExprResult(context.Start, expr, rf);
        }

        public override void ExitNameOfExpr([NotNull] NameOfExprContext context)
        {
        }

        public override void ExitEvalSingleKeyExpr([NotNull] EvalSingleKeyExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => x == null)) return;

            var args       = exprs.Skip(1);
            var evalKey    = ExitEvalKeyExpr(context.Start, exprs[0], args);
            context.result = new ExprResult(evalKey.Token, evalKey);
        }

        public override void ExitEvalTupleKeyExpr([NotNull] EvalTupleKeyExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length >= 1 || exprs.Any(x => x == null)) return;

            var args       = exprs.Skip(1);
            var evalKey    = ExitEvalKeyExpr(context.Start, exprs[0], args);
            context.result = new ExprResult(evalKey.Token, evalKey);
        }

        private TypedResult ExitEvalKeyExpr(IToken token, ExprResult expr, IEnumerable<ExprResult> args)
        {
            if (expr.Inner.Type.LogicalTypeNameEquals(TeuchiUdonType.Type) && args.All(x => x.Inner.Type.LogicalTypeNameEquals(TeuchiUdonType.Type)))
            {
                var exprType = expr.Inner.Type.GetArgAsType();
                if (!TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(exprType))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"specified key is invalid");
                    return new BottomResult(token);
                }

                var argTypes = args.Select(x => x.Inner.Type.GetArgAsType());
                if (exprType.LogicalTypeEquals(TeuchiUdonType.Array))
                {
                    var qt = new TeuchiUdonType(TeuchiUdonType.Array.LogicalName, argTypes);
                    if (TeuchiUdonTables.Instance.LogicalTypes.ContainsKey(qt))
                    {
                        var type = TeuchiUdonTables.Instance.LogicalTypes[qt];
                        return new EvalTypeResult(token, TeuchiUdonType.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"specified key is invalid");
                        return new BottomResult(token);
                    }
                }
                else if (exprType.LogicalTypeEquals(TeuchiUdonType.List))
                {
                    if (argTypes.All(x => x.RealType != null))
                    {
                        var type = new TeuchiUdonType
                        (
                            TeuchiUdonQualifier.Top,
                            TeuchiUdonType.List.Name,
                            argTypes,
                            TeuchiUdonType.List.LogicalName,
                            TeuchiUdonType.Buffer.GetRealName(),
                            TeuchiUdonType.Buffer.RealType
                        );
                        return new EvalTypeResult(token, TeuchiUdonType.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"specified key is invalid");
                        return new BottomResult(token);
                    }
                }
                else
                {
                    var qt = new TeuchiUdonType(TeuchiUdonTables.GetGenericTypeName(exprType, argTypes), argTypes);
                    if (TeuchiUdonTables.Instance.LogicalTypes.ContainsKey(qt))
                    {
                        var type = TeuchiUdonTables.Instance.LogicalTypes[qt];
                        return new EvalTypeResult(token, TeuchiUdonType.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"specified key is invalid");
                        return new BottomResult(token);
                    }
                }
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"specified key is invalid");
                return new BottomResult(token);
            }
        }

        public override void ExitPrefixExpr([NotNull] PrefixExprContext context)
        {
            var op   = context.op?.Text;
            var expr = context.expr()?.result;
            if (op == null || expr == null) return;
            
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var prefix     = new PrefixResult(context.Start, expr.Inner.Type, qual, op, expr);
            context.result = new ExprResult(prefix.Token, prefix);
        }

        public override void ExitMultiplicationExpr([NotNull] MultiplicationExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitAdditionExpr([NotNull] AdditionExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitShiftExpr([NotNull] ShiftExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitRelationExpr([NotNull] RelationExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, TeuchiUdonType.Bool, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitEqualityExpr([NotNull] EqualityExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, TeuchiUdonType.Bool, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitLogicalAndExpr([NotNull] LogicalAndExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitLogicalXorExpr([NotNull] LogicalXorExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitLogicalOrExpr([NotNull] LogicalOrExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitConditionalAndExpr([NotNull] ConditionalAndExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            if (!expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bool) || !expr2.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bool))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid operand type");
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, TeuchiUdonType.Bool, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitConditionalOrExpr([NotNull] ConditionalOrExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            if (!expr1.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bool) || !expr2.Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bool))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid operand type");
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, TeuchiUdonType.Bool, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitCoalescingExpr([NotNull] CoalescingExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            if (!expr1.Inner.Type.IsAssignableFrom(expr2.Inner.Type) && !expr2.Inner.Type.IsAssignableFrom(expr1.Inner.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid operand type");
                return;
            }

            var type       = expr1.Inner.Type.IsAssignableFrom(expr2.Inner.Type) ? expr1.Inner.Type : expr2.Inner.Type;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitConditionalExpr([NotNull] ConditionalExprContext context)
        {
            var exprs = context.expr().Select(x => x.result).ToArray();
            if (exprs.Length != 3 || exprs.Any(x => x == null)) return;

            if
            (
                !exprs[0].Inner.Type.LogicalTypeEquals(TeuchiUdonType.Bool) ||
                !exprs[1].Inner.Type.IsAssignableFrom(exprs[2].Inner.Type) &&
                !exprs[2].Inner.Type.IsAssignableFrom(exprs[1].Inner.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid operand type");
                return;
            }

            var type        = exprs[1].Inner.Type.IsAssignableFrom(exprs[2].Inner.Type) ? exprs[1].Inner.Type : exprs[2].Inner.Type;
            var conditional = new ConditionalResult(context.Start, type, exprs[0], exprs[1], exprs[2]);
            context.result  = new ExprResult(conditional.Token, conditional);
        }

        public override void ExitAssignExpr([NotNull] AssignExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (expr1 == null || expr2 == null) return;

            if (expr1.Inner.LeftValues.Length != 1 || !expr1.Inner.Type.IsAssignableFrom(expr2.Inner.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"cannot assign");
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, TeuchiUdonType.Unit, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void EnterLetInBindExpr([NotNull] LetInBindExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetLetInIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var scope          = new TeuchiUdonScope(new TeuchiUdonLetIn(index, qual), TeuchiUdonScopeMode.LetIn);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitLetInBindExpr([NotNull] LetInBindExprContext context)
        {
            var varBind = context.varBind()?.result;
            var expr    = context.expr   ()?.result;
            if (varBind == null || expr == null) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var letInBind  = new LetInBindResult(context.Start, expr.Inner.Type, index, qual, varBind, expr);
            context.result = new ExprResult(letInBind.Token, letInBind);
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
            var varDecl = context.varDecl()?.result;
            var expr    = context.expr   ()?.result;
            if (varDecl == null || expr == null) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var qual    = TeuchiUdonQualifierStack.Instance.Peek();
            var inType  = TeuchiUdonType.ToOneType(varDecl.Types);
            var outType = expr.Inner.Type;
            var type    = TeuchiUdonType.Func.ApplyArgsAsFunc(inType, outType);

            var varBind = qual.GetLast<TeuchiUdonVarBind>();
            if (varBind != null && varBind.Qualifier == TeuchiUdonQualifier.Top && varBind.VarNames.Length == 1 && TeuchiUdonTables.Instance.Events.ContainsKey(varBind.VarNames[0]))
            {
                var ev   = TeuchiUdonTables.Instance.Events[varBind.VarNames[0]];
                var args = varDecl.Vars.ToArray();
                if (ev.OutTypes.Length != args.Length)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"arguments of '{varBind.VarNames[0]}' event is not compatible");
                }
                else
                {
                    var evTypes  = TeuchiUdonType.ToOneType(ev.OutTypes);
                    var argTypes = TeuchiUdonType.ToOneType(args.Select(x => x.Type));
                    if (evTypes.IsAssignableFrom(argTypes))
                    {
                        foreach (var (n, t) in ev.OutParamUdonNames.Zip(ev.OutTypes, (n, t) => (n, t)))
                        {
                            var name = TeuchiUdonTables.GetEventParamName(ev.Name, n);
                            if (!TeuchiUdonTables.IsValidVarName(name))
                            {
                                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"'{name}' is invalid variable name");
                                continue;
                            }
                            
                            var v = new TeuchiUdonVar(TeuchiUdonTables.Instance.GetVarIndex(), TeuchiUdonQualifier.Top, name, t, false, true);
                            if (TeuchiUdonTables.Instance.Vars.ContainsKey(v))
                            {
                                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"'{v.Name}' conflicts with another variable");
                            }
                            else
                            {
                                TeuchiUdonTables.Instance.Vars.Add(v, v);
                            }
                        }
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"arguments of '{varBind.VarNames[0]}' event is not compatible");
                    }
                }
            }

            var index      = context.tableIndex;
            var func       = new FuncResult(context.Start, type, index, qual, varDecl, expr);
            context.result = new ExprResult(func.Token, func);
        }

        public override void ExitUnitLiteral([NotNull] UnitLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var type       = TeuchiUdonType.Unit;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            context.result = new LiteralResult(context.Start, type, tableIndex, "()", null);
        }

        public override void ExitNullLiteral([NotNull] NullLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text       = context.GetText();
            var type       = TeuchiUdonType.Bottom;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            context.result = new LiteralResult(context.Start, type, tableIndex, text, null);
        }

        public override void ExitBoolLiteral([NotNull] BoolLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text       = context.GetText();
            var type       = TeuchiUdonType.Bool;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToBoolValue(context.Start, text);
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        private object ToBoolValue(IToken token, string text)
        {
            try
            {
                return Convert.ToBoolean(text);
            }
            catch
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to int");
                return null;
            }
        }

        public override void ExitIntegerLiteral([NotNull] IntegerLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

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
            var value      = ToIntegerValue(context.Start, type, text.Substring(index, count), basis);
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        public override void ExitHexIntegerLiteral([NotNull] HexIntegerLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

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
            var value      = ToIntegerValue(context.Start, type, text.Substring(index, count), basis);
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        public override void ExitBinIntegerLiteral([NotNull] BinIntegerLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

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
            var value      = ToIntegerValue(context.Start, type, text.Substring(index, count), basis);
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        private object ToIntegerValue(IToken token, TeuchiUdonType type, string text, int basis)
        {
            if (type.LogicalTypeEquals(TeuchiUdonType.Int))
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
            else if (type.LogicalTypeEquals(TeuchiUdonType.UInt))
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
            else if (type.LogicalTypeEquals(TeuchiUdonType.Long))
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
            else if (type.LogicalTypeEquals(TeuchiUdonType.ULong))
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

        public override void ExitRealLiteral([NotNull] RealLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text = context.GetText().Replace("_", "").ToLower();

            var count = text.Length;
            var type  = TeuchiUdonType.Float;

            if (text.EndsWith("f"))
            {
                count--;
            }
            else if (text.EndsWith("d"))
            {
                count--;
                type = TeuchiUdonType.Double;
            }
            else if (text.EndsWith("m"))
            {
                count--;
                type = TeuchiUdonType.Decimal;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToRealValue(context.Start, type, text.Substring(0, count));
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        private object ToRealValue(IToken token, TeuchiUdonType type, string text)
        {
            if (type.LogicalTypeEquals(TeuchiUdonType.Float))
            {
                try
                {
                    return Convert.ToSingle(text);
                }
                catch
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to float");
                    return null;
                }
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Double))
            {
                try
                {
                    return Convert.ToDouble(text);
                }
                catch
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to double");
                    return null;
                }
            }
            else if (type.LogicalTypeEquals(TeuchiUdonType.Decimal))
            {
                try
                {
                    return Convert.ToDecimal(text);
                }
                catch
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to decimal");
                    return null;
                }
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"failed to convert '{token.Text}' to unknown");
                return null;
            }
        }

        public override void ExitCharacterLiteral([NotNull] CharacterLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text       = context.GetText();
            var type       = TeuchiUdonType.Char;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToCharacterValue(context.Start, text.Substring(1, text.Length - 2));
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        public override void ExitRegularString([NotNull] RegularStringContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text       = context.GetText();
            var type       = TeuchiUdonType.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToRegularStringValue(context.Start, text.Substring(1, text.Length - 2));
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        public override void ExitVervatiumString([NotNull] VervatiumStringContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text       = context.GetText();
            var type       = TeuchiUdonType.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToVervatiumStringValue(context.Start, text.Substring(2, text.Length - 3));
            context.result = new LiteralResult(context.Start, type, tableIndex, text, value);
        }

        private object ToCharacterValue(IToken token, string text)
        {
            var ch = EscapeRegularString(token, text);

            if (ch.Length != 1)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"invalid length of character literal");
                return null;
            }

            return ch[0];
        }

        private object ToRegularStringValue(IToken token, string text)
        {
            return EscapeRegularString(token, text);
        }

        private object ToVervatiumStringValue(IToken token, string text)
        {
            return EscapeVervatiumString(token, text);
        }

        private string EscapeRegularString(IToken token, string text)
        {
            return EscapeString(token, text, reader =>
            {
                var ch = reader.Read();
                if (ch == -1) return null;
                if (ch == '\\')
                {
                    var escaped = reader.Read();
                    switch (escaped)
                    {
                        case '\'':
                            return "'";
                        case '"':
                            return "\"";
                        case '\\':
                            return "\\";
                        case '0':
                            return "\0";
                        case 'a':
                            return "\a";
                        case 'b':
                            return "\b";
                        case 'f':
                            return "\f";
                        case 'n':
                            return "\n";
                        case 'r':
                            return "\r";
                        case 't':
                            return "\t";
                        case 'v':
                            return "\v";
                        case 'x':
                        {
                            var d0 = CharNumberToInt(reader.Peek());
                            if (d0 == -1)
                            {
                                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"invalid char detected");
                                return "";
                            }
                            reader.Read();
                            var u = d0;

                            for (var i = 1; i < 4; i++)
                            {
                                var d = CharNumberToInt(reader.Peek());
                                if (d == -1)
                                {
                                    return ((char)u).ToString();
                                }
                                reader.Read();
                                u = u * 16 + d;
                            }

                            return ((char)u).ToString();
                        }
                        case 'u':
                        {
                            var u = 0L;
                            for (var i = 0; i < 4; i++)
                            {
                                var d = CharNumberToInt(reader.Read());
                                if (d == -1)
                                {
                                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"invalid char detected");
                                    return "";
                                }
                                u = u * 16 + d;
                            }
                            return ((char)u).ToString();
                        }
                        case 'U':
                        {
                            var u0 = 0L;
                            for (var i = 0; i < 4; i++)
                            {
                                var d = CharNumberToInt(reader.Read());
                                if (d == -1)
                                {
                                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"invalid char detected");
                                    return "";
                                }
                                u0 = u0 * 16 + d;
                            }
                            var u1 = 0L;
                            for (var i = 0; i < 4; i++)
                            {
                                var d = CharNumberToInt(reader.Read());
                                if (d == -1)
                                {
                                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"invalid char detected");
                                    return "";
                                }
                                u1 = u1 * 16 + d;
                            }
                            return ((char)u0).ToString() + ((char)u1).ToString();
                        }
                        default:
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"invalid char detected");
                            return "";
                    }
                }
                return ((char)ch).ToString();
            });
        }

        private long CharNumberToInt(int ch)
        {
            if ('0' <= ch && ch <= '9') return ch - '0';
            if ('A' <= ch && ch <= 'F') return ch - 'A' + 10;
            if ('a' <= ch && ch <= 'f') return ch - 'a' + 10;
            return -1;
        }

        private string EscapeVervatiumString(IToken token, string text)
        {
            return EscapeString(token, text, reader =>
            {
                var ch = reader.Read();
                if (ch == -1) return null;
                if (ch == '"')
                {
                    var escaped = reader.Read();
                    if (escaped == '"')
                    {
                        return "\"";
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"invalid char detected");
                        return "";
                    }
                }
                return ((char)ch).ToString();
            });
        }

        private string EscapeString(IToken token, string text, Func<StringReader, string> consumeFunc)
        {
            var result = new StringBuilder();
            using (var reader = new StringReader(text))
            {
                for (;;)
                {
                    var ch = consumeFunc(reader);
                    if (ch == null) return result.ToString();
                    result.Append(ch);
                }
            }
        }

        public override void ExitThisLiteral([NotNull] ThisLiteralContext context)
        {
            if (context.ChildCount == 0) return;

            var text       = context.GetText();
            var type       = TeuchiUdonType.String;
            context.result = new ThisResult(context.Start);
        }
    }
}
