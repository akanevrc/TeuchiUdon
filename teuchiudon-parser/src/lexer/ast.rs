use std::rc::Rc;
use crate::parser;

#[derive(Clone, Debug, PartialEq)]
pub struct Keyword<'input> {
    pub slice: &'input str,
    pub kind: KeywordKind,
}

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
    Newty,
    Pub,
    Ref,
    Return,
    Smooth,
    Struct,
    Sync,
    This,
    True,
    Ty,
    Use,
    While,
}

#[derive(Clone, Debug, PartialEq)]
pub struct OpCode<'input> {
    pub slice: &'input str,
    pub kind: OpCodeKind,
}

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
pub struct Ident<'input> {
    pub slice: &'input str,
}

#[derive(Clone, Debug, PartialEq)]
pub struct Literal<'input> {
    pub slice: &'input str,
    pub kind: Rc<LiteralKind<'input>>,
}

#[derive(Clone, Debug, PartialEq)]
pub enum LiteralKind<'input> {
    Unit {
        left: Rc<OpCode<'input>>,
        right: Rc<OpCode<'input>>,
    },
    Null {
        keyword: Rc<Keyword<'input>>,
    },
    Bool {
        keyword: Rc<Keyword<'input>>,
    },
    PureInteger {
        slice: &'input str,
    },
    DecInteger {
        slice: &'input str,
    },
    HexInteger {
        slice: &'input str,
    },
    BinInteger {
        slice: &'input str,
    },
    RealNumber {
        slice: &'input str,
    },
    Character {
        slice: &'input str,
    },
    RegularString {
        slice: &'input str,
    },
    VerbatiumString {
        slice: &'input str,
    },
    This {
        keyword: Rc<Keyword<'input>>,
    },
}

#[derive(Clone, Debug, PartialEq)]
pub struct InterpolatedString<'input> {
    pub slice: &'input str,
    pub string_parts: Vec<&'input str>,
    pub exprs: Vec<Rc<parser::ast::Expr<'input>>>,
}
