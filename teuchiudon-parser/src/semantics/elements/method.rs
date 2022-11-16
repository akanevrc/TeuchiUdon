use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    element::{
        SemanticElement,
        ValueElement,
    },
    ty::{
        Ty,
        TyKey,
    }
};

#[derive(Clone, Debug)]
pub struct Method {
    pub id: usize,
    pub ty: Rc<Ty>,
    pub name: String,
    pub param_tys: Vec<Rc<Ty>>,
    pub in_tys: Vec<Rc<Ty>>,
    pub out_tys: Vec<Rc<Ty>>,
    pub param_in_outs: Vec<MethodParamInOut>,
    pub real_name: String,
    pub param_real_names: Vec<String>,
    pub in_real_names: Vec<String>,
    pub out_real_names: Vec<String>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum MethodParamInOut {
    In,
    InOut,
    Out,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct MethodKey {
    pub ty: TyKey,
    pub name: String,
    pub in_tys: Vec<TyKey>,
}

impl_key_value_elements!(
    MethodKey,
    Method,
    MethodKey {
        ty: self.ty.to_key(),
        name: self.name.clone(),
        in_tys: self.in_tys.iter().map(|x| x.to_key()).collect()
    },
    method_store
);

impl SemanticElement for MethodKey {
    fn description(&self) -> String {
        format!(
            "{}::{}({})",
            self.ty.description(),
            self.name.description(),
            self.in_tys.iter().map(|x| x.description()).collect::<Vec<_>>().join(", ")
        )
    }

    fn logical_name(&self) -> String {
        format!(
            "method[{}][{}][{}]",
            self.ty.logical_name(),
            self.name.logical_name(),
            self.in_tys.iter().map(|x| x.logical_name()).collect::<Vec<_>>().join("][")
        )
    }
}

impl Method {
    pub fn new(
        context: &Context,
        ty: Rc<Ty>,
        name: String,
        param_tys: Vec<Rc<Ty>>,
        param_in_outs: Vec<MethodParamInOut>,
        real_name: String,
        param_real_names: Vec<String>,
    ) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.method_store.next_id(),
            ty,
            name,
            param_tys: param_tys.clone(),
            in_tys: Self::iter_in_or_in_out(param_tys.iter(), param_in_outs.iter()).collect(),
            out_tys: Self::iter_out(param_tys.iter(), param_in_outs.iter()).collect(),
            param_in_outs: param_in_outs.clone(),
            real_name,
            param_real_names: param_real_names.clone(),
            in_real_names: Self::iter_in_or_in_out(param_real_names.iter(), param_in_outs.iter()).collect(),
            out_real_names: Self::iter_out(param_real_names.iter(), param_in_outs.iter()).collect(),
        });
        let key = value.to_key();
        context.method_store.add(key, value.clone())?;
        Ok(value)
    }

    fn iter_in_or_in_out<'a, T: Clone + 'a>(
        iter: impl Iterator<Item = &'a T> + 'a,
        ios: impl Iterator<Item = &'a MethodParamInOut> + 'a
    ) -> impl Iterator<Item = T> + 'a {
        iter.zip(ios).filter_map(|(x, io)| (*io != MethodParamInOut::Out).then_some(x.clone()))
    }

    fn iter_out<'a, T: Clone + 'a>(
        iter: impl Iterator<Item = &'a T> + 'a,
        ios: impl Iterator<Item = &'a MethodParamInOut> + 'a
    ) -> impl Iterator<Item = T> + 'a {
        iter.zip(ios).filter_map(|(x, io)| (*io == MethodParamInOut::Out).then_some(x.clone()))
    }
}
