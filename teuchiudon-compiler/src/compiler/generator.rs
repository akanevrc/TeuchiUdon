use std::{
    cell::RefCell,
    rc::Rc,
};
use teuchiudon_parser::semantics::{
    ast,
    elements::{
        label::DataLabel,
        literal::Literal,
        var::Var,
    },
};
use crate::assembly::{
    AsmLiteral,
    Instruction,
};
use crate::context::Context;
use super::routine;

pub fn generate_data_part(context: &Context) -> impl Iterator<Item = Instruction> + '_ {
    context.var_labels.values().flat_map(|x| routine::decl_data(x.clone(), AsmLiteral::Null))
    .chain(context.literal_labels.values().flat_map(|x| routine::decl_data(x.clone(), AsmLiteral::Null)))
}

pub fn generate_code_part<'context: 'semantics, 'semantics>(
    context: &'context Context,
    target: &'semantics ast::Target,
) -> impl Iterator<Item = Instruction> + 'semantics {
    visit_body(context, &target.body)
}

pub fn visit_body<'context: 'semantics, 'semantics>(
    context: &'context Context,
    body: &'semantics ast::Body,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    Box::new(body.top_stats.iter().flat_map(|x| visit_top_stat(context, x)))
}

pub fn visit_top_stat<'context: 'semantics, 'semantics>(
    context: &'context Context,
    top_stat: &'semantics ast::TopStat,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    match &top_stat.detail {
        ast::TopStatDetail::VarBind { access_attr: _, sync_attr: _, var_bind } =>
            visit_var_bind(context, var_bind),
        ast::TopStatDetail::Stat { stat } =>
            visit_stat(context, stat),
        _ =>
            error("top_stat".to_owned()),
    }
}

pub fn visit_var_bind<'context: 'semantics, 'semantics>(
    context: &'context Context,
    var_bind: &'semantics ast::VarBind,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    Box::new(
        visit_expr(context, &var_bind.expr)
        .chain(visit_var_decl(context, &var_bind.var_decl))
    )
}

pub fn visit_var_decl<'context: 'semantics, 'semantics>(
    context: &'context Context,
    var_decl: &'semantics ast::VarDecl,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    match &var_decl.detail {
        ast::VarDeclDetail::SingleDecl { mut_attr: _, ident: _, ty_expr: _, var } =>
            Box::new(routine::set(var_label(context, var))),
        ast::VarDeclDetail::TupleDecl { var_decls } =>
            Box::new(var_decls.iter().rev().flat_map(|x| visit_var_decl(context, x))),
    }
}

pub fn visit_stat<'context: 'semantics, 'semantics>(
    context: &'context Context,
    stat: &'semantics ast::Stat,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    match &stat.detail {
        ast::StatDetail::Expr { expr } =>
            visit_expr(context, expr),
        _ =>
            error("stat".to_owned())
    }
}

pub fn visit_expr<'context: 'semantics, 'semantics>(
    context: &'context Context,
    expr: &'semantics Rc<ast::Expr>,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    match &expr.detail {
        ast::ExprDetail::Term { term } =>
            visit_term(context, &term),
        ast::ExprDetail::InfixOp { left, op, right } =>
            visit_infix_op(context, left, op, right)
    }
}

pub fn visit_infix_op<'context: 'semantics, 'semantics>(
    context: &'context Context,
    left: &'semantics Rc<ast::Expr>,
    op: &'semantics ast::Op,
    right: &'semantics Rc<ast::Expr>,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    match op {
        ast::Op::TyAccess =>
            visit_ty_access_op(context, left, right),
        ast::Op::EvalFn =>
            visit_eval_fn_op(context, left, right),
        _ =>
            error("infix_op".to_owned()),
    }
}

fn visit_ty_access_op<'context: 'semantics, 'semantics>(
    context: &'context Context,
    _left: &'semantics Rc<ast::Expr>,
    right: &'semantics Rc<ast::Expr>,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    let ast::ExprDetail::Term { term } = &right.detail
        else {
            return error("ty_access_op".to_owned());
        };
    let ast::TermDetail::EvalVar { ident: _, var } = &term.detail
        else {
            return error("ty_access_op".to_owned());
        };
    let Some(var) = &*var.borrow()
        else {
            return error("ty_access_op".to_owned());
        };

    Box::new(routine::get(var_label(context, &var)))
}

fn visit_eval_fn_op<'context: 'semantics, 'semantics>(
    context: &'context Context,
    left: &'semantics Rc<ast::Expr>,
    right: &'semantics Rc<ast::Expr>,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    let ast::ExprDetail::Term { term } = &right.detail
        else {
            return error("eval_fn_op".to_owned());
        };
    let ast::TermDetail::ApplyFn { args, method } = &term.detail
        else {
            return error("eval_fn_op".to_owned());
        };
    let Some(method) = &*method.borrow()
        else {
            return error("eval_fn_op".to_owned());
        };

    let args = args.iter().flat_map(|x| visit_expr(context, &x.expr));
    Box::new(
        visit_expr(context, left)
        .chain(routine::call_method(args, method.real_name.clone()))
    )
}

pub fn visit_term<'context: 'semantics, 'semantics>(
    context: &'context Context,
    term: &'semantics Rc<ast::Term>,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    match &term.detail {
        ast::TermDetail::Literal { literal } =>
            visit_literal_term(context, literal),
        ast::TermDetail::EvalVar { ident: _, var } =>
            visit_eval_var_term(context, var),
        _ =>
            error("term".to_owned())
    }
}

fn visit_literal_term<'context: 'semantics, 'semantics>(
    context: &'context Context,
    literal: &'semantics Rc<Literal>,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    Box::new(routine::get(literal_label(context, literal)))
}

fn visit_eval_var_term<'context: 'semantics, 'semantics>(
    context: &'context Context,
    var: &'semantics RefCell<Option<Rc<Var>>>,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    if let Some(var) = &*var.borrow() {
        Box::new(routine::get(var_label(context, &var)))
    }
    else {
        error("eval_var_term".to_owned())
    }
}

fn error(message: String) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new(routine::comment(format!("Error detected: `{}`", message)))
}

fn literal_label(context: &Context, literal: &Rc<Literal>) -> Rc<DataLabel> {
    context.literal_labels.get(literal).unwrap().clone()
}

fn var_label(context: &Context, var: &Rc<Var>) -> Rc<DataLabel> {
    context.var_labels.get(var).unwrap().clone()
}
