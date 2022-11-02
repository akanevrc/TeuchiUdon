use std::vec::IntoIter;
use nom_supreme::error::{
    GenericErrorTree,
    StackContext,
};
use crate::semantics::SemanticError;
use super::{
    ErrorTree,
    ErrorTreeIter,
};

pub trait HasContextIter<'input: 'error, 'error> {
    type ContextIter: Iterator<Item = (&'input str, &'error String)>;
    fn context_iter(&'error self) -> Self::ContextIter;
}

impl<'input: 'error, 'error> HasContextIter<'input, 'error> for ErrorTree<'input> {
    type ContextIter = IntoIter<(&'input str, &'error String)>;

    fn context_iter(&'error self) -> Self::ContextIter {
        ErrorTreeIter::new(self)
        .filter_map(|x|
            if let GenericErrorTree::Stack { base: _, contexts } = x { Some(contexts) } else { None }
        )
        .flat_map(|x| x)
        .filter_map(|x|
            if let StackContext::Context(s) = &x.1 { Some((x.0, s)) } else { None }
        )
        .collect::<Vec<_>>()
        .into_iter()
    }
}

impl<'input: 'error, 'error> HasContextIter<'input, 'error> for Vec<SemanticError<'input>> {
    type ContextIter = IntoIter<(&'input str, &'error String)>;

    fn context_iter(&'error self) -> Self::ContextIter {
        self.iter()
        .map(|x| (x.slice.unwrap_or(""), &x.message))
        .collect::<Vec<_>>()
        .into_iter()
    }
}
