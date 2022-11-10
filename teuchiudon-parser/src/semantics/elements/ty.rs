use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    base_ty::BaseTy,
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    qual::Qual,
};

#[derive(Clone, Debug)]
pub struct Ty {
    pub id: usize,
    pub base: Rc<BaseTy>,
    pub args: Vec<TyArg>,
    pub real_name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct TyKey {
    pub qual: Qual,
    pub name: String,
    pub args: Vec<TyArg>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum TyArg {
    Qual(Qual),
    Ty(TyKey),
}

impl_key_value_elements!(
    TyKey,
    Ty,
    TyKey {
        qual: self.base.qual.clone(),
        name: self.base.name.clone(),
        args: self.args.clone()
    },
    format!(
        "{}{}<{}>",
        self.qual.description(),
        self.name.description(),
        self.args.iter().map(|x| x.description()).collect::<Vec<_>>().join(", ")
    ),
    ty_store
);

impl SemanticElement for TyArg {
    fn description(&self) -> String {
        match self {
            Self::Qual(x) => x.description(),
            Self::Ty(x) => x.description(),
        }
    }
}

impl Ty {
    pub fn new(context: &Context, base: Rc<BaseTy>, args: Vec<TyArg>, real_name: String) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.ty_store.next_id(),
            base,
            args,
            real_name,
        });
        let key = value.to_key();
        context.ty_store.add(key, value.clone());
        value
    }

    pub fn get(context: &Context, qual: Qual, name: String, args: Vec<TyArg>) -> Result<Rc<Self>, ElementError> {
        TyKey::new(qual, name, args).consume_key(context)
    }

    pub fn get_from_name(context: &Context, name: &str) -> Result<Rc<Self>, ElementError> {
        TyKey::from_name(name).consume_key(context)
    }

    pub fn arg_as_qual(&self) -> Qual {
        if self.args.len() == 1 {
            match &self.args[0] {
                TyArg::Qual(x) =>
                    x.clone(),
                _ =>
                    panic!("Illegal state"),
            }
        }
        else {
            panic!("Illegal state")
        }
    }

    pub fn arg_as_ty(&self) -> TyKey {
        if self.args.len() == 1 {
            match &self.args[0] {
                TyArg::Ty(x) =>
                    x.clone(),
                _ =>
                    panic!("Illegal state"),
            }
        }
        else {
            panic!("Illegal state")
        }
    }
}

impl TyKey {
    pub fn new(qual: Qual, name: String, args: Vec<TyArg>) -> Self {
        Self {
            qual,
            name,
            args,
        }
    }

    pub fn from_name(name: &str) -> Self {
        Self {
            qual: Qual::TOP,
            name: name.to_owned(),
            args: Vec::new(),
        }
    }
}
