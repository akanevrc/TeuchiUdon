use nom::{
    error::Error,
    IResult,
    bytes::complete::tag,
    sequence::tuple,
};
use super::lexer::{
    items::{
        LexerItems,
        LexerItemsSource,
    },
    token::Token,
    tokens::Tokens,
};

pub type Parsed<'source> = (LexerItems<'source, Token>, LexerItems<'source, Token>, LexerItems<'source, Token>);
pub type ParsedResult<'source> = IResult<LexerItems<'source, Token>, Parsed<'source>>;

pub fn parse<'src>(src: &'src LexerItemsSource<'src, Token>) -> ParsedResult<'src> {
    let items = LexerItems::new(&src);
    let tokens_hello = Tokens::new(&[Token::Hello, Token::Comma]);
    let tokens_name = Tokens::new(&[Token::Name]);
    let tokens_bang = Tokens::new(&[Token::Bang]);
    let tag_hello = tag::<Tokens<Token>, LexerItems<Token>, Error<LexerItems<Token>>>(tokens_hello);
    let tag_name = tag::<Tokens<Token>, LexerItems<Token>, Error<LexerItems<Token>>>(tokens_name);
    let tag_bang = tag::<Tokens<Token>, LexerItems<Token>, Error<LexerItems<Token>>>(tokens_bang);
    tuple((tag_hello, tag_name, tag_bang))(items)
}
