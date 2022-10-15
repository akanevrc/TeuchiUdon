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
        many0,
    },
    sequence::{
        tuple,
        preceded,
    },
};
use crate::ParsedResult;

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
pub fn line_comment_char0(input: &str) -> ParsedResult<()> {
    value((), alt((is_not("\r\n"), success(""))))(input)
}

pub fn delimited_comment(input: &str) -> ParsedResult<()> {
    value((), tuple((tag("{/"), delimited_comment_char0, tag("/}"))))(input)
}

pub fn delimited_comment_char0(input: &str) -> ParsedResult<()> {
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
pub fn control<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Control> {
    move |input: &'input str| value(ast::Control::from(name), tuple((tag(name), peek_code_delimit)))(input)
}

#[inline]
pub fn encloser<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Encloser> {
    move |input: &'input str| value(ast::Encloser::from(name), tag(name))(input)
}

#[inline]
pub fn delimiter<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::Delimiter> {
    move |input: &'input str| value(ast::Delimiter::from(name), tag(name))(input)
}

#[inline]
pub fn end<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::End> {
    move |input: &'input str| value(ast::End::from(name), tag(name))(input)
}

#[inline]
pub fn op_code<'name: 'input, 'input>(name: &'name str) -> impl FnMut(&'input str) -> ParsedResult<'input, ast::OpCode> {
    move |input: &'input str| value(ast::OpCode::from(name), tag(name))(input)
}

#[inline]
pub fn peek_code_delimit(input: &str) -> ParsedResult<()> {
    value((), not(ident_part_char))(input)
}

#[inline]
pub fn ident(input: &str) -> ParsedResult<ast::Ident> {
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
}

#[inline]
pub fn ident_start_char(input: &str) -> ParsedResult<char> {
    one_of("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")(input)
}

#[inline]
pub fn ident_part_char(input: &str) -> ParsedResult<char> {
    alt((ident_start_char, one_of("_0123456789")))(input)
}

#[inline]
pub fn unit_literal(input: &str) -> ParsedResult<ast::Literal> {
    value(ast::Literal::Unit, tuple((encloser("("), whitespace0, encloser(")"))))(input)
}

#[inline]
pub fn null_literal(input: &str) -> ParsedResult<ast::Literal> {
    value(ast::Literal::Null, tuple((tag("null"), peek_code_delimit)))(input)
}

#[inline]
pub fn bool_literal(input: &str) -> ParsedResult<ast::Literal> {
    map(tuple((alt((tag("true"), tag("false"))), peek_code_delimit)), |x| ast::Literal::Bool(x.0.to_owned()))(input)
}

#[inline]
pub fn integer_literal(input: &str) -> ParsedResult<ast::Literal> {
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
}

#[inline]
pub fn hex_integer_literal(input: &str) -> ParsedResult<ast::Literal> {
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
}

#[inline]
pub fn bin_integer_literal(input: &str) -> ParsedResult<ast::Literal> {
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
}

#[inline]
pub fn real_number_literal(input: &str) -> ParsedResult<ast::Literal> {
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
}

#[inline]
pub fn digit_char(input: &str) -> ParsedResult<char> {
    one_of("0123456789")(input)
}

#[inline]
pub fn hex_digit_char(input: &str) -> ParsedResult<char> {
    one_of("0123456789ABCDEFabcdef")(input)
}

#[inline]
pub fn bin_digit_char(input: &str) -> ParsedResult<char> {
    one_of("01")(input)
}

#[inline]
pub fn integer_suffix(input: &str) -> ParsedResult<String> {
    map(
        alt((tuple((one_of("Ll"), opt(one_of("Uu")))), tuple((one_of("Uu"), opt(one_of("Ll")))))),
        |x| format!("{}{}", x.0, x.1.map_or(String::new(), |y| y.to_string()))
    )(input)
}

#[inline]
pub fn real_number_suffix(input: &str) -> ParsedResult<char> {
    one_of("FfDdMm")(input)
}

#[inline]
pub fn exponent_part(input: &str) -> ParsedResult<String> {
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
