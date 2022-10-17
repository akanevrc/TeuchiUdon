pub mod ast;

use nom::{
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
        map,
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
pub fn control<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Control> {
    move |input: &'input str| LexedResult(
        value(ast::Control::from(name), tuple((tag(name), peek_code_delimit)))(input)
    )
}

#[inline]
pub fn encloser<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Encloser> {
    move |input: &'input str| LexedResult(
        value(ast::Encloser::from(name), tag(name))(input)
    )
}

#[inline]
pub fn delimiter<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> LexedResult<'input, ast::Delimiter> {
    move |input: &'input str| LexedResult(
        value(ast::Delimiter::from(name), tag(name))(input)
    )
}

#[inline]
pub fn end<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> LexedResult<'input, ast::End> {
    move |input: &'input str| LexedResult(
        value(ast::End::from(name), tag(name))(input)
    )
}

#[inline]
pub fn op_code<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> LexedResult<'input, ast::OpCode> {
    move |input: &'input str| LexedResult(
        value(ast::OpCode::from(name), tag(name))(input)
    )
}

#[inline]
fn peek_code_delimit(input: &str) -> ParsedResult<()> {
    value((), not(ident_part_char))(input)
}

#[inline]
pub fn ident(input: &str) -> LexedResult<ast::Ident> {
    LexedResult(
        map(
            tuple((
                ident_start_char,
                fold_many0(
                    ident_part_char,
                    || String::new(),
                    |mut acc, x| { acc.push(x); acc }
                ),
            )),
            |x| ast::Ident { name: format!("{}{}", x.0, x.1) },
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
pub fn unit_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        value(
            ast::Literal::Unit,
            tuple((
                unwrap_fn(encloser("(")),
                lex(encloser(")")),
            )),
        )(input)
    )
}

#[inline]
pub fn null_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            unwrap_fn(control("null")),
            |x| ast::Literal::Null(x),
        )(input)
    )
}

#[inline]
pub fn bool_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            alt((unwrap_fn(control("true")), unwrap_fn(control("false")))),
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
            |x| ast::Literal::Integer(format!("{}{}{}", x.0, x.1, x.2.unwrap_or(String::new())))
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
pub fn this_literal(input: &str) -> LexedResult<ast::Literal> {
    LexedResult(
        map(
            unwrap_fn(control("this")),
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

pub fn interpolated_string(input: &str) -> LexedResult<ast::InterpolatedString> {
    LexedResult(
        delimited(
            tag("$\""),
            map(
                tuple((
                    interpolated_string_part,
                    many0(
                        tuple((
                            interpolated_string_inside_expr,
                            interpolated_string_part,
                        )),
                    ),
                )),
                |x| ast::InterpolatedString {
                    string_parts: [x.0].into_iter().chain(x.1.into_iter().map(|y| y.1)).collect()
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

fn interpolated_string_inside_expr(input: &str) -> ParsedResult<()> {
    delimited(
        char('{'),
        value((), tag("expr")),
        char('}'),
    )(input)
}

pub fn eof(input: &str) -> LexedResult<()> {
    LexedResult(
        value((), nom::combinator::eof)(input)
    )
}
