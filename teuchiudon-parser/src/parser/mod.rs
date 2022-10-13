pub mod error_emitter;

use nom::{
    error::Error,
    combinator::eof,
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
use self::error_emitter::map_err;

pub type Parsed<'source> = (LexerItems<'source, Token>, LexerItems<'source, Token>, LexerItems<'source, Token>, LexerItems<'source, Token>);
pub type ParsedError = Vec<String>;
pub type ParsedResult<'source> = Result<(LexerItems<'source, Token>, Parsed<'source>), ParsedError>;

pub fn parse<'src>(src: &'src LexerItemsSource<'src, Token>) -> ParsedResult<'src> {
    let items = LexerItems::new(&src);
    let tokens_hello = Tokens::new(&[Token::Hello, Token::Comma]);
    let tokens_name = Tokens::new(&[Token::Name]);
    let tokens_bang = Tokens::new(&[Token::Bang]);
    let tag_hello = tag::<Tokens<Token>, LexerItems<Token>, Error<LexerItems<Token>>>(tokens_hello);
    let tag_name = tag::<Tokens<Token>, LexerItems<Token>, Error<LexerItems<Token>>>(tokens_name);
    let tag_bang = tag::<Tokens<Token>, LexerItems<Token>, Error<LexerItems<Token>>>(tokens_bang);
    map_err(&tuple((tag_hello, tag_name, tag_bang, eof))(items))
}
