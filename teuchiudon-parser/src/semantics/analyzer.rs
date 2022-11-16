use std::{
    cell::RefCell,
    collections::VecDeque,
    rc::Rc,
};
use crate::context::Context;
use crate::lexer;
use crate::parser;
use super::elements::element::KeyElement;
use super::{
    ast,
    SemanticError,
    elements::{
        element::{
            ValueElement,
        },
        literal::Literal,
        qual::{
            QualKey,
            Qual,
        },
        ty::Ty,
        var::{
            Var,
            VarKey,
        },
    },
};

pub fn target<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Target,
) -> Result<ast::Target<'parsed>, Vec<SemanticError<'parsed>>> {
    let body = match &node.body {
        Some(x) => body(context, x)?,
        None => empty_body(context)?,
    };
    Ok(ast::Target {
        parsed: Some(node),
        body,
    })
}

pub fn body<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Body,
) -> Result<ast::Body<'parsed>, Vec<SemanticError<'parsed>>> {
    let top_stats =
        node.top_stats.iter()
        .map(|x| top_stat(context, x))
        .collect::<Result<_, _>>()?;
    Ok(ast::Body {
        parsed: Some(node),
        top_stats,
    })
}

fn empty_body<'parsed>(
    _context: &Context,
) -> Result<ast::Body<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::Body {
        parsed: None,
        top_stats: Vec::new(),
    })
}

pub fn top_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TopStat,
) -> Result<ast::TopStat<'parsed>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::TopStatKind::VarBind { access_attr, sync_attr, var_bind } =>
            var_bind_top_stat(context, node, access_attr, sync_attr, var_bind),
        parser::ast::TopStatKind::FnBind { access_attr, fn_bind } =>
            fn_bind_top_stat(context, node, access_attr, fn_bind),
        parser::ast::TopStatKind::Stat { stat } =>
            stat_top_stat(context, node, stat),
    }
}

fn var_bind_top_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TopStat,
    access_attr: &'parsed Option<parser::ast::AccessAttr>,
    sync_attr: &'parsed Option<parser::ast::SyncAttr>,
    var_bind: &'parsed parser::ast::VarBind,
) -> Result<ast::TopStat<'parsed>, Vec<SemanticError<'parsed>>> {
    let access_attr = self::access_attr(context, access_attr)?;
    let sync_attr = self::sync_attr(context, sync_attr)?;
    let var_bind = self::var_bind(context, var_bind)?;
    Ok(ast::TopStat {
        parsed: Some(node),
        detail: ast::TopStatDetail::VarBind {
            access_attr,
            sync_attr,
            var_bind,
        },
    })
}

fn fn_bind_top_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TopStat,
    access_attr: &'parsed Option<parser::ast::AccessAttr>,
    fn_bind: &'parsed parser::ast::FnBind,
) -> Result<ast::TopStat<'parsed>, Vec<SemanticError<'parsed>>> {
    let access_attr = self::access_attr(context, access_attr)?;
    let fn_bind = self::fn_bind(context, fn_bind)?;
    Ok(ast::TopStat {
        parsed: Some(node),
        detail: ast::TopStatDetail::FnBind {
            access_attr,
            fn_bind,
        },
    })
}

fn stat_top_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TopStat,
    stat: &'parsed parser::ast::Stat,
) -> Result<ast::TopStat<'parsed>, Vec<SemanticError<'parsed>>> {
    let stat = self::stat(context, stat)?;
    Ok(ast::TopStat {
        parsed: Some(node),
        detail: ast::TopStatDetail::Stat {
            stat,
        },
    })
}

pub fn access_attr<'parsed>(
    context: &Context,
    node: &'parsed Option<parser::ast::AccessAttr>,
) -> Result<ast::AccessAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    match node {
        Some(attr) =>
            match &attr.attr.kind {
                lexer::ast::KeywordKind::Pub =>
                    pub_access_attr(context, attr),
                _ =>
                    panic!("Illegal state"),
            },
        None =>
            Ok(ast::AccessAttr { parsed: None, detail: ast::AccessAttrDetail::None }),
    }
    
}

fn pub_access_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::AccessAttr,
) -> Result<ast::AccessAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::AccessAttr {
        parsed: Some(node),
        detail: ast::AccessAttrDetail::Pub,
    })
}

fn sync_attr<'parsed>(
    context: &Context,
    node: &'parsed Option<parser::ast::SyncAttr>,
) -> Result<ast::SyncAttr<'parsed>, Vec<SemanticError<'parsed>>> {
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
            Ok(ast::SyncAttr { parsed: None, detail: ast::SyncAttrDetail::None }),
    }
    
}

fn sync_sync_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::SyncAttr,
) -> Result<ast::SyncAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::SyncAttr {
        parsed: Some(node),
        detail: ast::SyncAttrDetail::Sync,
    })
}

fn linear_sync_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::SyncAttr,
) -> Result<ast::SyncAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::SyncAttr {
        parsed: Some(node),
        detail: ast::SyncAttrDetail::Linear,
    })
}

fn smooth_sync_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::SyncAttr,
) -> Result<ast::SyncAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::SyncAttr {
        parsed: Some(node),
        detail: ast::SyncAttrDetail::Smooth,
    })
}

pub fn var_bind<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::VarBind,
) -> Result<ast::VarBind<'parsed>, Vec<SemanticError<'parsed>>> {
    let var_decl = var_decl(context, &node.var_decl)?;
    let expr = expr(context, &node.expr)?;
    Ok(ast::VarBind {
        parsed: Some(node),
        var_decl,
        expr,
    })
}

pub fn var_decl<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::VarDecl,
) -> Result<ast::VarDecl<'parsed>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::VarDeclKind::SingleDecl { mut_attr, ident, ty_expr } =>
            single_var_decl(context, node, mut_attr, ident, ty_expr),
        parser::ast::VarDeclKind::TupleDecl{ var_decls } =>
            tuple_var_decl(context, node, var_decls),
    }
}

fn single_var_decl<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::VarDecl,
    mut_attr: &'parsed Option<parser::ast::MutAttr>,
    ident: &'parsed lexer::ast::Ident,
    ty_expr: &'parsed Option<Rc<parser::ast::TyExpr>>,
) -> Result<ast::VarDecl<'parsed>, Vec<SemanticError<'parsed>>> {
    let mut_attr = self::mut_attr(context, mut_attr)?;
    let ident = self::ident(context, ident)?;
    let ty_expr = match ty_expr {
        Some(x) => self::ty_expr(context, x)?,
        None => hidden_unknown_ty_expr(context)?,
    };
    let qual =
        Qual::top(context)
        .map_err(|x| x.convert(None))?;
    let var = Var::force_new(
        context,
        qual,
        ident.name.clone(),
        ty_expr.ty.clone(),
        matches!(mut_attr.detail, ast::MutAttrDetail::Mut),
        false,
    )
    .map_err(|e| e.convert(ident.parsed.map(|x| x.slice)))?;
    Ok(ast::VarDecl {
        parsed: Some(node),
        detail: ast::VarDeclDetail::SingleDecl {
            mut_attr,
            ident,
            ty_expr,
            var: var.clone(),
        },
    })
}

fn tuple_var_decl<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::VarDecl,
    var_decls: &'parsed Vec<parser::ast::VarDecl>,
) -> Result<ast::VarDecl<'parsed>, Vec<SemanticError<'parsed>>> {
    let var_decls =
        var_decls.iter()
        .map(|x| var_decl(context, x))
        .collect::<Result<_, _>>()?;
    Ok(ast::VarDecl {
        parsed: Some(node),
        detail: ast::VarDeclDetail::TupleDecl {
            var_decls,
        },
    })
}

pub fn mut_attr<'parsed>(
    context: &Context,
    node: &'parsed Option<parser::ast::MutAttr>,
) -> Result<ast::MutAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    match node {
        Some(attr) =>
            match &attr.attr.kind {
                lexer::ast::KeywordKind::Mut =>
                    mut_mut_attr(context, attr),
                _ =>
                    panic!("Illegal state"),
            },
        None =>
            Ok(ast::MutAttr { parsed: None, detail: ast::MutAttrDetail::None }),
    }
}

fn mut_mut_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::MutAttr,
) -> Result<ast::MutAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::MutAttr {
        parsed: Some(node),
        detail: ast::MutAttrDetail::Mut,
    })
}

pub fn fn_bind<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::FnBind,
) -> Result<ast::FnBind<'parsed>, Vec<SemanticError<'parsed>>> {
    let fn_decl = fn_decl(context, &node.fn_decl)?;
    let stats_block = stats_block(context, &node.stats_block)?;
    Ok(ast::FnBind {
        parsed: Some(node),
        fn_decl,
        stats_block,
    })
}

pub fn fn_decl<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::FnDecl,
) -> Result<ast::FnDecl<'parsed>, Vec<SemanticError<'parsed>>> {
    let ident = ident(context, &node.ident)?;
    let var_decl = var_decl(context, &node.var_decl)?;
    let ty_expr = match &node.ty_expr {
        Some(x) => ty_expr(context, x)?,
        None => hidden_unit_ty_expr(context)?,
    };
    Ok(ast::FnDecl {
        parsed: Some(node),
        ident,
        var_decl,
        ty_expr,
    })
}

pub fn ty_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TyExpr,
) -> Result<Rc<ast::TyExpr<'parsed>>, Vec<SemanticError<'parsed>>> {
    construct_ty_expr_tree(context, node)
}

fn construct_ty_expr_tree<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TyExpr,
) -> Result<Rc<ast::TyExpr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let mut exprs = VecDeque::new();
    let mut ops = VecDeque::new();
    let term = ty_term(context, &node.ty_term)?;
    exprs.push_back(Rc::new(ast::TyExpr {
        parsed: Some(node),
        detail: ast::TyExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }));
    for op in &node.ty_ops {
        let (op, expr) = match &op.kind {
            parser::ast::TyOpKind::Access { op_code: _, ty_term } =>
                access_op_ty_expr(context, node, ty_term)?,
        };
        ops.push_back(op);
        exprs.push_back(expr);
    }
    expr_tree(context, node, exprs, ops)
}

fn access_op_ty_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TyExpr,
    term: &'parsed parser::ast::TyTerm,
) -> Result<(ast::TyOp, Rc<ast::TyExpr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = access_ty_op(context)?;
    let term = access_ty_term(context, term)?;
    let expr = Rc::new(ast::TyExpr {
        parsed: Some(node),
        detail: ast::TyExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn hidden_unknown_ty_expr<'parsed>(
    context: &Context,
) -> Result<Rc<ast::TyExpr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let term = Rc::new(ast::TyTerm {
        parsed: None,
        detail: ast::TyTermDetail::None,
        ty: Ty::get_from_name(context, "unknown".to_owned())
            .map_err(|e| e.convert(None))?,
    });
    Ok(Rc::new(ast::TyExpr {
        parsed: None,
        detail: ast::TyExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }))
}

fn hidden_unit_ty_expr<'parsed>(
    context: &Context,
) -> Result<Rc<ast::TyExpr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let term = Rc::new(ast::TyTerm {
        parsed: None,
        detail: ast::TyTermDetail::None,
        ty: Ty::get_from_name(context, "unit".to_owned())
            .map_err(|e| e.convert(None))?,
    });
    Ok(Rc::new(ast::TyExpr {
        parsed: None,
        detail: ast::TyExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }))
}

fn access_ty_op<'parsed>(
    _context: &Context,
) -> Result<ast::TyOp, Vec<SemanticError<'parsed>>> {
    Ok(ast::TyOp::Access)
}

pub fn ty_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TyTerm,
) -> Result<Rc<ast::TyTerm<'parsed>>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::TyTermKind::EvalTy { ident } =>
            eval_ty_ty_term(context, node, ident),
    }
}

pub fn access_ty_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TyTerm,
) -> Result<Rc<ast::TyTerm<'parsed>>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::TyTermKind::EvalTy { ident } =>
            eval_ty_access_ty_term(context, node, ident),
    }
}

fn eval_ty_ty_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TyTerm,
    ident: &'parsed lexer::ast::Ident,
) -> Result<Rc<ast::TyTerm<'parsed>>, Vec<SemanticError<'parsed>>> {
    let ident = self::ident(context, ident)?;
    let var =
        VarKey::new(QualKey::top(), ident.name.clone()).get_value(context)
        .map_err(|e| e.convert(Some(node.slice)))?;
    Ok(Rc::new(ast::TyTerm {
        parsed: Some(node),
        detail: ast::TyTermDetail::EvalTy {
            ident,
            var: RefCell::new(var.clone()),
        },
        ty: var.ty.clone(),
    }))
}

fn eval_ty_access_ty_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TyTerm,
    ident: &'parsed lexer::ast::Ident,
) -> Result<Rc<ast::TyTerm<'parsed>>, Vec<SemanticError<'parsed>>> {
    let ident = self::ident(context, ident)?;
    let var =
        Var::unknown(context)
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::TyTerm {
        parsed: Some(node),
        detail: ast::TyTermDetail::EvalTy {
            ident,
            var: RefCell::new(var.clone()),
        },
        ty: var.ty.clone(),
    }))
}

pub fn stats_block<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::StatsBlock,
) -> Result<ast::StatsBlock<'parsed>, Vec<SemanticError<'parsed>>> {
    let stats =
        node.stats.iter()
        .map(|x| stat(context, x))
        .collect::<Result<_, _>>()?;
    let ret = match &node.ret {
        Some(x) => expr(context, x)?,
        None => hidden_unit_expr(context)?,
    };
    Ok(ast::StatsBlock {
        parsed: Some(node),
        stats,
        ret,
    })
}

pub fn stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Stat,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::StatKind::Return { return_keyword: _, expr } =>
            return_stat(context, node, expr),
        parser::ast::StatKind::Continue { continue_keyword: _ } =>
            continue_stat(context, node),
        parser::ast::StatKind::Break { break_keyword: _ } =>
            break_stat(context, node),
        parser::ast::StatKind::VarBind { var_bind } =>
            var_bind_stat(context, node, var_bind),
        parser::ast::StatKind::FnBind { fn_bind } =>
            fn_bind_stat(context, node, fn_bind),
        parser::ast::StatKind::Expr { expr } =>
            expr_stat(context, node, expr),
    }
}

fn return_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Stat,
    expr: &'parsed Option<Rc<parser::ast::Expr>>,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    let expr = match expr {
        Some(x) => self::expr(context, x)?,
        None => hidden_unit_expr(context)?,
    };
    Ok(ast::Stat {
        parsed: Some(node),
        detail: ast::StatDetail::Return {
            expr,
        },
    })
}

fn continue_stat<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::Stat,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::Stat {
        parsed: Some(node),
        detail: ast::StatDetail::Continue,
    })
}

fn break_stat<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::Stat,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::Stat {
        parsed: Some(node),
        detail: ast::StatDetail::Break,
    })
}

fn var_bind_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Stat,
    var_bind: &'parsed parser::ast::VarBind,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    let var_bind = self::var_bind(context, var_bind)?;
    Ok(ast::Stat {
        parsed: Some(node),
        detail: ast::StatDetail::VarBind {
            var_bind,
        },
    })
}

fn fn_bind_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Stat,
    fn_bind: &'parsed parser::ast::FnBind,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    let fn_bind = self::fn_bind(context, fn_bind)?;
    Ok(ast::Stat {
        parsed: Some(node),
        detail: ast::StatDetail::FnBind {
            fn_bind,
        },
    })
}

fn expr_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Stat,
    expr: &'parsed parser::ast::Expr,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    let expr = self::expr(context, expr)?;
    Ok(ast::Stat {
        parsed: Some(node),
        detail: ast::StatDetail::Expr {
            expr,
        },
    })
}

pub fn expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
) -> Result<Rc<ast::Expr<'parsed>>, Vec<SemanticError<'parsed>>> {
    construct_expr_tree(context, node)
}

fn construct_expr_tree<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
) -> Result<Rc<ast::Expr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let mut exprs = VecDeque::new();
    let mut ops = VecDeque::new();
    let term = self::term(context, &node.term)?;
    exprs.push_back(Rc::new(ast::Expr {
        parsed: Some(node),
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }));
    for parser_op in &node.ops {
        let (op, expr) = match &parser_op.kind {
            parser::ast::OpKind::TyAccess { op_code: _, term } =>
                ty_access_op_expr(context, node, term)?,
            parser::ast::OpKind::Access { op_code, term } =>
                access_op_expr(context, node, op_code, term)?,
            parser::ast::OpKind::EvalFn { arg_exprs } =>
                eval_fn_op_expr(context, node, arg_exprs)?,
            parser::ast::OpKind::EvalSpreadFn { expr } =>
                eval_spread_fn_op_expr(context, node, expr)?,
            parser::ast::OpKind::EvalKey { expr } =>
                eval_key_op_expr(context, node, expr)?,
            parser::ast::OpKind::CastOp { as_keyword: _, ty_expr } =>
                cast_op_expr(context, node, ty_expr)?,
            parser::ast::OpKind::InfixOp { op_code, term } =>
                infix_op_expr(context, node, op_code, term)?,
            parser::ast::OpKind::Assign { term } =>
                assign_op_expr(context, node, term)?,
        };
        ops.push_back(op);
        exprs.push_back(expr);
    }
    expr_tree(context, node, exprs, ops)
}

fn ty_access_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    term: &'parsed parser::ast::Term,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = ty_access_op(context)?;
    let term = self::ty_access_term(context, term)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn access_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    op_code: &'parsed lexer::ast::OpCode,
    term: &'parsed parser::ast::Term,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = access_op(context, op_code)?;
    let term = self::term(context, term)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn eval_fn_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    arg_exprs: &'parsed Vec<parser::ast::ArgExpr>,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = eval_fn_op(context)?;
    let term = apply_fn_term(context, arg_exprs)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn eval_spread_fn_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    expr: &'parsed parser::ast::Expr,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = eval_spread_fn_op(context)?;
    let term = apply_spread_fn_term(context, expr)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn eval_key_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    expr: &'parsed parser::ast::Expr,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = eval_key_op(context)?;
    let term = apply_key_term(context, expr)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn cast_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    ty_expr: &'parsed parser::ast::TyExpr,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = cast_op(context)?;
    let term = ty_expr_term(context, ty_expr)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn infix_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    op_code: &'parsed lexer::ast::OpCode,
    term: &'parsed parser::ast::Term,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = infix_op(context, op_code)?;
    let term = self::term(context, term)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn assign_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    term: &'parsed parser::ast::Term,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = assign_op(context)?;
    let term = self::term(context, term)?;
    let expr = Rc::new(ast::Expr {
        parsed: Some(node),
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn hidden_unit_expr<'parsed>(
    context: &Context,
) -> Result<Rc<ast::Expr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let term = Rc::new(ast::Term {
        parsed: None,
        detail: ast::TermDetail::None,
        ty: Ty::get_from_name(context, "unit".to_owned())
            .map_err(|e| e.convert(None))?,
    });
    Ok(Rc::new(ast::Expr {
        parsed: None,
        detail: ast::ExprDetail::Term {
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }))
}

pub fn ty_access_op<'parsed>(
    _context: &Context,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    Ok(ast::Op::TyAccess)
}

fn access_op<'parsed>(
    _context: &Context,
    node: &'parsed lexer::ast::OpCode,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    match node.kind {
        lexer::ast::OpCodeKind::Dot =>
            Ok(ast::Op::Access),
        lexer::ast::OpCodeKind::CoalescingAccess =>
            Ok(ast::Op::CoalescingAccess),
        _ =>
            panic!("Illegal state"),
    }
}

fn eval_fn_op<'parsed>(
    _context: &Context,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    Ok(ast::Op::EvalFn)
}

fn eval_spread_fn_op<'parsed>(
    _context: &Context,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    Ok(ast::Op::EvalSpreadFn)
}

fn eval_key_op<'parsed>(
    _context: &Context,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    Ok(ast::Op::EvalKey)
}

fn cast_op<'parsed>(
    _context: &Context,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    Ok(ast::Op::CastOp)
}

fn infix_op<'parsed>(
    _context: &Context,
    node: &'parsed lexer::ast::OpCode,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
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

fn assign_op<'parsed>(
    _context: &Context,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    Ok(ast::Op::Assign)
}

pub fn term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::TermKind::PrefixOp { op_code, term } =>
            prefix_op_term(context, node, op_code, term),
        parser::ast::TermKind::Block { stats } =>
            block_term(context, node, stats),
        parser::ast::TermKind::Paren { expr } =>
            paren_term(context, node, expr),
        parser::ast::TermKind::Tuple { exprs } =>
            tuple_term(context, node, exprs),
        parser::ast::TermKind::ArrayCtor { iter_expr } =>
            array_ctor_term(context, node, iter_expr),
        parser::ast::TermKind::Literal { literal } =>
            literal_term(context, node, literal),
        parser::ast::TermKind::ThisLiteral { literal } =>
            this_literal_term(context, node, literal),
        parser::ast::TermKind::InterpolatedString { interpolated_string } =>
            interpolated_string_term(context, node, interpolated_string),
        parser::ast::TermKind::EvalVar { ident } =>
            eval_var_term(context, node, ident),
        parser::ast::TermKind::LetInBind { var_bind, in_keyword: _, expr } =>
            let_in_bind_term(context, node, var_bind, expr),
        parser::ast::TermKind::If { if_keyword: _, condition, if_part, else_part } =>
            if_term(context, node, condition, if_part, else_part),
        parser::ast::TermKind::While { while_keyword: _, condition, stats } =>
            while_term(context, node, condition, stats),
        parser::ast::TermKind::Loop { loop_keyword: _, stats } =>
            loop_term(context, node, stats),
        parser::ast::TermKind::For { for_binds, stats } =>
            for_term(context, node, for_binds, stats),
        parser::ast::TermKind::Closure { var_decl, expr } =>
            closure_term(context, node, var_decl, expr),
    }
}

fn ty_access_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::TermKind::EvalVar { ident } =>
            eval_var_ty_access_term(context, node, ident),
        _ =>
            Err(vec![SemanticError::new(Some(node.slice), "Illegal use of type access op `::`".to_owned())])
    }
}

fn prefix_op_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    op_code: &'parsed lexer::ast::OpCode,
    term: &'parsed parser::ast::Term,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let op = prefix_op(context, op_code)?;
    let term = self::term(context, term)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::PrefixOp {
            op,
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }))
}

fn block_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    stats: &'parsed parser::ast::StatsBlock,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let stats = stats_block(context, stats)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::Block {
            stats: stats.clone(),
        },
        ty: stats.ret.ty.clone(),
    }))
}

fn paren_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    expr: &'parsed parser::ast::Expr,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let expr = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::Paren {
            expr: expr.clone(),
        },
        ty: expr.ty.clone(),
    }))
}

fn tuple_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    exprs: &'parsed Vec<Rc<parser::ast::Expr>>,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let exprs =
        exprs.iter()
        .map(|x| expr(context, x))
        .collect::<Result<_, _>>()?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::Tuple {
            exprs,
        },
        ty: Ty::get_from_name(context, "tuple".to_owned())
            .map_err(|e| e.convert(None))?, // TODO
    }))
}

fn array_ctor_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    iter_expr: &'parsed Option<parser::ast::IterExpr>,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let iter_expr = match iter_expr {
        Some(x) => self::iter_expr(context, x)?,
        None => empty_iter_expr(context)?,
    };
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::ArrayCtor {
            iter_expr,
        },
        ty: Ty::get_from_name(context, "array".to_owned())
            .map_err(|e| e.convert(None))?, // TODO
    }))
}

fn literal_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    literal: &'parsed lexer::ast::Literal,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let literal = self::literal(context, literal)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::Literal {
            literal: literal.clone(),
        },
        ty: literal.ty.clone(),
    }))
}

fn this_literal_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    _literal: &'parsed lexer::ast::Literal,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::ThisLiteral,
        ty: Ty::get_from_name(context, "udon".to_owned())
            .map_err(|e| e.convert(None))?,
    }))
}

fn interpolated_string_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    interpolated_string: &'parsed lexer::ast::InterpolatedString,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let interpolated_string = self::interpolated_string(context, interpolated_string)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::InterpolatedString {
            interpolated_string,
        },
        ty: Ty::get_from_name(context, "string".to_owned())
            .map_err(|e| e.convert(None))?,
    }))
}

fn eval_var_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    ident: &'parsed lexer::ast::Ident,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let ident = self::ident(context, ident)?;
    let var =
        VarKey::new(QualKey::top(), ident.name.clone()).get_value(context)
        .map_err(|e| e.convert(Some(node.slice)))?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::EvalVar {
            ident,
            var: RefCell::new(var.clone()),
        },
        ty: var.ty.clone(),
    }))
}

fn eval_var_ty_access_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    ident: &'parsed lexer::ast::Ident,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let ident = self::ident(context, ident)?;
    let var =
        Var::unknown(context)
        .map_err(|e| e.convert(None))?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::EvalVar {
            ident,
            var: RefCell::new(var.clone()),
        },
        ty: var.ty.clone(),
    }))
}

fn let_in_bind_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    var_bind: &'parsed parser::ast::VarBind,
    expr: &'parsed parser::ast::Expr,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let var_bind = self::var_bind(context, var_bind)?;
    let expr = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::LetInBind {
            var_bind,
            expr: expr.clone(),
        },
        ty: expr.ty.clone(),
    }))
}

fn if_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    condition: &'parsed parser::ast::Expr,
    if_part: &'parsed parser::ast::StatsBlock,
    else_part: &'parsed Option<(lexer::ast::Keyword, parser::ast::StatsBlock)>,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let condition = expr(context, condition)?;
    let if_part = stats_block(context, if_part)?;
    let else_part = match else_part {
        Some((_, stats)) => Some(stats_block(context, stats)?),
        None => None,
    };
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::If {
            condition,
            if_part: if_part.clone(),
            else_part,
        },
        ty: if_part.ret.ty.clone(), // TODO
    }))
}

fn while_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    condition: &'parsed parser::ast::Expr,
    stats: &'parsed parser::ast::StatsBlock,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let condition = expr(context, condition)?;
    let stats = stats_block(context, stats)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::While {
            condition,
            stats,
        },
        ty: Ty::get_from_name(context, "unit".to_owned())
            .map_err(|e| e.convert(None))?,
    }))
}

fn loop_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    stats: &'parsed parser::ast::StatsBlock,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let stats = stats_block(context, stats)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::Loop {
            stats,
        },
        ty: Ty::get_from_name(context, "unit".to_owned())
            .map_err(|e| e.convert(None))?,
    }))
}

fn for_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    for_binds: &'parsed Vec<(lexer::ast::Keyword, parser::ast::ForBind)>,
    stats: &'parsed parser::ast::StatsBlock,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let for_binds =
        for_binds.iter()
        .map(|x| for_bind(context, &x.1))
        .collect::<Result<_, _>>()?;
    let stats = stats_block(context, stats)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::For {
            for_binds,
            stats,
        },
        ty: Ty::get_from_name(context, "unit".to_owned())
            .map_err(|e| e.convert(None))?,
    }))
}

fn closure_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    var_decl: &'parsed parser::ast::VarDecl,
    expr: &'parsed parser::ast::Expr,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let var_decl = self::var_decl(context, var_decl)?;
    let expr = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        parsed: Some(node),
        detail: ast::TermDetail::Closure {
            var_decl,
            expr,
        },
        ty: Ty::get_from_name(context, "closure".to_owned())
            .map_err(|e| e.convert(None))?, // TODO
    }))
}

fn ty_expr_term<'parsed>(
    context: &Context,
    ty_expr: &'parsed parser::ast::TyExpr,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let te = self::ty_expr(context, ty_expr)?;
    Ok(Rc::new(ast::Term {
        parsed: None,
        detail: ast::TermDetail::TyExpr {
            ty_expr: te,
        },
        ty: Ty::get_from_name(context, "type".to_owned())
            .map_err(|e| e.convert(None))?, // TODO
    }))
}

fn apply_fn_term<'parsed>(
    context: &Context,
    arg_exprs: &'parsed Vec<parser::ast::ArgExpr>,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let args =
        arg_exprs.iter()
        .map(|x| arg_expr(context, x))
        .collect::<Result<_, _>>()?;
    Ok(Rc::new(ast::Term {
        parsed: None,
        detail: ast::TermDetail::ApplyFn {
            args,
        },
        ty: Ty::get_from_name(context, "unit".to_owned())
            .map_err(|e| e.convert(None))?,
    }))
}

fn apply_spread_fn_term<'parsed>(
    context: &Context,
    expr: &'parsed parser::ast::Expr,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let arg = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        parsed: None,
        detail: ast::TermDetail::ApplySpreadFn {
            arg,
        },
        ty: Ty::get_from_name(context, "unit".to_owned())
            .map_err(|e| e.convert(None))?,
    }))
}

fn apply_key_term<'parsed>(
    context: &Context,
    expr: &'parsed parser::ast::Expr,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let key = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        parsed: None,
        detail: ast::TermDetail::ApplyKey {
            key,
        },
        ty: Ty::get_from_name(context, "unit".to_owned())
            .map_err(|e| e.convert(None))?,
    }))
}

fn prefix_op<'parsed>(
    _context: &Context,
    node: &'parsed lexer::ast::OpCode,
) -> Result<ast::PrefixOp, Vec<SemanticError<'parsed>>> {
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

pub fn iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::IterExpr,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::IterExprKind::Range { left, right } =>
            range_iter_expr(context, node, left, right),
        parser::ast::IterExprKind::SteppedRange { left, right, step } =>
            stepped_range_iter_expr(context, node, left, right, step),
        parser::ast::IterExprKind::Spread { expr } =>
            spread_iter_expr(context, node, expr),
        parser::ast::IterExprKind::Elements { exprs } =>
            elements_iter_expr(context, node, exprs),
    }
}

fn empty_iter_expr<'parsed>(
    _context: &Context,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::IterExpr { parsed: None, detail: ast::IterExprDetail::Empty })
}

fn range_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::IterExpr,
    left: &'parsed parser::ast::Expr,
    right: &'parsed parser::ast::Expr,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let left = expr(context, left)?;
    let right = expr(context, right)?;
    Ok(ast::IterExpr {
        parsed: Some(node),
        detail: ast::IterExprDetail::Range {
            left,
            right,
        },
    })
}

fn stepped_range_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::IterExpr,
    left: &'parsed parser::ast::Expr,
    right: &'parsed parser::ast::Expr,
    step: &'parsed parser::ast::Expr,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let left = expr(context, left)?;
    let right = expr(context, right)?;
    let step = expr(context, step)?;
    Ok(ast::IterExpr {
        parsed: Some(node),
        detail: ast::IterExprDetail::SteppedRange {
            left,
            right,
            step,
        },
    })
}

fn spread_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::IterExpr,
    expr: &'parsed parser::ast::Expr,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let expr = self::expr(context, expr)?;
    Ok(ast::IterExpr {
        parsed: Some(node),
        detail: ast::IterExprDetail::Spread {
            expr,
        },
    })
}

fn elements_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::IterExpr,
    exprs: &'parsed Vec<Rc<parser::ast::Expr>>,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let exprs =
        exprs.iter()
        .map(|x| expr(context, x))
        .collect::<Result<_, _>>()?;
    Ok(ast::IterExpr {
        parsed: Some(node),
        detail: ast::IterExprDetail::Elements {
            exprs,
        },
    })
}

pub fn arg_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ArgExpr,
) -> Result<ast::ArgExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let mut_attr = mut_attr(context, &node.mut_attr)?;
    let expr = expr(context, &node.expr)?;
    Ok(ast::ArgExpr {
        parsed: Some(node),
        mut_attr,
        expr,
    })
}

pub fn for_bind<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ForBind,
) -> Result<ast::ForBind<'parsed>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::ForBindKind::Let { let_keyword: _, var_decl, for_iter_expr } =>
            let_for_bind(context, node, var_decl, for_iter_expr),
        parser::ast::ForBindKind::Assign { left, for_iter_expr } =>
            assign_for_bind(context, node, left, for_iter_expr),
    }
}

fn let_for_bind<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ForBind,
    var_decl: &'parsed parser::ast::VarDecl,
    for_iter_expr: &'parsed parser::ast::ForIterExpr,
) -> Result<ast::ForBind<'parsed>, Vec<SemanticError<'parsed>>> {
    let var_decl = self::var_decl(context, var_decl)?;
    let for_iter_expr = self::for_iter_expr(context, for_iter_expr)?;
    Ok(ast::ForBind {
        parsed: Some(node),
        detail: ast::ForBindDetail::Let {
            var_decl,
            for_iter_expr,
        },
    })
}

fn assign_for_bind<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ForBind,
    left: &'parsed parser::ast::Expr,
    for_iter_expr: &'parsed parser::ast::ForIterExpr,
) -> Result<ast::ForBind<'parsed>, Vec<SemanticError<'parsed>>> {
    let left = self::expr(context, left)?;
    let for_iter_expr = self::for_iter_expr(context, for_iter_expr)?;
    Ok(ast::ForBind {
        parsed: Some(node),
        detail: ast::ForBindDetail::Assign {
            left,
            for_iter_expr,
        },
    })
}

pub fn for_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ForIterExpr,
) -> Result<ast::ForIterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
        parser::ast::ForIterExprKind::Range { left, right } =>
            range_for_iter_expr(context, node, left, right),
        parser::ast::ForIterExprKind::SteppedRange { left, right, step } =>
            stepped_range_for_iter_expr(context, node, left, right, step),
        parser::ast::ForIterExprKind::Spread { expr } =>
            spread_for_iter_expr(context, node, expr),
    }
}

fn range_for_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ForIterExpr,
    left: &'parsed parser::ast::Expr,
    right: &'parsed parser::ast::Expr,
) -> Result<ast::ForIterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let left = expr(context, left)?;
    let right = expr(context, right)?;
    Ok(ast::ForIterExpr {
        parsed: Some(node),
        detail: ast::ForIterExprDetail::Range {
            left,
            right,
        },
    })
}

fn stepped_range_for_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ForIterExpr,
    left: &'parsed parser::ast::Expr,
    right: &'parsed parser::ast::Expr,
    step: &'parsed parser::ast::Expr,
) -> Result<ast::ForIterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let left = expr(context, left)?;
    let right = expr(context, right)?;
    let step = expr(context, step)?;
    Ok(ast::ForIterExpr {
        parsed: Some(node),
        detail: ast::ForIterExprDetail::SteppedRange {
            left,
            right,
            step,
        },
    })
}

fn spread_for_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ForIterExpr,
    expr: &'parsed parser::ast::Expr,
) -> Result<ast::ForIterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let expr = self::expr(context, expr)?;
    Ok(ast::ForIterExpr {
        parsed: Some(node),
        detail: ast::ForIterExprDetail::Spread {
            expr,
        },
    })
}

pub fn ident<'parsed>(
    _context: &Context,
    node: &'parsed lexer::ast::Ident,
) -> Result<ast::Ident<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::Ident {
        parsed: Some(node),
        name: node.slice.to_owned(),
    })
}

pub fn literal<'parsed>(
    context: &Context,
    node: &'parsed lexer::ast::Literal,
) -> Result<Rc<Literal>, Vec<SemanticError<'parsed>>> {
    match &node.kind {
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

pub fn interpolated_string<'parsed>(
    context: &Context,
    node: &'parsed lexer::ast::InterpolatedString,
) -> Result<ast::InterpolatedString<'parsed>, Vec<SemanticError<'parsed>>> {
    let string_parts = node.string_parts.iter().map(|x| (*x).to_owned()).collect();
    let mut exprs = Vec::new();
    for x in &node.exprs {
        exprs.push(expr(context, x)?);
    }
    let exprs =
        node.exprs.iter()
        .map(|x| expr(context, x))
        .collect::<Result<_, _>>()?;
    Ok(ast::InterpolatedString {
        parsed: Some(node),
        string_parts,
        exprs,
    })
}

fn expr_tree<'parsed, ExprTree, SemanticOp, ParserExpr>(
    context: &Context,
    node: &'parsed ParserExpr,
    exprs: VecDeque<Rc<ExprTree>>,
    ops: VecDeque<SemanticOp>,
) -> Result<Rc<ExprTree>, Vec<SemanticError<'parsed>>>
where
    ExprTree: ast::ExprTree<'parsed, SemanticOp, ParserExpr>,
    SemanticOp: Clone,
{
    let mut es = exprs.into_iter().collect::<VecDeque<_>>();
    let mut os = ops.into_iter().collect::<VecDeque<_>>();
    for (pred, assoc) in ExprTree::priorities(context) {
        match assoc {
            ast::Assoc::Left =>
                (es, os) = left_assoc(context, node, pred, es, os)?,
            ast::Assoc::Right =>
                (es, os) = right_assoc(context, node, pred, es, os)?,
        }
    }
    if es.len() == 1 {
        Ok(es.pop_front().unwrap())
    }
    else {
        panic!("Illegal state")
    }
}

fn left_assoc<'parsed, ExprTree, SemanticOp, ParserExpr>(
    context: &Context,
    node: &'parsed ParserExpr,
    pred: &Box<dyn Fn(&SemanticOp) -> bool>,
    mut exprs: VecDeque<Rc<ExprTree>>,
    ops: VecDeque<SemanticOp>,
) -> Result<(VecDeque<Rc<ExprTree>>, VecDeque<SemanticOp>), Vec<SemanticError<'parsed>>>
where
    ExprTree: ast::ExprTree<'parsed, SemanticOp, ParserExpr>,
    SemanticOp: Clone,
{
    let expr_0 = exprs.pop_front().unwrap();
    let (mut acc_exprs, acc_ops, expr) =
        ops.into_iter().zip(exprs.into_iter())
        .fold(Ok::<_, Vec<SemanticError>>((VecDeque::new(), VecDeque::new(), expr_0)), |acc, x| {
            let mut acc = acc?;
            if pred(&x.0) {
                let infix_op = ExprTree::infix_op(context, node, acc.2, x.0, x.1)?;
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

fn right_assoc<'parsed, ExprTree, SemanticOp, ParserExpr>(
    context: &Context,
    node: &'parsed ParserExpr,
    pred: &Box<dyn Fn(&SemanticOp) -> bool>,
    mut exprs: VecDeque<Rc<ExprTree>>,
    ops: VecDeque<SemanticOp>,
) -> Result<(VecDeque<Rc<ExprTree>>, VecDeque<SemanticOp>), Vec<SemanticError<'parsed>>>
where
    ExprTree: ast::ExprTree<'parsed, SemanticOp, ParserExpr>,
    SemanticOp: Clone,
{
    let expr_0 = exprs.pop_back().unwrap();
    let (mut acc_exprs, acc_ops, expr) =
        ops.into_iter().rev().zip(exprs.into_iter().rev())
        .fold(Ok::<_, Vec<SemanticError>>((VecDeque::new(), VecDeque::new(), expr_0)), |acc, x| {
            let mut acc = acc?;
            if pred(&x.0) {
                let infix_op = ExprTree::infix_op(context, node, x.1, x.0, acc.2)?;
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

impl<'parsed> ast::ExprTree<'parsed, ast::TyOp, parser::ast::TyExpr<'parsed>> for ast::TyExpr<'parsed> {
    fn priorities(context: &Context) -> &Vec<(Box<dyn Fn(&ast::TyOp) -> bool>, ast::Assoc)> {
        &context.semantic_ty_op.priorities
    }

    fn infix_op(
        context: &Context,
        parsed: &'parsed parser::ast::TyExpr,
        left: Rc<Self>,
        op: ast::TyOp,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'parsed>>> {
        match &op {
            ast::TyOp::Access =>
                access_ty_infix_op(context, parsed, left, op, right),
        }
    }
}

fn access_ty_infix_op<'parsed>(
    context: &Context,
    parsed: &'parsed parser::ast::TyExpr,
    left: Rc<ast::TyExpr<'parsed>>,
    op: ast::TyOp,
    right: Rc<ast::TyExpr<'parsed>>,
) -> Result<Rc<ast::TyExpr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let ast::TyExprDetail::Term { term } = &right.detail
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` is not a term".to_owned())]);
        };
    let ast::TyTermDetail::EvalTy { ident, var } = &term.detail
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` cannot be evaluated".to_owned())]);
        };

    if left.ty.base_eq_with_name("qual") {
        let qual = left.ty.arg_as_qual();
        let v = VarKey::new(qual, ident.name.clone()).get_value(context)
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        var.replace(v.clone());
        Ok(Rc::new(ast::TyExpr {
            parsed: Some(parsed),
            detail: ast::TyExprDetail::InfixOp {
                left: left.clone(),
                op,
                right: right.clone(),
            },
            ty: v.ty.clone(),
        }))
    }
    else if left.ty.base_eq_with_name("type") {
        let parent = left.ty.arg_as_type().get_value(context)
            .map_err(|e| e.convert(left.parsed.map(|x| x.slice)))?;
        let qual = left.ty.base.qual.get_pushed_qual(context, parent.base.name.clone())
            .map_err(|e| e.convert(left.parsed.map(|x| x.slice)))?;
        let v = Var::get(context, qual.to_key(), ident.name.clone())
            .map_err(|e| e.convert(right.parsed.map(|x| x.slice)))?;
        var.replace(v.clone());
        Ok(Rc::new(ast::TyExpr {
            parsed: Some(parsed),
            detail: ast::TyExprDetail::InfixOp {
                left: left.clone(),
                op,
                right: right.clone(),
            },
            ty: v.ty.clone(),
        }))
    }
    else {
        Err(vec![SemanticError::new(None, "Left side of `::` is not a qualifier or a type".to_owned())])
    }
}

impl<'parsed> ast::ExprTree<'parsed, ast::Op, parser::ast::Expr<'parsed>> for ast::Expr<'parsed> {
    fn priorities(context: &Context) -> &Vec<(Box<dyn Fn(&ast::Op) -> bool>, ast::Assoc)> {
        &context.semantic_op.priorities
    }

    fn infix_op(
        context: &Context,
        parsed: &'parsed parser::ast::Expr,
        left: Rc<Self>,
        op: ast::Op,
        right: Rc<Self>,
    ) -> Result<Rc<Self>, Vec<SemanticError<'parsed>>> {
        match &op {
            ast::Op::Access =>
                ty_access_infix_op(context, parsed, left, op, right),
            _ =>
                panic!("Not implemented")
        }
    }
}

fn ty_access_infix_op<'parsed>(
    context: &Context,
    parsed: &'parsed parser::ast::Expr,
    left: Rc<ast::Expr<'parsed>>,
    op: ast::Op,
    right: Rc<ast::Expr<'parsed>>,
) -> Result<Rc<ast::Expr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let ast::ExprDetail::Term { term } = &right.detail
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` is not a term".to_owned())]);
        };
    let ast::TermDetail::EvalVar { ident, var } = &term.detail
        else {
            return Err(vec![SemanticError::new(None, "Right side of `::` cannot be evaluated".to_owned())]);
        };

    if left.ty.base_eq_with_name("qual") {
        let qual = left.ty.arg_as_qual();
        let v = VarKey::new(qual, ident.name.clone()).get_value(context)
            .map_err(|e| e.convert(Some(parsed.slice)))?;
        var.replace(v.clone());
        Ok(Rc::new(ast::Expr {
            parsed: Some(parsed),
            detail: ast::ExprDetail::InfixOp {
                left: left.clone(),
                op,
                right: right.clone(),
            },
            ty: v.ty.clone(),
        }))
    }
    else if left.ty.base_eq_with_name("type") {
        let parent = left.ty.arg_as_type().get_value(context)
            .map_err(|e| e.convert(left.parsed.map(|x| x.slice)))?;
        let qual = left.ty.base.qual.get_pushed_qual(context, parent.base.name.clone())
            .map_err(|e| e.convert(left.parsed.map(|x| x.slice)))?;
        let v = Var::get(context, qual.to_key(), ident.name.clone())
            .map_err(|e| e.convert(right.parsed.map(|x| x.slice)))?;
        var.replace(v.clone());
        Ok(Rc::new(ast::Expr {
            parsed: Some(parsed),
            detail: ast::ExprDetail::InfixOp {
                left: left.clone(),
                op,
                right: right.clone(),
            },
            ty: v.ty.clone(),
        }))
    }
    else {
        Err(vec![SemanticError::new(None, "Left side of `::` is not a qualifier or a type".to_owned())])
    }
}
