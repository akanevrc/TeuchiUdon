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
    Ty(TyRealKey),
}

impl PartialEq for Ty {
    fn eq(&self, other: &Self) -> bool {
        self.id == other.id
    }
}

impl Eq for Ty {}

impl Hash for Ty {
    fn hash<H: Hasher>(&self, state: &mut H) {
        self.id.hash(state);
    }
}

impl SemanticElement for Ty {
    fn description(&self) -> String {
        <Ty as ValueElement<TyKey>>::to_key(self).description()
    }
}

impl ValueElement<TyKey> for Ty {
    fn to_key(&self) -> TyKey {
        TyKey {
            qual: self.base.qual.clone(),
            name: self.base.name.clone(),
            args: self.args.clone(),
        }
    }
}

impl SemanticElement for TyKey {
    fn description(&self) -> String {
        if self.args.len() == 0 {
            format!("{}{}", self.qual.description(), self.name.description())
        }
        else {
            format!(
                "{}{}<{}>",
                self.qual.description(),
                self.name.description(),
                self.args.iter().map(|x| x.description()).collect::<Vec<_>>().join(", "),
            )
        }
    }
}

impl KeyElement<Ty> for TyKey {
    fn consume_key(self, context: &Context) -> Result<Rc<Ty>, ElementError> {
        context.ty_store.get(&self)
    }
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct TyRealKey {
    pub real_name: String,
}

impl ValueElement<TyRealKey> for Ty {
    fn to_key(&self) -> TyRealKey {
        TyRealKey {
            real_name: self.real_name.clone(),
        }
    }
}

impl SemanticElement for TyRealKey {
    fn description(&self) -> String {
        self.real_name.clone()
    }
}

impl KeyElement<Ty> for TyRealKey {
    fn consume_key(self, context: &Context) -> Result<Rc<Ty>, ElementError> {
        context.ty_real_store.get(&self)
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

impl Ty {
    pub fn new(context: &Context, base: Rc<BaseTy>, args: Vec<TyArg>, real_name: String) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.ty_store.next_id(),
            base,
            args,
            real_name,
        });
        let key = value.to_key();
        let real_key = value.to_key();
        context.ty_store.add(key, value.clone())?;
        context.ty_real_store.add(real_key, value.clone())?;
        Ok(value)
    }

    pub fn get(context: &Context, qual: Qual, name: String, args: Vec<TyArg>) -> Result<Rc<Self>, ElementError> {
        TyKey::new(qual, name, args).consume_key(context)
    }

    pub fn get_from_name(context: &Context, name: &str) -> Result<Rc<Self>, ElementError> {
        TyKey::from_name(name).consume_key(context)
    }

    pub fn get_from_real_name(context: &Context, real_name: String) -> Result<Rc<Self>, ElementError> {
        TyRealKey::new(real_name).consume_key(context)
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

    pub fn arg_as_ty(&self) -> TyRealKey {
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

impl TyRealKey {
    pub fn new(real_name: String) -> Self {
        Self {
            real_name,
        }
    }
}
