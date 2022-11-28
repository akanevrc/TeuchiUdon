use std::{
    cell::RefCell,
    collections::VecDeque,
    rc::Rc,
};
use crate::context::Context;
use crate::lexer;
use crate::parser;
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
        method::MethodParamInOut,
        scope::Scope,
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
            let ast::ExprDetail::Term { term } = var_bind.expr.detail.as_ref()
                else {
                    return Err(vec![SemanticError::new(Some(node.slice), "Public variable should be assigned from a literal".to_owned())]);
                };
            let ast::TermDetail::Literal { literal } = term.detail.as_ref()
                else {
                    return Err(vec![SemanticError::new(Some(node.slice), "Public variable should be assigned from a literal".to_owned())]);
                };
            let var = &var_bind.vars[0];
            ValuedVar::new(context, var.qual.clone(), var.name.clone(), var.ty.borrow().clone(), literal.clone())
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
            return Err(ElementError::new("Type inference not succeeded".to_owned()));
        }
        for t in ty_args {
            let t = t.get_value(context)?;
            infer(context, vars, t)?;
        }
        Ok(())
    }
    else {
        Err(ElementError::new("Type inference not succeeded".to_owned()))
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
    let term = ty_term(context, node.ty_term.clone())?;
    exprs.push_back(Rc::new(ast::TyExpr {
        parsed: Some(node.clone()),
        detail: Rc::new(ast::TyExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
    }));
    for op in &node.ty_ops {
        let (op, expr) = match op.kind.as_ref() {
            parser::ast::TyOpKind::Access { op_code: _, ty_term } =>
                access_op_ty_expr(context, node.clone(), ty_term.clone())?,
        };
        ops.push_back(op);
        exprs.push_back(expr);
    }
    expr_tree(context, node, exprs, ops)
}

fn access_op_ty_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyExpr<'input>>,
    term: Rc<parser::ast::TyTerm<'input>>,
) -> Result<(ast::TyOp, Rc<ast::TyExpr<'input>>), Vec<SemanticError<'input>>> {
    let op = access_ty_op(context)?;
    let term = access_ty_term(context, term)?;
    let expr = Rc::new(ast::TyExpr {
        parsed: Some(node),
        detail: Rc::new(ast::TyExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn hidden_unknown_ty_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> Result<Rc<ast::TyExpr<'input>>, Vec<SemanticError<'input>>> {
    let term = Rc::new(ast::TyTerm {
        parsed: None,
        detail: Rc::new(ast::TyTermDetail::None),
        ty: Ty::new_or_get_type_from_name(context, "unknown")
            .map_err(|e| e.convert(None))?,
    });
    Ok(Rc::new(ast::TyExpr {
        parsed: None,
        detail: Rc::new(ast::TyExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
    }))
}

fn hidden_unit_ty_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> Result<Rc<ast::TyExpr<'input>>, Vec<SemanticError<'input>>> {
    let term = Rc::new(ast::TyTerm {
        parsed: None,
        detail: Rc::new(ast::TyTermDetail::None),
        ty: Ty::new_or_get_type_from_name(context, "unit")
            .map_err(|e| e.convert(None))?,
    });
    Ok(Rc::new(ast::TyExpr {
        parsed: None,
        detail: Rc::new(ast::TyExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
    }))
}

fn access_ty_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::TyOp, Vec<SemanticError<'input>>> {
    Ok(ast::TyOp::Access)
}

pub fn ty_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyTerm<'input>>,
) -> Result<Rc<ast::TyTerm<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::TyTermKind::EvalTy { ident } =>
            eval_ty_ty_term(context, node.clone(), ident.clone()),
    }
}

pub fn access_ty_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyTerm<'input>>,
) -> Result<Rc<ast::TyTerm<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::TyTermKind::EvalTy { ident } =>
            eval_ty_access_ty_term(context, node.clone(), ident.clone()),
    }
}

fn eval_ty_ty_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyTerm<'input>>,
    ident: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::TyTerm<'input>>, Vec<SemanticError<'input>>> {
    let ident = self::ident(context, ident)?;
    let ty =
        context.qual_stack.find_ok(|qual|
            Ty::new_or_get_type(context, qual.clone(), ident.name.clone(), Vec::new())
            .or(Ty::new_or_get_qual_from_key(context, qual.pushed_qual(ident.name.clone())))
        ).ok_or(vec![SemanticError::new(Some(node.slice), format!("Specified qualifier `{}` not found", ident.name))])?;
    Ok(Rc::new(ast::TyTerm {
        parsed: Some(node),
        detail: Rc::new(ast::TyTermDetail::EvalTy {
            ident,
        }),
        ty,
    }))
}

fn eval_ty_access_ty_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::TyTerm<'input>>,
    ident: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::TyTerm<'input>>, Vec<SemanticError<'input>>> {
    let ident = self::ident(context, ident)?;
    let ty =
        Ty::get_from_name(context, "unknown")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::TyTerm {
        parsed: Some(node),
        detail: Rc::new(ast::TyTermDetail::EvalTy {
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
) -> Result<Rc<ast::Expr<'input>>, Vec<SemanticError<'input>>> {
    construct_expr_tree(context, node)
}

fn construct_expr_tree<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Expr<'input>>, Vec<SemanticError<'input>>> {
    let mut exprs = VecDeque::new();
    let mut ops = VecDeque::new();
    let term = self::term(context, node.term.clone())?;
    exprs.push_back(Rc::new(ast::Expr {
        parsed: Some(node.clone()),
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: term.data.clone(),
    }));
    for parser_op in &node.ops {
        let (op, expr) = match parser_op.kind.as_ref() {
            parser::ast::OpKind::TyAccess { op_code: _, term } =>
                ty_access_op_expr(context, node.clone(), term.clone())?,
            parser::ast::OpKind::Access { op_code, term } =>
                access_op_expr(context, node.clone(), op_code.clone(), term.clone())?,
            parser::ast::OpKind::EvalFn { arg_exprs } =>
                eval_fn_op_expr(context, node.clone(), arg_exprs)?,
            parser::ast::OpKind::EvalSpreadFn { expr } =>
                eval_spread_fn_op_expr(context, node.clone(), expr.clone())?,
            parser::ast::OpKind::EvalKey { expr } =>
                eval_key_op_expr(context, node.clone(), expr.clone())?,
            parser::ast::OpKind::CastOp { as_keyword: _, ty_expr } =>
                cast_op_expr(context, node.clone(), ty_expr.clone())?,
            parser::ast::OpKind::InfixOp { op_code, term } =>
                infix_op_expr(context, node.clone(), op_code.clone(), term.clone())?,
            parser::ast::OpKind::Assign { term } =>
                assign_op_expr(context, node.clone(), term.clone())?,
        };
        ops.push_back(op);
        exprs.push_back(expr);
    }
    expr_tree(context, node, exprs, ops)
}

fn ty_access_op_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
    term: Rc<parser::ast::Term<'input>>,
) -> Result<(ast::Op, Rc<ast::Expr<'input>>), Vec<SemanticError<'input>>> {
    let op = ty_access_op(context)?;
    let term = self::ty_access_term(context, term)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: term.data.clone(),
    });
    Ok((op, expr))
}

fn access_op_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
    op_code: Rc<lexer::ast::OpCode<'input>>,
    term: Rc<parser::ast::Term<'input>>,
) -> Result<(ast::Op, Rc<ast::Expr<'input>>), Vec<SemanticError<'input>>> {
    let op = access_op(context, op_code)?;
    let term = self::term(context, term)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: term.data.clone(),
    });
    Ok((op, expr))
}

fn eval_fn_op_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
    arg_exprs: &Vec<Rc<parser::ast::ArgExpr<'input>>>,
) -> Result<(ast::Op, Rc<ast::Expr<'input>>), Vec<SemanticError<'input>>> {
    let op = eval_fn_op(context)?;
    let term = apply_fn_term(context, arg_exprs)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: term.data.clone(),
    });
    Ok((op, expr))
}

fn eval_spread_fn_op_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<(ast::Op, Rc<ast::Expr<'input>>), Vec<SemanticError<'input>>> {
    let op = eval_spread_fn_op(context)?;
    let term = apply_spread_fn_term(context, expr)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: term.data.clone(),
    });
    Ok((op, expr))
}

fn eval_key_op_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<(ast::Op, Rc<ast::Expr<'input>>), Vec<SemanticError<'input>>> {
    let op = eval_key_op(context)?;
    let term = apply_key_term(context, expr)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: term.data.clone(),
    });
    Ok((op, expr))
}

fn cast_op_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
    ty_expr: Rc<parser::ast::TyExpr<'input>>,
) -> Result<(ast::Op, Rc<ast::Expr<'input>>), Vec<SemanticError<'input>>> {
    let op = cast_op(context)?;
    let term = ty_expr_term(context, ty_expr)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: term.data.clone(),
    });
    Ok((op, expr))
}

fn infix_op_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
    op_code: Rc<lexer::ast::OpCode<'input>>,
    term: Rc<parser::ast::Term<'input>>,
) -> Result<(ast::Op, Rc<ast::Expr<'input>>), Vec<SemanticError<'input>>> {
    let op = infix_op(context, op_code)?;
    let term = self::term(context, term)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: term.data.clone(),
    });
    Ok((op, expr))
}

fn assign_op_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Expr<'input>>,
    term: Rc<parser::ast::Term<'input>>,
) -> Result<(ast::Op, Rc<ast::Expr<'input>>), Vec<SemanticError<'input>>> {
    let op = assign_op(context)?;
    let term = self::term(context, term)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: term.data.clone(),
    });
    Ok((op, expr))
}

fn hidden_unit_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> Result<Rc<ast::Expr<'input>>, Vec<SemanticError<'input>>> {
    let term = Rc::new(ast::Term {
        parsed: None,
        detail: Rc::new(ast::TermDetail::None),
        ty: Ty::get_from_name(context, "unit")
            .map_err(|e| e.convert(None))?,
        data: RefCell::new(None),
    });
    Ok(Rc::new(ast::Expr {
        parsed: None,
        detail: Rc::new(ast::ExprDetail::Term {
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: RefCell::new(None),
    }))
}

pub fn ty_access_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::Op, Vec<SemanticError<'input>>> {
    Ok(ast::Op::TyAccess)
}

fn access_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<lexer::ast::OpCode>,
) -> Result<ast::Op, Vec<SemanticError<'input>>> {
    match node.kind {
        lexer::ast::OpCodeKind::Dot =>
            Ok(ast::Op::Access),
        lexer::ast::OpCodeKind::CoalescingAccess =>
            Ok(ast::Op::CoalescingAccess),
        _ =>
            panic!("Illegal state"),
    }
}

fn eval_fn_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::Op, Vec<SemanticError<'input>>> {
    Ok(ast::Op::EvalFn)
}

fn eval_spread_fn_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::Op, Vec<SemanticError<'input>>> {
    Ok(ast::Op::EvalSpreadFn)
}

fn eval_key_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::Op, Vec<SemanticError<'input>>> {
    Ok(ast::Op::EvalKey)
}

fn cast_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::Op, Vec<SemanticError<'input>>> {
    Ok(ast::Op::CastOp)
}

fn infix_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<lexer::ast::OpCode<'input>>,
) -> Result<ast::Op, Vec<SemanticError<'input>>> {
    match node.kind {
        lexer::ast::OpCodeKind::Star =>
            Ok(ast::Op::Mul),
        lexer::ast::OpCodeKind::Div =>
            Ok(ast::Op::Div),
        lexer::ast::OpCodeKind::Percent =>
            Ok(ast::Op::Mod),
        lexer::ast::OpCodeKind::Plus =>
            Ok(ast::Op::Add),
        lexer::ast::OpCodeKind::Minus =>
            Ok(ast::Op::Sub),
        lexer::ast::OpCodeKind::LeftShift =>
            Ok(ast::Op::LeftShift),
        lexer::ast::OpCodeKind::RightShift =>
            Ok(ast::Op::RightShift),
        lexer::ast::OpCodeKind::Lt =>
            Ok(ast::Op::Lt),
        lexer::ast::OpCodeKind::Gt =>
            Ok(ast::Op::Gt),
        lexer::ast::OpCodeKind::Le =>
            Ok(ast::Op::Le),
        lexer::ast::OpCodeKind::Ge =>
            Ok(ast::Op::Ge),
        lexer::ast::OpCodeKind::Eq =>
            Ok(ast::Op::Eq),
        lexer::ast::OpCodeKind::Ne =>
            Ok(ast::Op::Ne),
        lexer::ast::OpCodeKind::Amp =>
            Ok(ast::Op::BitAnd),
        lexer::ast::OpCodeKind::Caret =>
            Ok(ast::Op::BitXor),
        lexer::ast::OpCodeKind::Pipe =>
            Ok(ast::Op::BitOr),
        lexer::ast::OpCodeKind::And =>
            Ok(ast::Op::And),
        lexer::ast::OpCodeKind::Or =>
            Ok(ast::Op::Or),
        lexer::ast::OpCodeKind::Coalescing =>
            Ok(ast::Op::Coalescing),
        lexer::ast::OpCodeKind::RightPipeline =>
            Ok(ast::Op::RightPipeline),
        lexer::ast::OpCodeKind::LeftPipeline =>
            Ok(ast::Op::LeftPipeline),
        _ =>
            panic!("Illegal state"),
    }
}

fn assign_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
) -> Result<ast::Op, Vec<SemanticError<'input>>> {
    Ok(ast::Op::Assign)
}

pub fn term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::TermKind::PrefixOp { op_code, term } =>
            prefix_op_term(context, node.clone(), op_code.clone(), term.clone()),
        parser::ast::TermKind::Block { stats } =>
            block_term(context, node.clone(), stats.clone()),
        parser::ast::TermKind::Paren { expr } =>
            paren_term(context, node.clone(), expr.clone()),
        parser::ast::TermKind::Tuple { exprs } =>
            tuple_term(context, node.clone(), exprs),
        parser::ast::TermKind::ArrayCtor { iter_expr } =>
            array_ctor_term(context, node.clone(), iter_expr.clone()),
        parser::ast::TermKind::Literal { literal } =>
            literal_term(context, node.clone(), literal.clone()),
        parser::ast::TermKind::ThisLiteral { literal } =>
            this_literal_term(context, node.clone(), literal.clone()),
        parser::ast::TermKind::InterpolatedString { interpolated_string } =>
            interpolated_string_term(context, node.clone(), interpolated_string.clone()),
        parser::ast::TermKind::EvalVar { ident } =>
            eval_var_term(context, node.clone(), ident.clone()),
        parser::ast::TermKind::LetInBind { var_bind, in_keyword: _, expr } =>
            let_in_bind_term(context, node.clone(), var_bind.clone(), expr.clone()),
        parser::ast::TermKind::If { if_keyword: _, condition, if_part, else_part } =>
            if_term(context, node.clone(), condition.clone(), if_part.clone(), else_part.clone()),
        parser::ast::TermKind::While { while_keyword: _, condition, stats } =>
            while_term(context, node.clone(), condition.clone(), stats.clone()),
        parser::ast::TermKind::Loop { loop_keyword: _, stats } =>
            loop_term(context, node.clone(), stats.clone()),
        parser::ast::TermKind::For { for_binds, stats } =>
            for_term(context, node.clone(), for_binds, stats.clone()),
        parser::ast::TermKind::Closure { var_decl, expr } =>
            closure_term(context, node.clone(), var_decl.clone(), expr.clone()),
    }
}

fn ty_access_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    match node.kind.as_ref() {
        parser::ast::TermKind::EvalVar { ident } =>
            eval_var_ty_access_term(context, node.clone(), ident.clone()),
        _ =>
            Err(vec![SemanticError::new(Some(node.slice), "Illegal use of type access op `::`".to_owned())])
    }
}

fn prefix_op_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    op_code: Rc<lexer::ast::OpCode<'input>>,
    term: Rc<parser::ast::Term<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let op = prefix_op(context, op_code)?;
    let term = self::term(context, term)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::PrefixOp {
            op,
            term: term.clone(),
        }),
        ty: term.ty.clone(),
        data: RefCell::new(None), // TODO
    }))
}

fn block_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    stats: Rc<parser::ast::StatsBlock<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let scope = Scope::Block(context.block_id_factory.next_id());
    let stats = stats_block(context, stats, scope)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::Block {
            stats: stats.clone(),
        }),
        ty: stats.ret.ty.clone(),
        data: stats.ret.data.clone(),
    }))
}

fn paren_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let expr = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::Paren {
            expr: expr.clone(),
        }),
        ty: expr.ty.clone(),
        data: expr.data.clone(),
    }))
}

fn tuple_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    exprs: &Vec<Rc<parser::ast::Expr<'input>>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let exprs =
        exprs.iter()
        .map(|x| expr(context, x.clone()))
        .collect::<Result<Vec<_>, _>>()?;
    let data =
        exprs.iter()
        .filter_map(|x| x.data.borrow().clone())
        .flat_map(|x| x.into_iter())
        .collect::<Vec<_>>();
    let data = if data.len() == 0 { None } else { Some(data) };
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::Tuple {
            exprs,
        }),
        ty: Ty::get_from_name(context, "tuple")
            .map_err(|e| e.convert(None))?, // TODO
        data: RefCell::new(data),
    }))
}

fn array_ctor_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    iter_expr: Option<Rc<parser::ast::IterExpr<'input>>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let iter_expr = match iter_expr {
        Some(x) => self::iter_expr(context, x)?,
        None => empty_iter_expr(context)?,
    };
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::ArrayCtor {
            iter_expr,
        }),
        ty: Ty::get_from_name(context, "array")
            .map_err(|e| e.convert(None))?, // TODO
        data: RefCell::new(None), // TODO
    }))
}

fn literal_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    literal: Rc<lexer::ast::Literal<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let literal = self::literal(context, literal)?;
    let data = Some(vec![DataLabel::new(DataLabelKind::Literal(literal.clone()))]);
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::Literal {
            literal: literal.clone(),
        }),
        ty: literal.ty.clone(),
        data: RefCell::new(data),
    }))
}

fn this_literal_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    _literal: Rc<lexer::ast::Literal<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::ThisLiteral),
        ty: Ty::get_from_name(context, "udon")
            .map_err(|e| e.convert(None))?, // TODO
        data: RefCell::new(None), // TODO
    }))
}

fn interpolated_string_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    interpolated_string: Rc<lexer::ast::InterpolatedString<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let interpolated_string = self::interpolated_string(context, interpolated_string)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::InterpolatedString {
            interpolated_string,
        }),
        ty: Ty::get_from_name(context, "string")
            .map_err(|e| e.convert(None))?, // TODO
        data: RefCell::new(None), // TODO
    }))
}

fn eval_var_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    ident: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
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
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::EvalVar {
            ident,
            var: RefCell::new(var.ok()),
        }),
        ty,
        data: RefCell::new(data),
    }))
}

fn eval_var_ty_access_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    ident: Rc<lexer::ast::Ident<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let ident = self::ident(context, ident)?;
    let ty =
        Ty::get_from_name(context, "unknown")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::EvalVar {
            ident,
            var: RefCell::new(None),
        }),
        ty,
        data: RefCell::new(None),
    }))
}

fn let_in_bind_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    var_bind: Rc<parser::ast::VarBind<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let scope = Scope::Block(context.block_id_factory.next_id());
    context.qual_stack.push_scope(context, scope);
    let var_bind = self::var_bind(context, var_bind)?;
    let expr = self::expr(context, expr)?;
    context.qual_stack.pop();
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::LetInBind {
            var_bind,
            expr: expr.clone(),
        }),
        ty: expr.ty.clone(),
        data: expr.data.clone(),
    }))
}

fn if_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    condition: Rc<parser::ast::Expr<'input>>,
    if_part: Rc<parser::ast::StatsBlock<'input>>,
    else_part: Option<(Rc<lexer::ast::Keyword<'input>>, Rc<parser::ast::StatsBlock<'input>>)>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let condition = expr(context, condition)?;
    let if_scope = Scope::Block(context.block_id_factory.next_id());
    let if_part = stats_block(context, if_part, if_scope)?;
    let else_part = match else_part {
        Some((_, stats)) => {
            let else_scope = Scope::Block(context.block_id_factory.next_id());
            Some(stats_block(context, stats, else_scope)?)
        },
        None => None,
    };
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::If {
            condition,
            if_part: if_part.clone(),
            else_part,
        }),
        ty: if_part.ret.ty.clone(), // TODO
        data: if_part.ret.data.clone(), // TODO
    }))
}

fn while_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    condition: Rc<parser::ast::Expr<'input>>,
    stats: Rc<parser::ast::StatsBlock<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let condition = expr(context, condition)?;
    let scope = Scope::Loop(context.loop_id_factory.next_id());
    let stats = stats_block(context, stats, scope)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::While {
            condition,
            stats,
        }),
        ty: Ty::get_from_name(context, "unit")
            .map_err(|e| e.convert(None))?,
        data: RefCell::new(None),
    }))
}

fn loop_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    stats: Rc<parser::ast::StatsBlock<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let scope = Scope::Loop(context.loop_id_factory.next_id());
    let stats = stats_block(context, stats, scope)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::Loop {
            stats,
        }),
        ty: Ty::get_from_name(context, "unit")
            .map_err(|e| e.convert(None))?,
        data: RefCell::new(None),
    }))
}

fn for_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    for_binds: &Vec<(Rc<lexer::ast::Keyword<'input>>, Rc<parser::ast::ForBind<'input>>)>,
    stats: Rc<parser::ast::StatsBlock<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let for_binds =
        for_binds.iter()
        .map(|x| for_bind(context, x.1.clone()))
        .collect::<Result<_, _>>()?;
    let scope = Scope::Loop(context.loop_id_factory.next_id());
    let stats = stats_block(context, stats, scope)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::For {
            for_binds,
            stats,
        }),
        ty: Ty::get_from_name(context, "unit")
            .map_err(|e| e.convert(None))?,
        data: RefCell::new(None),
    }))
}

fn closure_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    node: Rc<parser::ast::Term<'input>>,
    var_decl: Rc<parser::ast::VarDecl<'input>>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let var_decl = self::var_decl(context, var_decl)?;
    let expr = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: Rc::new(ast::TermDetail::Closure {
            var_decl,
            expr,
        }),
        ty: Ty::get_from_name(context, "closure")
            .map_err(|e| e.convert(None))?, // TODO
        data: RefCell::new(None),
    }))
}

fn ty_expr_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    ty_expr: Rc<parser::ast::TyExpr<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let te = self::ty_expr(context, ty_expr)?;
    Ok(Rc::new(ast::Term {
        parsed: None,
        detail: Rc::new(ast::TermDetail::TyExpr {
            ty_expr: te,
        }),
        ty: Ty::get_from_name(context, "type")
            .map_err(|e| e.convert(None))?, // TODO
        data: RefCell::new(None),
    }))
}

fn apply_fn_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    arg_exprs: &Vec<Rc<parser::ast::ArgExpr<'input>>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let args =
        arg_exprs.iter()
        .map(|x| arg_expr(context, x.clone()))
        .collect::<Result<_, _>>()?;
    let ty =
        Ty::get_from_name(context, "unit")
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Term {
        parsed: None,
        detail: Rc::new(ast::TermDetail::ApplyFn {
            args,
            as_fn: RefCell::new(None),
        }),
        ty,
        data: RefCell::new(None),
    }))
}

fn apply_spread_fn_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let arg = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        parsed: None,
        detail: Rc::new(ast::TermDetail::ApplySpreadFn {
            arg,
        }),
        ty: Ty::get_from_name(context, "unit")
            .map_err(|e| e.convert(None))?,
        data: RefCell::new(None),
    }))
}

fn apply_key_term<'input: 'context, 'context>(
    context: &'context Context<'input>,
    expr: Rc<parser::ast::Expr<'input>>,
) -> Result<Rc<ast::Term<'input>>, Vec<SemanticError<'input>>> {
    let key = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        parsed: None,
        detail: Rc::new(ast::TermDetail::ApplyKey {
            key,
        }),
        ty: Ty::get_from_name(context, "unit")
            .map_err(|e| e.convert(None))?,
        data: RefCell::new(None),
    }))
}

fn prefix_op<'input: 'context, 'context>(
    _context: &'context Context<'input>,
    node: Rc<lexer::ast::OpCode<'input>>,
) -> Result<ast::PrefixOp, Vec<SemanticError<'input>>> {
    match node.kind {
        lexer::ast::OpCodeKind::Plus =>
            Ok(ast::PrefixOp::Plus),
        lexer::ast::OpCodeKind::Minus =>
            Ok(ast::PrefixOp::Minus),
        lexer::ast::OpCodeKind::Bang =>
            Ok(ast::PrefixOp::Bang),
        lexer::ast::OpCodeKind::Tilde =>
            Ok(ast::PrefixOp::Tilde),
        _ =>
            panic!("Illegal state"),
    }
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
        .collect::<Result<_, _>>()?;
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
            Literal::new_unit(context)
            .map_err(|e| e.convert(Some(left.slice))),
        lexer::ast::LiteralKind::Null { keyword } =>
            Literal::new_null(context)
            .map_err(|e| e.convert(Some(keyword.slice))),
        lexer::ast::LiteralKind::Bool { keyword } =>
            Literal::new_bool(context, (*keyword.slice).to_owned())
            .map_err(|e| e.convert(Some(keyword.slice))),
        lexer::ast::LiteralKind::PureInteger { slice } =>
            Literal::new_pure_integer(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::DecInteger { slice } =>
            Literal::new_dec_integer(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::HexInteger { slice } =>
            Literal::new_hex_integer(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::BinInteger { slice } =>
            Literal::new_bin_integer(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::RealNumber { slice } =>
            Literal::new_real_number(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::Character { slice } =>
            Literal::new_character(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::RegularString { slice } =>
            Literal::new_regular_string(context, (*slice).to_owned())
            .map_err(|e| e.convert(Some(slice))),
        lexer::ast::LiteralKind::VerbatiumString { slice } =>
            Literal::new_verbatium_string(context, (*slice).to_owned())
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
    let mut exprs = Vec::new();
    for x in &node.exprs {
        exprs.push(expr(context, x.clone())?);
    }
    let exprs =
        node.exprs.iter()
        .map(|x| expr(context, x.clone()))
        .collect::<Result<_, _>>()?;
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
    SemanticOp: Clone + 'context,
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
    SemanticOp: Clone,
{
    let expr_0 = exprs.pop_front().unwrap();
    let (mut acc_exprs, acc_ops, expr) =
        ops.into_iter().zip(exprs.into_iter())
        .fold(Ok::<_, Vec<SemanticError>>((VecDeque::new(), VecDeque::new(), expr_0)), |acc, x| {
            let mut acc = acc?;
            if pred(&x.0) {
                let infix_op = ExprTree::infix_op(context, node.clone(), acc.2, x.0, x.1)?;
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
    SemanticOp: Clone,
{
    let expr_0 = exprs.pop_back().unwrap();
    let (mut acc_exprs, acc_ops, expr) =
        ops.into_iter().rev().zip(exprs.into_iter().rev())
        .fold(Ok::<_, Vec<SemanticError>>((VecDeque::new(), VecDeque::new(), expr_0)), |acc, x| {
            let mut acc = acc?;
            if pred(&x.0) {
                let infix_op = ExprTree::infix_op(context, node.clone(), x.1, x.0, acc.2)?;
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
        op: ast::TyOp,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'input>>> {
        match &op {
            ast::TyOp::Access =>
                access_ty_infix_op(context, parsed, left, op, right),
        }
    }
}

fn access_ty_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    parsed: Rc<parser::ast::TyExpr<'input>>,
    left: Rc<ast::TyExpr<'input>>,
    op: ast::TyOp,
    right: Rc<ast::TyExpr<'input>>,
) -> Result<Rc<ast::TyExpr<'input>>, Vec<SemanticError<'input>>> {
    let ast::TyExprDetail::Term { term } = right.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` is not a term".to_owned())]);
        };
    let ast::TyTermDetail::EvalTy { ident } = term.detail.as_ref()
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
                op,
                right: right.clone(),
            }),
            ty,
        }))
    }
    else if left.ty.base_eq_with_name("type") {
        let parent = left.ty.arg_as_type().get_value(context)
            .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
        let qual = left.ty.base.qual.get_pushed_qual(context, parent.base.name.clone())
            .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
        let ty = Ty::new_or_get_type(context, qual.to_key(), ident.name.clone(), Vec::new())
            .map_err(|e| e.convert(right.parsed.clone().map(|x| x.slice)))?;
        Ok(Rc::new(ast::TyExpr {
            parsed: Some(parsed),
            detail: Rc::new(ast::TyExprDetail::InfixOp {
                left: left.clone(),
                op,
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
    ast::ExprTree<'input, 'context, ast::Op, parser::ast::Expr<'input>> for ast::Expr<'input>
{
    fn priorities(context: &'context Context<'input>) -> &'context Vec<(Box<dyn Fn(&ast::Op) -> bool>, ast::Assoc)> {
        &context.semantic_op.priorities
    }

    fn infix_op(
        context: &'context Context<'input>,
        parsed: Rc<parser::ast::Expr<'input>>,
        left: Rc<Self>,
        op: ast::Op,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'input>>> {
        match &op {
            ast::Op::TyAccess =>
                ty_access_infix_op(context, parsed, left, op, right),
            ast::Op::EvalFn =>
                eval_fn_infix_op(context, parsed, left, op, right),
            _ =>
                panic!("Not implemented")
        }
    }
}

fn ty_access_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    parsed: Rc<parser::ast::Expr<'input>>,
    left: Rc<ast::Expr<'input>>,
    op: ast::Op,
    right: Rc<ast::Expr<'input>>,
) -> Result<Rc<ast::Expr<'input>>, Vec<SemanticError<'input>>> {
    let ast::ExprDetail::Term { term } = right.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` is not a term".to_owned())]);
        };
    let ast::TermDetail::EvalVar { ident, var } = term.detail.as_ref()
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
        let data = v.map(|x| vec![DataLabel::new(DataLabelKind::Var(x))]);
        right.data.replace(data);
        Ok(Rc::new(ast::Expr {
            parsed: Some(parsed),
            detail: Rc::new(ast::ExprDetail::InfixOp {
                left: left.clone(),
                op,
                right: right.clone(),
            }),
            ty,
            data: right.data.clone(),
        }))
    }
    else if left.ty.base_eq_with_name("type") {
        let parent = left.ty.arg_as_type().get_value(context)
            .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
        let qual = parent.base.qual.get_pushed_qual(context, parent.base.name.clone())
            .map_err(|e| e.convert(left.parsed.clone().map(|x| x.slice)))?;
        let v = Var::get(context, qual.to_key(), ident.name.clone()).ok();
        let ty = match &v {
            Some(x) => {
                var.replace(Some(x.clone()));
                x.ty.borrow().clone()
            },
            None =>
                Ty::new_or_get_type(context, qual.to_key(), ident.name.clone(), Vec::new())
                .or(Ty::new_or_get_qual_from_key(context, qual.to_key().pushed_qual(ident.name.clone())))
                .map_err(|e| e.convert(right.parsed.clone().map(|x| x.slice)))?,
        };
        let data = v.map(|x| vec![DataLabel::new(DataLabelKind::Var(x))]);
        right.data.replace(data);
        Ok(Rc::new(ast::Expr {
            parsed: Some(parsed),
            detail: Rc::new(ast::ExprDetail::InfixOp {
                left: left.clone(),
                op,
                right: right.clone(),
            }),
            ty,
            data: right.data.clone(),
        }))
    }
    else {
        Err(vec![SemanticError::new(None, "Left side of `::` is not a qualifier or a type".to_owned())])
    }
}

fn eval_fn_infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
    parsed: Rc<parser::ast::Expr<'input>>,
    left: Rc<ast::Expr<'input>>,
    op: ast::Op,
    right: Rc<ast::Expr<'input>>,
) -> Result<Rc<ast::Expr<'input>>, Vec<SemanticError<'input>>> {
    let ast::ExprDetail::Term { term } = right.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `eval fn` is not a term".to_owned())]);
        };
    let ast::TermDetail::ApplyFn { args, as_fn } = term.detail.as_ref()
        else {
            return Err(vec![SemanticError::new(None, "Right side of `eval fn` cannot apply".to_owned())]);
        };

    if left.ty.base_eq_with_name("function") {
        let key = left.ty.arg_as_function();
        let fn_stats = key.get_value(context)
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        let data =
            args.iter()
            .filter_map(|x| x.expr.data.borrow().clone())
            .flat_map(|x| x.into_iter())
            .collect::<Vec<_>>();
        let eval_fn = EvalFn::new_or_get(context, fn_stats.clone(), data);
        as_fn.replace(Some(Rc::new(ast::AsFn::Fn(eval_fn.clone()))));
        right.data.replace(fn_stats.stats.ret.data.borrow().clone());
        Ok(Rc::new(ast::Expr {
            parsed: Some(parsed),
            detail: Rc::new(ast::ExprDetail::InfixOp {
                left: left.clone(),
                op,
                right: right.clone(),
            }),
            ty: fn_stats.ty.clone(),
            data: right.data.clone(),
        }))
    }
    else if left.ty.base_eq_with_name("method") {
        let in_tys = args.iter().map(|x| x.expr.ty.to_key()).collect();
        let key = left.ty.most_compatible_method(context, in_tys)
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        let m = key.get_value(context)
            .map_err(|e| e.convert(None))?;
        let ty = Ty::tys_to_ty(context, &m.out_tys)
            .map_err(|e| e.convert(None))?;
        as_fn.replace(Some(Rc::new(ast::AsFn::Method(m))));
        right.data.replace(None); // TODO
        Ok(Rc::new(ast::Expr {
            parsed: Some(parsed),
            detail: Rc::new(ast::ExprDetail::InfixOp {
                left: left.clone(),
                op,
                right: right.clone(),
            }),
            ty,
            data: RefCell::new(None), // TODO
        }))
    }
    else {
        return Err(vec![SemanticError::new(None, "Left side of `eval fn` is not a function or a method".to_owned())]);
    }
}
