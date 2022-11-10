pub mod context;
pub mod lexer;
pub mod parser;
pub mod semantics;
mod error;
mod macroes;

#[cfg(test)]
mod test;

use nom::Err;
use nom_supreme::final_parser::final_parser;
use self::{
    context::Context,
    error::{
        ErrorTree,
        parsed_error::convert_parsed_error,
        semantic_error::convert_semantic_error,
    },
};


pub type ParsedResult<'input, O> = Result<(&'input str, O), Err<ErrorTree<'input>>>;

pub fn parse<'context: 'input, 'input>(
    context: &'context Context,
    input: &'input str
) -> Result<parser::ast::Target<'input>, Vec<String>> {
    final_parser(parser::target(context))(input)
    .map_err(|e| convert_parsed_error(input, e))
}

pub fn analize<'context: 'input, 'input>(
    context: &'context Context,
    input: &'input str,
    parsed: &'input parser::ast::Target<'input>
) -> Result<semantics::ast::Target<'input>, Vec<String>> {
    semantics::analyzer::target(context, parsed)
    .map_err(|e| convert_semantic_error(input, e))
}
