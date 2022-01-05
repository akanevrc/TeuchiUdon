using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using UnityEngine;

namespace akanevrc.TeuchiUdon.Editor
{
    using static TeuchiUdonParser;

    public class TeuchiUdonListener : TeuchiUdonParserBaseListener
    {
        private Dictionary<string, VarBindResult> vars { get; } = new Dictionary<string, VarBindResult>();
        private List<FuncResult> funcs { get; } = new List<FuncResult>();
        private List<LiteralResult> literals { get; } = new List<LiteralResult>();
        private TeuchiUdonLogicalErrorHandler LogicalErrorHandler { get; }

        public TeuchiUdonListener(TeuchiUdonLogicalErrorHandler logicalErrorHandler)
        {
            LogicalErrorHandler = logicalErrorHandler;
        }

        public override void ExitBody([NotNull] BodyContext context)
        {
            foreach (var (id, result) in vars.Select(x => (x.Key, x.Value)))
            {
                Debug.Log($"{id} = {Eval(result.Expr)}");
            }
        }

        public override void ExitTopBind([NotNull] TopBindContext context)
        {
        }

        public override void ExitVarBind([NotNull] VarBindContext context)
        {
            var varDecl = context.varDecl().result;
            var expr    = context.expr   ().result;
            context.result = new VarBindResult(varDecl.Identifiers[0], expr, vars);
        }

        public override void ExitSingleVarDecl([NotNull] SingleVarDeclContext context)
        {
            context.result = context.singleDecl().result;
        }

        public override void ExitTupleVarDecl([NotNull] TupleVarDeclContext context)
        {
            context.result = context.tupleDecl().result;
        }

        public override void ExitSingleDecl([NotNull] SingleDeclContext context)
        {
            var identifier = context.identifier()[0].result;
            context.result = new VarDeclResult(new IdentifierResult[] { identifier });
        }

        public override void ExitTupleDecl([NotNull] TupleDeclContext context)
        {
            var identifiers = context.varDecl().SelectMany(vd => vd.result.Identifiers);
            context.result = new VarDeclResult(identifiers);
        }

        public override void ExitIdentifier([NotNull] IdentifierContext context)
        {
            var text = context.GetText().Replace("@", "");

            var token = context.IDENTIFIER().Symbol;
            context.result = new IdentifierResult(token, text);
        }

        public override void ExitEvalVarExpr([NotNull] EvalVarExprContext context)
        {
            context.result = new ExprResult(context.evalVar().result);
        }

        public override void ExitEvalFuncExpr([NotNull] EvalFuncExprContext context)
        {
            context.result = new ExprResult(context.evalFunc().result);
        }

        public override void ExitFuncExpr([NotNull] FuncExprContext context)
        {
            context.result = new ExprResult(context.func().result);
        }

        public override void ExitLiteralExpr([NotNull] LiteralExprContext context)
        {
            context.result = new ExprResult(context.literal().result);
        }

        public override void ExitEvalVar([NotNull] EvalVarContext context)
        {
            context.result = new EvalVarResult(context.identifier().result);
        }

        public override void ExitEvalFunc([NotNull] EvalFuncContext context)
        {
            var identifier = context.identifier().result;
            var expr       = context.expr      ().result;
            context.result = new EvalFuncResult(identifier, expr);
        }

        public override void ExitFunc([NotNull] FuncContext context)
        {
            var varDecl = context.varDecl().result;
            var expr    = context.expr   ().result;
            context.result = new FuncResult(varDecl, expr, funcs);
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
            var literal    = ToInteger(text.Substring(index, count), basis, type, token);
            context.result = new LiteralResult(token, literal, literals);
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
            var literal    = ToInteger(text.Substring(index, count), basis, type, token);
            context.result = new LiteralResult(token, literal, literals);
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
            var literal    = ToInteger(text.Substring(index, count), basis, type, token);
            context.result = new LiteralResult(token, literal, literals);
        }

        public override void ExitRealLiteral([NotNull] RealLiteralContext context)
        {
        }

        public override void ExitCharacterLiteral([NotNull] CharacterLiteralContext context)
        {
        }

        public override void ExitRegularString([NotNull] RegularStringContext context)
        {
        }

        public override void ExitVervatiumString([NotNull] VervatiumStringContext context)
        {
        }

        private object Eval(ExprResult expr)
        {
            return
                expr.Inner is EvalVarResult  evalVar  ? evalVar.Identifier.Identifier :
                expr.Inner is EvalFuncResult evalFunc ? $"{evalFunc.Identifier.Identifier} {Eval(evalFunc.Expr)}" :
                expr.Inner is FuncResult     func     ? $"{func.VarDecl.Identifiers[0].Identifier} -> {Eval(func.Expr)}" :
                expr.Inner is LiteralResult  literal  ? literal.Literal :
                "<expr>";
        }

        private object ToInteger(string text, int basis, Type type, IToken token)
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
    }
}
