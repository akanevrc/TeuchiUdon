use std::{
    cell::RefCell,
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
    pub is_mut: bool,
    pub is_tmp: bool,
    pub actual_name: RefCell<Option<Rc<DataLabel>>>,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
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
        is_mut: bool,
        actual_name: Option<Rc<DataLabel>>
    ) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.var_store.next_id(),
            qual,
            name,
            ty: RefCell::new(ty),
            is_mut,
            is_tmp: false,
            actual_name: RefCell::new(actual_name),
        });
        let key = value.to_key();
        context.var_store.force_add(key, value.clone());
        value
    }

    pub fn new_tmp<'input>(
        context: &Context<'input>,
        ty: Rc<Ty>,
    ) -> Rc<Self> {
        let id = context.var_store.next_id();
        let value = Rc::new(Self {
            id,
            qual: Qual::top(context),
            name: "_tmp".to_owned(),
            ty: RefCell::new(ty),
            is_mut: false,
            is_tmp: true,
            actual_name: RefCell::new(None),
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

    pub fn retain_term_prefix_op_tmp_vars(context: &Context, op: &ast::TermPrefixOp, ty: Rc<Ty>) -> Result<Vec<Rc<Self>>, ElementError> {
        match op {
            ast::TermPrefixOp::Plus =>
                Ok(Vec::new()),
            ast::TermPrefixOp::Minus =>
                Ok(vec![context.retain_tmp_var(ty.to_key())?]),
            ast::TermPrefixOp::Bang =>
                Ok(vec![context.retain_tmp_var(ty.to_key())?]),
            ast::TermPrefixOp::Tilde =>
                Ok(vec![context.retain_tmp_var(ty.to_key())?]),
        }
    }

    pub fn retain_term_infix_op_tmp_vars(_context: &Context, op: &ast::TermInfixOp, _left_ty: Rc<Ty>, _right_ty: Rc<Ty>) -> Result<Vec<Rc<Self>>, ElementError> {
        match op {
            _ =>
                panic!("Not implemented"),
        }
    }

    pub fn retain_factor_infix_op_tmp_vars(_context: &Context, op: &ast::FactorInfixOp, _left_ty: Rc<Ty>, _right_ty: Rc<Ty>) -> Result<Vec<Rc<Self>>, ElementError> {
        match op {
            ast::FactorInfixOp::TyAccess =>
                Ok(Vec::new()),
            ast::FactorInfixOp::EvalFn =>
                Ok(Vec::new()),
            _ =>
                panic!("Not implemented"),
        }
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
