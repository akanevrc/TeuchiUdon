use std::rc::Rc;
use crate::parser;

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum Keyword<'input> {
    As(&'input str),
    Break(&'input str),
    Continue(&'input str),
    Else(&'input str),
    Enum(&'input str),
    False(&'input str),
    Fn(&'input str),
    For(&'input str),
    If(&'input str),
    In(&'input str),
    Is(&'input str),
    Let(&'input str),
    Linear(&'input str),
    Loop(&'input str),
    Match(&'input str),
    Mod(&'input str),
    Mut(&'input str),
    Null(&'input str),
    Newtype(&'input str),
    Pub(&'input str),
    Ref(&'input str),
    Return(&'input str),
    Smooth(&'input str),
    Struct(&'input str),
    Sync(&'input str),
    This(&'input str),
    True(&'input str),
    Type(&'input str),
    Use(&'input str),
    While(&'input str),
}

#[derive(Clone, Copy, Debug, PartialEq)]
pub enum OpCode<'input> {
    OpenBrace(&'input str),
    CloseBrace(&'input str),
    OpenParen(&'input str),
    CloseParen(&'input str),
    OpenBracket(&'input str),
    CloseBracket(&'input str),
    Comma(&'input str),
    Colon(&'input str),
    Bind(&'input str),
    Semicolon(&'input str),
    Dot(&'input str),
    Plus(&'input str),
    Minus(&'input str),
    Star(&'input str),
    Div(&'input str),
    Percent(&'input str),
    Amp(&'input str),
    Pipe(&'input str),
    Caret(&'input str),
    Bang(&'input str),
    Tilde(&'input str),
    Lt(&'input str),
    Gt(&'input str),
    Wildcard(&'input str),
    Iter(&'input str),
    Arrow(&'input str),
    DoubleColon(&'input str),
    Coalescing(&'input str),
    CoalescingAccess(&'input str),
    And(&'input str),
    Or(&'input str),
    Eq(&'input str),
    Ne(&'input str),
    Le(&'input str),
    Ge(&'input str),
    LeftShift(&'input str),
    RightShift(&'input str),
    LeftPipeline(&'input str),
    RightPipeline(&'input str),
    Range(&'input str),
    Spread(&'input str),
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
