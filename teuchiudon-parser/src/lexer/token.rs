use logos::{
    Logos,
    skip,
};

#[derive(Logos, Clone, Copy, Debug, Eq, PartialEq)]
pub enum Token {
    #[token("Hello")]
    Hello,

    #[token(",")]
    Comma,

    #[regex(r"\w+")]
    Name,

    #[token("!")]
    Bang,

    #[error]
    #[regex(r"\s+", skip)]
    Error,
}
