pub mod ast;

use std::rc::Rc;
use function_name::named;
use nom::{
    Parser,
    branch::alt,
    combinator::{
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
pub fn target<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Target> {
    |input: &'input str| alt((
        value(ast::Target(None), tuple((opt(lex(lexer::byte_order_mark)), lex(lexer::eof)))),
        map(
            delimited(opt(lex(lexer::byte_order_mark)), body(context), lex(lexer::eof)),
            |x| ast::Target(Some(x)),
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
        many1(top_stat(context)),
        |x| ast::Body(x)
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
        tuple((
            opt(lex(lexer::keyword(context, "pub"))),
            opt(alt((
                lex(lexer::keyword(context, "sync")),
                lex(lexer::keyword(context, "linear")),
                lex(lexer::keyword(context, "smooth")),
            ))),
            var_bind(context),
            lex(lexer::op_code(context, ";")),
        )),
        |x| ast::TopStat::VarBind(
            x.0.map(|x| ast::AccessAttr(x)),
            x.1.map(|x| ast::SyncAttr(x)),
            x.2,
        ),
    )(input)
}

fn fn_bind_top_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TopStat> {
    |input: &'input str| map(
        tuple((
            opt(lex(lexer::keyword(context, "pub"))),
            fn_bind(context),
            lex(lexer::op_code(context, ";")),
        )),
        |x| ast::TopStat::FnBind(
            x.0.map(|x| ast::AccessAttr(x)),
            x.1,
        ),
    )(input)
}

fn stat_top_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TopStat> {
    |input: &'input str| map(
        stat(context),
        |x| ast::TopStat::Stat(x)
    )(input)
}

#[named]
pub fn var_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarBind> {
    |input: &'input str| map(
        tuple((
            lex(lexer::keyword(context, "let")),
            var_decl(context),
            lex(lexer::op_code(context, "=")),
            expr(context),
        )),
        |x| ast::VarBind(x.0, x.1, x.3),
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
        tuple((
            opt(lex(lexer::keyword(context, "mut"))),
            lex(lexer::ident(context)),
            opt(
                preceded(
                    lex(lexer::op_code(context, ":")),
                    type_expr(context),
                ),
            ),
        )),
        |x| ast::VarDecl::SingleDecl(
            x.0.map(|x| ast::MutAttr(x)),
            x.1,
            x.2,
        ),
    )(input)
}

fn tuple_var_decl<'context: 'input, 'encloser: 'input, 'input>(
    context: &'context Context,
    open: &'encloser str,
    close: &'encloser str,
    both: &'encloser str,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarDecl> {
    move |input: &'input str| map(
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
        |x| ast::VarDecl::TupleDecl(x.unwrap_or(Vec::new())),
    )(input)
}

#[named]
pub fn fn_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::FnBind> {
    |input: &'input str| map(
        tuple((
            lex(lexer::keyword(context, "fn")),
            fn_decl(context),
            stats_block(context),
        )),
        |x| ast::FnBind(x.0, x.1, x.2),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn fn_decl<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::FnDecl> {
    |input: &'input str| map(
        tuple((
            lex(lexer::ident(context)),
            tuple_var_decl(context, "(", ")", "()"),
            opt(preceded(
                lex(lexer::op_code(context, "->")),
                type_expr(context),
            )),
        )),
        |x| ast::FnDecl(x.0, x.1, x.2),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn type_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TypeExpr>> {
    |input: &'input str| map(
        tuple((type_term(context), many0(type_op(context)))),
        |x| Rc::new(ast::TypeExpr(x.0, x.1))
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn type_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TypeOp> {
    |input: &'input str| alt((
        access_type_op(context),
    ))
    .context(function_name!().to_owned())
    .parse(input)
}

fn access_type_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TypeOp> {
    |input: &'input str| map(
        tuple((
            lex(lexer::op_code(context, "::")),
            type_term(context),
        )),
        |x| ast::TypeOp::Access(x.0, x.1),
    )(input)
}

#[named]
pub fn type_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::TypeTerm>> {
    |input: &'input str| map(
        alt((
            eval_type_type_term(context),
        )),
        |x| Rc::new(x),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

fn eval_type_type_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TypeTerm> {
    |input: &'input str| map(
        lex(lexer::ident(context)),
        |x| ast::TypeTerm::EvalType(x),
    )(input)
}

#[named]
pub fn stats_block<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::StatsBlock> {
    |input: &'input str| map(
        delimited(
            lex(lexer::op_code(context, "{")),
            tuple((
                many0(stat(context)),
                opt(expr(context)),
            )),
            lex(lexer::op_code(context, "}")),
        ),
        |x| ast::StatsBlock(x.0, x.1),
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
            map(var_bind(context), |x| ast::Stat::VarBind(x)),
            map(fn_bind(context), |x| ast::Stat::FnBind(x)),
            map(expr(context), |x| ast::Stat::Expr(x)),
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
        tuple((
            lex(lexer::keyword(context, "return")),
            opt(expr(context)),
        )),
        |x| ast::Stat::Return(x.0, x.1),
    )(input)
}

fn continue_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Stat> {
    |input: &'input str| map(
        lex(lexer::keyword(context, "continue")),
        |x| ast::Stat::Continue(x),
    )(input)
}

fn break_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Stat> {
    |input: &'input str| map(
        lex(lexer::keyword(context, "break")),
        |x| ast::Stat::Break(x),
    )(input)
}

#[named]
pub fn expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<ast::Expr>> {
    |input: &'input str| map(
        tuple((term(context), many0(op(context)))),
        |x| Rc::new(ast::Expr(x.0, x.1)),
    )
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
pub fn op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| alt((
        type_access_op(context),
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

fn type_access_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        tuple((
            lex(lexer::op_code(context, "::")),
            term(context),
        )),
        |x| ast::Op::TypeAccess(x.0, x.1),
    )(input)
}

fn access_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        tuple((
            alt((lex(lexer::op_code(context, ".")), lex(lexer::op_code(context, "?.")))),
            term(context),
        )),
        |x| ast::Op::Access(x.0, x.1),
    )(input)
}

fn eval_fn_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
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
        |x| ast::Op::EvalFn(x.unwrap_or(Vec::new())),
    )(input)
}

fn eval_spread_fn_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        tuple((
            lex(lexer::op_code(context, "(")),
            lex(lexer::op_code(context, "...")),
            expr(context),
            lex(lexer::op_code(context, ")")),
        )),
        |x| ast::Op::EvalSpreadFn(x.2),
    )(input)
}

fn eval_key_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        delimited(
            lex(lexer::op_code(context, "[")),
            expr(context),
            lex(lexer::op_code(context, "]")),
        ),
        |x| ast::Op::EvalKey(x),
    )(input)
}

fn cast_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        tuple((
            lex(lexer::keyword(context, "as")),
            type_expr(context),
        )),
        |x| ast::Op::CastOp(x.0, x.1),
    )(input)
}

fn infix_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
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
        |x| ast::Op::InfixOp(x.0, x.1),
    )(input)
}

fn assign_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        tuple((
            lex(lexer::op_code(context, "=")),
            term(context),
        )),
        |x| ast::Op::Assign(x.1),
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
        tuple((
            alt((
                lex(lexer::op_code(context, "+")),
                lex(lexer::op_code(context, "-")),
                lex(lexer::op_code(context, "!")),
                lex(lexer::op_code(context, "~")),
            )),
            term(context),
        )),
        |x| ast::Term::PrefixOp(x.0, x.1),
    )(input)
}

fn block_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        stats_block(context),
        |x| ast::Term::Block(x),
    )(input)
}

fn paren_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        delimited(
            lex(lexer::op_code(context, "(")),
            expr(context),
            lex(lexer::op_code(context, ")")),
        ),
        |x| ast::Term::Paren(x),
    )(input)
}

fn tuple_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
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
        |x| ast::Term::Tuple(
            x.3.map_or(
                vec![x.1.clone()],
                |y| [x.1].into_iter().chain([y.0].into_iter()).chain(y.1.into_iter()).collect(),
            ),
        ),
    )(input)
}

fn array_ctor_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        delimited(
            lex(lexer::op_code(context, "[")),
            opt(iter_expr(context)),
            lex(lexer::op_code(context, "]")),
        ),
        |x| ast::Term::ArrayCtor(x),
    )(input)
}

fn literal_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
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
        |x| ast::Term::Literal(x),
    )(input)
}

fn this_literal_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        lex(lexer::this_literal(context)),
        |x| ast::Term::ThisLiteral(x),
    )(input)
}

fn interpolated_string_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        lex(lexer::interpolated_string(context)),
        |x| ast::Term::InterpolatedString(x),
    )(input)
}

fn eval_var_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        lex(lexer::ident(context)),
        |x| ast::Term::EvalVar(x),
    )(input)
}

fn let_in_bind_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        tuple((
            var_bind(context),
            lex(lexer::keyword(context, "in")),
            expr(context),
        )),
        |x| ast::Term::LetInBind(x.0, x.1, x.2),
    )(input)
}

fn if_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        tuple((
            lex(lexer::keyword(context, "if")),
            expr(context),
            stats_block(context),
            opt(
                tuple((
                    lex(lexer::keyword(context, "else")),
                    alt((
                        map(
                            if_term(context),
                            |x| ast::StatsBlock(Vec::new(), Some(Rc::new(ast::Expr(Rc::new(x), Vec::new())))),
                        ),
                        stats_block(context),
                    )),
                )),
            ),
        )),
        |x| ast::Term::If(x.0, x.1, x.2, x.3),
    )(input)
}

fn while_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        tuple((
            lex(lexer::keyword(context, "while")),
            expr(context),
            stats_block(context),
        )),
        |x| ast::Term::While(x.0, x.1, x.2),
    )(input)
}

fn loop_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        tuple((
            lex(lexer::keyword(context, "loop")),
            stats_block(context),
        )),
        |x| ast::Term::Loop(x.0, x.1),
    )(input)
}

fn for_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        tuple((
            many1(
                tuple((
                    lex(lexer::keyword(context, "for")),
                    for_bind(context),
                )),
            ),
            stats_block(context),
        )),
        |x| ast::Term::For(x.0, x.1),
    )(input)
}

fn closure_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        tuple((
            tuple_var_decl(context, "|", "|", "||"),
            expr(context),
        )),
        |x| ast::Term::Closure(x.0, x.1),
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
        |x|
            x.3.map_or(
                ast::IterExpr::Range(x.0.clone(), x.2.clone()),
                |y| ast::IterExpr::SteppedRange(x.0, x.2, y),
            ),
    )(input)
}

fn spread_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::IterExpr> {
    |input: &'input str| map(
        preceded(
            lex(lexer::op_code(context, "...")),
            expr(context),
        ),
        |x| ast::IterExpr::Spread(x),
    )(input)
}

fn elements_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::IterExpr> {
    |input: &'input str| map(
        terminated(
            separated_list1(lex(lexer::op_code(context, ",")), expr(context)),
            opt(lex(lexer::op_code(context, ","))),
        ),
        |x| ast::IterExpr::Elements(x),
    )(input)
}

#[named]
pub fn arg_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ArgExpr> {
    |input: &'input str| map(
        tuple((
            opt(lex(lexer::keyword(context, "mut"))),
            expr(context),
        )),
        |x| ast::ArgExpr(
            x.0.map(|y| ast::MutAttr(y)),
            x.1,
        ),
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
        tuple((
            lex(lexer::keyword(context, "let")),
            var_decl(context),
            lex(lexer::op_code(context, "<-")),
            for_iter_expr(context),
        )),
        |x| ast::ForBind::Let(x.0, x.1, x.3)
    )(input)
}

fn assign_for_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForBind> {
    |input: &'input str| map(
        separated_pair(
            expr(context),
            lex(lexer::op_code(context, "<-")),
            for_iter_expr(context),
        ),
        |x| ast::ForBind::Assign(x.0, x.1)
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
        |x|
            x.3.map_or(
                ast::ForIterExpr::Range(x.0.clone(), x.2.clone()),
                |y| ast::ForIterExpr::SteppedRange(x.0, x.2, y),
            ),
    )(input)
}

fn spread_for_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForIterExpr> {
    |input: &'input str| map(
        expr(context),
        |x| ast::ForIterExpr::Spread(x),
    )(input)
}
