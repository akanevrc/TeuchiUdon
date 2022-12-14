use std::{
    cell::RefCell,
    collections::VecDeque,
    fmt::Debug,
    rc::Rc,
};
use crate::context::Context;
use crate::lexer;
use crate::parser;
use super::elements::element::SemanticElement;
use super::{
    ast,
    SemanticError,
    elements::{
        ElementError,
        element::{
            KeyElement,
            ValueElement,
        },
        ev::Ev,
        ev_stats::EvStats,
        eval_fn::EvalFn,
        fn_stats::FnStats,
        label::{
            DataLabel,
            DataLabelKind,
        },
        literal::Literal,
        method::{
            Method,
            MethodParamInOut,
        },
        operation::{
            Operation,
            OperationKind
        },
        qual::Qual,
        scope::Scope,
        this_literal::ThisLiteral,
        top_stat::TopStat,
        ty::Ty,
        valued_var::ValuedVar,
        var::Var,
    },
};

pub fn target<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Target<'input>>,
) -> Result<Rc<ast::Target<'input>>, Vec<SemanticError<'input>>> {
    let body = match &node.body {
        Some(x) => body(context, x.clone())?,
        None => empty_body(context)?,
    };
    Ok(Rc::new(ast::Target {
        parsed: Some(node),
        body,
    }))
}

pub fn body<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Body<'input>>,
) -> Result<Rc<ast::Body<'input>>, Vec<SemanticError<'input>>> {
    let top_stats =
        node.top_stats.iter()
        .map(|x| top_stat(context, x.clone()))
        .collect::<Result<_, _>>()?;
    Ok(Rc::new(ast::Body {
        parsed: Some(node),
        top_stats,
    }))
}

fn empty_body<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<Rc<ast::Body<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::Body {
        parsed: None,
        top_stats: Vec::new(),
    }))
}

pub fn top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TopStat<'input>>,
) -> Result<Rc<ast::TopStat<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::TopStatKind::VarBind { access_attr, sync_attr, var_bind } =>
            var_bind_top_stat(context, node.clone(), access_attr.clone(), sync_attr.clone(), var_bind.clone()),
        parser::ast::TopStatKind::FnBind { access_attr, fn_bind } =>
            fn_bind_top_stat(context, node.clone(), access_attr.clone(), fn_bind.clone()),
        parser::ast::TopStatKind::Stat { stat } =>
            stat_top_stat(context, node.clone(), stat.clone()),
    }
}

fn var_bind_top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TopStat<'input>>,
    access_attr: Option<Rc<parser::ast::AccessAttr<'input>>>,
    sync_attr: Option<Rc<parser::ast::SyncAttr<'input>>>,
    var_bind: Rc<parser::ast::VarBind<'input>>,
) -> Result<Rc<ast::TopStat<'input>>, Vec<SemanticError<'input>>> {
    let access_attr = self::access_attr(context, access_attr)?;
    let sync_attr = self::sync_attr(context, sync_attr)?;
    let var_bind = self::var_bind(context, var_bind)?;
    match access_attr.detail {
        ast::AccessAttrDetail::None => {
            let top_stat = Rc::new(ast::TopStat {
                parsed: Some(node),
                detail: Rc::new(ast::TopStatDetail::VarBind {
                    access_attr,
                    sync_attr,
                    var_bind,
                }),
            });
            TopStat::new(context, top_stat.clone());
            Ok(top_stat)
        },
        ast::AccessAttrDetail::Pub => {
            if var_bind.vars.len() != 1 {
                return Err(vec![SemanticError::new(Some(node.slice), "Public variable must not be tuple".to_owned())]);
            }
            let var = &var_bind.vars[0];
            let Some(literal) = ast::expr_to_literal(context, var_bind.expr.clone())
                else {
                    return Err(vec![SemanticError::new(Some(node.slice), "Public variable should be assigned from a literal".to_owned())]);
                };
            ValuedVar::new(context, var.qual.clone(), var.name.clone(), var.ty.borrow().clone(), literal)
                .map_err(|e| e.convert(None))?;
            Ok(Rc::new(ast::TopStat {
                parsed: Some(node),
                detail: Rc::new(ast::TopStatDetail::None),
            }))
        },
    }
}

fn fn_bind_top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TopStat<'input>>,
    access_attr: Option<Rc<parser::ast::AccessAttr<'input>>>,
    fn_bind: Rc<parser::ast::FnBind<'input>>,
) -> Result<Rc<ast::TopStat<'input>>, Vec<SemanticError<'input>>> {
    let access_attr = self::access_attr(context, access_attr)?;
    let fn_bind = self::fn_bind(context, fn_bind)?;
    match access_attr.detail {
        ast::AccessAttrDetail::None => {
            Ok(Rc::new(ast::TopStat {
                parsed: Some(node),
                detail: Rc::new(ast::TopStatDetail::FnBind {
                    access_attr,
                    fn_bind,
                    ev: None,
                }),
            }))
        },
        ast::AccessAttrDetail::Pub => {
            let tys =
                fn_bind.fn_decl.var_decl.ty.ty_to_tys(context)
                .map_err(|e| e.convert(None))?;
            let ev =
                Ev::new_or_get(
                    context,
                    fn_bind.fn_decl.ident.name.clone(),
                    tys,
                    vec![MethodParamInOut::In],
                    fn_bind.fn_decl.ident.name.clone(),
                    fn_bind.fn_decl.var_decl.vars.iter().map(|x| x.name.clone()).collect(),
                );
            let ev_stats =
                EvStats::new(context, fn_bind.fn_decl.ident.name.clone(), fn_bind.stats_block.clone())
                .map_err(|e| e.convert(fn_bind.fn_decl.ident.parsed.clone().map(|x| x.slice)))?;
            Ok(Rc::new(ast::TopStat {
                parsed: Some(node),
                detail: Rc::new(ast::TopStatDetail::FnBind {
                    access_attr,
                    fn_bind,
                    ev: Some((ev, ev_stats)),
                }),
            }))
        },
    }
}

fn stat_top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TopStat<'input>>,
    stat: Rc<parser::ast::Stat<'input>>,
) -> Result<Rc<ast::TopStat<'input>>, Vec<SemanticError<'input>>> {
    let stat = self::stat(context, stat)?;
    let top_stat = Rc::new(ast::TopStat {
        parsed: Some(node),
        detail: Rc::new(ast::TopStatDetail::Stat {
            stat,
        }),
    });
    TopStat::new(context, top_stat.clone());
    Ok(top_stat)
}

pub fn access_attr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Option<Rc<parser::ast::AccessAttr<'input>>>,
) -> Result<Rc<ast::AccessAttr<'input>>, Vec<SemanticError<'input>>> {
    match node {
        Some(attr) =>
            match &attr.attr.kind {
                lexer::ast::KeywordKind::Pub =>
                    pub_access_attr(context, attr),
                _ =>
                    panic!("Illegal state"),
            },
        None =>
            Ok(Rc::new(ast::AccessAttr { parsed: None, detail: ast::AccessAttrDetail::None })),
    }
    
}

fn pub_access_attr<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<parser::ast::AccessAttr<'input>>,
) -> Result<Rc<ast::AccessAttr<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::AccessAttr {
        parsed: Some(node),
        detail: ast::AccessAttrDetail::Pub,
    }))
}

fn sync_attr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Option<Rc<parser::ast::SyncAttr<'input>>>,
) -> Result<Rc<ast::SyncAttr<'input>>, Vec<SemanticError<'input>>> {
    match node {
        Some(attr) =>
            match &attr.attr.kind {
                lexer::ast::KeywordKind::Sync =>
                    sync_sync_attr(context, attr),
                lexer::ast::KeywordKind::Linear =>
                    linear_sync_attr(context, attr),
                lexer::ast::KeywordKind::Smooth =>
                    smooth_sync_attr(context, attr),
                _ =>
                    panic!("Illegal state"),
            },
        None =>
            Ok(Rc::new(ast::SyncAttr { parsed: None, detail: ast::SyncAttrDetail::None })),
    }
    
}

fn sync_sync_attr<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<parser::ast::SyncAttr<'input>>,
) -> Result<Rc<ast::SyncAttr<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::SyncAttr {
        parsed: Some(node),
        detail: ast::SyncAttrDetail::Sync,
    }))
}

fn linear_sync_attr<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<parser::ast::SyncAttr<'input>>,
) -> Result<Rc<ast::SyncAttr<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::SyncAttr {
        parsed: Some(node),
        detail: ast::SyncAttrDetail::Linear,
    }))
}

fn smooth_sync_attr<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<parser::ast::SyncAttr<'input>>,
) -> Result<Rc<ast::SyncAttr<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::SyncAttr {
        parsed: Some(node),
        detail: ast::SyncAttrDetail::Smooth,
    }))
}

pub fn var_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::VarBind<'input>>,
) -> Result<Rc<ast::VarBind<'input>>, Vec<SemanticError<'input>>> {
    let var_decl = var_decl(context, node.var_decl.clone())?;
    let expr = expr(context, node.expr.clone())?;
    let expr = expr.release(context);
    let mut vars = var_decl.vars.iter().cloned().collect();
    infer(context, &mut vars, expr.ty.clone())
        .map_err(|e| e.convert(Some(node.slice)))?;
    Ok(Rc::new(ast::VarBind {
        parsed: Some(node),
        var_decl: var_decl.clone(),
        expr,
        vars: var_decl.vars.iter().cloned().collect(),
    }))
}

fn infer<'input: 'context, 'context>(
    context: &'context Context<'input>,
    vars: &mut VecDeque<Rc<Var>>,
    ty: Rc<Ty>,
) -> Result<(), ElementError> {
    if vars.len() == 0 && ty.base_eq_with_name("unit") {
        Ok(())
    }
    else if vars.len() == 1 && vars[0].ty.borrow().assignable_from(context, &ty) {
        let v = vars.pop_front().unwrap();
        let mut t = v.ty.borrow_mut();
        *t = t.infer(context, &ty)?;
        Ok(())
    }
    else if ty.base_eq_with_name("tuple") {
        let ty_args = ty.args_as_tuple();
        if vars.len() < ty_args.len() {
            return Err(ElementError::new(format!("Variable cannot be bound with type `{}`", ty.description())));
        }
        for t in ty_args {
            let t = t.get_value(context)?;
            infer(context, vars, t)?;
        }
        Ok(())
    }
    else {
        Err(ElementError::new(format!("Variable cannot be bound with type `{}`", ty.description())))
    }
}

pub fn var_decl<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::VarDecl<'input>>,
) -> Result<Rc<ast::VarDecl<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::VarDeclKind::SingleDecl { mut_attr, ident, ty_expr } =>
            single_var_decl(context, node.clone(), mut_attr.clone(), ident.clone(), ty_expr.clone()),
        parser::ast::VarDeclKind::TupleDecl{ var_decls } =>
            tuple_var_decl(context, node.clone(), var_decls),
    }
}

fn single_var_decl<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::VarDecl<'input>>,
    mut_attr: Option<Rc<parser::ast::MutAttr<'input>>>,
    ident: Rc<lexer::ast::Ident<'input>>,
    ty_expr: Option<Rc<parser::ast::TyExpr<'input>>>,
) -> Result<Rc<ast::VarDecl<'input>>, Vec<SemanticError<'input>>> {
    let mut_attr = self::mut_attr(context, mut_attr)?;
    let ident = self::ident(context, ident)?;
    let ty_expr = match ty_expr {
        Some(x) => self::ty_expr(context, x)?,
        None => hidden_unknown_ty_expr(context)?,
    };
    let qual =
        context.qual_stack.peek().get_value(context)
        .map_err(|x| x.convert(None))?;
    let ty =
        ty_expr.ty.arg_as_type().get_value(context)
        .map_err(|x| x.convert(None))?;
    let var = Var::force_new(
        context,
        qual,
        ident.name.clone(),
        ty.clone(),
        matches!(mut_attr.detail, ast::MutAttrDetail::Mut),
        None,
    );
    Ok(Rc::new(ast::VarDecl {
        parsed: Some(node),
        detail: Rc::new(ast::VarDeclDetail::SingleDecl {
            mut_attr,
            ident: ident.clone(),
            ty_expr,
            var: var.clone(),
        }),
        ty,
        vars: vec![var],
    }))
}

fn tuple_var_decl<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::VarDecl<'input>>,
    var_decls: &Vec<Rc<parser::ast::VarDecl<'input>>>,
) -> Result<Rc<ast::VarDecl<'input>>, Vec<SemanticError<'input>>> {
    let var_decls =
        var_decls.iter()
        .map(|x| var_decl(context, x.clone()))
        .collect::<Result<Vec<_>, _>>()?;
    let ty =
        Ty::new_or_get_tuple_from_keys(context, var_decls.iter().map(|x| x.ty.to_key()).collect())
        .map_err(|e| e.convert(None))?;
    let vars =
        var_decls.iter().flat_map(|x| x.vars.clone()).collect();
    Ok(Rc::new(ast::VarDecl {
        parsed: Some(node),
        detail: Rc::new(ast::VarDeclDetail::TupleDecl {
            var_decls,
        }),
        ty,
        vars,
    }))
}

pub fn mut_attr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Option<Rc<parser::ast::MutAttr<'input>>>,
) -> Result<Rc<ast::MutAttr<'input>>, Vec<SemanticError<'input>>> {
    match node {
        Some(attr) =>
            match &attr.attr.kind {
                lexer::ast::KeywordKind::Mut =>
                    mut_mut_attr(context, attr),
                _ =>
                    panic!("Illegal state"),
            },
        None =>
            Ok(Rc::new(ast::MutAttr { parsed: None, detail: ast::MutAttrDetail::None })),
    }
}

fn mut_mut_attr<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<parser::ast::MutAttr<'input>>,
) -> Result<Rc<ast::MutAttr<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::MutAttr {
        parsed: Some(node),
        detail: ast::MutAttrDetail::Mut,
    }))
}

pub fn fn_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::FnBind<'input>>,
) -> Result<Rc<ast::FnBind<'input>>, Vec<SemanticError<'input>>> {
    let fn_decl = fn_decl(context, node.fn_decl.clone())?;
    if fn_decl.var_decl.ty.args_as_tuple().len() != fn_decl.var_decl.vars.len() {
        return Err(vec![SemanticError::new(fn_decl.var_decl.parsed.clone().map(|x| x.slice), "Function arguments cannot be tuple".to_owned())]);
    }
    let qual =
        context.qual_stack.peek().get_value(context)
        .map_err(|e| e.convert(None))?;
    let stats_block = stats_block(context, node.stats_block.clone(), Scope::Fn(context.fn_stats_store.next_id()))?;
    let fn_stats =
        FnStats::new_or_get(
            context,
            qual,
            fn_decl.ident.name.clone(),
            fn_decl.ty_expr.ty.clone(),
            fn_decl.var_decl.vars.clone(),
            stats_block.clone(),
        )
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::FnBind {
        parsed: Some(node),
        fn_decl,
        stats_block,
        fn_stats,
    }))
}

pub fn fn_decl<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::FnDecl<'input>>,
) -> Result<Rc<ast::FnDecl<'input>>, Vec<SemanticError<'input>>> {
    let ident = ident(context, node.ident.clone())?;
    let var_decl = var_decl(context, node.var_decl.clone())?;
    let ty_expr = match &node.ty_expr {
        Some(x) => ty_expr(context, x.clone())?,
        None => hidden_unit_ty_expr(context)?,
    };
    Ok(Rc::new(ast::FnDecl {
        parsed: Some(node),
        ident,
        var_decl,
        ty_expr,
    }))
}

pub fn ty_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyExpr<'input>>,
) -> Result<Rc<ast::TyExpr<'input>>, Vec<SemanticError<'input>>> {
    construct_ty_expr_tree(context, node)
}

fn construct_ty_expr_tree<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyExpr<'input>>,
) -> Result<Rc<ast::TyExpr<'input>>, Vec<SemanticError<'input>>> {
    let mut exprs = VecDeque::new();
    let mut ops = VecDeque::new();
    let factor = ty_factor(context, node.ty_factor.clone())?;
    exprs.push_back(Rc::new(ast::TyExpr {
        parsed: Some(node.clone()),
        detail: Rc::new(ast::TyExprDetail::Factor {
            factor: factor.clone(),
        }),
        ty: factor.ty.clone(),
    }));
    for op in &node.ty_ops {
        let (op, expr) = match op.kind.as_ref() {
            parser::ast::TyOpKind::Access { op_code: _, ty_factor } =>
                access_op_ty_expr(context, node.clone(), ty_factor.clone())?,
        };
        ops.push_back(op);
        exprs.push_back(expr);
    }
    expr_tree(context, node, exprs, ops)
}

fn access_op_ty_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyExpr<'input>>,
    factor: Rc<parser::ast::TyFactor<'input>>,
) -> Result<(ast::TyOp, Rc<ast::TyExpr<'input>>), Vec<SemanticError<'input>>> {
    let op = access_ty_op(context)?;
    let factor = access_ty_factor(context, factor)?;
    let expr = Rc::new(ast::TyExpr {
        parsed: Some(node),
        detail: Rc::new(ast::TyExprDetail::Factor {
            factor: factor.clone(),
        }),
        ty: factor.ty.clone(),
    });
    Ok((op, expr))
}

fn hidden_unknown_ty_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> Result<Rc<ast::TyExpr<'input>>, Vec<SemanticError<'input>>> {
    let factor = Rc::new(ast::TyFactor {
        parsed: None,
        detail: Rc::new(ast::TyFactorDetail::None),
        ty: Ty::new_or_get_type_from_name(context, "unknown")
            .map_err(|e| e.convert(None))?,
    });
    Ok(Rc::new(ast::TyExpr {
        parsed: None,
        detail: Rc::new(ast::TyExprDetail::Factor {
            factor: factor.clone(),
        }),
        ty: factor.ty.clone(),
    }))
}

fn hidden_unit_ty_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> Result<Rc<ast::TyExpr<'input>>, Vec<SemanticError<'input>>> {
    let factor = Rc::new(ast::TyFactor {
        parsed: None,
        detail: Rc::new(ast::TyFactorDetail::None),
        ty: Ty::new_or_get_type_from_name(context, "unit")
            .map_err(|e| e.convert(None))?,
    });
    Ok(Rc::new(ast::TyExpr {
        parsed: None,
        detail: Rc::new(ast::TyExprDetail::Factor {
            factor: factor.clone(),
        }),
        ty: factor.ty.clone(),
    }))
}

fn access_ty_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::TyOp, Vec<SemanticError<'input>>> {
    Ok(ast::TyOp::Access)
}

pub fn ty_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyFactor<'input>>,
) -> Result<Rc<ast::TyFactor<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::TyFactorKind::EvalTy { ident } =>
            eval_ty_ty_factor(context, node.clone(), ident.clone()),
    }
}

pub fn access_ty_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyFactor<'input>>,
) -> Result<Rc<ast::TyFactor<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::TyFactorKind::EvalTy { ident } =>
            eval_ty_access_ty_factor(context, node.clone(), ident.clone()),
    }
}

fn eval_ty_ty_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyFactor<'input>>,
    ident: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::TyFactor<'input>>, Vec<SemanticError<'input>>> {
    let ident = self::ident(context, ident)?;
    let ty =
        context.qual_stack.find_ok(|qual|
            Ty::new_or_get_type(context, qual.clone(), ident.name.clone(), Vec::new())
            .or(Ty::new_or_get_qual_from_key(context, qual.pushed_qual(ident.name.clone())))
        ).ok_or(vec![SemanticError::new(Some(node.slice), format!("Specified qualifier `{}` not found", ident.name))])?;
    Ok(Rc::new(ast::TyFactor {
        parsed: Some(node),
        detail: Rc::new(ast::TyFactorDetail::EvalTy {
            ident,
        }),
        ty,
    }))
}

fn eval_ty_access_ty_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyFactor<'input>>,
    ident: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::TyFactor<'input>>, Vec<SemanticError<'input>>> {
    let ident = self::ident(context, ident)?;
    let ty =
        Ty::get_from_name(context, "unknown")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::TyFactor {
        parsed: Some(node),
        detail: Rc::new(ast::TyFactorDetail::EvalTy {
            ident,
        }),
        ty,
    }))
}

pub fn stats_block<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::StatsBlock<'input>>,
    scope: Scope,
) -> Result<Rc<ast::StatsBlock<'input>>, Vec<SemanticError<'input>>> {
    context.qual_stack.push_scope(context, scope);
    let stats =
        node.stats.iter()
        .map(|x| stat(context, x.clone()))
        .collect::<Result<_, _>>()?;
    let ret = match &node.ret {
        Some(x) => expr(context, x.clone())?,
        None => hidden_unit_expr(context)?,
    };
    context.qual_stack.pop();
    Ok(Rc::new(ast::StatsBlock {
        parsed: Some(node),
        stats,
        ret,
    }))
}

pub fn stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Stat<'input>>,
) -> Result<Rc<ast::Stat<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::StatKind::Return { return_keyword: _, expr } =>
            return_stat(context, node.clone(), expr.clone()),
        parser::ast::StatKind::Continue { continue_keyword: _ } =>
            continue_stat(context, node),
        parser::ast::StatKind::Break { break_keyword: _ } =>
            break_stat(context, node),
        parser::ast::StatKind::VarBind { var_bind } =>
            var_bind_stat(context, node.clone(), var_bind.clone()),
        parser::ast::StatKind::FnBind { fn_bind } =>
            fn_bind_stat(context, node.clone(), fn_bind.clone()),
        parser::ast::StatKind::Expr { expr } =>
            expr_stat(context, node.clone(), expr.clone()),
    }
}

fn return_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Stat<'input>>,
    expr: Option<Rc<parser::ast::Expr<'input>>>,
) -> Result<Rc<ast::Stat<'input>>, Vec<SemanticError<'input>>> {
    let expr = match expr {
        Some(x) => self::expr(context, x)?,
        None => hidden_unit_expr(context)?,
    };
    Ok(Rc::new(ast::Stat {
        parsed: Some(node),
        detail: Rc::new(ast::StatDetail::Return {
            expr,
        }),
    }))
}

fn continue_stat<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<parser::ast::Stat<'input>>,
) -> Result<Rc<ast::Stat<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::Stat {
        parsed: Some(node),
        detail: Rc::new(ast::StatDetail::Continue),
    }))
}

fn break_stat<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<parser::ast::Stat<'input>>,
) -> Result<Rc<ast::Stat<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::Stat {
        parsed: Some(node),
        detail: Rc::new(ast::StatDetail::Break),
    }))
}

fn var_bind_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Stat<'input>>,
    var_bind: Rc<parser::ast::VarBind<'input>>,
) -> Result<Rc<ast::Stat<'input>>, Vec<SemanticError<'input>>> {
    let var_bind = self::var_bind(context, var_bind)?;
    Ok(Rc::new(ast::Stat {
        parsed: Some(node),
        detail: Rc::new(ast::StatDetail::VarBind {
            var_bind,
        }),
    }))
}

fn fn_bind_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Stat<'input>>,
    fn_bind: Rc<parser::ast::FnBind<'input>>,
) -> Result<Rc<ast::Stat<'input>>, Vec<SemanticError<'input>>> {
    let fn_bind = self::fn_bind(context, fn_bind)?;
    Ok(Rc::new(ast::Stat {
        parsed: Some(node),
        detail: Rc::new(ast::StatDetail::FnBind {
            fn_bind,
        }),
    }))
}

fn expr_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Stat<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Stat<'input>>, Vec<SemanticError<'input>>> {
    let expr = self::expr(context, expr)?;
    let expr = expr.release(context);
    Ok(Rc::new(ast::Stat {
        parsed: Some(node),
        detail: Rc::new(ast::StatDetail::Expr {
            expr,
        }),
    }))
}

pub fn expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::RetainedExpr<'input>>, Vec<SemanticError<'input>>> {
    construct_expr_tree(context, node)
}

fn hidden_unit_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> Result<Rc<ast::RetainedExpr<'input>>, Vec<SemanticError<'input>>> {
    let ty =
        Ty::get_from_name(context, "unit")
        .map_err(|e| e.convert(None))?;
    let factor = Rc::new(ast::Factor {
        parsed: None,
        detail: Rc::new(ast::FactorDetail::None),
        ty: ty.clone(),
        data: RefCell::new(None),
    });
    let term = Rc::new(ast::Term {
        parsed: None,
        detail: Rc::new(ast::TermDetail::Factor { factor }),
        ty,
        tmp_vars: Vec::new(),
        data: RefCell::new(None),
    });
    Ok(Rc::new(ast::RetainedExpr::new(
        Rc::new(ast::Expr {
            parsed: None,
            detail: Rc::new(ast::ExprDetail::Term {
                term: term.clone(),
            }),
            ty: term.ty.clone(),
            tmp_vars: Vec::new(),
            data: RefCell::new(None),
        })
    )))
}

fn construct_expr_tree<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::RetainedExpr<'input>>, Vec<SemanticError<'input>>> {
    let mut terms = Vec::new();
    let mut ops = VecDeque::new();
    let prefix_ops =
        node.term.prefix_ops.iter()
        .map(|x| term_prefix_op(context, x.clone()))
        .collect::<Result<_, _>>()?;
    let term = self::term(context, node.term.clone())?;
    terms.push((prefix_ops, term));

    for parser_op in &node.term_ops {
        let (infix_op, prefix_ops, term) =
            match parser_op.kind.as_ref() {
                parser::ast::TermOpKind::CastOp { as_keyword: _, ty_expr } =>
                    cast_op_term(context, parser_op.clone(), ty_expr.clone())?,
                parser::ast::TermOpKind::InfixOp { op_code, term } =>
                    infix_op_term(context, parser_op.clone(), op_code.clone(), term.clone())?,
                parser::ast::TermOpKind::Assign { term } =>
                    assign_op_term(context, parser_op.clone(), term.clone())?,
            };
        ops.push_back(infix_op);
        terms.push((prefix_ops, term));
    }

    let exprs = terms.into_iter().map(|(prefix_ops, term)| {
        let term = term.release(context);
        let mut expr = Rc::new(ast::RetainedExpr::new(
            Rc::new(ast::Expr {
                parsed: Some(node.clone()),
                detail: Rc::new(ast::ExprDetail::Term { term: term.clone() }),
                ty: term.ty.clone(),
                tmp_vars: Vec::new(),
                data: term.data.clone(),
            })
        ));
        for op in prefix_ops.iter().rev() {
            let tmp_vars =
                Var::retain_term_prefix_op_tmp_vars(context, op, term.ty.clone())
                .map_err(|e| e.convert(Some(node.slice)))?;
            let op_methods =
                Method::get_term_prefix_op_methods(context, op, term.ty.clone())
                .map_err(|e| e.convert(Some(node.slice)))?;
            let op_literals =
                Literal::new_or_get_term_prefix_op_literals(context, op, term.ty.clone())
                .map_err(|e| e.convert(Some(node.slice)))?;
            let op_kind = OperationKind::TermPrefixOp(op.clone());
            let operation = Operation::new_or_get(context, term.ty.clone(), op_kind, op_methods, op_literals);
            expr = Rc::new(ast::RetainedExpr::new(
                Rc::new(ast::Expr {
                    parsed: Some(node.clone()),
                    detail: Rc::new(ast::ExprDetail::PrefixOp { op: op.clone(), expr: expr.release(context), operation }),
                    ty: term.ty.clone(),
                    tmp_vars,
                    data: term.data.clone(),
                })
            ));
        }
        Ok::<Rc<ast::RetainedExpr<'input>>, Vec<SemanticError<'input>>>(expr)
    })
    .collect::<Result<Vec<_>, _>>()?;

    expr_tree(context, node, VecDeque::from(exprs), ops)
}

fn cast_op_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _node: Rc<parser::ast::TermOp<'input>>,
    ty_expr: Rc<parser::ast::TyExpr<'input>>,
) -> Result<(ast::TermInfixOp, Vec<ast::TermPrefixOp>, Rc<ast::RetainedTerm<'input>>), Vec<SemanticError<'input>>> {
    let infix_op = term_cast_op(context)?;
    let factor = ty_expr_factor(context, ty_expr)?;
    let term = Rc::new(ast::RetainedTerm::new(Rc::new(ast::Term {
        parsed: None,
        detail: Rc::new(ast::TermDetail::Factor { factor: factor.clone() }),
        ty: factor.ty.clone(),
        tmp_vars: Vec::new(),
        data: factor.data.clone(),
    })));
    Ok((infix_op, Vec::new(), term))
}

fn infix_op_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _node: Rc<parser::ast::TermOp<'input>>,
    op_code: Rc<lexer::ast::OpCode<'input>>,
    term: Rc<parser::ast::Term<'input>>,
) -> Result<(ast::TermInfixOp, Vec<ast::TermPrefixOp>, Rc<ast::RetainedTerm<'input>>), Vec<SemanticError<'input>>> {
    let infix_op = term_infix_op(context, op_code)?;
    let prefix_ops =
        term.prefix_ops.iter()
        .map(|x| term_prefix_op(context, x.clone()))
        .collect::<Result<_, _>>()?;
    let term = self::term(context, term)?;
    Ok((infix_op, prefix_ops, term))
}

fn assign_op_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _node: Rc<parser::ast::TermOp<'input>>,
    term: Rc<parser::ast::Term<'input>>,
) -> Result<(ast::TermInfixOp, Vec<ast::TermPrefixOp>, Rc<ast::RetainedTerm<'input>>), Vec<SemanticError<'input>>> {
    let infix_op = term_assign_op(context)?;
    let prefix_ops =
        term.prefix_ops.iter()
        .map(|x| term_prefix_op(context, x.clone()))
        .collect::<Result<_, _>>()?;
    let term = self::term(context, term)?;
    Ok((infix_op, prefix_ops, term))
}

fn term_prefix_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<lexer::ast::OpCode<'input>>,
) -> Result<ast::TermPrefixOp, Vec<SemanticError<'input>>> {
    match node.kind {
        lexer::ast::OpCodeKind::Plus =>
            Ok(ast::TermPrefixOp::Plus),
        lexer::ast::OpCodeKind::Minus =>
            Ok(ast::TermPrefixOp::Minus),
        lexer::ast::OpCodeKind::Bang =>
            Ok(ast::TermPrefixOp::Bang),
        lexer::ast::OpCodeKind::Tilde =>
            Ok(ast::TermPrefixOp::Tilde),
        _ =>
            panic!("Illegal state"),
    }
}

fn term_cast_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::TermInfixOp, Vec<SemanticError<'input>>> {
    Ok(ast::TermInfixOp::CastOp)
}

fn term_infix_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<lexer::ast::OpCode<'input>>,
) -> Result<ast::TermInfixOp, Vec<SemanticError<'input>>> {
    match node.kind {
        lexer::ast::OpCodeKind::Star =>
            Ok(ast::TermInfixOp::Mul),
        lexer::ast::OpCodeKind::Div =>
            Ok(ast::TermInfixOp::Div),
        lexer::ast::OpCodeKind::Percent =>
            Ok(ast::TermInfixOp::Mod),
        lexer::ast::OpCodeKind::Plus =>
            Ok(ast::TermInfixOp::Add),
        lexer::ast::OpCodeKind::Minus =>
            Ok(ast::TermInfixOp::Sub),
        lexer::ast::OpCodeKind::LeftShift =>
            Ok(ast::TermInfixOp::LeftShift),
        lexer::ast::OpCodeKind::RightShift =>
            Ok(ast::TermInfixOp::RightShift),
        lexer::ast::OpCodeKind::Lt =>
            Ok(ast::TermInfixOp::Lt),
        lexer::ast::OpCodeKind::Gt =>
            Ok(ast::TermInfixOp::Gt),
        lexer::ast::OpCodeKind::Le =>
            Ok(ast::TermInfixOp::Le),
        lexer::ast::OpCodeKind::Ge =>
            Ok(ast::TermInfixOp::Ge),
        lexer::ast::OpCodeKind::Eq =>
            Ok(ast::TermInfixOp::Eq),
        lexer::ast::OpCodeKind::Ne =>
            Ok(ast::TermInfixOp::Ne),
        lexer::ast::OpCodeKind::Amp =>
            Ok(ast::TermInfixOp::BitAnd),
        lexer::ast::OpCodeKind::Caret =>
            Ok(ast::TermInfixOp::BitXor),
        lexer::ast::OpCodeKind::Pipe =>
            Ok(ast::TermInfixOp::BitOr),
        lexer::ast::OpCodeKind::And =>
            Ok(ast::TermInfixOp::And),
        lexer::ast::OpCodeKind::Or =>
            Ok(ast::TermInfixOp::Or),
        lexer::ast::OpCodeKind::Coalescing =>
            Ok(ast::TermInfixOp::Coalescing),
        lexer::ast::OpCodeKind::RightPipeline =>
            Ok(ast::TermInfixOp::RightPipeline),
        lexer::ast::OpCodeKind::LeftPipeline =>
            Ok(ast::TermInfixOp::LeftPipeline),
        _ =>
            panic!("Illegal state"),
    }
}

fn term_assign_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::TermInfixOp, Vec<SemanticError<'input>>> {
    Ok(ast::TermInfixOp::Assign)
}

pub fn term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
) -> Result<Rc<ast::RetainedTerm<'input>>, Vec<SemanticError<'input>>> {
    construct_term_tree(context, node)
}

fn construct_term_tree<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
) -> Result<Rc<ast::RetainedTerm<'input>>, Vec<SemanticError<'input>>> {
    let mut factors = Vec::new();
    let mut ops = VecDeque::new();
    let factor = self::factor(context, node.factor.clone())?;
    factors.push(factor);

    for parser_op in &node.factor_ops {
        let (infix_op, factor) =
            match parser_op.kind.as_ref() {
                parser::ast::FactorOpKind::TyAccess { op_code: _, factor } =>
                    ty_access_op_factor(context, parser_op.clone(), factor.clone())?,
                parser::ast::FactorOpKind::Access { op_code, factor } =>
                    access_op_factor(context, parser_op.clone(), op_code.clone(), factor.clone())?,
                parser::ast::FactorOpKind::EvalFn { arg_exprs } =>
                    eval_fn_op_factor(context, parser_op.clone(), arg_exprs)?,
                parser::ast::FactorOpKind::EvalSpreadFn { expr } =>
                    eval_spread_fn_op_factor(context, parser_op.clone(), expr.clone())?,
                parser::ast::FactorOpKind::EvalKey { expr } =>
                    eval_key_op_factor(context, parser_op.clone(), expr.clone())?,
            };
        ops.push_back(infix_op);
        factors.push(factor);
    }

    let terms = factors.into_iter().map(|factor|
        Rc::new(ast::RetainedTerm::new(
            Rc::new(ast::Term {
                parsed: Some(node.clone()),
                detail: Rc::new(ast::TermDetail::Factor { factor: factor.clone() }),
                ty: factor.ty.clone(),
                tmp_vars: Vec::new(),
                data: factor.data.clone(),
            }),
        ))
    )
    .collect();

    expr_tree(context, node, terms, ops)
}

fn ty_access_op_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _node: Rc<parser::ast::FactorOp<'input>>,
    factor: Rc<parser::ast::Factor<'input>>,
) -> Result<(ast::FactorInfixOp, Rc<ast::Factor<'input>>), Vec<SemanticError<'input>>> {
    let op = ty_access_op(context)?;
    let factor = self::ty_access_factor(context, factor)?;
    Ok((op, factor))
}

fn access_op_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _node: Rc<parser::ast::FactorOp<'input>>,
    op_code: Rc<lexer::ast::OpCode<'input>>,
    factor: Rc<parser::ast::Factor<'input>>,
) -> Result<(ast::FactorInfixOp, Rc<ast::Factor<'input>>), Vec<SemanticError<'input>>> {
    let op = access_op(context, op_code)?;
    let factor = self::access_factor(context, factor)?;
    Ok((op, factor))
}

fn eval_fn_op_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _node: Rc<parser::ast::FactorOp<'input>>,
    arg_exprs: &Vec<Rc<parser::ast::ArgExpr<'input>>>,
) -> Result<(ast::FactorInfixOp, Rc<ast::Factor<'input>>), Vec<SemanticError<'input>>> {
    let op = eval_fn_op(context)?;
    let factor = apply_fn_factor(context, arg_exprs)?;
    Ok((op, factor))
}

fn eval_spread_fn_op_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _node: Rc<parser::ast::FactorOp<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<(ast::FactorInfixOp, Rc<ast::Factor<'input>>), Vec<SemanticError<'input>>> {
    let op = eval_spread_fn_op(context)?;
    let factor = apply_spread_fn_factor(context, expr)?;
    Ok((op, factor))
}

fn eval_key_op_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    _node: Rc<parser::ast::FactorOp<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<(ast::FactorInfixOp, Rc<ast::Factor<'input>>), Vec<SemanticError<'input>>> {
    let op = eval_key_op(context)?;
    let factor = apply_key_factor(context, expr)?;
    Ok((op, factor))
}

fn ty_access_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::FactorInfixOp, Vec<SemanticError<'input>>> {
    Ok(ast::FactorInfixOp::TyAccess)
}

fn access_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<lexer::ast::OpCode>,
) -> Result<ast::FactorInfixOp, Vec<SemanticError<'input>>> {
    match node.kind {
        lexer::ast::OpCodeKind::Dot =>
            Ok(ast::FactorInfixOp::Access),
        lexer::ast::OpCodeKind::CoalescingAccess =>
            Ok(ast::FactorInfixOp::CoalescingAccess),
        _ =>
            panic!("Illegal state"),
    }
}

fn eval_fn_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::FactorInfixOp, Vec<SemanticError<'input>>> {
    Ok(ast::FactorInfixOp::EvalFn)
}

fn eval_spread_fn_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::FactorInfixOp, Vec<SemanticError<'input>>> {
    Ok(ast::FactorInfixOp::EvalSpreadFn)
}

fn eval_key_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::FactorInfixOp, Vec<SemanticError<'input>>> {
    Ok(ast::FactorInfixOp::EvalKey)
}

pub fn factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::FactorKind::Block { stats } =>
            block_factor(context, node.clone(), stats.clone()),
        parser::ast::FactorKind::Paren { expr } =>
            paren_factor(context, node.clone(), expr.clone()),
        parser::ast::FactorKind::Tuple { exprs } =>
            tuple_factor(context, node.clone(), exprs),
        parser::ast::FactorKind::ArrayCtor { iter_expr } =>
            array_ctor_factor(context, node.clone(), iter_expr.clone()),
        parser::ast::FactorKind::Literal { literal } =>
            literal_factor(context, node.clone(), literal.clone()),
        parser::ast::FactorKind::ThisLiteral { literal } =>
            this_literal_factor(context, node.clone(), literal.clone()),
        parser::ast::FactorKind::InterpolatedString { interpolated_string } =>
            interpolated_string_factor(context, node.clone(), interpolated_string.clone()),
        parser::ast::FactorKind::EvalVar { ident } =>
            eval_var_factor(context, node.clone(), ident.clone()),
        parser::ast::FactorKind::LetInBind { var_bind, in_keyword: _, expr } =>
            let_in_bind_factor(context, node.clone(), var_bind.clone(), expr.clone()),
        parser::ast::FactorKind::If { if_keyword: _, condition, if_part, else_part } =>
            if_factor(context, node.clone(), condition.clone(), if_part.clone(), else_part.clone()),
        parser::ast::FactorKind::While { while_keyword: _, condition, stats } =>
            while_factor(context, node.clone(), condition.clone(), stats.clone()),
        parser::ast::FactorKind::Loop { loop_keyword: _, stats } =>
            loop_factor(context, node.clone(), stats.clone()),
        parser::ast::FactorKind::For { for_binds, stats } =>
            for_factor(context, node.clone(), for_binds, stats.clone()),
        parser::ast::FactorKind::Closure { var_decl, expr } =>
            closure_factor(context, node.clone(), var_decl.clone(), expr.clone()),
    }
}

fn ty_access_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::FactorKind::EvalVar { ident } =>
            eval_var_ty_access_factor(context, node.clone(), ident.clone()),
        _ =>
            Err(vec![SemanticError::new(Some(node.slice), "Illegal use of type access op `::`".to_owned())])
    }
}

fn access_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::FactorKind::EvalVar { ident } =>
            eval_var_access_factor(context, node.clone(), ident.clone()),
        _ =>
            Err(vec![SemanticError::new(Some(node.slice), "Illegal use of access op `.`".to_owned())])
    }
}

fn block_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    stats: Rc<parser::ast::StatsBlock<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let scope = Scope::Block(context.block_id_factory.next_id());
    let stats = stats_block(context, stats, scope)?;
    let ret = stats.ret.release(context);
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::Block {
            stats: stats.clone(),
        }),
        ty: ret.ty.clone(),
        data: ret.data.clone(),
    }))
}

fn paren_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let expr = self::expr(context, expr)?;
    let expr = expr.release(context);
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::Paren {
            expr: expr.clone(),
        }),
        ty: expr.ty.clone(),
        data: expr.data.clone(),
    }))
}

fn tuple_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    exprs: &Vec<Rc<parser::ast::Expr<'input>>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let exprs =
        exprs.iter()
        .map(|x| expr(context, x.clone()))
        .collect::<Result<Vec<_>, _>>()?;
    let exprs = exprs.into_iter().map(|x| x.release(context)).collect::<Vec<_>>();
    let ty_keys = exprs.iter().map(|x| x.ty.to_key()).collect();
    let ty =
        Ty::new_or_get_tuple_from_keys(context, ty_keys)
        .map_err(|e| e.convert(None))?;
    let data =
        exprs.iter()
        .filter_map(|x| x.data.borrow().clone())
        .flat_map(|x| x.into_iter())
        .collect::<Vec<_>>();
    let data = if data.len() == 0 { None } else { Some(data) };
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::Tuple {
            exprs,
        }),
        ty,
        data: RefCell::new(data),
    }))
}

fn array_ctor_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    iter_expr: Option<Rc<parser::ast::IterExpr<'input>>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let iter_expr = match iter_expr {
        Some(x) => self::iter_expr(context, x)?,
        None => empty_iter_expr(context)?,
    };
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::ArrayCtor {
            iter_expr,
        }),
        ty: Ty::get_from_name(context, "array")
            .map_err(|e| e.convert(None))?, // TODO
        data: RefCell::new(None), // TODO
    }))
}

fn literal_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    literal: Rc<lexer::ast::Literal<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let literal = self::literal(context, literal)?;
    let data = Some(vec![DataLabel::new(DataLabelKind::Literal(literal.clone()))]);
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::Literal {
            literal: literal.clone(),
        }),
        ty: literal.ty.clone(),
        data: RefCell::new(data),
    }))
}

fn this_literal_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    _literal: Rc<lexer::ast::Literal<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let ty =
        Ty::get_from_name(context, "gameobject")
        .map_err(|e| e.convert(None))?;
    let literal = ThisLiteral::new_or_get(context, ty.clone());
    let data = Some(vec![DataLabel::new(DataLabelKind::ThisLiteral(literal.clone()))]);
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::ThisLiteral {
            literal,
        }),
        ty,
        data: RefCell::new(data),
    }))
}

fn interpolated_string_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    interpolated_string: Rc<lexer::ast::InterpolatedString<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let interpolated_string = self::interpolated_string(context, interpolated_string)?;
    let ty =
        Ty::get_from_name(context, "string")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::InterpolatedString {
            interpolated_string,
        }),
        ty,
        data: RefCell::new(None), // TODO
    }))
}

fn eval_var_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    ident: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let ident = self::ident(context, ident)?;
    let var =
        context.qual_stack.find_ok(|qual|
            Var::get(context, qual, ident.name.clone())
        ).ok_or(vec![SemanticError::new(Some(node.slice), format!("Specified variable `{}` not found", ident.name))]);
    let ty = match &var {
        Ok(x) =>
            x.ty.borrow().clone(),
        Err(e) =>
            context.qual_stack.find_ok(|qual|
                Ty::new_or_get_type(context, qual.clone(), ident.name.clone(), Vec::new())
                .or(Ty::new_or_get_qual_from_key(context, qual.pushed_qual(ident.name.clone())))
            ).ok_or(e.clone())?,
    };
    let data = var.clone().map(|x| vec![DataLabel::new(DataLabelKind::Var(x))]).ok();
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::EvalVar {
            ident,
            var: RefCell::new(var.ok()),
            getter: RefCell::new(None),
        }),
        ty,
        data: RefCell::new(data),
    }))
}

fn eval_var_ty_access_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    ident: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let ident = self::ident(context, ident)?;
    let ty =
        Ty::get_from_name(context, "unknown")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::EvalVar {
            ident,
            var: RefCell::new(None),
            getter: RefCell::new(None),
        }),
        ty,
        data: RefCell::new(None),
    }))
}

fn eval_var_access_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    ident: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let ident = self::ident(context, ident)?;
    let ty =
        Ty::get_from_name(context, "unknown")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::EvalVar {
            ident,
            var: RefCell::new(None),
            getter: RefCell::new(None),
        }),
        ty,
        data: RefCell::new(None),
    }))
}

fn let_in_bind_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    var_bind: Rc<parser::ast::VarBind<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let scope = Scope::Block(context.block_id_factory.next_id());
    context.qual_stack.push_scope(context, scope);
    let var_bind = self::var_bind(context, var_bind)?;
    let expr = self::expr(context, expr)?;
    let expr = expr.release(context);
    context.qual_stack.pop();
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::LetInBind {
            var_bind,
            expr: expr.clone(),
        }),
        ty: expr.ty.clone(),
        data: expr.data.clone(),
    }))
}

fn if_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    condition: Rc<parser::ast::Expr<'input>>,
    if_part: Rc<parser::ast::StatsBlock<'input>>,
    else_part: Option<(Rc<lexer::ast::Keyword<'input>>, Rc<parser::ast::StatsBlock<'input>>)>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let condition = expr(context, condition)?;
    let condition = condition.release(context);
    let if_scope = Scope::Block(context.block_id_factory.next_id());
    let if_part = stats_block(context, if_part, if_scope)?;
    let else_part = match else_part {
        Some((_, stats)) => {
            let else_scope = Scope::Block(context.block_id_factory.next_id());
            Some(stats_block(context, stats, else_scope)?)
        },
        None => None,
    };
    let ret = if_part.ret.release(context); // TODO
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::If {
            condition,
            if_part: if_part.clone(),
            else_part,
        }),
        ty: ret.ty.clone(), // TODO
        data: ret.data.clone(), // TODO
    }))
}

fn while_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    condition: Rc<parser::ast::Expr<'input>>,
    stats: Rc<parser::ast::StatsBlock<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let condition = expr(context, condition)?;
    let condition = condition.release(context);
    let scope = Scope::Loop(context.loop_id_factory.next_id());
    let stats = stats_block(context, stats, scope)?;
    let ty =
        Ty::get_from_name(context, "unit")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::While {
            condition,
            stats,
        }),
        ty,
        data: RefCell::new(None),
    }))
}

fn loop_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    stats: Rc<parser::ast::StatsBlock<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let scope = Scope::Loop(context.loop_id_factory.next_id());
    let stats = stats_block(context, stats, scope)?;
    let ty =
        Ty::get_from_name(context, "unit")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::Loop {
            stats,
        }),
        ty,
        data: RefCell::new(None),
    }))
}

fn for_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    for_binds: &Vec<(Rc<lexer::ast::Keyword<'input>>, Rc<parser::ast::ForBind<'input>>)>,
    stats: Rc<parser::ast::StatsBlock<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let for_binds =
        for_binds.iter()
        .map(|x| for_bind(context, x.1.clone()))
        .collect::<Result<_, _>>()?;
    let scope = Scope::Loop(context.loop_id_factory.next_id());
    let stats = stats_block(context, stats, scope)?;
    let ty =
        Ty::get_from_name(context, "unit")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::For {
            for_binds,
            stats,
        }),
        ty,
        data: RefCell::new(None),
    }))
}

fn closure_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Factor<'input>>,
    var_decl: Rc<parser::ast::VarDecl<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let var_decl = self::var_decl(context, var_decl)?;
    let expr = self::expr(context, expr)?;
    let expr = expr.release(context);
    Ok(Rc::new(ast::Factor {
        parsed: Some(node),
        detail: Rc::new(ast::FactorDetail::Closure {
            var_decl,
            expr,
        }),
        ty: Ty::get_from_name(context, "closure")
            .map_err(|e| e.convert(None))?, // TODO
        data: RefCell::new(None),
    }))
}

fn ty_expr_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    ty_expr: Rc<parser::ast::TyExpr<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let ty_expr = self::ty_expr(context, ty_expr)?;
    Ok(Rc::new(ast::Factor {
        parsed: None,
        detail: Rc::new(ast::FactorDetail::TyExpr {
            ty_expr: ty_expr.clone(),
        }),
        ty: ty_expr.ty.clone(),
        data: RefCell::new(None),
    }))
}

fn apply_fn_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    arg_exprs: &Vec<Rc<parser::ast::ArgExpr<'input>>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let args =
        arg_exprs.iter()
        .map(|x| arg_expr(context, x.clone()))
        .collect::<Result<_, _>>()?;
    let ty =
        Ty::get_from_name(context, "never")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Factor {
        parsed: None,
        detail: Rc::new(ast::FactorDetail::ApplyFn {
            args,
            as_fn: RefCell::new(None),
        }),
        ty,
        data: RefCell::new(None),
    }))
}

fn apply_spread_fn_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let arg = self::expr(context, expr)?;
    let arg = arg.release(context);
    let ty =
        Ty::get_from_name(context, "never")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Factor {
        parsed: None,
        detail: Rc::new(ast::FactorDetail::ApplySpreadFn {
            arg,
        }),
        ty,
        data: RefCell::new(None),
    }))
}

fn apply_key_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Factor<'input>>, Vec<SemanticError<'input>>> {
    let key = self::expr(context, expr)?;
    let key = key.release(context);
    let ty =
        Ty::get_from_name(context, "never")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Factor {
        parsed: None,
        detail: Rc::new(ast::FactorDetail::ApplyKey {
            key,
        }),
        ty,
        data: RefCell::new(None),
    }))
}

pub fn iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::IterExpr<'input>>,
) -> Result<Rc<ast::IterExpr<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::IterExprKind::Range { left, right } =>
            range_iter_expr(context, node.clone(), left.clone(), right.clone()),
        parser::ast::IterExprKind::SteppedRange { left, right, step } =>
            stepped_range_iter_expr(context, node.clone(), left.clone(), right.clone(), step.clone()),
        parser::ast::IterExprKind::Spread { expr } =>
            spread_iter_expr(context, node.clone(), expr.clone()),
        parser::ast::IterExprKind::Elements { exprs } =>
            elements_iter_expr(context, node.clone(), exprs),
    }
}

fn empty_iter_expr<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<Rc<ast::IterExpr<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::IterExpr { parsed: None, detail: Rc::new(ast::IterExprDetail::Empty) }))
}

fn range_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::IterExpr<'input>>,
    left: Rc<parser::ast::Expr<'input>>,
    right: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::IterExpr<'input>>, Vec<SemanticError<'input>>> {
    let left = expr(context, left)?;
    let right = expr(context, right)?;
    let left = left.release(context);
    let right = right.release(context);
    Ok(Rc::new(ast::IterExpr {
        parsed: Some(node),
        detail: Rc::new(ast::IterExprDetail::Range {
            left,
            right,
        }),
    }))
}

fn stepped_range_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::IterExpr<'input>>,
    left: Rc<parser::ast::Expr<'input>>,
    right: Rc<parser::ast::Expr<'input>>,
    step: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::IterExpr<'input>>, Vec<SemanticError<'input>>> {
    let left = expr(context, left)?;
    let right = expr(context, right)?;
    let step = expr(context, step)?;
    let left = left.release(context);
    let right = right.release(context);
    let step = step.release(context);
    Ok(Rc::new(ast::IterExpr {
        parsed: Some(node),
        detail: Rc::new(ast::IterExprDetail::SteppedRange {
            left,
            right,
            step,
        }),
    }))
}

fn spread_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::IterExpr<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::IterExpr<'input>>, Vec<SemanticError<'input>>> {
    let expr = self::expr(context, expr)?;
    let expr = expr.release(context);
    Ok(Rc::new(ast::IterExpr {
        parsed: Some(node),
        detail: Rc::new(ast::IterExprDetail::Spread {
            expr,
        }),
    }))
}

fn elements_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::IterExpr<'input>>,
    exprs: &Vec<Rc<parser::ast::Expr<'input>>>,
) -> Result<Rc<ast::IterExpr<'input>>, Vec<SemanticError<'input>>> {
    let exprs =
        exprs.iter()
        .map(|x| expr(context, x.clone()))
        .collect::<Result<Vec<_>, _>>()?;
    let exprs = exprs.into_iter().map(|x| x.release(context)).collect();
    Ok(Rc::new(ast::IterExpr {
        parsed: Some(node),
        detail: Rc::new(ast::IterExprDetail::Elements {
            exprs,
        }),
    }))
}

pub fn arg_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::ArgExpr<'input>>,
) -> Result<Rc<ast::ArgExpr<'input>>, Vec<SemanticError<'input>>> {
    let mut_attr = mut_attr(context, node.mut_attr.clone())?;
    let expr = expr(context, node.expr.clone())?;
    Ok(Rc::new(ast::ArgExpr {
        parsed: Some(node),
        mut_attr,
        expr,
    }))
}

pub fn for_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::ForBind<'input>>,
) -> Result<Rc<ast::ForBind<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::ForBindKind::Let { let_keyword: _, var_decl, for_iter_expr } =>
            let_for_bind(context, node.clone(), var_decl.clone(), for_iter_expr.clone()),
        parser::ast::ForBindKind::Assign { left, for_iter_expr } =>
            assign_for_bind(context, node.clone(), left.clone(), for_iter_expr.clone()),
    }
}

fn let_for_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::ForBind<'input>>,
    var_decl: Rc<parser::ast::VarDecl<'input>>,
    for_iter_expr: Rc<parser::ast::ForIterExpr<'input>>,
) -> Result<Rc<ast::ForBind<'input>>, Vec<SemanticError<'input>>> {
    let var_decl = self::var_decl(context, var_decl)?;
    let for_iter_expr = self::for_iter_expr(context, for_iter_expr)?;
    Ok(Rc::new(ast::ForBind {
        parsed: Some(node),
        detail: Rc::new(ast::ForBindDetail::Let {
            var_decl,
            for_iter_expr,
        }),
    }))
}

fn assign_for_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::ForBind<'input>>,
    left: Rc<parser::ast::Expr<'input>>,
    for_iter_expr: Rc<parser::ast::ForIterExpr<'input>>,
) -> Result<Rc<ast::ForBind<'input>>, Vec<SemanticError<'input>>> {
    let left = self::expr(context, left)?;
    let for_iter_expr = self::for_iter_expr(context, for_iter_expr)?;
    let left = left.release(context);
    Ok(Rc::new(ast::ForBind {
        parsed: Some(node),
        detail: Rc::new(ast::ForBindDetail::Assign {
            left,
            for_iter_expr,
        }),
    }))
}

pub fn for_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::ForIterExpr<'input>>,
) -> Result<Rc<ast::ForIterExpr<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::ForIterExprKind::Range { left, right } =>
            range_for_iter_expr(context, node.clone(), left.clone(), right.clone()),
        parser::ast::ForIterExprKind::SteppedRange { left, right, step } =>
            stepped_range_for_iter_expr(context, node.clone(), left.clone(), right.clone(), step.clone()),
        parser::ast::ForIterExprKind::Spread { expr } =>
            spread_for_iter_expr(context, node.clone(), expr.clone()),
    }
}

fn range_for_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::ForIterExpr<'input>>,
    left: Rc<parser::ast::Expr<'input>>,
    right: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::ForIterExpr<'input>>, Vec<SemanticError<'input>>> {
    let left = expr(context, left)?;
    let right = expr(context, right)?;
    let left = left.release(context);
    let right = right.release(context);
    Ok(Rc::new(ast::ForIterExpr {
        parsed: Some(node),
        detail: Rc::new(ast::ForIterExprDetail::Range {
            left,
            right,
        }),
    }))
}

fn stepped_range_for_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::ForIterExpr<'input>>,
    left: Rc<parser::ast::Expr<'input>>,
    right: Rc<parser::ast::Expr<'input>>,
    step: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::ForIterExpr<'input>>, Vec<SemanticError<'input>>> {
    let left = expr(context, left)?;
    let right = expr(context, right)?;
    let step = expr(context, step)?;
    let left = left.release(context);
    let right = right.release(context);
    let step = step.release(context);
    Ok(Rc::new(ast::ForIterExpr {
        parsed: Some(node),
        detail: Rc::new(ast::ForIterExprDetail::SteppedRange {
            left,
            right,
            step,
        }),
    }))
}

fn spread_for_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::ForIterExpr<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::ForIterExpr<'input>>, Vec<SemanticError<'input>>> {
    let expr = self::expr(context, expr)?;
    let expr = expr.release(context);
    Ok(Rc::new(ast::ForIterExpr {
        parsed: Some(node),
        detail: Rc::new(ast::ForIterExprDetail::Spread {
            expr,
        }),
    }))
}

pub fn ident<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::Ident<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::Ident {
        parsed: Some(node.clone()),
        name: node.slice.to_owned(),
    }))
}

pub fn literal<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<lexer::ast::Literal<'input>>,
) -> Result<Rc<Literal>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        lexer::ast::LiteralKind::Unit { left, right: _ } =>
            Literal::new_or_get_unit(context)
            .map_err(|e| e.convert(Some(left.slice))),
        lexer::ast::LiteralKind::Null { keyword } =>
            Literal::new_or_get_null(context)
            .map_err(|e| e.convert(Some(keyword.slice))),
        lexer::ast::LiteralKind::Bool { keyword } =>
            Literal::new_or_get_bool(context, (*keyword.slice).to_owned())
            .map_err(|e| e.convert(Some(keyword.slice))),
        lexer::ast::LiteralKind::PureInteger { slice } =>
            Literal::new_or_get_pure_integer(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::DecInteger { slice } =>
            Literal::new_or_get_dec_integer(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::HexInteger { slice } =>
            Literal::new_or_get_hex_integer(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::BinInteger { slice } =>
            Literal::new_or_get_bin_integer(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::RealNumber { slice } =>
            Literal::new_or_get_real_number(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::Character { slice } =>
            Literal::new_or_get_character(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::RegularString { slice } =>
            Literal::new_or_get_regular_string(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::VerbatiumString { slice } =>
            Literal::new_or_get_verbatium_string(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        _ =>
            panic!("Illegal state"),
        
    }
}

pub fn interpolated_string<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<lexer::ast::InterpolatedString<'input>>,
) -> Result<Rc<ast::InterpolatedString<'input>>, Vec<SemanticError<'input>>> {
    let string_parts = node.string_parts.iter().map(|x| (*x).to_owned()).collect();
    let exprs =
        node.exprs.iter()
        .map(|x| expr(context, x.clone()))
        .collect::<Result<Vec<_>, _>>()?;
    let exprs = exprs.into_iter().map(|x| x.release(context)).collect();
    Ok(Rc::new(ast::InterpolatedString {
        parsed: Some(node),
        string_parts,
        exprs,
    }))
}

fn expr_tree<'input: 'context, 'context, ExprTree, SemanticOp, ParserExpr>(
    context: &'context Context<'input>,
    node: Rc<ParserExpr>,
    exprs: VecDeque<Rc<ExprTree>>,
    ops: VecDeque<SemanticOp>,
) -> Result<Rc<ExprTree>, Vec<SemanticError<'input>>>
where
    ExprTree: ast::ExprTree<'input, 'context, SemanticOp, ParserExpr>,
    SemanticOp: Clone + Debug + 'context,
{
    let mut es = exprs.into_iter().collect::<VecDeque<_>>();
    let mut os = ops.into_iter().collect::<VecDeque<_>>();
    for (pred, assoc) in ExprTree::priorities(context) {
        match assoc {
            ast::Assoc::Left =>
                (es, os) = left_assoc(context, node.clone(), pred, es, os)?,
            ast::Assoc::Right =>
                (es, os) = right_assoc(context, node.clone(), pred, es, os)?,
        }
    }
    if es.len() == 1 {
        Ok(es.pop_front().unwrap())
    }
    else {
        panic!("Illegal state")
    }
}

fn left_assoc<'input: 'context, 'context, ExprTree, SemanticOp, ParserExpr>(
    context: &'context Context<'input>,
    node: Rc<ParserExpr>,
    pred: &Box<dyn Fn(&SemanticOp) -> bool>,
    mut exprs: VecDeque<Rc<ExprTree>>,
    ops: VecDeque<SemanticOp>,
) -> Result<(VecDeque<Rc<ExprTree>>, VecDeque<SemanticOp>), Vec<SemanticError<'input>>>
where
    ExprTree: ast::ExprTree<'input, 'context, SemanticOp, ParserExpr>,
    SemanticOp: Clone + Debug,
{
    let expr_0 = exprs.pop_front().unwrap();
    let (mut acc_exprs, acc_ops, expr) =
        ops.into_iter().zip(exprs.into_iter())
        .fold(Ok::<_, Vec<SemanticError>>((VecDeque::new(), VecDeque::new(), expr_0)), |acc, x| {
            let mut acc = acc?;
            if pred(&x.0) {
                let infix_op = ExprTree::infix_op(context, node.clone(), acc.2.clone(), &x.0, x.1.clone())?;
                Ok((acc.0, acc.1, infix_op))
            }
            else {
                acc.0.push_back(acc.2);
                acc.1.push_back(x.0);
                Ok((acc.0, acc.1, x.1))
            }
        })?;
    acc_exprs.push_back(expr);
    Ok((acc_exprs, acc_ops))
}

fn right_assoc<'input: 'context, 'context, ExprTree, SemanticOp, ParserExpr>(
    context: &'context Context<'input>,
    node: Rc<ParserExpr>,
    pred: &Box<dyn Fn(&SemanticOp) -> bool>,
    mut exprs: VecDeque<Rc<ExprTree>>,
    ops: VecDeque<SemanticOp>,
) -> Result<(VecDeque<Rc<ExprTree>>, VecDeque<SemanticOp>), Vec<SemanticError<'input>>>
where
    ExprTree: ast::ExprTree<'input, 'context, SemanticOp, ParserExpr>,
    SemanticOp: Clone + Debug,
{
    let expr_0 = exprs.pop_back().unwrap();
    let (mut acc_exprs, acc_ops, expr) =
        ops.into_iter().rev().zip(exprs.into_iter().rev())
        .fold(Ok::<_, Vec<SemanticError>>((VecDeque::new(), VecDeque::new(), expr_0)), |acc, x| {
            let mut acc = acc?;
            if pred(&x.0) {
                let infix_op = ExprTree::infix_op(context, node.clone(), x.1.clone(), &x.0, acc.2.clone())?;
                Ok((acc.0, acc.1, infix_op))
            }
            else {
                acc.0.push_front(acc.2);
                acc.1.push_front(x.0);
                Ok((acc.0, acc.1, x.1))
            }
        })?;
    acc_exprs.push_front(expr);
    Ok((acc_exprs, acc_ops))
}

impl<'input: 'context, 'context>
    ast::ExprTree<'input, 'context, ast::TyOp, parser::ast::TyExpr<'input>> for ast::TyExpr<'input>
{
    fn priorities(context: &'context Context<'input>) -> &'context Vec<(Box<dyn Fn(&ast::TyOp) -> bool>, ast::Assoc)> {
        &context.semantic_ty_op.priorities
    }

    fn infix_op(
        context: &'context Context<'input>,
        parsed: Rc<parser::ast::TyExpr<'input>>,
        left: Rc<Self>,
        op: &ast::TyOp,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'input>>> {
        match op {
            ast::TyOp::Access =>
                access_ty_infix_op(context, parsed, left, op, right),
        }
    }
}

fn access_ty_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    parsed: Rc<parser::ast::TyExpr<'input>>,
    left: Rc<ast::TyExpr<'input>>,
    op: &ast::TyOp,
    right: Rc<ast::TyExpr<'input>>,
) -> Result<Rc<ast::TyExpr<'input>>, Vec<SemanticError<'input>>> {
    let ast::TyExprDetail::Factor { factor } = right.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` is not a term".to_owned())]);
        };
    let ast::TyFactorDetail::EvalTy { ident } = factor.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` cannot be evaluated".to_owned())]);
        };

    if left.ty.base_eq_with_name("qual") {
        let qual = left.ty.arg_as_qual();
        let ty = Ty::new_or_get_type(context, qual.clone(), ident.name.clone(), Vec::new())
            .or(Ty::new_or_get_qual_from_key(context, qual.pushed_qual(ident.name.clone())))
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        Ok(Rc::new(ast::TyExpr {
            parsed: Some(parsed),
            detail: Rc::new(ast::TyExprDetail::InfixOp {
                left: left.clone(),
                op: op.clone(),
                right: right.clone(),
            }),
            ty,
        }))
    }
    else if left.ty.base_eq_with_name("type") {
        let parent = left.ty.arg_as_type().get_value(context)
            .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
        let qual = Qual::get_from_ty(context, parent)
            .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
        let ty = Ty::new_or_get_type(context, qual.to_key(), ident.name.clone(), Vec::new())
            .map_err(|e| e.convert(right.parsed.clone().map(|x| x.slice)))?;
        Ok(Rc::new(ast::TyExpr {
            parsed: Some(parsed),
            detail: Rc::new(ast::TyExprDetail::InfixOp {
                left: left.clone(),
                op: op.clone(),
                right: right.clone(),
            }),
            ty,
        }))
    }
    else {
        Err(vec![SemanticError::new(None, "Left side of `::` is not a qualifier or a type".to_owned())])
    }
}

impl<'input: 'context, 'context>
    ast::ExprTree<'input, 'context, ast::TermInfixOp, parser::ast::Expr<'input>> for ast::RetainedExpr<'input>
{
    fn priorities(context: &'context Context<'input>) -> &'context Vec<(Box<dyn Fn(&ast::TermInfixOp) -> bool>, ast::Assoc)> {
        &context.semantic_op.term_priorities
    }

    fn infix_op(
        context: &'context Context<'input>,
        parsed: Rc<parser::ast::Expr<'input>>,
        left: Rc<Self>,
        op: &ast::TermInfixOp,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'input>>> {
        match op {
            ast::TermInfixOp::Mul |
            ast::TermInfixOp::Div |
            ast::TermInfixOp::Mod |
            ast::TermInfixOp::Add |
            ast::TermInfixOp::Sub =>
                arithmetic_infix_op(context, parsed, left, op, right),
            _ =>
                panic!("Not implemented")
        }
    }
}

fn arithmetic_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    parsed: Rc<parser::ast::Expr<'input>>,
    left: Rc<ast::RetainedExpr<'input>>,
    op: &ast::TermInfixOp,
    right: Rc<ast::RetainedExpr<'input>>,
) -> Result<Rc<ast::RetainedExpr<'input>>, Vec<SemanticError<'input>>> {
    let left = left.release(context);
    let right = right.release(context);
    let operation = term_infix_operation(context, parsed.clone(), &op, left.ty.clone(), right.ty.clone())?;
    let tmp_vars =
            Var::retain_term_infix_op_tmp_vars(context, &op, left.ty.clone(), right.ty.clone())
            .map_err(|e| e.convert(Some(parsed.slice)))?;
    Ok(Rc::new(ast::RetainedExpr::new(
        Rc::new(ast::Expr {
            parsed: Some(parsed),
            detail: Rc::new(ast::ExprDetail::InfixOp {
                left: left.clone(),
                op: op.clone(),
                right: right.clone(),
                operation,
            }),
            ty: left.ty.clone(),
            tmp_vars,
            data: RefCell::new(None), // TODO
        })
    )))
}

impl<'input: 'context, 'context>
    ast::ExprTree<'input, 'context, ast::FactorInfixOp, parser::ast::Term<'input>> for ast::RetainedTerm<'input>
{
    fn priorities(context: &'context Context<'input>) -> &'context Vec<(Box<dyn Fn(&ast::FactorInfixOp) -> bool>, ast::Assoc)> {
        &context.semantic_op.factor_priorities
    }

    fn infix_op(
        context: &'context Context<'input>,
        parsed: Rc<parser::ast::Term<'input>>,
        left: Rc<Self>,
        op: &ast::FactorInfixOp,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'input>>> {
        match op {
            ast::FactorInfixOp::TyAccess =>
                ty_access_infix_op(context, parsed, left, op, right),
            ast::FactorInfixOp::Access =>
                access_infix_op(context, parsed, left, op, right),
            ast::FactorInfixOp::EvalFn =>
                eval_fn_infix_op(context, parsed, left, op, right),
            _ =>
                panic!("Not implemented")
        }
    }
}

fn ty_access_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    parsed: Rc<parser::ast::Term<'input>>,
    left: Rc<ast::RetainedTerm<'input>>,
    op: &ast::FactorInfixOp,
    right: Rc<ast::RetainedTerm<'input>>,
) -> Result<Rc<ast::RetainedTerm<'input>>, Vec<SemanticError<'input>>> {
    let left = left.release(context);
    let right = right.release(context);
    let ast::TermDetail::Factor { factor } = right.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` is not a term".to_owned())]);
        };
    let ast::FactorDetail::EvalVar { ident, var, getter } = factor.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` cannot be evaluated".to_owned())]);
        };

    if left.ty.base_eq_with_name("qual") {
        let qual = left.ty.arg_as_qual();
        let v = Var::get(context, qual.clone(), ident.name.clone()).ok();
        let ty = match &v {
            Some(x) => {
                var.replace(Some(x.clone()));
                x.ty.borrow().clone()
            },
            None =>
                Ty::new_or_get_type(context, qual.clone(), ident.name.clone(), Vec::new())
                .or(Ty::new_or_get_qual_from_key(context, qual.pushed_qual(ident.name.clone())))
                .map_err(|e| e.convert(right.parsed.clone().map(|x| x.slice)))?,
        };
        let tmp_vars =
            Var::retain_factor_infix_op_tmp_vars(context, &op, left.ty.clone(), right.ty.clone())
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        let operation = factor_infix_operation(context, parsed.clone(), &op, left.ty.clone(), right.ty.clone())?;
        let data = v.map(|x| vec![DataLabel::new(DataLabelKind::Var(x))]);
        right.data.replace(data);
        Ok(Rc::new(ast::RetainedTerm::new(
            Rc::new(ast::Term {
                parsed: Some(parsed),
                detail: Rc::new(ast::TermDetail::InfixOp {
                    left: left.clone(),
                    op: op.clone(),
                    right: right.clone(),
                    operation,
                    instance: None,
                }),
                ty,
                tmp_vars,
                data: RefCell::new(None), // TODO
            })
        )))
    }
    else if left.ty.base_eq_with_name("type") {
        let parent = left.ty.arg_as_type().get_value(context)
            .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
        let qual = Qual::get_from_ty(context, parent)
            .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
        let v = Var::get(context, qual.to_key(), ident.name.clone()).ok();
        let mut ty = match &v {
            Some(x) => {
                var.replace(Some(x.clone()));
                x.ty.borrow().clone()
            },
            None =>
                Ty::new_or_get_type(context, qual.to_key(), ident.name.clone(), Vec::new())
                .or(Ty::new_or_get_qual_from_key(context, qual.to_key().pushed_qual(ident.name.clone())))
                .map_err(|e| e.convert(right.parsed.clone().map(|x| x.slice)))?,
        };
        if ty.base_eq_with_name("getter") {
            let method =
                ty.arg_as_getter().get_value(context)
                .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
            let tvs =
                Var::retain_method_tmp_vars(context, method.clone())
                .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
            getter.replace(Some((tvs[0].clone(), method.clone())));
            ty = method.out_tys[0].clone()
        }
        let tmp_vars =
            Var::retain_factor_infix_op_tmp_vars(context, &op, left.ty.clone(), right.ty.clone())
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        let operation = factor_infix_operation(context, parsed.clone(), &op, left.ty.clone(), right.ty.clone())?;
        let data = v.map(|x| vec![DataLabel::new(DataLabelKind::Var(x))]);
        right.data.replace(data);
        Ok(Rc::new(ast::RetainedTerm::new(
            Rc::new(ast::Term {
                parsed: Some(parsed),
                detail: Rc::new(ast::TermDetail::InfixOp {
                    left: left.clone(),
                    op: op.clone(),
                    right: right.clone(),
                    operation,
                    instance: None,
                }),
                ty,
                tmp_vars,
                data: RefCell::new(None), // TODO
            })
        )))
    }
    else {
        Err(vec![SemanticError::new(None, "Left side of `::` is not a qualifier or a type".to_owned())])
    }
}

fn access_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    parsed: Rc<parser::ast::Term<'input>>,
    left: Rc<ast::RetainedTerm<'input>>,
    op: &ast::FactorInfixOp,
    right: Rc<ast::RetainedTerm<'input>>,
) -> Result<Rc<ast::RetainedTerm<'input>>, Vec<SemanticError<'input>>> {
    let left = left.release(context);
    let right = right.release(context);
    let ast::TermDetail::Factor { factor } = right.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `.` is not a term".to_owned())]);
        };
    let ast::FactorDetail::EvalVar { ident, var, getter } = factor.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `.` cannot be evaluated".to_owned())]);
        };

    if left.ty.is_dotnet_ty() {
        let quals = Qual::get_from_ty_parents(context, left.ty.clone())
            .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
        let v = quals.iter().find_map(|x|
            Var::get(context, x.to_key(), ident.name.clone()).ok()
        );
        let mut ty = match &v {
            Some(x) => {
                var.replace(Some(x.clone()));
                x.ty.borrow().clone()
            },
            None =>
                return Err(vec![SemanticError::new(None, "Right side of `.` cannot be evaluated as the element".to_owned())])
        };
        if ty.base_eq_with_name("getter") {
            let method =
                ty.arg_as_getter().get_value(context)
                .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
            let tvs =
                Var::retain_method_tmp_vars(context, method.clone())
                .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
            getter.replace(Some((tvs[0].clone(), method.clone())));
            ty = method.out_tys[0].clone()
        }
        let tmp_vars =
            Var::retain_factor_infix_op_tmp_vars(context, &op, left.ty.clone(), right.ty.clone())
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        let operation = factor_infix_operation(context, parsed.clone(), &op, left.ty.clone(), right.ty.clone())?;
        let data = v.map(|x| vec![DataLabel::new(DataLabelKind::Var(x))]);
        right.data.replace(data);
        Ok(Rc::new(ast::RetainedTerm::new(
            Rc::new(ast::Term {
                parsed: Some(parsed),
                detail: Rc::new(ast::TermDetail::InfixOp {
                    left: left.clone(),
                    op: op.clone(),
                    right: right.clone(),
                    operation,
                    instance: Some(left.ty.to_key()),
                }),
                ty,
                tmp_vars,
                data: RefCell::new(None), // TODO
            })
        )))
    }
    else {
        return Err(vec![SemanticError::new(None, "Left side of `.` cannot be accessed to the element".to_owned())]);
    }
}

fn eval_fn_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    parsed: Rc<parser::ast::Term<'input>>,
    left: Rc<ast::RetainedTerm<'input>>,
    op: &ast::FactorInfixOp,
    right: Rc<ast::RetainedTerm<'input>>,
) -> Result<Rc<ast::RetainedTerm<'input>>, Vec<SemanticError<'input>>> {
    let left = left.release(context);
    let instance = match left.detail.as_ref() {
        ast::TermDetail::Factor { factor: _ } =>
            None,
        ast::TermDetail::InfixOp { left: _, op: _, right: _, operation: _, instance } =>
            instance.clone(),
    };

    let right = right.release(context);
    let ast::TermDetail::Factor { factor } = right.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `eval fn` is not a term".to_owned())]);
        };
    let ast::FactorDetail::ApplyFn { args, as_fn } = factor.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `eval fn` cannot apply".to_owned())]);
        };
    let args = args.iter().map(|x| x.expr.release(context)).collect::<Vec<_>>();

    if left.ty.base_eq_with_name("function") {
        let key = left.ty.arg_as_function();
        let fn_stats = key.get_value(context)
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        let ret = fn_stats.stats.ret.release(context); // TODO
        let data =
            args.iter()
            .filter_map(|x| x.data.borrow().clone())
            .flat_map(|x| x.into_iter())
            .collect::<Vec<_>>();
        let eval_fn = EvalFn::new_or_get(context, fn_stats.clone(), data);
        let tmp_vars = Vec::new();
        let operation = factor_infix_operation(context, parsed.clone(), &op, left.ty.clone(), right.ty.clone())?;
        as_fn.replace(Some(Rc::new(ast::AsFn::Fn(eval_fn.clone()))));
        Ok(Rc::new(ast::RetainedTerm::new(
            Rc::new(ast::Term {
                parsed: Some(parsed),
                detail: Rc::new(ast::TermDetail::InfixOp {
                    left: left.clone(),
                    op: op.clone(),
                    right: right.clone(),
                    operation,
                    instance: Some(ret.ty.to_key()),
                }),
                ty: ret.ty.clone(),
                tmp_vars,
                data: ret.data.clone(),
            })
        )))
    }
    else if left.ty.base_eq_with_name("method") {
        let in_tys = args.iter().map(|x| x.ty.to_key()).collect();
        let key = left.ty.most_compatible_method(context, instance, in_tys)
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        let m = key.get_value(context)
            .map_err(|e| e.convert(None))?;
        let ty = Ty::tys_to_ty(context, &m.out_tys)
            .map_err(|e| e.convert(None))?;
        let tmp_vars =
            Var::retain_method_tmp_vars(context, m.clone())
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        let data = tmp_vars.iter().map(|x| DataLabel::new(DataLabelKind::Var(x.clone()))).collect();
        let operation = factor_infix_operation(context, parsed.clone(), &op, left.ty.clone(), right.ty.clone())?;
        as_fn.replace(Some(Rc::new(ast::AsFn::Method(m))));
        Ok(Rc::new(ast::RetainedTerm::new(
            Rc::new(ast::Term {
                parsed: Some(parsed),
                detail: Rc::new(ast::TermDetail::InfixOp {
                    left: left.clone(),
                    op: op.clone(),
                    right: right.clone(),
                    operation,
                    instance: tmp_vars.last().map(|x| x.ty.borrow().to_key()),
                }),
                ty,
                tmp_vars,
                data: RefCell::new(Some(data)),
            })
        )))
    }
    else {
        return Err(vec![SemanticError::new(None, "Left side of `eval fn` is not a function or a method".to_owned())]);
    }
}

fn term_infix_operation<'input>(
    context: &Context<'input>,
    parsed: Rc<parser::ast::Expr<'input>>,
    op: &ast::TermInfixOp,
    left_ty: Rc<Ty>,
    right_ty: Rc<Ty>,
) -> Result<Rc<Operation>, Vec<SemanticError<'input>>> {
    let op_methods =
        Method::get_term_infix_op_methods(context, &op, left_ty.clone(), right_ty.clone())
        .map_err(|e| e.convert(Some(parsed.slice)))?;
    let op_literals =
        Literal::get_term_infix_op_literals(context, &op, left_ty.clone(), right_ty.clone())
        .map_err(|e| e.convert(Some(parsed.slice)))?;
    let op_kind = OperationKind::TermInfixOp(op.clone());
    Ok(Operation::new_or_get(context, left_ty, op_kind, op_methods, op_literals))
}

fn factor_infix_operation<'input>(
    context: &Context<'input>,
    parsed: Rc<parser::ast::Term<'input>>,
    op: &ast::FactorInfixOp,
    left_ty: Rc<Ty>,
    right_ty: Rc<Ty>,
) -> Result<Rc<Operation>, Vec<SemanticError<'input>>> {
    let op_methods =
        Method::get_factor_infix_op_methods(context, &op, left_ty.clone(), right_ty.clone())
        .map_err(|e| e.convert(Some(parsed.slice)))?;
    let op_literals =
        Literal::get_factor_infix_op_literals(context, &op, left_ty.clone(), right_ty.clone())
        .map_err(|e| e.convert(Some(parsed.slice)))?;
    let op_kind = OperationKind::FactorInfixOp(op.clone());
    Ok(Operation::new_or_get(context, left_ty, op_kind, op_methods, op_literals))
}
