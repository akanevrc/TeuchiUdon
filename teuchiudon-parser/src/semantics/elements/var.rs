use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    element::{
        SemanticElement,
        ValueElement,
    },
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
    pub ty: Rc<Ty>,
    pub mut_attr: bool,
    pub is_system_var: bool,
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
            self.qual.qualify_logical_name("::"),
            self.name.logical_name()
        )
    }
}

impl Var {
    pub fn new(context: &Context, qual: Rc<Qual>, name: String, ty: Rc<Ty>, mut_attr: bool, is_system_var: bool) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.literal_store.next_id(),
            qual,
            name,
            ty,
            mut_attr,
            is_system_var,
        });
        let key = value.to_key();
        context.var_store.add(key, value.clone())?;
        Ok(value)
    }
}
