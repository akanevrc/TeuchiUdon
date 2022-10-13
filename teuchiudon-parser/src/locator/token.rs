use logos::Logos;

#[derive(Logos, Clone, Copy, Debug, Eq, PartialEq)]
pub enum Token {
    #[regex(r"[^\r\n]*(\r\n|\r|\n)?")]
    Line,

    #[error]
    Error,
}
