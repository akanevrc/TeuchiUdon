use crate::lexer::ast::{
    Keyword,
    KeywordKind,
};

pub struct KeywordContext {
    keywords: Vec<(&'static str, KeywordKind)>,
}

impl KeywordContext {
    pub fn new() -> Self {
        Self {
            keywords: vec![
                ("as", KeywordKind::As),
                ("break", KeywordKind::Break),
                ("continue", KeywordKind::Continue),
                ("else", KeywordKind::Else),
                ("enum", KeywordKind::Enum),
                ("false", KeywordKind::False),
                ("fn", KeywordKind::Fn),
                ("for", KeywordKind::For),
                ("if", KeywordKind::If),
                ("in", KeywordKind::In),
                ("is", KeywordKind::Is),
                ("let", KeywordKind::Let),
                ("linear", KeywordKind::Linear),
                ("loop", KeywordKind::Loop),
                ("match", KeywordKind::Match),
                ("mod", KeywordKind::Mod),
                ("mut", KeywordKind::Mut),
                ("newtype", KeywordKind::Newtype),
                ("null", KeywordKind::Null),
                ("pub", KeywordKind::Pub),
                ("ref", KeywordKind::Ref),
                ("return", KeywordKind::Return),
                ("smooth", KeywordKind::Smooth),
                ("struct", KeywordKind::Struct),
                ("sync", KeywordKind::Sync),
                ("this", KeywordKind::This),
                ("true", KeywordKind::True),
                ("type", KeywordKind::Type),
                ("use", KeywordKind::Use),
                ("while", KeywordKind::While),
            ]
        }
    }

    pub fn from_str<'context: 'input, 'input>(&'context self, name: &str, slice: &'input str) -> Option<Keyword<'input>> {
        self.keywords.iter().find(|x| x.0 == name).map(|x| Keyword(slice, x.1))
    }

    pub fn iter_keyword_str(&self) -> impl Iterator<Item = &str> {
        self.keywords.iter().map(|x| x.0)
    }
}
