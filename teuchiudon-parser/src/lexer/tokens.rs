use std::slice::Iter;
use logos::Logos;
use nom::InputLength;

#[derive(Clone)]
pub struct Tokens<'input, Token>
where
    Token: Logos<'input> + Copy + 'input
{
    pub tokens: &'input [Token],
}

impl<'input, Token> Tokens<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    pub fn new(tokens: &'input [Token]) -> Self {
        Self { tokens }
    }

    pub fn iter(&self) -> Iter<Token> {
        self.tokens.iter()
    }
}

impl<'input, Token> InputLength for Tokens<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    fn input_len(&self) -> usize {
        self.tokens.len()
    }
}
