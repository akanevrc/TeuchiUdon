use std::{
    cell::RefCell,
    collections::HashMap,
    hash::Hash,
    rc::Rc,
};
use crate::semantics::elements::ElementError;

#[derive(Debug)]
pub struct Store<Key, Value>
where
    Key: Clone + Eq + Hash,
{
    id_map: RefCell<HashMap<Key, usize>>,
    values: RefCell<Vec<Rc<Value>>>,
    not_found: fn(Key) -> ElementError,
}

impl<Key, Value> Store<Key, Value>
where
    Key: Clone + Eq + Hash,
{
    pub fn new(not_found: fn(Key) -> ElementError) -> Self {
        Self {
            id_map: RefCell::new(HashMap::new()),
            values: RefCell::new(Vec::new()),
            not_found,
        }
    }

    pub fn next_id(&self) -> usize {
        self.values.borrow().len()
    }

    pub fn add(&self, key: Key, value: Rc<Value>) {
        let len = self.values.borrow().len();
        self.id_map.borrow_mut().insert(key, len);
        self.values.borrow_mut().push(value.clone());
    }

    pub fn get(&self, key: Key) -> Option<Rc<Value>> {
        self.id_map.borrow().get(&key).map(|x| self.values.borrow()[*x].clone())
    }

    pub fn get_unwrap(&self, key: Key) -> Result<Rc<Value>, ElementError> {
        self.get(key.clone()).ok_or((self.not_found)(key))
    }
}
