use std::rc::Rc;
use crate::impl_key_value_elements;
use super::{
    element::SemanticElement,
    ty::{
        Ty,
        TyLogicalKey,
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
    pub param_names: Vec<String>,
    pub in_names: Vec<String>,
    pub out_names: Vec<String>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum MethodParamInOut {
    In,
    InOut,
    Out,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct MethodKey {
    pub ty: TyLogicalKey,
    pub name: String,
    pub in_tys: Vec<TyLogicalKey>,
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
