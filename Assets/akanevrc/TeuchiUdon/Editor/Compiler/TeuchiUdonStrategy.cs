using System;
using System.Collections.Generic;

namespace akanevrc.TeuchiUdon.Editor.Compiler
{
    public abstract class TeuchiUdonStrategy
    {
        public static TeuchiUdonStrategy Instance { get; private set; }

        public static void SetStrategy<T>() where T : TeuchiUdonStrategy, new()
        {
            Instance = new T();
        }

        public abstract IEnumerable<TeuchiUdonAssembly> GetDataPartFromTables(TeuchiUdonTables tables);
        public abstract IEnumerable<TeuchiUdonAssembly> GetCodePartFromTables(TeuchiUdonTables tables);

        public abstract IEnumerable<TeuchiUdonAssembly> GetDataPartFromBody(BodyResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> GetCodePartFromBody(BodyResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitTopBind(TopBindResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitTopExpr(TopExprResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitInitVarAttr(InitVarAttrResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitExportVarAttr(ExportVarAttrResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitSyncVarAttr(SyncVarAttrResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitInitExprAttr(InitExprAttrResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitVarBind(VarBindResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitVarDecl(VarDeclResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitIdentifier(IdentifierResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitJump(JumpResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitLetBind(LetBindResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitExpr(ExprResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitBottom(BottomResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitUnknownType(UnknownTypeResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitUnit(UnitResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitBlock(BlockResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitParen(ParenResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitLiteral(LiteralResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitThis(ThisResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitEvalVar(EvalVarResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitEvalType(EvalTypeResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitEvalQualifier(EvalQualifierResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitEvalFunc(EvalFuncResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitEvalMethod(EvalMethodResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitEvalQualifierCandidate(EvalQualifierCandidateResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitEvalMethodCandidate(EvalMethodCandidateResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitPrefix(PrefixResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitPostfix(PostfixResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitInfix(InfixResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitLetInBind(LetInBindResult result);
        public abstract IEnumerable<TeuchiUdonAssembly> VisitFunc(FuncResult result);

        protected IEnumerable<TeuchiUdonAssembly> Visit(TeuchiUdonParserResult result)
        {
                 if (result is BodyResult                   body                  ) return GetCodePartFromBody        (body);
            else if (result is TopBindResult                topBind               ) return VisitTopBind               (topBind);
            else if (result is TopExprResult                topExpr               ) return VisitTopExpr               (topExpr);
            else if (result is InitVarAttrResult            initVarAttr           ) return VisitInitVarAttr           (initVarAttr);
            else if (result is ExportVarAttrResult          exportVarAttr         ) return VisitExportVarAttr         (exportVarAttr);
            else if (result is SyncVarAttrResult            syncVarAttr           ) return VisitSyncVarAttr           (syncVarAttr);
            else if (result is InitExprAttrResult           initExprAttr          ) return VisitInitExprAttr          (initExprAttr);
            else if (result is VarBindResult                varBind               ) return VisitVarBind               (varBind);
            else if (result is VarDeclResult                varDecl               ) return VisitVarDecl               (varDecl);
            else if (result is IdentifierResult             identifier            ) return VisitIdentifier            (identifier);
            else if (result is JumpResult                   jump                  ) return VisitJump                  (jump);
            else if (result is LetBindResult                letBind               ) return VisitLetBind               (letBind);
            else if (result is ExprResult                   expr                  ) return VisitExpr                  (expr);
            else if (result is BottomResult                 bottom                ) return VisitBottom                (bottom);
            else if (result is UnknownTypeResult            unknownType           ) return VisitUnknownType           (unknownType);
            else if (result is UnitResult                   unit                  ) return VisitUnit                  (unit);
            else if (result is BlockResult                  block                 ) return VisitBlock                 (block);
            else if (result is ParenResult                  paren                 ) return VisitParen                 (paren);
            else if (result is LiteralResult                literal               ) return VisitLiteral               (literal);
            else if (result is ThisResult                   ths                   ) return VisitThis                  (ths);
            else if (result is EvalVarResult                evalVar               ) return VisitEvalVar               (evalVar);
            else if (result is EvalTypeResult               evalType              ) return VisitEvalType              (evalType);
            else if (result is EvalQualifierResult          evalQualifier         ) return VisitEvalQualifier         (evalQualifier);
            else if (result is EvalFuncResult               evalFunc              ) return VisitEvalFunc              (evalFunc);
            else if (result is EvalMethodResult             evalMethod            ) return VisitEvalMethod            (evalMethod);
            else if (result is EvalQualifierCandidateResult evalQualifierCandidate) return VisitEvalQualifierCandidate(evalQualifierCandidate);
            else if (result is EvalMethodCandidateResult    evalMethodCandidate   ) return VisitEvalMethodCandidate   (evalMethodCandidate);
            else if (result is PrefixResult                 prefix                ) return VisitPrefix                (prefix);
            else if (result is PostfixResult                postfix               ) return VisitPostfix               (postfix);
            else if (result is InfixResult                  infix                 ) return VisitInfix                 (infix);
            else if (result is LetInBindResult              letInBind             ) return VisitLetInBind             (letInBind);
            else if (result is FuncResult                   func                  ) return VisitFunc                  (func);
            else throw new InvalidOperationException("unsupported parser result type");
        }
    }
}
