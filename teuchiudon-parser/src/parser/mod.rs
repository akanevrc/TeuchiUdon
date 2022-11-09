pub mod ast;

use std::rc::Rc;
use function_name::named;
use nom::{
    Parser,
    branch::alt,
    combinator::{
        consumed,
        map,
        opt,
        value,
    },
    multi::{
        many0,
        many1,
        separated_list1,
    },
    sequence::{
        delimited,
        preceded,
        separated_pair,
        terminated,
        tuple,
    }
};
use nom_supreme::ParserExt;

use super::ParsedResult;
use crate::{context::Context, parser::ast::TyTermKind};
use crate::lexer::{
    self,
    lex,
};

#[named]
pub fn target<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Target> {
    |input: &'input str| alt((
        value(ast::Target { slice: input, body: None }, tuple((opt(lex(lexer::byte_order_mark)), lex(lexer::eof)))),
        map(
            consumed(
                delimited(opt(lex(lexer::byte_order_mark)), body(context), lex(lexer::eof)),
            ),
            |x| ast::Target { slice: x.0, body: Some(x.1) },
        ),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn body<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Body> {
    |input: &'input str| map(
        consumed(
            many1(top_stat(context)),
        ),
        |x| ast::Body { slice: x.0, top_stats: x.1 },
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn top_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TopStat> {
    |input: &'input str| alt((
        var_bind_top_stat(context),
        fn_bind_top_stat(context),
        stat_top_stat(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn var_bind_top_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TopStat> {
    |input: &'input str| map(
        consumed(
            tuple((
                consumed(
                    opt(lex(lexer::keyword(context, "pub"))),
                ),
                consumed(
                    opt(alt((
                        lex(lexer::keyword(context, "sync")),
                        lex(lexer::keyword(context, "linear")),
                        lex(lexer::keyword(context, "smooth")),
                    ))),
                ),
                var_bind(context),
                lex(lexer::op_code(context, ";")),
            )),
        ),
        |x| ast::TopStat {
            slice: x.0,
            kind: ast::TopStatKind::VarBind {
                access_attr: x.1.0.1.map(|y| ast::AccessAttr { slice: x.1.0.0, attr: y }),
                sync_attr: x.1.1.1.map(|y| ast::SyncAttr { slice: x.1.1.0, attr: y }),
                var_bind: x.1.2,
            },
        },
    )(input)
}

fn fn_bind_top_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TopStat> {
    |input: &'input str| map(
        consumed(
            tuple((
                consumed(
                    opt(lex(lexer::keyword(context, "pub"))),
                ),
                fn_bind(context),
                lex(lexer::op_code(context, ";")),
            )),
        ),
        |x| ast::TopStat {
            slice: x.0,
            kind: ast::TopStatKind::FnBind {
                access_attr: x.1.0.1.map(|y| ast::AccessAttr { slice: x.1.0.0, attr: y }),
                fn_bind: x.1.1,
            },
        },
    )(input)
}

fn stat_top_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TopStat> {
    |input: &'input str| map(
        consumed(
            stat(context),
        ),
        |x| ast::TopStat {
            slice: x.0,
            kind: ast::TopStatKind::Stat { stat: x.1 },
        },
    )(input)
}

#[named]
pub fn var_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarBind> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "let")),
                var_decl(context),
                lex(lexer::op_code(context, "=")),
                expr(context),
            )),
        ),
        |x| ast::VarBind {
            slice: x.0,
            let_keyword: x.1.0,
            var_decl: x.1.1,
            expr: x.1.3,
        },
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn var_decl<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarDecl> {
    |input: &'input str| alt((
        single_var_decl(context),
        tuple_var_decl(context, "(", ")", "()"),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn single_var_decl<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarDecl> {
    |input: &'input str| map(
        consumed(
            tuple((
                consumed(
                    opt(lex(lexer::keyword(context, "mut"))),
                ),
                lex(lexer::ident(context)),
                opt(
                    preceded(
                        lex(lexer::op_code(context, ":")),
                        ty_expr(context),
                    ),
                ),
            )),
        ),
        |x| ast::VarDecl {
            slice: x.0,
            kind: ast::VarDeclKind::SingleDecl {
                mut_attr: x.1.0.1.map(|y| ast::MutAttr { slice: x.1.0.0, attr: y }),
                ident: x.1.1,
                ty_expr: x.1.2,
            },
        }
    )(input)
}

fn tuple_var_decl<'context: 'input, 'input>(
    context: &'context Context,
    open: &'static str,
    close: &'static str,
    both: &'static str,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarDecl> {
    move |input: &'input str| map(
        consumed(
            alt((
                delimited(
                    lex(lexer::op_code(context, open)),
                    opt(
                        terminated(
                            separated_list1(lex(lexer::op_code(context, ",")), var_decl(context)),
                            opt(lex(lexer::op_code(context, ","))),
                        ),
                    ),
                    lex(lexer::op_code(context, close)),
                ),
                value(None, lex(lexer::op_code(context, both))),
            )),
        ),
        |x| ast::VarDecl {
            slice: x.0,
            kind: ast::VarDeclKind::TupleDecl {
                var_decls: x.1.unwrap_or(Vec::new()),
            },
        }
    )(input)
}

#[named]
pub fn fn_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::FnBind> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "fn")),
                fn_decl(context),
                stats_block(context),
            )),
        ),
        |x| ast::FnBind {
            slice: x.0,
            fn_keyword: x.1.0,
            fn_decl: x.1.1,
            stats_block: x.1.2,
        },
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn fn_decl<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::FnDecl> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::ident(context)),
                tuple_var_decl(context, "(", ")", "()"),
                opt(preceded(
                    lex(lexer::op_code(context, "->")),
                    ty_expr(context),
                )),
            )),
        ),
        |x| ast::FnDecl {
            slice: x.0,
            ident: x.1.0,
            var_decl: x.1.1,
            ty_expr: x.1.2,
        },
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn ty_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TyExpr>> {
    |input: &'input str| map(
        consumed(
            tuple((ty_term(context), many0(ty_op(context)))),
        ),
        |x| Rc::new(ast::TyExpr {
            slice: x.0,
            ty_term: x.1.0,
            ty_ops: x.1.1
        })
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn ty_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TyOp> {
    |input: &'input str| alt((
        access_ty_op(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn access_ty_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TyOp> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::op_code(context, "::")),
                ty_term(context),
            )),
        ),
        |x| ast::TyOp {
            slice: x.0,
            kind: ast::TyOpKind::Access {
                op_code: x.1.0,
                ty_term: x.1.1,
            },
        },
    )(input)
}

#[named]
pub fn ty_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TyTerm>> {
    |input: &'input str| map(
        alt((
            eval_ty_ty_term(context),
        )),
        |x| Rc::new(x),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

fn eval_ty_ty_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TyTerm> {
    |input: &'input str| map(
        consumed(
            lex(lexer::ident(context)),
        ),
        |x| ast::TyTerm {
            slice: x.0,
            kind: TyTermKind::EvalTy { ident: x.1 },
        },
    )(input)
}

#[named]
pub fn stats_block<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::StatsBlock> {
    |input: &'input str| map(
        consumed(
            delimited(
                lex(lexer::op_code(context, "{")),
                tuple((
                    many0(stat(context)),
                    opt(expr(context)),
                )),
                lex(lexer::op_code(context, "}")),
            ),
        ),
        |x| ast::StatsBlock {
            slice: x.0,
            stats: x.1.0,
            ret: x.1.1,
        },
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Stat> {
    |input: &'input str| terminated(
        alt((
            return_stat(context),
            continue_stat(context),
            break_stat(context),
            map(
                consumed(var_bind(context)),
                |x| ast::Stat {
                    slice: x.0,
                    kind: ast::StatKind::VarBind { var_bind: x.1 },
                },
            ),
            map(
                consumed(fn_bind(context)),
                |x| ast::Stat {
                    slice: x.0,
                    kind: ast::StatKind::FnBind { fn_bind: x.1 },
                },
            ),
            map(
                consumed(expr(context)),
                |x| ast::Stat {
                    slice: x.0,
                    kind: ast::StatKind::Expr { expr: x.1 },
                },
            ),
        )),
        lex(lexer::op_code(context, ";")),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

fn return_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Stat> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "return")),
                opt(expr(context)),
            )),
        ),
        |x| ast::Stat {
            slice: x.0,
            kind: ast::StatKind::Return { return_keyword: x.1.0, expr: x.1.1 },
        },
    )(input)
}

fn continue_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Stat> {
    |input: &'input str| map(
        consumed(
            lex(lexer::keyword(context, "continue")),
        ),
        |x| ast::Stat {
            slice: x.0,
            kind: ast::StatKind::Continue { continue_keyword: x.1 },
        },
    )(input)
}

fn break_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Stat> {
    |input: &'input str| map(
        consumed(
            lex(lexer::keyword(context, "break")),
        ),
        |x| ast::Stat {
            slice: x.0,
            kind: ast::StatKind::Break { break_keyword: x.1 },
        }
    )(input)
}

#[named]
pub fn expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Expr>> {
    |input: &'input str| map(
        consumed(
            tuple((term(context), many0(op(context)))),
        ),
        |x| Rc::new(ast::Expr {
            slice: x.0,
            term: x.1.0,
            ops: x.1.1,
        }),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| alt((
        ty_access_op(context),
        access_op(context),
        eval_fn_op(context),
        eval_spread_fn_op(context),
        eval_key_op(context),
        cast_op(context),
        infix_op(context),
        assign_op(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn ty_access_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::op_code(context, "::")),
                term(context),
            )),
        ),
        |x| ast::Op {
            slice: x.0,
            kind: ast::OpKind::TyAccess { op_code: x.1.0, term: x.1.1 },
        },
    )(input)
}

fn access_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        consumed(
            tuple((
                alt((lex(lexer::op_code(context, ".")), lex(lexer::op_code(context, "?.")))),
                term(context),
            )),
        ),
        |x| ast::Op {
            slice: x.0,
            kind: ast::OpKind::Access { op_code: x.1.0, term: x.1.1 },
        },
    )(input)
}

fn eval_fn_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        consumed(
            delimited(
                lex(lexer::op_code(context, "(")),
                opt(
                    terminated(
                        separated_list1(lex(lexer::op_code(context, ",")), arg_expr(context)),
                        opt(lex(lexer::op_code(context, ","))),
                    ),
                ),
                lex(lexer::op_code(context, ")")),
            ),
        ),
        |x| ast::Op {
            slice: x.0,
            kind: ast::OpKind::EvalFn { arg_exprs: x.1.unwrap_or(Vec::new()) },
        },
    )(input)
}

fn eval_spread_fn_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::op_code(context, "(")),
                lex(lexer::op_code(context, "...")),
                expr(context),
                lex(lexer::op_code(context, ")")),
            )),
        ),
        |x| ast::Op {
            slice: x.0,
            kind: ast::OpKind::EvalSpreadFn { expr: x.1.2 },
        },
    )(input)
}

fn eval_key_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        consumed(
            delimited(
                lex(lexer::op_code(context, "[")),
                expr(context),
                lex(lexer::op_code(context, "]")),
            ),
        ),
        |x| ast::Op {
            slice: x.0,
            kind: ast::OpKind::EvalKey { expr: x.1 },
        }
    )(input)
}

fn cast_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "as")),
                ty_expr(context),
            )),
        ),
        |x| ast::Op {
            slice: x.0,
            kind: ast::OpKind::CastOp { as_keyword: x.1.0, ty_expr: x.1.1 },
        },
    )(input)
}

fn infix_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        consumed(
            tuple((
                alt((
                    alt((
                        lex(lexer::op_code(context, "*")),
                        lex(lexer::op_code(context, "/")),
                        lex(lexer::op_code(context, "%")),
                        lex(lexer::op_code(context, "+")),
                        lex(lexer::op_code(context, "-")),
                        lex(lexer::op_code(context, "<<")),
                        lex(lexer::op_code(context, ">>")),
                    )),
                    alt((
                        lex(lexer::op_code(context, "<")),
                        lex(lexer::op_code(context, ">")),
                        lex(lexer::op_code(context, "<=")),
                        lex(lexer::op_code(context, ">=")),
                        lex(lexer::op_code(context, "==")),
                        lex(lexer::op_code(context, "!=")),
                        lex(lexer::op_code(context, "&")),
                        lex(lexer::op_code(context, "^")),
                        lex(lexer::op_code(context, "|")),
                        lex(lexer::op_code(context, "&&")),
                        lex(lexer::op_code(context, "||")),
                        lex(lexer::op_code(context, "??")),
                        lex(lexer::op_code(context, "|>")),
                        lex(lexer::op_code(context, "<|")),
                    )),
                )),
                term(context),
            )),
        ),
        |x| ast::Op {
            slice: x.0,
            kind: ast::OpKind::InfixOp { op_code: x.1.0, term: x.1.1 },
        },
    )(input)
}

fn assign_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::op_code(context, "=")),
                term(context),
            )),
        ),
        |x| ast::Op {
            slice: x.0,
            kind: ast::OpKind::Assign { term: x.1.1 },
        }
    )(input)
}

#[named]
pub fn term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Term>> {
    |input: &'input str| map(
        alt((
            prefix_op_term(context),
            block_term(context),
            paren_term(context),
            tuple_term(context),
            array_ctor_term(context),
            literal_term(context),
            this_literal_term(context),
            interpolated_string_term(context),
            eval_var_term(context),
            let_in_bind_term(context),
            if_term(context),
            while_term(context),
            loop_term(context),
            for_term(context),
            closure_term(context),
        )),
        |x| Rc::new(x),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

fn prefix_op_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            tuple((
                alt((
                    lex(lexer::op_code(context, "+")),
                    lex(lexer::op_code(context, "-")),
                    lex(lexer::op_code(context, "!")),
                    lex(lexer::op_code(context, "~")),
                )),
                term(context),
            )),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::PrefixOp { op_code: x.1.0, term: x.1.1 },
        }
    )(input)
}

fn block_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            stats_block(context),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::Block { stats: x.1 },
        },
    )(input)
}

fn paren_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            delimited(
                lex(lexer::op_code(context, "(")),
                expr(context),
                lex(lexer::op_code(context, ")")),
            ),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::Paren { expr: x.1 },
        },
    )(input)
}

fn tuple_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
        tuple((
                lex(lexer::op_code(context, "(")),
                expr(context),
                lex(lexer::op_code(context, ",")),
                opt(
                    tuple((
                        expr(context),
                        many0(
                            preceded(
                                lex(lexer::op_code(context, ",")),
                                expr(context),
                            ),
                        ),
                        opt(lex(lexer::op_code(context, ","))),
                    )),
                ),
                lex(lexer::op_code(context, ")")),
            )),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::Tuple {
                exprs: x.1.3.map_or(
                    vec![x.1.1.clone()],
                    |y| [x.1.1].into_iter().chain([y.0].into_iter()).chain(y.1.into_iter()).collect(),
                ),
            },
        },
    )(input)
}

fn array_ctor_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            delimited(
                lex(lexer::op_code(context, "[")),
                opt(iter_expr(context)),
                lex(lexer::op_code(context, "]")),
            ),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::ArrayCtor { iter_expr: x.1 },
        },
    )(input)
}

fn literal_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            alt((
                lex(lexer::unit_literal(context)),
                lex(lexer::null_literal(context)),
                lex(lexer::bool_literal(context)),
                lex(lexer::real_number_literal),
                lex(lexer::hex_integer_literal),
                lex(lexer::bin_integer_literal),
                lex(lexer::integer_literal),
                lex(lexer::character_literal),
                lex(lexer::regular_string_literal),
                lex(lexer::verbatium_string_literal),
            )),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::Literal { literal: x.1 },
        },
    )(input)
}

fn this_literal_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            lex(lexer::this_literal(context)),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::ThisLiteral { literal: x.1 },
        }
    )(input)
}

fn interpolated_string_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            lex(lexer::interpolated_string(context)),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::InterpolatedString { interpolated_string: x.1 },
        },
    )(input)
}

fn eval_var_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            lex(lexer::ident(context)),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::EvalVar { ident: x.1 },
        },
    )(input)
}

fn let_in_bind_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            tuple((
                var_bind(context),
                lex(lexer::keyword(context, "in")),
                expr(context),
            )),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::LetInBind { var_bind: x.1.0, in_keyword: x.1.1, expr: x.1.2 },
        },
    )(input)
}

fn if_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "if")),
                expr(context),
                stats_block(context),
                opt(
                    tuple((
                        lex(lexer::keyword(context, "else")),
                        alt((
                            map(
                                consumed(
                                    if_term(context),
                                ),
                                |x| ast::StatsBlock {
                                    slice: x.0,
                                    stats: Vec::new(),
                                    ret: Some(Rc::new(ast::Expr {
                                        slice: x.0,
                                        term: Rc::new(x.1),
                                        ops: Vec::new(),
                                    })),
                                },
                            ),
                            stats_block(context),
                        )),
                    )),
                ),
            )),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::If { if_keyword: x.1.0, condition: x.1.1, if_part: x.1.2, else_part: x.1.3 },
        },
    )(input)
}

fn while_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "while")),
                expr(context),
                stats_block(context),
            )),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::While { while_keyword: x.1.0, condition: x.1.1, stats: x.1.2 },
        },
    )(input)
}

fn loop_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "loop")),
                stats_block(context),
            )),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::Loop { loop_keyword: x.1.0, stats: x.1.1 },
        },
    )(input)
}

fn for_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            tuple((
                many1(
                    tuple((
                        lex(lexer::keyword(context, "for")),
                        for_bind(context),
                    )),
                ),
                stats_block(context),
            )),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::For { for_binds: x.1.0, stats: x.1.1 },
        },
    )(input)
}

fn closure_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        consumed(
            tuple((
                tuple_var_decl(context, "|", "|", "||"),
                expr(context),
            )),
        ),
        |x| ast::Term {
            slice: x.0,
            kind: ast::TermKind::Closure { var_decl: x.1.0, expr: x.1.1 },
        },
    )(input)
}

#[named]
pub fn iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::IterExpr> {
    |input: &'input str| alt((
        range_iter_expr(context),
        spread_iter_expr(context),
        elements_iter_expr(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn range_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::IterExpr> {
    |input: &'input str| map(
        consumed(
            tuple((
                expr(context),
                lex(lexer::op_code(context, "..")),
                expr(context),
                opt(
                    preceded(
                        lex(lexer::op_code(context, "..")),
                        expr(context),
                    ),
                ),
            )),
        ),
        |x|
            x.1.3.map_or(
                ast::IterExpr {
                    slice: x.0,
                    kind: ast::IterExprKind::Range { left: x.1.0.clone(), right: x.1.2.clone() },
                },
                |y| ast::IterExpr {
                    slice: x.0,
                    kind: ast::IterExprKind::SteppedRange { left: x.1.0, right: x.1.2, step: y },
                },
            ),
    )(input)
}

fn spread_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::IterExpr> {
    |input: &'input str| map(
        consumed(
            preceded(
                lex(lexer::op_code(context, "...")),
                expr(context),
            ),
        ),
        |x| ast::IterExpr {
            slice: x.0,
            kind: ast::IterExprKind::Spread { expr: x.1 },
        },
    )(input)
}

fn elements_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::IterExpr> {
    |input: &'input str| map(
        consumed(
            terminated(
                separated_list1(lex(lexer::op_code(context, ",")), expr(context)),
                opt(lex(lexer::op_code(context, ","))),
            ),
        ),
        |x| ast::IterExpr {
            slice: x.0,
            kind: ast::IterExprKind::Elements { exprs: x.1 },
        },
    )(input)
}

#[named]
pub fn arg_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ArgExpr> {
    |input: &'input str| map(
        consumed(
            tuple((
                consumed(
                    opt(lex(lexer::keyword(context, "mut"))),
                ),
                expr(context),
            )),
        ),
        |x| ast::ArgExpr {
            slice: x.0,
            mut_attr: x.1.0.1.map(|y| ast::MutAttr { slice: x.1.0.0, attr: y }),
            expr: x.1.1,
        },
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn for_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForBind> {
    |input: &'input str| alt((
        let_for_bind(context),
        assign_for_bind(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn let_for_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForBind> {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "let")),
                var_decl(context),
                lex(lexer::op_code(context, "<-")),
                for_iter_expr(context),
            )),
        ),
        |x| ast::ForBind {
            slice: x.0,
            kind: ast::ForBindKind::Let { let_keyword: x.1.0, var_decl: x.1.1, for_iter_expr: x.1.3 },
        },
    )(input)
}

fn assign_for_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForBind> {
    |input: &'input str| map(
        consumed(
            separated_pair(
                expr(context),
                lex(lexer::op_code(context, "<-")),
                for_iter_expr(context),
            ),
        ),
        |x| ast::ForBind {
            slice: x.0,
            kind: ast::ForBindKind::Assign { left: x.1.0, for_iter_expr: x.1.1 },
        },
    )(input)
}

#[named]
pub fn for_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForIterExpr> {
    |input: &'input str| alt((
        range_for_iter_expr(context),
        spread_for_iter_expr(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn range_for_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForIterExpr> {
    |input: &'input str| map(
        consumed(
            tuple((
                expr(context),
                lex(lexer::op_code(context, "..")),
                expr(context),
                opt(
                    preceded(
                        lex(lexer::op_code(context, "..")),
                        expr(context),
                    ),
                ),
            )),
        ),
        |x|
            x.1.3.map_or(
                ast::ForIterExpr {
                    slice: x.0,
                    kind: ast::ForIterExprKind::Range { left: x.1.0.clone(), right: x.1.2.clone() },
                },
                |y| ast::ForIterExpr {
                    slice: x.0,
                    kind: ast::ForIterExprKind::SteppedRange { left: x.1.0, right: x.1.2, step: y },
                },
            ),
    )(input)
}

fn spread_for_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForIterExpr> {
    |input: &'input str| map(
        consumed(
            expr(context),
        ),
        |x| ast::ForIterExpr {
            slice: x.0,
            kind: ast::ForIterExprKind::Spread { expr: x.1 },
        },
    )(input)
}
