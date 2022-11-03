use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    element::ValueElement,
    qual::Qual,
    ty::Ty,
};

#[derive(Clone, Debug)]
pub struct Var {
    pub id: usize,
    pub qual: Qual,
    pub name: String,
    pub ty: Rc<Ty>,
    pub mut_attr: bool,
    pub is_system_var: bool,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct VarKey {
    pub qual: Qual,
    pub name: String,
}

impl_key_value_elements!(
    VarKey,
    Var,
    VarKey { qual, name },
    format!("{}{}", qual, name),
    var_store
);

impl Var {
    pub fn new(context: &Context, qual: Qual, name: String, ty: Rc<Ty>, mut_attr: bool, is_system_var: bool) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.literal_store.next_id(),
            qual,
            name,
            ty,
            mut_attr,
            is_system_var,
        });
        let key = value.to_key();
        context.var_store.add(key, value.clone());
        value
    }
}
