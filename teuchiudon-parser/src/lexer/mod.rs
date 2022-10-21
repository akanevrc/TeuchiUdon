pub mod ast;

use nom::{
    error::VerboseError,
    branch::alt,
    bytes::complete::{
        is_not,
        tag,
    },
    character::complete::{
        anychar,
        char,
        multispace0,
        multispace1,
        none_of,
        one_of,
    },
    combinator::{
        consumed,
        fail,
        map,
        map_opt,
        not,
        opt,
        recognize,
        success,
        value,
    },
    multi::{
        many0,
        many1,
        many_m_n,
    },
    sequence::{
        delimited,
        preceded,
        separated_pair,
        terminated,
        tuple,
    }
};
use crate::ParsedResult;
use crate::context::Context;
use crate::parser;

#[derive(Debug, PartialEq)]
pub struct LexedResult<'input, O>(pub ParsedResult<'input, O>)
where
    O: PartialEq;

#[inline]
pub fn lex<'input, O>(lexer: impl FnMut(&'input str) -> LexedResult<'input, O>) -> impl FnMut(&'input str) -> ParsedResult<'input, O>
where
    O: PartialEq,
{
    preceded(ignore, unwrap_fn(lexer))
}

#[inline]
pub fn unwrap_fn<'input, O>(mut lexer: impl FnMut(&'input str) -> LexedResult<'input, O>) -> impl FnMut(&'input str) -> ParsedResult<'input, O>
where
    O: PartialEq,
{
    move |input: &'input str| lexer(input).0
}

fn ignore(input: &str) -> ParsedResult<()> {
    value((), many0(alt((line_comment, delimited_comment, whitespace1))))(input)
}

#[inline]
pub fn byte_order_mark(input: &str) -> ParsedResult<()> {
    value((), tag("\u{EF}\u{BB}\u{BF}"))(input)
}

#[inline]
pub fn whitespace0(input: &str) -> ParsedResult<()> {
    value((), multispace0)(input)
}

#[inline]
pub fn whitespace1(input: &str) -> ParsedResult<()> {
    value((), multispace1)(input)
}

#[inline]
pub fn newline(input: &str) -> ParsedResult<()> {
    value((), alt((tag("\r\n"), tag("\r"), tag("\n"))))(input)
}

#[inline]
pub fn line_comment(input: &str) -> ParsedResult<()> {
    value((), tuple((tag("//"), line_comment_char0, opt(newline))))(input)
}

#[inline]
fn line_comment_char0(input: &str) -> ParsedResult<()> {
    value((), alt((is_not("\r\n"), success(""))))(input)
}

pub fn delimited_comment(input: &str) -> ParsedResult<()> {
    value((), tuple((tag("{/"), delimited_comment_char0, tag("/}"))))(input)
}

fn delimited_comment_char0(input: &str) -> ParsedResult<()> {
    value(
        (),
        many0(
            alt((
                delimited_comment,
                value((), tuple((not(alt((tag("{/"), tag("/}")))), anychar))),
            )),
    ))(input)
}

#[inline]
pub fn keyword<'context: 'input, 'name: 'input, 'input>(
    context: &'context Context,
    name: &'name str,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Keyword> {
    move |input: &'input str| LexedResult(
        map_opt(terminated(tag(name), peek_code_delimit), |x| context.keyword.from_str(name, x))(input)
    )
}

#[inline]
fn is_not_keyword<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<()> {
    move |input: &'input str|
        if context.keyword.iter_keyword_str()
            .all(|x| not(tuple((tag::<&str, &str, VerboseError<&str>>(x), peek_code_delimit)))(input).is_ok())
        {
            success(())(input)
        }
        else {
            fail(input)
        }
}

#[inline]
pub fn op_code<'context: 'input, 'name: 'input, 'input>(
    context: &'context Context,
    name: &'name str,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::OpCode> {
    move |input: &'input str| LexedResult(
        preceded(
            is_not_op_code_substr(context, name),
            map_opt(tag(name), |x| context.op_code.from_str(name, x)),
        )(input),
    )
}

fn is_not_op_code_substr<'context: 'input, 'name: 'input, 'input>(
    context: &'context Context,
    name: &'name str,
) -> impl FnMut(&'input str) -> ParsedResult<()> {
    move |input: &'input str|
        if context.op_code.iter_op_code_str()
            .filter(|&x| x.len() > name.len())
            .all(|x| not(tag::<&str, &str, VerboseError<&str>>(x))(input).is_ok())
        {
            success(())(input)
        }
        else {
            fail(input)
        }
}

#[inline]
fn peek_code_delimit(input: &str) -> ParsedResult<()> {
    value((), not(ident_part_char))(input)
}

#[inline]
pub fn ident<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Ident> {
    move |input: &'input str| LexedResult(
        map(
            recognize(
                tuple((
                    is_not_keyword(context),
                    ident_start_char,
                    many0(ident_part_char),
                )),
            ),
            |x| ast::Ident(x),
        )(input)
    )
}

#[inline]
fn ident_start_char(input: &str) -> ParsedResult<&str> {
    recognize(
        one_of("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")
    )(input)
}

#[inline]
fn ident_part_char(input: &str) -> ParsedResult<&str> {
    alt((ident_start_char, recognize(one_of("_0123456789"))))(input)
}

#[inline]
pub fn unit_literal<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Literal> {
    move |input: &'input str| LexedResult(
        map(
            separated_pair(
                unwrap_fn(op_code(context, "(")),
                ignore,
                unwrap_fn(op_code(context, ")")),
            ),
            |x| ast::Literal::Unit(x.0, x.1),
        )(input)
    )
}

#[inline]
pub fn null_literal<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Literal> {
    move |input: &'input str| LexedResult(
        map(
            unwrap_fn(keyword(context, "null")),
            |x| ast::Literal::Null(x),
        )(input)
    )
}

#[inline]
pub fn bool_literal<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Literal> {
    move |input: &'input str| LexedResult(
        map(
            alt((unwrap_fn(keyword(context, "true")), unwrap_fn(keyword(context, "false")))),
            |x| ast::Literal::Bool(x),
        )(input)
    )
}

#[inline]
pub fn integer_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            consumed(
                tuple((
                    digit_char,
                    many0(tuple((many0(char('_')), digit_char))),
                    opt(integer_suffix),
                    peek_code_delimit,
                )),
            ),
            |x| x.1.2.map_or(
                ast::Literal::PureInteger(x.0),
                |_| ast::Literal::DecInteger(x.0),
            )
        )(input)
    )
}

#[inline]
pub fn hex_integer_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            recognize(
                tuple((
                    char('0'),
                    one_of("Xx"),
                    many1(tuple((many0(char('_')), hex_digit_char))),
                    opt(integer_suffix),
                    peek_code_delimit,
                )),
            ),
            |x| ast::Literal::HexInteger(x)
        )(input)
    )
}

#[inline]
pub fn bin_integer_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            recognize(
                tuple((
                    char('0'),
                    one_of("Bb"),
                    many1(tuple((many0(char('_')), bin_digit_char))),
                    opt(integer_suffix),
                    peek_code_delimit,
                )),
            ),
            |x| ast::Literal::BinInteger(x)
        )(input)
    )
}

#[inline]
pub fn real_number_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            alt((
                recognize(
                    tuple((
                        digit_char,
                        many0(tuple((many0(char('_')), digit_char))),
                        char('.'),
                        digit_char,
                        many0(tuple((many0(char('_')), digit_char))),
                        opt(exponent_part),
                        opt(real_number_suffix),
                        peek_code_delimit,
                    )),
                ),
                recognize(
                    tuple((
                        digit_char,
                        many0(tuple((many0(char('_')), digit_char))),
                        alt((
                            real_number_suffix,
                            recognize(
                                tuple((
                                    exponent_part,
                                    opt(real_number_suffix),
                                )),
                            ),
                        )),
                        peek_code_delimit,
                    )),
                )
            )),
            |x| ast::Literal::RealNumber(x),
        )(input)
    )
}

#[inline]
fn digit_char(input: &str) -> ParsedResult<&str> {
    recognize(
        one_of("0123456789"),
    )(input)
}

#[inline]
fn hex_digit_char(input: &str) -> ParsedResult<&str> {
    recognize(
        one_of("0123456789ABCDEFabcdef"),
    )(input)
}

#[inline]
fn bin_digit_char(input: &str) -> ParsedResult<&str> {
    recognize(
        one_of("01")
    )(input)
}

#[inline]
fn integer_suffix(input: &str) -> ParsedResult<&str> {
    recognize(
        alt((tuple((one_of("Ll"), opt(one_of("Uu")))), tuple((one_of("Uu"), opt(one_of("Ll")))))),
    )(input)
}

#[inline]
fn real_number_suffix(input: &str) -> ParsedResult<&str> {
    recognize(
        one_of("FfDdMm")
    )(input)
}

#[inline]
fn exponent_part(input: &str) -> ParsedResult<&str> {
    recognize(
        tuple((
            one_of("Ee"),
            opt(one_of("+-")),
            digit_char,
            many0(tuple((many0(char('_')), digit_char))),
        )),
    )(input)
}

#[inline]
pub fn character_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            delimited(
                char('\''),
                recognize(
                    alt((escape_sequence, character_char)),
                ),
                char('\''),
            ),
            |x| ast::Literal::Character(x),
        )(input)
    )
}

#[inline]
fn character_char(input: &str) -> ParsedResult<&str> {
    recognize(
        none_of("'\\\r\n")
    )(input)
}

#[inline]
pub fn regular_string_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            delimited(
                char('"'),
                recognize(
                    many0(
                        alt((escape_sequence, regular_string_char)),
                    ),
                ),
                char('"'),
            ),
            |x| ast::Literal::RegularString(x),
        )(input)
    )
}

#[inline]
fn regular_string_char(input: &str) -> ParsedResult<&str> {
    recognize(
        none_of("\"\\\r\n")
    )(input)
}

#[inline]
pub fn verbatium_string_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            delimited(
                tag("@\""),
                recognize(
                    many0(
                        alt((escape_sequence, verbatium_string_char)),
                    ),
                ),
                char('"'),
            ),
            |x| ast::Literal::VerbatiumString(x),
        )(input)
    )
}

#[inline]
fn verbatium_string_char(input: &str) -> ParsedResult<&str> {
    alt((
        recognize(none_of("\"")),
        recognize(tag("\"\"")),
    ))(input)
}

#[inline]
pub fn this_literal<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Literal> {
    move |input: &'input str| LexedResult(
        map(
            unwrap_fn(keyword(context, "this")),
            |x| ast::Literal::This(x),
        )(input)
    )
}

#[inline]
fn escape_sequence(input: &str) -> ParsedResult<&str> {
    alt((
        recognize(
            tuple((
                char('\\'),
                one_of("'\"\\0abfnrtv"),
            )),
        ),
        recognize(
            tuple((
                tag("\\x"),
                many_m_n(1, 4, hex_digit_char),
            )),
        ),
        recognize(
            tuple((
                tag("\\u"),
                many_m_n(4, 4, hex_digit_char),
            )),
        ),
        recognize(
            tuple((
                tag("\\U"),
                many_m_n(8, 8, hex_digit_char),
            )),
        ),
    ))
    (input)
}

pub fn interpolated_string<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::InterpolatedString> {
    |input: &'input str| LexedResult(
        map(
            delimited(
                tag("$\""),
                tuple((
                    interpolated_string_part,
                    many0(
                        tuple((
                            interpolated_string_inside_expr(context),
                            interpolated_string_part,
                        )),
                    ),
                )),
                char('"'),
            ),
            |x| {
                let (e, s): (Vec<parser::ast::Expr>, Vec<&str>) = x.1.into_iter().unzip();
                ast::InterpolatedString(
                    [x.0].into_iter().chain(s.into_iter()).collect(),
                    e,
                )
            },
        )(input)
    )
}

fn interpolated_string_char(input: &str) -> ParsedResult<&str> {
    recognize(
        none_of("{\"\\"),
    )(input)
}

fn interpolated_string_part(input: &str) -> ParsedResult<&str> {
    recognize(
        many0(
            alt((escape_sequence, interpolated_string_char)),
        ),
    )(input)
}

fn interpolated_string_inside_expr<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> ParsedResult<'input, parser::ast::Expr> {
    |input: &'input str| delimited(
        char('{'),
        parser::expr(context),
        char('}'),
    )(input)
}

pub fn eof(input: &str) -> LexedResult<()> {
    LexedResult(
        value((), nom::combinator::eof)(input)
    )
}
