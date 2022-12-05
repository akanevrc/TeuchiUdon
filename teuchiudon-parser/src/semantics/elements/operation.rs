use std::{
    collections::HashMap,
    fmt,
    rc::Rc,
};
use crate::impl_key_value_elements;
use crate::context::Context;
use crate::semantics::ast;
use super::{
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    literal::Literal,
    method::Method,
    ty::{
        Ty,
        TyKey,
    },
};

#[derive(Clone, Debug)]
pub struct Operation {
    pub id: usize,
    pub ty: Rc<Ty>,
    pub op: OperationKind,
    pub op_methods: HashMap<&'static str, Rc<Method>>,
    pub op_literals: HashMap<&'static str, Rc<Literal>>,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum OperationKind {
    TermPrefixOp(ast::TermPrefixOp),
    TermInfixOp(ast::TermInfixOp),
    FactorInfixOp(ast::FactorInfixOp),
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub struct OperationKey {
    pub ty: TyKey,
    pub op: OperationKind,
}

impl_key_value_elements!(
    OperationKey,
    Operation,
    OperationKey {
        ty: self.ty.to_key(),
        op: self.op.clone()
    },
    operation_store
);

impl SemanticElement for OperationKey {
    fn description(&self) -> String {
        format!(
            "{}::({})",
            self.ty.description(),
            self.op.description(),
        )
    }

    fn logical_name(&self) -> String {
        format!(
            "op[{}][{}]",
            self.ty.logical_name(),
            self.op.logical_name(),
        )
    }
}

impl SemanticElement for OperationKind {
    fn description(&self) -> String {
        self.to_string()
    }

    fn logical_name(&self) -> String {
        self.to_string()
    }
}

impl fmt::Display for OperationKind {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            OperationKind::TermPrefixOp(x) =>
                match x {
                    ast::TermPrefixOp::Plus => write!(f, "'+"),
                    ast::TermPrefixOp::Minus => write!(f, "'-"),
                    ast::TermPrefixOp::Bang => write!(f, "!"),
                    ast::TermPrefixOp::Tilde => write!(f, "~"),
                },
            OperationKind::TermInfixOp(x) =>
                match x {
                    ast::TermInfixOp::CastOp => write!(f, "as"),
                    ast::TermInfixOp::Mul => write!(f, "*"),
                    ast::TermInfixOp::Div => write!(f, "/"),
                    ast::TermInfixOp::Mod => write!(f, "%"),
                    ast::TermInfixOp::Add => write!(f, "+"),
                    ast::TermInfixOp::Sub => write!(f, "-"),
                    ast::TermInfixOp::LeftShift => write!(f, "<<"),
                    ast::TermInfixOp::RightShift => write!(f, ">>"),
                    ast::TermInfixOp::Lt => write!(f, "<"),
                    ast::TermInfixOp::Gt => write!(f, ">"),
                    ast::TermInfixOp::Le => write!(f, "<="),
                    ast::TermInfixOp::Ge => write!(f, ">="),
                    ast::TermInfixOp::Eq => write!(f, "=="),
                    ast::TermInfixOp::Ne => write!(f, "!="),
                    ast::TermInfixOp::BitAnd => write!(f, "&"),
                    ast::TermInfixOp::BitXor => write!(f, "^"),
                    ast::TermInfixOp::BitOr => write!(f, "|"),
                    ast::TermInfixOp::And => write!(f, "&&"),
                    ast::TermInfixOp::Or => write!(f, "||"),
                    ast::TermInfixOp::Coalescing => write!(f, "!"),
                    ast::TermInfixOp::RightPipeline => write!(f, "|>"),
                    ast::TermInfixOp::LeftPipeline => write!(f, "<|"),
                    ast::TermInfixOp::Assign => write!(f, "="),
                },
            OperationKind::FactorInfixOp(x) =>
                match x {
                    ast::FactorInfixOp::TyAccess => write!(f, "!"),
                    ast::FactorInfixOp::Access => write!(f, "!"),
                    ast::FactorInfixOp::CoalescingAccess => write!(f, "!"),
                    ast::FactorInfixOp::EvalFn => write!(f, "!"),
                    ast::FactorInfixOp::EvalSpreadFn => write!(f, "!"),
                    ast::FactorInfixOp::EvalKey => write!(f, "!"),
                },
        }
    }
}

impl Operation {
    pub fn new_or_get<'input>(
        context: &Context<'input>,
        ty: Rc<Ty>,
        op: OperationKind,
        op_methods: HashMap<&'static str, Rc<Method>>,
        op_literals: HashMap<&'static str, Rc<Literal>>,
    ) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.operation_store.next_id(),
            ty,
            op,
            op_methods,
            op_literals,
        });
        
        let key = value.to_key();
        match key.clone().get_value(context) {
            Ok(x) => return x,
            Err(_) => (),
        }

        context.operation_store.add(key, value.clone()).unwrap();
        value
    }
}

impl OperationKey {
    pub fn new(
        ty: TyKey,
        op: OperationKind,
    ) -> Self {
        Self {
            ty,
            op,
        }
    }
}
