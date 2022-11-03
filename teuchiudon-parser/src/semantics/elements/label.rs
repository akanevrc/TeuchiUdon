use std::rc::Rc;
use super::{
    literal::Literal,
    ty::Ty,
    var::Var,
};

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct DataLabel {
    pub ty: Rc<TyLabel>,
    pub kind: DataLabelKind,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum DataLabelKind {
    Literal(Rc<Literal>),
    Var(Rc<Var>),
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct CodeLabel {
    kind: CodeLabelKind,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum CodeLabelKind {
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct TyLabel {
    pub ty: Rc<Ty>,
}

impl DataLabel {
    pub fn new(kind: DataLabelKind) -> Self {
        let ty = Rc::new(match &kind {
            DataLabelKind::Literal(x) => TyLabel::new(x.ty.clone()),
            DataLabelKind::Var(x) => TyLabel::new(x.ty.clone()),
        });
        Self {
            ty,
            kind,
        }
    }
}

impl CodeLabel {
    pub fn new(kind: CodeLabelKind) -> Self {
        Self {
            kind,
        }
    }
}

impl TyLabel {
    pub fn new(ty: Rc<Ty>) -> Self {
        Self {
            ty,
        }
    }
}
