use std::{
    iter::Enumerate,
    slice::Iter,
};
use logos::{
    Lexer,
    Logos,
    SpannedIter,
};
use nom::{
    Compare,
    CompareResult,
    InputIter,
    InputLength,
    InputTake,
    Needed,
};
use super::tokens::Tokens;

pub type LexerItem<'source, Token> = <SpannedIter<'source, Token> as Iterator>::Item;

#[derive(Debug)]
pub struct LexerItemsSource<'source, Token>
where
    Token: Logos<'source> + Copy + 'source
{
    pub items: Vec<LexerItem<'source, Token>>,
}

#[derive(Clone, Debug)]
pub struct LexerItems<'source, Token>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    pub items: &'source [LexerItem<'source, Token>],
}

pub type LexerIter<'source, Token> = Iter<'source, LexerItem<'source, Token>>;

impl<'source, Token> LexerItemsSource<'source, Token>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    pub fn new(lexer: Lexer<'source, Token>) -> Self {
        Self { items: lexer.spanned().collect() }
    }
}

impl<'source, Token> LexerItems<'source, Token>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    pub fn new(src: &'source LexerItemsSource<'source, Token>) -> Self {
        Self { items: &src.items }
    }

    pub fn iter(&self) -> LexerIter<'source, Token> {
        self.items.iter()
    }
}

impl<'source, Token> InputIter for LexerItems<'source, Token>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    type Item = &'source LexerItem<'source, Token>;
    type Iter = Enumerate<Self::IterElem>;
    type IterElem = LexerIter<'source, Token>;

    fn iter_indices(&self) -> Self::Iter {
        self.iter_elements().enumerate()
    }

    fn iter_elements(&self) -> Self::IterElem {
        self.iter()
    }

    fn position<P>(&self, predicate: P) -> Option<usize>
    where
        P: Fn(Self::Item) -> bool
    {
        self.iter_elements().position(predicate)
    }

    fn slice_index(&self, count: usize) -> Result<usize, nom::Needed> {
        let iter_count = self.input_len();
        if count <= iter_count {
            Ok(count)
        }
        else {
            Err(Needed::new(iter_count - count))
        }
    }
}

impl<'source, Token> InputLength for LexerItems<'source, Token>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    fn input_len(&self) -> usize {
        self.items.len()
    }
}

impl<'source, Token> InputTake for LexerItems<'source, Token>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    fn take(&self, count: usize) -> Self {
        Self { items: &self.items[0..count] }
    }

    fn take_split(&self, count: usize) -> (Self, Self) {
        (Self { items: &self.items[count..self.items.len()] }, Self { items: &self.items[0..count] })
    }
}

impl<'source, Token> Compare<Tokens<'source, Token>> for LexerItems<'source, Token>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    fn compare(&self, t: Tokens<'source, Token>) -> CompareResult {
        if
            t.input_len() <= self.input_len() &&
            self.iter()
            .zip(t.iter())
            .all(|(x, y)| x.0 == *y)
        {
            CompareResult::Ok
        }
        else {
            CompareResult::Error
        }
    }

    fn compare_no_case(&self, t: Tokens<'source, Token>) -> CompareResult {
        self.compare(t)
    }
}
