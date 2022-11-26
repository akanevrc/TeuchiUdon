use std::{
    cell::RefCell,
    rc::Rc,
};

pub struct VecStore<Value> {
    values: RefCell<Vec<Rc<Value>>>,
}

impl<Value> VecStore<Value> {
    pub fn new() -> Self {
        Self {
            values: RefCell::new(Vec::new()),
        }
    }

    pub fn next_id(&self) -> usize {
        self.values.borrow().len()
    }

    pub fn push(&self, value: Rc<Value>) {
        self.values.borrow_mut().push(value);
    }

    pub fn values(&self) -> impl Iterator<Item = Rc<Value>> {
        self.values.borrow().iter().cloned().collect::<Vec<_>>().into_iter()
    }
}
