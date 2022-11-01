pub mod ty;
pub mod element;
pub mod label;
pub mod literal;
pub mod qual;
pub mod scope;
pub mod var;

use super::SemanticError;
use self::{
    element::SemanticElement,
    literal::LiteralKey,
    ty::TyKey,
    var::VarKey,
};

#[derive(Clone, Debug, PartialEq)]
pub enum ElementError {
    IllegalState(&'static str),
    LiteralNotFound(LiteralKey),
    TyNotFound(TyKey),
    VarNotFound(VarKey),
}

impl ElementError {
    pub fn convert<'parsed>(&self, slice: Option<&'parsed str>) -> SemanticError<'parsed> {
        SemanticError {
            slice,
            message: match self {
                Self::IllegalState(name) =>
                    format!("Illegal state, in {}", name),
                Self::LiteralNotFound(key) =>
                    format!("Specific literal `{}` not found", key.description()),
                Self::TyNotFound(key) =>
                    format!("Specific type `{}` not found", key.description()),
                Self::VarNotFound(key) =>
                    format!("Specific variable `{}` not found", key.description()),
            },
        }
    }
}
