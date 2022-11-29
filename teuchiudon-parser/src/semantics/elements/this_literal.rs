use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    ty::{
        Ty,
        TyLogicalKey,
    },
};

#[derive(Clone, Debug)]
pub struct ThisLiteral {
    pub id: usize,
    pub ty: Rc<Ty>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct ThisLiteralKey {
    pub ty: TyLogicalKey,
}

impl_key_value_elements!(
    ThisLiteralKey,
    ThisLiteral,
    ThisLiteralKey {
        ty: self.ty.to_key()
    },
    this_literal_store
);

impl SemanticElement for ThisLiteralKey {
    fn description(&self) -> String {
        format!(
            "'this': {}",
            self.ty.description()
        )
    }

    fn logical_name(&self) -> String {
        format!(
            "literal[{}][this]",
            self.ty.logical_name()
        )
    }
}

impl ThisLiteral {
    pub fn new_or_get<'input>(
        context: &Context<'input>,
        ty: Rc<Ty>
    ) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.this_literal_store.next_id(),
            ty,
        });

        let key = value.to_key();
        match key.clone().get_value(context) {
            Ok(x) => return x,
            Err(_) => (),
        }

        context.this_literal_store.add(key, value.clone()).unwrap();
        value
    }
}
