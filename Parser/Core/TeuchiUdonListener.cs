using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace akanevrc.TeuchiUdon
{
    using static TeuchiUdonParser;

    public class TeuchiUdonListener : TeuchiUdonParserBaseListener
    {
        public TargetResult TargetResult { get; private set; } = null;
        private TeuchiUdonParser Parser { get; }
        private List<JumpResult> Jumps { get; } = new List<JumpResult>();

        private JumpResult StoreJumpResult(JumpResult result)
        {
            Jumps.Add(result);
            return result;
        }

        private void PrepareJumpResults()
        {
            foreach (var jump in Jumps)
            {
                var block = jump.Block();
                if (!block.Type.IsAssignableFrom(jump.Value.Inner.Type))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(jump.Start, $"jump value type is not compatible");
                }
            }
        }

        public TeuchiUdonListener(TeuchiUdonParser parser)
        {
            Parser = parser;
        }

        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            if
            (
                context is TopStatementContext ||
                context is StatementContext    ||
                context is VarBindContext      ||
                context is IterExprContext     ||
                context is IsoExprContext      ||
                context is ExprContext && context.Parent is FuncExprContext
            )
            {
                TeuchiUdonOutValuePool.Instance.PushScope(TeuchiUdonQualifierStack.Instance.Peek().GetFuncQualifier());
            }
        }

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            if
            (
                context is TopStatementContext ||
                context is StatementContext    ||
                context is VarBindContext      ||
                context is IterExprContext     ||
                context is IsoExprContext      ||
                context is ExprContext && context.Parent is FuncExprContext
            )
            {
                TeuchiUdonOutValuePool.Instance.PopScope(TeuchiUdonQualifierStack.Instance.Peek().GetFuncQualifier());
            }
            else if (context is ExprContext expr && expr.result != null)
            {
                foreach (var o in expr.result.Inner.ReleasedChildren.SelectMany(x => x.ReleasedOutValues)) TeuchiUdonOutValuePool.Instance.ReleaseOutValue(o);
            }
        }

        private bool IsInvalid(TeuchiUdonParserResult result)
        {
            return result == null || !result.Valid;
        }

        public override void ExitTarget([NotNull] TargetContext context)
        {
            PrepareJumpResults();

            var body = context.body()?.result;
            if (IsInvalid(body))
            {
                context.result = new TargetResult(context.Start, context.Stop);
                return;
            }

            TargetResult   = new TargetResult(context.Start, context.Stop, body);
            context.result = TargetResult;
        }

        public override void EnterBody([NotNull] BodyContext context)
        {
            TeuchiUdonQualifierStack.Instance.Push(TeuchiUdonQualifier.Top);
        }

        public override void ExitBody([NotNull] BodyContext context)
        {
            var topStatements = context.topStatement().Select(x => x.result).ToArray();
            if (topStatements.Any(x => IsInvalid(x)))
            {
                context.result = new BodyResult(context.Start, context.Stop);
                return;
            }

            context.result = new BodyResult(context.Start, context.Stop, topStatements);
        }

        public override void ExitVarBindTopStatement([NotNull] VarBindTopStatementContext context)
        {
            var varBind = context.varBind()?.result;
            var attrs   = context.varAttr().Select(x => x.result);
            if (IsInvalid(varBind) || attrs.Any(x => IsInvalid(x)))
            {
                context.result = new TopBindResult(context.Start, context.Stop);
                return;
            }

            var (pub, sync, valid) = ExtractFromVarAttrs(attrs);
            if (!valid)
            {
                context.result = new TopBindResult(context.Start, context.Stop);
                return;
            }

            if (varBind.Vars.Length != 1)
            {
                if (pub || sync != TeuchiUdonSyncMode.Disable)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"tuple cannot be specified with any attributes");
                    context.result = new TopBindResult(context.Start, context.Stop);
                    return;
                }
            }
            else if (varBind.Vars[0].Type.IsFunc())
            {
                var v = varBind.Vars[0];

                if (varBind.Expr.Inner is FuncResult func)
                {
                    if ((pub || TeuchiUdonTables.Instance.Events.ContainsKey(v.Name)) && !TeuchiUdonTables.Instance.EventFuncs.ContainsKey(v))
                    {
                        TeuchiUdonTables.Instance.EventFuncs.Add(v, func.Func);
                    }

                    if (sync != TeuchiUdonSyncMode.Disable)
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"function cannot be specified with @sync, @linear, or @smooth");
                        context.result = new TopBindResult(context.Start, context.Stop);
                        return;
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"toplevel variable cannot be assigned from function indirectly");
                    context.result = new TopBindResult(context.Start, context.Stop);
                    return;
                }
            }
            else
            {
                var v = varBind.Vars[0];

                if (TeuchiUdonTables.Instance.Events.ContainsKey(v.Name))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"event must be function");
                    context.result = new TopBindResult(context.Start, context.Stop);
                    return;
                }

                if (pub)
                {
                    if (varBind.Expr.Inner is LiteralResult literal)
                    {
                        if (!TeuchiUdonTables.Instance.PublicVars.ContainsKey(v))
                        {
                            TeuchiUdonTables.Instance.PublicVars.Add(v, literal.Literal);
                        }
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"public valiable cannot be bound non-literal expression");
                        context.result = new TopBindResult(context.Start, context.Stop);
                        return;
                    }
                }

                if (sync == TeuchiUdonSyncMode.Sync && !v.Type.IsSyncableType())
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid valiable type to be specified with @sync");
                    context.result = new TopBindResult(context.Start, context.Stop);
                    return;
                }
                else if (sync == TeuchiUdonSyncMode.Linear && !v.Type.IsLinearSyncableType())
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid valiable type to be specified with @linear");
                    context.result = new TopBindResult(context.Start, context.Stop);
                    return;
                }
                else if (sync == TeuchiUdonSyncMode.Smooth && !v.Type.IsSmoothSyncableType())
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid valiable type to be specified with @smooth");
                    context.result = new TopBindResult(context.Start, context.Stop);
                    return;
                }
                else if (sync != TeuchiUdonSyncMode.Disable)
                {
                    if (!TeuchiUdonTables.Instance.SyncedVars.ContainsKey(v))
                    {
                        TeuchiUdonTables.Instance.SyncedVars.Add(v, sync);
                    }
                }
            }

            context.result = new TopBindResult(context.Start, context.Stop, varBind, pub, sync);
        }

        private (bool pub, TeuchiUdonSyncMode sync, bool valid) ExtractFromVarAttrs(IEnumerable<VarAttrResult> attrs)
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
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(publicAttr.Start, $"multiple @public detected");
                        return (false, TeuchiUdonSyncMode.Disable, false);
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
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(syncAttr.Start, $"multiple @sync, @linear, or @smooth detected");
                        return (false, TeuchiUdonSyncMode.Disable, false);
                    }
                }
            }

            return (pub, sync, true);
        }

        public override void ExitExprTopStatement([NotNull] ExprTopStatementContext context)
        {
            var expr  = context.expr()?.result;
            if (IsInvalid(expr))
            {
                context.result = new TopExprResult(context.Start, context.Stop);
                return;
            }

            expr.ReturnsValue = false;
            context.result = new TopExprResult(context.Start, context.Stop, expr);
        }

        public override void ExitPublicVarAttr([NotNull] PublicVarAttrContext context)
        {
            context.result = new PublicVarAttrResult(context.Start, context.Stop);
        }

        public override void ExitSyncVarAttr([NotNull] SyncVarAttrContext context)
        {
            context.result = new SyncVarAttrResult(context.Start, context.Stop, TeuchiUdonSyncMode.Sync);
        }

        public override void ExitLinearVarAttr([NotNull] LinearVarAttrContext context)
        {
            context.result = new SyncVarAttrResult(context.Start, context.Stop, TeuchiUdonSyncMode.Linear);
        }

        public override void ExitSmoothVarAttr([NotNull] SmoothVarAttrContext context)
        {
            context.result = new SyncVarAttrResult(context.Start, context.Stop, TeuchiUdonSyncMode.Smooth);
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
                varDecl is TupleVarDeclContext  tv ? tv.qualifiedVar().Select(x => x?.identifier()?.GetText() ?? "").ToArray() : Enumerable.Empty<string>();
            var scope   = new TeuchiUdonScope(new TeuchiUdonVarBind(index, qual, varNames), TeuchiUdonScopeMode.VarBind);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitVarBind([NotNull] VarBindContext context)
        {
            var varDecl = context.varDecl()?.result;
            var expr    = context.expr   ()?.result;
            if (IsInvalid(varDecl) || IsInvalid(expr))
            {
                context.result = new VarBindResult(context.Start, context.Stop);
                return;
            }

            TeuchiUdonQualifierStack.Instance.Pop();

            var mut = context.MUT() != null;

            var index = context.tableIndex;
            var qual  = TeuchiUdonQualifierStack.Instance.Peek();

            var vars = (TeuchiUdonVar[])null;
            if (varDecl.Vars.Length == 1)
            {
                var v = varDecl.Vars[0];
                var t = expr.Inner.Type;
                if (v.Type.IsAssignableFrom(t))
                {
                    if (t.ContainsUnknown())
                    {
                        expr.Inner.BindType(v.Type);
                    }

                    vars = new TeuchiUdonVar[]
                    {
                        new TeuchiUdonVar
                        (
                            TeuchiUdonTables.Instance.GetVarIndex(),
                            v.Qualifier,
                            v.Name,
                            v.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Unknown) ? t : v.Type,
                            mut,
                            false
                        )
                    };
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variable");
                    context.result = new VarBindResult(context.Start, context.Stop);
                    return;
                }
            }
            else if (varDecl.Vars.Length >= 2)
            {
                if (expr.Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple))
                {
                    var vs = varDecl.Vars;
                    var ts = expr.Inner.Type.GetArgsAsTuple().ToArray();
                    if (vs.Length == ts.Length && varDecl.Types.Zip(ts, (v, t) => (v, t)).All(x => x.v.IsAssignableFrom(x.t)))
                    {
                        if (expr.Inner.Type.ContainsUnknown())
                        {
                            expr.Inner.BindType(TeuchiUdonType.ToOneType(vs.Select(x => x.Type)));
                        }
                        
                        vars = varDecl.Vars
                            .Zip(ts, (v, t) => (v, t))
                            .Select(x =>
                                new TeuchiUdonVar
                                (
                                    TeuchiUdonTables.Instance.GetVarIndex(),
                                    x.v.Qualifier,
                                    x.v.Name,
                                    x.v.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Unknown) ? x.t : x.v.Type,
                                    mut,
                                    false
                                )
                            )
                            .ToArray();
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variables");
                        context.result = new VarBindResult(context.Start, context.Stop);
                        return;
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variables");
                    context.result = new VarBindResult(context.Start, context.Stop);
                    return;
                }
            }

            if (mut && expr.Inner.Type.IsFunc())
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"function variable cannot be mutable");
                context.result = new VarBindResult(context.Start, context.Stop);
                return;
            }

            context.result = new VarBindResult(context.Start, context.Stop, index, qual, vars, varDecl, expr);
        }

        public override void ExitUnitVarDecl([NotNull] UnitVarDeclContext context)
        {
            var qualifiedVars = Enumerable.Empty<QualifiedVarResult>();
            context.result    = ExitVarDecl(context.Start, context.Stop, qualifiedVars, context.isActual);
        }

        public override void ExitSingleVarDecl([NotNull] SingleVarDeclContext context)
        {
            var qualifiedVar = context.qualifiedVar()?.result;
            if (IsInvalid(qualifiedVar)) return;

            var qualifiedVars = new QualifiedVarResult[] { qualifiedVar };
            context.result    = ExitVarDecl(context.Start, context.Stop, qualifiedVars, context.isActual);
        }

        public override void ExitTupleVarDecl([NotNull] TupleVarDeclContext context)
        {
            var qualifiedVars = context.qualifiedVar().Select(x => x.result);
            if (qualifiedVars.Any(x => IsInvalid(x))) return;

            context.result = ExitVarDecl(context.Start, context.Stop, qualifiedVars, context.isActual);
        }

        private VarDeclResult ExitVarDecl(IToken start, IToken stop, IEnumerable<QualifiedVarResult> qualifiedVars, bool isActual)
        {
            if (!isActual && qualifiedVars.Any(x => x.Qualified.Inner.Type.ContainsNonDetFunc()))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"function arguments cannot contain nondeterministic function");
                return new VarDeclResult(start, stop);
            }

            var oldQual = (TeuchiUdonQualifier)null;
            if (isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var qualifier = TeuchiUdonQualifierStack.Instance.Peek();

            if (isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);

            return new VarDeclResult(start, stop, qualifier, qualifiedVars);
        }

        public override void ExitQualifiedVar([NotNull] QualifiedVarContext context)
        {
            var identifier = context.identifier()?.result;
            var qualified  = context.expr      ()?.result;
            if (IsInvalid(identifier)) return;

            if (qualified == null)
            {
                qualified = new ExprResult(context.Start, context.Stop, new UnknownTypeResult(context.Start, context.Stop)) { ReturnsValue = false };
            }
            else
            {
                if (!qualified.Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"qualified type {qualified.Inner.Type} is not a type");
                    context.result = new QualifiedVarResult(context.Start, context.Stop);
                    return;
                }
                qualified.ReturnsValue = false;
            }

            context.result = new QualifiedVarResult(context.Start, context.Stop, identifier, qualified);
        }

        public override void ExitIdentifier([NotNull] IdentifierContext context)
        {
            if (context.ChildCount == 0) return;

            var name       = context.GetText().Replace("@", "");
            context.result = new IdentifierResult(context.Start, context.Stop, name);
        }

        public override void ExitReturnUnitStatement([NotNull] ReturnUnitStatementContext context)
        {
            var value      = new ExprResult(context.Start, context.Stop, new UnitResult(context.Start, context.Stop));
            context.result = ExitReturnStatement(context.Start, context.Stop, value);
        }

        public override void ExitReturnValueStatement([NotNull] ReturnValueStatementContext context)
        {
            var value = context.expr()?.result;
            if (IsInvalid(value)) return;

            context.result = ExitReturnStatement(context.Start, context.Stop, value);
        }

        private JumpResult ExitReturnStatement(IToken start, IToken stop, ExprResult value)
        {
            var qb = TeuchiUdonQualifierStack.Instance.Peek().GetFuncBlock();
            if (qb == null)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, "no func exists for return");
                return new JumpResult(start, stop);
            }

            return StoreJumpResult
            (
                new JumpResult
                (
                    start,
                    stop,
                    value,
                    () => TeuchiUdonTables.Instance.Blocks.ContainsKey(qb) ? TeuchiUdonTables.Instance.Blocks[qb]        : TeuchiUdonBlock.InvalidBlock,
                    () => TeuchiUdonTables.Instance.Blocks.ContainsKey(qb) ? TeuchiUdonTables.Instance.Blocks[qb].Return : TeuchiUdonBlock.InvalidBlock
                )
            );
        }

        public override void ExitContinueUnitStatement([NotNull] ContinueUnitStatementContext context)
        {
            var value      = new ExprResult(context.Start, context.Stop, new UnitResult(context.Start, context.Stop));
            context.result = ExitContinueStatement(context.Start, context.Stop, value);
        }

        public override void ExitContinueValueStatement([NotNull] ContinueValueStatementContext context)
        {
            var value = context.expr()?.result;
            if (IsInvalid(value)) return;

            context.result = ExitContinueStatement(context.Start, context.Stop, value);
        }

        private JumpResult ExitContinueStatement(IToken start, IToken stop, ExprResult value)
        {
            var qb = TeuchiUdonQualifierStack.Instance.Peek().GetLoopBlock();
            if (qb == null)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, "no loop exists for continue");
                return new JumpResult(start, stop);
            }

            return StoreJumpResult
            (
                new JumpResult(start, stop, value, () => TeuchiUdonTables.Instance.Blocks[qb], () => TeuchiUdonTables.Instance.Blocks[qb].Continue)
            );
        }

        public override void ExitBreakUnitStatement([NotNull] BreakUnitStatementContext context)
        {
            var value      = new ExprResult(context.Start, context.Stop, new UnitResult(context.Start, context.Stop));
            context.result = ExitBreakStatement(context.Start, context.Stop, value);
        }

        public override void ExitBreakValueStatement([NotNull] BreakValueStatementContext context)
        {
            var value = context.expr()?.result;
            if (IsInvalid(value)) return;

            context.result = ExitBreakStatement(context.Start, context.Stop, value);
        }

        private JumpResult ExitBreakStatement(IToken start, IToken stop, ExprResult value)
        {
            var qb = TeuchiUdonQualifierStack.Instance.Peek().GetLoopBlock();
            if (qb == null)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, "no loop exists for break");
                return new JumpResult(start, stop);
            }

            return StoreJumpResult
            (
                new JumpResult(start, stop, value, () => TeuchiUdonTables.Instance.Blocks[qb], () => TeuchiUdonTables.Instance.Blocks[qb].Break)
            );
        }

        public override void ExitLetBindStatement([NotNull] LetBindStatementContext context)
        {
            var varBind = context.varBind()?.result;
            if (IsInvalid(varBind)) return;

            context.result = new LetBindResult(context.Start, context.Stop, varBind);
        }

        public override void ExitExprStatement([NotNull] ExprStatementContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr)) return;

            expr.ReturnsValue = false;
            context.result    = expr;
        }

        private TeuchiUdonScopeMode GetScopeMode(ParserRuleContext context)
        {
            return
                context.Parent is FuncExprContext ?
                    TeuchiUdonScopeMode.FuncBlock :
                context.Parent is ForExprContext   ||
                context.Parent is WhileExprContext ||
                context.Parent is LoopExprContext ?
                    TeuchiUdonScopeMode.LoopBlock :
                    TeuchiUdonScopeMode.Block;
        }

        public override void EnterUnitBlockExpr([NotNull] UnitBlockExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetBlockIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var scopeMode      = GetScopeMode(context);
            var scope          = new TeuchiUdonScope(new TeuchiUdonBlock(index, qual), scopeMode);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitUnitBlockExpr([NotNull] UnitBlockExprContext context)
        {
            var statements = context.statement().Select(x => x.result);
            if (statements.Any(x => IsInvalid(x))) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var type       = PrimitiveTypes.Instance.Unit;
            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var expr       = new ExprResult(context.Start, context.Stop, new UnitResult(context.Start, context.Stop));
            var block      = new BlockResult(context.Start, context.Stop, type, index, qual, statements, expr);
            context.result = new ExprResult(block.Start, block.Stop, block);
        }

        public override void EnterValueBlockExpr([NotNull] ValueBlockExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetBlockIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var scopeMode      = GetScopeMode(context);
            var scope          = new TeuchiUdonScope(new TeuchiUdonBlock(index, qual), scopeMode);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitValueBlockExpr([NotNull] ValueBlockExprContext context)
        {
            var expr       = context.expr()?.result;
            var statements = context.statement().Select(x => x.result);
            if (IsInvalid(expr) || statements.Any(x => IsInvalid(x))) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var type       = expr.Inner.Type;
            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var block      = new BlockResult(context.Start, context.Stop, type, index, qual, statements, expr);
            context.result = new ExprResult(block.Start, block.Stop, block);
        }

        public override void ExitParenExpr([NotNull] ParenExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr)) return;

            var type       = expr.Inner.Type;
            var paren      = new ParenResult(context.Start, context.Stop, type, expr);
            context.result = new ExprResult(paren.Start, paren.Stop, paren);
        }

        public override void ExitTupleExpr([NotNull] TupleExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length < 2 || exprs.Any(x => IsInvalid(x))) return;

            var type       = PrimitiveTypes.Instance.Tuple.ApplyArgsAsTuple(exprs.Select(x => x.Inner.Type));
            var tuple      = new TupleResult(context.Start, context.Stop, type, exprs);
            context.result = new ExprResult(tuple.Start, tuple.Stop, tuple);
        }

        public override void ExitEmptyArrayCtorExpr([NotNull] EmptyArrayCtorExprContext context)
        {
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var type       = PrimitiveTypes.Instance.Unknown.ToArrayType();
            var arrayCtor  = new ArrayCtorResult(context.Start, context.Stop, type, qual, Enumerable.Empty<IterExprResult>());
            context.result = new ExprResult(arrayCtor.Start, arrayCtor.Stop, arrayCtor);
        }

        public override void ExitArrayCtorExpr([NotNull] ArrayCtorExprContext context)
        {
            var iterExpr = context.iterExpr()?.result;
            if (IsInvalid(iterExpr)) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var type       = iterExpr.Type.ToArrayType();
            var arrayCtor  = new ArrayCtorResult(context.Start, context.Stop, type, qual, new IterExprResult[] { iterExpr });
            context.result = new ExprResult(arrayCtor.Start, arrayCtor.Stop, arrayCtor);
        }

        public override void ExitEmptyListCtorExpr([NotNull] EmptyListCtorExprContext context)
        {
            throw new NotImplementedException();
        }

        public override void ExitListCtorExpr([NotNull] ListCtorExprContext context)
        {
            throw new NotImplementedException();
        }

        public override void EnterLiteralExpr([NotNull] LiteralExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetLiteralIndex();
            context.tableIndex = index;
        }

        public override void ExitLiteralExpr([NotNull] LiteralExprContext context)
        {
            var literal = context.literal()?.result;
            if (IsInvalid(literal)) return;

            context.result = new ExprResult(literal.Start, literal.Stop, literal);
        }

        public override void ExitThisLiteralExpr([NotNull] ThisLiteralExprContext context)
        {
            var thisLiteral = context.thisLiteral()?.result;
            if (IsInvalid(thisLiteral)) return;

            context.result = new ExprResult(thisLiteral.Start, thisLiteral.Stop, thisLiteral);
        }

        public override void EnterInterpolatedRegularStringExpr([NotNull] InterpolatedRegularStringExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetLiteralIndex();
            context.tableIndex = index;
        }

        public override void ExitInterpolatedRegularStringExpr([NotNull] InterpolatedRegularStringExprContext context)
        {
            var str = context.interpolatedRegularString()?.result;
            if (IsInvalid(str)) return;

            context.result = new ExprResult(str.Start, str.Stop, str);
        }

        public override void ExitEvalVarExpr([NotNull] EvalVarExprContext context)
        {
            var identifier = context.identifier()?.result;
            if (IsInvalid(identifier)) return;

            var evalVar = (TypedResult)null;
            if (context.Parent is AccessExprContext)
            {
                evalVar = new EvalVarCandidateResult(context.Start, context.Stop, identifier);
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
                        evalVar  = new EvalVarResult(context.Start, context.Stop, type, v);
                        break;
                    }

                    var qt = new TeuchiUdonType(qual, identifier.Name);
                    if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                    {
                        var type  = TeuchiUdonTables.Instance.Types[qt];
                        var outer = PrimitiveTypes.Instance.Type.ApplyArgAsType(type);
                        evalVar   = new EvalTypeResult(context.Start, context.Stop, outer, type);
                        break;
                    }

                    if
                    (
                        TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(qt) &&
                        (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                    )
                    {
                        var type  = TeuchiUdonTables.Instance.GenericRootTypes[qt];
                        var outer = PrimitiveTypes.Instance.Type.ApplyArgAsType(type);
                        evalVar   = new EvalTypeResult(context.Start, context.Stop, outer, type);
                        break;
                    }
                }

                if (evalVar == null)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"'{identifier.Name}' is not defined");
                    context.result = new ExprResult(context.Start, context.Stop);
                    return;
                }
            }

            context.result = new ExprResult(evalVar.Start, evalVar.Stop, evalVar);
        }

        public override void ExitEvalTypeOfExpr([NotNull] EvalTypeOfExprContext context)
        {
            var typeOf     = new EvalTypeOfResult(context.Start, context.Stop);
            context.result = new ExprResult(typeOf.Start, typeOf.Stop, typeOf);
        }

        public override void ExitAccessExpr([NotNull] AccessExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

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
                        eval     = new EvalVarResult(varCandidate1.Start, varCandidate1.Stop, type, v);
                        break;
                    }

                    var qt = new TeuchiUdonType(qual, varCandidate1.Identifier.Name);
                    if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                    {
                        var type  = TeuchiUdonTables.Instance.Types[qt];
                        var outer = PrimitiveTypes.Instance.Type.ApplyArgAsType(type);
                        eval      = new EvalTypeResult(varCandidate1.Start, varCandidate1.Stop, outer, type);
                        break;
                    }

                    if
                    (
                        TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(qt) &&
                        (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                    )
                    {
                        var type  = TeuchiUdonTables.Instance.GenericRootTypes[qt];
                        var outer = PrimitiveTypes.Instance.Type.ApplyArgAsType(type);
                        eval      = new EvalTypeResult(varCandidate1.Start, varCandidate1.Stop, outer, type);
                        break;
                    }
                }

                var scopes  = new TeuchiUdonScope[] { new TeuchiUdonScope(new TextLabel(varCandidate1.Identifier.Name)) };
                var newQual = new TeuchiUdonQualifier(scopes);
                if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(newQual))
                {
                    var q    = TeuchiUdonTables.Instance.Qualifiers[newQual];
                    var type = PrimitiveTypes.Instance.Qual.ApplyArgAsQual(q);
                    eval     = new EvalQualifierResult(varCandidate1.Start, varCandidate1.Stop, type, q);
                }

                if (eval == null)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate1.Start, $"'{varCandidate1.Identifier.Name}' is not defined");
                    context.result = new ExprResult(varCandidate1.Start, varCandidate1.Stop);
                    return;
                }
                
                expr1 = new ExprResult(eval.Start, eval.Stop, eval);
            }
            var type1 = expr1.Inner.Type;

            if (expr2.Inner is EvalVarCandidateResult varCandidate2)
            {
                var eval = (TypedResult)null;
                do
                {
                    if (type1.LogicalTypeNameEquals(PrimitiveTypes.Instance.Qual))
                    {
                        var qual = type1.GetArgAsQual();
                        var qv   = new TeuchiUdonVar(qual, varCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                        {
                            var v    = TeuchiUdonTables.Instance.Vars[qv];
                            var type = v.Type;
                            eval     = new EvalVarResult(varCandidate2.Start, varCandidate2.Stop, type, v);
                            break;
                        }

                        var qt = new TeuchiUdonType(qual, varCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = PrimitiveTypes.Instance.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate2.Start, varCandidate2.Stop, outer, type);
                            break;
                        }

                        if
                        (
                            TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(qt) &&
                            (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                        )
                        {
                            var type  = TeuchiUdonTables.Instance.GenericRootTypes[qt];
                            var outer = PrimitiveTypes.Instance.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate2.Start, varCandidate2.Stop, outer, type);
                            break;
                        }

                        var scope    = new TeuchiUdonScope(new TextLabel(varCandidate2.Identifier.Name));
                        var appended = qual.Append(scope);
                        if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(appended))
                        {
                            var q    = TeuchiUdonTables.Instance.Qualifiers[appended];
                            var type = PrimitiveTypes.Instance.Qual.ApplyArgAsQual(q);
                            eval     = new EvalQualifierResult(varCandidate2.Start, varCandidate2.Stop, type, q);
                            break;
                        }
                    }
                    else if (type1.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type))
                    {
                        var t  = type1.GetArgAsType();
                        var sc = new TeuchiUdonScope(new TextLabel(t.LogicalName));
                        var q  = t.Qualifier.Append(sc);
                        var qt = new TeuchiUdonType(q, varCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = PrimitiveTypes.Instance.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate2.Start, varCandidate2.Stop, outer, type);
                            break;
                        }

                        if
                        (
                            TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(qt) &&
                            (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                        )
                        {
                            var type  = TeuchiUdonTables.Instance.GenericRootTypes[qt];
                            var outer = PrimitiveTypes.Instance.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate2.Start, varCandidate2.Stop, outer, type);
                            break;
                        }

                        if
                        (
                            TeuchiUdonTables.Instance.TypeToMethods.ContainsKey(type1) &&
                            TeuchiUdonTables.Instance.TypeToMethods[type1].ContainsKey(varCandidate2.Identifier.Name)
                        )
                        {
                            var ms   = TeuchiUdonTables.Instance.TypeToMethods[type1][varCandidate2.Identifier.Name];
                            var type = PrimitiveTypes.Instance.Method.ApplyArgsAsMethod(ms);
                            eval     = new MethodResult(varCandidate2.Start, varCandidate2.Stop, type, varCandidate2.Identifier);
                            break;
                        }

                        var qg = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetGetterName(varCandidate2.Identifier.Name));
                        var g  = TeuchiUdonTables.Instance.GetMostCompatibleMethodsWithoutInTypes(qg, 0).ToArray();
                        var qs = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetSetterName(varCandidate2.Identifier.Name));
                        var s  = TeuchiUdonTables.Instance.GetMostCompatibleMethodsWithoutInTypes(qs, 1).ToArray();
                        if (g.Length == 1 && s.Length == 1 && g[0].OutTypes.Length == 1 && IsValidSetter(expr1))
                        {
                            eval = new EvalGetterSetterResult(varCandidate2.Start, varCandidate2.Stop, g[0].OutTypes[0], qualifier, g[0], s[0]);
                            break;
                        }
                        else if (g.Length == 1 && g[0].OutTypes.Length == 1)
                        {
                            eval = new EvalGetterResult(varCandidate2.Start, varCandidate2.Stop, g[0].OutTypes[0], qualifier, g[0]);
                            break;
                        }
                        else if (s.Length == 1 && IsValidSetter(expr1))
                        {
                            eval = new EvalSetterResult(varCandidate2.Start, varCandidate2.Stop, s[0].InTypes[1], qualifier, s[0]);
                            break;
                        }
                        else if (g.Length >= 2 || s.Length >= 2)
                        {
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate2.Start, $"method '{varCandidate2.Identifier.Name}' has multiple overloads");
                            context.result = new ExprResult(varCandidate2.Start, varCandidate2.Stop);
                            return;
                        }

                        if (t.RealType.IsEnum)
                        {
                            var name  = varCandidate2.Identifier.Name;
                            var index = Array.IndexOf(t.RealType.GetEnumNames(), name);
                            if (index >= 0)
                            {
                                var value = t.RealType.GetEnumValues().GetValue(index);
                                eval = new LiteralResult(varCandidate2.Start, varCandidate2.Stop, t, TeuchiUdonTables.Instance.GetLiteralIndex(), name, value);
                                break;
                            }
                            else
                            {
                                TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate2.Start, $"'{name}' is not enum value");
                                context.result = new ExprResult(varCandidate2.Start, varCandidate2.Stop);
                                return;
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
                            var type = PrimitiveTypes.Instance.Method.ApplyArgsAsMethod(ms);
                            eval     = new MethodResult(varCandidate2.Start, varCandidate2.Stop, type, varCandidate2.Identifier);
                            break;
                        }

                        var qg = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetGetterName(varCandidate2.Identifier.Name));
                        var g  = TeuchiUdonTables.Instance.GetMostCompatibleMethodsWithoutInTypes(qg, 1).ToArray();
                        var qs = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetSetterName(varCandidate2.Identifier.Name));
                        var s  = TeuchiUdonTables.Instance.GetMostCompatibleMethodsWithoutInTypes(qs, 2).ToArray();
                        if (g.Length == 1 && s.Length == 1 && g[0].OutTypes.Length == 1 && IsValidSetter(expr1))
                        {
                            eval = new EvalGetterSetterResult(varCandidate2.Start, varCandidate2.Stop, g[0].OutTypes[0], qualifier, g[0], s[0]);
                            break;
                        }
                        else if (g.Length == 1 && g[0].OutTypes.Length == 1)
                        {
                            eval = new EvalGetterResult(varCandidate2.Start, varCandidate2.Stop, g[0].OutTypes[0], qualifier, g[0]);
                            break;
                        }
                        else if (s.Length == 1 && IsValidSetter(expr1))
                        {
                            eval = new EvalSetterResult(varCandidate2.Start, varCandidate2.Stop, PrimitiveTypes.Instance.Setter.ApplyArgAsSetter(s[0].InTypes[1]), qualifier, s[0]);
                            break;
                        }
                        else if (g.Length >= 2 || s.Length >= 2)
                        {
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate2.Start, $"method '{varCandidate2.Identifier.Name}' has multiple overloads");
                            context.result = new ExprResult(varCandidate2.Start, varCandidate2.Stop);
                            return;
                        }
                    }

                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate2.Start, $"'{varCandidate2.Identifier.Name}' is not defined");
                    context.result = new ExprResult(varCandidate2.Start, varCandidate2.Stop);
                    return;
                } while (false);

                expr2 = new ExprResult(eval.Start, eval.Stop, eval);
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(expr1.Start, $"invalid '{op}' operator");
                context.result = new ExprResult(expr1.Start, expr1.Stop);
                return;
            }

            var infix      = new InfixResult(expr1.Start, expr1.Stop, expr2.Inner.Type, qualifier, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        private bool IsValidSetter(ExprResult expr)
        {
            var type = expr.Inner.Type;
            if (type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type)) return true;
            if (type.RealType == null) return false;
            if (!type.RealType.IsValueType) return true;
            return expr.Inner is EvalVarResult;
        }

        public override void ExitCastExpr([NotNull] CastExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr)) return;

            var type = expr.Inner.Type;
            if (!type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression is not a type");
                return;
            }

            var cast = new EvalCastResult(context.Start, context.Stop, PrimitiveTypes.Instance.Cast.ApplyArgAsCast(type.GetArgAsType()), expr);
            context.result = new ExprResult(cast.Start, cast.Stop, cast);
        }

        private bool IsValidConvertType(TeuchiUdonType type)
        {
            return new TeuchiUdonType[]
            {
                PrimitiveTypes.Instance.Bool,
                PrimitiveTypes.Instance.Byte,
                PrimitiveTypes.Instance.Char,
                PrimitiveTypes.Instance.DateTime,
                PrimitiveTypes.Instance.Decimal,
                PrimitiveTypes.Instance.Double,
                PrimitiveTypes.Instance.Short,
                PrimitiveTypes.Instance.Int,
                PrimitiveTypes.Instance.Long,
                PrimitiveTypes.Instance.SByte,
                PrimitiveTypes.Instance.Float,
                PrimitiveTypes.Instance.String,
                PrimitiveTypes.Instance.UShort,
                PrimitiveTypes.Instance.UInt,
                PrimitiveTypes.Instance.ULong
            }
            .Any(x => type.LogicalTypeEquals(x));
        }

        public override void ExitEvalUnitFuncExpr([NotNull] EvalUnitFuncExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr)) return;

            var args       = Enumerable.Empty<ArgExprResult>();
            context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, expr, args);
        }

        public override void ExitEvalSingleFuncExpr([NotNull] EvalSingleFuncExprContext context)
        {
            var expr = context.expr   ()?.result;
            var arg  = context.argExpr()?.result;
            if (IsInvalid(expr) || IsInvalid(arg)) return;

            var args = new ArgExprResult[] { arg };
            context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, expr, args);
        }

        public override void ExitEvalTupleFuncExpr([NotNull] EvalTupleFuncExprContext context)
        {
            var expr = context.expr   ()?.result;
            var args = context.argExpr().Select(x => x?.result).ToArray();
            if (IsInvalid(expr) || args.Length < 2 || args.Any(x => IsInvalid(x))) return;

            context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, expr, args);
        }

        private ExprResult ExitEvalFuncExprWithArgs(IToken start, IToken stop, ExprResult expr, IEnumerable<ArgExprResult> argExprs)
        {
            var type     = expr.Inner.Type;
            var args     = argExprs.Select(x => x.Expr).ToArray();
            var argRefs  = argExprs.Select(x => x.Ref ).ToArray();
            var qual     = TeuchiUdonQualifierStack.Instance.Peek();
            var evalFunc = (TypedResult)null;

            if (type.IsFunc())
            {
                if (argRefs.Any(x => x))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"arguments of func cannot be ref");
                    return new ExprResult(start, stop);
                }
                else
                {
                    var argTypes = argExprs.Select(x => x.Expr.Inner.Type).ToArray();
                    var iType    = TeuchiUdonType.ToOneType(argTypes);
                    var oType    = PrimitiveTypes.Instance.Unknown;
                    if (type.IsAssignableFrom(PrimitiveTypes.Instance.DetFunc.ApplyArgsAsFunc(iType, oType)))
                    {
                        var outType = type.GetArgAsFuncOutType();
                        var index   = TeuchiUdonTables.Instance.GetEvalFuncIndex();
                        evalFunc    = new EvalFuncResult(start, stop, outType, index, qual, expr, args);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"arguments of func is not compatible");
                        return new ExprResult(start, stop);
                    }
                }
            }
            else if (type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Method) && expr.Inner.Instance != null)
            {
                var instanceType = expr.Inner.Instance.Inner.Type.RealType == null ? Enumerable.Empty<TeuchiUdonType>() : new TeuchiUdonType[] { expr.Inner.Instance.Inner.Type };
                var inTypes      = instanceType.Concat(args.Select(x => x.Inner.Type));
                var inRefs       = instanceType.Select(_ => false).Concat(argRefs);
                var ms           = type.GetMostCompatibleMethods(inTypes).ToArray();
                if (ms.Length == 0)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"method is not defined");
                    return new ExprResult(start, stop);
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
                        if (expr.Inner is InfixResult infix && infix.Op == "?.")
                        {
                            evalFunc = new EvalCoalescingMethodResult(start, stop, outType, qual, method, infix.Expr1, infix.Expr2, args);
                        }
                        else
                        {
                            evalFunc = new EvalMethodResult(start, stop, outType, qual, method, expr, args);
                        }
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"ref mark of method is not compatible");
                        return new ExprResult(start, stop);
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"method has multiple overloads");
                    return new ExprResult(start, stop);
                }
            }
            else if (type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type))
            {
                var inTypes = args.Select(x => x.Inner.Type);
                var qm      = new TeuchiUdonMethod(type, "ctor", inTypes);
                var ms      = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
                if (ms.Length == 0)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"ctor is not defined");
                    return new ExprResult(start, stop);
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
                        evalFunc = new EvalMethodResult(start, stop, outType, qual, method, expr, args);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"ref mark of ctor is not compatible");
                        return new ExprResult(start, stop);
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"ctor has multiple overloads");
                    return new ExprResult(start, stop);
                }
            }
            else if (type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Cast))
            {
                if (args.Length != 1 || argRefs.Any(x => x))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"cast must be specified with one argument");
                    return new ExprResult(start, stop);
                }
                else
                {
                    var type1 = type.GetArgAsCast();
                    var type2 = args[0].Inner.Type;
                    if (type1.IsAssignableFrom(type2) || type2.IsAssignableFrom(type1))
                    {
                        if (type2.ContainsUnknown())
                        {
                            args[0].Inner.BindType(type1);
                        }
                        evalFunc = new TypeCastResult(start, stop, type1, expr, args[0]);
                    }
                    else if (IsValidConvertType(type1) && IsValidConvertType(type2))
                    {
                        evalFunc = new ConvertCastResult(start, stop, type1, qual, expr, args[0]);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"specified type cannot be cast");
                        return new ExprResult(start, stop);
                    }
                }
            }
            else if (type.LogicalTypeEquals(PrimitiveTypes.Instance.TypeOf))
            {
                if (args.Length != 1 || argRefs.Any(x => x))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"typeof must be specified with one argument");
                    return new ExprResult(start, stop);
                }
                else if (args[0].Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type) && args[0].Inner.Type.GetArgAsType().RealType != null)
                {
                    evalFunc = new TypeOfResult(start, stop, args[0].Inner.Type.GetArgAsType());
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"typeof argument must be a type");
                    return new ExprResult(start, stop);
                }
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"expression is not a function or method");
                return new ExprResult(start, stop);
            }

            return new ExprResult(evalFunc.Start, evalFunc.Stop, evalFunc);
        }

        public override void ExitEvalSpreadFuncExpr([NotNull] EvalSpreadFuncExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x))) return;

            context.result = ExitEvalFuncExprWithSpread(context.Start, context.Stop, exprs[0], exprs[1]);
        }

        private ExprResult ExitEvalFuncExprWithSpread(IToken start, IToken stop, ExprResult expr, ExprResult arg)
        {
            var type     = expr.Inner.Type;
            var qual     = TeuchiUdonQualifierStack.Instance.Peek();
            var evalFunc = (TypedResult)null;

            if (!arg.Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"spread expression is not a tuple type");
                return new ExprResult(start, stop);
            }
            else if (type.IsFunc())
            {
                var argTypes = arg.Inner.Type.GetArgsAsTuple().ToArray();
                var iType    = TeuchiUdonType.ToOneType(argTypes);
                var oType    = PrimitiveTypes.Instance.Unknown;
                if (type.IsAssignableFrom(PrimitiveTypes.Instance.DetFunc.ApplyArgsAsFunc(iType, oType)))
                {
                    var outType = type.GetArgAsFuncOutType();
                    var index   = TeuchiUdonTables.Instance.GetEvalFuncIndex();
                    evalFunc    = new EvalSpreadFuncResult(start, stop, outType, index, qual, expr, arg);
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"arguments of func is not compatible");
                    return new ExprResult(start, stop);
                }
            }
            else if (type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Method) && expr.Inner.Instance != null)
            {
                var instanceType = expr.Inner.Instance.Inner.Type.RealType == null ? Enumerable.Empty<TeuchiUdonType>() : new TeuchiUdonType[] { expr.Inner.Instance.Inner.Type };
                var inTypes      = instanceType.Concat(arg.Inner.Type.GetArgsAsTuple());
                var ms           = type.GetMostCompatibleMethods(inTypes).ToArray();
                if (ms.Length == 0)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"method is not defined");
                    return new ExprResult(start, stop);
                }
                else if (ms.Length == 1)
                {
                    var method = ms[0];
                    if (method.InParamInOuts.All(x => x != TeuchiUdonMethodParamInOut.InOut))
                    {
                        var outType = TeuchiUdonType.ToOneType(method.OutTypes);
                        if (expr.Inner is InfixResult infix && infix.Op == "?.")
                        {
                            evalFunc = new EvalCoalescingSpreadMethodResult(start, stop, outType, qual, method, infix.Expr1, infix.Expr2, arg);
                        }
                        else
                        {
                            evalFunc = new EvalSpreadMethodResult(start, stop, outType, qual, method, expr, arg);
                        }
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"ref mark of method is not compatible");
                        return new ExprResult(start, stop);
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"method has multiple overloads");
                    return new ExprResult(start, stop);
                }
            }
            else if (type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type))
            {
                var inTypes = arg.Inner.Type.GetArgsAsTuple();
                var qm      = new TeuchiUdonMethod(type, "ctor", inTypes);
                var ms      = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
                if (ms.Length == 0)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"ctor is not defined");
                    return new ExprResult(start, stop);
                }
                else if (ms.Length == 1)
                {
                    var method = ms[0];
                    if (method.InParamInOuts.All(x => x != TeuchiUdonMethodParamInOut.InOut))
                    {
                        var outType = TeuchiUdonType.ToOneType(method.OutTypes);
                        evalFunc = new EvalSpreadMethodResult(start, stop, outType, qual, method, expr, arg);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"ref mark of ctor is not compatible");
                        return new ExprResult(start, stop);
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"ctor has multiple overloads");
                    return new ExprResult(start, stop);
                }
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"expression is not a function or method");
                return new ExprResult(start, stop);
            }

            return new ExprResult(evalFunc.Start, evalFunc.Stop, evalFunc);
        }

        public override void ExitEvalSingleKeyExpr([NotNull] EvalSingleKeyExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x))) return;

            var args       = exprs.Skip(1);
            var evalKey    = ExitEvalKeyExpr(context.Start, context.Stop, exprs[0], args);
            context.result = new ExprResult(evalKey.Start, evalKey.Stop, evalKey);
        }

        public override void ExitEvalTupleKeyExpr([NotNull] EvalTupleKeyExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length < 2 || exprs.Any(x => IsInvalid(x))) return;

            var args       = exprs.Skip(1);
            var evalKey    = ExitEvalKeyExpr(context.Start, context.Stop, exprs[0], args);
            context.result = new ExprResult(evalKey.Start, evalKey.Stop, evalKey);
        }

        private TypedResult ExitEvalKeyExpr(IToken start, IToken stop, ExprResult expr, IEnumerable<ExprResult> args)
        {
            var argArray = args.ToArray();
            if (expr.Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type) && args.All(x => x.Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Type)))
            {
                var exprType = expr.Inner.Type.GetArgAsType();
                if (!TeuchiUdonTables.Instance.GenericRootTypes.ContainsKey(exprType))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"specified key is invalid");
                    return new InvalidResult(start, stop);
                }

                var argTypes = args.Select(x => x.Inner.Type.GetArgAsType()).ToArray();
                if (exprType.LogicalTypeEquals(PrimitiveTypes.Instance.Array))
                {
                    if (argTypes.Length == 1)
                    {
                        var type = argTypes[0].ToArrayType();
                        return new EvalTypeResult(start, stop, PrimitiveTypes.Instance.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"specified key is invalid");
                        return new InvalidResult(start, stop);
                    }
                }
                else if (exprType.LogicalTypeEquals(PrimitiveTypes.Instance.List))
                {
                    if (argTypes.Length == 1)
                    {
                        var type = PrimitiveTypes.Instance.List
                            .ApplyArgAsList(argTypes[0])
                            .ApplyRealType(PrimitiveTypes.Instance.AnyArray.GetRealName(), PrimitiveTypes.Instance.AnyArray.RealType);
                        return new EvalTypeResult(start, stop, PrimitiveTypes.Instance.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"specified key is invalid");
                        return new InvalidResult(start, stop);
                    }
                }
                else if (exprType.LogicalTypeEquals(PrimitiveTypes.Instance.Func))
                {
                    if (argTypes.Length == 2)
                    {
                        var type = PrimitiveTypes.Instance.Func
                            .ApplyArgsAsFunc(argTypes[0], argTypes[1])
                            .ApplyRealType(PrimitiveTypes.Instance.UInt.GetRealName(), PrimitiveTypes.Instance.UInt.RealType);
                        return new EvalTypeResult(start, stop, PrimitiveTypes.Instance.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"specified key is invalid");
                        return new InvalidResult(start, stop);
                    }
                }
                else if (exprType.LogicalTypeEquals(PrimitiveTypes.Instance.DetFunc))
                {
                    if (argTypes.Length == 2)
                    {
                        var type = PrimitiveTypes.Instance.DetFunc
                            .ApplyArgsAsFunc(argTypes[0], argTypes[1])
                            .ApplyRealType(PrimitiveTypes.Instance.UInt.GetRealName(), PrimitiveTypes.Instance.UInt.RealType);
                        return new EvalTypeResult(start, stop, PrimitiveTypes.Instance.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"specified key is invalid");
                        return new InvalidResult(start, stop);
                    }
                }
                else
                {
                    var qt = new TeuchiUdonType(TeuchiUdonTables.GetGenericTypeName(exprType, argTypes), argTypes);
                    if (TeuchiUdonTables.Instance.LogicalTypes.ContainsKey(qt))
                    {
                        var type = TeuchiUdonTables.Instance.LogicalTypes[qt];
                        return new EvalTypeResult(start, stop, PrimitiveTypes.Instance.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"specified key is invalid");
                        return new InvalidResult(start, stop);
                    }
                }
            }
            else if (argArray.Length == 1 && argArray[0].Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Int))
            {
                if (expr.Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Array))
                {
                    var qual = TeuchiUdonQualifierStack.Instance.Peek();
                    return new EvalArrayIndexerResult(start, stop, expr.Inner.Type.GetArgAsArray(), qual, expr, argArray[0]);
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"specified key is invalid");
                    return new InvalidResult(start, stop);
                }
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(start, $"specified key is invalid");
                return new InvalidResult(start, stop);
            }
        }

        public override void ExitPrefixExpr([NotNull] PrefixExprContext context)
        {
            var op   = context.op?.Text;
            var expr = context.expr()?.result;
            if (op == null || IsInvalid(expr)) return;
            
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var prefix     = new PrefixResult(context.Start, context.Stop, expr.Inner.Type, qual, op, expr);
            context.result = new ExprResult(prefix.Start, prefix.Stop, prefix);
        }

        public override void ExitMultiplicationExpr([NotNull] MultiplicationExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitAdditionExpr([NotNull] AdditionExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitShiftExpr([NotNull] ShiftExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitRelationExpr([NotNull] RelationExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, PrimitiveTypes.Instance.Bool, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitEqualityExpr([NotNull] EqualityExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, PrimitiveTypes.Instance.Bool, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitLogicalAndExpr([NotNull] LogicalAndExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitLogicalXorExpr([NotNull] LogicalXorExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitLogicalOrExpr([NotNull] LogicalOrExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitConditionalAndExpr([NotNull] ConditionalAndExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            if (!expr1.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Bool) || !expr2.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Bool))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid operand type");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, PrimitiveTypes.Instance.Bool, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitConditionalOrExpr([NotNull] ConditionalOrExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            if (!expr1.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Bool) || !expr2.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Bool))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid operand type");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, PrimitiveTypes.Instance.Bool, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitCoalescingExpr([NotNull] CoalescingExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            if (!expr1.Inner.Type.IsAssignableFrom(expr2.Inner.Type) && !expr2.Inner.Type.IsAssignableFrom(expr1.Inner.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid operand type");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var type       = expr1.Inner.Type.IsAssignableFrom(expr2.Inner.Type) ? expr1.Inner.Type : expr2.Inner.Type;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, type, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
        }

        public override void ExitRightPipelineExpr([NotNull] RightPipelineExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x))) return;

            if (exprs[0].Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple))
            {
                context.result = ExitEvalFuncExprWithSpread(context.Start, context.Stop, exprs[1], exprs[0]);
            }
            else
            {
                var args       = new ArgExprResult[] { new ArgExprResult(exprs[0].Start, exprs[0].Stop, exprs[0], false) };
                context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, exprs[1], args);
            }
        }

        public override void ExitLeftPipelineExpr([NotNull] LeftPipelineExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x))) return;

            if (exprs[1].Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple))
            {
                context.result = ExitEvalFuncExprWithSpread(context.Start, context.Stop, exprs[0], exprs[1]);
            }
            else
            {
                var args       = new ArgExprResult[] { new ArgExprResult(exprs[1].Start, exprs[1].Stop, exprs[1], false) };
                context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, exprs[0], args);
            }
        }

        public override void ExitAssignExpr([NotNull] AssignExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2) return;

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2)) return;

            if (expr1.Inner.LeftValues.Length != 1 || !expr1.Inner.Type.IsAssignableFrom(expr2.Inner.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"cannot be assigned");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var infix      = new InfixResult(context.Start, context.Stop, PrimitiveTypes.Instance.Unit, qual, op, expr1, expr2);
            context.result = new ExprResult(infix.Start, infix.Stop, infix);
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
            if (IsInvalid(varBind) || IsInvalid(expr)) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var letInBind  = new LetInBindResult(context.Start, context.Stop, expr.Inner.Type, index, qual, varBind, expr);
            context.result = new ExprResult(letInBind.Start, letInBind.Stop, letInBind);
        }

        public override void ExitIfExpr([NotNull] IfExprContext context)
        {
            var isoExpr   = context.isoExpr  ()?.result;
            var statement = context.statement()?.result;
            if (IsInvalid(isoExpr) || IsInvalid(statement)) return;

            if (!isoExpr.Expr.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Bool))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"condition expression must be bool type");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            if (statement is LetBindResult)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"if expression cannot contain let bind");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var if_        = new IfResult(context.Start, context.Stop, PrimitiveTypes.Instance.Unit, new ExprResult[] { isoExpr.Expr }, new StatementResult[] { statement });
            context.result = new ExprResult(if_.Start, if_.Stop, if_);
        }

        public override void ExitIfElifExpr([NotNull] IfElifExprContext context)
        {
            var isoExprs   = context.isoExpr  ().Select(x => x?.result);
            var statements = context.statement().Select(x => x?.result);
            if (isoExprs.Any(x => IsInvalid(x)) || statements.Any(x => IsInvalid(x))) return;

            if (!isoExprs.All(x => x.Expr.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Bool)))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"condition expression must be bool type");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            if (statements.Any(x => x is LetBindResult))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"if expression cannot contain let bind");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var if_        = new IfResult(context.Start, context.Stop, PrimitiveTypes.Instance.Unit, isoExprs.Select(x => x.Expr), statements);
            context.result = new ExprResult(if_.Start, if_.Stop, if_);
        }

        public override void ExitIfElseExpr([NotNull] IfElseExprContext context)
        {
            var isoExprs = context.isoExpr().Select(x => x?.result);
            if (isoExprs.Any(x => IsInvalid(x))) return;

            var exprs = isoExprs.Select(x => x.Expr).ToArray();
            if (!exprs[0].Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Bool))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"condition expression must be bool type");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var type = TeuchiUdonType.GetUpperType(new TeuchiUdonType[] { exprs[1].Inner.Type, exprs[2].Inner.Type });
            if (type.ContainsUnknown())
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"if expression types are not compatible");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var if_        = new IfElseResult(context.Start, context.Stop, type, new ExprResult[] { exprs[0] }, new ExprResult[] { exprs[1] }, exprs[2]);
            context.result = new ExprResult(if_.Start, if_.Stop, if_);
        }

        public override void ExitIfElifElseExpr([NotNull] IfElifElseExprContext context)
        {
            var isoExprs = context.isoExpr().Select(x => x?.result);
            if (isoExprs.Any(x => IsInvalid(x))) return;

            var exprs      = isoExprs.Select(x => x.Expr).ToArray();
            var init       = exprs.Take(exprs.Length - 1);
            var conditions = init.Where((x, i) => i % 2 == 0);
            var thenParts  = init.Where((x, i) => i % 2 == 1);
            var elsePart   = exprs[exprs.Length - 1];

            if (!conditions.All(x => x.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Bool)))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"condition expression must be bool type");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var type = TeuchiUdonType.GetUpperType(thenParts.Concat(new ExprResult[] { elsePart }).Select(x => x.Inner.Type));
            if (type.ContainsUnknown())
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"if expression types are not compatible");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var if_        = new IfElseResult(context.Start, context.Stop, type, conditions, thenParts, elsePart);
            context.result = new ExprResult(if_.Start, if_.Stop, if_);
        }

        public override void ExitWhileExpr([NotNull] WhileExprContext context)
        {
            var isoExpr = context.isoExpr()?.result;
            var expr    = context.expr   ()?.result;
            if (IsInvalid(isoExpr) || IsInvalid(expr)) return;

            if (!isoExpr.Expr.Inner.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Bool))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"condition expression must be bool type");
                context.result = new ExprResult(context.Start, context.Stop);
                return;
            }

            var while_     = new WhileResult(context.Start, context.Stop, PrimitiveTypes.Instance.Unit, isoExpr.Expr, expr);
            context.result = new ExprResult(while_.Start, while_.Stop, while_);
        }

        public override void EnterForExpr([NotNull] ForExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetBlockIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var scope          = new TeuchiUdonScope(new TeuchiUdonFor(index), TeuchiUdonScopeMode.For);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitForExpr([NotNull] ForExprContext context)
        {
            var forBinds = context.forBind().Select(x => x?.result);
            var expr     = context.expr()?.result;
            if (forBinds.Any(x => IsInvalid(x)) || IsInvalid(expr)) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var index      = context.tableIndex;
            var for_       = new ForResult(context.Start, context.Stop, PrimitiveTypes.Instance.Unit, index, forBinds, expr);
            context.result = new ExprResult(for_.Start, for_.Stop, for_);
        }

        public override void ExitLoopExpr([NotNull] LoopExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr)) return;
            
            var loop       = new LoopResult(context.Start, context.Stop, PrimitiveTypes.Instance.Unit, expr);
            context.result = new ExprResult(loop.Start, loop.Stop, loop);
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
            if (IsInvalid(varDecl) || IsInvalid(expr)) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var qual          = TeuchiUdonQualifierStack.Instance.Peek();
            var inType        = TeuchiUdonType.ToOneType(varDecl.Types);
            var outType       = expr.Inner.Type;
            var deterministic = expr.Inner.Deterministic && varDecl.Vars.All(x => !x.Type.ContainsFunc());

            var type =
                deterministic ?
                    new TeuchiUdonType
                    (
                        TeuchiUdonQualifier.Top,
                        PrimitiveTypes.Instance.DetFunc.Name,
                        new TeuchiUdonType[] { inType, outType },
                        PrimitiveTypes.Instance.DetFunc.LogicalName,
                        PrimitiveTypes.Instance.UInt.GetRealName(),
                        PrimitiveTypes.Instance.UInt.RealType
                    ) :
                    new TeuchiUdonType
                    (
                        TeuchiUdonQualifier.Top,
                        PrimitiveTypes.Instance.Func.Name,
                        new TeuchiUdonType[] { inType, outType },
                        PrimitiveTypes.Instance.Func.LogicalName,
                        PrimitiveTypes.Instance.UInt.GetRealName(),
                        PrimitiveTypes.Instance.UInt.RealType
                    );

            var varBind = qual.GetLast<TeuchiUdonVarBind>();
            var args    = varDecl.Vars;
            if (varBind != null && varBind.Qualifier == TeuchiUdonQualifier.Top && varBind.VarNames.Length == 1 && TeuchiUdonTables.Instance.Events.ContainsKey(varBind.VarNames[0]))
            {
                var ev = TeuchiUdonTables.Instance.Events[varBind.VarNames[0]];
                if (ev.OutTypes.Length != args.Length)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"arguments of '{varBind.VarNames[0]}' event is not compatible");
                    context.result = new ExprResult(context.Start, context.Stop); 
                    return;
                }
                else
                {
                    var evTypes  = TeuchiUdonType.ToOneType(ev.OutTypes);
                    var argTypes = TeuchiUdonType.ToOneType(args.Select(x => x.Type));
                    if (evTypes.IsAssignableFrom(argTypes))
                    {
                        args =
                            args
                            .Zip(ev.OutParamUdonNames, (a, n) => (  a,   n))
                            .Zip(ev.OutTypes         , (x, t) => (x.a, x.n, t))
                            .Select(x =>
                                {
                                    var name = TeuchiUdonTables.GetEventParamName(ev.Name, x.n);
                                    if (!TeuchiUdonTables.IsValidVarName(name))
                                    {
                                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"'{name}' is invalid variable name");
                                        return x.a;
                                    }
                                    
                                    var v = new TeuchiUdonVar(TeuchiUdonTables.Instance.GetVarIndex(), TeuchiUdonQualifier.Top, name, x.t, false, true);
                                    if (TeuchiUdonTables.Instance.Vars.ContainsKey(v))
                                    {
                                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"'{v.Name}' conflicts with another variable");
                                        return x.a;
                                    }
                                    else
                                    {
                                        TeuchiUdonTables.Instance.Vars.Add(v, v);
                                        return v;
                                    }
                                })
                            .ToArray();
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"arguments of '{varBind.VarNames[0]}' event is not compatible");
                        context.result = new ExprResult(context.Start, context.Stop);
                        return;
                    }
                }
            }

            var index      = context.tableIndex;
            var func       = new FuncResult(context.Start, context.Stop, type, index, qual, args, varDecl, expr, deterministic);
            context.result = new ExprResult(func.Start, func.Stop, func);
        }
        
        public override void ExitElementsIterExpr([NotNull] ElementsIterExprContext context)
        {
            var isoExprs = context.isoExpr().Select(x => x?.result);
            if (isoExprs.Any(x => IsInvalid(x))) return;

            var exprs = isoExprs.Select(x => x.Expr);
            var type  = TeuchiUdonType.GetUpperType(exprs.Select(x => x.Inner.Type));
            if (type.LogicalTypeEquals(PrimitiveTypes.Instance.Unknown))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"array element types are incompatible");
                context.result = new ElementsIterExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            context.result = new ElementsIterExprResult(context.Start, context.Stop, type, qual, exprs);
        }

        public override void ExitRangeIterExpr([NotNull] RangeIterExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x))) return;

            if (!exprs[0].Inner.Type.IsSignedIntegerType() || !exprs[1].Inner.Type.IsSignedIntegerType())
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"range expression is not signed integer type");
                context.result = new RangeIterExprResult(context.Start, context.Stop);
                return;
            }
            else if (!exprs[0].Inner.Type.LogicalTypeEquals(exprs[1].Inner.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"range expression type is incompatible");
                context.result = new RangeIterExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            context.result = new RangeIterExprResult(context.Start, context.Stop, exprs[0].Inner.Type, qual, exprs[0], exprs[1]);
        }

        public override void ExitSteppedRangeIterExpr([NotNull] SteppedRangeIterExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 3 || exprs.Any(x => IsInvalid(x))) return;

            if (!exprs[0].Inner.Type.IsSignedIntegerType() || !exprs[1].Inner.Type.IsSignedIntegerType() || !exprs[2].Inner.Type.IsSignedIntegerType())
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"range expression is not signed integer type");
                context.result = new SteppedRangeIterExprResult(context.Start, context.Stop);
                return;
            }
            else if (!exprs[0].Inner.Type.LogicalTypeEquals(exprs[1].Inner.Type) || !exprs[0].Inner.Type.LogicalTypeEquals(exprs[2].Inner.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"range expression type is incompatible");
                context.result = new SteppedRangeIterExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            context.result = new SteppedRangeIterExprResult(context.Start, context.Stop, exprs[0].Inner.Type, qual, exprs[0], exprs[1], exprs[2]);
        }

        public override void ExitSpreadIterExpr([NotNull] SpreadIterExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr)) return;

            if (!expr.Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Array))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"spread expression is not a array type");
                context.result = new SpreadIterExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            context.result = new SpreadIterExprResult(context.Start, context.Stop, expr.Inner.Type.GetArgAsArray(), qual, expr);
        }

        public override void ExitIsoExpr([NotNull] IsoExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr)) return;

            context.result = new IsoExprResult(context.Start, context.Stop, expr);
        }

        public override void ExitArgExpr([NotNull] ArgExprContext context)
        {
            var expr = context.expr()?.result;
            var rf   = context.REF() != null;
            if (IsInvalid(expr)) return;

            context.result = new ArgExprResult(context.Start, context.Stop, expr, rf);
        }

        public override void EnterLetForBind([NotNull] LetForBindContext context)
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
                varDecl is TupleVarDeclContext  tv ? tv.qualifiedVar().Select(x => x?.identifier()?.GetText() ?? "").ToArray() : Enumerable.Empty<string>();
            var scope   = new TeuchiUdonScope(new TeuchiUdonVarBind(index, qual, varNames), TeuchiUdonScopeMode.VarBind);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitLetForBind([NotNull] LetForBindContext context)
        {
            var varDecl     = context.varDecl    ()?.result;
            var forIterExpr = context.forIterExpr()?.result;
            if (IsInvalid(varDecl) || IsInvalid(forIterExpr)) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var index = context.tableIndex;
            var qual  = TeuchiUdonQualifierStack.Instance.Peek();

            var vars = (TeuchiUdonVar[])null;
            if (varDecl.Vars.Length == 1)
            {
                var v = varDecl.Vars[0];
                var t = forIterExpr.Type;
                if (v.Type.IsAssignableFrom(t))
                {
                    if (t.ContainsUnknown())
                    {
                        forIterExpr.BindType(v.Type);
                    }

                    vars = new TeuchiUdonVar[]
                    {
                        new TeuchiUdonVar
                        (
                            TeuchiUdonTables.Instance.GetVarIndex(),
                            v.Qualifier,
                            v.Name,
                            v.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Unknown) ? t : v.Type,
                            true,
                            false
                        )
                    };
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variable");
                    vars = Array.Empty<TeuchiUdonVar>();
                }
            }
            else if (varDecl.Vars.Length >= 2)
            {
                if (forIterExpr.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Tuple))
                {
                    var vs = varDecl.Vars;
                    var ts = forIterExpr.Type.GetArgsAsTuple().ToArray();
                    if (vs.Length == ts.Length && varDecl.Types.Zip(ts, (v, t) => (v, t)).All(x => x.v.IsAssignableFrom(x.t)))
                    {
                        if (forIterExpr.Type.ContainsUnknown())
                        {
                            forIterExpr.BindType(TeuchiUdonType.ToOneType(vs.Select(x => x.Type)));
                        }
                        
                        vars = varDecl.Vars
                            .Zip(ts, (v, t) => (v, t))
                            .Select(x =>
                                new TeuchiUdonVar
                                (
                                    TeuchiUdonTables.Instance.GetVarIndex(),
                                    x.v.Qualifier,
                                    x.v.Name,
                                    x.v.Type.LogicalTypeEquals(PrimitiveTypes.Instance.Unknown) ? x.t : x.v.Type,
                                    true,
                                    false
                                )
                            )
                            .ToArray();
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variables");
                        vars = Array.Empty<TeuchiUdonVar>();
                    }
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression cannot be assigned to variables");
                    vars = Array.Empty<TeuchiUdonVar>();
                }
            }

            if (forIterExpr.Type.IsFunc())
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"function variable cannot be mutable");
            }

            context.result = new LetForBindResult(context.Start, context.Stop, index, qual, vars, varDecl, forIterExpr);
        }

        public override void ExitAssignForBind([NotNull] AssignForBindContext context)
        {
            var expr        = context.expr       ()?.result;
            var forIterExpr = context.forIterExpr()?.result;
            if (IsInvalid(expr) || IsInvalid(forIterExpr)) return;

            if (expr.Inner.LeftValues.Length != 1 || !expr.Inner.Type.IsAssignableFrom(forIterExpr.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"cannot be assigned");
                context.result = new AssignForBindResult(context.Start, context.Stop);
                return;
            }

            context.result = new AssignForBindResult(context.Start, context.Stop, expr, forIterExpr);
        }

        public override void ExitRangeForIterExpr([NotNull] RangeForIterExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x))) return;

            if (!exprs[0].Inner.Type.IsSignedIntegerType() || !exprs[1].Inner.Type.IsSignedIntegerType())
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"range expression is not signed integer type");
                context.result = new RangeIterExprResult(context.Start, context.Stop);
                return;
            }
            else if (!exprs[0].Inner.Type.LogicalTypeEquals(exprs[1].Inner.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"range expression type is incompatible");
                context.result = new RangeIterExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            context.result = new RangeIterExprResult(context.Start, context.Stop, exprs[0].Inner.Type, qual, exprs[0], exprs[1]);
        }

        public override void ExitSteppedRangeForIterExpr([NotNull] SteppedRangeForIterExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 3 || exprs.Any(x => IsInvalid(x))) return;

            if (!exprs[0].Inner.Type.IsSignedIntegerType() || !exprs[1].Inner.Type.IsSignedIntegerType() || !exprs[2].Inner.Type.IsSignedIntegerType())
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"range expression is not signed integer type");
                context.result = new SteppedRangeIterExprResult(context.Start, context.Stop);
                return;
            }
            else if (!exprs[0].Inner.Type.LogicalTypeEquals(exprs[1].Inner.Type) || !exprs[0].Inner.Type.LogicalTypeEquals(exprs[2].Inner.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"range expression type is incompatible");
                context.result = new SteppedRangeIterExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            context.result = new SteppedRangeIterExprResult(context.Start, context.Stop, exprs[0].Inner.Type, qual, exprs[0], exprs[1], exprs[2]);
        }

        public override void ExitSpreadForIterExpr([NotNull] SpreadForIterExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr)) return;

            if (!expr.Inner.Type.LogicalTypeNameEquals(PrimitiveTypes.Instance.Array))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"spread expression is not a array type");
                context.result = new SpreadIterExprResult(context.Start, context.Stop);
                return;
            }

            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            context.result = new SpreadIterExprResult(context.Start, context.Stop, expr.Inner.Type.GetArgAsArray(), qual, expr);
        }

        public override void ExitUnitLiteral([NotNull] UnitLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var type       = PrimitiveTypes.Instance.Unit;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, "()", null);
        }

        public override void ExitNullLiteral([NotNull] NullLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text       = context.GetText();
            var type       = PrimitiveTypes.Instance.NullType;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, text, null);
        }

        public override void ExitBoolLiteral([NotNull] BoolLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text       = context.GetText();
            var type       = PrimitiveTypes.Instance.Bool;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToBoolValue(context.Start, text);
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, text, value);
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
            var type  = PrimitiveTypes.Instance.Int;

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = PrimitiveTypes.Instance.ULong;
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = PrimitiveTypes.Instance.Long;
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = PrimitiveTypes.Instance.UInt;
            }

            if (count >= 2 && text.StartsWith("0"))
            {
                index++;
                count--;
                basis = 8;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue(context.Start, type, text.Substring(index, count), basis);
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, text, value);
        }

        public override void ExitHexIntegerLiteral([NotNull] HexIntegerLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text = context.GetText().Replace("_", "").ToLower();

            var index = 2;
            var count = text.Length - 2;
            var basis = 16;
            var type  = PrimitiveTypes.Instance.Int;

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = PrimitiveTypes.Instance.ULong;
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = PrimitiveTypes.Instance.Long;
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = PrimitiveTypes.Instance.UInt;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue(context.Start, type, text.Substring(index, count), basis);
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, text, value);
        }

        public override void ExitBinIntegerLiteral([NotNull] BinIntegerLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text = context.GetText().Replace("_", "").ToLower();

            var index = 2;
            var count = text.Length - 2;
            var basis = 2;
            var type  = PrimitiveTypes.Instance.Int;

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = PrimitiveTypes.Instance.ULong;
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = PrimitiveTypes.Instance.Long;
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = PrimitiveTypes.Instance.UInt;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue(context.Start, type, text.Substring(index, count), basis);
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, text, value);
        }

        private object ToIntegerValue(IToken token, TeuchiUdonType type, string text, int basis)
        {
            if (type.LogicalTypeEquals(PrimitiveTypes.Instance.Int))
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
            else if (type.LogicalTypeEquals(PrimitiveTypes.Instance.UInt))
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
            else if (type.LogicalTypeEquals(PrimitiveTypes.Instance.Long))
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
            else if (type.LogicalTypeEquals(PrimitiveTypes.Instance.ULong))
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
            var type  = PrimitiveTypes.Instance.Float;

            if (text.EndsWith("f"))
            {
                count--;
            }
            else if (text.EndsWith("d"))
            {
                count--;
                type = PrimitiveTypes.Instance.Double;
            }
            else if (text.EndsWith("m"))
            {
                count--;
                type = PrimitiveTypes.Instance.Decimal;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToRealValue(context.Start, type, text.Substring(0, count));
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, text, value);
        }

        private object ToRealValue(IToken token, TeuchiUdonType type, string text)
        {
            if (type.LogicalTypeEquals(PrimitiveTypes.Instance.Float))
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
            else if (type.LogicalTypeEquals(PrimitiveTypes.Instance.Double))
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
            else if (type.LogicalTypeEquals(PrimitiveTypes.Instance.Decimal))
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
            var type       = PrimitiveTypes.Instance.Char;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToCharacterValue(context.Start, text.Substring(1, text.Length - 2));
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, text, value);
        }

        public override void ExitRegularString([NotNull] RegularStringContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text       = context.GetText();
            var type       = PrimitiveTypes.Instance.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToRegularStringValue(context.Start, text.Substring(1, text.Length - 2));
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, text, value);
        }

        public override void ExitVervatiumString([NotNull] VervatiumStringContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null) return;

            var text       = context.GetText();
            var type       = PrimitiveTypes.Instance.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToVervatiumStringValue(context.Start, text.Substring(2, text.Length - 3));
            context.result = new LiteralResult(context.Start, context.Stop, type, tableIndex, text, value);
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
            var type       = PrimitiveTypes.Instance.String;
            context.result = new ThisResult(context.Start, context.Stop);
        }

        public override void ExitInterpolatedRegularString([NotNull] InterpolatedRegularStringContext context)
        {
            var parts = context.interpolatedRegularStringPart().Select(x => x?.result);
            if (parts.Any(x => IsInvalid(x))) return;

            var exprs = parts.Where(x => x is ExprInterpolatedStringPartResult).Select(x => ((ExprInterpolatedStringPartResult)x).Expr);
            if (exprs.Any(x => !x.Inner.Type.IsDotNetType()))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"expression type is incompatible");
                return;
            }

            var i = 0;
            var partStrings =
                parts.Select(x =>
                    x is RegularStringInterpolatedStringPartResult str  ? str.RawString   :
                    x is ExprInterpolatedStringPartResult          expr ? "{" + (i++) + "}" :
                    ""
                );

            var joined     = string.Join("", partStrings);
            var literal    = TeuchiUdonLiteral.CreateValue(((InterpolatedRegularStringExprContext)context.Parent).tableIndex, joined, PrimitiveTypes.Instance.String);
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            context.result = new InterpolatedStringResult(context.Start, context.Stop, qual, literal, exprs);
        }

        public override void ExitRegularStringInterpolatedStringPart([NotNull] RegularStringInterpolatedStringPartContext context)
        {
            var rawString = context.REGULAR_STRING_INSIDE()?.GetText();
            if (rawString == null) return;

            var invalid = Regex.Match(rawString, @"\{\d+\}");
            if (invalid.Success)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"invalid word '{invalid.Value}' detected");
                return;
            }

            context.result = new RegularStringInterpolatedStringPartResult(context.Start, context.Stop, rawString);
        }

        public override void ExitExprInterpolatedStringPart([NotNull] ExprInterpolatedStringPartContext context)
        {
            var isoExpr = context.isoExpr()?.result;
            if (IsInvalid(isoExpr)) return;

            context.result = new ExprInterpolatedStringPartResult(context.Start, context.Stop, isoExpr.Expr);
        }
    }
}
