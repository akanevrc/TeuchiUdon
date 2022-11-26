use std::rc::Rc;
use crate::Context;
use crate::semantics::ast;

#[derive(Clone, Debug)]
pub struct TopStat<'input> {
    pub id: usize,
    pub stat: Rc<ast::TopStat<'input>>,
}

impl<'input> TopStat<'input> {
    pub fn new(
        context: &Context<'input>,
        stat: Rc<ast::TopStat<'input>>,
    ) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.top_stat_store.next_id(),
            stat,
        });
        context.top_stat_store.push(value.clone());
        value
    }
}
