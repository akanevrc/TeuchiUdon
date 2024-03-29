use std::{
    cell::RefCell,
    rc::Rc,
};
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    label::DataLabel,
    qual::{
        Qual,
        QualKey,
    },
    ty::Ty,
};

#[derive(Clone, Debug)]
pub struct Var {
    pub id: usize,
    pub qual: Rc<Qual>,
    pub name: String,
    pub ty: RefCell<Rc<Ty>>,
    pub mut_attr: bool,
    pub actual_name: RefCell<Option<Rc<DataLabel>>>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct VarKey {
    pub qual: QualKey,
    pub name: String,
}

impl_key_value_elements!(
    VarKey,
    Var,
    VarKey {
        qual: self.qual.to_key(),
        name: self.name.clone()
    },
    var_store
);

impl SemanticElement for VarKey {
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

impl Var {
    pub fn force_new<'input>(
        context: &Context<'input>,
        qual: Rc<Qual>,
        name: String,
        ty: Rc<Ty>,
        mut_attr: bool,
        actual_name: Option<Rc<DataLabel>>
    ) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.var_store.next_id(),
            qual,
            name,
            ty: RefCell::new(ty),
            mut_attr,
            actual_name: RefCell::new(actual_name),
        });
        let key = value.to_key();
        context.var_store.force_add(key, value.clone());
        value
    }

    pub fn get<'input>(
        context: &Context<'input>,
        qual: QualKey,
        name: String
    ) -> Result<Rc<Self>, ElementError> {
        VarKey::new(qual, name).get_value(context)
    }
}

impl VarKey {
    pub fn new(qual: QualKey, name: String) -> Self {
        Self {
            qual,
            name,
        }
    }
}
