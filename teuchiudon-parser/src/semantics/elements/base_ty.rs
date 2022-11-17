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
    qual::{
        Qual,
        QualKey,
    },
    ty::{
        Ty,
        TyArg,
        TyKey,
    }
};

#[derive(Clone, Debug)]
pub struct BaseTy {
    pub id: usize,
    pub qual: Rc<Qual>,
    pub name: String,
    pub logical_name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct BaseTyKey {
    pub qual: QualKey,
    pub name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct BaseTyLogicalKey {
    pub logical_name: String,
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

    fn logical_name(&self) -> String {
        <BaseTy as ValueElement<BaseTyLogicalKey>>::to_key(self).logical_name()
    }
}

impl ValueElement<BaseTyKey> for BaseTy {
    fn to_key(&self) -> BaseTyKey {
        BaseTyKey {
            qual: self.qual.to_key(),
            name: self.name.clone(),
        }
    }
}

impl SemanticElement for BaseTyKey {
    fn description(&self) -> String {
        format!(
            "{}{}",
            self.qual.qualify_description("::"),
            self.name.description()
        )
    }

    fn logical_name(&self) -> String {
        format!(
            "{}{}",
            self.qual.qualify_logical_name(">"),
            self.name.logical_name()
        )
    }
}

impl KeyElement<BaseTy> for BaseTyKey {
    fn get_value(&self, context: &Context) -> Result<Rc<BaseTy>, ElementError> {
        context.base_ty_store.get(self)
    }
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
        self.logical_name.description()
    }

    fn logical_name(&self) -> String {
        self.logical_name.logical_name()
    }
}

impl KeyElement<BaseTy> for BaseTyLogicalKey {
    fn get_value(&self, context: &Context) -> Result<Rc<BaseTy>, ElementError> {
        context.base_ty_logical_store.get(self)
    }
}

impl BaseTy {
    pub fn new(context: &Context, qual: Rc<Qual>, name: String, logical_name: String) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.base_ty_store.next_id(),
            qual,
            name,
            logical_name,
        });
        let key = value.to_key();
        let logical_key = value.to_key();
        context.base_ty_store.add(key, value.clone())?;
        context.base_ty_logical_store.add(logical_key, value.clone())?;
        Ok(value)
    }

    pub fn get(context: &Context, qual: QualKey, name: String) -> Result<Rc<Self>, ElementError> {
        BaseTyKey::new(qual, name).get_value(context)
    }

    pub fn get_from_name(context: &Context, name: &str) -> Result<Rc<Self>, ElementError> {
        BaseTyKey::from_name(name).get_value(context)
    }

    pub fn get_from_logical_name(context: &Context, logical_name: String) -> Result<Rc<Self>, ElementError> {
        BaseTyLogicalKey::new(logical_name).get_value(context)
    }

    pub fn new_or_get_applied(self: &Rc<Self>, context: &Context, args: Vec<TyArg>) -> Result<Rc<Ty>, ElementError> {
        let key: BaseTyKey = self.to_key();
        Ok(Ty::new_or_get(context, key.get_value(context)?, args))
    }

    pub fn new_or_get_applied_zero(self: &Rc<Self>, context: &Context) -> Result<Rc<Ty>, ElementError> {
        self.new_or_get_applied(context, Vec::new())
    }

    pub fn get_applied(self: &Rc<Self>, context: &Context, args: Vec<TyArg>) -> Result<Rc<Ty>, ElementError> {
        Ty::get(context, self.qual.to_key(), self.name.clone(), args)
    }

    pub fn get_applied_zero(self: &Rc<Self>, context: &Context) -> Result<Rc<Ty>, ElementError> {
        self.get_applied(context, Vec::new())
    }

    pub fn eq_with(self: &Rc<Self>, ty: &Rc<Ty>) -> bool {
        *self == ty.base
    }
}

impl BaseTyKey {
    pub fn new(qual: QualKey, name: String) -> Self {
        Self {
            qual,
            name,
        }
    }

    pub fn from_name(name: &str) -> Self {
        Self {
            qual: QualKey::top(),
            name: name.to_owned(),
        }
    }

    pub fn new_applied(&self, args: Vec<TyArg>) -> TyKey {
        TyKey::new(self.qual.clone(), self.name.clone(), args)
    }

    pub fn new_applied_zero(&self) -> TyKey {
        self.new_applied(Vec::new())
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

    pub fn eq_with(&self, ty: &Rc<Ty>) -> bool {
        *self == ty.base.to_key()
    }
}
