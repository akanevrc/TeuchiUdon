use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    element::ValueElement,
    qual::Qual,
};

#[derive(Clone, Debug)]
pub struct Ty {
    pub id: usize,
    pub qual: Qual,
    pub name: String,
    pub logical_name: String,
    pub real_name: Option<String>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct TyKey {
    pub qual: Qual,
    pub name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum TyArg {
    Ty(Rc<Ty>),
}

impl_key_value_elements!(
    TyKey,
    Ty,
    TyKey { qual, name },
    format!("{}{}", qual, name),
    ty_store
);

impl Ty {
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
}

impl TyKey {
    pub fn from_name(name: &str) -> Self {
        Self {
            qual: Qual::TOP,
            name: name.to_owned(),
        }
    }
}
