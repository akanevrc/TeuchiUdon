pub mod base_ty;
pub mod element;
pub mod ev;
pub mod label;
pub mod literal;
pub mod method;
pub mod named_methods;
pub mod qual;
pub mod scope;
pub mod ty;
pub mod ty_op;
pub mod var;

use super::SemanticError;

#[derive(Clone, Debug, PartialEq)]
pub struct ElementError {
    pub message: String,
}

impl ElementError {
    pub fn new(message: String) -> Self {
        Self {
            message
        }
    }

    pub fn convert(self, slice: Option<&str>) -> Vec<SemanticError> {
        vec![SemanticError::new(
            slice,
            self.message,
        )]
    }
}
