use logos::Logos;
use crate::lexer::token::Token;

#[test]
fn test() {
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
