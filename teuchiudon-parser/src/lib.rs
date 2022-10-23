pub mod context;
pub mod error;
pub mod lexer;
pub mod parser;

#[cfg(test)]
mod test;

#[cfg(test)]
#[macro_use]
extern crate assert_matches;

use nom::{
    Err,
    Finish,
    error::VerboseError,
};
use context::Context;
use error::convert_error;
use parser::{
    ast::Target,
    target,
};

pub type ParsedResult<'input, O> = Result<(&'input str, O), Err<VerboseError<&'input str>>>;

pub fn parse<'context: 'input, 'input>(context: &'context Context, input: &'input str) -> Result<Target<'input>, String> {
    target(context)(input).finish()
    .map(|x| x.1)
    .map_err(|e| convert_error(input, e))
}
