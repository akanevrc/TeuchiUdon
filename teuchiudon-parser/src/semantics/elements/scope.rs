use std::rc::Rc;
use super::{
    element::SemanticElement,
    ty::Ty,
};

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum Scope {
    Block,
    FnBlock,
    LoopBlock,
    For,
    Fn,
    Closure,
    LetIn,
    Ty(Rc<Ty>),
    VarBind,
}

impl SemanticElement for Scope {
    fn description(&self) -> String {
        match self {
            Self::Ty(ty) =>
                ty.description(),
            _ =>
                "Not implemented error occured".to_owned(),
        }
    }
}
