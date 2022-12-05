pub mod base_ty;
pub mod element;
pub mod ev;
pub mod ev_stats;
pub mod eval_fn;
pub mod fn_stats;
pub mod label;
pub mod literal;
pub mod method;
pub mod named_methods;
pub mod operation;
pub mod qual;
pub mod scope;
pub mod this_literal;
pub mod top_stat;
pub mod ty;
pub mod ty_op;
pub mod valued_var;
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
