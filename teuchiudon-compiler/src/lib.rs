use lalrpop_util::{ParseError, lexer::Token};
use teuchiudon_parser::grammar::TargetParser;

pub fn compile(input: &str) -> Result<&str, ParseError<usize, Token<'_>, &'static str>> {
    TargetParser::new().parse(input)
}

#[test]
fn test_compile() {
    assert_eq!(compile("Hello, TeuchiUdon!").unwrap(), "TeuchiUdon");
}
