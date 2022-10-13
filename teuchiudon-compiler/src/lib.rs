use teuchiudon_parser::{
    lexer::lex,
    parser::parse,
};

pub fn compile(input: &str) -> Result<String, String> {
    let src = lex(input);
    let result = parse(&src);
    match result {
        Ok(x) => Ok(input[x.1.1.items[0].1.clone()].to_owned()),
        Err(_) => Err("Error!".to_owned()),
    }
}

#[test]
fn test_compile() {
    assert_eq!(compile("Hello, TeuchiUdon!"), Ok("TeuchiUdon".to_owned()));
}
