use itertools::Itertools;
use super::{
    element::SemanticElement,
    scope::Scope,
};

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct Qual {
    pub scopes: Vec<Scope>,
}

impl SemanticElement for Qual {
    fn description(&self) -> String {
        self.scopes.iter().map(|x| x.description()).join("::")
    }
}

impl Qual {
    pub const TOP: Self = Self {
        scopes: Vec::new(),
    };
}
