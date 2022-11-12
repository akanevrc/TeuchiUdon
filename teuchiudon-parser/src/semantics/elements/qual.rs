use std::rc::Rc;
use crate::context::Context;
use super::{
    ElementError,
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    scope::Scope,
};

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct Qual {
    pub scopes: Vec<Scope>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct QualKey {
    pub scopes: Vec<Scope>,
}

impl SemanticElement for Qual {
    fn description(&self) -> String {
        self.qualify("::")
    }
}

impl ValueElement<QualKey> for Qual {
    fn to_key(&self) -> QualKey {
        QualKey {
            scopes: self.scopes.clone(),
        }
    }
}

impl SemanticElement for QualKey {
    fn description(&self) -> String {
        self.qualify_self("::")
    }
}

impl KeyElement<Qual> for QualKey {
    fn get_value(&self, context: &Context) -> Result<Rc<Qual>, super::ElementError> {
        context.qual_store.get(self)
    }
}

impl Qual {
    pub fn top(context: &Context) -> Rc<Self> {
        Self::new_or_get(context, Vec::new())
    }

    fn new_or_get_one(context: &Context, scopes: Vec<Scope>) -> Rc<Self> {
        let qual = Rc::new(Self {
            scopes
        });
        let key = qual.to_key();
        context.qual_store.add(key, qual.clone()).ok();
        qual
    }

    pub fn new_or_get(context: &Context, scopes: Vec<Scope>) -> Rc<Self> {
        for n in 1..scopes.len() {
            Self::new_or_get_one(context, scopes.clone().into_iter().take(n).collect());
        }
        Self::new_or_get_one(context, scopes)
    }

    pub fn new_or_get_quals(context: &Context, quals: Vec<String>) -> Rc<Self> {
        Self::new_or_get(context, quals.into_iter().map(|x| Scope::Qual(x)).collect())
    }

    pub fn new_or_get_added(&self, context: &Context, scope: Scope) -> Rc<Self> {
        let mut cloned = self.scopes.clone();
        cloned.push(scope);
        Self::new_or_get_one(context, cloned)
    }

    pub fn new_or_get_added_qual(&self, context: &Context, qual: String) -> Rc<Self> {
        self.new_or_get_added(context, Scope::Qual(qual))
    }

    pub fn get(context: &Context, scopes: Vec<Scope>) -> Result<Rc<Self>, ElementError> {
        QualKey::new(scopes).get_value(context)
    }

    pub fn get_added(&self, context: &Context, scope: Scope) -> Result<Rc<Self>, ElementError> {
        let mut cloned = self.scopes.clone();
        cloned.push(scope);
        Self::get(context, cloned)
    }

    pub fn get_added_qual(&self, context: &Context, qual: String) -> Result<Rc<Self>, ElementError> {
        self.get_added(context, Scope::Qual(qual))
    }

    pub fn qualify_self(&self, sep: &str) -> String {
        self.to_key().qualify_self(sep)
    }

    pub fn qualify(&self, sep: &str) -> String {
        self.to_key().qualify(sep)
    }
}

impl QualKey {
    pub fn top() -> Self {
        Self::new(Vec::new())
    }

    pub fn new(scopes: Vec<Scope>) -> Self {
        Self {
            scopes,
        }
    }

    pub fn new_quals(quals: Vec<String>) -> Self {
        Self {
            scopes: quals.into_iter().map(|x| Scope::Qual(x)).collect(),
        }
    }

    pub fn added(&self, scope: Scope) -> Self {
        let mut cloned = self.scopes.clone();
        cloned.push(scope);
        Self::new(cloned)
    }

    pub fn added_qual(&self, qual: String) -> Self {
        Self::added(&self, Scope::Qual(qual))
    }

    pub fn qualify_self(&self, sep: &str) -> String {
        self.scopes.iter().map(|x| x.description()).collect::<Vec<_>>().join(sep)
    }

    pub fn qualify(&self, sep: &str) -> String {
        let q = self.qualify_self(sep);
        if q.len() == 0 { q } else { q + sep }
    }
}
