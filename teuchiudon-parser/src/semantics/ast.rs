use std::rc::Rc;
use crate::context::Context;
use crate::lexer;
use crate::parser;
use super::elements;

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
pub enum TopStat<'parsed> {
    VarBind {
        parsed: Option<&'parsed parser::ast::TopStat<'parsed>>,
        access_attr: AccessAttr<'parsed>,
        sync_attr: SyncAttr<'parsed>,
        var_bind: VarBind<'parsed>,
    },
    FnBind {
        parsed: Option<&'parsed parser::ast::TopStat<'parsed>>,
        access_attr: AccessAttr<'parsed>,
        fn_bind: FnBind<'parsed>,
    },
    Stat {
        parsed: Option<&'parsed parser::ast::TopStat<'parsed>>,
        stat: Stat<'parsed>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub enum AccessAttr<'parsed> {
    None,
    Pub {
        parsed: Option<&'parsed parser::ast::AccessAttr<'parsed>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub enum SyncAttr<'parsed> {
    None,
    Sync {
        parsed: Option<&'parsed parser::ast::SyncAttr<'parsed>>,
    },
    Linear {
        parsed: Option<&'parsed parser::ast::SyncAttr<'parsed>>,
    },
    Smooth {
        parsed: Option<&'parsed parser::ast::SyncAttr<'parsed>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarBind<'parsed> {
    pub parsed: Option<&'parsed parser::ast::VarBind<'parsed>>,
    pub var_decl: VarDecl<'parsed>,
    pub expr: Rc<Expr<'parsed>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum VarDecl<'parsed> {
    SingleDecl {
        parsed: Option<&'parsed parser::ast::VarDecl<'parsed>>,
        mut_attr: MutAttr<'parsed>,
        ident: Ident<'parsed>,
        type_expr: Rc<TypeExpr<'parsed>>,
        var: Rc<elements::var::Var>,
    },
    TupleDecl {
        parsed: Option<&'parsed parser::ast::VarDecl<'parsed>>,
        var_decls: Vec<VarDecl<'parsed>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub enum MutAttr<'parsed> {
    None,
    Mut {
        parsed: Option<&'parsed parser::ast::MutAttr<'parsed>>,
    },
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
    pub type_expr: Rc<TypeExpr<'parsed>>
}

#[derive(Clone, Debug, PartialEq)]
pub struct TypeExpr<'parsed> {
    pub detail: TypeExprDetail<'parsed>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TypeExprDetail<'parsed> {
    Term {
        parsed: Option<&'parsed parser::ast::TypeExpr<'parsed>>,
        term: Rc<TypeTerm<'parsed>>,
    },
    InfixOp {
        parsed: Option<&'parsed parser::ast::TypeExpr<'parsed>>,
        left: Rc<TypeExpr<'parsed>>,
        op: TypeOp,
        right: Rc<TypeExpr<'parsed>>,
    },
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum TypeOp {
    Access,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TypeTerm<'parsed> {
    pub detail: TypeTermDetail<'parsed>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TypeTermDetail<'parsed> {
    EvalType {
        parsed: Option<&'parsed parser::ast::TypeTerm<'parsed>>,
        ident: Ident<'parsed>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct StatsBlock<'parsed> {
    pub parsed: Option<&'parsed parser::ast::StatsBlock<'parsed>>,
    pub stats: Vec<Stat<'parsed>>,
    pub ret: Rc<Expr<'parsed>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum Stat<'parsed> {
    Return {
        parsed: Option<&'parsed parser::ast::Stat<'parsed>>,
        expr: Rc<Expr<'parsed>>,
    },
    Continue {
        parsed: Option<&'parsed parser::ast::Stat<'parsed>>,
    },
    Break {
        parsed: Option<&'parsed parser::ast::Stat<'parsed>>,
    },
    VarBind {
        parsed: Option<&'parsed parser::ast::Stat<'parsed>>,
        var_bind: VarBind<'parsed>,
    },
    FnBind {
        parsed: Option<&'parsed parser::ast::Stat<'parsed>>,
        fn_bind: FnBind<'parsed>,
    },
    Expr {
        parsed: Option<&'parsed parser::ast::Stat<'parsed>>,
        expr: Rc<Expr<'parsed>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct Expr<'parsed> {
    pub detail: ExprDetail<'parsed>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ExprDetail<'parsed> {
    Term {
        parsed: Option<&'parsed parser::ast::Expr<'parsed>>,
        term: Rc<Term<'parsed>>,
    },
    InfixOp {
        parsed: Option<&'parsed parser::ast::Expr<'parsed>>,
        left: Rc<Expr<'parsed>>,
        op: Op,
        right: Rc<Expr<'parsed>>,
    },
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum Op {
    TypeAccess,
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
    pub detail: TermDetail<'parsed>,
    pub ty: Rc<elements::ty::Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TermDetail<'parsed> {
    TypeExpr {
        parsed: Option<&'parsed parser::ast::TypeExpr<'parsed>>,
        type_expr: Rc<TypeExpr<'parsed>>,
    },
    ApplyFn {
        parsed: Option<&'parsed Vec<parser::ast::ArgExpr<'parsed>>>,
        args: Vec<ArgExpr<'parsed>>,
    },
    ApplySpreadFn {
        parsed: Option<&'parsed parser::ast::Expr<'parsed>>,
        arg: Rc<Expr<'parsed>>,
    },
    ApplyKey {
        parsed: Option<&'parsed parser::ast::Expr<'parsed>>,
        key: Rc<Expr<'parsed>>,
    },
    PrefixOp {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        op: PrefixOp,
        term: Rc<Term<'parsed>>,
    },
    Block {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        stats: StatsBlock<'parsed>,
    },
    Paren {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        expr: Rc<Expr<'parsed>>,
    },
    Tuple {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        exprs: Vec<Rc<Expr<'parsed>>>,
    },
    ArrayCtor {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        iter_expr: IterExpr<'parsed>,
    },
    Literal {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        literal: Rc<elements::literal::Literal>,
    },
    ThisLiteral {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        literal: ThisLiteral<'parsed>,
    },
    InterpolatedString {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        interpolated_string: InterpolatedString<'parsed>,
    },
    EvalVar {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        ident: Ident<'parsed>,
    },
    LetInBind {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        var_bind: VarBind<'parsed>,
        expr: Rc<Expr<'parsed>>,
    },
    If {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        condition: Rc<Expr<'parsed>>,
        if_part: StatsBlock<'parsed>,
        else_part: Option<StatsBlock<'parsed>>,
    },
    While {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        condition: Rc<Expr<'parsed>>,
        stats: StatsBlock<'parsed>,
    },
    Loop {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        stats: StatsBlock<'parsed>,
    },
    For {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
        for_binds: Vec<ForBind<'parsed>>,
        stats: StatsBlock<'parsed>,
    },
    Closure {
        parsed: Option<&'parsed parser::ast::Term<'parsed>>,
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
pub enum IterExpr<'parsed> {
    Empty,
    Range {
        parsed: Option<&'parsed parser::ast::IterExpr<'parsed>>,
        left: Rc<Expr<'parsed>>,
        right: Rc<Expr<'parsed>>,
    },
    SteppedRange {
        parsed: Option<&'parsed parser::ast::IterExpr<'parsed>>,
        left: Rc<Expr<'parsed>>,
        right: Rc<Expr<'parsed>>,
        step: Rc<Expr<'parsed>>,
    },
    Spread {
        parsed: Option<&'parsed parser::ast::IterExpr<'parsed>>,
        expr: Rc<Expr<'parsed>>,
    },
    Elements {
        parsed: Option<&'parsed parser::ast::IterExpr<'parsed>>,
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
pub enum ForBind<'parsed> {
    Let {
        parsed: Option<&'parsed parser::ast::ForBind<'parsed>>,
        var_decl: VarDecl<'parsed>,
        for_iter_expr: ForIterExpr<'parsed>,
    },
    Assign {
        parsed: Option<&'parsed parser::ast::ForBind<'parsed>>,
        left: Rc<Expr<'parsed>>,
        for_iter_expr: ForIterExpr<'parsed>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub enum ForIterExpr<'parsed> {
    Range {
        parsed: Option<&'parsed parser::ast::ForIterExpr<'parsed>>,
        left: Rc<Expr<'parsed>>,
        right: Rc<Expr<'parsed>>,
    },
    SteppedRange {
        parsed: Option<&'parsed parser::ast::ForIterExpr<'parsed>>,
        left: Rc<Expr<'parsed>>,
        right: Rc<Expr<'parsed>>,
        step: Rc<Expr<'parsed>>,
    },
    Spread {
        parsed: Option<&'parsed parser::ast::ForIterExpr<'parsed>>,
        expr: Rc<Expr<'parsed>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct Ident<'parsed> {
    pub parsed: Option<&'parsed lexer::ast::Ident<'parsed>>,
    pub name: String,
}

#[derive(Clone, Debug, PartialEq)]
pub struct ThisLiteral<'parsed> {
    pub parsed: Option<&'parsed lexer::ast::Literal<'parsed>>,
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
        parsed: &'parsed ParserExpr,
        left: Rc<Self>,
        op: SemanticOp,
        right: Rc<Self>,
    ) -> Rc<Self>;
}

impl<'parsed> ExprTree<'parsed, TypeOp, parser::ast::TypeExpr<'parsed>> for TypeExpr<'parsed> {
    fn priorities(context: &Context) -> &Vec<(Box<dyn Fn(&TypeOp) -> bool>, Assoc)> {
        &context.semantic_type_op.priorities
    }

    fn infix_op(
        parsed: &'parsed parser::ast::TypeExpr,
        left: Rc<Self>,
        op: TypeOp,
        right: Rc<Self>,
    ) -> Rc<Self> {
        Rc::new(Self {
            detail: TypeExprDetail::InfixOp {
                parsed: Some(parsed),
                left: left.clone(),
                op,
                right,
            },
            ty: left.ty.clone(),
        })
    }
}

impl<'parsed> ExprTree<'parsed, Op, parser::ast::Expr<'parsed>> for Expr<'parsed> {
    fn priorities(context: &Context) -> &Vec<(Box<dyn Fn(&Op) -> bool>, Assoc)> {
        &context.semantic_op.priorities
    }

    fn infix_op(
        parsed: &'parsed parser::ast::Expr,
        left: Rc<Self>,
        op: Op,
        right: Rc<Self>,
    ) -> Rc<Self> {
        Rc::new(Self {
            detail: ExprDetail::InfixOp {
                parsed: Some(parsed),
                left: left.clone(),
                op,
                right,
            },
            ty: left.ty.clone(), // TODO
        })
    }
}
