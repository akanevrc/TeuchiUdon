use crate::semantics::ast::{
    Assoc,
    TyOp,
};

pub struct SemanticTyOpContext {
    pub priorities: Vec<(Box<dyn Fn(&TyOp) -> bool>, Assoc)>,
}

impl SemanticTyOpContext {
    pub fn new() -> Self {
        Self {
            priorities: vec![
                (Box::new(|op_code: &TyOp| *op_code == TyOp::Access), Assoc::Left),
            ],
        }
    }
}
