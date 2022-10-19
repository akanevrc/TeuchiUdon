use crate::parser;

#[derive(Clone, Copy, Debug, Eq, PartialEq)]
pub enum Keyword {
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

#[derive(Clone, Copy, Debug, Eq, PartialEq)]
pub enum OpCode {
    OpenBrace,
    CloseBrace,
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,
    OpenChevron,
    CloseChevron,
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

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Ident(pub String);

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Literal {
    Unit,
    Null(Keyword),
    Bool(Keyword),
    Integer(String),
    HexInteger(String),
    BinInteger(String),
    RealNumber(String),
    Character(String),
    RegularString(String),
    VerbatiumString(String),
    This(Keyword),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct InterpolatedString
{
    pub string_parts: Vec<String>,
    pub exprs: Vec<parser::ast::Expr>,
}
