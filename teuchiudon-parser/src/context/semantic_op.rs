use crate::semantics::ast::{
    Assoc,
    TermInfixOp,
    FactorInfixOp,
};

pub struct SemanticOpContext {
    pub term_priorities: Vec<(Box<dyn Fn(&TermInfixOp) -> bool>, Assoc)>,
    pub factor_priorities: Vec<(Box<dyn Fn(&FactorInfixOp) -> bool>, Assoc)>,
}

impl SemanticOpContext {
    pub fn new() -> Self {
        Self {
            term_priorities: vec![
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::CastOp), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::Mul || *op_code == TermInfixOp::Div || *op_code == TermInfixOp::Mod), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::Add || *op_code == TermInfixOp::Sub), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::LeftShift || *op_code == TermInfixOp::RightShift), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::Lt || *op_code == TermInfixOp::Gt || *op_code == TermInfixOp::Le || *op_code == TermInfixOp::Ge), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::Eq || *op_code == TermInfixOp::Ne), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::BitAnd), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::BitXor), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::BitOr), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::And), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::Or), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::Coalescing), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::RightPipeline), Assoc::Left),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::LeftPipeline), Assoc::Right),
                (Box::new(|op_code: &TermInfixOp| *op_code == TermInfixOp::Assign), Assoc::Right),
            ],
            factor_priorities: vec![
                (Box::new(|op_code: &FactorInfixOp| *op_code == FactorInfixOp::TyAccess), Assoc::Left),
                (Box::new(|op_code: &FactorInfixOp| *op_code == FactorInfixOp::Access || *op_code == FactorInfixOp::CoalescingAccess), Assoc::Left),
                (Box::new(|op_code: &FactorInfixOp| *op_code == FactorInfixOp::EvalFn), Assoc::Left),
                (Box::new(|op_code: &FactorInfixOp| *op_code == FactorInfixOp::EvalSpreadFn), Assoc::Left),
                (Box::new(|op_code: &FactorInfixOp| *op_code == FactorInfixOp::EvalKey), Assoc::Left),
            ],
        }
    }
}
