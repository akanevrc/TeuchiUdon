use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    element::ValueElement,
    literal::Literal,
    qual::Qual,
    ty::Ty,
    var::VarKey,
};

#[derive(Clone, Debug)]
pub struct ValuedVar {
    pub id: usize,
    pub qual: Rc<Qual>,
    pub name: String,
    pub ty: Rc<Ty>,
    pub literal: Rc<Literal>,
}

impl_key_value_elements!(
    VarKey,
    ValuedVar,
    VarKey {
        qual: self.qual.to_key(),
        name: self.name.clone()
    },
    valued_var_store
);

impl ValuedVar {
    pub fn new<'input>(
        context: &Context<'input>,
        qual: Rc<Qual>,
        name: String,
        ty: Rc<Ty>,
        literal: Rc<Literal>,
    ) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.valued_var_store.next_id(),
            qual,
            name,
            ty,
            literal,
        });
        let key = value.to_key();
        context.valued_var_store.add(key, value.clone())?;
        Ok(value)
    }
}
