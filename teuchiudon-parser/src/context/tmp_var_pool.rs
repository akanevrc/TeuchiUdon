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
use crate::semantics::elements::{
    ElementError,
    element::{
        KeyElement,
        ValueElement,
    },
    ty::TyLogicalKey,
    var::Var,
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

    pub fn retain(&self, context: &Context, ty: TyLogicalKey) -> Result<Rc<Var>, ElementError> {
        let mut map = self.pool.borrow_mut();
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
                let ty = ty.get_value(context)?;
                Ok(Var::new_tmp(context, ty))
            }
        }
    }

    pub fn release(&self, var: Rc<Var>) {
        let mut map = self.pool.borrow_mut();
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
}