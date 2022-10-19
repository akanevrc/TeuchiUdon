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
    keywords: Vec<(&'static str, Keyword)>,
}

impl KeywordContext {
    fn new() -> Self {
        Self {
            keywords: vec![
                ("as", Keyword::As),
                ("break", Keyword::Break),
                ("continue", Keyword::Continue),
                ("else", Keyword::Else),
                ("enum", Keyword::Enum),
                ("false", Keyword::False),
                ("fn", Keyword::Fn),
                ("for", Keyword::For),
                ("if", Keyword::If),
                ("in", Keyword::In),
                ("is", Keyword::Is),
                ("let", Keyword::Let),
                ("linear", Keyword::Linear),
                ("loop", Keyword::Loop),
                ("match", Keyword::Match),
                ("mod", Keyword::Mod),
                ("mut", Keyword::Mut),
                ("newtype", Keyword::Newtype),
                ("null", Keyword::Null),
                ("pub", Keyword::Pub),
                ("ref", Keyword::Ref),
                ("return", Keyword::Return),
                ("smooth", Keyword::Smooth),
                ("struct", Keyword::Struct),
                ("sync", Keyword::Sync),
                ("this", Keyword::This),
                ("true", Keyword::True),
                ("type", Keyword::Type),
                ("use", Keyword::Use),
                ("while", Keyword::While),
            ]
        }
    }

    pub fn from_str(&self, s: &str) -> Keyword {
        self.keywords.iter().find(|x| x.0 == s).unwrap().1
    }

    pub fn iter_keyword_str(&self) -> impl Iterator<Item = &str> {
        self.keywords.iter().map(|x| x.0)
    }
}

pub struct OpCodeContext {
    op_codes: Vec<(&'static str, OpCode)>,
}

impl OpCodeContext {
    fn new() -> Self {
        Self {
            op_codes: vec![
                ("{", OpCode::OpenBrace),
                ("}", OpCode::CloseBrace),
                ("(", OpCode::OpenParen),
                (")", OpCode::CloseParen),
                ("[", OpCode::OpenBracket),
                ("]", OpCode::CloseBracket),
                (",", OpCode::Comma),
                (":", OpCode::Colon),
                ("=", OpCode::Bind),
                (";", OpCode::Semicolon),
                (".", OpCode::Dot),
                ("+", OpCode::Plus),
                ("-", OpCode::Minus),
                ("*", OpCode::Star),
                ("/", OpCode::Div),
                ("%", OpCode::Percent),
                ("&", OpCode::Amp),
                ("|", OpCode::Pipe),
                ("^", OpCode::Caret),
                ("!", OpCode::Bang),
                ("~", OpCode::Tilde),
                ("<", OpCode::Lt),
                (">", OpCode::Gt),
                ("_", OpCode::Wildcard),
                ("<-", OpCode::Iter),
                ("->", OpCode::Arrow),
                ("::", OpCode::DoubleColon),
                ("??", OpCode::Coalescing),
                ("?.", OpCode::CoalescingAccess),
                ("&&", OpCode::And),
                ("||", OpCode::Or),
                ("==", OpCode::Eq),
                ("!=", OpCode::Ne),
                ("<=", OpCode::Le),
                (">=", OpCode::Ge),
                ("<<", OpCode::LeftShift),
                (">>", OpCode::RightShift),
                ("<|", OpCode::LeftPipeline),
                ("|>", OpCode::RightPipeline),
                ("..", OpCode::Range),
                ("...", OpCode::Spread),
            ]
        }
    }

    pub fn from_str(&self, s: &str) -> OpCode {
        self.op_codes.iter().find(|x| x.0 == s).unwrap().1
    }

    pub fn iter_op_code_str(&self) -> impl Iterator<Item = &str> {
        self.op_codes.iter().map(|x| x.0)
    }
}
