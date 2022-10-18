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
use crate::lexer::{
    self as lexer,
    lex,
};

pub fn target(input: &str) -> ParsedResult<ast::Target> {
    alt((
        value(ast::Target(None), lex(lexer::eof)),
        map(terminated(body, lex(lexer::eof)), |x| ast::Target(Some(x)))
    ))(input)
}

pub fn body(input: &str) -> ParsedResult<ast::Body> {
    map(many1(top_stat), |x| ast::Body(x))(input)
}

pub fn top_stat(input: &str) -> ParsedResult<ast::TopStat> {
    alt((
        var_bind_top_stat,
        fn_bind_top_stat,
        stat_top_stat,
    ))(input)
}

fn var_bind_top_stat(input: &str) -> ParsedResult<ast::TopStat> {
    map(
        tuple((
            opt(lex(lexer::keyword("pub"))),
            opt(alt((
                lex(lexer::keyword("sync")),
                lex(lexer::keyword("linear")),
                lex(lexer::keyword("smooth")),
            ))),
            var_bind,
            lex(lexer::end(";")),
        )),
        |x| ast::TopStat::VarBind(
            x.0.map(|x| ast::AccessAttr(x)),
            x.1.map(|x| ast::SyncAttr(x)),
            x.2,
        ),
    )(input)
}

fn fn_bind_top_stat(input: &str) -> ParsedResult<ast::TopStat> {
    map(
        tuple((
            opt(lex(lexer::keyword("pub"))),
            fn_bind,
        )),
        |x| ast::TopStat::FnBind(
            x.0.map(|x| ast::AccessAttr(x)),
            x.1,
        ),
    )(input)
}

fn stat_top_stat(input: &str) -> ParsedResult<ast::TopStat> {
    map(stat, |x| ast::TopStat::Stat(x))(input)
}

pub fn var_bind(input: &str) -> ParsedResult<ast::VarBind> {
    map(
        tuple((
            lex(lexer::keyword("let")),
            opt(lex(lexer::keyword("mut"))),
            var_decl,
            lex(lexer::delimiter("=")),
            expr,
        )),
        |x| ast::VarBind(
            x.0,
            x.1.map(|x| ast::MutAttr(x)),
            x.2,
            x.4,
        ),
    )(input)
}

pub fn var_decl(input: &str) -> ParsedResult<ast::VarDecl> {
    alt((
        single_var_decl,
        tuple_var_decl,
    ))(input)
}

fn single_var_decl(input: &str) -> ParsedResult<ast::VarDecl> {
    map(
        var_decl_part,
        |x| ast::VarDecl::SingleDecl(x),
    )(input)
}

fn tuple_var_decl(input: &str) -> ParsedResult<ast::VarDecl> {
    map(
        delimited(
            lex(lexer::encloser("(")),
            opt(
                terminated(
                    separated_list1(lex(lexer::delimiter(",")), var_decl),
                    opt(lex(lexer::delimiter(","))),
                ),
            ),
            lex(lexer::encloser(")")),
        ),
        |x| ast::VarDecl::TupleDecl(x.unwrap_or(Vec::new())),
    )(input)
}

pub fn var_decl_part(input: &str) -> ParsedResult<ast::VarDeclPart> {
    map(
        tuple((
            ident,
            opt(
                preceded(
                    lex(lexer::delimiter(":")),
                    type_expr,
                ),
            ),
        )),
        |x| ast::VarDeclPart(x.0, x.1),
    )(input)
}

pub fn fn_bind(input: &str) -> ParsedResult<ast::FnBind> {
    map(
        tuple((
            lex(lexer::keyword("fn")),
            fn_decl,
            stats_block,
        )),
        |x| ast::FnBind(x.0, x.1, x.2),
    )(input)
}

pub fn fn_decl(input: &str) -> ParsedResult<ast::FnDecl> {
    map(
        tuple((
            ident,
            tuple_var_decl,
            opt(preceded(
                lex(lexer::delimiter("->")),
                type_expr,
            )),
        )),
        |x| ast::FnDecl(x.0, x.1, x.2),
    )(input)
}

pub fn ident(input: &str) -> ParsedResult<ast::Ident> {
    map(
        lex(lexer::ident),
        |x| ast::Ident(x),
    )(input)
}

pub fn type_expr(input: &str) -> ParsedResult<ast::TypeExpr> {
    alt((
        map(
            tuple((type_term, type_op)),
            |x| ast::TypeExpr(x.0, Some(x.1))
        ),
        map(
            type_term,
            |x| ast::TypeExpr(x, None)
        ),
    ))(input)
}

pub fn type_op(input: &str) -> ParsedResult<ast::TypeOp> {
    alt((
        access_type_expr,
    ))(input)
}

fn access_type_expr(input: &str) -> ParsedResult<ast::TypeOp> {
    map(
        tuple((
            lex(lexer::op_code("::")),
            type_expr,
        )),
        |x| ast::TypeOp::Access(x.0, Box::new(x.1)),
    )(input)
}

pub fn type_term(input: &str) -> ParsedResult<ast::TypeTerm> {
    alt((
        eval_type_type_term,
    ))(input)
}

fn eval_type_type_term(input: &str) -> ParsedResult<ast::TypeTerm> {
    map(
        ident,
        |x| ast::TypeTerm::EvalType(x),
    )(input)
}

pub fn stats_block(input: &str) -> ParsedResult<ast::StatsBlock> {
    map(
        delimited(
            lex(lexer::encloser("{")),
            tuple((
                many0(
                    terminated(
                        alt((
                            return_stat,
                            continue_stat,
                            break_stat,
                            map(var_bind, |x| ast::Stat::VarBind(x)),
                            map(expr, |x| ast::Stat::Expr(x)),
                        )),
                        lex(lexer::end(";")),
                    ),
                ),
                opt(expr),
            )),
            lex(lexer::encloser("}")),
        ),
        |x| ast::StatsBlock(x.0, x.1.map(|y| Box::new(y))),
    )(input)
}

pub fn stat(input: &str) -> ParsedResult<ast::Stat> {
    terminated(
        alt((
            return_stat,
            continue_stat,
            break_stat,
            map(var_bind, |x| ast::Stat::VarBind(x)),
            map(expr, |x| ast::Stat::Expr(x)),
        )),
        lex(lexer::end(";")),
    )(input)
}

fn return_stat(input: &str) -> ParsedResult<ast::Stat> {
    map(
        tuple((
            lex(lexer::keyword("return")),
            opt(expr),
        )),
        |x| ast::Stat::Return(x.0, x.1),
    )(input)
}

fn continue_stat(input: &str) -> ParsedResult<ast::Stat> {
    map(
        lex(lexer::keyword("continue")),
        |x| ast::Stat::Continue(x),
    )(input)
}

fn break_stat(input: &str) -> ParsedResult<ast::Stat> {
    map(
        lex(lexer::keyword("break")),
        |x| ast::Stat::Break(x),
    )(input)
}

pub fn expr(input: &str) -> ParsedResult<ast::Expr> {
    alt((
        map(
            tuple((term, op)),
            |x| ast::Expr(x.0, Some(x.1)),
        ),
        map(
            term,
            |x| ast::Expr(x, None),
        ),
    ))(input)
}

pub fn op(input: &str) -> ParsedResult<ast::Op> {
    alt((
        type_access_op,
        access_op,
        eval_fn_op,
        eval_spread_fn_op,
        eval_key_op,
        cast_op,
        infix_op,
        assign_op,
    ))(input)
}

fn type_access_op(input: &str) -> ParsedResult<ast::Op> {
    map(
        tuple((
            lex(lexer::op_code("::")),
            expr,
        )),
        |x| ast::Op::TypeAccess(x.0, Box::new(x.1)),
    )(input)
}

fn access_op(input: &str) -> ParsedResult<ast::Op> {
    map(
        tuple((
            alt((lex(lexer::op_code(".")), lex(lexer::op_code("?.")))),
            expr,
        )),
        |x| ast::Op::Access(x.0, Box::new(x.1)),
    )(input)
}

fn eval_fn_op(input: &str) -> ParsedResult<ast::Op> {
    map(
        delimited(
            lex(lexer::encloser("(")),
            opt(
                terminated(
                    separated_list1(lex(lexer::delimiter(",")), arg_expr),
                    lex(lexer::delimiter(",")),
                ),
            ),
            lex(lexer::encloser(")")),
        ),
        |x| ast::Op::EvalFn(x.unwrap_or(Vec::new())),
    )(input)
}

fn eval_spread_fn_op(input: &str) -> ParsedResult<ast::Op> {
    map(
        tuple((
            lex(lexer::encloser("(")),
            lex(lexer::delimiter("...")),
            expr,
            lex(lexer::encloser(")")),
        )),
        |x| ast::Op::EvalSpreadFn(Box::new(x.2)),
    )(input)
}

fn eval_key_op(input: &str) -> ParsedResult<ast::Op> {
    map(
        delimited(
            lex(lexer::encloser("[")),
            expr,
            lex(lexer::encloser("]")),
        ),
        |x| ast::Op::EvalKey(Box::new(x)),
    )(input)
}

fn cast_op(input: &str) -> ParsedResult<ast::Op> {
    map(
        tuple((
            lex(lexer::keyword("as")),
            type_expr,
        )),
        |x| ast::Op::CastOp(x.0, x.1),
    )(input)
}

fn infix_op(input: &str) -> ParsedResult<ast::Op> {
    map(
        tuple((
            alt((
                alt((
                    lex(lexer::op_code("*")),
                    lex(lexer::op_code("/")),
                    lex(lexer::op_code("%")),
                    lex(lexer::op_code("+")),
                    lex(lexer::op_code("-")),
                    lex(lexer::op_code("<<")),
                    lex(lexer::op_code(">>")),
                )),
                alt((
                    lex(lexer::op_code("<")),
                    lex(lexer::op_code(">")),
                    lex(lexer::op_code("<=")),
                    lex(lexer::op_code(">=")),
                    lex(lexer::op_code("==")),
                    lex(lexer::op_code("!=")),
                    lex(lexer::op_code("&")),
                    lex(lexer::op_code("^")),
                    lex(lexer::op_code("|")),
                    lex(lexer::op_code("&&")),
                    lex(lexer::op_code("||")),
                    lex(lexer::op_code("??")),
                    lex(lexer::op_code("|>")),
                    lex(lexer::op_code("<|")),
                )),
            )),
            expr,
        )),
        |x| ast::Op::InfixOp(x.0, Box::new(x.1)),
    )(input)
}

fn assign_op(input: &str) -> ParsedResult<ast::Op> {
    map(
        tuple((
            lex(lexer::delimiter("<-")),
            expr,
        )),
        |x| ast::Op::Assign(Box::new(x.1)),
    )(input)
}

pub fn term(input: &str) -> ParsedResult<ast::Term> {
    alt((
        prefix_op_term,
        block_term,
        paren_term,
        tuple_term,
        array_ctor_term,
        literal_term,
        this_literal_term,
        interpolated_string_term,
        eval_var_term,
        let_in_bind_term,
        if_term,
        while_term,
        loop_term,
        for_term,
        closure_term,
    ))(input)
}

fn prefix_op_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        tuple((
            alt((
                lex(lexer::op_code("+")),
                lex(lexer::op_code("-")),
                lex(lexer::op_code("!")),
                lex(lexer::op_code("~")),
            )),
            term,
        )),
        |x| ast::Term::PrefixOp(x.0, Box::new(x.1)),
    )(input)
}

fn block_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        stats_block,
        |x| ast::Term::Block(x),
    )(input)
}

fn paren_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        delimited(
            lex(lexer::encloser("(")),
            expr,
            lex(lexer::encloser(")")),
        ),
        |x| ast::Term::Paren(Box::new(x)),
    )(input)
}

fn tuple_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        delimited(
            lex(lexer::encloser("(")),
            alt((
                map(
                    terminated(
                        expr,
                        lex(lexer::delimiter(",")),
                    ),
                    |x| vec![x],
                ),
                map(
                    tuple((
                        expr,
                        many1(
                            preceded(
                                lex(lexer::delimiter(",")),
                                expr,
                            ),
                        ),
                        opt(lex(lexer::delimiter(","))),
                    )),
                    |x|
                        [x.0].into_iter().chain(x.1.into_iter()).collect(),
                ),
            )),
            lex(lexer::encloser(")")),
        ),
        |x| ast::Term::Tuple(x),
    )(input)
}

fn array_ctor_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        delimited(
            lex(lexer::encloser("[")),
            opt(iter_expr),
            lex(lexer::encloser("]")),
        ),
        |x| ast::Term::ArrayCtor(x),
    )(input)
}

fn literal_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        alt((
            lex(lexer::unit_literal),
            lex(lexer::null_literal),
            lex(lexer::bool_literal),
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

fn this_literal_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        lex(lexer::this_literal),
        |x| ast::Term::Literal(x),
    )(input)
}

fn interpolated_string_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        lex(lexer::interpolated_string),
        |x| ast::Term::InterpolatedString(x),
    )(input)
}

fn eval_var_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        ident,
        |x| ast::Term::EvalVar(x),
    )(input)
}

fn let_in_bind_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        tuple((
            var_bind,
            lex(lexer::keyword("in")),
            expr,
        )),
        |x| ast::Term::LetInBind(Box::new(x.0), x.1, Box::new(x.2)),
    )(input)
}

fn if_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        tuple((
            lex(lexer::keyword("if")),
            expr,
            stats_block,
            opt(
                tuple((
                    lex(lexer::keyword("else")),
                    alt((
                        map(
                            if_term,
                            |x| ast::StatsBlock(Vec::new(), Some(Box::new(ast::Expr(x, None)))),
                        ),
                        stats_block,
                    )),
                )),
            ),
        )),
        |x| ast::Term::If(x.0, Box::new(x.1), x.2, x.3),
    )(input)
}

fn while_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        tuple((
            lex(lexer::keyword("while")),
            expr,
            stats_block,
        )),
        |x| ast::Term::While(x.0, Box::new(x.1), x.2),
    )(input)
}

fn loop_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        tuple((
            lex(lexer::keyword("loop")),
            stats_block,
        )),
        |x| ast::Term::Loop(x.0, x.1),
    )(input)
}

fn for_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        tuple((
            many1(
                tuple((
                    lex(lexer::keyword("for")),
                    for_bind,
                )),
            ),
            stats_block,
        )),
        |x| ast::Term::For(x.0, x.1),
    )(input)
}

fn closure_term(input: &str) -> ParsedResult<ast::Term> {
    map(
        tuple((
            lex(lexer::encloser("|")),
            tuple_var_decl,
            lex(lexer::encloser("|")),
            expr,
        )),
        |x| ast::Term::Closure(x.1, Box::new(x.3)),
    )(input)
}

pub fn iter_expr(input: &str) -> ParsedResult<ast::IterExpr> {
    alt((
        elements_iter_expr,
        range_iter_expr,
        spread_iter_expr,
    ))(input)
}

fn elements_iter_expr(input: &str) -> ParsedResult<ast::IterExpr> {
    map(
        terminated(
            separated_list1(lex(lexer::delimiter(",")), expr),
            opt(lex(lexer::delimiter(","))),
        ),
        |x| ast::IterExpr::Elements(x),
    )(input)
}

fn range_iter_expr(input: &str) -> ParsedResult<ast::IterExpr> {
    map(
        tuple((
            expr,
            lex(lexer::op_code("..")),
            expr,
            opt(
                preceded(
                    lex(lexer::op_code("..")),
                    expr,
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

fn spread_iter_expr(input: &str) -> ParsedResult<ast::IterExpr> {
    map(
        preceded(
            lex(lexer::delimiter("...")),
            expr,
        ),
        |x| ast::IterExpr::Spread(Box::new(x)),
    )(input)
}

pub fn arg_expr(input: &str) -> ParsedResult<ast::ArgExpr> {
    map(
        tuple((
            opt(lex(lexer::keyword("mut"))),
            expr,
        )),
        |x| ast::ArgExpr(
            x.0.map(|y| ast::MutAttr(y)),
            x.1,
        ),
    )(input)
}

pub fn for_bind(input: &str) -> ParsedResult<ast::ForBind> {
    alt((
        let_for_bind,
        assign_for_bind,
    ))(input)
}

fn let_for_bind(input: &str) -> ParsedResult<ast::ForBind> {
    map(
        tuple((
            lex(lexer::keyword("let")),
            var_decl,
            lex(lexer::delimiter("<-")),
            for_iter_expr,
        )),
        |x| ast::ForBind::Let(x.0, x.1, x.3)
    )(input)
}

fn assign_for_bind(input: &str) -> ParsedResult<ast::ForBind> {
    fn left_expr(input: &str) -> ParsedResult<ast::Expr> {
        alt((
            map(
                tuple((term, left_op)),
                |x| ast::Expr(x.0, Some(x.1)),
            ),
            map(
                term,
                |x| ast::Expr(x, None),
            ),
        ))(input)
    }

    fn left_op(input: &str) -> ParsedResult<ast::Op> {
        alt((
            access_op,
            eval_fn_op,
            eval_spread_fn_op,
            eval_key_op,
            cast_op,
            infix_op,
        ))(input)
    }

    map(
        separated_pair(
            left_expr,
            lex(lexer::delimiter("<-")),
            for_iter_expr,
        ),
        |x| ast::ForBind::Assign(Box::new(x.0), x.1)
    )(input)
}

pub fn for_iter_expr(input: &str) -> ParsedResult<ast::ForIterExpr> {
    alt((
        range_for_iter_expr,
        spread_for_iter_expr,
    ))(input)
}

fn range_for_iter_expr(input: &str) -> ParsedResult<ast::ForIterExpr> {
    map(
        tuple((
            expr,
            lex(lexer::op_code("..")),
            expr,
            opt(
                preceded(
                    lex(lexer::op_code("..")),
                    expr,
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

fn spread_for_iter_expr(input: &str) -> ParsedResult<ast::ForIterExpr> {
    map(
        expr,
        |x| ast::ForIterExpr::Spread(Box::new(x)),
    )(input)
}
