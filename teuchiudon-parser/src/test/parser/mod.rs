mod error_emitter;

use crate::lexer::{
    token::Token,
    lex,
};
use crate::parser::parse;

#[test]
fn test_ok() {
    let input = "Hello, world!";
    let src = lex(input);
    let result = parse(&src);
    match result {
        Ok(res) => {
            assert_eq!(res.1.0.items, &[(Token::Hello, 0..5), (Token::Comma, 5..6)]);
            assert_eq!(res.1.1.items, &[(Token::Name, 7..12)]);
            assert_eq!(res.1.2.items, &[(Token::Bang, 12..13)]);
            assert_eq!(&input[res.1.1.items[0].1.clone()], "world");
        },
        Err(_) => unreachable!(),
    }
}

#[test]
fn test_err_invalid_token() {
    let input = "Hello-world!";
    let src = lex(input);
    let result = parse(&src);
    match result {
        Ok(_) => unreachable!(),
        Err(_) => (),
    }
}

#[test]
fn test_err_insufficient_tokens() {
    let input = "Hello, world";
    let src = lex(input);
    let result = parse(&src);
    match result {
        Ok(_) => unreachable!(),
        Err(_) => (),
    }
}

#[test]
fn test_err_excessive_tokens() {
    let input = "Hello, world!!";
    let src = lex(input);
    let result = parse(&src);
    match result {
        Ok(_) => unreachable!(),
        Err(_) => (),
    }
}
