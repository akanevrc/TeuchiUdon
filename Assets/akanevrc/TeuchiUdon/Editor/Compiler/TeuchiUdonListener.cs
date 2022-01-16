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
            var body = context.body().result;
            TeuchiUdonAssemblyWriter.Instance.PushDataPart(body.GetAssemblyDataPart());
            TeuchiUdonAssemblyWriter.Instance.PushCodePart(body.GetAssemblyCodePart());
            TeuchiUdonAssemblyWriter.Instance.WriteAll(Parser.Output);
        }

        public override void EnterBody([NotNull] BodyContext context)
        {
            TeuchiUdonQualifierStack.Instance.Push(TeuchiUdonQualifier.Top);
        }

        public override void ExitBody([NotNull] BodyContext context)
        {
            // foreach (var (id, result) in vars.Select(x => (x.Key, x.Value)))
            // {
            //     Debug.Log($"{id} = {Eval(result.Expr)}");
            // }

            var topStatements = context.topStatement().Select(x => x.result).ToArray();
            context.result    = new BodyResult(context.Start, topStatements);
        }

        public override void ExitVarBindTopStatement([NotNull] VarBindTopStatementContext context)
        {
            var varBind    = context.varBind().result;
            context.result = new TopStatementResult(context.Start, varBind);
        }

        public override void ExitExprTopStatement([NotNull] ExprTopStatementContext context)
        {
            var expr       = context.expr().result;
            context.result = new TopStatementResult(context.Start, expr);
        }

        public override void EnterVarBind([NotNull] VarBindContext context)
        {
            TeuchiUdonQualifierStack.Instance.PushName(((SingleVarDeclContext)context.varDecl()).identifier().GetText());
        }

        public override void ExitVarBind([NotNull] VarBindContext context)
        {
            TeuchiUdonQualifierStack.Instance.Pop();

            var varDecl    = context.varDecl().result;
            var expr       = context.expr   ().result;
            var type       = expr.Inner.Type;

            varDecl.Vars[0].SetExpr(expr);

            if (varDecl.Types[0] != TeuchiUdonType.Bottom && !varDecl.Types[0].IsAssignableFrom(type))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(context.Start, $"type of '{varDecl.Vars[0].Name}' is not compatible");
            }
            
            context.result = new VarBindResult(context.Start, varDecl, type, expr);
        }

        public override void ExitUnitVarDecl([NotNull] UnitVarDeclContext context)
        {
            var oldQual = (TeuchiUdonQualifier)null;
            if (context.isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var qualifier   = TeuchiUdonQualifierStack.Instance.Peek();
            var identifiers = new IdentifierResult[0];
            var qualifieds  = new QualifiedResult [0];
            context.result  = new VarDeclResult(context.Start, qualifier, identifiers, qualifieds);

            if (context.isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);
        }

        public override void ExitSingleVarDecl([NotNull] SingleVarDeclContext context)
        {
            var oldQual = (TeuchiUdonQualifier)null;
            if (context.isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var qualifier   = TeuchiUdonQualifierStack.Instance.Peek();
            var identifiers = new IdentifierResult[] { context.identifier().result };
            var qualifieds  = new QualifiedResult[]
            {
                context.qualified()?.result ?? new QualifiedResult(context.Start, new IdentifierResult[0], TeuchiUdonType.Bottom)
            };
            context.result  = new VarDeclResult(context.Start, qualifier, identifiers, qualifieds);

            if (context.isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);
        }

        public override void ExitTupleVarDecl([NotNull] TupleVarDeclContext context)
        {
            var oldQual = (TeuchiUdonQualifier)null;
            if (context.isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var qualifier   = TeuchiUdonQualifierStack.Instance.Peek();
            var identifiers = context.identifier().Select(x => x .result);
            var qualifieds  = context.qualified ().Select(x => x?.result ?? new QualifiedResult(context.Start, new IdentifierResult[0], TeuchiUdonType.Bottom));
            context.result = new VarDeclResult(context.Start, qualifier, identifiers, qualifieds);

            if (context.isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);
        }

        public override void ExitQualified([NotNull] QualifiedContext context)
        {
            var identifiers = context.identifier().Select(x => x.result).ToArray();
            var qual        = new TeuchiUdonQualifier(identifiers.Take(identifiers.Length - 1).Select(x => x.Name));
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

        public override void EnterUnitBlockExpr([NotNull] UnitBlockExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetBlockIndex();
            context.tableIndex = index;
            TeuchiUdonQualifierStack.Instance.PushName(new TeuchiUdonBlock(index).GetUdonName());
        }

        public override void ExitUnitBlockExpr([NotNull] UnitBlockExprContext context)
        {
            var exprs      = context.expr().Select(x => x.result);
            var type       = TeuchiUdonType.Unit;
            var index      = context.tableIndex;
            var parens     = new BlockResult(context.Start, type, index, exprs);
            context.result = new ExprResult(parens.Token, parens);
        }

        public override void EnterValueBlockExpr([NotNull] ValueBlockExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetBlockIndex();
            context.tableIndex = index;
            TeuchiUdonQualifierStack.Instance.PushName(new TeuchiUdonBlock(index).GetUdonName());
        }

        public override void ExitValueBlockExpr([NotNull] ValueBlockExprContext context)
        {
            var exprs      = context.expr().Select(x => x.result);
            var type       = exprs.Last().Inner.Type;
            var index      = context.tableIndex;
            var parens     = new BlockResult(context.Start, type, index, exprs);
            context.result = new ExprResult(parens.Token, parens);
        }

        public override void ExitParensExpr([NotNull] ParensExprContext context)
        {
            var expr       = context.expr().result;
            var type       = expr.Inner.Type;
            var parens     = new ParensResult(context.Start, type, expr);
            context.result = new ExprResult(parens.Token, parens);
        }

        public override void EnterLiteralExpr([NotNull] LiteralExprContext context)
        {
            var index          = TeuchiUdonTables.Instance.GetLiteralIndex();
            context.tableIndex = index;
            TeuchiUdonQualifierStack.Instance.PushName(new TeuchiUdonLiteral(index).GetUdonName());
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
                var qual = TeuchiUdonQualifierStack.Instance.Peek();
                var qv   = new TeuchiUdonVar(qual, identifier.Name);
                if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                {
                    var v    = TeuchiUdonTables.Instance.Vars[qv];
                    var type = v.Expr.Inner.Type;
                    evalVar  = new EvalVarResult(context.Start, type, v, identifier);
                }
                else
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
            var args       = new ExprResult[0];
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(context.Start, identifier, args, parent);
        }

        public override void ExitEvalSingleFuncExpr([NotNull] EvalSingleFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = new ExprResult    [] { context.expr().result };
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
            var evalFunc = (TypedResult)null;

            if (parent is AccessExprContext)
            {
                evalFunc = new EvalMethodCandidateResult(token, identifier, args);
            }
            else
            {
                var qual = TeuchiUdonQualifierStack.Instance.Peek();
                var qv   = new TeuchiUdonVar(qual, identifier.Name);
                if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                {
                    var v            = TeuchiUdonTables.Instance.Vars[qv];
                    var type         = v.Expr.Inner.Type;
                    var unitArgTypes = args.Any() ? args.Select(x => x.Inner.Type) : new TeuchiUdonType[] { TeuchiUdonType.Unit };
                    if (type == TeuchiUdonType.Func.Apply(unitArgTypes))
                    {
                        throw new NotImplementedException();
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
                do
                {
                    var qual = TeuchiUdonQualifierStack.Instance.Peek();
                    var qt   = new TeuchiUdonType(qual, qualCandidate1.Identifier.Name);
                    if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                    {
                        var type  = TeuchiUdonTables.Instance.Types[qt];
                        var outer = TeuchiUdonType.Type.Apply(new object[] { type });
                        eval      = new EvalTypeResult(qualCandidate1.Token, outer, type, qualCandidate1.Identifier);
                        break;
                    }
                    
                    var newQual = new TeuchiUdonQualifier(new string[] { qualCandidate1.Identifier.Name });
                    if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(newQual))
                    {
                        var q    = TeuchiUdonTables.Instance.Qualifiers[newQual];
                        var type = TeuchiUdonType.Qual.Apply(new object[] { q });
                        eval     = new EvalQualifierResult(qualCandidate1.Token, type, q, qualCandidate1.Identifier);
                        break;
                    }

                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(qualCandidate1.Token, $"'{qualCandidate1.Identifier.Name}' is not defined");
                    eval = new BottomResult(qualCandidate1.Token);
                } while (false);

                expr1 = new ExprResult(eval.Token, eval);
            }
            else if (expr1.Inner is EvalMethodCandidateResult methodCandidate1)
            {
                var eval = (TypedResult)null;

                var qual = TeuchiUdonQualifierStack.Instance.Peek();
                var qv   = new TeuchiUdonVar(qual, methodCandidate1.Identifier.Name);
                if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                {
                    var v            = TeuchiUdonTables.Instance.Vars[qv];
                    var type         = v.Expr.Inner.Type;
                    var unitArgTypes = methodCandidate1.Args.Any() ?
                        methodCandidate1.Args.Select(x => x.Inner.Type) :
                        new TeuchiUdonType[] { TeuchiUdonType.Unit };
                    if (type == TeuchiUdonType.Func.Apply(unitArgTypes))
                    {
                        throw new NotImplementedException();
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
                        var qual = (TeuchiUdonQualifier)type1.Args[0];
                        var qv   = new TeuchiUdonVar(qual, qualCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                        {
                            var v    = TeuchiUdonTables.Instance.Vars[qv];
                            var type = v.Expr.Inner.Type;
                            eval     = new EvalVarResult(qualCandidate2.Token, type, v, qualCandidate2.Identifier);
                            break;
                        }

                        var qt = new TeuchiUdonType(qual, qualCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = TeuchiUdonType.Type.Apply(new object[] { type });
                            eval      = new EvalTypeResult(qualCandidate2.Token, outer, type, qualCandidate2.Identifier);
                            break;
                        }

                        var appended = qual.Append(qualCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Qualifiers.ContainsKey(appended))
                        {
                            var q    = TeuchiUdonTables.Instance.Qualifiers[appended];
                            var type = TeuchiUdonType.Qual.Apply(new object[] { q });
                            eval     = new EvalQualifierResult(qualCandidate2.Token, type, q, qualCandidate2.Identifier);
                            break;
                        }
                    }
                    else if (type1.TypeNameEquals(TeuchiUdonType.Type))
                    {
                        var t  = (TeuchiUdonType)type1.Args[0];
                        var q  = t.Qualifier.Append(t.Name);
                        var qt = new TeuchiUdonType(q, qualCandidate2.Identifier.Name);
                        if (TeuchiUdonTables.Instance.Types.ContainsKey(qt))
                        {
                            var type  = TeuchiUdonTables.Instance.Types[qt];
                            var outer = TeuchiUdonType.Type.Apply(new object[] { type });
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
                            var type = m[0].OutTypes.Length == 0 ? TeuchiUdonType.Void : m[0].OutTypes[0];
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
                            var type = m[0].OutTypes.Length == 0 ? TeuchiUdonType.Void : m[0].OutTypes[0];
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
            TeuchiUdonQualifierStack.Instance.PushName(new TeuchiUdonFunc(index).GetUdonName());
        }

        public override void ExitFuncExpr([NotNull] FuncExprContext context)
        {
            TeuchiUdonQualifierStack.Instance.Pop();

            var varDecl = context.varDecl().result;
            var expr    = context.expr   ().result;
            var type    = TeuchiUdonType.Func.Apply(varDecl.Types);

            if (!(context.Parent is VarBindContext varBind))
            {
                throw new NotImplementedException();
            }

            var index      = context.tableIndex;
            var qual       = TeuchiUdonQualifierStack.Instance.Peek();
            var name       = varBind.varDecl().result.Vars[0].Name;
            var func       = new FuncResult(context.Start, type, index, qual, name, varDecl, expr);
            context.result = new ExprResult(func.Token, func);
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
