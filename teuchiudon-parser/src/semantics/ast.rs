use std::{
    cell::RefCell,
    rc::Rc,
};
use crate::context::Context;
use crate::lexer;
use crate::parser;
use super::{
    SemanticError,
    elements,
};

#[derive(Clone, Debug, PartialEq)]
pub struct Target<'input> {
    pub parsed: Option<Rc<parser::ast::Target<'input>>>,
    pub body: Rc<Body<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Body<'input> {
    pub parsed: Option<Rc<parser::ast::Body<'input>>>,
    pub top_stats: Vec<Rc<TopStat<'input>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TopStat<'input> {
    pub parsed: Option<Rc<parser::ast::TopStat<'input>>>,
    pub detail: Rc<TopStatDetail<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TopStatDetail<'input> {
    VarBind {
        access_attr: Rc<AccessAttr<'input>>,
        sync_attr: Rc<SyncAttr<'input>>,
        var_bind: Rc<VarBind<'input>>,
    },
    FnBind {
        access_attr: Rc<AccessAttr<'input>>,
        fn_bind: Rc<FnBind<'input>>,
        ev: Option<(Rc<elements::ev::Ev>, Rc<elements::ev_stats::EvStats<'input>>)>,
    },
    Stat {
        stat: Rc<Stat<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct AccessAttr<'input> {
    pub parsed: Option<Rc<parser::ast::AccessAttr<'input>>>,
    pub detail: AccessAttrDetail,
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum AccessAttrDetail {
    None,
    Pub,
}

#[derive(Clone, Debug, PartialEq)]
pub struct SyncAttr<'input> {
    pub parsed: Option<Rc<parser::ast::SyncAttr<'input>>>,
    pub detail: SyncAttrDetail,
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum SyncAttrDetail {
    None,
    Sync,
    Linear,
    Smooth,
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarBind<'input> {
    pub parsed: Option<Rc<parser::ast::VarBind<'input>>>,
    pub var_decl: Rc<VarDecl<'input>>,
    pub expr: Rc<Expr<'input>>,
    pub vars: Vec<Rc<elements::var::Var>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarDecl<'input> {
    pub parsed: Option<Rc<parser::ast::VarDecl<'input>>>,
    pub detail: Rc<VarDeclDetail<'input>>,
    pub ty: Rc<elements::ty::Ty>,
    pub vars: Vec<Rc<elements::var::Var>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum VarDeclDetail<'input> {
    SingleDecl {
        mut_attr: Rc<MutAttr<'input>>,
        ident: Rc<Ident<'input>>,
        ty_expr: Rc<TyExpr<'input>>,
        var: Rc<elements::var::Var>,
    },
    TupleDecl {
        var_decls: Vec<Rc<VarDecl<'input>>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct MutAttr<'input> {
    pub parsed: Option<Rc<parser::ast::MutAttr<'input>>>,
    pub detail: MutAttrDetail,
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum MutAttrDetail {
    None,
    Mut,
}

#[derive(Clone, Debug, PartialEq)]
pub struct FnBind<'input> {
    pub parsed: Option<Rc<parser::ast::FnBind<'input>>>,
    pub fn_decl: Rc<FnDecl<'input>>,
    pub stats_block: Rc<StatsBlock<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct FnDecl<'input> {
    pub parsed: Option<Rc<parser::ast::FnDecl<'input>>>,
    pub ident: Rc<Ident<'input>>,
    pub var_decl: Rc<VarDecl<'input>>,
    pub ty_expr: Rc<TyExpr<'input>>
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyExpr<'input> {
    pub parsed: Option<Rc<parser::ast::TyExpr<'input>>>,
    pub detail: Rc<TyExprDetail<'input>>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyExprDetail<'input> {
    Term {
        term: Rc<TyTerm<'input>>,
    },
    InfixOp {
        left: Rc<TyExpr<'input>>,
        op: TyOp,
        right: Rc<TyExpr<'input>>,
    },
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum TyOp {
    Access,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyTerm<'input> {
    pub parsed: Option<Rc<parser::ast::TyTerm<'input>>>,
    pub detail: Rc<TyTermDetail<'input>>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyTermDetail<'input> {
    None,
    EvalTy {
        ident: Rc<Ident<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct StatsBlock<'input> {
    pub parsed: Option<Rc<parser::ast::StatsBlock<'input>>>,
    pub stats: Vec<Rc<Stat<'input>>>,
    pub ret: Rc<Expr<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Stat<'input> {
    pub parsed: Option<Rc<parser::ast::Stat<'input>>>,
    pub detail: Rc<StatDetail<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum StatDetail<'input> {
    Return {
        expr: Rc<Expr<'input>>,
    },
    Continue,
    Break,
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
    pub parsed: Option<Rc<parser::ast::Expr<'input>>>,
    pub detail: Rc<ExprDetail<'input>>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ExprDetail<'input> {
    Term {
        term: Rc<Term<'input>>,
    },
    InfixOp {
        left: Rc<Expr<'input>>,
        op: Op,
        right: Rc<Expr<'input>>,
    },
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum Op {
    TyAccess,
    Access,
    CoalescingAccess,
    EvalFn,
    EvalSpreadFn,
    EvalKey,
    CastOp,
    Mul,
    Div,
    Mod,
    Add,
    Sub,
    LeftShift,
    RightShift,
    Lt,
    Gt,
    Le,
    Ge,
    Eq,
    Ne,
    BitAnd,
    BitXor,
    BitOr,
    And,
    Or,
    Coalescing,
    RightPipeline,
    LeftPipeline,
    Assign,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Term<'input> {
    pub parsed: Option<Rc<parser::ast::Term<'input>>>,
    pub detail: Rc<TermDetail<'input>>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TermDetail<'input> {
    None,
    TyExpr {
        ty_expr: Rc<TyExpr<'input>>,
    },
    ApplyFn {
        args: Vec<Rc<ArgExpr<'input>>>,
        method: RefCell<Option<Rc<AsFn>>>,
    },
    ApplySpreadFn {
        arg: Rc<Expr<'input>>,
    },
    ApplyKey {
        key: Rc<Expr<'input>>,
    },
    PrefixOp {
        op: PrefixOp,
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
        iter_expr: Rc<IterExpr<'input>>,
    },
    Literal {
        literal: Rc<elements::literal::Literal>,
    },
    ThisLiteral,
    InterpolatedString {
        interpolated_string: Rc<InterpolatedString<'input>>,
    },
    EvalVar {
        ident: Rc<Ident<'input>>,
        var: RefCell<Option<Rc<elements::var::Var>>>,
    },
    LetInBind {
        var_bind: Rc<VarBind<'input>>,
        expr: Rc<Expr<'input>>,
    },
    If {
        condition: Rc<Expr<'input>>,
        if_part: Rc<StatsBlock<'input>>,
        else_part: Option<Rc<StatsBlock<'input>>>,
    },
    While {
        condition: Rc<Expr<'input>>,
        stats: Rc<StatsBlock<'input>>,
    },
    Loop {
        stats: Rc<StatsBlock<'input>>,
    },
    For {
        for_binds: Vec<Rc<ForBind<'input>>>,
        stats: Rc<StatsBlock<'input>>,
    },
    Closure {
        var_decl: Rc<VarDecl<'input>>,
        expr: Rc<Expr<'input>>,
    },
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum PrefixOp {
    Plus,
    Minus,
    Bang,
    Tilde,
}

#[derive(Clone, Debug, PartialEq)]
pub enum AsFn {
    Method(Rc<elements::method::Method>),
}

#[derive(Clone, Debug, PartialEq)]
pub struct IterExpr<'input> {
    pub parsed: Option<Rc<parser::ast::IterExpr<'input>>>,
    pub detail: Rc<IterExprDetail<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum IterExprDetail<'input> {
    Empty,
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
    pub parsed: Option<Rc<parser::ast::ArgExpr<'input>>>,
    pub mut_attr: Rc<MutAttr<'input>>,
    pub expr: Rc<Expr<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct ForBind<'input> {
    pub parsed: Option<Rc<parser::ast::ForBind<'input>>>,
    pub detail: Rc<ForBindDetail<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ForBindDetail<'input> {
    Let {
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
    pub parsed: Option<Rc<parser::ast::ForIterExpr<'input>>>,
    pub detail: Rc<ForIterExprDetail<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ForIterExprDetail<'input> {
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
}

#[derive(Clone, Debug, PartialEq)]
pub struct Ident<'input> {
    pub parsed: Option<Rc<lexer::ast::Ident<'input>>>,
    pub name: String,
}

#[derive(Clone, Debug, PartialEq)]
pub struct InterpolatedString<'input> {
    pub parsed: Option<Rc<lexer::ast::InterpolatedString<'input>>>,
    pub string_parts: Vec<String>,
    pub exprs: Vec<Rc<Expr<'input>>>,
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum Assoc {
    Left,
    Right,
}

pub trait ExprTree<'input: 'context, 'context, SemanticOp, ParserExpr> {
    fn priorities(context: &'context Context<'input>) -> &'context Vec<(Box<dyn Fn(&SemanticOp) -> bool>, Assoc)>;

    fn infix_op(
        context: &'context Context<'input>,
        parsed: Rc<ParserExpr>,
        left: Rc<Self>,
        op: SemanticOp,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'input>>>;
}
