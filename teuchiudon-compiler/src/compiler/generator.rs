use std::{
    cell::RefCell,
    rc::Rc,
};
use teuchiudon_parser::semantics::{
    ast,
    elements::{
        ev::Ev,
        label::{
            CodeLabel,
            DataLabel,
            ExternLabel,
        },
        literal::Literal,
        method::Method,
        operation::Operation,
        var::Var,
    },
};
use crate::assembly::{
    AsmLiteral,
    Instruction,
};
use crate::context::Context;
use super::routine;

pub fn generate_data_part<'input>(
    context: &Context<'input>
) -> Vec<Instruction> {
    context.valued_vars.keys().flat_map(|x| {
        let v = &context.var_labels[x];
        routine::export_data(v.clone())
        .chain(routine::decl_data(v.clone(), AsmLiteral::Null))
    })
    .chain(
        context.var_labels.iter()
        .filter(|(k, _)| !context.valued_vars.contains_key(*k))
        .flat_map(|(_, v)| {
            routine::decl_data(v.clone(), AsmLiteral::Null)
        }
    ))
    .chain(context.literal_labels.values().flat_map(|x|
        routine::decl_data(x.clone(), AsmLiteral::Null)
    ))
    .collect()
}

pub fn generate_code_part<'input>(
    context: &Context<'input>,
) -> Vec<Vec<Instruction>> {
    if context.ev_stats.iter().find(|(ev, _)| ev.real_name == "_start").is_none() {
        let top_stats =
            Box::new(
                context.top_stats.iter().flat_map(|x| visit_top_stat(context, x.stat.clone()))
            );
        vec![routine::decl_start_ev(CodeLabel::from_name("_start"), top_stats, empty()).collect()]
    }
    else {
        Vec::new()
    }
    .into_iter()
    .chain(
        context.ev_stats.iter()
        .map(|(ev, ev_stats)|
            if ev.real_name == "_start" {
                let top_stats =
                    Box::new(
                        context.top_stats.iter().flat_map(|x| visit_top_stat(context, x.stat.clone()))
                    );
                routine::decl_start_ev(ev_label(context, ev.clone()), top_stats, visit_stats_block(context, ev_stats.stats.clone()))
                .collect()
            }
            else {
                routine::decl_ev(ev_label(context, ev.clone()), visit_stats_block(context, ev_stats.stats.clone()))
                .collect()
            }
        )
    )
    .collect()
}

pub fn visit_top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    top_stat: Rc<ast::TopStat<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match top_stat.detail.as_ref() {
        ast::TopStatDetail::None =>
            empty(),
        ast::TopStatDetail::VarBind { access_attr, sync_attr, var_bind } =>
            visit_var_bind_top_stat(context, access_attr.clone(), sync_attr.clone(), var_bind.clone()),
        ast::TopStatDetail::Stat { stat } =>
            visit_stat_top_stat(context, stat.clone()),
        _ =>
            error("Illegal state".to_owned()),
    }
}

pub fn visit_var_bind_top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _access_attr: Rc<ast::AccessAttr<'input>>,
    _sync_attr: Rc<ast::SyncAttr<'input>>,
    var_bind: Rc<ast::VarBind<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    visit_var_bind(context, var_bind)
}

pub fn visit_stat_top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    stat: Rc<ast::Stat<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    visit_stat(context, stat)
}

pub fn visit_var_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
    var_bind: Rc<ast::VarBind<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    Box::new(
        visit_expr(context, var_bind.expr.clone())
        .chain(visit_var_decl(context, var_bind.var_decl.clone()))
    )
}

pub fn visit_var_decl<'input: 'context, 'context>(
    context: &'context Context<'input>,
    var_decl: Rc<ast::VarDecl<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match var_decl.detail.as_ref() {
        ast::VarDeclDetail::SingleDecl { mut_attr: _, ident: _, ty_expr: _, var } =>
            Box::new(routine::set(var_label(context, var.clone()))),
        ast::VarDeclDetail::TupleDecl { var_decls } =>
            Box::new(var_decls.clone().into_iter().rev().flat_map(|x| visit_var_decl(context, x.clone()))),
    }
}

pub fn visit_stats_block<'input: 'context, 'context>(
    context: &'context Context<'input>,
    stats_block: Rc<ast::StatsBlock<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    Box::new(
        stats_block.stats.clone().into_iter().flat_map(|x| visit_stat(context, x.clone()))
        .chain(visit_expr(context, stats_block.ret.get()))
    )
}

pub fn visit_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    stat: Rc<ast::Stat<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match stat.detail.as_ref() {
        ast::StatDetail::VarBind { var_bind } =>
            visit_var_bind_stat(context, var_bind.clone()),
        ast::StatDetail::Expr { expr } =>
            visit_expr_stat(context, expr.clone()),
        _ =>
            error("stat".to_owned())
    }
}

pub fn visit_var_bind_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    var_bind: Rc<ast::VarBind<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    visit_var_bind(context, var_bind)
}

pub fn visit_expr_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    expr: Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    visit_expr(context, expr)
}

pub fn visit_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    expr: Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match expr.detail.as_ref() {
        ast::ExprDetail::Term { term } =>
            visit_term(context, term.clone()),
        ast::ExprDetail::PrefixOp { op, expr: sub_expr, operation } =>
            visit_term_prefix_op(context, op, sub_expr.clone(), expr.tmp_vars.clone(), operation.clone()),
        ast::ExprDetail::InfixOp { left, op, right, operation } =>
            visit_term_infix_op(context, left.clone(), op, right.clone(), expr.tmp_vars.clone(), operation.clone()),
    }
}

fn visit_term_prefix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    op: &ast::TermPrefixOp,
    expr: Rc<ast::Expr<'input>>,
    tmp_vars: Vec<Rc<Var>>,
    operation: Rc<Operation>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match op {
        ast::TermPrefixOp::Plus =>
            visit_expr(context, expr),
        ast::TermPrefixOp::Minus |
        ast::TermPrefixOp::Bang => {
            let args = visit_expr(context, expr);
            let out_vars = tmp_vars.into_iter().map(|x| context.var_labels[&x].clone()).collect();
            let method = context.method_labels[&operation.op_methods["op"]].clone();
            routine::call_method(args, out_vars, method)
        },
        ast::TermPrefixOp::Tilde => {
            let literal = context.literal_labels[&operation.op_literals["mask"]].clone();
            let args = Box::new(visit_expr(context, expr).chain(routine::get(literal)));
            let out_vars = tmp_vars.into_iter().map(|x| context.var_labels[&x].clone()).collect();
            let method = context.method_labels[&operation.op_methods["op"]].clone();
            routine::call_method(args, out_vars, method)
        },
    }
}

fn visit_term_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    left: Rc<ast::Expr<'input>>,
    op: &ast::TermInfixOp,
    right: Rc<ast::Expr<'input>>,
    tmp_vars: Vec<Rc<Var>>,
    operation: Rc<Operation>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match op {
        ast::TermInfixOp::Mul |
        ast::TermInfixOp::Div |
        ast::TermInfixOp::Mod |
        ast::TermInfixOp::Add |
        ast::TermInfixOp::Sub => {
            let args = Box::new(visit_expr(context, left).chain(visit_expr(context, right)));
            let out_vars = tmp_vars.into_iter().map(|x| context.var_labels[&x].clone()).collect();
            let method = context.method_labels[&operation.op_methods["op"]].clone();
            routine::call_method(args, out_vars, method)
        },
        _ =>
            error("term_infix_op".to_owned())
    }
}

pub fn visit_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    term: Rc<ast::Term<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match term.detail.as_ref() {
        ast::TermDetail::Factor { factor } =>
            visit_factor(context, factor.clone()),
        ast::TermDetail::InfixOp { left, op, right, operation, instance: _ } =>
            visit_factor_infix_op(context, left.clone(), op, right.clone(), term.tmp_vars.clone(), operation.clone())
    }
}

pub fn visit_factor_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    left: Rc<ast::Term<'input>>,
    op: &ast::FactorInfixOp,
    right: Rc<ast::Term<'input>>,
    tmp_vars: Vec<Rc<Var>>,
    _operation: Rc<Operation>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match op {
        ast::FactorInfixOp::TyAccess =>
            visit_ty_access_op(context, left, right),
        ast::FactorInfixOp::Access =>
            visit_access_op(context, left, right),
        ast::FactorInfixOp::EvalFn =>
            visit_eval_fn_op(context, left, right, tmp_vars),
        _ =>
            error("factor_infix_op".to_owned()),
    }
}

fn visit_ty_access_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _left: Rc<ast::Term<'input>>,
    right: Rc<ast::Term<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    let ast::TermDetail::Factor { factor } = right.detail.as_ref()
        else {
            return error("ty_access_op".to_owned());
        };
    let ast::FactorDetail::EvalVar { ident: _, var, getter } = factor.detail.as_ref()
        else {
            return error("ty_access_op".to_owned());
        };
    let var = var.borrow();
    let Some(var) = var.as_ref()
        else {
            return error("ty_access_op".to_owned());
        };

    

    if let Some((tmp_var, getter)) = getter.borrow().as_ref() {
        let data = var_label(context, tmp_var.clone());
        let ext = method_label(context, getter.clone());
        Box::new(routine::call_method(empty(), vec![data], ext))
    }
    else {
        Box::new(routine::get(var_label(context, var.clone())))
    }
}

fn visit_access_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    left: Rc<ast::Term<'input>>,
    right: Rc<ast::Term<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    let ast::TermDetail::Factor { factor } = right.detail.as_ref()
        else {
            return error("access_op".to_owned());
        };
    let ast::FactorDetail::EvalVar { ident: _, var, getter } = factor.detail.as_ref()
        else {
            return error("access_op".to_owned());
        };
    let var = var.borrow();
    let Some(var) = var.as_ref()
        else {
            return error("access_op".to_owned());
        };

    if let Some((tmp_var, getter)) = getter.borrow().as_ref() {
        let data = var_label(context, tmp_var.clone());
        let ext = method_label(context, getter.clone());
        Box::new(
            visit_term(context, left)
            .chain(routine::call_method(empty(), vec![data], ext))
        )
    }
    else {
        Box::new(
            visit_term(context, left)
            .chain(routine::get(var_label(context, var.clone())))
        )
    }
}

fn visit_eval_fn_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    left: Rc<ast::Term<'input>>,
    right: Rc<ast::Term<'input>>,
    tmp_vars: Vec<Rc<Var>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    let ast::TermDetail::Factor { factor } = right.detail.as_ref()
        else {
            return error("eval_fn_op".to_owned());
        };
    let ast::FactorDetail::ApplyFn { args, as_fn } = factor.detail.as_ref()
        else {
            return error("eval_fn_op".to_owned());
        };

    match as_fn.borrow().as_ref() {
        Some(x) => match x.as_ref() {
            ast::AsFn::Fn(f) => {
                for (v, d) in f.fn_stats.vars.iter().zip(f.data.iter()) {
                    v.actual_name.replace(Some(d.clone()));
                }
                visit_stats_block(context, f.fn_stats.stats.clone())
            },
            ast::AsFn::Method(m) => {
                let args =
                    Box::new(
                        args.clone().into_iter().flat_map(|x| visit_expr(context, x.expr.get()))
                    );
                let out_vars = tmp_vars.into_iter().map(|x| context.var_labels[&x].clone()).collect();
                Box::new(
                    visit_term(context, left)
                    .chain(routine::call_method(args, out_vars, method_label(context, m.clone())))
                )
            },
        }
        None =>
            error("eval_fn_op".to_owned()),
    }
}

pub fn visit_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    factor: Rc<ast::Factor<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match factor.detail.as_ref() {
        ast::FactorDetail::None =>
            empty(),
        ast::FactorDetail::Block { stats } =>
            visit_block_factor(context, stats.clone()),
        ast::FactorDetail::Literal { literal } =>
            visit_literal_factor(context, literal.clone()),
        ast::FactorDetail::EvalVar { ident: _, var, getter } =>
            visit_eval_var_factor(context, var, getter),
        _ =>
            error("term".to_owned())
    }
}

fn visit_block_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    stats: Rc<ast::StatsBlock<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    visit_stats_block(context, stats)
}

fn visit_literal_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    literal: Rc<Literal>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    Box::new(routine::get(literal_label(context, literal)))
}

fn visit_eval_var_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    var: &RefCell<Option<Rc<Var>>>,
    getter: &RefCell<Option<(Rc<Var>, Rc<Method>)>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    if let Some(var) = var.borrow().as_ref() {
        if let Some((tmp_var, getter)) = getter.borrow().as_ref() {
            let data = var_label(context, tmp_var.clone());
            let ext = method_label(context, getter.clone());
            Box::new(routine::call_method(empty(), vec![data], ext))
        }
        else {
            Box::new(routine::get(var_label(context, var.clone())))
        }
    }
    else {
        error("eval_var_term".to_owned())
    }
}

fn error<'context>(message: String) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    Box::new(routine::comment(format!("Error detected: `{}`", message)))
}

fn empty<'context>() -> Box<dyn Iterator<Item = Instruction> + 'context> {
    Box::new([].into_iter())
}

fn ev_label(context: &Context, ev: Rc<Ev>) -> Rc<CodeLabel> {
    context.ev_labels.get(&ev).unwrap().clone()
}

fn literal_label(context: &Context, literal: Rc<Literal>) -> Rc<DataLabel> {
    context.literal_labels.get(&literal).unwrap().clone()
}

fn method_label(context: &Context, method: Rc<Method>) -> Rc<ExternLabel> {
    context.method_labels.get(&method).unwrap().clone()
}

fn var_label(context: &Context, var: Rc<Var>) -> Rc<DataLabel> {
    context.var_labels.get(&var).unwrap().clone()
}
