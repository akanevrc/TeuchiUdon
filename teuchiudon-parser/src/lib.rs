pub mod context;
pub mod lexer;
pub mod parser;

#[cfg(test)]
mod test;

#[cfg(test)]
#[macro_use]
extern crate assert_matches;

use nom::{
    Err,
    error::VerboseError,
};

pub type ParsedResult<'input, O> = Result<(&'input str, O), Err<VerboseError<&'input str>>>;
