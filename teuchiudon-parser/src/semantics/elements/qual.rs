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
    pub id: usize,
    pub scopes: Vec<Scope>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct QualKey {
    pub scopes: Vec<Scope>,
}

impl SemanticElement for Qual {
    fn description(&self) -> String {
        self.to_key().description()
    }

    fn logical_name(&self) -> String {
        self.to_key().logical_name()
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
        self.qualify_description_self("::")
    }

    fn logical_name(&self) -> String {
        self.qualify_logical_name_self(">")
    }
}

impl<'input> KeyElement<'input, Qual> for QualKey {
    fn get_value(&self, context: &Context<'input>) -> Result<Rc<Qual>, ElementError> {
        context.qual_store.get(self)
    }
}

impl Qual {
    pub fn top<'input>(
        context: &Context<'input>
    ) -> Rc<Self> {
        Self::new_or_get(context, Vec::new())
    }

    fn new_or_get_one<'input>(
        context: &Context<'input>,
        scopes: Vec<Scope>
    ) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.qual_store.next_id(),
            scopes: scopes.clone(),
        });
        let key = value.to_key();
        context.qual_store.add(key.clone(), value.clone()).ok();
        value
    }

    pub fn new_or_get<'input>(
        context: &Context<'input>,
        scopes: Vec<Scope>
    ) -> Rc<Self> {
        for n in 1..scopes.len() {
            Self::new_or_get_one(context, scopes.clone().into_iter().take(n).collect());
        }
        Self::new_or_get_one(context, scopes)
    }

    pub fn new_or_get_quals<'input>(
        context: &Context<'input>,
        quals: Vec<String>
    ) -> Rc<Self> {
        Self::new_or_get(context, quals.into_iter().map(|x| Scope::Qual(x)).collect())
    }

    pub fn new_or_get_pushed<'input>(
        &self,
        context: &Context<'input>,
        scope: Scope
    ) -> Rc<Self> {
        let mut cloned = self.scopes.clone();
        cloned.push(scope);
        Self::new_or_get_one(context, cloned)
    }

    pub fn new_or_get_pushed_qual<'input>(
        &self,
        context: &Context<'input>,
        qual: String
    ) -> Rc<Self> {
        self.new_or_get_pushed(context, Scope::Qual(qual))
    }

    pub fn new_or_get_popped<'input>(
        &self,
        context: &Context<'input>
    ) -> Rc<Self> {
        let mut cloned = self.scopes.clone();
        cloned.pop();
        Self::new_or_get_one(context, cloned)
    }

    pub fn get<'input>(
        context: &Context<'input>,
        scopes: Vec<Scope>
    ) -> Result<Rc<Self>, ElementError> {
        QualKey::new(scopes).get_value(context)
    }

    pub fn get_pushed<'input>(
        &self,
        context: &Context<'input>,
        scope: Scope
    ) -> Result<Rc<Self>, ElementError> {
        let mut cloned = self.scopes.clone();
        cloned.push(scope);
        Self::get(context, cloned)
    }

    pub fn get_pushed_qual<'input>(
        &self,
        context: &Context<'input>,
        qual: String
    ) -> Result<Rc<Self>, ElementError> {
        self.get_pushed(context, Scope::Qual(qual))
    }

    pub fn get_popped<'input>(
        &self,
        context: &Context<'input>
    ) -> Result<Rc<Self>, ElementError> {
        let mut cloned = self.scopes.clone();
        cloned.pop();
        Self::get(context, cloned)
    }

    pub fn qualify_description_self(&self, sep: &str) -> String {
        self.to_key().qualify_description_self(sep)
    }

    pub fn qualify_description(&self, sep: &str) -> String {
        self.to_key().qualify_description(sep)
    }

    pub fn qualify_logical_name_self(&self, sep: &str) -> String {
        self.to_key().qualify_logical_name_self(sep)
    }

    pub fn qualify_logical_name(&self, sep: &str) -> String {
        self.to_key().qualify_logical_name(sep)
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

    pub fn pushed(&self, scope: Scope) -> Self {
        let mut cloned = self.scopes.clone();
        cloned.push(scope);
        Self::new(cloned)
    }

    pub fn pushed_qual(&self, qual: String) -> Self {
        Self::pushed(&self, Scope::Qual(qual))
    }

    pub fn popped(&self) -> Self {
        let mut cloned = self.scopes.clone();
        cloned.pop();
        Self::new(cloned)
    }

    pub fn qualify_description_self(&self, sep: &str) -> String {
        self.scopes.iter().map(|x| x.description()).collect::<Vec<_>>().join(sep)
    }

    pub fn qualify_description(&self, sep: &str) -> String {
        let q = self.qualify_description_self(sep);
        if q.len() == 0 { q } else { q + sep }
    }

    pub fn qualify_logical_name_self(&self, sep: &str) -> String {
        self.scopes.iter().map(|x| x.logical_name()).collect::<Vec<_>>().join(sep)
    }

    pub fn qualify_logical_name(&self, sep: &str) -> String {
        let q = self.qualify_logical_name_self(sep);
        if q.len() == 0 { q } else { q + sep }
    }
}
