pub mod ast;

use std::{
    error::Error,
    rc::Rc,
};
use function_name::named;
use nom::{
    Parser,
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
use nom_supreme::{
    error::GenericErrorTree,
    ParserExt,
};
use crate::ParsedResult;
use crate::context::Context;
use crate::parser;

#[derive(Debug)]
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

#[inline]
fn ignore(input: &str) -> ParsedResult<()> {
    value((), many0(alt((line_comment, delimited_comment, whitespace1))))(input)
}

#[named]
#[inline]
pub fn byte_order_mark(input: &str) -> LexedResult<()> {
    LexedResult(
        value((), tag("\u{EF}\u{BB}\u{BF}"))
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[named]
#[inline]
pub fn whitespace0(input: &str) -> ParsedResult<()> {
    value((), multispace0)
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
#[inline]
pub fn whitespace1(input: &str) -> ParsedResult<()> {
    value((), multispace1)
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
#[inline]
pub fn newline(input: &str) -> ParsedResult<()> {
    value((), alt((tag("\r\n"), tag("\r"), tag("\n"))))
    .context(function_name!().to_owned())
    .parse(input)
}

#[named]
#[inline]
pub fn line_comment(input: &str) -> ParsedResult<()> {
    value((), tuple((tag("//"), line_comment_char0, opt(newline))))
    .context(function_name!().to_owned())
    .parse(input)
}

#[inline]
fn line_comment_char0(input: &str) -> ParsedResult<()> {
    value((), alt((is_not("\r\n"), success(""))))(input)
}

#[named]
#[inline]
pub fn delimited_comment(input: &str) -> ParsedResult<()> {
    value((), tuple((tag("{/"), delimited_comment_char0, tag("/}"))))
    .context(function_name!().to_owned())
    .parse(input)
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

#[named]
#[inline]
pub fn keyword<'input: 'context, 'context>(
    context: &'context Context<'input>,
    name: &'static str,
) -> impl FnMut(&'input str) -> LexedResult<'input, Rc<ast::Keyword<'input>>> + 'context {
    move |input: &'input str| LexedResult(
        map_opt(terminated(tag(name), peek_code_delimit), |x| context.keyword.from_str(name, x))
        .context(format!("{}: {}", function_name!(), name))
        .parse(input)
    )
}

#[inline]
fn is_not_keyword<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<()> + 'context {
    move |input: &'input str|
        if context.keyword.iter_keyword_str()
            .all(|x| not(tuple((tag::<&str, &str, GenericErrorTree<&'input str, &'static str, String, Box<dyn Error + Send + Sync + 'static>>>(x), peek_code_delimit)))(input).is_ok())
        {
            success(())(input)
        }
        else {
            fail(input)
        }
}

#[named]
#[inline]
pub fn op_code<'input: 'context, 'context>(
    context: &'context Context<'input>,
    name: &'static str,
) -> impl FnMut(&'input str) -> LexedResult<'input, Rc<ast::OpCode<'input>>> + 'context {
    move |input: &'input str| LexedResult(
        preceded(
            is_not_op_code_substr(context, name),
            map_opt(tag(name), |x| context.op_code.from_str(name, x)),
        )
        .context(format!("{}: {}", function_name!(), name))
        .parse(input)
    )
}

fn is_not_op_code_substr<'input: 'context, 'context>(
    context: &'context Context<'input>,
    name: &'static str,
) -> impl FnMut(&'input str) -> ParsedResult<()> + 'context {
    move |input: &'input str|
        if context.op_code.iter_op_code_str()
            .filter(|&x| x.len() > name.len())
            .all(|x| not(tag::<&str, &str, GenericErrorTree<&'input str, &'static str, String, Box<dyn Error + Send + Sync + 'static>>>(x))(input).is_ok())
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

#[named]
#[inline]
pub fn ident<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> LexedResult<'input, Rc<ast::Ident<'input>>> + 'context {
    move |input: &'input str| LexedResult(
        map(
            recognize(
                tuple((
                    is_not_keyword(context),
                    ident_start_char,
                    many0(ident_part_char),
                )),
            ),
            |x| Rc::new(ast::Ident { slice: x }),
        )
        .context(function_name!().to_owned())
        .parse(input)
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

#[named]
#[inline]
pub fn unit_literal<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> LexedResult<'input, Rc<ast::Literal<'input>>> + 'context {
    move |input: &'input str| LexedResult(
        map(
            consumed(
                separated_pair(
                    unwrap_fn(op_code(context, "(")),
                    ignore,
                    unwrap_fn(op_code(context, ")")),
                ),
            ),
            |x| Rc::new(ast::Literal { slice: x.0, kind: Rc::new(ast::LiteralKind::Unit { left: x.1.0, right: x.1.1 }) }),
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[named]
#[inline]
pub fn null_literal<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> LexedResult<'input, Rc<ast::Literal<'input>>> + 'context {
    move |input: &'input str| LexedResult(
        map(
            unwrap_fn(keyword(context, "null")),
            |x| Rc::new(ast::Literal { slice: x.slice, kind: Rc::new(ast::LiteralKind::Null { keyword: x }) }),
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[named]
#[inline]
pub fn bool_literal<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> LexedResult<'input, Rc<ast::Literal<'input>>> + 'context {
    move |input: &'input str| LexedResult(
        map(
            alt((unwrap_fn(keyword(context, "true")), unwrap_fn(keyword(context, "false")))),
            |x| Rc::new(ast::Literal { slice: x.slice, kind: Rc::new(ast::LiteralKind::Bool { keyword: x }) }),
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[named]
#[inline]
pub fn integer_literal<'input>(input: &'input str) -> LexedResult<Rc<ast::Literal<'input>>> {
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
                Rc::new(ast::Literal { slice: x.0, kind: Rc::new(ast::LiteralKind::PureInteger { slice: x.0 }) }),
                |_| Rc::new(ast::Literal { slice: x.0, kind: Rc::new(ast::LiteralKind::DecInteger { slice: x.0 }) }),
            )
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[named]
#[inline]
pub fn hex_integer_literal<'input>(input: &'input str) -> LexedResult<Rc<ast::Literal<'input>>> {
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
            |x| Rc::new(ast::Literal { slice: x, kind: Rc::new(ast::LiteralKind::HexInteger { slice: x }) })
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[named]
#[inline]
pub fn bin_integer_literal<'input>(input: &'input str) -> LexedResult<Rc<ast::Literal<'input>>> {
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
            |x| Rc::new(ast::Literal { slice: x, kind: Rc::new(ast::LiteralKind::BinInteger { slice: x }) })
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[named]
#[inline]
pub fn real_number_literal<'input>(input: &'input str) -> LexedResult<Rc<ast::Literal<'input>>> {
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
            |x| Rc::new(ast::Literal { slice: x, kind: Rc::new(ast::LiteralKind::RealNumber { slice: x }) }),
        )
        .context(function_name!().to_owned())
        .parse(input)
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

#[named]
#[inline]
pub fn character_literal<'input>(input: &'input str) -> LexedResult<Rc<ast::Literal<'input>>> {
    LexedResult(
        map(
            delimited(
                char('\''),
                recognize(
                    alt((escape_sequence, character_char)),
                ),
                char('\''),
            ),
            |x| Rc::new(ast::Literal { slice: x, kind: Rc::new(ast::LiteralKind::Character { slice: x }) }),
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[inline]
fn character_char(input: &str) -> ParsedResult<&str> {
    recognize(
        none_of("'\\\r\n")
    )(input)
}

#[named]
#[inline]
pub fn regular_string_literal<'input>(input: &'input str) -> LexedResult<Rc<ast::Literal<'input>>> {
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
            |x| Rc::new(ast::Literal { slice: x, kind: Rc::new(ast::LiteralKind::RegularString { slice: x }) }),
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[inline]
fn regular_string_char(input: &str) -> ParsedResult<&str> {
    recognize(
        none_of("\"\\\r\n")
    )(input)
}

#[named]
#[inline]
pub fn verbatium_string_literal<'input>(input: &'input str) -> LexedResult<Rc<ast::Literal<'input>>> {
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
            |x| Rc::new(ast::Literal { slice: x, kind: Rc::new(ast::LiteralKind::VerbatiumString { slice: x }) }),
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[inline]
fn verbatium_string_char(input: &str) -> ParsedResult<&str> {
    alt((
        recognize(none_of("\"")),
        recognize(tag("\"\"")),
    ))(input)
}

#[named]
#[inline]
pub fn this_literal<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> LexedResult<'input, Rc<ast::Literal<'input>>> + 'context {
    move |input: &'input str| LexedResult(
        map(
            unwrap_fn(keyword(context, "this")),
            |x| Rc::new(ast::Literal { slice: x.slice, kind: Rc::new(ast::LiteralKind::This { keyword: x }) }),
        )
        .context(function_name!().to_owned())
        .parse(input)
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

#[named]
#[inline]
pub fn interpolated_string<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> LexedResult<'input, Rc<ast::InterpolatedString<'input>>> + 'context {
    |input: &'input str| LexedResult(
        map(
            delimited(
                tag("$\""),
                consumed(
                    tuple((
                        interpolated_string_part,
                        many0(
                            tuple((
                                interpolated_string_inside_expr(context),
                                interpolated_string_part,
                            )),
                        ),
                    )),
                ),
                char('"'),
            ),
            |x| {
                let (e, s): (Vec<Rc<parser::ast::Expr>>, Vec<&str>) = x.1.1.into_iter().unzip();
                Rc::new(ast::InterpolatedString {
                    slice: x.0,
                    string_parts: [x.1.0].into_iter().chain(s.into_iter()).collect(),
                    exprs: e,
                })
            },
        )
        .context(function_name!().to_owned())
        .parse(input)
    )
}

#[inline]
fn interpolated_string_char(input: &str) -> ParsedResult<&str> {
    recognize(
        none_of("{\"\\"),
    )(input)
}

#[inline]
fn interpolated_string_part(input: &str) -> ParsedResult<&str> {
    recognize(
        many0(
            alt((escape_sequence, interpolated_string_char)),
        ),
    )(input)
}

#[inline]
fn interpolated_string_inside_expr<'input: 'context, 'context>(
    context: &'context Context<'input>,
) -> impl FnMut(&'input str) -> ParsedResult<'input, Rc<parser::ast::Expr<'input>>> + 'context {
    |input: &'input str| delimited(
        char('{'),
        parser::expr(context),
        char('}'),
    )(input)
}

#[named]
#[inline]
pub fn eof(input: &str) -> LexedResult<()> {
    LexedResult(
        value((), nom::combinator::eof)
        .context(function_name!().to_owned())
        .parse(input)
    )
}
