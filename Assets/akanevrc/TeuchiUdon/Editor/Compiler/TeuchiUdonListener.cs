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
        private TeuchiUdonLogicalErrorHandler LogicalErrorHandler { get; }

        public TeuchiUdonListener(TeuchiUdonParser parser, TeuchiUdonLogicalErrorHandler logicalErrorHandler)
        {
            Parser = parser;
            LogicalErrorHandler = logicalErrorHandler;
        }

        public override void EnterTarget([NotNull] TargetContext context)
        {
            TeuchiUdonTables        .Instance.Init();
            TeuchiUdonQualifierStack.Instance.Init();
            TeuchiUdonAssemblyWriter.Instance.Init();
        }

        public override void ExitTarget([NotNull] TargetContext context)
        {
            var body = context.body().result;
            TeuchiUdonAssemblyWriter.Instance.PushDataBlock(body.GetAssemblyDataPart());
            TeuchiUdonAssemblyWriter.Instance.PushCodeBlock(body.GetAssemblyCodePart());
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
            context.result    = new BodyResult(topStatements[0].Token, topStatements);
        }

        public override void ExitVarBindTopStatement([NotNull] VarBindTopStatementContext context)
        {
            var varBind    = context.varBind().result;
            context.result = new TopStatementResult(varBind.Token, varBind);
        }

        public override void ExitExprTopStatement([NotNull] ExprTopStatementContext context)
        {
            var expr       = context.expr().result;
            context.result = new TopStatementResult(expr.Token, expr);
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
                LogicalErrorHandler.ReportError(varDecl.Token, $"type of '{varDecl.Vars[0].Name}' is not compatible");
            }
            
            context.result = new VarBindResult(varDecl.Token, varDecl, type, expr);
        }

        public override void ExitUnitVarDecl([NotNull] UnitVarDeclContext context)
        {
            var oldQual = (TeuchiUdonQualifier)null;
            if (context.isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var token       = context.OPEN_PARENS().Symbol;
            var qualifier   = TeuchiUdonQualifierStack.Instance.Peek();
            var identifiers = new IdentifierResult[0];
            var qualifieds  = new QualifiedResult [0];
            context.result  = new VarDeclResult(token, qualifier, identifiers, qualifieds);

            if (context.isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);
        }

        public override void ExitSingleVarDecl([NotNull] SingleVarDeclContext context)
        {
            var oldQual = (TeuchiUdonQualifier)null;
            if (context.isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var token       = context.identifier().result.Token;
            var qualifier   = TeuchiUdonQualifierStack.Instance.Peek();
            var identifiers = new IdentifierResult[] { context.identifier().result };
            var qualifieds  = new QualifiedResult[]
            {
                context.qualified()?.result ?? new QualifiedResult(token, new IdentifierResult[0], TeuchiUdonType.Bottom)
            };
            context.result  = new VarDeclResult(token, qualifier, identifiers, qualifieds);

            if (context.isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);
        }

        public override void ExitTupleVarDecl([NotNull] TupleVarDeclContext context)
        {
            var oldQual = (TeuchiUdonQualifier)null;
            if (context.isActual) oldQual = TeuchiUdonQualifierStack.Instance.Pop();

            var token       = context.OPEN_PARENS().Symbol;
            var qualifier   = TeuchiUdonQualifierStack.Instance.Peek();
            var identifiers = context.identifier().Select(x => x .result);
            var qualifieds  = context.qualified ().Select(x => x?.result ?? new QualifiedResult(token, new IdentifierResult[0], TeuchiUdonType.Bottom));
            context.result = new VarDeclResult(token, qualifier, identifiers, qualifieds);

            if (context.isActual) TeuchiUdonQualifierStack.Instance.Push(oldQual);
        }

        public override void ExitQualified([NotNull] QualifiedContext context)
        {
            var identifiers = context.identifier().Select(x => x.result).ToArray();
            var token       = identifiers[0].Token;
            var qual        = new TeuchiUdonQualifier(identifiers.Take(identifiers.Length - 1).Select(x => x.Name));
            var t           = new TeuchiUdonType(qual, identifiers[identifiers.Length - 1].Name);
            var type        = (TeuchiUdonType)null;
            if (TeuchiUdonTables.Instance.Types.ContainsKey(t))
            {
                type = TeuchiUdonTables.Instance.Types[t];
            }
            else
            {
                LogicalErrorHandler.ReportError(token, $"'{t}' is not defined");
                type = TeuchiUdonType.Bottom;
            }

            context.result = new QualifiedResult(token, identifiers, type);
        }

        public override void ExitIdentifier([NotNull] IdentifierContext context)
        {
            var token      = context.IDENTIFIER().Symbol;
            var name       = context.GetText().Replace("@", "");
            context.result = new IdentifierResult(token, name);
        }

        public override void ExitParensExpr([NotNull] ParensExprContext context)
        {
            var token      = context.OPEN_PARENS().Symbol;
            var expr       = context.expr().result;
            var type       = expr.Inner.Type;
            var parens     = new ParensResult(token, expr);
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
            var type    = literal.Type;
            context.result = new ExprResult(literal.Token, literal);
        }

        public override void ExitEvalVarExpr([NotNull] EvalVarExprContext context)
        {
            var identifier = context.identifier().result;
            var evalVar = (TypedResult)null;

            if (context.Parent is AccessExprContext)
            {
                evalVar = new EvalQualifierCandidateResult(identifier.Token, identifier);
            }
            else
            {
                var qual = TeuchiUdonQualifierStack.Instance.Peek();
                var qv   = new TeuchiUdonVar(qual, identifier.Name);
                if (TeuchiUdonTables.Instance.Vars.ContainsKey(qv))
                {
                    var v    = TeuchiUdonTables.Instance.Vars[qv];
                    var type = v.Expr.Inner.Type;
                    evalVar  = new EvalVarResult(identifier.Token, type, v, identifier);
                }
                else
                {
                    LogicalErrorHandler.ReportError(identifier.Token, $"'{identifier.Name}' is not defined");
                    evalVar = new BottomResult(identifier.Token);
                }
            }

            context.result = new ExprResult(evalVar.Token, evalVar);
        }

        public override void ExitEvalUnitFuncExpr([NotNull] EvalUnitFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = new ExprResult[0];
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(identifier, args, parent);
        }

        public override void ExitEvalSingleFuncExpr([NotNull] EvalSingleFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = new ExprResult    [] { context.expr().result };
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(identifier, args, parent);
        }

        public override void ExitEvalTupleFuncExpr([NotNull] EvalTupleFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = context.expr().Select(x => x.result).ToArray();
            var parent     = context.Parent;
            context.result = ExitEvalFuncExpr(identifier, args, parent);
        }

        private ExprResult ExitEvalFuncExpr
        (
            IdentifierResult identifier,
            IEnumerable<ExprResult> args,
            RuleContext parent
        )
        {
            var evalFunc = (TypedResult)null;

            if (parent is AccessExprContext)
            {
                evalFunc = new EvalMethodCandidateResult(identifier.Token, identifier, args);
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
                            LogicalErrorHandler.ReportError(identifier.Token, $"arguments of '{identifier.Name}' is not compatible");
                        }
                        else
                        {
                            LogicalErrorHandler.ReportError(identifier.Token, $"'{identifier.Name}' is not a function");
                        }
                        evalFunc = new BottomResult(identifier.Token);
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

                    LogicalErrorHandler.ReportError(qualCandidate1.Token, $"'{qualCandidate1.Identifier.Name}' is not defined");
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
                            LogicalErrorHandler.ReportError(methodCandidate1.Token, $"arguments of '{methodCandidate1.Identifier.Name}' is not compatible");
                        }
                        else
                        {
                            LogicalErrorHandler.ReportError(methodCandidate1.Token, $"'{methodCandidate1.Identifier.Name}' is not a function");
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
                            LogicalErrorHandler.ReportError(qualCandidate2.Token, $"arguments of method '{qualCandidate2.Identifier.Name}' is nondeterministic");
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
                            LogicalErrorHandler.ReportError(qualCandidate2.Token, $"arguments of method '{qualCandidate2.Identifier.Name}' is nondeterministic");
                            break;
                        }
                    }

                    LogicalErrorHandler.ReportError(qualCandidate2.Token, $"'{qualCandidate2.Identifier.Name}' is not defined");
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
                            LogicalErrorHandler.ReportError(methodCandidate2.Token, $"arguments of method '{methodCandidate2.Identifier.Name}' is nondeterministic");
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
                            LogicalErrorHandler.ReportError(methodCandidate2.Token, $"arguments of method '{methodCandidate2.Identifier.Name}' is nondeterministic");
                            break;
                        }
                    }

                    LogicalErrorHandler.ReportError(methodCandidate2.Token, $"'{methodCandidate2.Identifier.Name}' is not defined");
                    eval = new BottomResult(methodCandidate2.Token);
                } while (false);

                expr2 = new ExprResult(eval.Token, eval);
            }
            else
            {
                LogicalErrorHandler.ReportError(expr1.Token, $"invalid '.' operator");
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
            var func       = new FuncResult(varDecl.Token, type, index, qual, name, varDecl, expr);
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

            var token      = context.INTEGER_LITERAL().Symbol;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue(text.Substring(index, count), basis, type, token);
            context.result = new LiteralResult(token, type, tableIndex, text, value);
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

            var token      = context.HEX_INTEGER_LITERAL().Symbol;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue(text.Substring(index, count), basis, type, token);
            context.result = new LiteralResult(token, type, tableIndex, text, value);
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

            var token      = context.BIN_INTEGER_LITERAL().Symbol;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToIntegerValue(text.Substring(index, count), basis, type, token);
            context.result = new LiteralResult(token, type, tableIndex, text, value);
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
            var token      = context.REGULAR_STRING().Symbol;
            var type       = TeuchiUdonType.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToRegularStringValue(text.Substring(1, text.Length - 2));
            context.result = new LiteralResult(token, type, tableIndex, text, value);
        }

        public override void ExitVervatiumString([NotNull] VervatiumStringContext context)
        {
            var text       = context.GetText();
            var token      = context.VERBATIUM_STRING().Symbol;
            var type       = TeuchiUdonType.String;
            var tableIndex = ((LiteralExprContext)context.Parent).tableIndex;
            var value      = ToVervatiumStringValue(text.Substring(2, text.Length - 3));
            context.result = new LiteralResult(token, type, tableIndex, text, value);
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

        private object ToIntegerValue(string text, int basis, TeuchiUdonType type, IToken token)
        {
            if (type == TeuchiUdonType.Int)
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
            else if (type == TeuchiUdonType.UInt)
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
            else if (type == TeuchiUdonType.Long)
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
            else if (type == TeuchiUdonType.ULong)
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
