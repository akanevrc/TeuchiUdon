pub mod analyzer;
pub mod ast;
pub mod elements;

#[derive(Clone, Debug, PartialEq)]
pub struct SemanticError<'input> {
    pub slice: Option<&'input str>,
    pub message: String,
}

impl<'input> SemanticError<'input> {
    pub fn new(slice: Option<&'input str>, message: String) -> Self {
        SemanticError {
            slice,
            message,
        }
    }
}
