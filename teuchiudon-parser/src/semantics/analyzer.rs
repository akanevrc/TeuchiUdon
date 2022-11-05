use std::{
    collections::VecDeque,
    rc::Rc,
};
use function_name::named;
use crate::context::Context;
use crate::lexer;
use crate::parser;
use super::{
    ast,
    elements::{
        self,
        element::KeyElement,
    },
    SemanticError,
};

pub fn target<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Target,
) -> Result<ast::Target<'parsed>, Vec<SemanticError<'parsed>>> {
    let body = match &node.0 {
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
    let mut top_stats = Vec::new();
    for x in &node.0 {
        top_stats.push(top_stat(context, x)?);
    }
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
    match node {
        parser::ast::TopStat::VarBind(access_attr, sync_attr, var_bind) =>
            var_bind_top_stat(context, node, access_attr, sync_attr, var_bind),
        parser::ast::TopStat::FnBind(access_attr, fn_bind) =>
            fn_bind_top_stat(context, node, access_attr, fn_bind),
        parser::ast::TopStat::Stat(stat) =>
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
    Ok(ast::TopStat::VarBind {
        parsed: Some(node),
        access_attr,
        sync_attr,
        var_bind,
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
    Ok(ast::TopStat::FnBind {
        parsed: Some(node),
        access_attr,
        fn_bind,
    })
}

fn stat_top_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TopStat,
    stat: &'parsed parser::ast::Stat,
) -> Result<ast::TopStat<'parsed>, Vec<SemanticError<'parsed>>> {
    let stat = self::stat(context, stat)?;
    Ok(ast::TopStat::Stat {
        parsed: Some(node),
        stat,
    })
}

#[named]
pub fn access_attr<'parsed>(
    context: &Context,
    node: &'parsed Option<parser::ast::AccessAttr>,
) -> Result<ast::AccessAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    match node {
        Some(attr) =>
            match &attr.0.1 {
                lexer::ast::KeywordKind::Pub =>
                    pub_access_attr(context, attr),
                _ =>
                    Err(vec![
                        SemanticError { slice: None, message: format!("Illegal state, in {}", function_name!()) },
                    ]),
            },
        None =>
            Ok(ast::AccessAttr::None),
    }
    
}

fn pub_access_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::AccessAttr,
) -> Result<ast::AccessAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::AccessAttr::Pub {
        parsed: Some(node),
    })
}

#[named]
fn sync_attr<'parsed>(
    context: &Context,
    node: &'parsed Option<parser::ast::SyncAttr>,
) -> Result<ast::SyncAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    match node {
        Some(attr) =>
            match &attr.0.1 {
                lexer::ast::KeywordKind::Sync =>
                    sync_sync_attr(context, attr),
                lexer::ast::KeywordKind::Linear =>
                    linear_sync_attr(context, attr),
                lexer::ast::KeywordKind::Smooth =>
                    smooth_sync_attr(context, attr),
                _ =>
                    Err(vec![
                        SemanticError { slice: None, message: format!("Illegal state, in {}", function_name!()) },
                    ]),
            },
        None =>
            Ok(ast::SyncAttr::None),
    }
    
}

fn sync_sync_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::SyncAttr,
) -> Result<ast::SyncAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::SyncAttr::Sync {
        parsed: Some(node),
    })
}

fn linear_sync_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::SyncAttr,
) -> Result<ast::SyncAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::SyncAttr::Linear {
        parsed: Some(node),
    })
}

fn smooth_sync_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::SyncAttr,
) -> Result<ast::SyncAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::SyncAttr::Smooth {
        parsed: Some(node),
    })
}

pub fn var_bind<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::VarBind,
) -> Result<ast::VarBind<'parsed>, Vec<SemanticError<'parsed>>> {
    let var_decl = var_decl(context, &node.1)?;
    let expr = expr(context, &node.2)?;
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
    match node {
        parser::ast::VarDecl::SingleDecl(mut_attr, ident, type_expr) =>
            single_var_decl(context, node, mut_attr, ident, type_expr),
        parser::ast::VarDecl::TupleDecl(var_decls) =>
            tuple_var_decl(context, node, var_decls),
    }
}

fn single_var_decl<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::VarDecl,
    mut_attr: &'parsed Option<parser::ast::MutAttr>,
    ident: &'parsed lexer::ast::Ident,
    type_expr: &'parsed Option<Rc<parser::ast::TypeExpr>>,
) -> Result<ast::VarDecl<'parsed>, Vec<SemanticError<'parsed>>> {
    let mut_attr = self::mut_attr(context, mut_attr)?;
    let ident = self::ident(context, ident)?;
    let type_expr = match type_expr {
        Some(x) => self::type_expr(context, x)?,
        None => hidden_unknown_type_expr(context)?,
    };
    let var = elements::var::Var::new(
        context,
        elements::qual::Qual::TOP,
        ident.name.clone(),
        type_expr.ty.clone(),
        matches!(mut_attr, ast::MutAttr::Mut { parsed: _ }),
        false,
    );
    Ok(ast::VarDecl::SingleDecl {
        parsed: Some(node),
        mut_attr,
        ident,
        type_expr,
        var: var.clone(),
    })
}

fn tuple_var_decl<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::VarDecl,
    var_decls: &'parsed Vec<parser::ast::VarDecl>,
) -> Result<ast::VarDecl<'parsed>, Vec<SemanticError<'parsed>>> {
    let mut vs = Vec::new();
    for x in var_decls {
        vs.push(var_decl(context, x)?);
    }
    Ok(ast::VarDecl::TupleDecl {
        parsed: Some(node),
        var_decls: vs,
    })
}

#[named]
pub fn mut_attr<'parsed>(
    context: &Context,
    node: &'parsed Option<parser::ast::MutAttr>,
) -> Result<ast::MutAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    match node {
        Some(attr) =>
            match &attr.0.1 {
                lexer::ast::KeywordKind::Mut =>
                    mut_mut_attr(context, attr),
                _ =>
                    Err(vec![
                        SemanticError { slice: None, message: format!("Illegal state, in {}", function_name!()) },
                    ]),
            },
        None =>
            Ok(ast::MutAttr::None),
    }
}

fn mut_mut_attr<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::MutAttr,
) -> Result<ast::MutAttr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::MutAttr::Mut {
        parsed: Some(node),
    })
}

pub fn fn_bind<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::FnBind,
) -> Result<ast::FnBind<'parsed>, Vec<SemanticError<'parsed>>> {
    let fn_decl = fn_decl(context, &node.1)?;
    let stats_block = stats_block(context, &node.2)?;
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
    let ident = ident(context, &node.0)?;
    let var_decl = var_decl(context, &node.1)?;
    let type_expr = match &node.2 {
        Some(x) => type_expr(context, x)?,
        None => hidden_unit_type_expr(context)?,
    };
    Ok(ast::FnDecl {
        parsed: Some(node),
        ident,
        var_decl,
        type_expr,
    })
}

pub fn type_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TypeExpr,
) -> Result<Rc<ast::TypeExpr<'parsed>>, Vec<SemanticError<'parsed>>> {
    construct_type_expr_tree(context, node)
}

fn construct_type_expr_tree<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TypeExpr,
) -> Result<Rc<ast::TypeExpr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let mut exprs = VecDeque::new();
    let mut ops = VecDeque::new();
    let term = type_term(context, &node.0)?;
    exprs.push_back(Rc::new(ast::TypeExpr {
        detail: ast::TypeExprDetail::Term {
            parsed: Some(node),
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }));
    for op in &node.1 {
        let (op, expr) = match op {
            parser::ast::TypeOp::Access(_, term) =>
                access_op_type_expr(context, node, term)?,
        };
        ops.push_back(op);
        exprs.push_back(expr);
    }
    expr_tree(context, node, exprs, ops)
}

fn access_op_type_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TypeExpr,
    term: &'parsed parser::ast::TypeTerm,
) -> Result<(ast::TypeOp, Rc<ast::TypeExpr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = access_type_op(context)?;
    let term = type_term(context, term)?;
    let expr = Rc::new(ast::TypeExpr {
        detail: ast::TypeExprDetail::Term {
            parsed: Some(node),
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn hidden_unknown_type_expr<'parsed>(
    context: &Context,
) -> Result<Rc<ast::TypeExpr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let term = Rc::new(ast::TypeTerm {
        detail: ast::TypeTermDetail::None,
        ty: elements::ty::TyKey::from_name("unknown").consume_key(context).unwrap(),
    });
    Ok(Rc::new(ast::TypeExpr {
        detail: ast::TypeExprDetail::Term {
            parsed: None,
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }))
}

fn hidden_unit_type_expr<'parsed>(
    context: &Context,
) -> Result<Rc<ast::TypeExpr<'parsed>>, Vec<SemanticError<'parsed>>> {
    let term = Rc::new(ast::TypeTerm {
        detail: ast::TypeTermDetail::None,
        ty: elements::ty::TyKey::from_name("unit").consume_key(context).unwrap(),
    });
    Ok(Rc::new(ast::TypeExpr {
        detail: ast::TypeExprDetail::Term {
            parsed: None,
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }))
}

fn access_type_op<'parsed>(
    _context: &Context,
) -> Result<ast::TypeOp, Vec<SemanticError<'parsed>>> {
    Ok(ast::TypeOp::Access)
}

pub fn type_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TypeTerm,
) -> Result<Rc<ast::TypeTerm<'parsed>>, Vec<SemanticError<'parsed>>> {
    match node {
        parser::ast::TypeTerm::EvalType(ident) =>
            eval_type_type_term(context, node, ident),
    }
}

fn eval_type_type_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::TypeTerm,
    ident: &'parsed lexer::ast::Ident,
) -> Result<Rc<ast::TypeTerm<'parsed>>, Vec<SemanticError<'parsed>>> {
    let ident = self::ident(context, ident)?;
    Ok(Rc::new(ast::TypeTerm {
        detail: ast::TypeTermDetail::EvalType {
            parsed: Some(node),
            ident
        },
        ty: elements::ty::TyKey::from_name("int").consume_key(context).unwrap(), // TODO
    }))
}

pub fn stats_block<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::StatsBlock,
) -> Result<ast::StatsBlock<'parsed>, Vec<SemanticError<'parsed>>> {
    let mut stats = Vec::new();
    for x in &node.0 {
        stats.push(stat(context, x)?);
    }
    let ret = match &node.1 {
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
    match node {
        parser::ast::Stat::Return(_, expr) =>
            return_stat(context, node, expr),
        parser::ast::Stat::Continue(_) =>
            continue_stat(context, node),
        parser::ast::Stat::Break(_) =>
            break_stat(context, node),
        parser::ast::Stat::VarBind(var_bind) =>
            var_bind_stat(context, node, var_bind),
        parser::ast::Stat::FnBind(fn_bind) =>
            fn_bind_stat(context, node, fn_bind),
        parser::ast::Stat::Expr(expr) =>
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
    Ok(ast::Stat::Return {
        parsed: Some(node),
        expr,
    })
}

fn continue_stat<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::Stat,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::Stat::Continue {
        parsed: Some(node),
    })
}

fn break_stat<'parsed>(
    _context: &Context,
    node: &'parsed parser::ast::Stat,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::Stat::Break {
        parsed: Some(node),
    })
}

fn var_bind_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Stat,
    var_bind: &'parsed parser::ast::VarBind,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    let var_bind = self::var_bind(context, var_bind)?;
    Ok(ast::Stat::VarBind {
        parsed: Some(node),
        var_bind,
    })
}

fn fn_bind_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Stat,
    fn_bind: &'parsed parser::ast::FnBind,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    let fn_bind = self::fn_bind(context, fn_bind)?;
    Ok(ast::Stat::FnBind {
        parsed: Some(node),
        fn_bind,
    })
}

fn expr_stat<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Stat,
    expr: &'parsed parser::ast::Expr,
) -> Result<ast::Stat<'parsed>, Vec<SemanticError<'parsed>>> {
    let expr = self::expr(context, expr)?;
    Ok(ast::Stat::Expr {
        parsed: Some(node),
        expr,
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
    let term = self::term(context, &node.0)?;
    exprs.push_back(Rc::new(ast::Expr {
        detail: ast::ExprDetail::Term {
            parsed: Some(node),
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }));
    for parser_op in &node.1 {
        let (op, expr) = match parser_op {
            parser::ast::Op::TypeAccess(_, term) =>
                type_access_op_expr(context, node, term)?,
            parser::ast::Op::Access(op_code, term) =>
                access_op_expr(context, node, op_code, term)?,
            parser::ast::Op::EvalFn(arg_exprs) =>
                eval_fn_op_expr(context, node, arg_exprs)?,
            parser::ast::Op::EvalSpreadFn(expr) =>
                eval_spread_fn_op_expr(context, node, expr)?,
            parser::ast::Op::EvalKey(expr) =>
                eval_key_op_expr(context, node, expr)?,
            parser::ast::Op::CastOp(_, type_expr) =>
                cast_op_expr(context, node, type_expr)?,
            parser::ast::Op::InfixOp(op_code, term) =>
                infix_op_expr(context, node, op_code, term)?,
            parser::ast::Op::Assign(term) =>
                assign_op_expr(context, node, term)?,
        };
        ops.push_back(op);
        exprs.push_back(expr);
    }
    expr_tree(context, node, exprs, ops)
}

fn type_access_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    term: &'parsed parser::ast::Term,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = type_access_op(context)?;
    let term = self::term(context, term)?;
    let expr = Rc::new(ast::Expr {
        detail: ast::ExprDetail::Term {
            parsed: Some(node),
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
        detail: ast::ExprDetail::Term {
            parsed: Some(node),
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
        detail: ast::ExprDetail::Term {
            parsed: Some(node),
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
        detail: ast::ExprDetail::Term {
            parsed: Some(node),
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
        detail: ast::ExprDetail::Term {
            parsed: Some(node),
            term: term.clone(),
        },
        ty: term.ty.clone(),
    });
    Ok((op, expr))
}

fn cast_op_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Expr,
    type_expr: &'parsed parser::ast::TypeExpr,
) -> Result<(ast::Op, Rc<ast::Expr<'parsed>>), Vec<SemanticError<'parsed>>> {
    let op = cast_op(context)?;
    let term = type_expr_term(context, type_expr)?;
    let expr = Rc::new(ast::Expr {
        detail: ast::ExprDetail::Term {
            parsed: Some(node),
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
        detail: ast::ExprDetail::Term {
            parsed: Some(node),
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
        detail: ast::ExprDetail::Term {
            parsed: Some(node),
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
        detail: ast::TermDetail::None,
        ty: elements::ty::TyKey::from_name("unit").consume_key(context).unwrap(),
    });
    Ok(Rc::new(ast::Expr {
        detail: ast::ExprDetail::Term {
            parsed: None,
            term: term.clone(),
        },
        ty: term.ty.clone(),
    }))
}

pub fn type_access_op<'parsed>(
    _context: &Context,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    Ok(ast::Op::TypeAccess)
}

#[named]
fn access_op<'parsed>(
    _context: &Context,
    node: &'parsed lexer::ast::OpCode,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    match node.1 {
        lexer::ast::OpCodeKind::Dot =>
            Ok(ast::Op::Access),
        lexer::ast::OpCodeKind::CoalescingAccess =>
            Ok(ast::Op::CoalescingAccess),
        _ =>
            Err(vec![
                SemanticError { slice: None, message: format!("Illegal state, in {}", function_name!()) },
            ]),
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

#[named]
fn infix_op<'parsed>(
    _context: &Context,
    node: &'parsed lexer::ast::OpCode,
) -> Result<ast::Op, Vec<SemanticError<'parsed>>> {
    match node.1 {
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
            Err(vec![
                SemanticError { slice: None, message: format!("Illegal state, in {}", function_name!()) },
            ]),
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
    match node {
        parser::ast::Term::PrefixOp(op_code, term) =>
            prefix_op_term(context, node, op_code, term),
        parser::ast::Term::Block(stats) =>
            block_term(context, node, stats),
        parser::ast::Term::Paren(expr) =>
            paren_term(context, node, expr),
        parser::ast::Term::Tuple(exprs) =>
            tuple_term(context, node, exprs),
        parser::ast::Term::ArrayCtor(iter_expr) =>
            array_ctor_term(context, node, iter_expr),
        parser::ast::Term::Literal(literal) =>
            literal_term(context, node, literal),
        parser::ast::Term::ThisLiteral(literal) =>
            this_literal_term(context, node, literal),
        parser::ast::Term::InterpolatedString(interpolated_string) =>
            interpolated_string_term(context, node, interpolated_string),
        parser::ast::Term::EvalVar(ident) =>
            eval_var_term(context, node, ident),
        parser::ast::Term::LetInBind(var_bind, _, expr) =>
            let_in_bind_term(context, node, var_bind, expr),
        parser::ast::Term::If(_, condition, if_part, else_part) =>
            if_term(context, node, condition, if_part, else_part),
        parser::ast::Term::While(_, condition, stats) =>
            while_term(context, node, condition, stats),
        parser::ast::Term::Loop(_, stats) =>
            loop_term(context, node, stats),
        parser::ast::Term::For(for_binds, stats) =>
            for_term(context, node, for_binds, stats),
        parser::ast::Term::Closure(var_decl, expr) =>
            closure_term(context, node, var_decl, expr),
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
        detail: ast::TermDetail::PrefixOp {
            parsed: Some(node),
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
        detail: ast::TermDetail::Block {
            parsed: Some(node),
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
        detail: ast::TermDetail::Paren {
            parsed: Some(node),
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
    let mut es = Vec::new();
    for x in exprs {
        es.push(expr(context, x)?);
    }
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::Tuple {
            parsed: Some(node),
            exprs: es,
        },
        ty: elements::ty::TyKey::from_name("tuple").consume_key(context).unwrap(), // TODO
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
        detail: ast::TermDetail::ArrayCtor {
            parsed: Some(node),
            iter_expr,
        },
        ty: elements::ty::TyKey::from_name("array").consume_key(context).unwrap(), // TODO
    }))
}

fn literal_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    literal: &'parsed lexer::ast::Literal,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let literal = self::literal(context, literal)?;
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::Literal {
            parsed: Some(node),
            literal: literal.clone(),
        },
        ty: literal.ty.clone(),
    }))
}

fn this_literal_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    literal: &'parsed lexer::ast::Literal,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let literal = this_literal(context, literal)?;
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::ThisLiteral {
            parsed: Some(node),
            literal,
        },
        ty: elements::ty::TyKey::from_name("udon").consume_key(context).unwrap(),
    }))
}

fn interpolated_string_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    interpolated_string: &'parsed lexer::ast::InterpolatedString,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let interpolated_string = self::interpolated_string(context, interpolated_string)?;
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::InterpolatedString {
            parsed: Some(node),
            interpolated_string,
        },
        ty: elements::ty::TyKey::from_name("string").consume_key(context).unwrap(),
    }))
}

fn eval_var_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    ident: &'parsed lexer::ast::Ident,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let ident = self::ident(context, ident)?;
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::EvalVar {
            parsed: Some(node),
            ident,
        },
        ty: elements::ty::TyKey::from_name("int").consume_key(context).unwrap(), // TODO
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
        detail: ast::TermDetail::LetInBind {
            parsed: Some(node),
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
        detail: ast::TermDetail::If {
            parsed: Some(node),
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
        detail: ast::TermDetail::While {
            parsed: Some(node),
            condition,
            stats,
        },
        ty: elements::ty::TyKey::from_name("unit").consume_key(context).unwrap(),
    }))
}

fn loop_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    stats: &'parsed parser::ast::StatsBlock,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let stats = stats_block(context, stats)?;
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::Loop {
            parsed: Some(node),
            stats,
        },
        ty: elements::ty::TyKey::from_name("unit").consume_key(context).unwrap(),
    }))
}

fn for_term<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::Term,
    for_binds: &'parsed Vec<(lexer::ast::Keyword, parser::ast::ForBind)>,
    stats: &'parsed parser::ast::StatsBlock,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let mut fbs = Vec::new();
    for x in for_binds {
        fbs.push(for_bind(context, &x.1)?);
    }
    let stats = stats_block(context, stats)?;
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::For {
            parsed: Some(node),
            for_binds: fbs,
            stats,
        },
        ty: elements::ty::TyKey::from_name("unit").consume_key(context).unwrap(),
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
        detail: ast::TermDetail::Closure {
            parsed: Some(node),
            var_decl,
            expr,
        },
        ty: elements::ty::TyKey::from_name("closure").consume_key(context).unwrap(), // TODO
    }))
}

fn type_expr_term<'parsed>(
    context: &Context,
    type_expr: &'parsed parser::ast::TypeExpr,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let te = self::type_expr(context, type_expr)?;
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::TypeExpr {
            parsed: Some(type_expr),
            type_expr: te,
        },
        ty: elements::ty::TyKey::from_name("type").consume_key(context).unwrap(), // TODO
    }))
}

fn apply_fn_term<'parsed>(
    context: &Context,
    arg_exprs: &'parsed Vec<parser::ast::ArgExpr>,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let mut args = Vec::new();
    for x in arg_exprs {
        args.push(arg_expr(context, x)?);
    }
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::ApplyFn {
            parsed: Some(arg_exprs),
            args,
        },
        ty: elements::ty::TyKey::from_name("unit").consume_key(context).unwrap(),
    }))
}

fn apply_spread_fn_term<'parsed>(
    context: &Context,
    expr: &'parsed parser::ast::Expr,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let arg = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::ApplySpreadFn {
            parsed: Some(expr),
            arg,
        },
        ty: elements::ty::TyKey::from_name("unit").consume_key(context).unwrap(),
    }))
}

fn apply_key_term<'parsed>(
    context: &Context,
    expr: &'parsed parser::ast::Expr,
) -> Result<Rc<ast::Term<'parsed>>, Vec<SemanticError<'parsed>>> {
    let key = self::expr(context, expr)?;
    Ok(Rc::new(ast::Term {
        detail: ast::TermDetail::ApplyKey {
            parsed: Some(expr),
            key,
        },
        ty: elements::ty::TyKey::from_name("unit").consume_key(context).unwrap(),
    }))
}

#[named]
fn prefix_op<'parsed>(
    _context: &Context,
    node: &'parsed lexer::ast::OpCode,
) -> Result<ast::PrefixOp, Vec<SemanticError<'parsed>>> {
    match node.1 {
        lexer::ast::OpCodeKind::Plus =>
            Ok(ast::PrefixOp::Plus),
        lexer::ast::OpCodeKind::Minus =>
            Ok(ast::PrefixOp::Minus),
        lexer::ast::OpCodeKind::Bang =>
            Ok(ast::PrefixOp::Bang),
        lexer::ast::OpCodeKind::Tilde =>
            Ok(ast::PrefixOp::Tilde),
        _ =>
            Err(vec![
                SemanticError { slice: None, message: format!("Illegal state, in {}", function_name!()) },
            ]),
    }
}

pub fn iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::IterExpr,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    match node {
        parser::ast::IterExpr::Range(left, right) =>
            range_iter_expr(context, node, left, right),
        parser::ast::IterExpr::SteppedRange(left, right, step) =>
            stepped_range_iter_expr(context, node, left, right, step),
        parser::ast::IterExpr::Spread(expr) =>
            spread_iter_expr(context, node, expr),
        parser::ast::IterExpr::Elements(exprs) =>
            elements_iter_expr(context, node, exprs),
    }
}

fn empty_iter_expr<'parsed>(
    _context: &Context,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::IterExpr::Empty)
}

fn range_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::IterExpr,
    left: &'parsed parser::ast::Expr,
    right: &'parsed parser::ast::Expr,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let left = expr(context, left)?;
    let right = expr(context, right)?;
    Ok(ast::IterExpr::Range {
        parsed: Some(node),
        left,
        right,
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
    Ok(ast::IterExpr::SteppedRange {
        parsed: Some(node),
        left,
        right,
        step,
    })
}

fn spread_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::IterExpr,
    expr: &'parsed parser::ast::Expr,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let expr = self::expr(context, expr)?;
    Ok(ast::IterExpr::Spread {
        parsed: Some(node),
        expr,
    })
}

fn elements_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::IterExpr,
    exprs: &'parsed Vec<Rc<parser::ast::Expr>>,
) -> Result<ast::IterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let mut es = Vec::new();
    for x in exprs {
        es.push(expr(context, x)?);
    }
    Ok(ast::IterExpr::Elements {
        parsed: Some(node),
        exprs: es,
    })
}

pub fn arg_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ArgExpr,
) -> Result<ast::ArgExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let mut_attr = mut_attr(context, &node.0)?;
    let expr = expr(context, &node.1)?;
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
    match node {
        parser::ast::ForBind::Let(_, var_decl, for_iter_expr) =>
            let_for_bind(context, node, var_decl, for_iter_expr),
        parser::ast::ForBind::Assign(left, for_iter_expr) =>
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
    Ok(ast::ForBind::Let {
        parsed: Some(node),
        var_decl,
        for_iter_expr,
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
    Ok(ast::ForBind::Assign {
        parsed: Some(node),
        left,
        for_iter_expr,
    })
}

pub fn for_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ForIterExpr,
) -> Result<ast::ForIterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    match node {
        parser::ast::ForIterExpr::Range(left, right) =>
            range_for_iter_expr(context, node, left, right),
        parser::ast::ForIterExpr::SteppedRange(left, right, step) =>
            stepped_range_for_iter_expr(context, node, left, right, step),
        parser::ast::ForIterExpr::Spread(expr) =>
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
    Ok(ast::ForIterExpr::Range {
        parsed: Some(node),
        left,
        right,
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
    Ok(ast::ForIterExpr::SteppedRange {
        parsed: Some(node),
        left,
        right,
        step,
    })
}

fn spread_for_iter_expr<'parsed>(
    context: &Context,
    node: &'parsed parser::ast::ForIterExpr,
    expr: &'parsed parser::ast::Expr,
) -> Result<ast::ForIterExpr<'parsed>, Vec<SemanticError<'parsed>>> {
    let expr = self::expr(context, expr)?;
    Ok(ast::ForIterExpr::Spread {
        parsed: Some(node),
        expr,
    })
}

pub fn ident<'parsed>(
    _context: &Context,
    node: &'parsed lexer::ast::Ident,
) -> Result<ast::Ident<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::Ident {
        parsed: Some(node),
        name: node.0.to_owned(),
    })
}

#[named]
pub fn literal<'parsed>(
    context: &Context,
    node: &'parsed lexer::ast::Literal,
) -> Result<Rc<elements::literal::Literal>, Vec<SemanticError<'parsed>>> {
    match node {
        lexer::ast::Literal::Unit(op_code, _) =>
            elements::literal::Literal::new_unit(context)
            .map_err(|e| vec![e.convert(Some(op_code.0))]),
        lexer::ast::Literal::Null(keyword) =>
            elements::literal::Literal::new_null(context)
            .map_err(|e| vec![e.convert(Some(keyword.0))]),
        lexer::ast::Literal::Bool(lexer::ast::Keyword(s, _)) =>
            elements::literal::Literal::new_bool(context, (*s).to_owned())
            .map_err(|e| vec![e.convert(Some(s))]),
        lexer::ast::Literal::PureInteger(s) =>
            elements::literal::Literal::new_pure_integer(context, (*s).to_owned())
            .map_err(|e| vec![e.convert(Some(s))]),
        lexer::ast::Literal::DecInteger(s) =>
            elements::literal::Literal::new_dec_integer(context, (*s).to_owned())
            .map_err(|e| vec![e.convert(Some(s))]),
        lexer::ast::Literal::HexInteger(s) =>
            elements::literal::Literal::new_hex_integer(context, (*s).to_owned())
            .map_err(|e| vec![e.convert(Some(s))]),
        lexer::ast::Literal::BinInteger(s) =>
            elements::literal::Literal::new_bin_integer(context, (*s).to_owned())
            .map_err(|e| vec![e.convert(Some(s))]),
        lexer::ast::Literal::RealNumber(s) =>
            elements::literal::Literal::new_real_number(context, (*s).to_owned())
            .map_err(|e| vec![e.convert(Some(s))]),
        lexer::ast::Literal::Character(s) =>
            elements::literal::Literal::new_character(context, (*s).to_owned())
            .map_err(|e| vec![e.convert(Some(s))]),
        lexer::ast::Literal::RegularString(s) =>
            elements::literal::Literal::new_regular_string(context, (*s).to_owned())
            .map_err(|e| vec![e.convert(Some(s))]),
        lexer::ast::Literal::VerbatiumString(s) =>
            elements::literal::Literal::new_verbatium_string(context, (*s).to_owned())
            .map_err(|e| vec![e.convert(Some(s))]),
        _ =>
            Err(vec![
                SemanticError { slice: None, message: format!("Illegal state, in {}", function_name!()) },
            ]),
        
    }
}

fn this_literal<'parsed>(
    _context: &Context,
    node: &'parsed lexer::ast::Literal,
) -> Result<ast::ThisLiteral<'parsed>, Vec<SemanticError<'parsed>>> {
    Ok(ast::ThisLiteral {
        parsed: Some(node),
    })
}

pub fn interpolated_string<'parsed>(
    context: &Context,
    node: &'parsed lexer::ast::InterpolatedString,
) -> Result<ast::InterpolatedString<'parsed>, Vec<SemanticError<'parsed>>> {
    let string_parts = node.0.iter().map(|x| (*x).to_owned()).collect();
    let mut exprs = Vec::new();
    for x in &node.1 {
        exprs.push(expr(context, x)?);
    }
    Ok(ast::InterpolatedString {
        parsed: Some(node),
        string_parts,
        exprs,
    })
}

#[named]
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
                (es, os) = left_assoc(node, pred, es, os),
            ast::Assoc::Right =>
                (es, os) = right_assoc(node, pred, es, os),
        }
    }
    if es.len() == 1 {
        Ok(es.pop_front().unwrap())
    }
    else {
        Err(vec![
            SemanticError { slice: None, message: format!("Illegal state, in {}", function_name!()) },
        ])
    }
}

fn left_assoc<'parsed, ExprTree, SemanticOp, ParserExpr>(
    node: &'parsed ParserExpr,
    pred: &Box<dyn Fn(&SemanticOp) -> bool>,
    mut exprs: VecDeque<Rc<ExprTree>>,
    ops: VecDeque<SemanticOp>,
) -> (VecDeque<Rc<ExprTree>>, VecDeque<SemanticOp>)
where
    ExprTree: ast::ExprTree<'parsed, SemanticOp, ParserExpr>,
    SemanticOp: Clone,
{
    let expr_0 = exprs.pop_front().unwrap();
    let (mut acc_exprs, acc_ops, expr) = ops.into_iter().zip(exprs.into_iter())
    .fold((VecDeque::new(), VecDeque::new(), expr_0), |mut acc, x| {
        if pred(&x.0) {
            let infix_op = ExprTree::infix_op(node, acc.2, x.0, x.1);
            (acc.0, acc.1, infix_op)
        }
        else {
            acc.0.push_back(acc.2);
            acc.1.push_back(x.0);
            (acc.0, acc.1, x.1)
        }
    });
    acc_exprs.push_back(expr);
    (acc_exprs, acc_ops)
}

fn right_assoc<'parsed, ExprTree, SemanticOp, ParserExpr>(
    node: &'parsed ParserExpr,
    pred: &Box<dyn Fn(&SemanticOp) -> bool>,
    mut exprs: VecDeque<Rc<ExprTree>>,
    ops: VecDeque<SemanticOp>,
) -> (VecDeque<Rc<ExprTree>>, VecDeque<SemanticOp>)
where
    ExprTree: ast::ExprTree<'parsed, SemanticOp, ParserExpr>,
    SemanticOp: Clone,
{
    let expr_0 = exprs.pop_back().unwrap();
    let (mut acc_exprs, acc_ops, expr) = ops.into_iter().rev().zip(exprs.into_iter().rev())
    .fold((VecDeque::new(), VecDeque::new(), expr_0), |mut acc, x| {
        if pred(&x.0) {
            let infix_op = ExprTree::infix_op(node, x.1, x.0, acc.2);
            (acc.0, acc.1, infix_op)
        }
        else {
            acc.0.push_front(acc.2);
            acc.1.push_front(x.0);
            (acc.0, acc.1, x.1)
        }
    });
    acc_exprs.push_front(expr);
    (acc_exprs, acc_ops)
}
