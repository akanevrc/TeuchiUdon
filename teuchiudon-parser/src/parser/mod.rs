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
        value(ast::Target::Empty, lex(lexer::eof)),
        map(terminated(body, lex(lexer::eof)), |x| ast::Target::Body(x))
    ))(input)
}

pub fn body(input: &str) -> ParsedResult<ast::Body> {
    map(many1(top_stat), |x| ast::Body::Stats(x))(input)
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
            x.0.map_or(ast::AccessAttr::None, |x| ast::AccessAttr::Attr(x)),
            x.1.map_or(ast::SyncAttr::None, |x| ast::SyncAttr::Attr(x)),
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
            x.0.map_or(ast::AccessAttr::None, |x| ast::AccessAttr::Attr(x)),
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
        |x| ast::VarBind::Bind(
            x.0,
            x.1.map_or(ast::MutAttr::None, |x| ast::MutAttr::Attr(x)),
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
        |x| ast::VarDeclPart::Part(x.0, x.1.unwrap_or(ast::TypeExpr::None)),
    )(input)
}

pub fn fn_bind(input: &str) -> ParsedResult<ast::FnBind> {
    map(
        tuple((
            lex(lexer::keyword("fn")),
            fn_decl,
            stats_block,
        )),
        |x| ast::FnBind::Bind(x.0, x.1, x.2),
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
        |x| ast::FnDecl::Decl(x.0, x.1, x.2.unwrap_or(ast::TypeExpr::None)),
    )(input)
}

pub fn ident(input: &str) -> ParsedResult<ast::Ident> {
    map(
        lex(lexer::ident),
        |x| ast::Ident::Ident(x),
    )(input)
}

pub fn stats_block(input: &str) -> ParsedResult<Vec<ast::Stat>> {
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
        |mut x| x.1.map_or(
            x.0.clone(),
            |y| { x.0.push(ast::Stat::ImplicitReturn(y)); x.0 }
        )
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
        |x| ast::Stat::Return(
            x.0,
            x.1.unwrap_or(ast::Expr::Literal(lexer::ast::Literal::Unit)),
        ),
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
        alt((
            block_expr,
            paren_expr,
            tuple_expr,
            array_ctor_expr,
            literal_expr,
            this_literal_expr,
            interpolated_string_expr,
            eval_var_expr,
            eval_type_of_expr,
        )),
        alt((
            type_access_expr,
            access_expr,
            eval_func_expr,
            eval_spread_func_expr,
            eval_key_expr,
            prefix_op_expr,
            cast_expr,
            infix_op_expr,
            assign_expr,
        )),
        alt((
            let_in_bind_expr,
            if_expr,
            while_expr,
            loop_expr,
            for_expr,
            closure_expr,
        )),
    ))(input)
}

fn block_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        stats_block,
        |x| ast::Expr::Block(x),
    )(input)
}

fn paren_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        delimited(
            lex(lexer::encloser("(")),
            expr,
            lex(lexer::encloser(")")),
        ),
        |x| ast::Expr::Paren(Box::new(x)),
    )(input)
}

fn tuple_expr(input: &str) -> ParsedResult<ast::Expr> {
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
        |x| ast::Expr::Tuple(x),
    )(input)
}

fn array_ctor_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        delimited(
            lex(lexer::encloser("[")),
            opt(iter_expr),
            lex(lexer::encloser("]")),
        ),
        |x| ast::Expr::ArrayCtor(x.unwrap_or(ast::IterExpr::None)),
    )(input)
}

fn literal_expr(input: &str) -> ParsedResult<ast::Expr> {
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
        |x| ast::Expr::Literal(x),
    )(input)
}

fn this_literal_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        lex(lexer::this_literal),
        |x| ast::Expr::Literal(x),
    )(input)
}

fn interpolated_string_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        lex(lexer::interpolated_string),
        |x| ast::Expr::InterpolatedString(x),
    )(input)
}

fn eval_var_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        ident,
        |x| ast::Expr::EvalVar(x),
    )(input)
}

fn eval_type_of_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        lex(lexer::keyword("typeof")),
        |x| ast::Expr::EvalTypeOf(x),
    )(input)
}

fn type_access_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        tuple((
            type_expr,
            lex(lexer::op_code("::")),
            expr,
        )),
        |x| ast::Expr::TypeAccess(x.0, x.1, Box::new(x.2)),
    )(input)
}

fn access_expr(input: &str) -> ParsedResult<ast::Expr> {
    fn upper_expr(input: &str) -> ParsedResult<ast::Expr> {
        alt((
            block_expr,
            paren_expr,
            tuple_expr,
            array_ctor_expr,
            literal_expr,
            this_literal_expr,
            interpolated_string_expr,
            eval_var_expr,
            eval_type_of_expr,
            type_access_expr,
        ))(input)
    }

    map(
        tuple((
            upper_expr,
            alt((lex(lexer::op_code(".")), lex(lexer::op_code("?.")))),
            expr,
        )),
        |x| ast::Expr::Access(Box::new(x.0), x.1, Box::new(x.2)),
    )(input)
}

fn eval_func_expr(input: &str) -> ParsedResult<ast::Expr> {
    fn upper_expr(input: &str) -> ParsedResult<ast::Expr> {
        alt((
            block_expr,
            paren_expr,
            tuple_expr,
            array_ctor_expr,
            literal_expr,
            this_literal_expr,
            interpolated_string_expr,
            eval_var_expr,
            eval_type_of_expr,
            type_access_expr,
            access_expr,
        ))(input)
    }

    map(
        tuple((
            upper_expr,
            lex(lexer::encloser("(")),
            many0(arg_expr),
            lex(lexer::encloser(")")),
        )),
        |x| ast::Expr::EvalFunc(Box::new(x.0), x.2),
    )(input)
}

fn eval_spread_func_expr(input: &str) -> ParsedResult<ast::Expr> {
    fn upper_expr(input: &str) -> ParsedResult<ast::Expr> {
        alt((
            block_expr,
            paren_expr,
            tuple_expr,
            array_ctor_expr,
            literal_expr,
            this_literal_expr,
            interpolated_string_expr,
            eval_var_expr,
            eval_type_of_expr,
            type_access_expr,
            access_expr,
            eval_func_expr,
        ))(input)
    }

    map(
        tuple((
            upper_expr,
            lex(lexer::encloser("(")),
            lex(lexer::delimiter("...")),
            expr,
            lex(lexer::encloser(")")),
        )),
        |x| ast::Expr::EvalSpreadFunc(Box::new(x.0), Box::new(x.3)),
    )(input)
}

fn eval_key_expr(input: &str) -> ParsedResult<ast::Expr> {
    fn upper_expr(input: &str) -> ParsedResult<ast::Expr> {
        alt((
            block_expr,
            paren_expr,
            tuple_expr,
            array_ctor_expr,
            literal_expr,
            this_literal_expr,
            interpolated_string_expr,
            eval_var_expr,
            eval_type_of_expr,
            type_access_expr,
            access_expr,
            eval_func_expr,
            eval_spread_func_expr,
        ))(input)
    }

    map(
        tuple((
            upper_expr,
            lex(lexer::encloser("[")),
            expr,
            lex(lexer::encloser("]")),
        )),
        |x| ast::Expr::EvalKey(Box::new(x.0), Box::new(x.2)),
    )(input)
}

fn prefix_op_expr(input: &str) -> ParsedResult<ast::Expr> {
    fn upper_expr(input: &str) -> ParsedResult<ast::Expr> {
        alt((
            block_expr,
            paren_expr,
            tuple_expr,
            array_ctor_expr,
            literal_expr,
            this_literal_expr,
            interpolated_string_expr,
            eval_var_expr,
            eval_type_of_expr,
            type_access_expr,
            access_expr,
            eval_func_expr,
            eval_spread_func_expr,
            eval_key_expr,
        ))(input)
    }

    map(
        tuple((
            alt((
                lex(lexer::op_code("+")),
                lex(lexer::op_code("-")),
                lex(lexer::op_code("!")),
                lex(lexer::op_code("~")),
            )),
            upper_expr,
        )),
        |x| ast::Expr::PrefixOp(x.0, Box::new(x.1)),
    )(input)
}

fn cast_expr(input: &str) -> ParsedResult<ast::Expr> {
    fn upper_expr(input: &str) -> ParsedResult<ast::Expr> {
        alt((
            block_expr,
            paren_expr,
            tuple_expr,
            array_ctor_expr,
            literal_expr,
            this_literal_expr,
            interpolated_string_expr,
            eval_var_expr,
            eval_type_of_expr,
            type_access_expr,
            access_expr,
            eval_func_expr,
            eval_spread_func_expr,
            eval_key_expr,
            prefix_op_expr,
        ))(input)
    }

    map(
        tuple((
            upper_expr,
            lex(lexer::keyword("as")),
            type_expr,
        )),
        |x| ast::Expr::Cast(Box::new(x.0), x.1, x.2),
    )(input)
}

fn infix_op_expr(input: &str) -> ParsedResult<ast::Expr> {
    fn upper_expr(input: &str) -> ParsedResult<ast::Expr> {
        alt((
            block_expr,
            paren_expr,
            tuple_expr,
            array_ctor_expr,
            literal_expr,
            this_literal_expr,
            interpolated_string_expr,
            eval_var_expr,
            eval_type_of_expr,
            type_access_expr,
            access_expr,
            eval_func_expr,
            eval_spread_func_expr,
            eval_key_expr,
            prefix_op_expr,
            cast_expr,
        ))(input)
    }

    map(
        tuple((
            upper_expr,
            alt((
                lex(lexer::op_code("*")),
                lex(lexer::op_code("/")),
                lex(lexer::op_code("%")),
                lex(lexer::op_code("+")),
                lex(lexer::op_code("-")),
                lex(lexer::op_code("<<")),
                lex(lexer::op_code(">>")),
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
            expr,
        )),
        |x| ast::Expr::InfixOp(Box::new(x.0), x.1, Box::new(x.2)),
    )(input)
}

fn assign_expr(input: &str) -> ParsedResult<ast::Expr> {
    fn upper_expr(input: &str) -> ParsedResult<ast::Expr> {
        alt((
            block_expr,
            paren_expr,
            tuple_expr,
            array_ctor_expr,
            literal_expr,
            this_literal_expr,
            interpolated_string_expr,
            eval_var_expr,
            eval_type_of_expr,
            type_access_expr,
            access_expr,
            eval_func_expr,
            eval_spread_func_expr,
            eval_key_expr,
            prefix_op_expr,
            cast_expr,
            infix_op_expr,
        ))(input)
    }

    map(
        separated_pair(
            upper_expr,
            lex(lexer::delimiter("<-")),
            expr,
        ),
        |x| ast::Expr::Assign(Box::new(x.0), Box::new(x.1)),
    )(input)
}

fn let_in_bind_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        tuple((
            var_bind,
            lex(lexer::keyword("in")),
            expr,
        )),
        |x| ast::Expr::LetInBind(Box::new(x.0), x.1, Box::new(x.2)),
    )(input)
}

fn if_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        tuple((
            lex(lexer::keyword("if")),
            expr,
            stats_block,
            opt(
                tuple((
                    lex(lexer::keyword("else")),
                    alt((
                        map(if_expr, |x| vec![ast::Stat::Expr(x)]),
                        stats_block,
                    )),
                )),
            ),
        )),
        |x| ast::Expr::If(x.0, Box::new(x.1), x.2, x.3),
    )(input)
}

fn while_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        tuple((
            lex(lexer::keyword("while")),
            expr,
            stats_block,
        )),
        |x| ast::Expr::While(x.0, Box::new(x.1), x.2),
    )(input)
}

fn loop_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        tuple((
            lex(lexer::keyword("loop")),
            stats_block,
        )),
        |x| ast::Expr::Loop(x.0, x.1),
    )(input)
}

fn for_expr(input: &str) -> ParsedResult<ast::Expr> {
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
        |x| {
            let (f, fb) = x.0.into_iter().unzip();
            ast::Expr::For(f, fb, x.1)
        },
    )(input)
}

fn closure_expr(input: &str) -> ParsedResult<ast::Expr> {
    map(
        tuple((
            lex(lexer::encloser("|")),
            tuple_var_decl,
            lex(lexer::encloser("|")),
            expr,
        )),
        |x| ast::Expr::Closure(x.1, Box::new(x.3)),
    )(input)
}

pub fn type_expr(input: &str) -> ParsedResult<ast::TypeExpr> {
    alt((
        eval_type_type_expr,
        type_access_type_expr,
    ))(input)
}

fn eval_type_type_expr(input: &str) -> ParsedResult<ast::TypeExpr> {
    map(
        ident,
        |x| ast::TypeExpr::EvalType(x),
    )(input)
}

fn type_access_type_expr(input: &str) -> ParsedResult<ast::TypeExpr> {
    fn upper_type_expr(input: &str) -> ParsedResult<ast::TypeExpr> {
        eval_type_type_expr(input)
    }

    map(
        tuple((
            upper_type_expr,
            lex(lexer::op_code("::")),
            type_expr,
        )),
        |x| ast::TypeExpr::TypeAccess(Box::new(x.0), x.1, Box::new(x.2)),
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
        |x| ast::ArgExpr::Expr(
            x.0.map_or(ast::MutAttr::None, |y| ast::MutAttr::Attr(y)),
            x.1
        )
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
    map(
        separated_pair(
            expr,
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
