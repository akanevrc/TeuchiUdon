use self::items::LexerItemsSource;

pub mod items;
pub mod token;
pub mod tokens;

use logos::Logos;
use self::token::Token;

pub fn lex<'input>(input: &'input str) -> LexerItemsSource<'input, Token>{
    let lexer = Token::lexer(input);
    LexerItemsSource::new(lexer)
}
