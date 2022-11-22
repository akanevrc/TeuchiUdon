use std::rc::Rc;
use crate::lexer;

#[derive(Clone, Debug, PartialEq)]
pub struct Target<'input> {
    pub slice: &'input str,
    pub body: Option<Rc<Body<'input>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Body<'input> {
    pub slice: &'input str,
    pub top_stats: Vec<Rc<TopStat<'input>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TopStat<'input> {
    pub slice: &'input str,
    pub kind: Rc<TopStatKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TopStatKind<'input> {
    VarBind {
        access_attr: Option<Rc<AccessAttr<'input>>>,
        sync_attr: Option<Rc<SyncAttr<'input>>>,
        var_bind: Rc<VarBind<'input>>,
    },
    FnBind {
        access_attr: Option<Rc<AccessAttr<'input>>>,
        fn_bind: Rc<FnBind<'input>>,
    },
    Stat {
        stat: Rc<Stat<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct AccessAttr<'input> {
    pub slice: &'input str,
    pub attr: Rc<lexer::ast::Keyword<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct SyncAttr<'input> {
    pub slice: &'input str,
    pub attr: Rc<lexer::ast::Keyword<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarBind<'input> {
    pub slice: &'input str,
    pub let_keyword: Rc<lexer::ast::Keyword<'input>>,
    pub var_decl: Rc<VarDecl<'input>>,
    pub expr: Rc<Expr<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarDecl<'input> {
    pub slice: &'input str,
    pub kind: Rc<VarDeclKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum VarDeclKind<'input> {
    SingleDecl {
        mut_attr: Option<Rc<MutAttr<'input>>>,
        ident: Rc<lexer::ast::Ident<'input>>,
        ty_expr: Option<Rc<TyExpr<'input>>>,
    },
    TupleDecl {
        var_decls: Vec<Rc<VarDecl<'input>>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct MutAttr<'input> {
    pub slice: &'input str,
    pub attr: Rc<lexer::ast::Keyword<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct FnBind<'input> {
    pub slice: &'input str,
    pub fn_keyword: Rc<lexer::ast::Keyword<'input>>,
    pub fn_decl: Rc<FnDecl<'input>>,
    pub stats_block: Rc<StatsBlock<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct FnDecl<'input> {
    pub slice: &'input str,
    pub ident: Rc<lexer::ast::Ident<'input>>,
    pub var_decl: Rc<VarDecl<'input>>,
    pub ty_expr: Option<Rc<TyExpr<'input>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyExpr<'input> {
    pub slice: &'input str,
    pub ty_term: Rc<TyTerm<'input>>,
    pub ty_ops: Vec<Rc<TyOp<'input>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyOp<'input> {
    pub slice: &'input str,
    pub kind: Rc<TyOpKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyOpKind<'input> {
    Access {
        op_code: Rc<lexer::ast::OpCode<'input>>,
        ty_term: Rc<TyTerm<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyTerm<'input> {
    pub slice: &'input str,
    pub kind: Rc<TyTermKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyTermKind<'input> {
    EvalTy {
        ident: Rc<lexer::ast::Ident<'input>>
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct StatsBlock<'input> {
    pub slice: &'input str,
    pub stats: Vec<Rc<Stat<'input>>>,
    pub ret: Option<Rc<Expr<'input>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Stat<'input> {
    pub slice: &'input str,
    pub kind: Rc<StatKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum StatKind<'input> {
    Return {
        return_keyword: Rc<lexer::ast::Keyword<'input>>,
        expr: Option<Rc<Expr<'input>>>,
    },
    Continue {
        continue_keyword: Rc<lexer::ast::Keyword<'input>>,
    },
    Break {
        break_keyword: Rc<lexer::ast::Keyword<'input>>,
    },
    VarBind {
        var_bind: Rc<VarBind<'input>>,
    },
    FnBind {
        fn_bind: Rc<FnBind<'input>>,
    },
    Expr {
        expr: Rc<Expr<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct Expr<'input> {
    pub slice: &'input str,
    pub term: Rc<Term<'input>>,
    pub ops: Vec<Rc<Op<'input>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Op<'input> {
    pub slice: &'input str,
    pub kind: Rc<OpKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum OpKind<'input> {
    TyAccess {
        op_code: Rc<lexer::ast::OpCode<'input>>,
        term: Rc<Term<'input>>,
    },
    Access {
        op_code: Rc<lexer::ast::OpCode<'input>>,
        term: Rc<Term<'input>>,
    },
    EvalFn {
        arg_exprs: Vec<Rc<ArgExpr<'input>>>,
    },
    EvalSpreadFn {
        expr: Rc<Expr<'input>>,
    },
    EvalKey {
        expr: Rc<Expr<'input>>,
    },
    CastOp {
        as_keyword: Rc<lexer::ast::Keyword<'input>>,
        ty_expr: Rc<TyExpr<'input>>,
    },
    InfixOp {
        op_code: Rc<lexer::ast::OpCode<'input>>,
        term: Rc<Term<'input>>,
    },
    Assign {
        term: Rc<Term<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct Term<'input> {
    pub slice: &'input str,
    pub kind: Rc<TermKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TermKind<'input> {
    PrefixOp {
        op_code: Rc<lexer::ast::OpCode<'input>>,
        term: Rc<Term<'input>>,
    },
    Block {
        stats: Rc<StatsBlock<'input>>,
    },
    Paren {
        expr: Rc<Expr<'input>>,
    },
    Tuple {
        exprs: Vec<Rc<Expr<'input>>>,
    },
    ArrayCtor {
        iter_expr: Option<Rc<IterExpr<'input>>>,
    },
    Literal {
        literal: Rc<lexer::ast::Literal<'input>>,
    },
    ThisLiteral {
        literal: Rc<lexer::ast::Literal<'input>>,
    },
    InterpolatedString {
        interpolated_string: Rc<lexer::ast::InterpolatedString<'input>>,
    },
    EvalVar {
        ident: Rc<lexer::ast::Ident<'input>>,
    },
    LetInBind {
        var_bind: Rc<VarBind<'input>>,
        in_keyword: Rc<lexer::ast::Keyword<'input>>,
        expr: Rc<Expr<'input>>,
    },
    If {
        if_keyword: Rc<lexer::ast::Keyword<'input>>,
        condition: Rc<Expr<'input>>,
        if_part: Rc<StatsBlock<'input>>,
        else_part: Option<(Rc<lexer::ast::Keyword<'input>>, Rc<StatsBlock<'input>>)>,
    },
    While {
        while_keyword: Rc<lexer::ast::Keyword<'input>>,
        condition: Rc<Expr<'input>>,
        stats: Rc<StatsBlock<'input>>,
    },
    Loop {
        loop_keyword: Rc<lexer::ast::Keyword<'input>>,
        stats: Rc<StatsBlock<'input>>,
    },
    For {
        for_binds: Vec<(Rc<lexer::ast::Keyword<'input>>, Rc<ForBind<'input>>)>,
        stats: Rc<StatsBlock<'input>>,
    },
    Closure {
        var_decl: Rc<VarDecl<'input>>,
        expr: Rc<Expr<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct IterExpr<'input> {
    pub slice: &'input str,
    pub kind: Rc<IterExprKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum IterExprKind<'input> {
    Range {
        left: Rc<Expr<'input>>,
        right: Rc<Expr<'input>>,
    },
    SteppedRange {
        left: Rc<Expr<'input>>,
        right: Rc<Expr<'input>>,
        step: Rc<Expr<'input>>,
    },
    Spread {
        expr: Rc<Expr<'input>>,
    },
    Elements {
        exprs: Vec<Rc<Expr<'input>>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct ArgExpr<'input> {
    pub slice: &'input str,
    pub mut_attr: Option<Rc<MutAttr<'input>>>,
    pub expr: Rc<Expr<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct ForBind<'input> {
    pub slice: &'input str,
    pub kind: Rc<ForBindKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ForBindKind<'input> {
    Let {
        let_keyword: Rc<lexer::ast::Keyword<'input>>,
        var_decl: Rc<VarDecl<'input>>,
        for_iter_expr: Rc<ForIterExpr<'input>>,
    },
    Assign {
        left: Rc<Expr<'input>>,
        for_iter_expr: Rc<ForIterExpr<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct ForIterExpr<'input> {
    pub slice: &'input str,
    pub kind: Rc<ForIterExprKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ForIterExprKind<'input> {
    Range {
        left: Rc<Expr<'input>>,
        right: Rc<Expr<'input>>,
    },
    SteppedRange {
        left: Rc<Expr<'input>>,
        right: Rc<Expr<'input>>,
        step: Rc<Expr<'input>>,
    },
    Spread {
        expr: Rc<Expr<'input>>
    },
}
