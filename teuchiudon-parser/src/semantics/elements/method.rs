use std::{
    collections::HashMap,
    rc::Rc,
};
use crate::impl_key_value_elements;
use crate::context::Context;
use crate::semantics::ast;
use super::{
    ElementError,
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    ty::{
        Ty,
        TyKey,
    },
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

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum MethodParamInOut {
    In,
    InOut,
    Out,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
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
    pub fn new<'input>(
        context: &Context<'input>,
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

    pub fn get(
        context: &Context,
        ty: TyKey,
        name: String,
        in_tys: Vec<TyKey>,
    ) -> Result<Rc<Self>, ElementError>
    {
        MethodKey::new(ty, name, in_tys).get_value(context)
    }

    pub fn get_term_prefix_op_methods<'input>(
        context: &Context<'input>,
        op: &ast::TermPrefixOp,
        ty: Rc<Ty>
    ) -> Result<HashMap<&'static str, Rc<Self>>, ElementError> {
        match op {
            ast::TermPrefixOp::Plus =>
                Ok(HashMap::new()),
            ast::TermPrefixOp::Minus =>
                Ok(vec![("op", Self::get_unary_op_method(context, ty, "op_UnaryMinus")?)].into_iter().collect()),
            ast::TermPrefixOp::Bang =>
                Ok(vec![("op", Self::get_unary_op_method(context, ty, "op_UnaryNegation")?)].into_iter().collect()),
            ast::TermPrefixOp::Tilde =>
                Ok(vec![("op", Self::get_binary_op_method(context, ty.clone(), ty, "op_LogicalXor")?)].into_iter().collect()),
        }
    }

    pub fn get_term_infix_op_methods<'input>(
        context: &Context<'input>,
        op: &ast::TermInfixOp,
        left_ty: Rc<Ty>,
        right_ty: Rc<Ty>
    ) -> Result<HashMap<&'static str, Rc<Self>>, ElementError> {
        match op {
            ast::TermInfixOp::Mul =>
                Ok(vec![("op", Self::get_binary_op_method(context, left_ty.clone(), right_ty.clone(), "op_Multiply").or(Self::get_binary_op_method(context, left_ty, right_ty, "op_Multiplication"))?)].into_iter().collect()),
            ast::TermInfixOp::Div =>
                Ok(vec![("op", Self::get_binary_op_method(context, left_ty, right_ty, "op_Division")?)].into_iter().collect()),
            ast::TermInfixOp::Mod =>
                Ok(vec![("op", Self::get_binary_op_method(context, left_ty.clone(), right_ty.clone(), "op_Remainder").or(Self::get_binary_op_method(context, left_ty, right_ty, "op_Modulus"))?)].into_iter().collect()),
            ast::TermInfixOp::Add =>
                Ok(vec![("op", Self::get_binary_op_method(context, left_ty, right_ty, "op_Addition")?)].into_iter().collect()),
            ast::TermInfixOp::Sub =>
                Ok(vec![("op", Self::get_binary_op_method(context, left_ty, right_ty, "op_Subtraction")?)].into_iter().collect()),
            ast::TermInfixOp::LeftShift =>
                Ok(vec![("op", Self::get_asymmetric_op_method(context, left_ty, right_ty, "op_LeftShift")?)].into_iter().collect()),
            ast::TermInfixOp::RightShift =>
                Ok(vec![("op", Self::get_asymmetric_op_method(context, left_ty, right_ty, "op_RightShift")?)].into_iter().collect()),
            _ =>
                panic!("Not implemented"),
        }
    }

    pub fn get_factor_infix_op_methods<'input>(
        _context: &Context<'input>,
        op: &ast::FactorInfixOp,
        _left_ty: Rc<Ty>,
        _right_ty: Rc<Ty>
    ) -> Result<HashMap<&'static str, Rc<Self>>, ElementError> {
        match op {
            ast::FactorInfixOp::TyAccess |
            ast::FactorInfixOp::Access |
            ast::FactorInfixOp::EvalFn =>
                Ok(HashMap::new()),
            _ =>
                panic!("Not implemented"),
        }
    }

    fn get_unary_op_method(context: &Context, ty: Rc<Ty>, name: &str) -> Result<Rc<Self>, ElementError> {
        let key = ty.to_key();
        let type_key = Ty::new_or_get_type_from_key(context, ty.to_key())?.to_key();
        Method::get(context, type_key, name.to_owned(), vec![key])
    }

    fn get_binary_op_method(context: &Context, left_ty: Rc<Ty>, right_ty: Rc<Ty>, name: &str) -> Result<Rc<Self>, ElementError> {
        let ty = if left_ty.assignable_from(context, &right_ty) { left_ty } else { right_ty };
        let key: TyKey = ty.to_key();
        let type_key = Ty::new_or_get_type_from_key(context, ty.to_key())?.to_key();
        Method::get(context, type_key, name.to_owned(), vec![key.clone(), key])
    }

    fn get_asymmetric_op_method(context: &Context, left_ty: Rc<Ty>, right_ty: Rc<Ty>, name: &str) -> Result<Rc<Self>, ElementError> {
        let left_key = left_ty.to_key();
        let right_key = right_ty.to_key();
        let type_key = Ty::new_or_get_type_from_key(context, left_ty.to_key())?.to_key();
        Method::get(context, type_key, name.to_owned(), vec![left_key, right_key])
    }
}

impl MethodKey {
    pub fn new(
        ty: TyKey,
        name: String,
        in_tys: Vec<TyKey>,
    ) -> Self {
        Self {
            ty,
            name,
            in_tys,
        }
    }
}
