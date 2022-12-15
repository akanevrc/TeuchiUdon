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
    method::Method,
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

    pub fn retain_term_prefix_op_tmp_vars<'input>(
        context: &Context<'input>,
        op: &ast::TermPrefixOp,
        ty: Rc<Ty>
    ) -> Result<Vec<Rc<Self>>, ElementError> {
        match op {
            ast::TermPrefixOp::Plus =>
                Ok(Vec::new()),
            ast::TermPrefixOp::Minus |
            ast::TermPrefixOp::Bang |
            ast::TermPrefixOp::Tilde =>
                Ok(vec![context.retain_tmp_var(ty.to_key())?]),
        }
    }

    pub fn retain_term_infix_op_tmp_vars<'input>(
        context: &Context,
        op: &ast::TermInfixOp,
        left_ty: Rc<Ty>,
        right_ty: Rc<Ty>
    ) -> Result<Vec<Rc<Self>>, ElementError> {
        match op {
            ast::TermInfixOp::Mul |
            ast::TermInfixOp::Div |
            ast::TermInfixOp::Mod |
            ast::TermInfixOp::Add |
            ast::TermInfixOp::Sub => {
                let ty = if left_ty.assignable_from(context, &right_ty) { left_ty } else { right_ty };
                Ok(vec![context.retain_tmp_var(ty.to_key())?])
            },
            ast::TermInfixOp::LeftShift |
            ast::TermInfixOp::RightShift =>
                Ok(vec![context.retain_tmp_var(left_ty.to_key())?]),
            _ =>
                panic!("Not implemented"),
        }
    }

    pub fn retain_factor_infix_op_tmp_vars<'input>(
        _context: &Context,
        op: &ast::FactorInfixOp,
        _left_ty: Rc<Ty>,
        _right_ty: Rc<Ty>
    ) -> Result<Vec<Rc<Self>>, ElementError> {
        match op {
            ast::FactorInfixOp::TyAccess |
            ast::FactorInfixOp::Access =>
                Ok(Vec::new()),
            ast::FactorInfixOp::EvalFn =>
                panic!("Illegal state"),
            _ =>
                panic!("Not implemented"),
        }
    }

    pub fn retain_method_tmp_vars(
        context: &Context,
        method: Rc<Method>,
    ) -> Result<Vec<Rc<Self>>, ElementError> {
        method.out_tys.iter().map(|x| context.retain_tmp_var(x.to_key())).collect()
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
