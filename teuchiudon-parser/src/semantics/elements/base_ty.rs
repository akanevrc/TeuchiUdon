use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    element::{
        KeyElement,
        ValueElement,
    },
    qual::Qual,
    ty::{
        Ty,
        TyKey,
        TyKeyArg,
    }
};

#[derive(Clone, Debug)]
pub struct BaseTy {
    pub id: usize,
    pub qual: Qual,
    pub name: String,
    pub logical_name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct BaseTyKey {
    pub qual: Qual,
    pub name: String,
}

impl_key_value_elements!(
    BaseTyKey,
    BaseTy,
    BaseTyKey {
        qual: self.qual.clone(),
        name: self.name.clone()
    },
    format!(
        "{}{}",
        self.qual.description(),
        self.name.description()
    ),
    base_ty_store
);

impl BaseTy {
    pub fn new(context: &Context, qual: Qual, name: String, logical_name: String) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.base_ty_store.next_id(),
            qual,
            name,
            logical_name,
        });
        let key = value.to_key();
        context.base_ty_store.add(key, value.clone());
        value
    }

    pub fn get_from_name(context: &Context, name: &str) -> Result<Rc<Self>, ElementError> {
        BaseTyKey::from_name(name).consume_key(context)
    }

    pub fn apply(self: &Rc<Self>, context: &Context, args: Vec<TyKeyArg>) -> Result<Rc<Ty>, ElementError> {
        self.to_key().apply(args).consume_key(context)
    }

    pub fn direct(self: &Rc<Self>, context: &Context) -> Result<Rc<Ty>, ElementError> {
        self.apply(context, Vec::new())
    }
}

impl BaseTyKey {
    pub fn new(qual: Qual, name: String) -> Self {
        Self {
            qual,
            name,
        }
    }

    pub fn from_name(name: &str) -> Self {
        Self {
            qual: Qual::TOP,
            name: name.to_owned(),
        }
    }

    pub fn apply(&self, args: Vec<TyKeyArg>) -> TyKey {
        TyKey::new(self.qual.clone(), self.name.clone(), args)
    }

    pub fn direct(&self) -> TyKey {
        self.apply(Vec::new())
    }

    pub fn eq_with(&self, ty: &Rc<Ty>) -> bool {
        *self == ty.base.to_key()
    }
}
