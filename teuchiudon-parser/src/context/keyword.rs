use crate::lexer::ast::Keyword;

pub struct KeywordContext {
    keywords: Vec<(&'static str, Box<dyn Fn(&str) -> Keyword>)>,
}

impl KeywordContext {
    pub fn new<'str>() -> Self {
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
