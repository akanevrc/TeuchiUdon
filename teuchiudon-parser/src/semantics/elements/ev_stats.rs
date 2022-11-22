use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::Context;
use crate::semantics::ast;
use super::{
    ElementError,
    element::ValueElement,
    ev::EvKey,
};

#[derive(Clone, Debug)]
pub struct EvStats<'input> {
    pub id: usize,
    pub name: String,
    pub stats: Rc<ast::StatsBlock<'input>>,
}

impl_key_value_elements!(
    EvKey,
    EvStats<'input>,
    EvKey {
        name: self.name.clone()
    },
    ev_stats_store
);

impl<'input> EvStats<'input> {
    pub fn new(
        context: &Context<'input>,
        name: String,
        stats: Rc<ast::StatsBlock<'input>>,
    ) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.ev_stats_store.next_id(),
            name,
            stats,
        });
        let key = value.to_key();
        context.ev_stats_store.add(key, value.clone())?;
        Ok(value)
    }
}
