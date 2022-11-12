use std::{
    cell::RefCell,
    collections::HashMap,
    hash::Hash,
    rc::Rc,
};
use crate::semantics::elements::{
    ElementError,
    element::SemanticElement,
};

pub struct Store<Key, Value>
where
    Key: Clone + Eq + Hash + SemanticElement,
{
    id_map: RefCell<HashMap<Key, usize>>,
    values: RefCell<Vec<Rc<Value>>>,
    not_found: fn(&Key) -> String,
}

impl<Key, Value> Store<Key, Value>
where
    Key: Clone + Eq + Hash + SemanticElement,
{
    pub fn new(not_found: fn(&Key) -> String) -> Self {
        Self {
            id_map: RefCell::new(HashMap::new()),
            values: RefCell::new(Vec::new()),
            not_found,
        }
    }

    pub fn next_id(&self) -> usize {
        self.values.borrow().len()
    }

    pub fn add(&self, key: Key, value: Rc<Value>) -> Result<(), ElementError> {
        let len = self.values.borrow().len();
        let mut id_map = self.id_map.borrow_mut();
        if id_map.contains_key(&key) {
            return Err(ElementError::new(format!("Registration duplicated: `{}`", key.description())));
        }
        id_map.insert(key, len);
        self.values.borrow_mut().push(value);
        Ok(())
    }

    pub fn get(&self, key: &Key) -> Result<Rc<Value>, ElementError> {
        self.id_map.borrow().get(key).map(|x| self.values.borrow()[*x].clone()).ok_or(ElementError::new((self.not_found)(key)))
    }

    pub fn values(&self) -> impl Iterator<Item = Rc<Value>> {
        self.values.borrow().iter().cloned().collect::<Vec<_>>().into_iter()
    }
}
