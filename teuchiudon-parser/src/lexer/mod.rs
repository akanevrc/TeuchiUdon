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
        fail,
        map,
        map_opt,
        not,
        opt,
        success,
        value,
    },
    multi::{
        fold_many0,
        fold_many1,
        fold_many_m_n,
        many0,
    },
    sequence::{
        delimited,
        preceded,
        tuple,
    },
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
    preceded(
        many0(alt((line_comment, delimited_comment, whitespace1))),
        unwrap_fn(lexer),
    )
}

#[inline]
pub fn unwrap_fn<'input, O>(mut lexer: impl FnMut(&'input str) -> LexedResult<'input, O>) -> impl FnMut(&'input str) -> ParsedResult<'input, O>
where
    O: PartialEq,
{
    move |input: &'input str| lexer(input).0
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
                value((), preceded(not(alt((tag("{/"), tag("/}")))), anychar)),
            )),
    ))(input)
}

#[inline]
pub fn keyword<'context: 'input, 'name: 'input, 'input>(
    context: &'context Context,
    name: &'name str,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Keyword> {
    move |input: &'input str| LexedResult(
        map_opt(tuple((tag(name), peek_code_delimit)), |_| context.keyword.from_str(name))(input)
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
            map_opt(tag(name), |_| context.op_code.from_str(name)),
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
            tuple((
                is_not_keyword(context),
                ident_start_char,
                fold_many0(
                    ident_part_char,
                    || String::new(),
                    |mut acc, x| { acc.push(x); acc }
                ),
            )),
            |x| ast::Ident(format!("{}{}", x.1, x.2)),
        )(input)
    )
}

#[inline]
fn ident_start_char(input: &str) -> ParsedResult<char> {
    one_of("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")(input)
}

#[inline]
fn ident_part_char(input: &str) -> ParsedResult<char> {
    alt((ident_start_char, one_of("_0123456789")))(input)
}

#[inline]
pub fn unit_literal<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Literal> {
    move |input: &'input str| LexedResult(
        value(
            ast::Literal::Unit,
            tuple((
                unwrap_fn(op_code(context, "(")),
                lex(op_code(context, ")")),
            )),
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
            tuple((
                digit_char,
                fold_many0(
                    tuple((many0(char('_')), digit_char)),
                    || String::new(),
                    |mut acc, x| { acc.push(x.1); acc },
                ),
                opt(integer_suffix),
                peek_code_delimit,
            )),
            |x| x.2.map_or(
                ast::Literal::PureInteger(format!("{}{}", x.0, x.1)),
                |y| ast::Literal::DecInteger(format!("{}{}{}", x.0, x.1, y)),
            )
        )(input)
    )
}

#[inline]
pub fn hex_integer_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            tuple((
                char('0'),
                one_of("Xx"),
                fold_many1(
                    tuple((many0(char('_')), hex_digit_char)),
                    || String::new(),
                    |mut acc, x| { acc.push(x.1); acc },
                ),
                opt(integer_suffix),
                peek_code_delimit,
            )),
            |x| ast::Literal::HexInteger(format!("{}{}", x.2, x.3.unwrap_or(String::new())))
        )(input)
    )
}

#[inline]
pub fn bin_integer_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            tuple((
                char('0'),
                one_of("Bb"),
                fold_many1(
                    tuple((many0(char('_')), bin_digit_char)),
                    || String::new(),
                    |mut acc, x| { acc.push(x.1); acc },
                ),
                opt(integer_suffix),
                peek_code_delimit,
            )),
            |x| ast::Literal::BinInteger(format!("{}{}", x.2, x.3.unwrap_or(String::new())))
        )(input)
    )
}

#[inline]
pub fn real_number_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        alt((
            map(
                tuple((
                    digit_char,
                    fold_many0(
                        tuple((many0(char('_')), digit_char)),
                        || String::new(),
                        |mut acc, x| { acc.push(x.1); acc }
                    ),
                    char('.'),
                    digit_char,
                    fold_many0(
                        tuple((many0(char('_')), digit_char)),
                        || String::new(),
                        |mut acc, x| { acc.push(x.1); acc }
                    ),
                    opt(exponent_part),
                    opt(real_number_suffix),
                    peek_code_delimit,
                )),
                |x| ast::Literal::RealNumber(
                    format!("{}{}{}{}{}{}{}", x.0, x.1, x.2, x.3, x.4, x.5.unwrap_or(String::new()), x.6.map_or(String::new(), |y| y.to_string())),
                ),
            ),
            map(
                tuple((
                    digit_char,
                    fold_many0(
                        tuple((many0(char('_')), digit_char)),
                        || String::new(),
                        |mut acc, x| { acc.push(x.1); acc }
                    ),
                    alt((
                        map(real_number_suffix, |x| x.to_string()),
                        map(
                            tuple((
                                exponent_part,
                                opt(real_number_suffix),
                            )),
                            |x| format!("{}{}", x.0, x.1.map_or(String::new(), |y| y.to_string()))
                        ),
                    )),
                    peek_code_delimit,
                )),
                |x| ast::Literal::RealNumber(
                    format!("{}{}{}", x.0, x.1, x.2),
                ),
            ),
        ))(input)
    )
}

#[inline]
fn digit_char(input: &str) -> ParsedResult<char> {
    one_of("0123456789")(input)
}

#[inline]
fn hex_digit_char(input: &str) -> ParsedResult<char> {
    one_of("0123456789ABCDEFabcdef")(input)
}

#[inline]
fn bin_digit_char(input: &str) -> ParsedResult<char> {
    one_of("01")(input)
}

#[inline]
fn integer_suffix(input: &str) -> ParsedResult<String> {
    map(
        alt((tuple((one_of("Ll"), opt(one_of("Uu")))), tuple((one_of("Uu"), opt(one_of("Ll")))))),
        |x| format!("{}{}", x.0, x.1.map_or(String::new(), |y| y.to_string()))
    )(input)
}

#[inline]
fn real_number_suffix(input: &str) -> ParsedResult<char> {
    one_of("FfDdMm")(input)
}

#[inline]
fn exponent_part(input: &str) -> ParsedResult<String> {
    map(
        tuple((
            one_of("Ee"),
            opt(one_of("+-")),
            digit_char,
            fold_many0(
                tuple((many0(char('_')), digit_char)),
                || String::new(),
                |mut acc, x| { acc.push(x.1); acc }
            ),
        )),
        |x| format!("{}{}{}{}", x.0, x.1.map_or(String::new(), |y| y.to_string()), x.2, x.3)
    )
    (input)
}

#[inline]
pub fn character_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        delimited(
            char('\''),
            map(
                alt((escape_sequence, map(character_char, |x| x.to_string()))),
                |x| ast::Literal::Character(x),
            ),
            char('\''),
        )(input)
    )
}

#[inline]
fn character_char(input: &str) -> ParsedResult<char> {
    none_of("'\\\r\n")(input)
}

#[inline]
pub fn regular_string_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        delimited(
            char('"'),
            map(
                many0(
                    alt((escape_sequence, map(regular_string_char, |x| x.to_string()))),
                ),
                |x| ast::Literal::RegularString(x.concat()),
            ),
            char('"'),
        )(input)
    )
}

#[inline]
fn regular_string_char(input: &str) -> ParsedResult<char> {
    none_of("\"\\\r\n")(input)
}

#[inline]
pub fn verbatium_string_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        delimited(
            tag("@\""),
            map(
                many0(
                    alt((escape_sequence, verbatium_string_char)),
                ),
                |x| ast::Literal::VerbatiumString(x.concat()),
            ),
            char('"'),
        )(input)
    )
}

#[inline]
fn verbatium_string_char(input: &str) -> ParsedResult<String> {
    alt((
        map(none_of("\""), |x| x.to_string()),
        map(tag("\"\""), |x: &str| x.to_owned()),
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
fn escape_sequence(input: &str) -> ParsedResult<String> {
    alt((
        map(
            tuple((
                char('\\'),
                one_of("'\"\\0abfnrtv"),
            )),
            |x| format!("{}{}", x.0, x.1),
        ),
        map(
            tuple((
                tag("\\x"),
                fold_many_m_n(1, 4, hex_digit_char, || String::new(), |mut acc, x| { acc.push(x); acc }),
            )),
            |x| format!("{}{}", x.0, x.1),
        ),
        map(
            tuple((
                tag("\\u"),
                fold_many_m_n(4, 4, hex_digit_char, || String::new(), |mut acc, x| { acc.push(x); acc }),
            )),
            |x| format!("{}{}", x.0, x.1),
        ),
        map(
            tuple((
                tag("\\U"),
                fold_many_m_n(8, 8, hex_digit_char, || String::new(), |mut acc, x| { acc.push(x); acc }),
            )),
            |x| format!("{}{}", x.0, x.1),
        ),
    ))
    (input)
}

pub fn interpolated_string<'context: 'input, 'input>(
    context: &'context Context,
) -> impl FnMut(&'input str) -> LexedResult<'input, ast::InterpolatedString> {
    |input: &'input str| LexedResult(
        delimited(
            tag("$\""),
            map(
                tuple((
                    interpolated_string_part,
                    many0(
                        tuple((
                            interpolated_string_inside_expr(context),
                            interpolated_string_part,
                        )),
                    ),
                )),
                |x| {
                    let (e, s): (Vec<parser::ast::Expr>, Vec<String>) = x.1.into_iter().unzip();
                    ast::InterpolatedString(
                        [x.0].into_iter().chain(s.into_iter()).collect(),
                        e,
                    )
                },
            ),
            char('"'),
        )(input)
    )
}

fn interpolated_string_char(input: &str) -> ParsedResult<char> {
    none_of("{\"\\")(input)
}

fn interpolated_string_part(input: &str) -> ParsedResult<String> {
    map(
        many0(
            alt((escape_sequence, map(interpolated_string_char, |x| x.to_string()))),
        ),
        |x| x.concat(),
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
