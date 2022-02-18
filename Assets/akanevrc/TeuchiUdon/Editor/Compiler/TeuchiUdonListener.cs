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

        public override void ExitEveryRule([NotNull] ParserRuleContext context)
        {
            if (context is TopStatementContext || context is StatementContext)
            {
                TeuchiUdonTables.Instance.ResetOutValueIndex();
            }
        }

        public override void ExitTarget([NotNull] TargetContext context)
        {
            var body = context.body()?.result;

            TeuchiUdonAssemblyWriter.Instance.PushDataPart
            (
                TeuchiUdonStrategy.Instance.GetDataPartFromTables()
            );
            TeuchiUdonAssemblyWriter.Instance.PushCodePart
            (
                TeuchiUdonStrategy.Instance.GetCodePartFromTables(),
                TeuchiUdonStrategy.Instance.GetCodePartFromResult(body == null ? (TeuchiUdonParserResult)new UnitResult(null) : (TeuchiUdonParserResult)body)
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
            context.result    = new BodyResult(context.Start, topStatements);
        }

        public override void ExitVarBindTopStatement([NotNull] VarBindTopStatementContext context)
        {
            var varBind = context.varBind()?.result;
            if (varBind == null) return;

            var attrs                = context.varAttr().Select(x => x.result);
            var (init, export, sync) = ExtractFromVarAttrs(attrs);

            if (varBind.Vars.Length != 1 && (init != null || export || sync != TeuchiUdonSyncMode.Disable))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"tuple cannot be specified with any attributes");
            }
            else if (varBind.Vars.Length == 1 && varBind.Vars[0].Type.LogicalTypeNameEquals(TeuchiUdonType.Func))
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
                if (export)
                {
                    if (init == null && varBind.Expr.Inner is LiteralResult literal)
                    {
                        TeuchiUdonTables.Instance.ExportedVars.Add(varBind.Vars[0], literal.Literal);
                    }
                    else
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"exported valiable cannot be bound non-literal expression");
                    }
                }
                else if (init == null)
                {
                    init = "_start";
                }
                
                if (sync != TeuchiUdonSyncMode.Disable)
                {
                    TeuchiUdonTables.Instance.SyncedVars.Add(varBind.Vars[0], sync);
                }
                if (export || sync != TeuchiUdonSyncMode.Disable)
                {
                    TeuchiUdonTables.Instance.UnbufferedVars.Add(varBind.Vars[0], varBind.Vars[0]);
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
            var expr = context.expr()?.result;
            if (expr == null) return;

            var attrs = context.exprAttr().Select(x => x.result);
            var init  = ExtractFromExprAttrs(attrs);

            if (init == null)
            {
                init = "_start";
            }

            expr.ReturnsValue = false;
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
            var identifier = context.identifier()?.result;
            if (identifier == null) return;

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
            var identifier = context.identifier()?.result;
            if (identifier == null) return;

            context.result = new InitExprAttrResult(context.Start, identifier);
        }

        public override void EnterVarBind([NotNull] VarBindContext context)
        {
            var varDecl = context.varDecl();
            if (varDecl == null || varDecl is SingleVarDeclContext c && c.identifier() == null) return;

            var index          = TeuchiUdonTables.Instance.GetVarBindIndex();
            context.tableIndex = index;
            var qual           = TeuchiUdonQualifierStack.Instance.Peek();
            var varNames       =
                varDecl is SingleVarDeclContext sv ? new string[] { sv.identifier().GetText() } :
                varDecl is TupleVarDeclContext  tv ? tv.identifier().Select(x => x.GetText()).ToArray() : new string[0];
            var scope   = new TeuchiUdonScope(new TeuchiUdonVarBind(index, qual, varNames), TeuchiUdonScopeMode.VarBind);
            TeuchiUdonQualifierStack.Instance.PushScope(scope);
        }

        public override void ExitVarBind([NotNull] VarBindContext context)
        {
            var varDecl = context.varDecl()?.result;
            var expr    = context.expr   ()?.result;
            if (varDecl == null || expr == null) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var index = context.tableIndex;
            var qual  = TeuchiUdonQualifierStack.Instance.Peek();

            var vars = (TeuchiUdonVar[])null;
            if (varDecl.Vars.Length == 1)
            {
                var v = varDecl.Vars[0];
                var t = expr.Inner.Type;
                if (v.Type.IsAssignableFrom(t))
                {
                    
                    vars = new TeuchiUdonVar[] { new TeuchiUdonVar(TeuchiUdonTables.Instance.GetVarIndex(), v.Qualifier, v.Name, v.Type.LogicalTypeNameEquals(TeuchiUdonType.Unknown) ? t : v.Type) };
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
                            .Select(x => new TeuchiUdonVar(TeuchiUdonTables.Instance.GetVarIndex(), x.v.Qualifier, x.v.Name, x.v.Type.LogicalTypeNameEquals(TeuchiUdonType.Unknown) ? x.t : x.v.Type))
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

            context.result = new VarBindResult(context.Start, index, qual, vars, varDecl, expr);
        }

        public override void ExitUnitVarDecl([NotNull] UnitVarDeclContext context)
        {
            var identifiers = new IdentifierResult[0];
            var qualifieds  = new ExprResult[0];

            context.result  = ExitVarDecl(context.Start, identifiers, qualifieds, context.isActual);
        }

        public override void ExitSingleVarDecl([NotNull] SingleVarDeclContext context)
        {
            var identifier = context.identifier()?.result;
            if (identifier == null) return;

            var identifiers = new IdentifierResult[] { identifier };
            var expr        = context.expr()?.result ?? new ExprResult(context.Start, new UnknownTypeResult(context.Start)) { ReturnsValue = false };

            if (!expr.Inner.Type.LogicalTypeNameEquals(TeuchiUdonType.Type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"qualified type {expr.Inner.Type} is not a type");
            }

            var qualifieds = new ExprResult[] { expr };
            context.result = ExitVarDecl(context.Start, identifiers, qualifieds, context.isActual);
        }

        public override void ExitTupleVarDecl([NotNull] TupleVarDeclContext context)
        {
            var identifiers = context.identifier().Select(x => x .result);
            var qualifieds  = context.expr()
                .Select(x => x?.result ?? new ExprResult(context.Start, new UnknownTypeResult(context.Start)) { ReturnsValue = false })
                .ToArray();

            foreach (var q in qualifieds)
            {
                if (!q.Inner.Type.LogicalTypeNameEquals(TeuchiUdonType.Type))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"qualified type {q.Inner.Type} is not a type");
                }
            }

            context.result = ExitVarDecl(context.Start, identifiers, qualifieds, context.isActual);
        }

        private VarDeclResult ExitVarDecl(IToken token, IEnumerable<IdentifierResult> identifiers, IEnumerable<ExprResult> qualifieds, bool isActual)
        {
            var oldQual = (TeuchiUdonQualifier)null;
            if (isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var qualifier = TeuchiUdonQualifierStack.Instance.Peek();

            if (isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);

            return new VarDeclResult(token, qualifier, identifiers, qualifieds);
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

            var label = scope == null ? (IIndexedLabel)InvalidLabel.Instance : (IIndexedLabel)((TeuchiUdonFunc)scope.Label).Return;
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
            var expr = context.expr()?.result;
            if (expr == null) return;

            TeuchiUdonQualifierStack.Instance.Pop();

            var type       = expr.Inner.Type;
            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var statements = context.statement().Select(x => x.result);
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
                        evalVar  = new EvalVarResult(context.Start, type, v, identifier);
                        break;
                    }

                    var qt = new TeuchiUdonType(qual, identifier.Name);
                    if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                    {
                        var type  = TeuchiUdonTables.Instance.Types[qt];
                        var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                        evalVar   = new EvalTypeResult(context.Start, outer, type, identifier);
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

            var expr1 = exprs[0].result;
            var expr2 = exprs[1].result;

            if (expr1.Inner is EvalVarCandidateResult varCandidate1)
            {
                var eval = (TypedResult)null;

                var scopes  = new TeuchiUdonScope[] { new TeuchiUdonScope(new TextLabel(varCandidate1.Identifier.Name)) };
                var newQual = new TeuchiUdonQualifier(scopes);
                if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(newQual))
                {
                    var q    = TeuchiUdonTables.Instance.Qualifiers[newQual];
                    var type = TeuchiUdonType.Qual.ApplyArgAsQual(q);
                    eval     = new EvalQualifierResult(varCandidate1.Token, type, q, varCandidate1.Identifier);
                }
                else
                {
                    foreach (var qual in TeuchiUdonQualifierStack.Instance.Qualifiers)
                    {
                        var qt = new TeuchiUdonType(qual, varCandidate1.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate1.Token, outer, type, varCandidate1.Identifier);
                            break;
                        }
                    }

                    if (eval == null)
                    {
                        TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate1.Token, $"'{varCandidate1.Identifier.Name}' is not defined");
                        eval = new BottomResult(varCandidate1.Token);
                    }
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
                        var qual     = type1.GetArgAsQual();
                        var scope    = new TeuchiUdonScope(new TextLabel(varCandidate2.Identifier.Name));
                        var appended = qual.Append(scope);
                        if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(appended))
                        {
                            var q    = TeuchiUdonTables.Instance.Qualifiers[appended];
                            var type = TeuchiUdonType.Qual.ApplyArgAsQual(q);
                            eval     = new EvalQualifierResult(varCandidate2.Token, type, q, varCandidate2.Identifier);
                            break;
                        }

                        var qt = new TeuchiUdonType(qual, varCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = TeuchiUdonType.Type.ApplyArgAsType(type);
                            eval      = new EvalTypeResult(varCandidate2.Token, outer, type, varCandidate2.Identifier);
                            break;
                        }

                        var qv   = new TeuchiUdonVar(qual, varCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                        {
                            var v    = TeuchiUdonTables.Instance.Vars[qv];
                            var type = v.Type;
                            eval     = new EvalVarResult(varCandidate2.Token, type, v, varCandidate2.Identifier);
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
                            eval      = new EvalTypeResult(varCandidate2.Token, outer, type, varCandidate2.Identifier);
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

                        var qg = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetGetterName(varCandidate2.Identifier.Name), new TeuchiUdonType[0]);
                        var g  = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qg).ToArray();
                        var qs = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetSetterName(varCandidate2.Identifier.Name), new TeuchiUdonType[] { type1 });
                        var s  = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qs).ToArray();
                        if (g.Length == 1 && s.Length == 1)
                        {
                            throw new NotImplementedException();
                            //break;
                        }
                        else if (g.Length == 1)
                        {
                            throw new NotImplementedException();
                            //break;
                        }
                        else if (s.Length == 1)
                        {
                            throw new NotImplementedException();
                            //break;
                        }
                        else if (g.Length >= 2 || s.Length >= 2)
                        {
                            TeuchiUdonLogicalErrorHandler.Instance.ReportError(varCandidate2.Token, $"arguments of method '{varCandidate2.Identifier.Name}' is nondeterministic");
                            eval = new BottomResult(varCandidate2.Token);
                            break;
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

                        var qg = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetGetterName(varCandidate2.Identifier.Name), new TeuchiUdonType[0]);
                        var g  = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qg).ToArray();
                        var qs = new TeuchiUdonMethod(type1, TeuchiUdonTables.GetSetterName(varCandidate2.Identifier.Name), new TeuchiUdonType[] { type1 });
                        var s  = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qs).ToArray();
                        if (g.Length == 1 && s.Length == 1)
                        {
                            throw new NotImplementedException();
                            //break;
                        }
                        else if (g.Length == 1)
                        {
                            throw new NotImplementedException();
                            //break;
                        }
                        else if (s.Length == 1)
                        {
                            throw new NotImplementedException();
                            //break;
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

            var infix      = new InfixResult(expr1.Token, expr2.Inner.Type, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitEvalUnitFuncExpr([NotNull] EvalUnitFuncExprContext context)
        {
            var expr = context.expr()?.result;
            if (expr == null) return;

            var args       = new ExprResult[] { new ExprResult(context.Start, new UnitResult(context.Start)) };
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(context.Start, expr, args, parent);
        }

        public override void ExitEvalSingleFuncExpr([NotNull] EvalSingleFuncExprContext context)
        {
            var exprs = context.expr().Select(x => x.result).ToArray();
            if (exprs.Length != 2) return;

            var expr       = exprs[0];
            var args       = exprs.Skip(1);
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(context.Start, expr, args, parent);
        }

        public override void ExitEvalTupleFuncExpr([NotNull] EvalTupleFuncExprContext context)
        {
            var exprs = context.expr().Select(x => x.result).ToArray();
            if (exprs.Length < 3) return;

            var expr       = exprs[0];
            var args       = exprs.Skip(1);
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(context.Start, expr, args, parent);
        }

        private ExprResult ExitEvalFuncExpr(IToken token, ExprResult expr, IEnumerable<ExprResult> args, RuleContext parent)
        {
            var index    = TeuchiUdonTables.Instance.GetEvalFuncIndex();
            var type     = expr.Inner.Type;
            var evalFunc = (TypedResult)null;

            if (type.LogicalTypeNameEquals(TeuchiUdonType.Func))
            {
                var qual     = TeuchiUdonQualifierStack.Instance.Peek();
                var argTypes = args.Select(x => x.Inner.Type).ToArray();
                var iType    =
                    argTypes.Length == 0 ? TeuchiUdonType.Unit :
                    argTypes.Length >= 2 ? TeuchiUdonType.Tuple.ApplyArgsAsTuple(argTypes) :
                    argTypes[0];
                var oType    = TeuchiUdonType.Unknown;
                if (type.IsAssignableFromFunc(TeuchiUdonType.Func.ApplyArgsAsFunc(iType, oType)))
                {
                    var outType = type.GetArgAsFuncOutType();
                    evalFunc    = new EvalFuncResult(token, outType, index, qual, expr, args);
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"arguments of func is not compatible");
                    evalFunc = new BottomResult(token);
                }
            }
            else if (type.LogicalTypeNameEquals(TeuchiUdonType.Method))
            {
                var argTypes = args.Select(x => x.Inner.Type).ToArray();
                var ms       = type.GetMostCompatibleMethods(argTypes).ToArray();
                if (ms.Length == 0)
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"method is not defined");
                    evalFunc = new BottomResult(token);
                }
                else if (ms.Length == 1)
                {
                    var method  = ms[0];
                    var outType =
                        method.OutTypes.Length == 0 ? TeuchiUdonType.Unit :
                        method.OutTypes.Length >= 2 ? TeuchiUdonType.Tuple.ApplyArgsAsTuple(method.OutTypes) :
                        method.OutTypes[0];
                    evalFunc = new EvalMethodResult(token, outType, method, expr, args);
                }
                else
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"arguments of method is nondeterministic");
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

        public override void ExitNameOfExpr([NotNull] NameOfExprContext context)
        {
        }

        public override void ExitPostfixExpr([NotNull] PostfixExprContext context)
        {
            var op   = context.op?.Text;
            var expr = context.expr()?.result;
            if (op == null || expr == null) return;

            if ((op == "++" || op == "--") && expr.Inner.LeftValues.Length != 1)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"operand of '{op}' must be a left value");
            }

            var postfix    = new PostfixResult(context.Start, expr.Inner.Type, op, expr);
            context.result = new ExprResult(postfix.Token, postfix);
        }

        public override void ExitPrefixExpr([NotNull] PrefixExprContext context)
        {
            var op   = context.op?.Text;
            var expr = context.expr()?.result;
            if (op == null || expr == null) return;

            if ((op == "++" || op == "--") && expr.Inner.LeftValues.Length != 1)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"operand of '{op}' must be a left value");
            }
            
            var prefix     = new PrefixResult(context.Start, expr.Inner.Type, op, expr);
            context.result = new ExprResult(prefix.Token, prefix);
        }

        public override void ExitRangeExpr([NotNull] RangeExprContext context)
        {
        }

        public override void ExitMultiplicationExpr([NotNull] MultiplicationExprContext context)
        {
        }

        public override void ExitAdditionExpr([NotNull] AdditionExprContext context)
        {
        }

        public override void ExitShiftExpr([NotNull] ShiftExprContext context)
        {
        }

        public override void ExitRelationExpr([NotNull] RelationExprContext context)
        {
        }

        public override void ExitEqualityExpr([NotNull] EqualityExprContext context)
        {
        }

        public override void ExitLogicalAndExpr([NotNull] LogicalAndExprContext context)
        {
        }

        public override void ExitLogicalXorExpr([NotNull] LogicalXorExprContext context)
        {
        }

        public override void ExitLogicalOrExpr([NotNull] LogicalOrExprContext context)
        {
        }

        public override void ExitConditionalAndExpr([NotNull] ConditionalAndExprContext context)
        {
        }

        public override void ExitConditionalXorExpr([NotNull] ConditionalXorExprContext context)
        {
        }

        public override void ExitConditionalOrExpr([NotNull] ConditionalOrExprContext context)
        {
        }

        public override void ExitCoalescingExpr([NotNull] CoalescingExprContext context)
        {
        }

        public override void ExitConditionalExpr([NotNull] ConditionalExprContext context)
        {
        }

        public override void ExitAssignExpr([NotNull] AssignExprContext context)
        {
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

            var argTypes  = varDecl.Types.Any() ? varDecl.Types : new TeuchiUdonType[] { TeuchiUdonType.Unit };
            var funcTypes = argTypes.Concat(new TeuchiUdonType[] { expr.Inner.Type });
            var inType    =
                argTypes.Length == 0 ? TeuchiUdonType.Unit :
                argTypes.Length >= 2 ? TeuchiUdonType.Tuple.ApplyArgsAsTuple(argTypes) :
                argTypes[0];
            var outType   = expr.Inner.Type;
            var type      = TeuchiUdonType.Func.ApplyArgsAsFunc(inType, outType);

            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
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
