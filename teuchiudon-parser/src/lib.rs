pub mod context;
pub mod error;
pub mod lexer;
pub mod parser;
pub mod semantics;
mod macroes;

#[cfg(test)]
mod test;

use nom::Err;
use nom_supreme::final_parser::final_parser;
use self::context::Context;
use self::error::{
    ErrorTree,
    convert::convert_error,
};
use self::parser::{
    ast::Target,
    target,
};

pub type ParsedResult<'input, O> = Result<(&'input str, O), Err<ErrorTree<'input>>>;

pub fn parse<'context: 'input, 'input>(context: &'context Context, input: &'input str) -> Result<Target<'input>, Vec<String>> {
    final_parser(target(context))(input)
    .map_err(|e| convert_error(input, e))
}
