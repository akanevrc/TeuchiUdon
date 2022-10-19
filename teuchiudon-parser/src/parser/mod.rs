pub mod ast;

use nom::{
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
use super::ParsedResult;
use crate::context::Context;
use crate::lexer::{
    self,
    lex,
};

pub fn target<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Target> {
    |input: &'input str| alt((
        value(ast::Target(None), lex(lexer::eof)),
        map(terminated(body(context), lex(lexer::eof)), |x| ast::Target(Some(x)))
    ))(input)
}

pub fn body<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Body> {
    |input: &'input str| map(
        many1(top_stat(context)),
        |x| ast::Body(x)
    )(input)
}

pub fn top_stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TopStat> {
    |input: &'input str| alt((
        var_bind_top_stat(context),
        fn_bind_top_stat(context),
        stat_top_stat(context),
    ))(input)
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

pub fn var_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarBind> {
    |input: &'input str| map(
        tuple((
            lex(lexer::keyword(context, "let")),
            opt(lex(lexer::keyword(context, "mut"))),
            var_decl(context),
            lex(lexer::op_code(context, "=")),
            expr(context),
        )),
        |x| ast::VarBind(
            x.0,
            x.1.map(|x| ast::MutAttr(x)),
            x.2,
            x.4,
        ),
    )(input)
}

pub fn var_decl<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarDecl> {
    |input: &'input str| alt((
        single_var_decl(context),
        tuple_var_decl(context),
    ))(input)
}

fn single_var_decl<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarDecl> {
    |input: &'input str| map(
        var_decl_part(context),
        |x| ast::VarDecl::SingleDecl(x),
    )(input)
}

fn tuple_var_decl<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarDecl> {
    |input: &'input str| map(
        delimited(
            lex(lexer::op_code(context, "(")),
            opt(
                terminated(
                    separated_list1(lex(lexer::op_code(context, ",")), var_decl(context)),
                    opt(lex(lexer::op_code(context, ","))),
                ),
            ),
            lex(lexer::op_code(context, ")")),
        ),
        |x| ast::VarDecl::TupleDecl(x.unwrap_or(Vec::new())),
    )(input)
}

pub fn var_decl_part<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::VarDeclPart> {
    |input: &'input str| map(
        tuple((
            lex(lexer::ident(context)),
            opt(
                preceded(
                    lex(lexer::op_code(context, ":")),
                    type_expr(context),
                ),
            ),
        )),
        |x| ast::VarDeclPart(x.0, x.1),
    )(input)
}

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
    )(input)
}

pub fn fn_decl<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::FnDecl> {
    |input: &'input str| map(
        tuple((
            lex(lexer::ident(context)),
            tuple_var_decl(context),
            opt(preceded(
                lex(lexer::op_code(context, "->")),
                type_expr(context),
            )),
        )),
        |x| ast::FnDecl(x.0, x.1, x.2),
    )(input)
}

pub fn type_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TypeExpr> {
    |input: &'input str| map(
        tuple((type_term(context), opt(type_op(context)))),
        |x| ast::TypeExpr(x.0, x.1)
    )(input)
}

pub fn type_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TypeOp> {
    |input: &'input str| alt((
        access_type_op(context),
    ))(input)
}

fn access_type_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TypeOp> {
    |input: &'input str| map(
        tuple((
            lex(lexer::op_code(context, "::")),
            type_expr(context),
        )),
        |x| ast::TypeOp::Access(x.0, Box::new(x.1)),
    )(input)
}

pub fn type_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TypeTerm> {
    |input: &'input str| alt((
        eval_type_type_term(context),
    ))(input)
}

fn eval_type_type_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::TypeTerm> {
    |input: &'input str| map(
        lex(lexer::ident(context)),
        |x| ast::TypeTerm::EvalType(x),
    )(input)
}

pub fn stats_block<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::StatsBlock> {
    |input: &'input str| map(
        delimited(
            lex(lexer::op_code(context, "{")),
            tuple((
                many0(
                    terminated(
                        alt((
                            return_stat(context),
                            continue_stat(context),
                            break_stat(context),
                            map(var_bind(context), |x| ast::Stat::VarBind(x)),
                            map(expr(context), |x| ast::Stat::Expr(x)),
                        )),
                        lex(lexer::op_code(context, ";")),
                    ),
                ),
                opt(expr(context)),
            )),
            lex(lexer::op_code(context, "}")),
        ),
        |x| ast::StatsBlock(x.0, x.1.map(|y| Box::new(y))),
    )(input)
}

pub fn stat<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Stat> {
    |input: &'input str| terminated(
        alt((
            return_stat(context),
            continue_stat(context),
            break_stat(context),
            map(var_bind(context), |x| ast::Stat::VarBind(x)),
            map(expr(context), |x| ast::Stat::Expr(x)),
        )),
        lex(lexer::op_code(context, ";")),
    )(input)
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

pub fn expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Expr> {
    |input: &'input str| map(
        tuple((term(context), opt(op(context)))),
        |x| ast::Expr(x.0, x.1),
    )(input)
}

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
    ))(input)
}

fn type_access_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        tuple((
            lex(lexer::op_code(context, "::")),
            expr(context),
        )),
        |x| ast::Op::TypeAccess(x.0, Box::new(x.1)),
    )(input)
}

fn access_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        tuple((
            alt((lex(lexer::op_code(context, ".")), lex(lexer::op_code(context, "?.")))),
            expr(context),
        )),
        |x| ast::Op::Access(x.0, Box::new(x.1)),
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
                    lex(lexer::op_code(context, ",")),
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
        |x| ast::Op::EvalSpreadFn(Box::new(x.2)),
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
        |x| ast::Op::EvalKey(Box::new(x)),
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
            expr(context),
        )),
        |x| ast::Op::InfixOp(x.0, Box::new(x.1)),
    )(input)
}

fn assign_op<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Op> {
    |input: &'input str| map(
        tuple((
            lex(lexer::op_code(context, "=")),
            expr(context),
        )),
        |x| ast::Op::Assign(Box::new(x.1)),
    )(input)
}

pub fn term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| alt((
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
    ))(input)
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
        |x| ast::Term::PrefixOp(x.0, Box::new(x.1)),
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
        |x| ast::Term::Paren(Box::new(x)),
    )(input)
}

fn tuple_term<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Term> {
    |input: &'input str| map(
        delimited(
            lex(lexer::op_code(context, "(")),
            alt((
                map(
                    terminated(
                        expr(context),
                        lex(lexer::op_code(context, ",")),
                    ),
                    |x| vec![x],
                ),
                map(
                    tuple((
                        expr(context),
                        many1(
                            preceded(
                                lex(lexer::op_code(context, ",")),
                                expr(context),
                            ),
                        ),
                        opt(lex(lexer::op_code(context, ","))),
                    )),
                    |x|
                        [x.0].into_iter().chain(x.1.into_iter()).collect(),
                ),
            )),
            lex(lexer::op_code(context, ")")),
        ),
        |x| ast::Term::Tuple(x),
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
            lex(lexer::integer_literal),
            lex(lexer::hex_integer_literal),
            lex(lexer::bin_integer_literal),
            lex(lexer::real_number_literal),
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
        |x| ast::Term::Literal(x),
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
        |x| ast::Term::LetInBind(Box::new(x.0), x.1, Box::new(x.2)),
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
                            |x| ast::StatsBlock(Vec::new(), Some(Box::new(ast::Expr(x, None)))),
                        ),
                        stats_block(context),
                    )),
                )),
            ),
        )),
        |x| ast::Term::If(x.0, Box::new(x.1), x.2, x.3),
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
        |x| ast::Term::While(x.0, Box::new(x.1), x.2),
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
            lex(lexer::op_code(context, "|")),
            tuple_var_decl(context),
            lex(lexer::op_code(context, "|")),
            expr(context),
        )),
        |x| ast::Term::Closure(x.1, Box::new(x.3)),
    )(input)
}

pub fn iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::IterExpr> {
    |input: &'input str| alt((
        range_iter_expr(context),
        spread_iter_expr(context),
        elements_iter_expr(context),
    ))(input)
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
                ast::IterExpr::Range(Box::new(x.0.clone()), Box::new(x.2.clone())),
                |y| ast::IterExpr::SteppedRange(Box::new(x.0), Box::new(x.2), Box::new(y)),
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
        |x| ast::IterExpr::Spread(Box::new(x)),
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
            Box::new(x.1),
        ),
    )(input)
}

pub fn for_bind<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForBind> {
    |input: &'input str| alt((
        let_for_bind(context),
        assign_for_bind(context),
    ))(input)
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
        |x| ast::ForBind::Assign(Box::new(x.0), x.1)
    )(input)
}

pub fn for_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForIterExpr> {
    |input: &'input str| alt((
        range_for_iter_expr(context),
        spread_for_iter_expr(context),
    ))(input)
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
                ast::ForIterExpr::Range(Box::new(x.0.clone()), Box::new(x.2.clone())),
                |y| ast::ForIterExpr::SteppedRange(Box::new(x.0), Box::new(x.2), Box::new(y)),
            ),
    )(input)
}

fn spread_for_iter_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::ForIterExpr> {
    |input: &'input str| map(
        expr(context),
        |x| ast::ForIterExpr::Spread(Box::new(x)),
    )(input)
}
