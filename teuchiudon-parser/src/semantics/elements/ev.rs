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
    method::MethodParamInOut,
    ty::Ty,
};

#[derive(Clone, Debug)]
pub struct Ev {
    pub id: usize,
    pub name: String,
    pub in_tys: Vec<Rc<Ty>>,
    pub real_name: String,
    pub in_real_names: Vec<String>,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub struct EvKey {
    pub name: String,
}

impl_key_value_elements!(
    EvKey,
    Ev,
    EvKey {
        name: self.name.clone()
    },
    ev_store
);

impl SemanticElement for EvKey {
    fn description(&self) -> String {
        self.name.description()
    }

    fn logical_name(&self) -> String {
        self.name.logical_name()
    }
}

impl Ev {
    pub fn new<'input>(
        context: &Context<'input>,
        name: String,
        param_tys: Vec<Rc<Ty>>,
        param_in_outs: Vec<MethodParamInOut>,
        real_name: String,
        param_real_names: Vec<String>,
    ) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.ev_store.next_id(),
            name,
            in_tys: Self::iter_in_or_in_out(param_tys.iter(), param_in_outs.iter()).collect(),
            real_name,
            in_real_names: Self::iter_in_or_in_out(param_real_names.iter(), param_in_outs.iter()).collect(),
        });
        let key = value.to_key();
        context.ev_store.add(key, value.clone())?;
        Ok(value)
    }

    pub fn new_or_get<'input>(
        context: &Context<'input>,
        name: String,
        param_tys: Vec<Rc<Ty>>,
        param_in_outs: Vec<MethodParamInOut>,
        real_name: String,
        param_real_names: Vec<String>,
    ) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.ev_store.next_id(),
            name,
            in_tys: Self::iter_in_or_in_out(param_tys.iter(), param_in_outs.iter()).collect(),
            real_name,
            in_real_names: Self::iter_in_or_in_out(param_real_names.iter(), param_in_outs.iter()).collect(),
        });

        let key = value.to_key();
        match key.clone().get_value(context) {
            Ok(x) => return x,
            Err(_) => (),
        }

        context.ev_store.add(key, value.clone()).unwrap();
        value
    }

    fn iter_in_or_in_out<'a, T: Clone + 'a>(
        iter: impl Iterator<Item = &'a T> + 'a,
        ios: impl Iterator<Item = &'a MethodParamInOut> + 'a
    ) -> impl Iterator<Item = T> + 'a {
        iter.zip(ios).filter_map(|(x, io)| (*io != MethodParamInOut::Out).then_some(x.clone()))
    }

    pub fn get<'input>(
        context: &Context<'input>,
        name: String
    ) -> Result<Rc<Self>, ElementError> {
        EvKey::new(name).get_value(context)
    }
}

impl EvKey {
    pub fn new(name: String) -> Self {
        Self {
            name,
        }
    }
}
