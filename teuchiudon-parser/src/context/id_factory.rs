use std::cell::RefCell;

pub struct IdFactory {
    id: RefCell<usize>,
}

impl IdFactory {
    pub fn new() -> Self {
        Self {
            id: RefCell::new(0),
        }
    }

    pub fn next_id(&self) -> usize {
        let mut id = self.id.borrow_mut();
        let ret = *id;
        *id += 1;
        ret
    }
}
