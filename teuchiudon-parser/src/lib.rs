pub mod context;
pub mod lexer;
pub mod parser;

#[cfg(test)]
mod test;

use nom::{
    Err,
    error::VerboseError,
};

pub type ParsedResult<'input, O> = Result<(&'input str, O), Err<VerboseError<&'input str>>>;
