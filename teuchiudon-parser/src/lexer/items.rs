use std::{
    iter::Enumerate,
    ops::Deref,
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

pub type LexerItem<'input, Token> = <SpannedIter<'input, Token> as Iterator>::Item;

#[derive(Debug)]
pub struct LexerItemsSource<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    pub input: &'input str,
    pub items: Vec<LexerItem<'input, Token>>,
}

#[derive(Clone, Debug)]
pub struct LexerItems<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    pub input: &'input str,
    pub items: &'input [LexerItem<'input, Token>],
}

pub type LexerIter<'input, Token> = Iter<'input, LexerItem<'input, Token>>;

impl<'input, Token> LexerItemsSource<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    pub fn new(input: &'input str, lexer: Lexer<'input, Token>) -> Self {
        Self { input, items: lexer.spanned().collect() }
    }
}

impl<'input, Token> LexerItems<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    pub fn new(src: &'input LexerItemsSource<'input, Token>) -> Self {
        Self { input: src.input, items: &src.items }
    }

    pub fn iter(&self) -> LexerIter<'input, Token> {
        self.items.iter()
    }
}

impl<'input, Token> Deref for LexerItems<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    type Target = str;

    fn deref(&self) -> &Self::Target {
        self.input
    }
}

impl<'input, Token> InputIter for LexerItems<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    type Item = &'input LexerItem<'input, Token>;
    type Iter = Enumerate<Self::IterElem>;
    type IterElem = LexerIter<'input, Token>;

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

impl<'input, Token> InputLength for LexerItems<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    fn input_len(&self) -> usize {
        self.items.len()
    }
}

impl<'input, Token> InputTake for LexerItems<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    fn take(&self, count: usize) -> Self {
        let input_count = if count < self.items.len() { self.items[count].1.start } else { self.items[count - 1].1.end } - self.items[0].1.start;
        Self { input: &self.input[..input_count], items: &self.items[..count] }
    }

    fn take_split(&self, count: usize) -> (Self, Self) {
        let input_count = if count < self.items.len() { self.items[count].1.start } else { self.items[count - 1].1.end } - self.items[0].1.start;
        (
            Self { input: &self.input[input_count..], items: &self.items[count..] },
            Self { input: &self.input[..input_count], items: &self.items[..count] },
        )
    }
}

impl<'input, Token> Compare<Tokens<'input, Token>> for LexerItems<'input, Token>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    fn compare(&self, t: Tokens<'input, Token>) -> CompareResult {
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

    fn compare_no_case(&self, t: Tokens<'input, Token>) -> CompareResult {
        self.compare(t)
    }
}
