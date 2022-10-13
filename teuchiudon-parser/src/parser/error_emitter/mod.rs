use logos::Logos;
use nom::{
    Err,
    IResult,
    error::Error,
};
use crate::lexer::{
    items::LexerItems,
    token::Token,
};
use crate::parser::{
    Parsed,
    ParsedError,
    ParsedResult,
};

pub fn map_err<'source>(result: &IResult<LexerItems<'source, Token>, Parsed<'source>>) -> ParsedResult<'source>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    match result {
        Ok(x) => Ok(x.clone()),
        Err(e) => Err(nom_err_to_parsed_err(&e))
    }
}

fn nom_err_to_parsed_err<'source, Token>(err: &Err<Error<LexerItems<'source, Token>>>) -> ParsedError
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    match err {
        Err::Incomplete(_) => vec!["Input data is incomplete.".to_owned()],
        Err::Error(_) => vec!["Parse error occurred.".to_owned()],
        Err::Failure(_) => vec!["Parse error occurred.".to_owned()],
    }
}
