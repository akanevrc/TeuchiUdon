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
        private List<JumpResult> Jumps { get; } = new List<JumpResult>();

        private TeuchiUdonPrimitives Primitives { get; }
        private TeuchiUdonStaticTables StaticTables { get; }
        private TeuchiUdonInvalids Invalids { get; }
        private TeuchiUdonLogicalErrorHandler LogicalErrorHandler { get; }
        private TeuchiUdonTables Tables { get; }
        private TeuchiUdonTypeOps TypeOps { get; }
        private TeuchiUdonTableOps TableOps { get; }
        private TeuchiUdonQualifierStack QualifierStack { get; }
        private TeuchiUdonOutValuePool OutValuePool { get; }
        private TeuchiUdonSyntaxOps SyntaxOps { get; }
        private TeuchiUdonParserResultOps ParserResultOps { get; }

        public TeuchiUdonListener
        (
            TeuchiUdonPrimitives primitives,
            TeuchiUdonStaticTables staticTables,
            TeuchiUdonInvalids invalids,
            TeuchiUdonLogicalErrorHandler logicalErrorHandler,
            TeuchiUdonTables tables,
            TeuchiUdonTypeOps typeOps,
            TeuchiUdonTableOps tableOps,
            TeuchiUdonQualifierStack qualifierStack,
            TeuchiUdonOutValuePool outValuePool,
            TeuchiUdonSyntaxOps syntaxOps,
            TeuchiUdonParserResultOps parserResultOps
        )
        {
            Primitives          = primitives;
            StaticTables        = staticTables;
            Invalids            = invalids;
            LogicalErrorHandler = logicalErrorHandler;
            Tables              = tables;
            TypeOps             = typeOps;
            TableOps            = tableOps;
            QualifierStack      = qualifierStack;
            OutValuePool        = outValuePool;
            SyntaxOps           = syntaxOps;
            ParserResultOps     = parserResultOps;
        }

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
                if (!TypeOps.IsAssignableFrom(block.Type, jump.Value.Inner.Type))
                {
                    LogicalErrorHandler.ReportError(jump.Start, $"jump value type is not compatible");
                }
            }
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
                OutValuePool.PushScope(QualifierStack.Peek().GetFuncQualifier());
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
                OutValuePool.PopScope(QualifierStack.Peek().GetFuncQualifier());
            }
            else if (context is ExprContext expr && expr.result != null)
            {
                foreach (var o in expr.result.Inner.ReleasedChildren.SelectMany(x => x.ReleasedOutValues)) OutValuePool.ReleaseOutValue(o);
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
                context.result = ParserResultOps.CreateTarget(context.Start, context.Stop);
                return;
            }

            TargetResult   = ParserResultOps.CreateTarget(context.Start, context.Stop, body);
            context.result = TargetResult;
        }

        public override void EnterBody([NotNull] BodyContext context)
        {
            QualifierStack.Push(TeuchiUdonQualifier.Top);
        }

        public override void ExitBody([NotNull] BodyContext context)
        {
            var topStatements = context.topStatement().Select(x => x.result).ToArray();
            if (topStatements.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateBody(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateBody(context.Start, context.Stop, topStatements);
        }

        public override void ExitVarBindTopStatement([NotNull] VarBindTopStatementContext context)
        {
            var varBind = context.varBind()?.result;
            var attrs   = context.varAttr().Select(x => x.result);
            if (IsInvalid(varBind) || attrs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                return;
            }

            var (pub, sync, valid) = ExtractFromVarAttrs(attrs);
            if (!valid)
            {
                context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                return;
            }

            if (varBind.Vars.Length != 1)
            {
                if (pub || sync != TeuchiUdonSyncMode.Disable)
                {
                    LogicalErrorHandler.ReportError(context.Start, $"tuple cannot be specified with any attributes");
                    context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                    return;
                }
            }
            else if (TypeOps.IsFunc(varBind.Vars[0].Type))
            {
                var v = varBind.Vars[0];

                if (varBind.Expr.Inner is FuncResult func)
                {
                    if ((pub || StaticTables.Events.ContainsKey(v.Name)) && !Tables.EventFuncs.ContainsKey(v))
                    {
                        Tables.EventFuncs.Add(v, func.Func);
                    }

                    if (sync != TeuchiUdonSyncMode.Disable)
                    {
                        LogicalErrorHandler.ReportError(context.Start, $"function cannot be specified with @sync, @linear, or @smooth");
                        context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                        return;
                    }
                }
                else
                {
                    LogicalErrorHandler.ReportError(context.Start, $"toplevel variable cannot be assigned from function indirectly");
                    context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                    return;
                }
            }
            else
            {
                var v = varBind.Vars[0];

                if (StaticTables.Events.ContainsKey(v.Name))
                {
                    LogicalErrorHandler.ReportError(context.Start, $"event must be function");
                    context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                    return;
                }

                if (pub)
                {
                    if (varBind.Expr.Inner is LiteralResult literal)
                    {
                        if (!Tables.PublicVars.ContainsKey(v))
                        {
                            Tables.PublicVars.Add(v, literal.Literal);
                        }
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(context.Start, $"public valiable cannot be bound non-literal expression");
                        context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                        return;
                    }
                }

                if (sync == TeuchiUdonSyncMode.Sync && !TypeOps.IsSyncableType(v.Type))
                {
                    LogicalErrorHandler.ReportError(context.Start, $"invalid valiable type to be specified with @sync");
                    context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                    return;
                }
                else if (sync == TeuchiUdonSyncMode.Linear && !TypeOps.IsLinearSyncableType(v.Type))
                {
                    LogicalErrorHandler.ReportError(context.Start, $"invalid valiable type to be specified with @linear");
                    context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                    return;
                }
                else if (sync == TeuchiUdonSyncMode.Smooth && !TypeOps.IsSmoothSyncableType(v.Type))
                {
                    LogicalErrorHandler.ReportError(context.Start, $"invalid valiable type to be specified with @smooth");
                    context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop);
                    return;
                }
                else if (sync != TeuchiUdonSyncMode.Disable)
                {
                    if (!Tables.SyncedVars.ContainsKey(v))
                    {
                        Tables.SyncedVars.Add(v, sync);
                    }
                }
            }

            context.result = ParserResultOps.CreateTopBind(context.Start, context.Stop, varBind, pub, sync);
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
                        LogicalErrorHandler.ReportError(publicAttr.Start, $"multiple @public detected");
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
                        LogicalErrorHandler.ReportError(syncAttr.Start, $"multiple @sync, @linear, or @smooth detected");
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
                context.result = ParserResultOps.CreateTopExpr(context.Start, context.Stop);
                return;
            }

            expr.ReturnsValue = false;
            context.result = ParserResultOps.CreateTopExpr(context.Start, context.Stop, expr);
        }

        public override void ExitPublicVarAttr([NotNull] PublicVarAttrContext context)
        {
            context.result = ParserResultOps.CreatePublicVarAttr(context.Start, context.Stop);
        }

        public override void ExitSyncVarAttr([NotNull] SyncVarAttrContext context)
        {
            context.result = ParserResultOps.CreateSyncVarAttr(context.Start, context.Stop, TeuchiUdonSyncMode.Sync);
        }

        public override void ExitLinearVarAttr([NotNull] LinearVarAttrContext context)
        {
            context.result = ParserResultOps.CreateSyncVarAttr(context.Start, context.Stop, TeuchiUdonSyncMode.Linear);
        }

        public override void ExitSmoothVarAttr([NotNull] SmoothVarAttrContext context)
        {
            context.result = ParserResultOps.CreateSyncVarAttr(context.Start, context.Stop, TeuchiUdonSyncMode.Smooth);
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

            var index          = Tables.GetVarBindIndex();
            context.tableIndex = index;
            var qual           = QualifierStack.Peek();
            var varNames       =
                varDecl is SingleVarDeclContext sv ? new string[] { sv.qualifiedVar()?.identifier()?.GetText() ?? "" } :
                varDecl is TupleVarDeclContext  tv ? tv.qualifiedVar().Select(x => x?.identifier()?.GetText() ?? "").ToArray() : Enumerable.Empty<string>();
            var scope   = new TeuchiUdonScope(new TeuchiUdonVarBind(index, qual, varNames), TeuchiUdonScopeMode.VarBind);
            QualifierStack.PushScope(scope);
        }

        public override void ExitVarBind([NotNull] VarBindContext context)
        {
            var varDecl = context.varDecl()?.result;
            var expr    = context.expr   ()?.result;
            if (IsInvalid(varDecl) || IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateVarBind(context.Start, context.Stop);
                return;
            }

            QualifierStack.Pop();

            var mut = context.MUT() != null;

            var index = context.tableIndex;
            var qual  = QualifierStack.Peek();

            var vars = (TeuchiUdonVar[])null;
            if (varDecl.Vars.Length == 1)
            {
                var v = varDecl.Vars[0];
                var t = expr.Inner.Type;
                if (TypeOps.IsAssignableFrom(v.Type, t))
                {
                    if (TypeOps.ContainsUnknown(t))
                    {
                        ParserResultOps.BindType(expr.Inner, v.Type);
                    }

                    vars = new TeuchiUdonVar[]
                    {
                        new TeuchiUdonVar
                        (
                            Tables.GetVarIndex(),
                            v.Qualifier,
                            v.Name,
                            v.Type.LogicalTypeEquals(Primitives.Unknown) ? t : v.Type,
                            mut,
                            false
                        )
                    };
                }
                else
                {
                    LogicalErrorHandler.ReportError(context.Start, $"expression cannot be assigned to variable");
                    context.result = ParserResultOps.CreateVarBind(context.Start, context.Stop);
                    return;
                }
            }
            else if (varDecl.Vars.Length >= 2)
            {
                if (expr.Inner.Type.LogicalTypeNameEquals(Primitives.Tuple))
                {
                    var vs = varDecl.Vars;
                    var ts = expr.Inner.Type.GetArgsAsTuple().ToArray();
                    if (vs.Length == ts.Length && varDecl.Types.Zip(ts, (v, t) => (v, t)).All(x => TypeOps.IsAssignableFrom(x.v, x.t)))
                    {
                        if (TypeOps.ContainsUnknown(expr.Inner.Type))
                        {
                            ParserResultOps.BindType(expr.Inner, SyntaxOps.ToOneType(vs.Select(x => x.Type)));
                        }
                        
                        vars = varDecl.Vars
                            .Zip(ts, (v, t) => (v, t))
                            .Select(x =>
                                new TeuchiUdonVar
                                (
                                    Tables.GetVarIndex(),
                                    x.v.Qualifier,
                                    x.v.Name,
                                    x.v.Type.LogicalTypeEquals(Primitives.Unknown) ? x.t : x.v.Type,
                                    mut,
                                    false
                                )
                            )
                            .ToArray();
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(context.Start, $"expression cannot be assigned to variables");
                        context.result = ParserResultOps.CreateVarBind(context.Start, context.Stop);
                        return;
                    }
                }
                else
                {
                    LogicalErrorHandler.ReportError(context.Start, $"expression cannot be assigned to variables");
                    context.result = ParserResultOps.CreateVarBind(context.Start, context.Stop);
                    return;
                }
            }

            if (mut && TypeOps.IsFunc(expr.Inner.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"function variable cannot be mutable");
                context.result = ParserResultOps.CreateVarBind(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateVarBind(context.Start, context.Stop, index, qual, vars, varDecl, expr);
        }

        public override void ExitUnitVarDecl([NotNull] UnitVarDeclContext context)
        {
            var qualifiedVars = Enumerable.Empty<QualifiedVarResult>();
            context.result    = ExitVarDecl(context.Start, context.Stop, qualifiedVars, context.isActual);
        }

        public override void ExitSingleVarDecl([NotNull] SingleVarDeclContext context)
        {
            var qualifiedVar = context.qualifiedVar()?.result;
            if (IsInvalid(qualifiedVar))
            {
                context.result = ParserResultOps.CreateVarDecl(context.Start, context.Stop);
                return;
            }

            var qualifiedVars = new QualifiedVarResult[] { qualifiedVar };
            context.result    = ExitVarDecl(context.Start, context.Stop, qualifiedVars, context.isActual);
        }

        public override void ExitTupleVarDecl([NotNull] TupleVarDeclContext context)
        {
            var qualifiedVars = context.qualifiedVar().Select(x => x.result);
            if (qualifiedVars.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateVarDecl(context.Start, context.Stop);
                return;
            }

            context.result = ExitVarDecl(context.Start, context.Stop, qualifiedVars, context.isActual);
        }

        private VarDeclResult ExitVarDecl(IToken start, IToken stop, IEnumerable<QualifiedVarResult> qualifiedVars, bool isActual)
        {
            if (!isActual && qualifiedVars.Any(x => TypeOps.ContainsNonDetFunc(x.Qualified.Inner.Type)))
            {
                LogicalErrorHandler.ReportError(start, $"function arguments cannot contain nondeterministic function");
                return ParserResultOps.CreateVarDecl(start, stop);
            }

            var oldQual = (TeuchiUdonQualifier)null;
            if (isActual) oldQual = QualifierStack.Pop();

            var qualifier = QualifierStack.Peek();

            if (isActual) QualifierStack.Push(oldQual);

            return ParserResultOps.CreateVarDecl(start, stop, qualifier, qualifiedVars);
        }

        public override void ExitQualifiedVar([NotNull] QualifiedVarContext context)
        {
            var identifier = context.identifier()?.result;
            var qualified  = context.expr      ()?.result;
            if (IsInvalid(identifier))
            {
                context.result = ParserResultOps.CreateQualifiedVar(context.Start, context.Stop);
                return;
            }

            if (qualified == null)
            {
                qualified = ParserResultOps.CreateExpr(context.Start, context.Stop, ParserResultOps.CreateUnknownType(context.Start, context.Stop), false);
            }
            else
            {
                if (!qualified.Inner.Type.LogicalTypeNameEquals(Primitives.Type))
                {
                    LogicalErrorHandler.ReportError(context.Start, $"qualified type {qualified.Inner.Type} is not a type");
                    context.result = ParserResultOps.CreateQualifiedVar(context.Start, context.Stop);
                    return;
                }
                qualified.ReturnsValue = false;
            }

            context.result = ParserResultOps.CreateQualifiedVar(context.Start, context.Stop, identifier, qualified);
        }

        public override void ExitIdentifier([NotNull] IdentifierContext context)
        {
            if (context.ChildCount == 0)
            {
                context.result = ParserResultOps.CreateIdentifier(context.Start, context.Stop);
                return;
            }

            var name       = context.GetText().Replace("@", "");
            context.result = ParserResultOps.CreateIdentifier(context.Start, context.Stop, name);
        }

        public override void ExitReturnUnitStatement([NotNull] ReturnUnitStatementContext context)
        {
            var value      = ParserResultOps.CreateExpr(context.Start, context.Stop, ParserResultOps.CreateUnit(context.Start, context.Stop));
            context.result = ExitReturnStatement(context.Start, context.Stop, value);
        }

        public override void ExitReturnValueStatement([NotNull] ReturnValueStatementContext context)
        {
            var value = context.expr()?.result;
            if (IsInvalid(value))
            {
                context.result = ParserResultOps.CreateJump(context.Start, context.Stop);
                return;
            }

            context.result = ExitReturnStatement(context.Start, context.Stop, value);
        }

        private JumpResult ExitReturnStatement(IToken start, IToken stop, ExprResult value)
        {
            var qb = QualifierStack.Peek().GetFuncBlock();
            if (qb == null)
            {
                LogicalErrorHandler.ReportError(start, "no func exists for return");
                return ParserResultOps.CreateJump(start, stop);
            }

            return StoreJumpResult
            (
                ParserResultOps.CreateJump
                (
                    start,
                    stop,
                    value,
                    () => Tables.Blocks.ContainsKey(qb) ? Tables.Blocks[qb]        : Invalids.InvalidBlock,
                    () => Tables.Blocks.ContainsKey(qb) ? Tables.Blocks[qb].Return : Invalids.InvalidBlock
                )
            );
        }

        public override void ExitContinueUnitStatement([NotNull] ContinueUnitStatementContext context)
        {
            var value      = ParserResultOps.CreateExpr(context.Start, context.Stop, ParserResultOps.CreateUnit(context.Start, context.Stop));
            context.result = ExitContinueStatement(context.Start, context.Stop, value);
        }

        public override void ExitContinueValueStatement([NotNull] ContinueValueStatementContext context)
        {
            var value = context.expr()?.result;
            if (IsInvalid(value))
            {
                context.result = ParserResultOps.CreateJump(context.Start, context.Stop);
                return;
            }

            context.result = ExitContinueStatement(context.Start, context.Stop, value);
        }

        private JumpResult ExitContinueStatement(IToken start, IToken stop, ExprResult value)
        {
            var qb = QualifierStack.Peek().GetLoopBlock();
            if (qb == null)
            {
                LogicalErrorHandler.ReportError(start, "no loop exists for continue");
                return ParserResultOps.CreateJump(start, stop);
            }

            return StoreJumpResult
            (
                ParserResultOps.CreateJump(start, stop, value, () => Tables.Blocks[qb], () => Tables.Blocks[qb].Continue)
            );
        }

        public override void ExitBreakUnitStatement([NotNull] BreakUnitStatementContext context)
        {
            var value      = ParserResultOps.CreateExpr(context.Start, context.Stop, ParserResultOps.CreateUnit(context.Start, context.Stop));
            context.result = ExitBreakStatement(context.Start, context.Stop, value);
        }

        public override void ExitBreakValueStatement([NotNull] BreakValueStatementContext context)
        {
            var value = context.expr()?.result;
            if (IsInvalid(value))
            {
                context.result = ParserResultOps.CreateJump(context.Start, context.Stop);
                return;
            }

            context.result = ExitBreakStatement(context.Start, context.Stop, value);
        }

        private JumpResult ExitBreakStatement(IToken start, IToken stop, ExprResult value)
        {
            var qb = QualifierStack.Peek().GetLoopBlock();
            if (qb == null)
            {
                LogicalErrorHandler.ReportError(start, "no loop exists for break");
                return ParserResultOps.CreateJump(start, stop);
            }

            return StoreJumpResult
            (
                ParserResultOps.CreateJump(start, stop, value, () => Tables.Blocks[qb], () => Tables.Blocks[qb].Break)
            );
        }

        public override void ExitLetBindStatement([NotNull] LetBindStatementContext context)
        {
            var varBind = context.varBind()?.result;
            if (IsInvalid(varBind))
            {
                context.result = ParserResultOps.CreateLetBind(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateLetBind(context.Start, context.Stop, varBind);
        }

        public override void ExitExprStatement([NotNull] ExprStatementContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

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
            var index          = Tables.GetBlockIndex();
            context.tableIndex = index;
            var qual           = QualifierStack.Peek();
            var scopeMode      = GetScopeMode(context);
            var scope          = new TeuchiUdonScope(new TeuchiUdonBlock(index, qual), scopeMode);
            QualifierStack.PushScope(scope);
        }

        public override void ExitUnitBlockExpr([NotNull] UnitBlockExprContext context)
        {
            var statements = context.statement().Select(x => x.result);
            if (statements.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            QualifierStack.Pop();

            var type       = Primitives.Unit;
            var index      = context.tableIndex;
            var qual       = QualifierStack.Peek();
            var expr       = ParserResultOps.CreateExpr(context.Start, context.Stop, ParserResultOps.CreateUnit(context.Start, context.Stop));
            var block      = ParserResultOps.CreateBlock(context.Start, context.Stop, type, index, qual, statements, expr);
            context.result = ParserResultOps.CreateExpr(block.Start, block.Stop, block);
        }

        public override void EnterValueBlockExpr([NotNull] ValueBlockExprContext context)
        {
            var index          = Tables.GetBlockIndex();
            context.tableIndex = index;
            var qual           = QualifierStack.Peek();
            var scopeMode      = GetScopeMode(context);
            var scope          = new TeuchiUdonScope(new TeuchiUdonBlock(index, qual), scopeMode);
            QualifierStack.PushScope(scope);
        }

        public override void ExitValueBlockExpr([NotNull] ValueBlockExprContext context)
        {
            var expr       = context.expr()?.result;
            var statements = context.statement().Select(x => x.result);
            if (IsInvalid(expr) || statements.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            QualifierStack.Pop();

            var type       = expr.Inner.Type;
            var index      = context.tableIndex;
            var qual       = QualifierStack.Peek();
            var block      = ParserResultOps.CreateBlock(context.Start, context.Stop, type, index, qual, statements, expr);
            context.result = ParserResultOps.CreateExpr(block.Start, block.Stop, block);
        }

        public override void ExitParenExpr([NotNull] ParenExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var type       = expr.Inner.Type;
            var paren      = ParserResultOps.CreateParen(context.Start, context.Stop, type, expr);
            context.result = ParserResultOps.CreateExpr(paren.Start, paren.Stop, paren);
        }

        public override void ExitTupleExpr([NotNull] TupleExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length < 2 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var type       = Primitives.Tuple.ApplyArgsAsTuple(exprs.Select(x => x.Inner.Type));
            var tuple      = ParserResultOps.CreateTuple(context.Start, context.Stop, type, exprs);
            context.result = ParserResultOps.CreateExpr(tuple.Start, tuple.Stop, tuple);
        }

        public override void ExitEmptyArrayCtorExpr([NotNull] EmptyArrayCtorExprContext context)
        {
            var qual       = QualifierStack.Peek();
            var type       = TypeOps.ToArrayType(Primitives.Unknown);
            var arrayCtor  = ParserResultOps.CreateArrayCtor(context.Start, context.Stop, type, qual, Enumerable.Empty<IterExprResult>());
            context.result = ParserResultOps.CreateExpr(arrayCtor.Start, arrayCtor.Stop, arrayCtor);
        }

        public override void ExitArrayCtorExpr([NotNull] ArrayCtorExprContext context)
        {
            var iterExpr = context.iterExpr()?.result;
            if (IsInvalid(iterExpr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var type       = TypeOps.ToArrayType(iterExpr.Type);
            var arrayCtor  = ParserResultOps.CreateArrayCtor(context.Start, context.Stop, type, qual, new IterExprResult[] { iterExpr });
            context.result = ParserResultOps.CreateExpr(arrayCtor.Start, arrayCtor.Stop, arrayCtor);
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
            var index          = Tables.GetLiteralIndex();
            context.tableIndex = index;
        }

        public override void ExitLiteralExpr([NotNull] LiteralExprContext context)
        {
            var literal = context.literal()?.result;
            if (IsInvalid(literal))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateExpr(literal.Start, literal.Stop, literal);
        }

        public override void ExitThisLiteralExpr([NotNull] ThisLiteralExprContext context)
        {
            var thisLiteral = context.thisLiteral()?.result;
            if (IsInvalid(thisLiteral))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateExpr(thisLiteral.Start, thisLiteral.Stop, thisLiteral);
        }

        public override void EnterInterpolatedRegularStringExpr([NotNull] InterpolatedRegularStringExprContext context)
        {
            var index          = Tables.GetLiteralIndex();
            context.tableIndex = index;
        }

        public override void ExitInterpolatedRegularStringExpr([NotNull] InterpolatedRegularStringExprContext context)
        {
            var str = context.interpolatedRegularString()?.result;
            if (IsInvalid(str))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateExpr(str.Start, str.Stop, str);
        }

        public override void ExitEvalVarExpr([NotNull] EvalVarExprContext context)
        {
            var identifier = context.identifier()?.result;
            if (IsInvalid(identifier))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var evalVar = (TypedResult)null;
            if (context.Parent is AccessExprContext)
            {
                evalVar = ParserResultOps.CreateEvalVarCandidate(context.Start, context.Stop, identifier);
            }
            else
            {
                foreach (var qual in QualifierStack.Qualifiers)
                {
                    var qv = new TeuchiUdonVar(qual, identifier.Name);
                    if (Tables.Vars.ContainsKey(qv))
                    {
                        var v    = Tables.Vars[qv];
                        var type = v.Type;
                        evalVar  = ParserResultOps.CreateEvalVar(context.Start, context.Stop, type, v);
                        break;
                    }

                    var qt = new TeuchiUdonType(qual, identifier.Name);
                    if (StaticTables.Types.ContainsKey(qt))
                    {
                        var type  = StaticTables.Types[qt];
                        var outer = Primitives.Type.ApplyArgAsType(type);
                        evalVar   = ParserResultOps.CreateEvalType(context.Start, context.Stop, outer, type);
                        break;
                    }

                    if
                    (
                        StaticTables.GenericRootTypes.ContainsKey(qt) &&
                        (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                    )
                    {
                        var type  = StaticTables.GenericRootTypes[qt];
                        var outer = Primitives.Type.ApplyArgAsType(type);
                        evalVar   = ParserResultOps.CreateEvalType(context.Start, context.Stop, outer, type);
                        break;
                    }
                }

                if (evalVar == null)
                {
                    LogicalErrorHandler.ReportError(context.Start, $"'{identifier.Name}' is not defined");
                    context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                    return;
                }
            }

            context.result = ParserResultOps.CreateExpr(evalVar.Start, evalVar.Stop, evalVar);
        }

        public override void ExitEvalTypeOfExpr([NotNull] EvalTypeOfExprContext context)
        {
            var typeOf     = ParserResultOps.CreateEvalTypeOf(context.Start, context.Stop);
            context.result = ParserResultOps.CreateExpr(typeOf.Start, typeOf.Stop, typeOf);
        }

        public override void ExitAccessExpr([NotNull] AccessExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qualifier = QualifierStack.Peek();

            if (expr1.Inner is EvalVarCandidateResult varCandidate1)
            {
                var eval = (TypedResult)null;
                foreach (var qual in QualifierStack.Qualifiers)
                {
                    var qv = new TeuchiUdonVar(qual, varCandidate1.Identifier.Name);
                    if (Tables.Vars.ContainsKey(qv))
                    {
                        var v    = Tables.Vars[qv];
                        var type = v.Type;
                        eval     = ParserResultOps.CreateEvalVar(varCandidate1.Start, varCandidate1.Stop, type, v);
                        break;
                    }

                    var qt = new TeuchiUdonType(qual, varCandidate1.Identifier.Name);
                    if (StaticTables.Types.ContainsKey(qt))
                    {
                        var type  = StaticTables.Types[qt];
                        var outer = Primitives.Type.ApplyArgAsType(type);
                        eval      = ParserResultOps.CreateEvalType(varCandidate1.Start, varCandidate1.Stop, outer, type);
                        break;
                    }

                    if
                    (
                        StaticTables.GenericRootTypes.ContainsKey(qt) &&
                        (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                    )
                    {
                        var type  = StaticTables.GenericRootTypes[qt];
                        var outer = Primitives.Type.ApplyArgAsType(type);
                        eval      = ParserResultOps.CreateEvalType(varCandidate1.Start, varCandidate1.Stop, outer, type);
                        break;
                    }
                }

                var scopes  = new TeuchiUdonScope[] { new TeuchiUdonScope(new TextLabel(varCandidate1.Identifier.Name)) };
                var newQual = new TeuchiUdonQualifier(scopes);
                if (StaticTables.Qualifiers.ContainsKey(newQual))
                {
                    var q    = StaticTables.Qualifiers[newQual];
                    var type = Primitives.Qual.ApplyArgAsQual(q);
                    eval     = ParserResultOps.CreateEvalQualifier(varCandidate1.Start, varCandidate1.Stop, type, q);
                }

                if (eval == null)
                {
                    LogicalErrorHandler.ReportError(varCandidate1.Start, $"'{varCandidate1.Identifier.Name}' is not defined");
                    context.result = ParserResultOps.CreateExpr(varCandidate1.Start, varCandidate1.Stop);
                    return;
                }
                
                expr1 = ParserResultOps.CreateExpr(eval.Start, eval.Stop, eval);
            }
            var type1 = expr1.Inner.Type;

            if (expr2.Inner is EvalVarCandidateResult varCandidate2)
            {
                var eval = (TypedResult)null;
                do
                {
                    if (type1.LogicalTypeNameEquals(Primitives.Qual))
                    {
                        var qual = type1.GetArgAsQual();
                        var qv   = new TeuchiUdonVar(qual, varCandidate2.Identifier.Name);
                        if (Tables.Vars.ContainsKey(qv))
                        {
                            var v    = Tables.Vars[qv];
                            var type = v.Type;
                            eval     = ParserResultOps.CreateEvalVar(varCandidate2.Start, varCandidate2.Stop, type, v);
                            break;
                        }

                        var qt = new TeuchiUdonType(qual, varCandidate2.Identifier.Name);
                        if (StaticTables.Types.ContainsKey(qt))
                        {
                            var type  = StaticTables.Types[qt];
                            var outer = Primitives.Type.ApplyArgAsType(type);
                            eval      = ParserResultOps.CreateEvalType(varCandidate2.Start, varCandidate2.Stop, outer, type);
                            break;
                        }

                        if
                        (
                            StaticTables.GenericRootTypes.ContainsKey(qt) &&
                            (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                        )
                        {
                            var type  = StaticTables.GenericRootTypes[qt];
                            var outer = Primitives.Type.ApplyArgAsType(type);
                            eval      = ParserResultOps.CreateEvalType(varCandidate2.Start, varCandidate2.Stop, outer, type);
                            break;
                        }

                        var scope    = new TeuchiUdonScope(new TextLabel(varCandidate2.Identifier.Name));
                        var appended = qual.Append(scope);
                        if (StaticTables.Qualifiers.ContainsKey(appended))
                        {
                            var q    = StaticTables.Qualifiers[appended];
                            var type = Primitives.Qual.ApplyArgAsQual(q);
                            eval     = ParserResultOps.CreateEvalQualifier(varCandidate2.Start, varCandidate2.Stop, type, q);
                            break;
                        }
                    }
                    else if (type1.LogicalTypeNameEquals(Primitives.Type))
                    {
                        var t  = type1.GetArgAsType();
                        var sc = new TeuchiUdonScope(new TextLabel(t.LogicalName));
                        var q  = t.Qualifier.Append(sc);
                        var qt = new TeuchiUdonType(q, varCandidate2.Identifier.Name);
                        if (StaticTables.Types.ContainsKey(qt))
                        {
                            var type  = StaticTables.Types[qt];
                            var outer = Primitives.Type.ApplyArgAsType(type);
                            eval      = ParserResultOps.CreateEvalType(varCandidate2.Start, varCandidate2.Stop, outer, type);
                            break;
                        }

                        if
                        (
                            StaticTables.GenericRootTypes.ContainsKey(qt) &&
                            (context.Parent is EvalSingleKeyExprContext || context.Parent is EvalTupleKeyExprContext)
                        )
                        {
                            var type  = StaticTables.GenericRootTypes[qt];
                            var outer = Primitives.Type.ApplyArgAsType(type);
                            eval      = ParserResultOps.CreateEvalType(varCandidate2.Start, varCandidate2.Stop, outer, type);
                            break;
                        }

                        if
                        (
                            StaticTables.TypeToMethods.ContainsKey(type1) &&
                            StaticTables.TypeToMethods[type1].ContainsKey(varCandidate2.Identifier.Name)
                        )
                        {
                            var ms   = StaticTables.TypeToMethods[type1][varCandidate2.Identifier.Name];
                            var type = Primitives.Method.ApplyArgsAsMethod(ms);
                            eval     = ParserResultOps.CreateMethod(varCandidate2.Start, varCandidate2.Stop, type, varCandidate2.Identifier);
                            break;
                        }

                        var qg = new TeuchiUdonMethod(type1, TeuchiUdonTableOps.GetGetterName(varCandidate2.Identifier.Name));
                        var g  = TableOps.GetMostCompatibleMethodsWithoutInTypes(qg, 0).ToArray();
                        var qs = new TeuchiUdonMethod(type1, TeuchiUdonTableOps.GetSetterName(varCandidate2.Identifier.Name));
                        var s  = TableOps.GetMostCompatibleMethodsWithoutInTypes(qs, 1).ToArray();
                        if (g.Length == 1 && s.Length == 1 && g[0].OutTypes.Length == 1 && IsValidSetter(expr1))
                        {
                            eval = ParserResultOps.CreateEvalGetterSetter(varCandidate2.Start, varCandidate2.Stop, g[0].OutTypes[0], qualifier, g[0], s[0]);
                            break;
                        }
                        else if (g.Length == 1 && g[0].OutTypes.Length == 1)
                        {
                            eval = ParserResultOps.CreateEvalGetter(varCandidate2.Start, varCandidate2.Stop, g[0].OutTypes[0], qualifier, g[0]);
                            break;
                        }
                        else if (s.Length == 1 && IsValidSetter(expr1))
                        {
                            eval = ParserResultOps.CreateEvalSetter(varCandidate2.Start, varCandidate2.Stop, s[0].InTypes[1], qualifier, s[0]);
                            break;
                        }
                        else if (g.Length >= 2 || s.Length >= 2)
                        {
                            LogicalErrorHandler.ReportError(varCandidate2.Start, $"method '{varCandidate2.Identifier.Name}' has multiple overloads");
                            context.result = ParserResultOps.CreateExpr(varCandidate2.Start, varCandidate2.Stop);
                            return;
                        }

                        if (t.RealType.IsEnum)
                        {
                            var name  = varCandidate2.Identifier.Name;
                            var index = Array.IndexOf(t.RealType.GetEnumNames(), name);
                            if (index >= 0)
                            {
                                var value = t.RealType.GetEnumValues().GetValue(index);
                                eval = ParserResultOps.CreateLiteral(varCandidate2.Start, varCandidate2.Stop, t, Tables.GetLiteralIndex(), name, value);
                                break;
                            }
                            else
                            {
                                LogicalErrorHandler.ReportError(varCandidate2.Start, $"'{name}' is not enum value");
                                context.result = ParserResultOps.CreateExpr(varCandidate2.Start, varCandidate2.Stop);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if
                        (
                            StaticTables.TypeToMethods.ContainsKey(type1) &&
                            StaticTables.TypeToMethods[type1].ContainsKey(varCandidate2.Identifier.Name)
                        )
                        {
                            var ms   = StaticTables.TypeToMethods[type1][varCandidate2.Identifier.Name];
                            var type = Primitives.Method.ApplyArgsAsMethod(ms);
                            eval     = ParserResultOps.CreateMethod(varCandidate2.Start, varCandidate2.Stop, type, varCandidate2.Identifier);
                            break;
                        }

                        var qg = new TeuchiUdonMethod(type1, TeuchiUdonTableOps.GetGetterName(varCandidate2.Identifier.Name));
                        var g  = TableOps.GetMostCompatibleMethodsWithoutInTypes(qg, 1).ToArray();
                        var qs = new TeuchiUdonMethod(type1, TeuchiUdonTableOps.GetSetterName(varCandidate2.Identifier.Name));
                        var s  = TableOps.GetMostCompatibleMethodsWithoutInTypes(qs, 2).ToArray();
                        if (g.Length == 1 && s.Length == 1 && g[0].OutTypes.Length == 1 && IsValidSetter(expr1))
                        {
                            eval = ParserResultOps.CreateEvalGetterSetter(varCandidate2.Start, varCandidate2.Stop, g[0].OutTypes[0], qualifier, g[0], s[0]);
                            break;
                        }
                        else if (g.Length == 1 && g[0].OutTypes.Length == 1)
                        {
                            eval = ParserResultOps.CreateEvalGetter(varCandidate2.Start, varCandidate2.Stop, g[0].OutTypes[0], qualifier, g[0]);
                            break;
                        }
                        else if (s.Length == 1 && IsValidSetter(expr1))
                        {
                            eval = ParserResultOps.CreateEvalSetter(varCandidate2.Start, varCandidate2.Stop, Primitives.Setter.ApplyArgAsSetter(s[0].InTypes[1]), qualifier, s[0]);
                            break;
                        }
                        else if (g.Length >= 2 || s.Length >= 2)
                        {
                            LogicalErrorHandler.ReportError(varCandidate2.Start, $"method '{varCandidate2.Identifier.Name}' has multiple overloads");
                            context.result = ParserResultOps.CreateExpr(varCandidate2.Start, varCandidate2.Stop);
                            return;
                        }
                    }

                    LogicalErrorHandler.ReportError(varCandidate2.Start, $"'{varCandidate2.Identifier.Name}' is not defined");
                    context.result = ParserResultOps.CreateExpr(varCandidate2.Start, varCandidate2.Stop);
                    return;
                } while (false);

                expr2 = ParserResultOps.CreateExpr(eval.Start, eval.Stop, eval);
            }
            else
            {
                LogicalErrorHandler.ReportError(expr1.Start, $"invalid '{op}' operator");
                context.result = ParserResultOps.CreateExpr(expr1.Start, expr1.Stop);
                return;
            }

            var infix      = ParserResultOps.CreateInfix(expr1.Start, expr1.Stop, expr2.Inner.Type, qualifier, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        private bool IsValidSetter(ExprResult expr)
        {
            var type = expr.Inner.Type;
            if (type.LogicalTypeNameEquals(Primitives.Type)) return true;
            if (type.RealType == null) return false;
            if (!type.RealType.IsValueType) return true;
            return expr.Inner is EvalVarResult;
        }

        public override void ExitCastExpr([NotNull] CastExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var type = expr.Inner.Type;
            if (!type.LogicalTypeNameEquals(Primitives.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"expression is not a type");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var cast = ParserResultOps.CreateEvalCast(context.Start, context.Stop, Primitives.Cast.ApplyArgAsCast(type.GetArgAsType()), expr);
            context.result = ParserResultOps.CreateExpr(cast.Start, cast.Stop, cast);
        }

        private bool IsValidConvertType(TeuchiUdonType type)
        {
            return new TeuchiUdonType[]
            {
                Primitives.Bool,
                Primitives.Byte,
                Primitives.Char,
                Primitives.DateTime,
                Primitives.Decimal,
                Primitives.Double,
                Primitives.Short,
                Primitives.Int,
                Primitives.Long,
                Primitives.SByte,
                Primitives.Float,
                Primitives.String,
                Primitives.UShort,
                Primitives.UInt,
                Primitives.ULong
            }
            .Any(x => type.LogicalTypeEquals(x));
        }

        public override void ExitEvalUnitFuncExpr([NotNull] EvalUnitFuncExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var args       = Enumerable.Empty<ArgExprResult>();
            context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, expr, args);
        }

        public override void ExitEvalSingleFuncExpr([NotNull] EvalSingleFuncExprContext context)
        {
            var expr = context.expr   ()?.result;
            var arg  = context.argExpr()?.result;
            if (IsInvalid(expr) || IsInvalid(arg))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var args = new ArgExprResult[] { arg };
            context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, expr, args);
        }

        public override void ExitEvalTupleFuncExpr([NotNull] EvalTupleFuncExprContext context)
        {
            var expr = context.expr   ()?.result;
            var args = context.argExpr().Select(x => x?.result).ToArray();
            if (IsInvalid(expr) || args.Length < 2 || args.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, expr, args);
        }

        private ExprResult ExitEvalFuncExprWithArgs(IToken start, IToken stop, ExprResult expr, IEnumerable<ArgExprResult> argExprs)
        {
            var type     = expr.Inner.Type;
            var args     = argExprs.Select(x => x.Expr).ToArray();
            var argRefs  = argExprs.Select(x => x.Ref ).ToArray();
            var qual     = QualifierStack.Peek();
            var evalFunc = (TypedResult)null;

            if (TypeOps.IsFunc(type))
            {
                if (argRefs.Any(x => x))
                {
                    LogicalErrorHandler.ReportError(start, $"arguments of func cannot be ref");
                    return ParserResultOps.CreateExpr(start, stop);
                }
                else
                {
                    var argTypes = argExprs.Select(x => x.Expr.Inner.Type).ToArray();
                    var iType    = SyntaxOps.ToOneType(argTypes);
                    var oType    = Primitives.Unknown;
                    if (TypeOps.IsAssignableFrom(type, Primitives.DetFunc.ApplyArgsAsFunc(iType, oType)))
                    {
                        var outType = type.GetArgAsFuncOutType();
                        var index   = Tables.GetEvalFuncIndex();
                        evalFunc    = ParserResultOps.CreateEvalFunc(start, stop, outType, index, qual, expr, args);
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"arguments of func is not compatible");
                        return ParserResultOps.CreateExpr(start, stop);
                    }
                }
            }
            else if (type.LogicalTypeNameEquals(Primitives.Method) && expr.Inner.Instance != null)
            {
                var instanceType = expr.Inner.Instance.Inner.Type.RealType == null ? Enumerable.Empty<TeuchiUdonType>() : new TeuchiUdonType[] { expr.Inner.Instance.Inner.Type };
                var inTypes      = instanceType.Concat(args.Select(x => x.Inner.Type));
                var inRefs       = instanceType.Select(_ => false).Concat(argRefs);
                var ms           = TypeOps.GetMostCompatibleMethods(type, inTypes).ToArray();
                if (ms.Length == 0)
                {
                    LogicalErrorHandler.ReportError(start, $"method is not defined");
                    return ParserResultOps.CreateExpr(start, stop);
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
                        var outType = SyntaxOps.ToOneType(method.OutTypes);
                        if (expr.Inner is InfixResult infix && infix.Op == "?.")
                        {
                            evalFunc = ParserResultOps.CreateEvalCoalescingMethod(start, stop, outType, qual, method, infix.Expr1, infix.Expr2, args);
                        }
                        else
                        {
                            evalFunc = ParserResultOps.CreateEvalMethod(start, stop, outType, qual, method, expr, args);
                        }
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"ref mark of method is not compatible");
                        return ParserResultOps.CreateExpr(start, stop);
                    }
                }
                else
                {
                    LogicalErrorHandler.ReportError(start, $"method has multiple overloads");
                    return ParserResultOps.CreateExpr(start, stop);
                }
            }
            else if (type.LogicalTypeNameEquals(Primitives.Type))
            {
                var inTypes = args.Select(x => x.Inner.Type);
                var qm      = new TeuchiUdonMethod(type, "ctor", inTypes);
                var ms      = TableOps.GetMostCompatibleMethods(qm).ToArray();
                if (ms.Length == 0)
                {
                    LogicalErrorHandler.ReportError(start, $"ctor is not defined");
                    return ParserResultOps.CreateExpr(start, stop);
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
                        var outType = SyntaxOps.ToOneType(method.OutTypes);
                        evalFunc = ParserResultOps.CreateEvalMethod(start, stop, outType, qual, method, expr, args);
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"ref mark of ctor is not compatible");
                        return ParserResultOps.CreateExpr(start, stop);
                    }
                }
                else
                {
                    LogicalErrorHandler.ReportError(start, $"ctor has multiple overloads");
                    return ParserResultOps.CreateExpr(start, stop);
                }
            }
            else if (type.LogicalTypeNameEquals(Primitives.Cast))
            {
                if (args.Length != 1 || argRefs.Any(x => x))
                {
                    LogicalErrorHandler.ReportError(start, $"cast must be specified with one argument");
                    return ParserResultOps.CreateExpr(start, stop);
                }
                else
                {
                    var type1 = type.GetArgAsCast();
                    var type2 = args[0].Inner.Type;
                    if (TypeOps.IsAssignableFrom(type1, type2) || TypeOps.IsAssignableFrom(type2, type1))
                    {
                        if (TypeOps.ContainsUnknown(type2))
                        {
                            ParserResultOps.BindType(args[0].Inner, type1);
                        }
                        evalFunc = ParserResultOps.CreateTypeCast(start, stop, type1, expr, args[0]);
                    }
                    else if (IsValidConvertType(type1) && IsValidConvertType(type2))
                    {
                        evalFunc = ParserResultOps.CreateConvertCast(start, stop, type1, qual, expr, args[0]);
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"specified type cannot be cast");
                        return ParserResultOps.CreateExpr(start, stop);
                    }
                }
            }
            else if (type.LogicalTypeEquals(Primitives.TypeOf))
            {
                if (args.Length != 1 || argRefs.Any(x => x))
                {
                    LogicalErrorHandler.ReportError(start, $"typeof must be specified with one argument");
                    return ParserResultOps.CreateExpr(start, stop);
                }
                else if (args[0].Inner.Type.LogicalTypeNameEquals(Primitives.Type) && args[0].Inner.Type.GetArgAsType().RealType != null)
                {
                    evalFunc = ParserResultOps.CreateTypeOf(start, stop, args[0].Inner.Type.GetArgAsType());
                }
                else
                {
                    LogicalErrorHandler.ReportError(start, $"typeof argument must be a type");
                    return ParserResultOps.CreateExpr(start, stop);
                }
            }
            else
            {
                LogicalErrorHandler.ReportError(start, $"expression is not a function or method");
                return ParserResultOps.CreateExpr(start, stop);
            }

            return ParserResultOps.CreateExpr(evalFunc.Start, evalFunc.Stop, evalFunc);
        }

        public override void ExitEvalSpreadFuncExpr([NotNull] EvalSpreadFuncExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            context.result = ExitEvalFuncExprWithSpread(context.Start, context.Stop, exprs[0], exprs[1]);
        }

        private ExprResult ExitEvalFuncExprWithSpread(IToken start, IToken stop, ExprResult expr, ExprResult arg)
        {
            var type     = expr.Inner.Type;
            var qual     = QualifierStack.Peek();
            var evalFunc = (TypedResult)null;

            if (!arg.Inner.Type.LogicalTypeNameEquals(Primitives.Tuple))
            {
                LogicalErrorHandler.ReportError(start, $"spread expression is not a tuple type");
                return ParserResultOps.CreateExpr(start, stop);
            }
            else if (TypeOps.IsFunc(type))
            {
                var argTypes = arg.Inner.Type.GetArgsAsTuple().ToArray();
                var iType    = SyntaxOps.ToOneType(argTypes);
                var oType    = Primitives.Unknown;
                if (TypeOps.IsAssignableFrom(type, Primitives.DetFunc.ApplyArgsAsFunc(iType, oType)))
                {
                    var outType = type.GetArgAsFuncOutType();
                    var index   = Tables.GetEvalFuncIndex();
                    evalFunc    = ParserResultOps.CreateEvalSpreadFunc(start, stop, outType, index, qual, expr, arg);
                }
                else
                {
                    LogicalErrorHandler.ReportError(start, $"arguments of func is not compatible");
                    return ParserResultOps.CreateExpr(start, stop);
                }
            }
            else if (type.LogicalTypeNameEquals(Primitives.Method) && expr.Inner.Instance != null)
            {
                var instanceType = expr.Inner.Instance.Inner.Type.RealType == null ? Enumerable.Empty<TeuchiUdonType>() : new TeuchiUdonType[] { expr.Inner.Instance.Inner.Type };
                var inTypes      = instanceType.Concat(arg.Inner.Type.GetArgsAsTuple());
                var ms           = TypeOps.GetMostCompatibleMethods(type, inTypes).ToArray();
                if (ms.Length == 0)
                {
                    LogicalErrorHandler.ReportError(start, $"method is not defined");
                    return ParserResultOps.CreateExpr(start, stop);
                }
                else if (ms.Length == 1)
                {
                    var method = ms[0];
                    if (method.InParamInOuts.All(x => x != TeuchiUdonMethodParamInOut.InOut))
                    {
                        var outType = SyntaxOps.ToOneType(method.OutTypes);
                        if (expr.Inner is InfixResult infix && infix.Op == "?.")
                        {
                            evalFunc = ParserResultOps.CreateEvalCoalescingSpreadMethod(start, stop, outType, qual, method, infix.Expr1, infix.Expr2, arg);
                        }
                        else
                        {
                            evalFunc = ParserResultOps.CreateEvalSpreadMethod(start, stop, outType, qual, method, expr, arg);
                        }
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"ref mark of method is not compatible");
                        return ParserResultOps.CreateExpr(start, stop);
                    }
                }
                else
                {
                    LogicalErrorHandler.ReportError(start, $"method has multiple overloads");
                    return ParserResultOps.CreateExpr(start, stop);
                }
            }
            else if (type.LogicalTypeNameEquals(Primitives.Type))
            {
                var inTypes = arg.Inner.Type.GetArgsAsTuple();
                var qm      = new TeuchiUdonMethod(type, "ctor", inTypes);
                var ms      = TableOps.GetMostCompatibleMethods(qm).ToArray();
                if (ms.Length == 0)
                {
                    LogicalErrorHandler.ReportError(start, $"ctor is not defined");
                    return ParserResultOps.CreateExpr(start, stop);
                }
                else if (ms.Length == 1)
                {
                    var method = ms[0];
                    if (method.InParamInOuts.All(x => x != TeuchiUdonMethodParamInOut.InOut))
                    {
                        var outType = SyntaxOps.ToOneType(method.OutTypes);
                        evalFunc = ParserResultOps.CreateEvalSpreadMethod(start, stop, outType, qual, method, expr, arg);
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"ref mark of ctor is not compatible");
                        return ParserResultOps.CreateExpr(start, stop);
                    }
                }
                else
                {
                    LogicalErrorHandler.ReportError(start, $"ctor has multiple overloads");
                    return ParserResultOps.CreateExpr(start, stop);
                }
            }
            else
            {
                LogicalErrorHandler.ReportError(start, $"expression is not a function or method");
                return ParserResultOps.CreateExpr(start, stop);
            }

            return ParserResultOps.CreateExpr(evalFunc.Start, evalFunc.Stop, evalFunc);
        }

        public override void ExitEvalSingleKeyExpr([NotNull] EvalSingleKeyExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var args       = exprs.Skip(1);
            var evalKey    = ExitEvalKeyExpr(context.Start, context.Stop, exprs[0], args);
            context.result = ParserResultOps.CreateExpr(evalKey.Start, evalKey.Stop, evalKey);
        }

        public override void ExitEvalTupleKeyExpr([NotNull] EvalTupleKeyExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length < 2 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var args       = exprs.Skip(1);
            var evalKey    = ExitEvalKeyExpr(context.Start, context.Stop, exprs[0], args);
            context.result = ParserResultOps.CreateExpr(evalKey.Start, evalKey.Stop, evalKey);
        }

        private TypedResult ExitEvalKeyExpr(IToken start, IToken stop, ExprResult expr, IEnumerable<ExprResult> args)
        {
            var argArray = args.ToArray();
            if (expr.Inner.Type.LogicalTypeNameEquals(Primitives.Type) && args.All(x => x.Inner.Type.LogicalTypeNameEquals(Primitives.Type)))
            {
                var exprType = expr.Inner.Type.GetArgAsType();
                if (!StaticTables.GenericRootTypes.ContainsKey(exprType))
                {
                    LogicalErrorHandler.ReportError(start, $"specified key is invalid");
                    return ParserResultOps.CreateInvalid(start, stop);
                }

                var argTypes = args.Select(x => x.Inner.Type.GetArgAsType()).ToArray();
                if (exprType.LogicalTypeEquals(Primitives.Array))
                {
                    if (argTypes.Length == 1)
                    {
                        var type = TypeOps.ToArrayType(argTypes[0]);
                        return ParserResultOps.CreateEvalType(start, stop, Primitives.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"specified key is invalid");
                        return ParserResultOps.CreateInvalid(start, stop);
                    }
                }
                else if (exprType.LogicalTypeEquals(Primitives.List))
                {
                    if (argTypes.Length == 1)
                    {
                        var type = Primitives.List
                            .ApplyArgAsList(argTypes[0], TypeOps.ToArrayType(argTypes[0]))
                            .ApplyRealType(TypeOps.GetRealName(Primitives.AnyArray), Primitives.AnyArray.RealType);
                        return ParserResultOps.CreateEvalType(start, stop, Primitives.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"specified key is invalid");
                        return ParserResultOps.CreateInvalid(start, stop);
                    }
                }
                else if (exprType.LogicalTypeEquals(Primitives.Func))
                {
                    if (argTypes.Length == 2)
                    {
                        var type = Primitives.Func
                            .ApplyArgsAsFunc(argTypes[0], argTypes[1])
                            .ApplyRealType(TypeOps.GetRealName(Primitives.UInt), Primitives.UInt.RealType);
                        return ParserResultOps.CreateEvalType(start, stop, Primitives.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"specified key is invalid");
                        return ParserResultOps.CreateInvalid(start, stop);
                    }
                }
                else if (exprType.LogicalTypeEquals(Primitives.DetFunc))
                {
                    if (argTypes.Length == 2)
                    {
                        var type = Primitives.DetFunc
                            .ApplyArgsAsFunc(argTypes[0], argTypes[1])
                            .ApplyRealType(TypeOps.GetRealName(Primitives.UInt), Primitives.UInt.RealType);
                        return ParserResultOps.CreateEvalType(start, stop, Primitives.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"specified key is invalid");
                        return ParserResultOps.CreateInvalid(start, stop);
                    }
                }
                else
                {
                    var qt = new TeuchiUdonType(TeuchiUdonTableOps.GetGenericTypeName(exprType, argTypes), argTypes);
                    if (StaticTables.LogicalTypes.ContainsKey(qt))
                    {
                        var type = StaticTables.LogicalTypes[qt];
                        return ParserResultOps.CreateEvalType(start, stop, Primitives.Type.ApplyArgAsType(type), type);
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(start, $"specified key is invalid");
                        return ParserResultOps.CreateInvalid(start, stop);
                    }
                }
            }
            else if (argArray.Length == 1 && argArray[0].Inner.Type.LogicalTypeEquals(Primitives.Int))
            {
                if (expr.Inner.Type.LogicalTypeNameEquals(Primitives.Array))
                {
                    var qual = QualifierStack.Peek();
                    return ParserResultOps.CreateEvalArrayIndexer(start, stop, expr.Inner.Type.GetArgAsArray(), qual, expr, argArray[0]);
                }
                else
                {
                    LogicalErrorHandler.ReportError(start, $"specified key is invalid");
                    return ParserResultOps.CreateInvalid(start, stop);
                }
            }
            else
            {
                LogicalErrorHandler.ReportError(start, $"specified key is invalid");
                return ParserResultOps.CreateInvalid(start, stop);
            }
        }

        public override void ExitPrefixExpr([NotNull] PrefixExprContext context)
        {
            var op   = context.op?.Text;
            var expr = context.expr()?.result;
            if (op == null || IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }
            
            var qual       = QualifierStack.Peek();
            var prefix     = ParserResultOps.CreatePrefix(context.Start, context.Stop, expr.Inner.Type, qual, op, expr);
            context.result = ParserResultOps.CreateExpr(prefix.Start, prefix.Stop, prefix);
        }

        public override void ExitMultiplicationExpr([NotNull] MultiplicationExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitAdditionExpr([NotNull] AdditionExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitShiftExpr([NotNull] ShiftExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitRelationExpr([NotNull] RelationExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, Primitives.Bool, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitEqualityExpr([NotNull] EqualityExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, Primitives.Bool, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitLogicalAndExpr([NotNull] LogicalAndExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitLogicalXorExpr([NotNull] LogicalXorExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitLogicalOrExpr([NotNull] LogicalOrExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, expr1.Inner.Type, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitConditionalAndExpr([NotNull] ConditionalAndExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (!expr1.Inner.Type.LogicalTypeEquals(Primitives.Bool) || !expr2.Inner.Type.LogicalTypeEquals(Primitives.Bool))
            {
                LogicalErrorHandler.ReportError(context.Start, $"invalid operand type");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, Primitives.Bool, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitConditionalOrExpr([NotNull] ConditionalOrExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (!expr1.Inner.Type.LogicalTypeEquals(Primitives.Bool) || !expr2.Inner.Type.LogicalTypeEquals(Primitives.Bool))
            {
                LogicalErrorHandler.ReportError(context.Start, $"invalid operand type");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, Primitives.Bool, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitCoalescingExpr([NotNull] CoalescingExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (!TypeOps.IsAssignableFrom(expr1.Inner.Type, expr2.Inner.Type) && !TypeOps.IsAssignableFrom(expr2.Inner.Type, expr1.Inner.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"invalid operand type");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var type       = TypeOps.IsAssignableFrom(expr1.Inner.Type, expr2.Inner.Type) ? expr1.Inner.Type : expr2.Inner.Type;
            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, type, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void ExitRightPipelineExpr([NotNull] RightPipelineExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (exprs[0].Inner.Type.LogicalTypeNameEquals(Primitives.Tuple))
            {
                context.result = ExitEvalFuncExprWithSpread(context.Start, context.Stop, exprs[1], exprs[0]);
            }
            else
            {
                var args       = new ArgExprResult[] { ParserResultOps.CreateArgExpr(exprs[0].Start, exprs[0].Stop, exprs[0], false) };
                context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, exprs[1], args);
            }
        }

        public override void ExitLeftPipelineExpr([NotNull] LeftPipelineExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (exprs[1].Inner.Type.LogicalTypeNameEquals(Primitives.Tuple))
            {
                context.result = ExitEvalFuncExprWithSpread(context.Start, context.Stop, exprs[0], exprs[1]);
            }
            else
            {
                var args       = new ArgExprResult[] { ParserResultOps.CreateArgExpr(exprs[1].Start, exprs[1].Stop, exprs[1], false) };
                context.result = ExitEvalFuncExprWithArgs(context.Start, context.Stop, exprs[0], args);
            }
        }

        public override void ExitAssignExpr([NotNull] AssignExprContext context)
        {
            var op    = context.op?.Text;
            var exprs = context.expr();
            if (op == null || exprs.Length != 2)
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var expr1 = exprs[0]?.result;
            var expr2 = exprs[1]?.result;
            if (IsInvalid(expr1) || IsInvalid(expr2))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (expr1.Inner.LeftValues.Length != 1 || !TypeOps.IsAssignableFrom(expr1.Inner.Type, expr2.Inner.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"cannot be assigned");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            var infix      = ParserResultOps.CreateInfix(context.Start, context.Stop, Primitives.Unit, qual, op, expr1, expr2);
            context.result = ParserResultOps.CreateExpr(infix.Start, infix.Stop, infix);
        }

        public override void EnterLetInBindExpr([NotNull] LetInBindExprContext context)
        {
            var index          = Tables.GetLetInIndex();
            context.tableIndex = index;
            var qual           = QualifierStack.Peek();
            var scope          = new TeuchiUdonScope(new TeuchiUdonLetIn(index, qual), TeuchiUdonScopeMode.LetIn);
            QualifierStack.PushScope(scope);
        }

        public override void ExitLetInBindExpr([NotNull] LetInBindExprContext context)
        {
            var varBind = context.varBind()?.result;
            var expr    = context.expr   ()?.result;
            if (IsInvalid(varBind) || IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            QualifierStack.Pop();

            var index      = context.tableIndex;
            var qual       = QualifierStack.Peek();
            var letInBind  = ParserResultOps.CreateLetInBind(context.Start, context.Stop, expr.Inner.Type, index, qual, varBind, expr);
            context.result = ParserResultOps.CreateExpr(letInBind.Start, letInBind.Stop, letInBind);
        }

        public override void ExitIfExpr([NotNull] IfExprContext context)
        {
            var isoExpr   = context.isoExpr  ()?.result;
            var statement = context.statement()?.result;
            if (IsInvalid(isoExpr) || IsInvalid(statement))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (!isoExpr.Expr.Inner.Type.LogicalTypeEquals(Primitives.Bool))
            {
                LogicalErrorHandler.ReportError(context.Start, $"condition expression must be bool type");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (statement is LetBindResult)
            {
                LogicalErrorHandler.ReportError(context.Start, $"if expression cannot contain let bind");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var if_        = ParserResultOps.CreateIf(context.Start, context.Stop, Primitives.Unit, new ExprResult[] { isoExpr.Expr }, new StatementResult[] { statement });
            context.result = ParserResultOps.CreateExpr(if_.Start, if_.Stop, if_);
        }

        public override void ExitIfElifExpr([NotNull] IfElifExprContext context)
        {
            var isoExprs   = context.isoExpr  ().Select(x => x?.result);
            var statements = context.statement().Select(x => x?.result);
            if (isoExprs.Any(x => IsInvalid(x)) || statements.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (!isoExprs.All(x => x.Expr.Inner.Type.LogicalTypeEquals(Primitives.Bool)))
            {
                LogicalErrorHandler.ReportError(context.Start, $"condition expression must be bool type");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (statements.Any(x => x is LetBindResult))
            {
                LogicalErrorHandler.ReportError(context.Start, $"if expression cannot contain let bind");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var if_        = ParserResultOps.CreateIf(context.Start, context.Stop, Primitives.Unit, isoExprs.Select(x => x.Expr), statements);
            context.result = ParserResultOps.CreateExpr(if_.Start, if_.Stop, if_);
        }

        public override void ExitIfElseExpr([NotNull] IfElseExprContext context)
        {
            var isoExprs = context.isoExpr().Select(x => x?.result);
            if (isoExprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var exprs = isoExprs.Select(x => x.Expr).ToArray();
            if (!exprs[0].Inner.Type.LogicalTypeEquals(Primitives.Bool))
            {
                LogicalErrorHandler.ReportError(context.Start, $"condition expression must be bool type");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var type = SyntaxOps.GetUpperType(new TeuchiUdonType[] { exprs[1].Inner.Type, exprs[2].Inner.Type });
            if (TypeOps.ContainsUnknown(type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"if expression types are not compatible");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var if_        = ParserResultOps.CreateIfElse(context.Start, context.Stop, type, new ExprResult[] { exprs[0] }, new ExprResult[] { exprs[1] }, exprs[2]);
            context.result = ParserResultOps.CreateExpr(if_.Start, if_.Stop, if_);
        }

        public override void ExitIfElifElseExpr([NotNull] IfElifElseExprContext context)
        {
            var isoExprs = context.isoExpr().Select(x => x?.result);
            if (isoExprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var exprs      = isoExprs.Select(x => x.Expr).ToArray();
            var init       = exprs.Take(exprs.Length - 1);
            var conditions = init.Where((x, i) => i % 2 == 0);
            var thenParts  = init.Where((x, i) => i % 2 == 1);
            var elsePart   = exprs[exprs.Length - 1];

            if (!conditions.All(x => x.Inner.Type.LogicalTypeEquals(Primitives.Bool)))
            {
                LogicalErrorHandler.ReportError(context.Start, $"condition expression must be bool type");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var type = SyntaxOps.GetUpperType(thenParts.Concat(new ExprResult[] { elsePart }).Select(x => x.Inner.Type));
            if (TypeOps.ContainsUnknown(type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"if expression types are not compatible");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var if_        = ParserResultOps.CreateIfElse(context.Start, context.Stop, type, conditions, thenParts, elsePart);
            context.result = ParserResultOps.CreateExpr(if_.Start, if_.Stop, if_);
        }

        public override void ExitWhileExpr([NotNull] WhileExprContext context)
        {
            var isoExpr = context.isoExpr()?.result;
            var expr    = context.expr   ()?.result;
            if (IsInvalid(isoExpr) || IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            if (!isoExpr.Expr.Inner.Type.LogicalTypeEquals(Primitives.Bool))
            {
                LogicalErrorHandler.ReportError(context.Start, $"condition expression must be bool type");
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            var while_     = ParserResultOps.CreateWhile(context.Start, context.Stop, Primitives.Unit, isoExpr.Expr, expr);
            context.result = ParserResultOps.CreateExpr(while_.Start, while_.Stop, while_);
        }

        public override void EnterForExpr([NotNull] ForExprContext context)
        {
            var index          = Tables.GetBlockIndex();
            context.tableIndex = index;
            var qual           = QualifierStack.Peek();
            var scope          = new TeuchiUdonScope(new TeuchiUdonFor(index), TeuchiUdonScopeMode.For);
            QualifierStack.PushScope(scope);
        }

        public override void ExitForExpr([NotNull] ForExprContext context)
        {
            var forBinds = context.forBind().Select(x => x?.result);
            var expr     = context.expr()?.result;
            if (forBinds.Any(x => IsInvalid(x)) || IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            QualifierStack.Pop();

            var index      = context.tableIndex;
            var for_       = ParserResultOps.CreateFor(context.Start, context.Stop, Primitives.Unit, index, forBinds, expr);
            context.result = ParserResultOps.CreateExpr(for_.Start, for_.Stop, for_);
        }

        public override void ExitLoopExpr([NotNull] LoopExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }
            
            var loop       = ParserResultOps.CreateLoop(context.Start, context.Stop, Primitives.Unit, expr);
            context.result = ParserResultOps.CreateExpr(loop.Start, loop.Stop, loop);
        }

        public override void EnterFuncExpr([NotNull] FuncExprContext context)
        {
            var index          = Tables.GetFuncIndex();
            context.tableIndex = index;
            var qual           = QualifierStack.Peek();
            var scope          = new TeuchiUdonScope(new TeuchiUdonFunc(index, qual), TeuchiUdonScopeMode.Func);
            QualifierStack.PushScope(scope);
        }

        public override void ExitFuncExpr([NotNull] FuncExprContext context)
        {
            var varDecl = context.varDecl()?.result;
            var expr    = context.expr   ()?.result;
            if (IsInvalid(varDecl) || IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                return;
            }

            QualifierStack.Pop();

            var qual          = QualifierStack.Peek();
            var inType        = SyntaxOps.ToOneType(varDecl.Types);
            var outType       = expr.Inner.Type;
            var deterministic = ParserResultOps.Deterministic(expr.Inner) && varDecl.Vars.All(x => !TypeOps.ContainsFunc(x.Type));

            var type =
                deterministic ?
                    new TeuchiUdonType
                    (
                        TeuchiUdonQualifier.Top,
                        Primitives.DetFunc.Name,
                        new TeuchiUdonType[] { inType, outType },
                        Primitives.DetFunc.LogicalName,
                        TypeOps.GetRealName(Primitives.UInt),
                        Primitives.UInt.RealType
                    ) :
                    new TeuchiUdonType
                    (
                        TeuchiUdonQualifier.Top,
                        Primitives.Func.Name,
                        new TeuchiUdonType[] { inType, outType },
                        Primitives.Func.LogicalName,
                        TypeOps.GetRealName(Primitives.UInt),
                        Primitives.UInt.RealType
                    );

            var varBind = qual.GetLast<TeuchiUdonVarBind>();
            var args    = varDecl.Vars;
            if (varBind != null && varBind.Qualifier == TeuchiUdonQualifier.Top && varBind.VarNames.Length == 1 && StaticTables.Events.ContainsKey(varBind.VarNames[0]))
            {
                var ev = StaticTables.Events[varBind.VarNames[0]];
                if (ev.OutTypes.Length != args.Length)
                {
                    LogicalErrorHandler.ReportError(context.Start, $"arguments of '{varBind.VarNames[0]}' event is not compatible");
                    context.result = ParserResultOps.CreateExpr(context.Start, context.Stop); 
                    return;
                }
                else
                {
                    var evTypes  = SyntaxOps.ToOneType(ev.OutTypes);
                    var argTypes = SyntaxOps.ToOneType(args.Select(x => x.Type));
                    if (TypeOps.IsAssignableFrom(evTypes, argTypes))
                    {
                        args =
                            args
                            .Zip(ev.OutParamUdonNames, (a, n) => (  a,   n))
                            .Zip(ev.OutTypes         , (x, t) => (x.a, x.n, t))
                            .Select(x =>
                                {
                                    var name = TeuchiUdonTableOps.GetEventParamName(ev.Name, x.n);
                                    if (!TeuchiUdonTableOps.IsValidVarName(name))
                                    {
                                        LogicalErrorHandler.ReportError(context.Start, $"'{name}' is invalid variable name");
                                        return x.a;
                                    }
                                    
                                    var v = new TeuchiUdonVar(Tables.GetVarIndex(), TeuchiUdonQualifier.Top, name, x.t, false, true);
                                    if (Tables.Vars.ContainsKey(v))
                                    {
                                        LogicalErrorHandler.ReportError(context.Start, $"'{v.Name}' conflicts with another variable");
                                        return x.a;
                                    }
                                    else
                                    {
                                        Tables.Vars.Add(v, v);
                                        return v;
                                    }
                                })
                            .ToArray();
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(context.Start, $"arguments of '{varBind.VarNames[0]}' event is not compatible");
                        context.result = ParserResultOps.CreateExpr(context.Start, context.Stop);
                        return;
                    }
                }
            }

            var index      = context.tableIndex;
            var func       = ParserResultOps.CreateFunc(context.Start, context.Stop, type, index, qual, args, varDecl, expr, deterministic);
            context.result = ParserResultOps.CreateExpr(func.Start, func.Stop, func);
        }
        
        public override void ExitElementsIterExpr([NotNull] ElementsIterExprContext context)
        {
            var isoExprs = context.isoExpr().Select(x => x?.result);
            if (isoExprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateElementsIterExpr(context.Start, context.Stop);
                return;
            }

            var exprs = isoExprs.Select(x => x.Expr);
            var type  = SyntaxOps.GetUpperType(exprs.Select(x => x.Inner.Type));
            if (type.LogicalTypeEquals(Primitives.Unknown))
            {
                LogicalErrorHandler.ReportError(context.Start, $"array element types are incompatible");
                context.result = ParserResultOps.CreateElementsIterExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            context.result = ParserResultOps.CreateElementsIterExpr(context.Start, context.Stop, type, qual, exprs);
        }

        public override void ExitRangeIterExpr([NotNull] RangeIterExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateRangeIterExpr(context.Start, context.Stop);
                return;
            }

            if (!TypeOps.IsSignedIntegerType(exprs[0].Inner.Type) || !TypeOps.IsSignedIntegerType(exprs[1].Inner.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"range expression is not signed integer type");
                context.result = ParserResultOps.CreateRangeIterExpr(context.Start, context.Stop);
                return;
            }
            else if (!exprs[0].Inner.Type.LogicalTypeEquals(exprs[1].Inner.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"range expression type is incompatible");
                context.result = ParserResultOps.CreateRangeIterExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            context.result = ParserResultOps.CreateRangeIterExpr(context.Start, context.Stop, exprs[0].Inner.Type, qual, exprs[0], exprs[1]);
        }

        public override void ExitSteppedRangeIterExpr([NotNull] SteppedRangeIterExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 3 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateSteppedRangeIterExpr(context.Start, context.Stop);
                return;
            }

            if
            (
                !TypeOps.IsSignedIntegerType(exprs[0].Inner.Type) ||
                !TypeOps.IsSignedIntegerType(exprs[1].Inner.Type) ||
                !TypeOps.IsSignedIntegerType(exprs[2].Inner.Type)
            )
            {
                LogicalErrorHandler.ReportError(context.Start, $"range expression is not signed integer type");
                context.result = ParserResultOps.CreateSteppedRangeIterExpr(context.Start, context.Stop);
                return;
            }
            else if (!exprs[0].Inner.Type.LogicalTypeEquals(exprs[1].Inner.Type) || !exprs[0].Inner.Type.LogicalTypeEquals(exprs[2].Inner.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"range expression type is incompatible");
                context.result = ParserResultOps.CreateSteppedRangeIterExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            context.result = ParserResultOps.CreateSteppedRangeIterExpr(context.Start, context.Stop, exprs[0].Inner.Type, qual, exprs[0], exprs[1], exprs[2]);
        }

        public override void ExitSpreadIterExpr([NotNull] SpreadIterExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateSpreadIterExpr(context.Start, context.Stop);
                return;
            }

            if (!expr.Inner.Type.LogicalTypeNameEquals(Primitives.Array))
            {
                LogicalErrorHandler.ReportError(context.Start, $"spread expression is not a array type");
                context.result = ParserResultOps.CreateSpreadIterExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            context.result = ParserResultOps.CreateSpreadIterExpr(context.Start, context.Stop, expr.Inner.Type.GetArgAsArray(), qual, expr);
        }

        public override void ExitIsoExpr([NotNull] IsoExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateIsoExpr(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateIsoExpr(context.Start, context.Stop, expr);
        }

        public override void ExitArgExpr([NotNull] ArgExprContext context)
        {
            var expr = context.expr()?.result;
            var rf   = context.REF() != null;
            if (IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateArgExpr(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateArgExpr(context.Start, context.Stop, expr, rf);
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

            var index          = Tables.GetVarBindIndex();
            context.tableIndex = index;
            var qual           = QualifierStack.Peek();
            var varNames       =
                varDecl is SingleVarDeclContext sv ? new string[] { sv.qualifiedVar()?.identifier()?.GetText() ?? "" } :
                varDecl is TupleVarDeclContext  tv ? tv.qualifiedVar().Select(x => x?.identifier()?.GetText() ?? "").ToArray() : Enumerable.Empty<string>();
            var scope   = new TeuchiUdonScope(new TeuchiUdonVarBind(index, qual, varNames), TeuchiUdonScopeMode.VarBind);
            QualifierStack.PushScope(scope);
        }

        public override void ExitLetForBind([NotNull] LetForBindContext context)
        {
            var varDecl     = context.varDecl    ()?.result;
            var forIterExpr = context.forIterExpr()?.result;
            if (IsInvalid(varDecl) || IsInvalid(forIterExpr))
            {
                context.result = ParserResultOps.CreateLetForBind(context.Start, context.Stop);
                return;
            }

            QualifierStack.Pop();

            var index = context.tableIndex;
            var qual  = QualifierStack.Peek();

            var vars = (TeuchiUdonVar[])null;
            if (varDecl.Vars.Length == 1)
            {
                var v = varDecl.Vars[0];
                var t = forIterExpr.Type;
                if (TypeOps.IsAssignableFrom(v.Type, t))
                {
                    if (TypeOps.ContainsUnknown(t))
                    {
                        ParserResultOps.BindType(forIterExpr, v.Type);
                    }

                    vars = new TeuchiUdonVar[]
                    {
                        new TeuchiUdonVar
                        (
                            Tables.GetVarIndex(),
                            v.Qualifier,
                            v.Name,
                            v.Type.LogicalTypeEquals(Primitives.Unknown) ? t : v.Type,
                            true,
                            false
                        )
                    };
                }
                else
                {
                    LogicalErrorHandler.ReportError(context.Start, $"expression cannot be assigned to variable");
                    vars = Array.Empty<TeuchiUdonVar>();
                }
            }
            else if (varDecl.Vars.Length >= 2)
            {
                if (forIterExpr.Type.LogicalTypeNameEquals(Primitives.Tuple))
                {
                    var vs = varDecl.Vars;
                    var ts = forIterExpr.Type.GetArgsAsTuple().ToArray();
                    if (vs.Length == ts.Length && varDecl.Types.Zip(ts, (v, t) => (v, t)).All(x => TypeOps.IsAssignableFrom(x.v, x.t)))
                    {
                        if (TypeOps.ContainsUnknown(forIterExpr.Type))
                        {
                            ParserResultOps.BindType(forIterExpr, SyntaxOps.ToOneType(vs.Select(x => x.Type)));
                        }
                        
                        vars = varDecl.Vars
                            .Zip(ts, (v, t) => (v, t))
                            .Select(x =>
                                new TeuchiUdonVar
                                (
                                    Tables.GetVarIndex(),
                                    x.v.Qualifier,
                                    x.v.Name,
                                    x.v.Type.LogicalTypeEquals(Primitives.Unknown) ? x.t : x.v.Type,
                                    true,
                                    false
                                )
                            )
                            .ToArray();
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(context.Start, $"expression cannot be assigned to variables");
                        vars = Array.Empty<TeuchiUdonVar>();
                    }
                }
                else
                {
                    LogicalErrorHandler.ReportError(context.Start, $"expression cannot be assigned to variables");
                    vars = Array.Empty<TeuchiUdonVar>();
                }
            }

            if (TypeOps.IsFunc(forIterExpr.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"function variable cannot be mutable");
            }

            context.result = ParserResultOps.CreateLetForBind(context.Start, context.Stop, index, qual, vars, varDecl, forIterExpr);
        }

        public override void ExitAssignForBind([NotNull] AssignForBindContext context)
        {
            var expr        = context.expr       ()?.result;
            var forIterExpr = context.forIterExpr()?.result;
            if (IsInvalid(expr) || IsInvalid(forIterExpr))
            {
                context.result = ParserResultOps.CreateAssignForBind(context.Start, context.Stop);
                return;
            }

            if (expr.Inner.LeftValues.Length != 1 || !TypeOps.IsAssignableFrom(expr.Inner.Type, forIterExpr.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"cannot be assigned");
                context.result = ParserResultOps.CreateAssignForBind(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateAssignForBind(context.Start, context.Stop, expr, forIterExpr);
        }

        public override void ExitRangeForIterExpr([NotNull] RangeForIterExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 2 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateRangeIterExpr(context.Start, context.Stop);
                return;
            }

            if (!TypeOps.IsSignedIntegerType(exprs[0].Inner.Type) || !TypeOps.IsSignedIntegerType(exprs[1].Inner.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"range expression is not signed integer type");
                context.result = ParserResultOps.CreateRangeIterExpr(context.Start, context.Stop);
                return;
            }
            else if (!exprs[0].Inner.Type.LogicalTypeEquals(exprs[1].Inner.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"range expression type is incompatible");
                context.result = ParserResultOps.CreateRangeIterExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            context.result = ParserResultOps.CreateRangeIterExpr(context.Start, context.Stop, exprs[0].Inner.Type, qual, exprs[0], exprs[1]);
        }

        public override void ExitSteppedRangeForIterExpr([NotNull] SteppedRangeForIterExprContext context)
        {
            var exprs = context.expr().Select(x => x?.result).ToArray();
            if (exprs.Length != 3 || exprs.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateSteppedRangeIterExpr(context.Start, context.Stop);
                return;
            }

            if
            (
                !TypeOps.IsSignedIntegerType(exprs[0].Inner.Type) ||
                !TypeOps.IsSignedIntegerType(exprs[1].Inner.Type) ||
                !TypeOps.IsSignedIntegerType(exprs[2].Inner.Type)
            )
            {
                LogicalErrorHandler.ReportError(context.Start, $"range expression is not signed integer type");
                context.result = ParserResultOps.CreateSteppedRangeIterExpr(context.Start, context.Stop);
                return;
            }
            else if (!exprs[0].Inner.Type.LogicalTypeEquals(exprs[1].Inner.Type) || !exprs[0].Inner.Type.LogicalTypeEquals(exprs[2].Inner.Type))
            {
                LogicalErrorHandler.ReportError(context.Start, $"range expression type is incompatible");
                context.result = ParserResultOps.CreateSteppedRangeIterExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            context.result = ParserResultOps.CreateSteppedRangeIterExpr(context.Start, context.Stop, exprs[0].Inner.Type, qual, exprs[0], exprs[1], exprs[2]);
        }

        public override void ExitSpreadForIterExpr([NotNull] SpreadForIterExprContext context)
        {
            var expr = context.expr()?.result;
            if (IsInvalid(expr))
            {
                context.result = ParserResultOps.CreateSpreadIterExpr(context.Start, context.Stop);
                return;
            }

            if (!expr.Inner.Type.LogicalTypeNameEquals(Primitives.Array))
            {
                LogicalErrorHandler.ReportError(context.Start, $"spread expression is not a array type");
                context.result = ParserResultOps.CreateSpreadIterExpr(context.Start, context.Stop);
                return;
            }

            var qual       = QualifierStack.Peek();
            context.result = ParserResultOps.CreateSpreadIterExpr(context.Start, context.Stop, expr.Inner.Type.GetArgAsArray(), qual, expr);
        }

        public override void ExitUnitLiteral([NotNull] UnitLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var type       = Primitives.Unit;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, "()", null);
        }

        public override void ExitNullLiteral([NotNull] NullLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var text       = context.GetText();
            var type       = Primitives.NullType;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, text, null);
        }

        public override void ExitBoolLiteral([NotNull] BoolLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var text       = context.GetText();
            var type       = Primitives.Bool;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToBoolValue(context.Start, text);
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, text, value);
        }

        private object ToBoolValue(IToken token, string text)
        {
            try
            {
                return Convert.ToBoolean(text);
            }
            catch
            {
                LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to int");
                return null;
            }
        }

        public override void ExitIntegerLiteral([NotNull] IntegerLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var text = context.GetText().Replace("_", "").ToLower();

            var index = 0;
            var count = text.Length;
            var basis = 10;
            var type  = Primitives.Int;

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = Primitives.ULong;
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = Primitives.Long;
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = Primitives.UInt;
            }

            if (count >= 2 && text.StartsWith("0"))
            {
                index++;
                count--;
                basis = 8;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue(context.Start, type, text.Substring(index, count), basis);
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, text, value);
        }

        public override void ExitHexIntegerLiteral([NotNull] HexIntegerLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var text = context.GetText().Replace("_", "").ToLower();

            var index = 2;
            var count = text.Length - 2;
            var basis = 16;
            var type  = Primitives.Int;

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = Primitives.ULong;
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = Primitives.Long;
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = Primitives.UInt;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue(context.Start, type, text.Substring(index, count), basis);
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, text, value);
        }

        public override void ExitBinIntegerLiteral([NotNull] BinIntegerLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var text = context.GetText().Replace("_", "").ToLower();

            var index = 2;
            var count = text.Length - 2;
            var basis = 2;
            var type  = Primitives.Int;

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = Primitives.ULong;
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = Primitives.Long;
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = Primitives.UInt;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue(context.Start, type, text.Substring(index, count), basis);
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, text, value);
        }

        private object ToIntegerValue(IToken token, TeuchiUdonType type, string text, int basis)
        {
            if (type.LogicalTypeEquals(Primitives.Int))
            {
                try
                {
                    return Convert.ToInt32(text, basis);
                }
                catch
                {
                    LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to int");
                    return null;
                }
            }
            else if (type.LogicalTypeEquals(Primitives.UInt))
            {
                try
                {
                    return Convert.ToUInt32(text, basis);
                }
                catch
                {
                    LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to uint");
                    return null;
                }
            }
            else if (type.LogicalTypeEquals(Primitives.Long))
            {
                try
                {
                    return Convert.ToInt64(text, basis);
                }
                catch
                {
                    LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to long");
                    return null;
                }
            }
            else if (type.LogicalTypeEquals(Primitives.ULong))
            {
                try
                {
                    return Convert.ToUInt64(text, basis);
                }
                catch
                {
                    LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to ulong");
                    return null;
                }
            }
            else
            {
                LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to unknown");
                return null;
            }
        }

        public override void ExitRealLiteral([NotNull] RealLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var text = context.GetText().Replace("_", "").ToLower();

            var count = text.Length;
            var type  = Primitives.Float;

            if (text.EndsWith("f"))
            {
                count--;
            }
            else if (text.EndsWith("d"))
            {
                count--;
                type = Primitives.Double;
            }
            else if (text.EndsWith("m"))
            {
                count--;
                type = Primitives.Decimal;
            }

            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToRealValue(context.Start, type, text.Substring(0, count));
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, text, value);
        }

        private object ToRealValue(IToken token, TeuchiUdonType type, string text)
        {
            if (type.LogicalTypeEquals(Primitives.Float))
            {
                try
                {
                    return Convert.ToSingle(text);
                }
                catch
                {
                    LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to float");
                    return null;
                }
            }
            else if (type.LogicalTypeEquals(Primitives.Double))
            {
                try
                {
                    return Convert.ToDouble(text);
                }
                catch
                {
                    LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to double");
                    return null;
                }
            }
            else if (type.LogicalTypeEquals(Primitives.Decimal))
            {
                try
                {
                    return Convert.ToDecimal(text);
                }
                catch
                {
                    LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to decimal");
                    return null;
                }
            }
            else
            {
                LogicalErrorHandler.ReportError(token, $"failed to convert '{token.Text}' to unknown");
                return null;
            }
        }

        public override void ExitCharacterLiteral([NotNull] CharacterLiteralContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var text       = context.GetText();
            var type       = Primitives.Char;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToCharacterValue(context.Start, text.Substring(1, text.Length - 2));
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, text, value);
        }

        public override void ExitRegularString([NotNull] RegularStringContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var text       = context.GetText();
            var type       = Primitives.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToRegularStringValue(context.Start, text.Substring(1, text.Length - 2));
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, text, value);
        }

        public override void ExitVervatiumString([NotNull] VervatiumStringContext context)
        {
            if (context.ChildCount == 0 || context.Parent == null)
            {
                context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop);
                return;
            }

            var text       = context.GetText();
            var type       = Primitives.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToVervatiumStringValue(context.Start, text.Substring(2, text.Length - 3));
            context.result = ParserResultOps.CreateLiteral(context.Start, context.Stop, type, tableIndex, text, value);
        }

        private object ToCharacterValue(IToken token, string text)
        {
            var ch = EscapeRegularString(token, text);

            if (ch.Length != 1)
            {
                LogicalErrorHandler.ReportError(token, $"invalid length of character literal");
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
                                LogicalErrorHandler.ReportError(token, $"invalid char detected");
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
                                    LogicalErrorHandler.ReportError(token, $"invalid char detected");
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
                                    LogicalErrorHandler.ReportError(token, $"invalid char detected");
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
                                    LogicalErrorHandler.ReportError(token, $"invalid char detected");
                                    return "";
                                }
                                u1 = u1 * 16 + d;
                            }
                            return ((char)u0).ToString() + ((char)u1).ToString();
                        }
                        default:
                            LogicalErrorHandler.ReportError(token, $"invalid char detected");
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
                        LogicalErrorHandler.ReportError(token, $"invalid char detected");
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
            if (context.ChildCount == 0)
            {
                context.result = ParserResultOps.CreateThis(context.Start, context.Stop);
                return;
            }

            var text       = context.GetText();
            var type       = Primitives.String;
            context.result = ParserResultOps.CreateThis(context.Start, context.Stop);
        }

        public override void ExitInterpolatedRegularString([NotNull] InterpolatedRegularStringContext context)
        {
            var parts = context.interpolatedRegularStringPart().Select(x => x?.result);
            if (parts.Any(x => IsInvalid(x)))
            {
                context.result = ParserResultOps.CreateInterpolatedString(context.Start, context.Stop);
                return;
            }

            var exprs = parts.Where(x => x is ExprInterpolatedStringPartResult).Select(x => ((ExprInterpolatedStringPartResult)x).Expr);
            if (exprs.Any(x => !TypeOps.IsDotNetType(x.Inner.Type)))
            {
                LogicalErrorHandler.ReportError(context.Start, $"expression type is incompatible");
                context.result = ParserResultOps.CreateInterpolatedString(context.Start, context.Stop);
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
            var literal    = SyntaxOps.CreateValueLiteral(((InterpolatedRegularStringExprContext)context.Parent).tableIndex, joined, Primitives.String);
            var qual       = QualifierStack.Peek();
            context.result = ParserResultOps.CreateInterpolatedString(context.Start, context.Stop, qual, literal, exprs);
        }

        public override void ExitRegularStringInterpolatedStringPart([NotNull] RegularStringInterpolatedStringPartContext context)
        {
            var rawString = context.REGULAR_STRING_INSIDE()?.GetText();
            if (rawString == null)
            {
                context.result = ParserResultOps.CreateRegularStringInterpolatedStringPart(context.Start, context.Stop);
                return;
            }

            var invalid = Regex.Match(rawString, @"\{\d+\}");
            if (invalid.Success)
            {
                LogicalErrorHandler.ReportError(context.Start, $"invalid word '{invalid.Value}' detected");
                context.result = ParserResultOps.CreateRegularStringInterpolatedStringPart(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateRegularStringInterpolatedStringPart(context.Start, context.Stop, rawString);
        }

        public override void ExitExprInterpolatedStringPart([NotNull] ExprInterpolatedStringPartContext context)
        {
            var isoExpr = context.isoExpr()?.result;
            if (IsInvalid(isoExpr))
            {
                context.result = ParserResultOps.CreateExprInterpolatedStringPart(context.Start, context.Stop);
                return;
            }

            context.result = ParserResultOps.CreateExprInterpolatedStringPart(context.Start, context.Stop, isoExpr.Expr);
        }
    }
}
