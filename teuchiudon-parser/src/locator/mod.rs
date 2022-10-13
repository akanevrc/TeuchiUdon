pub mod locations;
pub mod token;

use logos::Logos;
use self::{
    locations::Locations,
    token::Token,
};

pub fn locate(input: &str) -> Locations {
    let lexer = Token::lexer(input);
    Locations::new(lexer)
}
