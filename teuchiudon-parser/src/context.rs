use super::lexer::ast::{
    Keyword,
    OpCode,
};

pub struct Context {
    pub keyword: KeywordContext,
    pub op_code: OpCodeContext,
}

impl Context {
    pub fn new() -> Self {
        Self {
            keyword: KeywordContext::new(),
            op_code: OpCodeContext::new(),
        }
    }
}

pub struct KeywordContext {
    keywords: Vec<(&'static str, Box<dyn Fn(&str) -> Keyword>)>,
}

impl KeywordContext {
    fn new<'str>() -> Self {
        Self {
            keywords: vec![
                ("as", Box::new(|input: &str| Keyword::As(input))),
                ("break", Box::new(|input: &str| Keyword::Break(input))),
                ("continue", Box::new(|input: &str| Keyword::Continue(input))),
                ("else", Box::new(|input: &str| Keyword::Else(input))),
                ("enum", Box::new(|input: &str| Keyword::Enum(input))),
                ("false", Box::new(|input: &str| Keyword::False(input))),
                ("fn", Box::new(|input: &str| Keyword::Fn(input))),
                ("for", Box::new(|input: &str| Keyword::For(input))),
                ("if", Box::new(|input: &str| Keyword::If(input))),
                ("in", Box::new(|input: &str| Keyword::In(input))),
                ("is", Box::new(|input: &str| Keyword::Is(input))),
                ("let", Box::new(|input: &str| Keyword::Let(input))),
                ("linear", Box::new(|input: &str| Keyword::Linear(input))),
                ("loop", Box::new(|input: &str| Keyword::Loop(input))),
                ("match", Box::new(|input: &str| Keyword::Match(input))),
                ("mod", Box::new(|input: &str| Keyword::Mod(input))),
                ("mut", Box::new(|input: &str| Keyword::Mut(input))),
                ("newtype", Box::new(|input: &str| Keyword::Newtype(input))),
                ("null", Box::new(|input: &str| Keyword::Null(input))),
                ("pub", Box::new(|input: &str| Keyword::Pub(input))),
                ("ref", Box::new(|input: &str| Keyword::Ref(input))),
                ("return", Box::new(|input: &str| Keyword::Return(input))),
                ("smooth", Box::new(|input: &str| Keyword::Smooth(input))),
                ("struct", Box::new(|input: &str| Keyword::Struct(input))),
                ("sync", Box::new(|input: &str| Keyword::Sync(input))),
                ("this", Box::new(|input: &str| Keyword::This(input))),
                ("true", Box::new(|input: &str| Keyword::True(input))),
                ("type", Box::new(|input: &str| Keyword::Type(input))),
                ("use", Box::new(|input: &str| Keyword::Use(input))),
                ("while", Box::new(|input: &str| Keyword::While(input))),
            ]
        }
    }

    pub fn from_str<'s>(&'s self, name: &str, slice: &'s str) -> Option<Keyword> {
        self.keywords.iter().find(|x| x.0 == name).map(|x| x.1(slice))
    }

    pub fn iter_keyword_str(&self) -> impl Iterator<Item = &str> {
        self.keywords.iter().map(|x| x.0)
    }
}

pub struct OpCodeContext {
    op_codes: Vec<(&'static str, Box<dyn Fn(&str) -> OpCode>)>,
}

impl OpCodeContext {
    fn new() -> Self {
        Self {
            op_codes: vec![
                ("{", Box::new(|input: &str| OpCode::OpenBrace(input))),
                ("}", Box::new(|input: &str| OpCode::CloseBrace(input))),
                ("(", Box::new(|input: &str| OpCode::OpenParen(input))),
                (")", Box::new(|input: &str| OpCode::CloseParen(input))),
                ("[", Box::new(|input: &str| OpCode::OpenBracket(input))),
                ("]", Box::new(|input: &str| OpCode::CloseBracket(input))),
                (",", Box::new(|input: &str| OpCode::Comma(input))),
                (":", Box::new(|input: &str| OpCode::Colon(input))),
                ("=", Box::new(|input: &str| OpCode::Bind(input))),
                (";", Box::new(|input: &str| OpCode::Semicolon(input))),
                (".", Box::new(|input: &str| OpCode::Dot(input))),
                ("+", Box::new(|input: &str| OpCode::Plus(input))),
                ("-", Box::new(|input: &str| OpCode::Minus(input))),
                ("*", Box::new(|input: &str| OpCode::Star(input))),
                ("/", Box::new(|input: &str| OpCode::Div(input))),
                ("%", Box::new(|input: &str| OpCode::Percent(input))),
                ("&", Box::new(|input: &str| OpCode::Amp(input))),
                ("|", Box::new(|input: &str| OpCode::Pipe(input))),
                ("^", Box::new(|input: &str| OpCode::Caret(input))),
                ("!", Box::new(|input: &str| OpCode::Bang(input))),
                ("~", Box::new(|input: &str| OpCode::Tilde(input))),
                ("<", Box::new(|input: &str| OpCode::Lt(input))),
                (">", Box::new(|input: &str| OpCode::Gt(input))),
                ("_", Box::new(|input: &str| OpCode::Wildcard(input))),
                ("<-", Box::new(|input: &str| OpCode::Iter(input))),
                ("->", Box::new(|input: &str| OpCode::Arrow(input))),
                ("::", Box::new(|input: &str| OpCode::DoubleColon(input))),
                ("??", Box::new(|input: &str| OpCode::Coalescing(input))),
                ("?.", Box::new(|input: &str| OpCode::CoalescingAccess(input))),
                ("&&", Box::new(|input: &str| OpCode::And(input))),
                ("||", Box::new(|input: &str| OpCode::Or(input))),
                ("==", Box::new(|input: &str| OpCode::Eq(input))),
                ("!=", Box::new(|input: &str| OpCode::Ne(input))),
                ("<=", Box::new(|input: &str| OpCode::Le(input))),
                (">=", Box::new(|input: &str| OpCode::Ge(input))),
                ("<<", Box::new(|input: &str| OpCode::LeftShift(input))),
                (">>", Box::new(|input: &str| OpCode::RightShift(input))),
                ("<|", Box::new(|input: &str| OpCode::LeftPipeline(input))),
                ("|>", Box::new(|input: &str| OpCode::RightPipeline(input))),
                ("..", Box::new(|input: &str| OpCode::Range(input))),
                ("...", Box::new(|input: &str| OpCode::Spread(input))),
            ]
        }
    }

    pub fn from_str<'s>(&'s self, name: &str, slice: &'s str) -> Option<OpCode> {
        self.op_codes.iter().find(|x| x.0 == name).map(|x| x.1(slice))
    }

    pub fn iter_op_code_str(&self) -> impl Iterator<Item = &str> {
        self.op_codes.iter().map(|x| x.0)
    }
}
