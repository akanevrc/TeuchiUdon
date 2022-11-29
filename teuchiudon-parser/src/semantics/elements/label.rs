use std::rc::Rc;
use super::{
    ev::Ev,
    literal::Literal,
    method::Method,
    this_literal::ThisLiteral,
    ty::Ty,
    var::Var,
};

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub struct DataLabel {
    pub ty: Rc<TyLabel>,
    pub kind: DataLabelKind,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum DataLabelKind {
    Literal(Rc<Literal>),
    ThisLiteral(Rc<ThisLiteral>),
    Var(Rc<Var>),
    Indirect(Rc<CodeLabel>, u32),
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub struct CodeLabel {
    pub kind: CodeLabelKind,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum CodeLabelKind {
    Ev(Rc<Ev>),
    Text(String),
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub struct TyLabel {
    pub kind: TyLabelKind,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum TyLabelKind {
    Ty(Rc<Ty>),
    Addr,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub struct ExternLabel {
    pub kind: ExternLabelKind,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum ExternLabelKind {
    Method(Rc<Method>),
}

impl DataLabel {
    pub fn new(kind: DataLabelKind) -> Rc<Self> {
        let ty = match &kind {
            DataLabelKind::Literal(x) => TyLabel::new(TyLabelKind::Ty(x.ty.clone())),
            DataLabelKind::ThisLiteral(x) => TyLabel::new(TyLabelKind::Ty(x.ty.clone())),
            DataLabelKind::Var(x) => TyLabel::new(TyLabelKind::Ty(x.ty.borrow().clone())),
            DataLabelKind::Indirect(_, _) => TyLabel::new(TyLabelKind::Addr),
        };
        Rc::new(Self {
            ty,
            kind,
        })
    }
}

impl CodeLabel {
    pub fn new(kind: CodeLabelKind) -> Rc<Self> {
        Rc::new(Self {
            kind,
        })
    }

    pub fn from_name(name: &str) -> Rc<Self> {
        Rc::new(Self {
            kind: CodeLabelKind::Text(name.to_owned()),
        })
    }
}

impl TyLabel {
    pub fn new(kind: TyLabelKind) -> Rc<Self> {
        Rc::new(Self {
            kind,
        })
    }
}

impl ExternLabel {
    pub fn new(kind: ExternLabelKind) -> Rc<Self> {
        Rc::new(Self {
            kind,
        })
    }
}
