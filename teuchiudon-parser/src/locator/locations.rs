use std::{
    collections::HashMap,
    ops::Range,
};
use logos::Lexer;
use super::token::Token;

pub struct Locations {
    pub lines: HashMap<usize, Range<usize>>,
}

impl Locations {
    pub fn new(lexer: Lexer<Token>) -> Self {
        let mut lines = HashMap::new();
        for (i, (token, range)) in lexer.spanned().enumerate() {
            match token {
                Token::Line => {
                    lines.insert(i, range.clone());
                },
                Token::Error => unreachable!(),
            }
        }
        Self { lines }
    }

    pub fn to_line_column(&self, index: usize) -> Option<(usize, usize)> {
        for (line, range) in &self.lines {
            if range.contains(&index) {
                return Some((line + 1, index - range.start + 1));
            }
        }
        None
    }
}
