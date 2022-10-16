
#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Control {
    As,
    Break,
    Continue,
    Else,
    Enum,
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
    Newtype,
    Pub,
    Ref,
    Return,
    Smooth,
    Struct,
    Sync,
    Type,
    Typeof,
    Use,
    While,
}

impl From<&str> for Control {
    fn from(x: &str) -> Self {
        match x {
            "as" => Self::As,
            "break" => Self::Break,
            "continue" => Self::Continue,
            "else" => Self::Else,
            "enum" => Self::Enum,
            "fn" => Self::Fn,
            "for" => Self::For,
            "if" => Self::If,
            "in" => Self::In,
            "is" => Self::Is,
            "let" => Self::Let,
            "linear" => Self::Linear,
            "loop" => Self::Loop,
            "match" => Self::Match,
            "mod" => Self::Mod,
            "mut" => Self::Mut,
            "newtype" => Self::Newtype,
            "pub" => Self::Pub,
            "ref" => Self::Ref,
            "return" => Self::Return,
            "smooth" => Self::Smooth,
            "struct" => Self::Struct,
            "sync" => Self::Sync,
            "type" => Self::Type,
            "typeof" => Self::Typeof,
            "use" => Self::Use,
            "while" => Self::While,
            _ => panic!(),
        }
    }
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Encloser {
    OpenBrace,
    CloseBrace,
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,
    OpenChevron,
    CloseChevron,
    Pipe,
}

impl From<&str> for Encloser {
    fn from(x: &str) -> Self {
        match x {
            "{" => Self::OpenBrace,
            "}" => Self::CloseBrace,
            "(" => Self::OpenParen,
            ")" => Self::CloseParen,
            "[" => Self::OpenBracket,
            "]" => Self::CloseBracket,
            "<" => Self::OpenChevron,
            ">" => Self::CloseChevron,
            "|" => Self::Pipe,
            _ => panic!(),
        }
    }
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Delimiter {
    Comma,
    Colon,
    Bind,
    Assign,
    Arrow,
}

impl From<&str> for Delimiter {
    fn from(x: &str) -> Self {
        match x {
            "," => Self::Comma,
            ":" => Self::Colon,
            "=" => Self::Bind,
            "<-" => Self::Arrow,
            "->" => Self::Arrow,
            _ => panic!(),
        }
    }
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum End {
    Semicolon,
}

impl From<&str> for End {
    fn from(x: &str) -> Self {
        match x {
            ";" => Self::Semicolon,
            _ => panic!(),
        }
    }
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum OpCode {
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

impl From<&str> for OpCode {
    fn from(x: &str) -> Self {
        match x {
            "." => Self::Dot,
            "+" => Self::Plus,
            "-" => Self::Minus,
            "*" => Self::Star,
            "/" => Self::Div,
            "%" => Self::Percent,
            "&" => Self::Amp,
            "|" => Self::Pipe,
            "^" => Self::Caret,
            "!" => Self::Bang,
            "~" => Self::Tilde,
            "<" => Self::Lt,
            ">" => Self::Gt,
            "_" => Self::Wildcard,
            "::" => Self::DoubleColon,
            "??" => Self::Coalescing,
            "?." => Self::CoalescingAccess,
            "&&" => Self::And,
            "||" => Self::Or,
            "==" => Self::Eq,
            "!=" => Self::Ne,
            "<=" => Self::Le,
            ">=" => Self::Ge,
            "<<" => Self::LeftShift,
            ">>" => Self::RightShift,
            "<|" => Self::LeftPipeline,
            "|>" => Self::RightPipeline,
            ".." => Self::Range,
            "..." => Self::Spread,
            _ => panic!(),
        }
    }
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Ident {
    pub name: String,
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Literal {
    Unit,
    Null,
    Bool(String),
    Integer(String),
    HexInteger(String),
    BinInteger(String),
    RealNumber(String),
    Character(String),
    RegularString(String),
    VerbatiumString(String),
    This,
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct InterpolatedString
{
    pub string_parts: Vec<String>,
}
