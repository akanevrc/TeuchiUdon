pub mod analyzer;
pub mod ast;
pub mod elements;

pub struct SemanticError<'parsed> {
    pub slice: Option<&'parsed str>,
    pub message: String,
}
