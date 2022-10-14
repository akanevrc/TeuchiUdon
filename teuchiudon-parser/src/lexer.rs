use nom::{
    branch::alt,
    bytes::complete::{
        is_not,
        tag,
    },
    character::complete::{
        anychar,
        multispace0,
    },
    combinator::{
        not,
        success,
        value,
    },
    multi::{
        many0,
        many_m_n,
    },
    sequence::{tuple, preceded},
};
use super::ParsedResult;

#[inline]
pub fn byte_order_mark(input: &str) -> ParsedResult<()> {
    value((), tag("\u{EF}\u{BB}\u{BF}"))(input)
}

#[inline]
pub fn whitespace0(input: &str) -> ParsedResult<()> {
    value((), multispace0)(input)
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
