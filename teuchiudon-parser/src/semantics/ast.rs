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
pub struct Target<'parsed> {
    pub parsed: Option<&'parsed parser::ast::Target<'parsed>>,
    pub body: Body<'parsed>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Body<'parsed> {
    pub parsed: Option<&'parsed parser::ast::Body<'parsed>>,
    pub top_stats: Vec<TopStat<'parsed>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TopStat<'parsed> {
    pub parsed: Option<&'parsed parser::ast::TopStat<'parsed>>,
    pub detail: TopStatDetail<'parsed>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TopStatDetail<'parsed> {
    VarBind {
        access_attr: AccessAttr<'parsed>,
        sync_attr: SyncAttr<'parsed>,
        var_bind: VarBind<'parsed>,
    },
    FnBind {
        access_attr: AccessAttr<'parsed>,
        fn_bind: FnBind<'parsed>,
    },
    Stat {
        stat: Stat<'parsed>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct AccessAttr<'parsed> {
    pub parsed: Option<&'parsed parser::ast::AccessAttr<'parsed>>,
    pub detail: AccessAttrDetail,
}

#[derive(Clone, Debug, PartialEq)]
pub enum AccessAttrDetail {
    None,
    Pub,
}

#[derive(Clone, Debug, PartialEq)]
pub struct SyncAttr<'parsed> {
    pub parsed: Option<&'parsed parser::ast::SyncAttr<'parsed>>,
    pub detail: SyncAttrDetail,
}

#[derive(Clone, Debug, PartialEq)]
pub enum SyncAttrDetail {
    None,
    Sync,
    Linear,
    Smooth,
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarBind<'parsed> {
    pub parsed: Option<&'parsed parser::ast::VarBind<'parsed>>,
    pub var_decl: VarDecl<'parsed>,
    pub expr: Rc<Expr<'parsed>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarDecl<'parsed> {
    pub parsed: Option<&'parsed parser::ast::VarDecl<'parsed>>,
    pub detail: VarDeclDetail<'parsed>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum VarDeclDetail<'parsed> {
    SingleDecl {
        mut_attr: MutAttr<'parsed>,
        ident: Ident<'parsed>,
        ty_expr: Rc<TyExpr<'parsed>>,
        var: Rc<elements::var::Var>,
    },
    TupleDecl {
        var_decls: Vec<VarDecl<'parsed>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct MutAttr<'parsed> {
    pub parsed: Option<&'parsed parser::ast::MutAttr<'parsed>>,
    pub detail: MutAttrDetail,
}

#[derive(Clone, Debug, PartialEq)]
pub enum MutAttrDetail {
    None,
    Mut,
}

#[derive(Clone, Debug, PartialEq)]
pub struct FnBind<'parsed> {
    pub parsed: Option<&'parsed parser::ast::FnBind<'parsed>>,
    pub fn_decl: FnDecl<'parsed>,
    pub stats_block: StatsBlock<'parsed>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct FnDecl<'parsed> {
    pub parsed: Option<&'parsed parser::ast::FnDecl<'parsed>>,
    pub ident: Ident<'parsed>,
    pub var_decl: VarDecl<'parsed>,
    pub ty_expr: Rc<TyExpr<'parsed>>
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyExpr<'parsed> {
    pub parsed: Option<&'parsed parser::ast::TyExpr<'parsed>>,
    pub detail: TyExprDetail<'parsed>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyExprDetail<'parsed> {
    Term {
        term: Rc<TyTerm<'parsed>>,
    },
    InfixOp {
        left: Rc<TyExpr<'parsed>>,
        op: TyOp,
        right: Rc<TyExpr<'parsed>>,
    },
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum TyOp {
    Access,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyTerm<'parsed> {
    pub parsed: Option<&'parsed parser::ast::TyTerm<'parsed>>,
    pub detail: TyTermDetail<'parsed>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyTermDetail<'parsed> {
    None,
    EvalTy {
        ident: Ident<'parsed>,
        var: RefCell<Option<Rc<elements::var::Var>>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct StatsBlock<'parsed> {
    pub parsed: Option<&'parsed parser::ast::StatsBlock<'parsed>>,
    pub stats: Vec<Stat<'parsed>>,
    pub ret: Rc<Expr<'parsed>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Stat<'parsed> {
    pub parsed: Option<&'parsed parser::ast::Stat<'parsed>>,
    pub detail: StatDetail<'parsed>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum StatDetail<'parsed> {
    Return {
        expr: Rc<Expr<'parsed>>,
    },
    Continue,
    Break,
    VarBind {
        var_bind: VarBind<'parsed>,
    },
    FnBind {
        fn_bind: FnBind<'parsed>,
    },
    Expr {
        expr: Rc<Expr<'parsed>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct Expr<'parsed> {
    pub parsed: Option<&'parsed parser::ast::Expr<'parsed>>,
    pub detail: ExprDetail<'parsed>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ExprDetail<'parsed> {
    Term {
        term: Rc<Term<'parsed>>,
    },
    InfixOp {
        left: Rc<Expr<'parsed>>,
        op: Op,
        right: Rc<Expr<'parsed>>,
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
pub struct Term<'parsed> {
    pub parsed: Option<&'parsed parser::ast::Term<'parsed>>,
    pub detail: TermDetail<'parsed>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TermDetail<'parsed> {
    None,
    TyExpr {
        ty_expr: Rc<TyExpr<'parsed>>,
    },
    ApplyFn {
        args: Vec<ArgExpr<'parsed>>,
        method: RefCell<Option<Rc<elements::method::Method>>>,
    },
    ApplySpreadFn {
        arg: Rc<Expr<'parsed>>,
    },
    ApplyKey {
        key: Rc<Expr<'parsed>>,
    },
    PrefixOp {
        op: PrefixOp,
        term: Rc<Term<'parsed>>,
    },
    Block {
        stats: StatsBlock<'parsed>,
    },
    Paren {
        expr: Rc<Expr<'parsed>>,
    },
    Tuple {
        exprs: Vec<Rc<Expr<'parsed>>>,
    },
    ArrayCtor {
        iter_expr: IterExpr<'parsed>,
    },
    Literal {
        literal: Rc<elements::literal::Literal>,
    },
    ThisLiteral,
    InterpolatedString {
        interpolated_string: InterpolatedString<'parsed>,
    },
    EvalVar {
        ident: Ident<'parsed>,
        var: RefCell<Option<Rc<elements::var::Var>>>,
    },
    LetInBind {
        var_bind: VarBind<'parsed>,
        expr: Rc<Expr<'parsed>>,
    },
    If {
        condition: Rc<Expr<'parsed>>,
        if_part: StatsBlock<'parsed>,
        else_part: Option<StatsBlock<'parsed>>,
    },
    While {
        condition: Rc<Expr<'parsed>>,
        stats: StatsBlock<'parsed>,
    },
    Loop {
        stats: StatsBlock<'parsed>,
    },
    For {
        for_binds: Vec<ForBind<'parsed>>,
        stats: StatsBlock<'parsed>,
    },
    Closure {
        var_decl: VarDecl<'parsed>,
        expr: Rc<Expr<'parsed>>,
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
pub struct IterExpr<'parsed> {
    pub parsed: Option<&'parsed parser::ast::IterExpr<'parsed>>,
    pub detail: IterExprDetail<'parsed>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum IterExprDetail<'parsed> {
    Empty,
    Range {
        left: Rc<Expr<'parsed>>,
        right: Rc<Expr<'parsed>>,
    },
    SteppedRange {
        left: Rc<Expr<'parsed>>,
        right: Rc<Expr<'parsed>>,
        step: Rc<Expr<'parsed>>,
    },
    Spread {
        expr: Rc<Expr<'parsed>>,
    },
    Elements {
        exprs: Vec<Rc<Expr<'parsed>>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct ArgExpr<'parsed> {
    pub parsed: Option<&'parsed parser::ast::ArgExpr<'parsed>>,
    pub mut_attr: MutAttr<'parsed>,
    pub expr: Rc<Expr<'parsed>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct ForBind<'parsed> {
    pub parsed: Option<&'parsed parser::ast::ForBind<'parsed>>,
    pub detail: ForBindDetail<'parsed>
}

#[derive(Clone, Debug, PartialEq)]
pub enum ForBindDetail<'parsed> {
    Let {
        var_decl: VarDecl<'parsed>,
        for_iter_expr: ForIterExpr<'parsed>,
    },
    Assign {
        left: Rc<Expr<'parsed>>,
        for_iter_expr: ForIterExpr<'parsed>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct ForIterExpr<'parsed> {
    pub parsed: Option<&'parsed parser::ast::ForIterExpr<'parsed>>,
    pub detail: ForIterExprDetail<'parsed>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ForIterExprDetail<'parsed> {
    Range {
        left: Rc<Expr<'parsed>>,
        right: Rc<Expr<'parsed>>,
    },
    SteppedRange {
        left: Rc<Expr<'parsed>>,
        right: Rc<Expr<'parsed>>,
        step: Rc<Expr<'parsed>>,
    },
    Spread {
        expr: Rc<Expr<'parsed>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct Ident<'parsed> {
    pub parsed: Option<&'parsed lexer::ast::Ident<'parsed>>,
    pub name: String,
}

#[derive(Clone, Debug, PartialEq)]
pub struct InterpolatedString<'parsed> {
    pub parsed: Option<&'parsed lexer::ast::InterpolatedString<'parsed>>,
    pub string_parts: Vec<String>,
    pub exprs: Vec<Rc<Expr<'parsed>>>,
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum Assoc {
    Left,
    Right,
}

pub trait ExprTree<'parsed, SemanticOp, ParserExpr> {
    fn priorities(context: &Context) -> &Vec<(Box<dyn Fn(&SemanticOp) -> bool>, Assoc)>;

    fn infix_op(
        context: &Context,
        parsed: &'parsed ParserExpr,
        left: Rc<Self>,
        op: SemanticOp,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'parsed>>>;
}
