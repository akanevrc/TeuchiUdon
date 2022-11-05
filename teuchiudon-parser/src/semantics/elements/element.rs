use std::rc::Rc;
use crate::context::Context;
use super::ElementError;

pub trait SemanticElement {
    fn description(&self) -> String;
}

pub trait ValueElement<Key> {
    fn to_key(&self) -> Key;
}

pub trait KeyElement<Value> {
    fn consume_key(self, context: &Context) -> Option<Rc<Value>>;
    fn consume_key_or_err(self, context: &Context) -> Result<Rc<Value>, ElementError>;
}

impl SemanticElement for String {
    fn description(&self) -> String {
        self.clone()
    }
}