pub mod analyzer;
pub mod ast;
pub mod elements;

#[derive(Clone, Debug, PartialEq)]
pub struct SemanticError<'parsed> {
    pub slice: Option<&'parsed str>,
    pub message: String,
}
