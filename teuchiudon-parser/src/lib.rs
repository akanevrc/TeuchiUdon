pub mod lexer;
pub mod parser;

#[cfg(test)]
mod test;

use nom::{
    Err,
    error::VerboseError,
};

pub type ParsedResult<'input> = Result<(&'input str, &'input str), Err<VerboseError<&'input str>>>;
