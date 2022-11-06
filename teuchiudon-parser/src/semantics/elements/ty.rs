use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    qual::Qual,
};

#[derive(Clone, Debug)]
pub struct BaseTy {
    pub id: usize,
    pub qual: Qual,
    pub name: String,
    pub logical_name: String,
    pub real_name: Option<String>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct BaseTyKey {
    pub qual: Qual,
    pub name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct Ty {
    pub base: Rc<BaseTy>,
    pub args: Vec<TyArg>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum TyArg {
    Qual(Qual),
    Ty(Rc<Ty>),
}

impl_key_value_elements!(
    BaseTyKey,
    BaseTy,
    BaseTyKey { qual, name },
    format!("{}{}", qual, name),
    ty_store
);

impl SemanticElement for Ty {
    fn description(&self) -> String {
        format!(
            "{}{}<{}>",
            self.base.qual.description(),
            self.base.name.description(),
            self.args.iter().map(|x| x.description()).collect::<Vec<_>>().join(", ")
        )
    }
}

impl SemanticElement for TyArg {
    fn description(&self) -> String {
        match self {
            Self::Qual(x) => x.description(),
            Self::Ty(x) => x.description(),
        }
    }
}

impl BaseTy {
    pub fn new(context: &Context, qual: Qual, name: String, logical_name: String, real_name: Option<String>) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.ty_store.next_id(),
            qual,
            name,
            logical_name,
            real_name,
        });
        let key = value.to_key();
        context.ty_store.add(key, value.clone());
        value
    }

    pub fn get_from_name(context: &Context, name: &str) -> Rc<Self> {
        BaseTyKey::from_name(name).consume_key(context).unwrap()
    }

    pub fn get_from_name_or_err(context: &Context, name: &str) -> Result<Rc<Self>, ElementError> {
        BaseTyKey::from_name(name).consume_key_or_err(context)
    }

    pub fn apply(self: &Rc<Self>, args: Vec<TyArg>) -> Rc<Ty> {
        Rc::new(Ty {
            base: self.clone(),
            args,
        })
    }

    pub fn direct(self: &Rc<Self>) -> Rc<Ty> {
        self.apply(Vec::new())
    }
}

impl BaseTyKey {
    pub fn from_name(name: &str) -> Self {
        Self {
            qual: Qual::TOP,
            name: name.to_owned(),
        }
    }

    pub fn eq_with(&self, ty: &Rc<Ty>) -> bool {
        *self == ty.base.to_key()
    }
}

impl Ty {
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

    pub fn arg_as_ty(&self) -> Rc<Ty> {
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
