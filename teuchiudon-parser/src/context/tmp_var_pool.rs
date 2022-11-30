use std::{
    cell::RefCell,
    cmp::Reverse,
    collections::{
        BinaryHeap,
        HashMap,
    },
    rc::Rc,
};
use crate::context::Context;
use crate::semantics::{
    ast,
    elements::{
        ElementError,
        element::{
            KeyElement,
            ValueElement,
        },
        ty::{
            Ty,
            TyLogicalKey,
        },
        var::Var,
    },
};

pub struct TmpVarPool {
    pool: RefCell<HashMap<TyLogicalKey, BinaryHeap<Reverse<Rc<Var>>>>>,
}

impl TmpVarPool {
    pub fn new() -> Self {
        Self {
            pool: RefCell::new(HashMap::new()),
        }
    }
}

impl<'input> Context<'input> {
    pub fn retain_tmp_var(&self, ty: TyLogicalKey) -> Result<Rc<Var>, ElementError> {
        let mut map = self.tmp_var_pool.pool.borrow_mut();
        let heap = match map.get_mut(&ty) {
            Some(x) => x,
            None => {
                map.insert(ty.clone(), BinaryHeap::new());
                map.get_mut(&ty).unwrap()
            }
        };

        match heap.pop() {
            Some(rev) => Ok(rev.0),
            None => {
                let ty = ty.get_value(self)?;
                Ok(Var::new_tmp(self, ty))
            }
        }
    }

    pub fn release_tmp_var(&self, var: Rc<Var>) {
        let mut map = self.tmp_var_pool.pool.borrow_mut();
        let ty = var.ty.borrow();
        let key = ty.to_key();
        let heap = match map.get_mut(&key) {
            Some(x) => x,
            None => {
                map.insert(key.clone(), BinaryHeap::new());
                map.get_mut(&key).unwrap()
            }
        };
        heap.push(Reverse(var.clone()))
    }

    pub fn get_prefix_op_tmp_vars(&self, op: &ast::PrefixOp, ty: Rc<Ty>) -> Result<Vec<Rc<Var>>, ElementError> {
        match op {
            ast::PrefixOp::Plus =>
                Ok(Vec::new()),
            ast::PrefixOp::Minus =>
                Ok(vec![self.retain_tmp_var(ty.to_key())?]),
            ast::PrefixOp::Bang =>
                Ok(vec![self.retain_tmp_var(ty.to_key())?]),
            ast::PrefixOp::Tilde =>
                Ok(vec![self.retain_tmp_var(ty.to_key())?]),
        }
    }

    pub fn get_infix_op_tmp_vars(&self, op: &ast::Op, _left_ty: Rc<Ty>, _right_ty: Rc<Ty>) -> Result<Vec<Rc<Var>>, ElementError> {
        match op {
            ast::Op::TyAccess =>
                Ok(Vec::new()),
            ast::Op::EvalFn =>
                Ok(Vec::new()),
            _ =>
                panic!("Not implemented"),
        }
    }
}
