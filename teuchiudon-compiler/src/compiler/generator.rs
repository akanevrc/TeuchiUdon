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
    context.var_labels.values().flat_map(|x| routine::decl_data(x.clone(), AsmLiteral::Null))
    .chain(context.literal_labels.values().flat_map(|x| routine::decl_data(x.clone(), AsmLiteral::Null)))
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
    Box::new(stats_block.stats.clone().into_iter().flat_map(|x| visit_stat(context, x.clone())))
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
        ast::ExprDetail::InfixOp { left, op, right } =>
            visit_infix_op(context, left.clone(), op, right.clone())
    }
}

pub fn visit_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    left: Rc<ast::Expr<'input>>,
    op: &ast::Op,
    right: Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match op {
        ast::Op::TyAccess =>
            visit_ty_access_op(context, left, right),
        ast::Op::EvalFn =>
            visit_eval_fn_op(context, left, right),
        _ =>
            error("infix_op".to_owned()),
    }
}

fn visit_ty_access_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _left: Rc<ast::Expr<'input>>,
    right: Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    let ast::ExprDetail::Term { term } = right.detail.as_ref()
        else {
            return error("ty_access_op".to_owned());
        };
    let ast::TermDetail::EvalVar { ident: _, var } = term.detail.as_ref()
        else {
            return error("ty_access_op".to_owned());
        };
    let var = var.borrow();
    let Some(var) = var.as_ref()
        else {
            return error("ty_access_op".to_owned());
        };

    Box::new(routine::get(var_label(context, var.clone())))
}

fn visit_eval_fn_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    left: Rc<ast::Expr<'input>>,
    right: Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    let ast::ExprDetail::Term { term } = right.detail.as_ref()
        else {
            return error("eval_fn_op".to_owned());
        };
    let ast::TermDetail::ApplyFn { args, as_fn } = term.detail.as_ref()
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
                        args.clone().into_iter().flat_map(|x| visit_expr(context, x.expr.clone()))
                    );
                Box::new(
                    visit_expr(context, left)
                    .chain(routine::call_method(args, method_label(context, m.clone())))
                )
            },
        }
        None =>
            error("eval_fn_op".to_owned()),
    }
}

pub fn visit_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    term: Rc<ast::Term<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    match term.detail.as_ref() {
        ast::TermDetail::Block { stats } =>
            visit_block_term(context, stats.clone()),
        ast::TermDetail::Literal { literal } =>
            visit_literal_term(context, literal.clone()),
        ast::TermDetail::EvalVar { ident: _, var } =>
            visit_eval_var_term(context, var),
        _ =>
            error("term".to_owned())
    }
}

fn visit_block_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    stats: Rc<ast::StatsBlock<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    visit_stats_block(context, stats)
}

fn visit_literal_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    literal: Rc<Literal>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    Box::new(routine::get(literal_label(context, literal)))
}

fn visit_eval_var_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    var: &RefCell<Option<Rc<Var>>>,
) -> Box<dyn Iterator<Item = Instruction> + 'context> {
    if let Some(var) = var.borrow().as_ref() {
        Box::new(routine::get(var_label(context, var.clone())))
    }
    else {
        error("eval_var_term".to_owned())
    }
}

fn error(message: String) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new(routine::comment(format!("Error detected: `{}`", message)))
}

fn empty() -> Box<dyn Iterator<Item = Instruction>> {
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
