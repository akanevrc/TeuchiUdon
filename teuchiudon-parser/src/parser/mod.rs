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
use crate::context::Context;
use crate::lexer::{
    self,
    lex,
};

#[named]
pub fn target<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Target<'input>>> + 'context {
    |input: &'input str| alt((
        value(Rc::new(ast::Target { slice: input, body: None }), tuple((opt(lex(lexer::byte_order_mark)), lex(lexer::eof)))),
        map(
            consumed(
                delimited(opt(lex(lexer::byte_order_mark)), body(context), lex(lexer::eof)),
            ),
            |x| Rc::new(ast::Target { slice: x.0, body: Some(x.1) }),
        ),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn body<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Body<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            many1(top_stat(context)),
        ),
        |x| Rc::new(ast::Body { slice: x.0, top_stats: x.1 }),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TopStat<'input>>> + 'context {
    |input: &'input str| alt((
        var_bind_top_stat(context),
        fn_bind_top_stat(context),
        stat_top_stat(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn var_bind_top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TopStat<'input>>> + 'context {
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
        |x| Rc::new(ast::TopStat {
            slice: x.0,
            kind: Rc::new(ast::TopStatKind::VarBind {
                access_attr: x.1.0.1.map(|y| Rc::new(ast::AccessAttr { slice: x.1.0.0, attr: y })),
                sync_attr: x.1.1.1.map(|y| Rc::new(ast::SyncAttr { slice: x.1.1.0, attr: y })),
                var_bind: x.1.2,
            }),
        }),
    )(input)
}

fn fn_bind_top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TopStat<'input>>> + 'context {
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
        |x| Rc::new(ast::TopStat {
            slice: x.0,
            kind: Rc::new(ast::TopStatKind::FnBind {
                access_attr: x.1.0.1.map(|y| Rc::new(ast::AccessAttr { slice: x.1.0.0, attr: y })),
                fn_bind: x.1.1,
            }),
        }),
    )(input)
}

fn stat_top_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TopStat<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            stat(context),
        ),
        |x| Rc::new(ast::TopStat {
            slice: x.0,
            kind: Rc::new(ast::TopStatKind::Stat { stat: x.1 }),
        }),
    )(input)
}

#[named]
pub fn var_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::VarBind<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "let")),
                var_decl(context),
                lex(lexer::op_code(context, "=")),
                expr(context),
            )),
        ),
        |x| Rc::new(ast::VarBind {
            slice: x.0,
            let_keyword: x.1.0,
            var_decl: x.1.1,
            expr: x.1.3,
        }),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn var_decl<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::VarDecl<'input>>> + 'context {
    |input: &'input str| alt((
        single_var_decl(context),
        tuple_var_decl(context, "(", ")", "()"),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn single_var_decl<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::VarDecl<'input>>> + 'context {
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
        |x| Rc::new(ast::VarDecl {
            slice: x.0,
            kind: Rc::new(ast::VarDeclKind::SingleDecl {
                mut_attr: x.1.0.1.map(|y| Rc::new(ast::MutAttr { slice: x.1.0.0, attr: y })),
                ident: x.1.1,
                ty_expr: x.1.2,
            }),
        })
    )(input)
}

fn tuple_var_decl<'input: 'context, 'context>(
    context: &'context Context<'input>,
    open: &'static str,
    close: &'static str,
    both: &'static str,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::VarDecl<'input>>> + 'context {
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
        |x| Rc::new(ast::VarDecl {
            slice: x.0,
            kind: Rc::new(ast::VarDeclKind::TupleDecl {
                var_decls: x.1.unwrap_or(Vec::new()),
            }),
        })
    )(input)
}

#[named]
pub fn fn_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::FnBind<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "fn")),
                fn_decl(context),
                stats_block(context),
            )),
        ),
        |x| Rc::new(ast::FnBind {
            slice: x.0,
            fn_keyword: x.1.0,
            fn_decl: x.1.1,
            stats_block: x.1.2,
        }),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn fn_decl<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::FnDecl<'input>>> + 'context {
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
        |x| Rc::new(ast::FnDecl {
            slice: x.0,
            ident: x.1.0,
            var_decl: x.1.1,
            ty_expr: x.1.2,
        }),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn ty_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TyExpr<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((ty_factor(context), many0(ty_op(context)))),
        ),
        |x| Rc::new(ast::TyExpr {
            slice: x.0,
            ty_factor: x.1.0,
            ty_ops: x.1.1
        })
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn ty_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TyOp<'input>>> + 'context {
    |input: &'input str| alt((
        access_ty_op(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn access_ty_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TyOp<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::op_code(context, "::")),
                ty_factor(context),
            )),
        ),
        |x| Rc::new(ast::TyOp {
            slice: x.0,
            kind: Rc::new(ast::TyOpKind::Access {
                op_code: x.1.0,
                ty_factor: x.1.1,
            }),
        }),
    )(input)
}

#[named]
pub fn ty_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TyFactor<'input>>> + 'context {
    |input: &'input str| alt((
        eval_ty_ty_factor(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn eval_ty_ty_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TyFactor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            lex(lexer::ident(context)),
        ),
        |x| Rc::new(ast::TyFactor {
            slice: x.0,
            kind: Rc::new(ast::TyFactorKind::EvalTy { ident: x.1 }),
        }),
    )(input)
}

#[named]
pub fn stats_block<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::StatsBlock<'input>>> + 'context {
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
        |x| Rc::new(ast::StatsBlock {
            slice: x.0,
            stats: x.1.0,
            ret: x.1.1,
        }),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Stat<'input>>> + 'context {
    |input: &'input str| terminated(
        alt((
            return_stat(context),
            continue_stat(context),
            break_stat(context),
            map(
                consumed(var_bind(context)),
                |x| Rc::new(ast::Stat {
                    slice: x.0,
                    kind: Rc::new(ast::StatKind::VarBind { var_bind: x.1 }),
                }),
            ),
            map(
                consumed(fn_bind(context)),
                |x| Rc::new(ast::Stat {
                    slice: x.0,
                    kind: Rc::new(ast::StatKind::FnBind { fn_bind: x.1 }),
                }),
            ),
            map(
                consumed(expr(context)),
                |x| Rc::new(ast::Stat {
                    slice: x.0,
                    kind: Rc::new(ast::StatKind::Expr { expr: x.1 }),
                }),
            ),
        )),
        lex(lexer::op_code(context, ";")),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

fn return_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Stat<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "return")),
                opt(expr(context)),
            )),
        ),
        |x| Rc::new(ast::Stat {
            slice: x.0,
            kind: Rc::new(ast::StatKind::Return { return_keyword: x.1.0, expr: x.1.1 }),
        }),
    )(input)
}

fn continue_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Stat<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            lex(lexer::keyword(context, "continue")),
        ),
        |x| Rc::new(ast::Stat {
            slice: x.0,
            kind: Rc::new(ast::StatKind::Continue { continue_keyword: x.1 }),
        }),
    )(input)
}

fn break_stat<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Stat<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            lex(lexer::keyword(context, "break")),
        ),
        |x| Rc::new(ast::Stat {
            slice: x.0,
            kind: Rc::new(ast::StatKind::Break { break_keyword: x.1 }),
        })
    )(input)
}

#[named]
pub fn expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Expr<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                term(context),
                many0(term_op(context)),
            )),
        ),
        |x| Rc::new(ast::Expr {
            slice: x.0,
            term: x.1.0,
            term_ops: x.1.1,
        }),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn term_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TermOp<'input>>> + 'context {
    |input: &'input str| alt((
        cast_op(context),
        infix_op(context),
        assign_op(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn cast_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TermOp<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "as")),
                ty_expr(context),
            )),
        ),
        |x| Rc::new(ast::TermOp {
            slice: x.0,
            kind: Rc::new(ast::TermOpKind::CastOp { as_keyword: x.1.0, ty_expr: x.1.1 }),
        }),
    )(input)
}

fn infix_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TermOp<'input>>> + 'context {
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
        |x| Rc::new(ast::TermOp {
            slice: x.0,
            kind: Rc::new(ast::TermOpKind::InfixOp { op_code: x.1.0, term: x.1.1 }),
        }),
    )(input)
}

fn assign_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TermOp<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::op_code(context, "=")),
                term(context),
            )),
        ),
        |x| Rc::new(ast::TermOp {
            slice: x.0,
            kind: Rc::new(ast::TermOpKind::Assign { term: x.1.1 }),
        })
    )(input)
}

#[named]
pub fn term<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Term<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                many0(
                    alt((
                        lex(lexer::op_code(context, "+")),
                        lex(lexer::op_code(context, "-")),
                        lex(lexer::op_code(context, "!")),
                        lex(lexer::op_code(context, "~")),
                    )),
                ),
                factor(context),
                many0(factor_op(context)),
            )),
        ),
        |x| Rc::new(ast::Term {
            slice: x.0,
            prefix_ops: x.1.0,
            factor: x.1.1,
            factor_ops: x.1.2,
        }),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn factor_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::FactorOp<'input>>> + 'context {
    |input: &'input str| alt((
        ty_access_op(context),
        access_op(context),
        eval_fn_op(context),
        eval_spread_fn_op(context),
        eval_key_op(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn ty_access_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::FactorOp<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::op_code(context, "::")),
                factor(context),
            )),
        ),
        |x| Rc::new(ast::FactorOp {
            slice: x.0,
            kind: Rc::new(ast::FactorOpKind::TyAccess { op_code: x.1.0, factor: x.1.1 }),
        }),
    )(input)
}

fn access_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::FactorOp<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                alt((lex(lexer::op_code(context, ".")), lex(lexer::op_code(context, "?.")))),
                factor(context),
            )),
        ),
        |x| Rc::new(ast::FactorOp {
            slice: x.0,
            kind: Rc::new(ast::FactorOpKind::Access { op_code: x.1.0, factor: x.1.1 }),
        }),
    )(input)
}

fn eval_fn_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::FactorOp<'input>>> + 'context {
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
        |x| Rc::new(ast::FactorOp {
            slice: x.0,
            kind: Rc::new(ast::FactorOpKind::EvalFn { arg_exprs: x.1.unwrap_or(Vec::new()) }),
        }),
    )(input)
}

fn eval_spread_fn_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::FactorOp<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::op_code(context, "(")),
                lex(lexer::op_code(context, "...")),
                expr(context),
                lex(lexer::op_code(context, ")")),
            )),
        ),
        |x| Rc::new(ast::FactorOp {
            slice: x.0,
            kind: Rc::new(ast::FactorOpKind::EvalSpreadFn { expr: x.1.2 }),
        }),
    )(input)
}

fn eval_key_op<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::FactorOp<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            delimited(
                lex(lexer::op_code(context, "[")),
                expr(context),
                lex(lexer::op_code(context, "]")),
            ),
        ),
        |x| Rc::new(ast::FactorOp {
            slice: x.0,
            kind: Rc::new(ast::FactorOpKind::EvalKey { expr: x.1 }),
        })
    )(input)
}

#[named]
pub fn factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| alt((
        block_factor(context),
        paren_factor(context),
        tuple_factor(context),
        array_ctor_factor(context),
        literal_factor(context),
        this_literal_factor(context),
        interpolated_string_factor(context),
        eval_var_factor(context),
        let_in_bind_factor(context),
        if_factor(context),
        while_factor(context),
        loop_factor(context),
        for_factor(context),
        closure_factor(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn block_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            stats_block(context),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::Block { stats: x.1 }),
        }),
    )(input)
}

fn paren_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            delimited(
                lex(lexer::op_code(context, "(")),
                expr(context),
                lex(lexer::op_code(context, ")")),
            ),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::Paren { expr: x.1 }),
        }),
    )(input)
}

fn tuple_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
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
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::Tuple {
                exprs: x.1.3.map_or(
                    vec![x.1.1.clone()],
                    |y| [x.1.1].into_iter().chain([y.0].into_iter()).chain(y.1.into_iter()).collect(),
                ),
            }),
        }),
    )(input)
}

fn array_ctor_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            delimited(
                lex(lexer::op_code(context, "[")),
                opt(iter_expr(context)),
                lex(lexer::op_code(context, "]")),
            ),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::ArrayCtor { iter_expr: x.1 }),
        }),
    )(input)
}

fn literal_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
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
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::Literal { literal: x.1 }),
        }),
    )(input)
}

fn this_literal_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            lex(lexer::this_literal(context)),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::ThisLiteral { literal: x.1 }),
        })
    )(input)
}

fn interpolated_string_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            lex(lexer::interpolated_string(context)),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::InterpolatedString { interpolated_string: x.1 }),
        }),
    )(input)
}

fn eval_var_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            lex(lexer::ident(context)),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::EvalVar { ident: x.1 }),
        }),
    )(input)
}

fn let_in_bind_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                var_bind(context),
                lex(lexer::keyword(context, "in")),
                expr(context),
            )),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::LetInBind { var_bind: x.1.0, in_keyword: x.1.1, expr: x.1.2 }),
        }),
    )(input)
}

fn if_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
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
                                    if_factor(context),
                                ),
                                |x| Rc::new(ast::StatsBlock {
                                    slice: x.0,
                                    stats: Vec::new(),
                                    ret: Some(Rc::new(ast::Expr {
                                        slice: x.0,
                                        term: Rc::new(ast::Term {
                                            slice: x.1.slice,
                                            prefix_ops: Vec::new(),
                                            factor: x.1,
                                            factor_ops: Vec::new(),
                                        }),
                                        term_ops: Vec::new(),
                                    })),
                                }),
                            ),
                            stats_block(context),
                        )),
                    )),
                ),
            )),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::If { if_keyword: x.1.0, condition: x.1.1, if_part: x.1.2, else_part: x.1.3 }),
        }),
    )(input)
}

fn while_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "while")),
                expr(context),
                stats_block(context),
            )),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::While { while_keyword: x.1.0, condition: x.1.1, stats: x.1.2 }),
        }),
    )(input)
}

fn loop_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "loop")),
                stats_block(context),
            )),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::Loop { loop_keyword: x.1.0, stats: x.1.1 }),
        }),
    )(input)
}

fn for_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
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
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::For { for_binds: x.1.0, stats: x.1.1 }),
        }),
    )(input)
}

fn closure_factor<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Factor<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                tuple_var_decl(context, "|", "|", "||"),
                expr(context),
            )),
        ),
        |x| Rc::new(ast::Factor {
            slice: x.0,
            kind: Rc::new(ast::FactorKind::Closure { var_decl: x.1.0, expr: x.1.1 }),
        }),
    )(input)
}

#[named]
pub fn iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::IterExpr<'input>>> + 'context {
    |input: &'input str| alt((
        range_iter_expr(context),
        spread_iter_expr(context),
        elements_iter_expr(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn range_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::IterExpr<'input>>> + 'context {
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
                Rc::new(ast::IterExpr {
                    slice: x.0,
                    kind: Rc::new(ast::IterExprKind::Range { left: x.1.0.clone(), right: x.1.2.clone() }),
                }),
                |y| Rc::new(ast::IterExpr {
                    slice: x.0,
                    kind: Rc::new(ast::IterExprKind::SteppedRange { left: x.1.0, right: x.1.2, step: y }),
                }),
            ),
    )(input)
}

fn spread_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::IterExpr<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            preceded(
                lex(lexer::op_code(context, "...")),
                expr(context),
            ),
        ),
        |x| Rc::new(ast::IterExpr {
            slice: x.0,
            kind: Rc::new(ast::IterExprKind::Spread { expr: x.1 }),
        }),
    )(input)
}

fn elements_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::IterExpr<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            terminated(
                separated_list1(lex(lexer::op_code(context, ",")), expr(context)),
                opt(lex(lexer::op_code(context, ","))),
            ),
        ),
        |x| Rc::new(ast::IterExpr {
            slice: x.0,
            kind: Rc::new(ast::IterExprKind::Elements { exprs: x.1 }),
        }),
    )(input)
}

#[named]
pub fn arg_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::ArgExpr<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                consumed(
                    opt(lex(lexer::keyword(context, "mut"))),
                ),
                expr(context),
            )),
        ),
        |x| Rc::new(ast::ArgExpr {
            slice: x.0,
            mut_attr: x.1.0.1.map(|y| Rc::new(ast::MutAttr { slice: x.1.0.0, attr: y })),
            expr: x.1.1,
        }),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn for_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::ForBind<'input>>> + 'context {
    |input: &'input str| alt((
        let_for_bind(context),
        assign_for_bind(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn let_for_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::ForBind<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            tuple((
                lex(lexer::keyword(context, "let")),
                var_decl(context),
                lex(lexer::op_code(context, "<-")),
                for_iter_expr(context),
            )),
        ),
        |x| Rc::new(ast::ForBind {
            slice: x.0,
            kind: Rc::new(ast::ForBindKind::Let { let_keyword: x.1.0, var_decl: x.1.1, for_iter_expr: x.1.3 }),
        }),
    )(input)
}

fn assign_for_bind<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::ForBind<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            separated_pair(
                expr(context),
                lex(lexer::op_code(context, "<-")),
                for_iter_expr(context),
            ),
        ),
        |x| Rc::new(ast::ForBind {
            slice: x.0,
            kind: Rc::new(ast::ForBindKind::Assign { left: x.1.0, for_iter_expr: x.1.1 }),
        }),
    )(input)
}

#[named]
pub fn for_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::ForIterExpr<'input>>> + 'context {
    |input: &'input str| alt((
        range_for_iter_expr(context),
        spread_for_iter_expr(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn range_for_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::ForIterExpr<'input>>> + 'context {
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
                Rc::new(ast::ForIterExpr {
                    slice: x.0,
                    kind: Rc::new(ast::ForIterExprKind::Range { left: x.1.0.clone(), right: x.1.2.clone() }),
                }),
                |y| Rc::new(ast::ForIterExpr {
                    slice: x.0,
                    kind: Rc::new(ast::ForIterExprKind::SteppedRange { left: x.1.0, right: x.1.2, step: y }),
                }),
            ),
    )(input)
}

fn spread_for_iter_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::ForIterExpr<'input>>> + 'context {
    |input: &'input str| map(
        consumed(
            expr(context),
        ),
        |x| Rc::new(ast::ForIterExpr {
            slice: x.0,
            kind: Rc::new(ast::ForIterExprKind::Spread { expr: x.1 }),
        }),
    )(input)
}
