pub mod convert;

use std::error::Error;
use nom_supreme::error::GenericErrorTree;

pub type ErrorTree<'input> = GenericErrorTree<&'input str, &'static str, String, Box<dyn Error + Send + Sync + 'static>>;

#[derive(Clone, Debug)]
pub struct ErrorTreeIter<'input: 'error, 'error> {
    stack: Vec<&'error ErrorTree<'input>>,
}

impl<'input: 'error, 'error> ErrorTreeIter<'input, 'error> {
    pub fn new(error: &'error ErrorTree<'input>) -> Self {
        let mut stack = Vec::new();
        stack.push(error);
        Self { stack }
    }
}

impl<'input: 'error, 'error> Iterator for ErrorTreeIter<'input, 'error> {
    type Item = &'error ErrorTree<'input>;

    fn next(&mut self) -> Option<Self::Item> {
        match self.stack.pop() {
            Some(error) => {
                match error {
                    GenericErrorTree::Base { location: _, kind: _ } => (),
                    GenericErrorTree::Stack { base, contexts: _ } => {
                        self.stack.push(base);
                    },
                    GenericErrorTree::Alt(v) => {
                        for e in v.iter().rev() {
                            self.stack.push(e);
                        }
                    }
                }
                Some(error)
            },
            None => None,
        }
    }
}
