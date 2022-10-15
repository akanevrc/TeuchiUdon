pub mod ast;

use nom::{
    branch::alt,
    bytes::complete::{
        is_not,
        tag,
    },
    character::complete::{
        anychar,
        multispace0,
        multispace1,
        one_of,
    },
    combinator::{
        map,
        not,
        success,
        value,
    },
    multi::{
        fold_many0,
        many0,
        many_m_n,
    },
    sequence::{tuple, preceded},
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
    value((), tuple((tag("//"), line_comment_char0, many_m_n(0, 1, newline))))(input)
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
        many0(alt((delimited_comment, value((), preceded(not(alt((tag("{/"), tag("/}")))), anychar)))),
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
        tuple((ident_start_char, fold_many0(ident_part_char, || String::new(), |mut acc, x| { acc.push(x); acc }))),
        |x| ast::Ident { name: format!("{}{}", x.0, x.1) }
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
