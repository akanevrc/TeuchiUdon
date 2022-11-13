use std::rc::Rc;
use crate::context::Context;
use super::ElementError;

pub trait SemanticElement {
    fn description(&self) -> String;
    fn logical_name(&self) -> String;
}

pub trait ValueElement<Key> {
    fn to_key(&self) -> Key;
}

pub trait KeyElement<Value> {
    fn get_value(&self, context: &Context) -> Result<Rc<Value>, ElementError>;
}

impl SemanticElement for String {
    fn description(&self) -> String {
        self.clone()
    }

    fn logical_name(&self) -> String {
        self.clone()
    }
}
