use std::cell::RefCell;
use crate::context::Context;
use crate::semantics::elements::{
    qual::{
        Qual,
        QualKey,
    },
    scope::Scope,
};

pub struct QualStack {
    stack: RefCell<Vec<QualKey>>,
}

impl QualStack {
    pub fn new() -> Self {
        Self {
            stack: RefCell::new(vec![QualKey::top()]),
        }
    }

    pub fn push_scope(&self, context: &Context, scope: Scope) {
        let mut stack = self.stack.borrow_mut();
        let qual = stack.last().unwrap().pushed(scope);
        Qual::new_or_get(context, qual.scopes.clone());
        stack.push(qual);
    }

    pub fn peek(&self) -> QualKey {
        self.stack.borrow().last().unwrap().clone()
    }

    pub fn pop(&self) -> QualKey {
        let qual = self.stack.borrow_mut().pop();
        if qual.is_none() || qual.clone().unwrap() == QualKey::top() {
            panic!("Top scope has been popped");
        }
        qual.unwrap()
    }

    pub fn iter(&self) -> impl Iterator<Item = QualKey> {
        self.stack.borrow().iter().rev().cloned().collect::<Vec<_>>().into_iter()
    }

    pub fn find_ok<O, E>(&self, f: impl Fn(QualKey) -> Result<O, E>) -> Option<O> {
        for qual in self.iter() {
            match f(qual) {
                Ok(x) => return Some(x),
                Err(_) => (),
            }
        }
        None
    }
}
