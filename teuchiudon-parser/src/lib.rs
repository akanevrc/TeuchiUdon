pub mod lexer;
pub mod parser;

#[test]
fn test_lexer() {
    use logos::Logos;
    use lexer::token::Token;

    let mut lex = Token::lexer("Hello, world!");

    assert_eq!(lex.next(), Some(Token::Hello));
    assert_eq!(lex.slice(), "Hello");
    assert_eq!(lex.next(), Some(Token::Comma));
    assert_eq!(lex.slice(), ",");
    assert_eq!(lex.next(), Some(Token::Name));
    assert_eq!(lex.slice(), "world");
    assert_eq!(lex.next(), Some(Token::Bang));
    assert_eq!(lex.slice(), "!");
    assert_eq!(lex.next(), None);
}

#[test]
fn test_parser() {
    use lexer::{
        token::Token,
        lex,
    };
    use parser::parse;

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
