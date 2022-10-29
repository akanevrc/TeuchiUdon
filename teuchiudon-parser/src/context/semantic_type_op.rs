use crate::semantics::ast::{
    Assoc,
    TypeOp,
};

pub struct SemanticTypeOpContext {
    pub priorities: Vec<(Box<dyn Fn(&TypeOp) -> bool>, Assoc)>,
}

impl SemanticTypeOpContext {
    pub fn new() -> Self {
        Self {
            priorities: vec![
                (Box::new(|op_code: &TypeOp| *op_code == TypeOp::Access), Assoc::Left),
            ],
        }
    }
}
