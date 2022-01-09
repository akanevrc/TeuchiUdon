using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using UnityEngine;
using VRC.Udon.Editor;
using VRC.Udon.Graph;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    using static TeuchiUdonParser;

    public class TeuchiUdonListener : TeuchiUdonParserBaseListener
    {
        private TeuchiUdonParser Parser { get; }
        private TeuchiUdonLogicalErrorHandler LogicalErrorHandler { get; }
        private Dictionary<string, VarBindResult> vars { get; } = new Dictionary<string, VarBindResult>();
        private List<FuncResult> funcs { get; } = new List<FuncResult>();
        private List<LiteralResult> literals { get; } = new List<LiteralResult>();

        public TeuchiUdonListener(TeuchiUdonParser parser, TeuchiUdonLogicalErrorHandler logicalErrorHandler)
        {
            Parser = parser;
            LogicalErrorHandler = logicalErrorHandler;
        }

        public override void ExitTarget([NotNull] TargetContext context)
        {
            var typeToUdon    = new Dictionary<string, string>();
            var methodToUdon  = new Dictionary<string, Dictionary<string, Dictionary<int, List<UdonNodeDefinition>>>>();
            var topRegistries = UdonEditorManager.Instance.GetTopRegistries();
            foreach (var topReg in topRegistries)
            {
                foreach (var reg in topReg.Value)
                {
                    foreach (var def in reg.Value.GetNodeDefinitions())
                    {
                        if (def.type == null) continue;

                        var typeFullName     = def.type.FullName;
                        var splittedFullName = def.fullName.Split(new string[] { "." }, StringSplitOptions.None);

                        if (splittedFullName.Length == 2)
                        {
                            var udonTypeName   = splittedFullName[0];
                            var splittedMethod = splittedFullName[1].Split(new string[] { "__" }, StringSplitOptions.None);

                            if (!typeToUdon.ContainsKey(typeFullName))
                            {
                                typeToUdon.Add(typeFullName, udonTypeName);
                            }

                            if (splittedMethod.Length == 4)
                            {
                                if (!methodToUdon.ContainsKey(typeFullName))
                                {
                                    methodToUdon.Add(typeFullName, new Dictionary<string, Dictionary<int, List<UdonNodeDefinition>>>());
                                }

                                var methodToUdonType = methodToUdon[typeFullName];
                                var methodName       = splittedMethod[1];

                                if (!methodToUdonType.ContainsKey(methodName))
                                {
                                    methodToUdonType.Add(methodName, new Dictionary<int, List<UdonNodeDefinition>>());
                                }

                                var methodToUdonMethod = methodToUdonType[methodName];
                                var methodArgs         = splittedMethod[2].Split(new string[] { "_" }, StringSplitOptions.None);
                                var methodArgCount     = methodArgs.Length == 1 && methodArgs[0] == "SystemVoid" ? 0 : methodArgs.Length;

                                if (!methodToUdonMethod.ContainsKey(methodArgCount))
                                {
                                    methodToUdonMethod.Add(methodArgCount, new List<UdonNodeDefinition>());
                                }
                                
                                methodToUdonMethod[methodArgCount].Add(def);
                            }
                        }
                    }
                }
            }

            Parser.Output.WriteLine(".data_start");

            for (var i = 0; i < literals.Count; i++)
            {
                if (literals[i].Value is int)
                {
                    Parser.Output.WriteLine($"literal[{i}]: %SystemInt32, {literals[i].Text}");
                }
                else if (literals[i].Value is string)
                {
                    Parser.Output.WriteLine($"literal[{i}]: %SystemString, {literals[i].Text}");
                }
            }

            Parser.Output.WriteLine(".data_end");

            Parser.Output.WriteLine(".code_start");

            foreach (var (id, v) in vars.Select(x => (x.Key, x.Value)))
            {
                if (v.Expr.Inner is FuncResult func)
                {
                    Parser.Output.WriteLine($".export {id}");
                    Parser.Output.WriteLine($"{id}:");
                    
                    if (func.Expr.Inner is InfixResult lastInfix && lastInfix.Op == "." && lastInfix.Expr2.Inner is EvalFuncResult evalFunc)
                    {
                        var expr    = lastInfix.Expr1;
                        var typeIds = new Stack<string>();

                        while (expr.Inner is InfixResult infix && infix.Op == "." && infix.Expr2.Inner is EvalVarResult evalVar)
                        {
                            expr = infix.Expr1;
                            typeIds.Push(evalVar.Identifier.Name);
                        }

                        if (expr.Inner is EvalVarResult firstEvalVar)
                        {
                            typeIds.Push(firstEvalVar.Identifier.Name);
                            var typeId   = string.Join(".", typeIds);
                            var methodId = evalFunc.Identifier.Name;
                            var argCount = evalFunc.Args.Length;

                            if
                            (
                                methodToUdon.ContainsKey(typeId) &&
                                methodToUdon[typeId].ContainsKey(evalFunc.Identifier.Name) &&
                                methodToUdon[typeId][methodId].ContainsKey(argCount)
                            )
                            {
                                var defs = methodToUdon[typeId][methodId][argCount];

                                if (defs.Count == 1)
                                {
                                    foreach (var arg in evalFunc.Args)
                                    {
                                        Parser.Output.WriteLine($"PUSH, literal[{((LiteralResult)arg.Inner).Address}]");
                                    }
                                    Parser.Output.WriteLine($"EXTERN, \"{defs[0].fullName}\"");
                                }
                                else
                                {
                                    LogicalErrorHandler.ReportError(v.Expr.Token, $"{typeId}.{methodId}({argCount} args) conflicts");
                                }
                            }
                            else
                            {
                                LogicalErrorHandler.ReportError(v.Expr.Token, $"{typeId}.{methodId}({argCount} args) cannot be found");
                            }
                        }
                        else
                        {
                            LogicalErrorHandler.ReportError(expr.Inner.Token, $"{expr.Inner.GetType().Name} is not supported");
                        }
                    }
                    else
                    {
                        LogicalErrorHandler.ReportError(v.Expr.Inner.Token, $"{v.Expr.Inner.GetType().Name} is not supported");
                    }

                    Parser.Output.WriteLine($"JUMP, 0xFFFFFC");
                }
            }

            Parser.Output.WriteLine(".code_end");
        }

        public override void ExitBody([NotNull] BodyContext context)
        {
            // foreach (var (id, result) in vars.Select(x => (x.Key, x.Value)))
            // {
            //     Debug.Log($"{id} = {Eval(result.Expr)}");
            // }
        }

        public override void ExitTopBind([NotNull] TopBindContext context)
        {
        }

        public override void ExitVarBind([NotNull] VarBindContext context)
        {
            var varDecl    = context.varDecl().result;
            var expr       = context.expr   ().result;
            context.result = new VarBindResult(varDecl.Token, varDecl.SingleDecl.Identifier, expr, vars);
        }

        public override void ExitUnitVarDecl([NotNull] UnitVarDeclContext context)
        {
            var tupleDecl  = new TupleDeclResult(context.OPEN_PARENS().Symbol, new VarDeclResult[0]);
            context.result = new VarDeclResult(tupleDecl.Token, tupleDecl);
        }

        public override void ExitTupleVarDecl([NotNull] TupleVarDeclContext context)
        {
            var varDecls   = context.varDecl().Select(vd => vd.result);
            var tupleDecl  = new TupleDeclResult(context.OPEN_PARENS().Symbol, varDecls);
            context.result = new VarDeclResult(tupleDecl.Token, tupleDecl);
        }

        public override void ExitSingleVarDecl([NotNull] SingleVarDeclContext context)
        {
            var identifier = context.identifier() .result;
            var type       = context.qualified ()?.result;
            var token      = context.OPEN_PARENS()?.Symbol ?? identifier.Token;
            var singleDecl = new SingleDeclResult(token, identifier, type);
            context.result = new VarDeclResult(token, singleDecl);
        }

        public override void ExitQualified([NotNull] QualifiedContext context)
        {
            var identifiers = context.identifier().Select(id => id.result);
            context.result  = new QualifiedResult(identifiers.First().Token, identifiers);
        }

        public override void ExitIdentifier([NotNull] IdentifierContext context)
        {
            var name       = context.GetText().Replace("@", "");
            context.result = new IdentifierResult(context.IDENTIFIER().Symbol, name);
        }

        public override void ExitParensExpr([NotNull] ParensExprContext context)
        {
            var parens     = new ParensResult(context.OPEN_PARENS().Symbol, context.expr().result);
            context.result = new ExprResult(parens.Token, parens);
        }

        public override void ExitLiteralExpr([NotNull] LiteralExprContext context)
        {
            context.result = new ExprResult(context.literal().result.Token, context.literal().result);
        }

        public override void ExitEvalVarExpr([NotNull] EvalVarExprContext context)
        {
            var evalVar    = new EvalVarResult(context.identifier().result.Token, context.identifier().result);
            context.result = new ExprResult(evalVar.Token, evalVar);
        }

        public override void ExitEvalUnitFuncExpr([NotNull] EvalUnitFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = new ExprResult[0];
            var evalFunc   = new EvalFuncResult(identifier.Token, identifier, args);
            context.result = new ExprResult(evalFunc.Token, evalFunc);
        }

        public override void ExitEvalSingleFuncExpr([NotNull] EvalSingleFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = new ExprResult[] { context.expr().result };
            var evalFunc   = new EvalFuncResult(identifier.Token, identifier, args);
            context.result = new ExprResult(evalFunc.Token, evalFunc);
        }

        public override void ExitEvalTupleFuncExpr([NotNull] EvalTupleFuncExprContext context)
        {
            var identifier = context.identifier().result;
            var args       = context.expr().Select(e => e.result);
            var evalFunc   = new EvalFuncResult(identifier.Token, identifier, args);
            context.result = new ExprResult(evalFunc.Token, evalFunc);
        }

        public override void ExitAccessExpr([NotNull] AccessExprContext context)
        {
            var op         = ".";
            var expr1      = context.expr()[0].result;
            var expr2      = context.expr()[1].result;
            var infix      = new InfixResult(expr1.Token, op, expr1, expr2);
            context.result = new ExprResult(infix.Token, infix);
        }

        public override void ExitFuncExpr([NotNull] FuncExprContext context)
        {
            var varDecl = context.varDecl().result;
            var expr    = context.expr   ().result;
            var func    = new FuncResult(varDecl.Token, varDecl, expr, funcs);
            context.result = new ExprResult(func.Token, func);
        }

        public override void ExitIntegerLiteral([NotNull] IntegerLiteralContext context)
        {
            var text = context.GetText().Replace("_", "").ToLower();

            var index = 0;
            var count = text.Length;
            var basis = 10;
            var type  = typeof(int);

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = typeof(ulong);
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = typeof(long);
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = typeof(uint);
            }

            if (count >= 2 && text.StartsWith("0"))
            {
                index++;
                count--;
                basis = 8;
            }

            var token      = context.INTEGER_LITERAL().Symbol;
            var value      = ToIntegerValue(text.Substring(index, count), basis, type, token);
            context.result = new LiteralResult(token, value, text, literals);
        }

        public override void ExitHexIntegerLiteral([NotNull] HexIntegerLiteralContext context)
        {
            var text = context.GetText().Replace("_", "").ToLower();

            var index = 2;
            var count = text.Length - 2;
            var basis = 16;
            var type  = typeof(int);

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = typeof(ulong);
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = typeof(long);
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = typeof(uint);
            }

            var token      = context.HEX_INTEGER_LITERAL().Symbol;
            var value      = ToIntegerValue(text.Substring(index, count), basis, type, token);
            context.result = new LiteralResult(token, value, text, literals);
        }

        public override void ExitBinIntegerLiteral([NotNull] BinIntegerLiteralContext context)
        {
            var text = context.GetText().Replace("_", "").ToLower();

            var index = 2;
            var count = text.Length - 2;
            var basis = 2;
            var type  = typeof(int);

            if (text.EndsWith("ul") || text.EndsWith("lu"))
            {
                count -= 2;
                type   = typeof(ulong);
            }
            else if (text.EndsWith("l"))
            {
                count--;
                type = typeof(long);
            }
            else if (text.EndsWith("u"))
            {
                count--;
                type = typeof(uint);
            }

            var token      = context.BIN_INTEGER_LITERAL().Symbol;
            var value      = ToIntegerValue(text.Substring(index, count), basis, type, token);
            context.result = new LiteralResult(token, value, text, literals);
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
            var value      = ToRegularStringValue(text.Substring(1, text.Length - 2));
            context.result = new LiteralResult(token, value, text, literals);
        }

        public override void ExitVervatiumString([NotNull] VervatiumStringContext context)
        {
            var text       = context.GetText();
            var token      = context.VERBATIUM_STRING().Symbol;
            var value      = ToVervatiumStringValue(text.Substring(2, text.Length - 3));
            context.result = new LiteralResult(token, value, text, literals);
        }

        private object Eval(ExprResult expr)
        {
            return
                expr.Inner is LiteralResult  literal  ? $"<literal>{literal.Value}" :
                expr.Inner is EvalVarResult  evalVar  ? $"<evalVar>{evalVar.Identifier.Name}" :
                expr.Inner is EvalFuncResult evalFunc ? $"<evalFunc>{evalFunc.Identifier.Name}({string.Join(", ", evalFunc.Args.Select(a => Eval(a)))})" :
                expr.Inner is InfixResult    infix    ? $"<infix>{Eval(infix.Expr1)}{(infix.Op == "." ? infix.Op : $" {infix.Op} ")}{Eval(infix.Expr2)}" :
                expr.Inner is FuncResult     func     ?
                    func.VarDecl.SingleDecl == null ?
                        $"<func>({string.Join(", ", func.VarDecl.TupleDecl.Decls.Select(x => x.SingleDecl.Identifier.Name))}) -> {Eval(func.Expr)}" :
                        $"<func>{func.VarDecl.SingleDecl.Identifier.Name} -> {Eval(func.Expr)}" :
                "<expr>";
        }

        private object ToIntegerValue(string text, int basis, Type type, IToken token)
        {
            if (type == typeof(int))
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
            else if (type == typeof(uint))
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
            else if (type == typeof(long))
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
            else if (type == typeof(ulong))
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
