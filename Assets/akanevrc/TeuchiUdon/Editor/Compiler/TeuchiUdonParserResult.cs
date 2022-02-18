using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public abstract class TeuchiUdonParserResult
    {
        public IToken Token { get; }

        public TeuchiUdonParserResult(IToken token)
        {
            Token = token;
        }
    }

    public class BodyResult : TeuchiUdonParserResult
    {
        public TopStatementResult[] TopStatements { get; }

        public BodyResult(IToken token, IEnumerable<TopStatementResult> topStatements)
            : base(token)
        {
            TopStatements = topStatements.ToArray();
        }
    }

    public abstract class TopStatementResult : TeuchiUdonParserResult
    {
        public TopStatementResult(IToken token)
            : base(token)
        {
        }
    }

    public class TopBindResult : TopStatementResult
    {
        public VarBindResult VarBind { get; }
        public string Init { get; }
        public bool Export { get; }
        public TeuchiUdonSyncMode Sync { get; }

        public TopBindResult(IToken token, VarBindResult varBind, string init, bool export, TeuchiUdonSyncMode sync)
            : base(token)
        {
            VarBind = varBind;
            Init    = init;
            Export  = export;
            Sync    = sync;
        }
    }

    public class TopExprResult : TopStatementResult
    {
        public ExprResult Expr { get; }
        public string Init { get; }

        public TopExprResult(IToken token, ExprResult expr, string init)
            : base(token)
        {
            Expr = expr;
            Init = init;
        }
    }

    public abstract class VarAttrResult : TeuchiUdonParserResult
    {
        public VarAttrResult(IToken token)
            : base(token)
        {
        }
    }

    public class InitVarAttrResult : VarAttrResult
    {
        public IdentifierResult Identifier { get; }

        public InitVarAttrResult(IToken token, IdentifierResult identifier)
            : base(token)
        {
            Identifier = identifier;
        }
    }

    public class ExportVarAttrResult : VarAttrResult
    {
        public ExportVarAttrResult(IToken token)
            : base(token)
        {
        }
    }

    public class SyncVarAttrResult : VarAttrResult
    {
        public TeuchiUdonSyncMode Mode { get; }

        public SyncVarAttrResult(IToken token, TeuchiUdonSyncMode mode)
            : base(token)
        {
            Mode = mode;
        }
    }

    public abstract class ExprAttrResult : TeuchiUdonParserResult
    {
        public ExprAttrResult(IToken token)
            : base(token)
        {
        }
    }

    public class InitExprAttrResult : ExprAttrResult
    {
        public IdentifierResult Identifier { get; }

        public InitExprAttrResult(IToken token, IdentifierResult identifier)
            : base(token)
        {
            Identifier = identifier;
        }
    }

    public class VarBindResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVarBind VarBind { get; }
        public TeuchiUdonVar[] Vars { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public VarBindResult(IToken token, int index, TeuchiUdonQualifier qualifier, IEnumerable<TeuchiUdonVar> vars, VarDeclResult varDecl, ExprResult expr)
            : base(token)
        {
            VarBind = new TeuchiUdonVarBind(index, qualifier, vars.Select(x => x.Name));
            Vars    = vars.ToArray();
            VarDecl = varDecl;
            Expr    = expr;

            foreach (var v in Vars)
            {
                TeuchiUdonTables.Instance.Vars[v] = v;
            }
        }
    }

    public class VarDeclResult : TeuchiUdonParserResult
    {
        public TeuchiUdonVar[] Vars { get; }
        public TeuchiUdonType[] Types { get; }
        public IdentifierResult[] Identifiers { get; }
        public ExprResult[] Qualifieds { get; }

        public VarDeclResult(IToken token, TeuchiUdonQualifier qualifier, IEnumerable<IdentifierResult> identifiers, IEnumerable<ExprResult> qualifieds)
            : base(token)
        {
            var index   = TeuchiUdonTables.Instance.GetVarIndex();
            Types       = qualifieds .Select(x => x.Inner.Type.GetArgAsType()).ToArray();
            Vars        = identifiers.Zip(Types, (i, t) => (i, t)).Select(x => new TeuchiUdonVar(index, qualifier, x.i.Name, x.t)).ToArray();
            Identifiers = identifiers.ToArray();
            Qualifieds  = qualifieds .ToArray();

            TeuchiUdonTables.Instance.SetExpectCount(Qualifieds.Length);

            foreach (var v in Vars)
            {
                if (TeuchiUdonTables.Instance.Vars.ContainsKey(v))
                {
                    TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"{v.Name} conflicts with another variable");
                }
                else
                {
                    TeuchiUdonTables.Instance.Vars.Add(v, v);
                }
            }
        }
    }

    public class IdentifierResult : TeuchiUdonParserResult
    {
        public string Name { get; }

        public IdentifierResult(IToken token, string name)
            : base(token)
        {
            Name = name;
        }
    }

    public abstract class StatementResult : TeuchiUdonParserResult
    {
        public StatementResult(IToken token)
            : base(token)
        {
        }
    }

    public class JumpResult : StatementResult
    {
        public ExprResult Value { get; }
        public IIndexedLabel Label { get; }

        public JumpResult(IToken token, ExprResult value, IIndexedLabel label)
            : base(token)
        {
            Value = value;
            Label = label;
        }
    }

    public class LetBindResult : StatementResult
    {
        public VarBindResult VarBind { get; }

        public LetBindResult(IToken token, VarBindResult varBind)
            : base(token)
        {
            VarBind = varBind;
        }
    }

    public class ExprResult : StatementResult
    {
        public TypedResult Inner { get; }
        public bool ReturnsValue { get; set; } = true;

        public ExprResult(IToken token, TypedResult inner)
            : base(token)
        {
            Inner = inner;
        }
    }

    public abstract class TypedResult : TeuchiUdonParserResult
    {
        public TeuchiUdonType Type { get; }

        public TypedResult(IToken token, TeuchiUdonType type)
            : base(token)
        {
            Type = type;
        }

        public abstract TeuchiUdonVar[] LeftValues { get; }
    }

    public class BottomResult : TypedResult
    {
        public BottomResult(IToken token)
            : base(token, TeuchiUdonType.Bottom)
        {
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class UnknownTypeResult : TypedResult
    {
        public UnknownTypeResult(IToken token)
            : base(token, TeuchiUdonType.Type.ApplyArgAsType(TeuchiUdonType.Unknown))
        {
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class UnitResult : TypedResult
    {
        public UnitResult(IToken token)
            : base(token, TeuchiUdonType.Unit)
        {
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class BlockResult : TypedResult
    {
        public TeuchiUdonBlock Block { get; }
        public StatementResult[] Statements { get; }
        public ExprResult Expr { get; }

        public BlockResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, IEnumerable<StatementResult> statements, ExprResult expr)
            : base(token, type)
        {
            Block      = new TeuchiUdonBlock(index, qualifier);
            Statements = statements.ToArray();
            Expr       = expr;
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class ParenResult : TypedResult
    {
        public ExprResult Expr { get; }

        public ParenResult(IToken token, TeuchiUdonType type, ExprResult expr)
            : base(token, type)
        {
            Expr = expr;
        }

        public override TeuchiUdonVar[] LeftValues => Expr.Inner.LeftValues;
    }

    public class LiteralResult : TypedResult
    {
        public TeuchiUdonLiteral Literal { get; }

        public LiteralResult(IToken token, TeuchiUdonType type, int index, string text, object value)
            : base(token, type)
        {
            Literal = new TeuchiUdonLiteral(index, text, type, value);

            if (TeuchiUdonTables.Instance.Literals.ContainsKey(Literal))
            {
                Literal = TeuchiUdonTables.Instance.Literals[Literal];
            }
            else
            {
                TeuchiUdonTables.Instance.Literals.Add(Literal, Literal);
            }
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class ThisResult : TypedResult
    {
        public TeuchiUdonThis This { get; }

        public ThisResult(IToken token)
            : base(token, TeuchiUdonType.GameObject)
        {
            This = new TeuchiUdonThis();

            if (TeuchiUdonTables.Instance.This.ContainsKey(This))
            {
                This = TeuchiUdonTables.Instance.This[This];
            }
            else
            {
                TeuchiUdonTables.Instance.This.Add(This, This);
            }
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class EvalVarResult : TypedResult
    {
        public TeuchiUdonVar Var { get; }
        public IdentifierResult Identifier { get; }

        public EvalVarResult(IToken token, TeuchiUdonType type, TeuchiUdonVar v, IdentifierResult identifier)
            : base(token, type)
        {
            Var        = v;
            Identifier = identifier;
        }

        public override TeuchiUdonVar[] LeftValues => new TeuchiUdonVar[] { Var };
    }

    public class EvalTypeResult : TypedResult
    {
        public TeuchiUdonType InnerType { get; }
        public IdentifierResult Identifier { get; }

        public EvalTypeResult(IToken token, TeuchiUdonType type, TeuchiUdonType innerType, IdentifierResult identifier)
            : base(token, type)
        {
            InnerType  = innerType;
            Identifier = identifier;
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class EvalQualifierResult : TypedResult
    {
        public TeuchiUdonQualifier Qualifier { get; }
        public IdentifierResult Identifier { get; }

        public EvalQualifierResult(IToken token, TeuchiUdonType type, TeuchiUdonQualifier qualifier, IdentifierResult identifier)
            : base(token, type)
        {
            Qualifier  = qualifier;
            Identifier = identifier;
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class EvalGetterResult : TypedResult
    {
        public TeuchiUdonMethod Method { get; }
        public IdentifierResult Identifier { get; }

        public EvalGetterResult(IToken token, TeuchiUdonType type, TeuchiUdonMethod method, IdentifierResult identifier)
            : base(token, type)
        {
            Method     = method;
            Identifier = identifier;
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class EvalGetterSetterResult : TypedResult
    {
        public TeuchiUdonMethod Getter { get; }
        public TeuchiUdonMethod Setter { get; }
        public IdentifierResult Identifier { get; }

        public EvalGetterSetterResult(IToken token, TeuchiUdonType type, TeuchiUdonMethod getter, TeuchiUdonMethod setter, IdentifierResult identifier)
            : base(token, type)
        {
            Getter     = getter;
            Setter     = setter;
            Identifier = identifier;
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class EvalFuncResult : TypedResult
    {
        public TeuchiUdonEvalFunc EvalFunc { get; }
        public TeuchiUdonOutValue OutValue { get; }
        public ExprResult Expr { get; }
        public ExprResult[] Args { get; }

        public EvalFuncResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, ExprResult expr, IEnumerable<ExprResult> args)
            : base(token, type)
        {
            EvalFunc = new TeuchiUdonEvalFunc(index, qualifier);
            OutValue = TeuchiUdonTables.Instance.GetOutValues(1).First();
            Expr     = expr;
            Args     = args.ToArray();
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class EvalMethodResult : TypedResult
    {
        public TeuchiUdonMethod Method { get; }
        public ExprResult Expr { get; }
        public ExprResult[] Args { get; }
        public TeuchiUdonOutValue[] OutValues { get; }

        public EvalMethodResult(IToken token, TeuchiUdonType type, TeuchiUdonMethod method, ExprResult expr, IEnumerable<ExprResult> args)
            : base(token, type)
        {
            Method    = method;
            Expr      = expr;
            Args      = args.ToArray();
            OutValues = TeuchiUdonTables.Instance.GetOutValues(method.OutTypes.Length).ToArray();

            TeuchiUdonTables.Instance.SetExpectCount(method.InTypes.Length);
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class EvalVarCandidateResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public EvalVarCandidateResult(IToken token, IdentifierResult identifier)
            : base(token, TeuchiUdonType.Unknown)
        {
            Identifier = identifier;
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public abstract class UnaryOpResult : TypedResult
    {
        public string Op { get; }
        public ExprResult Expr { get; }
        public TeuchiUdonMethod[] Methods { get; }
        public TeuchiUdonOutValue[][] OutValuess { get; }
        public TeuchiUdonLiteral[] Literals { get; }

        public UnaryOpResult(IToken token, TeuchiUdonType type, string op, ExprResult expr)
            : base(token, type)
        {
            Op         = op;
            Expr       = expr;
            Methods    = GetMethods   ().ToArray();
            OutValuess = GetOutValuess().Select(x => x.ToArray()).ToArray();
            Literals   = GetLiterals  ().ToArray();

            TeuchiUdonTables.Instance.SetExpectCount(Methods.Length == 0 ? 0 : Methods.Max(x => x.InTypes.Length));
        }

        protected abstract IEnumerable<TeuchiUdonMethod> GetMethods();
        protected abstract IEnumerable<IEnumerable<TeuchiUdonOutValue>> GetOutValuess();
        protected abstract IEnumerable<TeuchiUdonLiteral> GetLiterals();

        protected TeuchiUdonType GetTypeFromLogicalName(string logicalName, bool isTypeType)
        {
            var qt = new TeuchiUdonType(logicalName);
            if (!TeuchiUdonTables.Instance.LogicalTypes.ContainsKey(qt))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"type '{logicalName}' is not defined");
                return TeuchiUdonType.Bottom;
            }

            var type = TeuchiUdonTables.Instance.LogicalTypes[qt];
            return isTypeType ? TeuchiUdonType.Type.ApplyArgAsType(type) : type;
        }

        protected TeuchiUdonMethod GetMethodFromName(string logicalTypeName, bool isTypeType, string methodName, params string[] inTypeNames)
        {
            var type    = GetTypeFromLogicalName(logicalTypeName, isTypeType);
            var inTypes = inTypeNames.Select(x => GetTypeFromLogicalName(x, false)).ToArray();
            var qm      = new TeuchiUdonMethod(type, methodName, inTypes);
            var m       = TeuchiUdonTables.Instance.GetMostCompatibleMethods(qm).ToArray();
            if (m.Length == 0)
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"operator '{Op}' is not defined");
                return null;
            }
            else if (m.Length == 1)
            {
                return m[0];
            }
            else
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(Token, $"arguments of operator '{Op}' is nondeterministic");
                return null;
            }
        }
    }

    public class PrefixResult : UnaryOpResult
    {
        public PrefixResult(IToken token, TeuchiUdonType type, string op, ExprResult expr)
            : base(token, type, op, expr)
        {
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];

        protected override IEnumerable<TeuchiUdonMethod> GetMethods()
        {
            var exprType = Expr.Inner.Type.LogicalName;

            switch (Op)
            {
                case "+":
                    return new TeuchiUdonMethod[0];
                case "-":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_UnaryMinus"   , exprType) };
                case "!":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_UnaryNegation", exprType) };
                case "~":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_LogicalXor"   , exprType, exprType) };
                case "++":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_Addition"     , exprType, exprType) };
                case "--":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_Subtraction"  , exprType, exprType) };
                default:
                    return new TeuchiUdonMethod[0];
            }
        }

        protected override IEnumerable<IEnumerable<TeuchiUdonOutValue>> GetOutValuess()
        {
            switch (Op)
            {
                case "+":
                    return new TeuchiUdonOutValue[0][];
                case "-":
                case "!":
                case "~":
                case "++":
                case "--":
                    return Methods.Select(x => TeuchiUdonTables.Instance.GetOutValues(x.OutTypes.Length));
                default:
                    return new TeuchiUdonOutValue[0][];
            }
        }

        protected override IEnumerable<TeuchiUdonLiteral> GetLiterals()
        {
            switch (Op)
            {
                case "+":
                case "-":
                case "!":
                    return new TeuchiUdonLiteral[0];
                case "~":
                    if (Methods[0] == null)
                    {
                        return new TeuchiUdonLiteral[] { null };
                    }
                    else
                    {
                        var index = TeuchiUdonTables.Instance.GetLiteralIndex();
                        return new TeuchiUdonLiteral[] { TeuchiUdonLiteral.CreateMask(index, Expr.Inner.Type) };
                    }
                case "++":
                case "--":
                    if (Methods[0] == null)
                    {
                        return new TeuchiUdonLiteral[] { null };
                    }
                    else
                    {
                        var index = TeuchiUdonTables.Instance.GetLiteralIndex();
                        return new TeuchiUdonLiteral[] { TeuchiUdonLiteral.CreateNumber(index, "1", Expr.Inner.Type) };
                    }
                default:
                    return new TeuchiUdonLiteral[0];
            }
        }
    }

    public class PostfixResult : UnaryOpResult
    {
        public TeuchiUdonOutValue[][] TmpValuess { get; }

        public PostfixResult(IToken token, TeuchiUdonType type, string op, ExprResult expr)
            : base(token, type, op, expr)
        {
            TmpValuess = Methods.Select(_ => TeuchiUdonTables.Instance.GetOutValues(1).ToArray()).ToArray();
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];

        protected override IEnumerable<TeuchiUdonMethod> GetMethods()
        {
            var exprType = Expr.Inner.Type.LogicalName;

            switch (Op)
            {
                case "++":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_Addition"   , exprType, exprType) };
                case "--":
                    return new TeuchiUdonMethod[] { GetMethodFromName(exprType, true, "op_Subtraction", exprType, exprType) };
                default:
                    return new TeuchiUdonMethod[0];
            }
        }

        protected override IEnumerable<IEnumerable<TeuchiUdonOutValue>> GetOutValuess()
        {
            switch (Op)
            {
                case "++":
                case "--":
                    return new TeuchiUdonOutValue[][] { new TeuchiUdonOutValue[] { TeuchiUdonTables.Instance.GetOutValues(1).First() } };
                default:
                    return new TeuchiUdonOutValue[0][];
            }
        }

        protected override IEnumerable<TeuchiUdonLiteral> GetLiterals()
        {
            switch (Op)
            {
                case "++":
                case "--":
                    if (Methods[0] == null)
                    {
                        return new TeuchiUdonLiteral[] { null };
                    }
                    else
                    {
                        var index = TeuchiUdonTables.Instance.GetLiteralIndex();
                        return new TeuchiUdonLiteral[] { TeuchiUdonLiteral.CreateNumber(index, "1", Expr.Inner.Type) };
                    }
                default:
                    return new TeuchiUdonLiteral[0];
            }
        }

        
    }

    public class InfixResult : TypedResult
    {
        public string Op { get; }
        public ExprResult Expr1 { get; }
        public ExprResult Expr2 { get; }

        public InfixResult(IToken token, TeuchiUdonType type, string op, ExprResult expr1, ExprResult expr2)
            : base(token, type)
        {
            Op    = op;
            Expr1 = expr1;
            Expr2 = expr2;
        }

        public override TeuchiUdonVar[] LeftValues => (Op == "." || Op == "?.") ? Expr2.Inner.LeftValues : new TeuchiUdonVar[0];
    }

    public class LetInBindResult : TypedResult
    {
        public TeuchiUdonLetIn LetIn { get; }
        public VarBindResult VarBind { get; }
        public ExprResult Expr { get; }

        public LetInBindResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, VarBindResult varBind, ExprResult expr)
            : base(token, type)
        {
            LetIn   = new TeuchiUdonLetIn(index, qualifier);
            VarBind = varBind;
            Expr    = expr;
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class FuncResult : TypedResult
    {
        public TeuchiUdonFunc Func { get; }
        public VarDeclResult VarDecl { get; }
        public ExprResult Expr { get; }

        public FuncResult(IToken token, TeuchiUdonType type, int index, TeuchiUdonQualifier qualifier, VarDeclResult varDecl, ExprResult expr)
            : base(token, type)
        {
            Func    = new TeuchiUdonFunc(index, qualifier, type, varDecl.Vars, expr);
            VarDecl = varDecl;
            Expr    = expr;

            TeuchiUdonTables.Instance.SetExpectCount(VarDecl.Vars.Length + 1);

            if (TeuchiUdonTables.Instance.Funcs.ContainsKey(Func))
            {
                TeuchiUdonLogicalErrorHandler.Instance.ReportError(token, $"{Func} conflicts with another function");
            }
            else
            {
                TeuchiUdonTables.Instance.Funcs.Add(Func, Func);
            }
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class MethodResult : TypedResult
    {
        public IdentifierResult Identifier { get; }

        public MethodResult(IToken token, TeuchiUdonType type, IdentifierResult identifier)
            : base(token, type)
        {
            Identifier = identifier;
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }

    public class SetterResult : TypedResult
    {
        public TeuchiUdonMethod Method { get; }
        public IdentifierResult Identifier { get; }

        public SetterResult(IToken token, TeuchiUdonMethod method, IdentifierResult identifier)
            : base(token, TeuchiUdonType.Unknown)
        {
            Method     = method;
            Identifier = identifier;
        }

        public override TeuchiUdonVar[] LeftValues { get; } = new TeuchiUdonVar[0];
    }
}
