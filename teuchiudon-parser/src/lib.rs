pub mod lexer;

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
    use logos::Logos;
    use nom::{
        bytes::complete::tag,
        error::Error,
        sequence::tuple,
    };
    use lexer::{
        items::{
            LexerItems,
            LexerItemsSource,
        },
        token::Token,
        tokens::Tokens,
    };

    let input = "Hello, world!";
    let lex = Token::lexer(input);
    let src = LexerItemsSource::new(lex);
    let items = LexerItems::new(&src);
    let tokens_hello = Tokens::new(&[Token::Hello, Token::Comma]);
    let tokens_name = Tokens::new(&[Token::Name]);
    let tokens_bang = Tokens::new(&[Token::Bang]);
    let tag_hello = tag::<Tokens<Token>, LexerItems<Token>, Error<LexerItems<Token>>>(tokens_hello);
    let tag_name = tag::<Tokens<Token>, LexerItems<Token>, Error<LexerItems<Token>>>(tokens_name);
    let tag_bang = tag::<Tokens<Token>, LexerItems<Token>, Error<LexerItems<Token>>>(tokens_bang);
    let result = tuple((tag_hello, tag_name, tag_bang))(items);
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
