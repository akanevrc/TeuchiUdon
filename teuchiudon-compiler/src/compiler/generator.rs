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

pub fn generate_data_part<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    Box::new(
        context.var_labels.values().flat_map(|x| routine::decl_data(x.clone(), AsmLiteral::Null))
        .chain(context.literal_labels.values().flat_map(|x| routine::decl_data(x.clone(), AsmLiteral::Null)))
    )
}

pub fn generate_code_part<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    _target: &'parsed Rc<ast::Target<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    Box::new(
        if context.ev_stats.iter().find(|(ev, _)| ev.real_name == "_start").is_none() {
            let top_stats =
                Box::new(
                    context.top_stats.iter().flat_map(|x| visit_top_stat(context, &x.stat))
                );
            routine::decl_start_ev(CodeLabel::from_name("_start"), top_stats, empty())
        }
        else {
            empty()
        }
        .chain(
            context.ev_stats.iter()
            .flat_map(|(ev, ev_stats)|
                if ev.real_name == "_start" {
                    let top_stats =
                        Box::new(
                            context.top_stats.iter().flat_map(|x| visit_top_stat(context, &x.stat))
                        );
                    routine::decl_start_ev(ev_label(context, ev), top_stats, visit_stats_block(context, &ev_stats.stats))
                }
                else {
                    routine::decl_ev(ev_label(context, ev), visit_stats_block(context, &ev_stats.stats))
                }
            )
        )
    )
}

pub fn visit_top_stat<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    top_stat: &'parsed Rc<ast::TopStat<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    match top_stat.detail.as_ref() {
        ast::TopStatDetail::None =>
            empty(),
        ast::TopStatDetail::VarBind { access_attr, sync_attr, var_bind } =>
            visit_var_bind_top_stat(context, access_attr, sync_attr, var_bind),
        ast::TopStatDetail::FnBind { access_attr, fn_bind, ev: _ } =>
            visit_fn_bind_top_stat(context, access_attr, fn_bind),
        ast::TopStatDetail::Stat { stat } =>
            visit_stat_top_stat(context, stat),
    }
}

pub fn visit_var_bind_top_stat<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    _access_attr: &'parsed Rc<ast::AccessAttr<'input>>,
    _sync_attr: &'parsed Rc<ast::SyncAttr<'input>>,
    var_bind: &'parsed Rc<ast::VarBind<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    visit_var_bind(context, var_bind)
}

pub fn visit_fn_bind_top_stat<'input: 'context, 'context: 'parsed, 'parsed>(
    _context: &'input Context<'input>,
    _access_attr: &'parsed Rc<ast::AccessAttr<'input>>,
    _fn_bind: &'parsed Rc<ast::FnBind<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    empty()
}

pub fn visit_stat_top_stat<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    stat: &'parsed Rc<ast::Stat<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    visit_stat(context, stat)
}

pub fn visit_var_bind<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    var_bind: &'parsed Rc<ast::VarBind<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    Box::new(
        visit_expr(context, &var_bind.expr)
        .chain(visit_var_decl(context, &var_bind.var_decl))
    )
}

pub fn visit_var_decl<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    var_decl: &'parsed Rc<ast::VarDecl<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    match var_decl.detail.as_ref() {
        ast::VarDeclDetail::SingleDecl { mut_attr: _, ident: _, ty_expr: _, var } =>
            Box::new(routine::set(var_label(context, var))),
        ast::VarDeclDetail::TupleDecl { var_decls } =>
            Box::new(var_decls.iter().rev().flat_map(|x| visit_var_decl(context, x))),
    }
}

pub fn visit_stats_block<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    stats_block: &'parsed Rc<ast::StatsBlock<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    Box::new(stats_block.stats.iter().flat_map(|x| visit_stat(context, x)))
}

pub fn visit_stat<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    stat: &'parsed Rc<ast::Stat<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    match stat.detail.as_ref() {
        ast::StatDetail::VarBind { var_bind } =>
            visit_var_bind_stat(context, var_bind),
        ast::StatDetail::Expr { expr } =>
            visit_expr_stat(context, expr),
        _ =>
            error("stat".to_owned())
    }
}

pub fn visit_var_bind_stat<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    var_bind: &'parsed Rc<ast::VarBind<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    visit_var_bind(context, var_bind)
}

pub fn visit_expr_stat<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    expr: &'parsed Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    visit_expr(context, expr)
}

pub fn visit_expr<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    expr: &'parsed Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    match expr.detail.as_ref() {
        ast::ExprDetail::Term { term } =>
            visit_term(context, term),
        ast::ExprDetail::InfixOp { left, op, right } =>
            visit_infix_op(context, left, op, right)
    }
}

pub fn visit_infix_op<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    left: &'parsed Rc<ast::Expr<'input>>,
    op: &'parsed ast::Op,
    right: &'parsed Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    match op {
        ast::Op::TyAccess =>
            visit_ty_access_op(context, left, right),
        ast::Op::EvalFn =>
            visit_eval_fn_op(context, left, right),
        _ =>
            error("infix_op".to_owned()),
    }
}

fn visit_ty_access_op<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    _left: &'parsed Rc<ast::Expr<'input>>,
    right: &'parsed Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
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

    Box::new(routine::get(var_label(context, var)))
}

fn visit_eval_fn_op<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    left: &'parsed Rc<ast::Expr<'input>>,
    right: &'parsed Rc<ast::Expr<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    let ast::ExprDetail::Term { term } = right.detail.as_ref()
        else {
            return error("eval_fn_op".to_owned());
        };
    let ast::TermDetail::ApplyFn { args, method } = term.detail.as_ref()
        else {
            return error("eval_fn_op".to_owned());
        };
    let method = method.borrow();
    let Some(as_fn) = method.as_ref()
        else {
            return error("eval_fn_op".to_owned());
        };
    let ast::AsFn::Method(method) = as_fn.as_ref();

    let args: Box<dyn Iterator<Item = Instruction>> =
        Box::new(
            args.iter().flat_map(|x| visit_expr(context, &x.expr))
        );
    Box::new(
        visit_expr(context, left)
        .chain(routine::call_method(args, method_label(context, method)))
    )
}

pub fn visit_term<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    term: &'parsed Rc<ast::Term<'input>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    match term.detail.as_ref() {
        ast::TermDetail::Block { stats } =>
            visit_block_term(context, stats),
        ast::TermDetail::Literal { literal } =>
            visit_literal_term(context, literal),
        ast::TermDetail::EvalVar { ident: _, var } =>
            visit_eval_var_term(context, var),
        _ =>
            error("term".to_owned())
    }
}

fn visit_block_term<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    stats: &'parsed Rc<ast::StatsBlock>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    visit_stats_block(context, stats)
}

fn visit_literal_term<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    literal: &'parsed Rc<Literal>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    Box::new(routine::get(literal_label(context, &literal)))
}

fn visit_eval_var_term<'input: 'context, 'context: 'parsed, 'parsed>(
    context: &'input Context<'input>,
    var: &'parsed RefCell<Option<Rc<Var>>>,
) -> Box<dyn Iterator<Item = Instruction> + 'parsed> {
    if let Some(var) = var.borrow().as_ref() {
        Box::new(routine::get(var_label(context, var)))
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

fn ev_label(context: &Context, ev: &Rc<Ev>) -> Rc<CodeLabel> {
    context.ev_labels.get(ev).unwrap().clone()
}

fn literal_label(context: &Context, literal: &Rc<Literal>) -> Rc<DataLabel> {
    context.literal_labels.get(literal).unwrap().clone()
}

fn method_label(context: &Context, method: &Rc<Method>) -> Rc<ExternLabel> {
    context.method_labels.get(method).unwrap().clone()
}

fn var_label(context: &Context, var: &Rc<Var>) -> Rc<DataLabel> {
    context.var_labels.get(var).unwrap().clone()
}
