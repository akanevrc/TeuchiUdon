use std::rc::Rc;
use super::{
    element::SemanticElement,
    ty::Ty,
};

pub trait HasLabel: SemanticElement {
    fn part_name(&self) -> String;
    fn full_name(&self) -> String;
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum Label {
    Ty(Rc<Ty>)
}
