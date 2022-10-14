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
        map,
        not,
        success,
    },
    multi::{
        fold_many0,
        many_m_n,
    },
    sequence::{tuple, preceded},
};
use super::ParsedResult;

#[inline]
pub fn byte_order_mark(input: &str) -> ParsedResult {
    map(tag("\u{EF}\u{BB}\u{BF}"), |_| "")(input)
}

#[inline]
pub fn whitespace0(input: &str) -> ParsedResult {
    map(multispace0, |_| "")(input)
}

#[inline]
pub fn newline(input: &str) -> ParsedResult {
    alt((tag("\r\n"), tag("\r"), tag("\n")))(input)
}

#[inline]
pub fn line_comment(input: &str) -> ParsedResult {
    map(tuple((tag("//"), line_comment_char0, many_m_n(0, 1, newline))), |_| "")(input)
}

#[inline]
pub fn line_comment_char0(input: &str) -> ParsedResult {
    map(alt((is_not("\r\n"), success(""))), |_| "")(input)
}

pub fn delimited_comment(input: &str) -> ParsedResult {
    map(tuple((tag("{/"), delimited_comment_char0, tag("/}"))), |_| "")(input)
}

pub fn delimited_comment_char0(input: &str) -> ParsedResult {
    fold_many0(
        alt((delimited_comment, map(preceded(not(alt((tag("{/"), tag("/}")))), anychar), |_| ""))),
        || "",
        |_, _| ""
    )(input)
}
