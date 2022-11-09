use std::rc::Rc;
use crate::lexer;

#[derive(Clone, Debug, PartialEq)]
pub struct Target<'input> {
    pub slice: &'input str,
    pub body: Option<Body<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Body<'input> {
    pub slice: &'input str,
    pub top_stats: Vec<TopStat<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TopStat<'input> {
    pub slice: &'input str,
    pub kind: TopStatKind<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TopStatKind<'input> {
    VarBind {
        access_attr: Option<AccessAttr<'input>>,
        sync_attr: Option<SyncAttr<'input>>,
        var_bind: VarBind<'input>,
    },
    FnBind {
        access_attr: Option<AccessAttr<'input>>,
        fn_bind: FnBind<'input>,
    },
    Stat {
        stat: Stat<'input>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct AccessAttr<'input> {
    pub slice: &'input str,
    pub attr: lexer::ast::Keyword<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct SyncAttr<'input> {
    pub slice: &'input str,
    pub attr: lexer::ast::Keyword<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarBind<'input> {
    pub slice: &'input str,
    pub let_keyword: lexer::ast::Keyword<'input>,
    pub var_decl: VarDecl<'input>,
    pub expr: Rc<Expr<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarDecl<'input> {
    pub slice: &'input str,
    pub kind: VarDeclKind<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum VarDeclKind<'input> {
    SingleDecl {
        mut_attr: Option<MutAttr<'input>>,
        ident: lexer::ast::Ident<'input>,
        ty_expr: Option<Rc<TyExpr<'input>>>,
    },
    TupleDecl {
        var_decls: Vec<VarDecl<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct MutAttr<'input> {
    pub slice: &'input str,
    pub attr: lexer::ast::Keyword<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct FnBind<'input> {
    pub slice: &'input str,
    pub fn_keyword: lexer::ast::Keyword<'input>,
    pub fn_decl: FnDecl<'input>,
    pub stats_block: StatsBlock<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct FnDecl<'input> {
    pub slice: &'input str,
    pub ident: lexer::ast::Ident<'input>,
    pub var_decl: VarDecl<'input>,
    pub ty_expr: Option<Rc<TyExpr<'input>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyExpr<'input> {
    pub slice: &'input str,
    pub ty_term: Rc<TyTerm<'input>>,
    pub ty_ops: Vec<TyOp<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyOp<'input> {
    pub slice: &'input str,
    pub kind: TyOpKind<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyOpKind<'input> {
    Access {
        op_code: lexer::ast::OpCode<'input>,
        ty_term: Rc<TyTerm<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyTerm<'input> {
    pub slice: &'input str,
    pub kind: TyTermKind<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyTermKind<'input> {
    EvalTy {
        ident: lexer::ast::Ident<'input>
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct StatsBlock<'input> {
    pub slice: &'input str,
    pub stats: Vec<Stat<'input>>,
    pub ret: Option<Rc<Expr<'input>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Stat<'input> {
    pub slice: &'input str,
    pub kind: StatKind<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum StatKind<'input> {
    Return {
        return_keyword: lexer::ast::Keyword<'input>,
        expr: Option<Rc<Expr<'input>>>,
    },
    Continue {
        continue_keyword: lexer::ast::Keyword<'input>,
    },
    Break {
        break_keyword: lexer::ast::Keyword<'input>,
    },
    VarBind {
        var_bind: VarBind<'input>,
    },
    FnBind {
        fn_bind: FnBind<'input>,
    },
    Expr {
        expr: Rc<Expr<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct Expr<'input> {
    pub slice: &'input str,
    pub term: Rc<Term<'input>>,
    pub ops: Vec<Op<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Op<'input> {
    pub slice: &'input str,
    pub kind: OpKind<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum OpKind<'input> {
    TyAccess {
        op_code: lexer::ast::OpCode<'input>,
        term: Rc<Term<'input>>,
    },
    Access {
        op_code: lexer::ast::OpCode<'input>,
        term: Rc<Term<'input>>,
    },
    EvalFn {
        arg_exprs: Vec<ArgExpr<'input>>,
    },
    EvalSpreadFn {
        expr: Rc<Expr<'input>>,
    },
    EvalKey {
        expr: Rc<Expr<'input>>,
    },
    CastOp {
        as_keyword: lexer::ast::Keyword<'input>,
        ty_expr: Rc<TyExpr<'input>>,
    },
    InfixOp {
        op_code: lexer::ast::OpCode<'input>,
        term: Rc<Term<'input>>,
    },
    Assign {
        term: Rc<Term<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct Term<'input> {
    pub slice: &'input str,
    pub kind: TermKind<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TermKind<'input> {
    PrefixOp {
        op_code: lexer::ast::OpCode<'input>,
        term: Rc<Term<'input>>,
    },
    Block {
        stats: StatsBlock<'input>,
    },
    Paren {
        expr: Rc<Expr<'input>>,
    },
    Tuple {
        exprs: Vec<Rc<Expr<'input>>>,
    },
    ArrayCtor {
        iter_expr: Option<IterExpr<'input>>,
    },
    Literal {
        literal: lexer::ast::Literal<'input>,
    },
    ThisLiteral {
        literal: lexer::ast::Literal<'input>,
    },
    InterpolatedString {
        interpolated_string: lexer::ast::InterpolatedString<'input>,
    },
    EvalVar {
        ident: lexer::ast::Ident<'input>,
    },
    LetInBind {
        var_bind: VarBind<'input>,
        in_keyword: lexer::ast::Keyword<'input>,
        expr: Rc<Expr<'input>>,
    },
    If {
        if_keyword: lexer::ast::Keyword<'input>,
        condition: Rc<Expr<'input>>,
        if_part: StatsBlock<'input>,
        else_part: Option<(lexer::ast::Keyword<'input>, StatsBlock<'input>)>,
    },
    While {
        while_keyword: lexer::ast::Keyword<'input>,
        condition: Rc<Expr<'input>>,
        stats: StatsBlock<'input>,
    },
    Loop {
        loop_keyword: lexer::ast::Keyword<'input>,
        stats: StatsBlock<'input>,
    },
    For {
        for_binds: Vec<(lexer::ast::Keyword<'input>, ForBind<'input>)>,
        stats: StatsBlock<'input>,
    },
    Closure {
        var_decl: VarDecl<'input>,
        expr: Rc<Expr<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct IterExpr<'input> {
    pub slice: &'input str,
    pub kind: IterExprKind<'input>,
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
    pub mut_attr: Option<MutAttr<'input>>,
    pub expr: Rc<Expr<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct ForBind<'input> {
    pub slice: &'input str,
    pub kind: ForBindKind<'input>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ForBindKind<'input> {
    Let {
        let_keyword: lexer::ast::Keyword<'input>,
        var_decl: VarDecl<'input>,
        for_iter_expr: ForIterExpr<'input>,
    },
    Assign {
        left: Rc<Expr<'input>>,
        for_iter_expr: ForIterExpr<'input>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct ForIterExpr<'input> {
    pub slice: &'input str,
    pub kind: ForIterExprKind<'input>,
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
