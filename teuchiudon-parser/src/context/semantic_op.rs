use crate::semantics::ast::{
    Assoc,
    Op,
};

pub struct SemanticOpContext {
    pub priorities: Vec<(Box<dyn Fn(&Op) -> bool>, Assoc)>,
}

impl SemanticOpContext {
    pub fn new() -> Self {
        Self {
            priorities: vec![
                (Box::new(|op_code: &Op| *op_code == Op::TypeAccess),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::Access || *op_code == Op::CoalescingAccess),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::EvalFn),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::EvalSpreadFn),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::EvalKey),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::CastOp),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::Mul || *op_code == Op::Div || *op_code == Op::Mod),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::Add || *op_code == Op::Sub),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::LeftShift || *op_code == Op::RightShift),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::Lt || *op_code == Op::Gt || *op_code == Op::Le || *op_code == Op::Ge),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::Eq || *op_code == Op::Ne),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::BitAnd),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::BitXor),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::BitOr),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::And),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::Or),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::Coalescing),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::RightPipeline),Assoc::Left),
                (Box::new(|op_code: &Op| *op_code == Op::LeftPipeline),Assoc::Right),
                (Box::new(|op_code: &Op| *op_code == Op::Assign),Assoc::Right),
            ],
        }
    }
}
