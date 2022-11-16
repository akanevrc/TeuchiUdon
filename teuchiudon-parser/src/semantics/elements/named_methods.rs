use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    method::Method,
    element::{
        SemanticElement,
        ValueElement,
    },
    ty::{
        Ty,
        TyKey,
    },
    var::Var,
};

#[derive(Clone, Debug)]
pub struct NamedMethods {
    pub id: usize,
    pub methods: Vec<Rc<Method>>,
    pub ty: Rc<Ty>,
    pub name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct NamedMethodsKey {
    pub ty: TyKey,
    pub name: String,
}

impl_key_value_elements!(
    NamedMethodsKey,
    NamedMethods,
    NamedMethodsKey {
        ty: self.ty.to_key(),
        name: self.name.clone()
    },
    named_methods_store
);

impl SemanticElement for NamedMethodsKey {
    fn description(&self) -> String {
        format!(
            "{}::{}",
            self.ty.description(),
            self.name.description(),
        )
    }

    fn logical_name(&self) -> String {
        format!(
            "method[{}][{}]",
            self.ty.logical_name(),
            self.name.logical_name(),
        )
    }
}

impl NamedMethods {
    pub fn from_methods(context: &Context, methods: Vec<Rc<Method>>) -> Result<Rc<Self>, ElementError> {
        if methods.len() == 0 {
            panic!("Illegal state");
        }
        let value = Rc::new(Self {
            id: context.named_methods_store.next_id(),
            methods: methods.clone(),
            ty: methods[0].ty.clone(),
            name: methods[0].name.clone(),
        });
        let key = value.to_key();
        context.named_methods_store.add(key.clone(), value.clone())?;

        let pushed = value.ty.base.qual.new_or_get_pushed_qual(context, value.ty.base.name.clone())?;
        let ty = Ty::get_method_from_key(context, key)?;
        Var::force_new(context, pushed, value.name.clone(), ty, false, false)?;
        Ok(value)
    }
}
