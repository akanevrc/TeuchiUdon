use std::{
    hash::{
        Hash,
        Hasher,
    },
    rc::Rc,
};
use crate::context::Context;
use super::{
    ElementError,
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    qual::Qual,
    ty::{
        Ty,
        TyArg,
        TyKey,
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

impl PartialEq for BaseTy {
    fn eq(&self, other: &Self) -> bool {
        self.id == other.id
    }
}

impl Eq for BaseTy {}

impl Hash for BaseTy {
    fn hash<H: Hasher>(&self, state: &mut H) {
        self.id.hash(state);
    }
}

impl SemanticElement for BaseTy {
    fn description(&self) -> String {
        <BaseTy as ValueElement<BaseTyKey>>::to_key(self).description()
    }
}

impl ValueElement<BaseTyKey> for BaseTy {
    fn to_key(&self) -> BaseTyKey {
        BaseTyKey {
            qual: self.qual.clone(),
            name: self.name.clone(),
        }
    }
}

impl SemanticElement for BaseTyKey {
    fn description(&self) -> String {
        format!("{}{}", self.qual.description(), self.name.description())
    }
}

impl KeyElement<BaseTy> for BaseTyKey {
    fn consume_key(self, context: &Context) -> Result<Rc<BaseTy>, ElementError> {
        context.base_ty_store.get(&self)
    }
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct BaseTyLogicalKey {
    pub logical_name: String,
}

impl ValueElement<BaseTyLogicalKey> for BaseTy {
    fn to_key(&self) -> BaseTyLogicalKey {
        BaseTyLogicalKey {
            logical_name: self.logical_name.clone(),
        }
    }
}

impl SemanticElement for BaseTyLogicalKey {
    fn description(&self) -> String {
        self.logical_name.clone()
    }
}

impl KeyElement<BaseTy> for BaseTyLogicalKey {
    fn consume_key(self, context: &Context) -> Result<Rc<BaseTy>, ElementError> {
        context.base_ty_logical_store.get(&self)
    }
}

impl BaseTy {
    pub fn new(context: &Context, qual: Qual, name: String, logical_name: String) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.base_ty_store.next_id(),
            qual,
            name,
            logical_name,
        });
        let key = value.to_key();
        let logical_key = value.to_key();
        context.base_ty_store.add(key, value.clone());
        context.base_ty_logical_store.add(logical_key, value.clone());
        value
    }

    pub fn get(context: &Context, qual: Qual, name: String) -> Result<Rc<Self>, ElementError> {
        BaseTyKey::new(qual, name).consume_key(context)
    }

    pub fn get_from_name(context: &Context, name: &str) -> Result<Rc<Self>, ElementError> {
        BaseTyKey::from_name(name).consume_key(context)
    }

    pub fn get_from_logical_name(context: &Context, logical_name: String) -> Result<Rc<Self>, ElementError> {
        BaseTyLogicalKey::new(logical_name).consume_key(context)
    }

    pub fn apply(self: &Rc<Self>, context: &Context, args: Vec<TyArg>) -> Result<Rc<Ty>, ElementError> {
        <BaseTy as ValueElement<BaseTyKey>>::to_key(self).apply(args).consume_key(context)
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

    pub fn apply(&self, args: Vec<TyArg>) -> TyKey {
        TyKey::new(self.qual.clone(), self.name.clone(), args)
    }

    pub fn direct(&self) -> TyKey {
        self.apply(Vec::new())
    }

    pub fn eq_with(&self, ty: &Rc<Ty>) -> bool {
        *self == ty.base.to_key()
    }
}

impl BaseTyLogicalKey {
    pub fn new(logical_name: String) -> Self {
        Self {
            logical_name,
        }
    }
}
