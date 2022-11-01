use std::rc::Rc;
use crate::parser;

#[derive(Clone, Debug, PartialEq)]
pub struct Keyword<'input>(pub &'input str, pub KeywordKind);

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum KeywordKind {
    As,
    Break,
    Continue,
    Else,
    Enum,
    False,
    Fn,
    For,
    If,
    In,
    Is,
    Let,
    Linear,
    Loop,
    Match,
    Mod,
    Mut,
    Null,
    Newtype,
    Pub,
    Ref,
    Return,
    Smooth,
    Struct,
    Sync,
    This,
    True,
    Type,
    Use,
    While,
}

#[derive(Clone, Debug, PartialEq)]
pub struct OpCode<'input>(pub &'input str, pub OpCodeKind);

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum OpCodeKind {
    OpenBrace,
    CloseBrace,
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,
    Comma,
    Colon,
    Bind,
    Semicolon,
    Dot,
    Plus,
    Minus,
    Star,
    Div,
    Percent,
    Amp,
    Pipe,
    Caret,
    Bang,
    Tilde,
    Lt,
    Gt,
    Wildcard,
    Iter,
    Arrow,
    DoubleColon,
    Coalescing,
    CoalescingAccess,
    And,
    Or,
    Eq,
    Ne,
    Le,
    Ge,
    LeftShift,
    RightShift,
    LeftPipeline,
    RightPipeline,
    Range,
    Spread,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Ident<'input>(pub &'input str);

#[derive(Clone, Debug, PartialEq)]
pub enum Literal<'input> {
    Unit(OpCode<'input>, OpCode<'input>),
    Null(Keyword<'input>),
    Bool(Keyword<'input>),
    PureInteger(&'input str),
    DecInteger(&'input str),
    HexInteger(&'input str),
    BinInteger(&'input str),
    RealNumber(&'input str),
    Character(&'input str),
    RegularString(&'input str),
    VerbatiumString(&'input str),
    This(Keyword<'input>),
}

#[derive(Clone, Debug, PartialEq)]
pub struct InterpolatedString<'input>(pub Vec<&'input str>, pub Vec<Rc<parser::ast::Expr<'input>>>);
