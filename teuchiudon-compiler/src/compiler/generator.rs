use std::rc::Rc;
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
    .chain(
        context.literal_labels.values().flat_map(|x| routine::decl_data(x.clone(), AsmLiteral::Null)),
    )
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
    match top_stat {
        ast::TopStat::VarBind { parsed: _, access_attr: _, sync_attr: _, var_bind } =>
            visit_var_bind(context, var_bind),
        _ =>
            Box::new([].into_iter()),
    }
}

pub fn visit_var_bind<'context: 'semantics, 'semantics>(
    context: &'context Context,
    var_bind: &'semantics ast::VarBind,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    Box::new(
        visit_expr(context, &var_bind.expr)
        .chain(
            visit_var_decl(context, &var_bind.var_decl),
        )
    )
}

pub fn visit_var_decl<'context: 'semantics, 'semantics>(
    context: &'context Context,
    var_decl: &'semantics ast::VarDecl,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    match var_decl {
        ast::VarDecl::SingleDecl { parsed: _, mut_attr: _, ident: _, ty_expr: _, var } =>
            Box::new(routine::set(get_var_label(context, var))),
        ast::VarDecl::TupleDecl { parsed: _, var_decls } =>
            Box::new(var_decls.iter().rev().flat_map(|x| visit_var_decl(context, x))),
    }
}

pub fn visit_expr<'context: 'semantics, 'semantics>(
    context: &'context Context,
    expr: &'semantics ast::Expr,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    match &expr.detail {
        ast::ExprDetail::Term { parsed: _, term } =>
            visit_term(context, &term),
        ast::ExprDetail::InfixOp { parsed: _, left: _, op: _, right: _ } =>
            Box::new([].into_iter()),
    }
}

pub fn visit_term<'context: 'semantics, 'semantics>(
    context: &'context Context,
    term: &'semantics ast::Term,
) -> Box<dyn Iterator<Item = Instruction> + 'semantics> {
    match &term.detail {
        ast::TermDetail::Literal { parsed: _, literal } =>
            Box::new(routine::get(get_literal_label(context, literal))),
        _ =>
            Box::new([].into_iter()),
    }
}

fn get_literal_label(context: &Context, literal: &Rc<Literal>) -> Rc<DataLabel> {
    context.literal_labels.get(literal).unwrap().clone()
}

fn get_var_label(context: &Context, var: &Rc<Var>) -> Rc<DataLabel> {
    context.var_labels.get(var).unwrap().clone()
}
