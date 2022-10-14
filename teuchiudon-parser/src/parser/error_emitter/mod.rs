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

pub fn map_err<'input>(input: LexerItems<'input, Token>, result: ParsingResult<'input>) -> ParsedResult<'input>
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    match result {
        Ok(x) => Ok(x.clone()),
        Err(e) => Err(nom_err_to_parsed_err(input, e))
    }
}

fn nom_err_to_parsed_err<'input, Token>(input: LexerItems<'input, Token>, err: Err<VerboseError<LexerItems<'input, Token>>>) -> ParsedError
where
    Token: Logos<'input> + Copy + PartialEq + 'input
{
    match err {
        Err::Incomplete(_) => vec!["Input data is incomplete.".to_owned()],
        Err::Error(x) => vec![convert_error(input, x)],
        Err::Failure(x) => vec![convert_error(input, x)],
    }
}
