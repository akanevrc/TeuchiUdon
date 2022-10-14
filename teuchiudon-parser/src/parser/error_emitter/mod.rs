use logos::Logos;
use nom::{
    Err,
    error::VerboseError,
    error::convert_error,
};
use crate::lexer::{
    items::LexerItems,
    token::Token,
};
use crate::parser::{
    ParsedError,
    ParsedResult,
    ParsingResult,
};

pub fn map_err<'source>(input: LexerItems<'source, Token>, result: ParsingResult<'source>) -> ParsedResult<'source>
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    match result {
        Ok(x) => Ok(x.clone()),
        Err(e) => Err(nom_err_to_parsed_err(input, e))
    }
}

fn nom_err_to_parsed_err<'source, Token>(input: LexerItems<'source, Token>, err: Err<VerboseError<LexerItems<'source, Token>>>) -> ParsedError
where
    Token: Logos<'source> + Copy + PartialEq + 'source
{
    match err {
        Err::Incomplete(_) => vec!["Input data is incomplete.".to_owned()],
        Err::Error(x) => vec![convert_error(input, x)],
        Err::Failure(x) => vec![convert_error(input, x)],
    }
}
