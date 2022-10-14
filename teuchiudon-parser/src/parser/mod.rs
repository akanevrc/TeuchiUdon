pub mod error_emitter;

use nom::{
    Err,
    error::VerboseError,
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

type ParsingResult<'source> = Result<(LexerItems<'source, Token>, Parsed<'source>), Err<VerboseError<LexerItems<'source, Token>>>>;

pub fn parse<'src>(src: &'src LexerItemsSource<'src, Token>) -> ParsedResult<'src> {
    let items = LexerItems::new(&src);
    let tokens_hello = Tokens::new(&[Token::Hello, Token::Comma]);
    let tokens_name = Tokens::new(&[Token::Name]);
    let tokens_bang = Tokens::new(&[Token::Bang]);
    let tag_hello = tag(tokens_hello);
    let tag_name = tag(tokens_name);
    let tag_bang = tag(tokens_bang);
    map_err(items.clone(), tuple((tag_hello, tag_name, tag_bang, eof))(items))
}
