use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    element::{
        KeyElement,
        ValueElement, SemanticElement,
    },
    fn_stats::{
        FnKey,
        FnStats,
    },
    label::DataLabel,
};

#[derive(Clone, Debug)]
pub struct EvalFn<'input> {
    pub id: usize,
    pub fn_stats: Rc<FnStats<'input>>,
    pub data: Vec<Rc<DataLabel>>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct EvalFnKey {
    pub fn_key: FnKey,
    pub data: Vec<Rc<DataLabel>>,
}

impl_key_value_elements!(
    EvalFnKey,
    EvalFn<'input>,
    EvalFnKey {
        fn_key: self.fn_stats.to_key(),
        data: self.data.clone()
    },
    eval_fn_store
);

impl SemanticElement for EvalFnKey {
    fn description(&self) -> String {
        format!("{}", self.fn_key.description())
    }

    fn logical_name(&self) -> String {
        panic!("Illegal state");
    }
}

impl<'input> EvalFn<'input> {
    pub fn new_or_get(
        context: &Context<'input>,
        fn_stats: Rc<FnStats<'input>>,
        data: Vec<Rc<DataLabel>>,
    ) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.eval_fn_store.next_id(),
            fn_stats,
            data,
        });

        let key = value.to_key();
        match key.clone().get_value(context) {
            Ok(x) => return x,
            Err(_) => (),
        }

        context.eval_fn_store.add(key, value.clone()).unwrap();
        value
    }
}
