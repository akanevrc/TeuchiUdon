use std::{
    cell::RefCell,
    fmt::Debug,
    rc::Rc,
};
use crate::context::Context;
use crate::lexer;
use crate::parser;
use super::elements::ty::TyLogicalKey;
use super::{
    SemanticError,
    elements::{
        ev::Ev,
        ev_stats::EvStats,
        eval_fn::EvalFn,
        fn_stats::FnStats,
        label::DataLabel,
        literal::Literal,
        method::Method,
        operation::Operation,
        this_literal::ThisLiteral,
        ty::Ty,
        var::Var,
    },
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
    None,
    VarBind {
        access_attr: Rc<AccessAttr<'input>>,
        sync_attr: Rc<SyncAttr<'input>>,
        var_bind: Rc<VarBind<'input>>,
    },
    FnBind {
        access_attr: Rc<AccessAttr<'input>>,
        fn_bind: Rc<FnBind<'input>>,
        ev: Option<(Rc<Ev>, Rc<EvStats<'input>>)>,
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
    pub vars: Vec<Rc<Var>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct VarDecl<'input> {
    pub parsed: Option<Rc<parser::ast::VarDecl<'input>>>,
    pub detail: Rc<VarDeclDetail<'input>>,
    pub ty: Rc<Ty>,
    pub vars: Vec<Rc<Var>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum VarDeclDetail<'input> {
    SingleDecl {
        mut_attr: Rc<MutAttr<'input>>,
        ident: Rc<Ident<'input>>,
        ty_expr: Rc<TyExpr<'input>>,
        var: Rc<Var>,
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
    pub fn_stats: Rc<FnStats<'input>>,
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
    pub ty: Rc<Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyExprDetail<'input> {
    Factor {
        factor: Rc<TyFactor<'input>>,
    },
    InfixOp {
        left: Rc<TyExpr<'input>>,
        op: TyOp,
        right: Rc<TyExpr<'input>>,
    },
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum TyOp {
    Access,
}

#[derive(Clone, Debug, PartialEq)]
pub struct TyFactor<'input> {
    pub parsed: Option<Rc<parser::ast::TyFactor<'input>>>,
    pub detail: Rc<TyFactorDetail<'input>>,
    pub ty: Rc<Ty>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TyFactorDetail<'input> {
    None,
    EvalTy {
        ident: Rc<Ident<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct StatsBlock<'input> {
    pub parsed: Option<Rc<parser::ast::StatsBlock<'input>>>,
    pub stats: Vec<Rc<Stat<'input>>>,
    pub ret: Rc<RetainedExpr<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Stat<'input> {
    pub parsed: Option<Rc<parser::ast::Stat<'input>>>,
    pub detail: Rc<StatDetail<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum StatDetail<'input> {
    Return {
        expr: Rc<RetainedExpr<'input>>,
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
    pub ty: Rc<Ty>,
    pub tmp_vars: Vec<Rc<Var>>,
    pub data: RefCell<Option<Vec<Rc<DataLabel>>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum ExprDetail<'input> {
    Term {
        term: Rc<Term<'input>>,
    },
    PrefixOp {
        op: TermPrefixOp,
        expr: Rc<Expr<'input>>,
        operation: Rc<Operation>,
    },
    InfixOp {
        left: Rc<Expr<'input>>,
        op: TermInfixOp,
        right: Rc<Expr<'input>>,
        operation: Rc<Operation>,
    },
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum TermPrefixOp {
    Plus,
    Minus,
    Bang,
    Tilde,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum TermInfixOp {
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
    pub ty: Rc<Ty>,
    pub tmp_vars: Vec<Rc<Var>>,
    pub data: RefCell<Option<Vec<Rc<DataLabel>>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum TermDetail<'input> {
    Factor {
        factor: Rc<Factor<'input>>,
    },
    InfixOp {
        left: Rc<Term<'input>>,
        op: FactorInfixOp,
        right: Rc<Term<'input>>,
        operation: Rc<Operation>,
        instance: Option<TyLogicalKey>,
    },
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum FactorInfixOp {
    TyAccess,
    Access,
    CoalescingAccess,
    EvalFn,
    EvalSpreadFn,
    EvalKey,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Factor<'input> {
    pub parsed: Option<Rc<parser::ast::Factor<'input>>>,
    pub detail: Rc<FactorDetail<'input>>,
    pub ty: Rc<Ty>,
    pub data: RefCell<Option<Vec<Rc<DataLabel>>>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum FactorDetail<'input> {
    None,
    TyExpr {
        ty_expr: Rc<TyExpr<'input>>,
    },
    ApplyFn {
        args: Vec<Rc<ArgExpr<'input>>>,
        as_fn: RefCell<Option<Rc<AsFn<'input>>>>,
    },
    ApplySpreadFn {
        arg: Rc<Expr<'input>>,
    },
    ApplyKey {
        key: Rc<Expr<'input>>,
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
        literal: Rc<Literal>,
    },
    ThisLiteral {
        literal: Rc<ThisLiteral>,
    },
    InterpolatedString {
        interpolated_string: Rc<InterpolatedString<'input>>,
    },
    EvalVar {
        ident: Rc<Ident<'input>>,
        var: RefCell<Option<Rc<Var>>>,
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

#[derive(Clone, Debug, PartialEq)]
pub enum AsFn<'input> {
    Fn(Rc<EvalFn<'input>>),
    Method(Rc<Method>),
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
    pub expr: Rc<RetainedExpr<'input>>,
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

pub trait ExprTree<'input: 'context, 'context, SemanticOp: Debug, ParserExpr>: Debug {
    fn priorities(context: &'context Context<'input>) -> &'context Vec<(Box<dyn Fn(&SemanticOp) -> bool>, Assoc)>;

    fn infix_op(
        context: &'context Context<'input>,
        parsed: Rc<ParserExpr>,
        left: Rc<Self>,
        op: &SemanticOp,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'input>>>;
}

#[derive(Clone, Debug, PartialEq)]
pub struct RetainedExpr<'input> {
    expr: Rc<Expr<'input>>,
}

impl<'input> RetainedExpr<'input> {
    pub fn new(expr: Rc<Expr<'input>>) -> Self {
        Self {
            expr,
        }
    }

    pub fn release(&self, context: &Context) -> Rc<Expr<'input>> {
        for v in &self.expr.tmp_vars {
            context.release_tmp_var(v.clone());
        }
        self.expr.clone()
    }

    pub fn get(&self) -> Rc<Expr<'input>> {
        self.expr.clone()
    }
}

#[derive(Clone, Debug, PartialEq)]
pub struct RetainedTerm<'input> {
    term: Rc<Term<'input>>,
}

impl<'input> RetainedTerm<'input> {
    pub fn new(term: Rc<Term<'input>>) -> Self {
        Self {
            term,
        }
    }

    pub fn release(&self, context: &Context) -> Rc<Term<'input>> {
        for v in &self.term.tmp_vars {
            context.release_tmp_var(v.clone());
        }
        self.term.clone()
    }

    pub fn get(&self) -> Rc<Term<'input>> {
        self.term.clone()
    }
}

pub fn expr_to_literal(context: &Context, expr: Rc<Expr>) -> Option<Rc<Literal>> {
    expr_to_literal_rec(context, expr, String::new())
}

fn expr_to_literal_rec(context: &Context, expr: Rc<Expr>, mut prefix: String) -> Option<Rc<Literal>> {
    match expr.detail.as_ref() {
        ExprDetail::Term { term } =>
            match term.detail.as_ref() {
                TermDetail::Factor { factor } =>
                    match factor.detail.as_ref() {
                        FactorDetail::Paren { expr } =>
                            expr_to_literal_rec(context, expr.clone(), prefix),
                        FactorDetail::Literal { literal } => {
                            if
                                literal.ty.is_boolean(context) && prefix.chars().all(|x| x == '!' || x == '~') ||
                                literal.ty.is_signed_integer(context) && prefix.chars().all(|x| x == '+' || x == '-' || x == '~') ||
                                literal.ty.is_integer(context) && prefix.chars().all(|x| x == '+' || x == '~') ||
                                literal.ty.is_signed_number(context) && prefix.chars().all(|x| x == '+' || x == '-') ||
                                literal.ty.is_text(context) && prefix.is_empty()
                            {
                                let text = format!("{}{}", prefix, literal.text);
                                Some(Literal::new_or_get(context, text, literal.ty.clone()))
                            }
                            else {
                                None
                            }
                        },
                        _ => None,
                    },
                _ => None,
            },
        ExprDetail::PrefixOp { op, expr, operation: _ } => {
            let op_ch = match op {
                TermPrefixOp::Plus => '+',
                TermPrefixOp::Minus => '-',
                TermPrefixOp::Bang => '!',
                TermPrefixOp::Tilde => '~',
            };
            prefix.insert(0, op_ch);
            expr_to_literal_rec(context, expr.clone(), prefix)
        },
        _ => None,
    }
}
