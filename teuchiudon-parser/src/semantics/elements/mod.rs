use super::SemanticError;

pub mod base_ty;
pub mod element;
pub mod label;
pub mod literal;
pub mod method;
pub mod qual;
pub mod scope;
pub mod ty;
pub mod var;

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
        vec![SemanticError {
            slice,
            message: self.message,
        }]
    }
}
