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
        self.qualify("::")
    }
}

impl Qual {
    pub const TOP: Self = Self {
        scopes: Vec::new(),
    };

    pub fn qualify(&self, sep: &'static str) -> String {
        let desc = self.scopes.iter().map(|x| x.description()).collect::<Vec<_>>().join(sep);
        if desc.len() == 0 { desc } else { desc + sep }
    }
}
