use std::{
    collections::HashMap,
    rc::Rc,
};
use itertools::Itertools;
use crate::context::Context;
use crate::semantics::elements::ElementError;
use crate::semantics::{
    ast,
    elements::{
        element::ValueElement,
        method::{
            Method,
            OpMethodKey,
        },
        named_methods::{
            NamedMethods,
            NamedMethodsKey,
        },
        ty::Ty,
    },
};

impl<'input> Context<'input> {
    pub fn register_named_methods(&self) -> Result<(), Vec<String>> {
        let kv =
            self.method_store.values()
            .into_group_map_by(|x| NamedMethodsKey::new(x.ty.to_key(), x.name.clone()));
        for methods in kv.into_values() {
            NamedMethods::from_methods(self, methods)
                .map_err(|x| vec![x.message])?;
        }
        Ok(())
    }

    pub fn get_prefix_op_methods(&self, op: &ast::PrefixOp, ty: Rc<Ty>) -> Result<HashMap<OpMethodKey, Rc<Method>>, ElementError> {
        match op {
            ast::PrefixOp::Plus =>
                Ok(HashMap::new()),
            ast::PrefixOp::Minus =>
                Ok(vec![(OpMethodKey::Op, self.get_method(ty, "op_UnaryMinus")?)].into_iter().collect()),
            ast::PrefixOp::Bang =>
                Ok(vec![(OpMethodKey::Op, self.get_method(ty, "op_UnaryNegation")?)].into_iter().collect()),
            ast::PrefixOp::Tilde =>
                Ok(vec![(OpMethodKey::Op, self.get_method(ty, "op_LogicalXor")?)].into_iter().collect()),
        }
    }

    pub fn get_infix_op_methods(&self, op: &ast::Op, _left_ty: Rc<Ty>, _right_ty: Rc<Ty>) -> Result<HashMap<OpMethodKey, Rc<Method>>, ElementError> {
        match op {
            ast::Op::TyAccess =>
                Ok(HashMap::new()),
            ast::Op::EvalFn =>
                Ok(HashMap::new()),
            _ =>
                panic!("Not implemented"),
        }
    }

    fn get_method(&self, ty: Rc<Ty>, name: &str) -> Result<Rc<Method>, ElementError> {
        let key = ty.to_key();
        let type_key = Ty::new_or_get_type_from_key(self, ty.to_key())?.to_key();
        Method::get(self, type_key, name.to_owned(), vec![key])
    }
}
