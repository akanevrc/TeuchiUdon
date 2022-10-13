use std::slice::Iter;
use logos::Logos;
use nom::InputLength;

#[derive(Clone)]
pub struct Tokens<'source, Token>
where
    Token: Logos<'source> + Copy + 'source
{
    pub tokens: &'source [Token],
}

impl<'source, Token> Tokens<'source, Token>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    pub fn new(tokens: &'source [Token]) -> Self {
        Self { tokens }
    }

    pub fn iter(&self) -> Iter<Token> {
        self.tokens.iter()
    }
}

impl<'source, Token> InputLength for Tokens<'source, Token>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    fn input_len(&self) -> usize {
        self.tokens.len()
    }
}
