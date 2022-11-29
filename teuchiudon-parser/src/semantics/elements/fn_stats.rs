use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use crate::semantics::ast;
use super::{
    ElementError,
    element::{
        KeyElement,
        SemanticElement,
        ValueElement,
    },
    qual::{
        Qual,
        QualKey,
    },
    ty::Ty,
    var::Var,
};

#[derive(Clone, Debug)]
pub struct FnStats<'input> {
    pub id: usize,
    pub qual: Rc<Qual>,
    pub name: String,
    pub ty: Rc<Ty>,
    pub vars: Vec<Rc<Var>>,
    pub stats: Rc<ast::StatsBlock<'input>>,
}

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub struct FnKey {
    pub qual: QualKey,
    pub name: String,
}

impl_key_value_elements!(
    FnKey,
    FnStats<'input>,
    FnKey {
        qual: self.qual.to_key(),
        name: self.name.clone()
    },
    fn_stats_store
);

impl SemanticElement for FnKey {
    fn description(&self) -> String {
        format!(
            "{}{}",
            self.qual.qualify_description("::"),
            self.name.description()
        )
    }

    fn logical_name(&self) -> String {
        format!(
            "{}{}",
            self.qual.qualify_logical_name(">"),
            self.name.logical_name()
        )
    }
}

impl<'input> FnStats<'input> {
    pub fn new_or_get(
        context: &Context<'input>,
        qual: Rc<Qual>,
        name: String,
        ty: Rc<Ty>,
        vars: Vec<Rc<Var>>,
        stats: Rc<ast::StatsBlock<'input>>,
    ) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.fn_stats_store.next_id(),
            qual: qual.clone(),
            name: name.clone(),
            ty,
            vars,
            stats,
        });

        let key = value.to_key();
        match key.clone().get_value(context) {
            Ok(x) => return Ok(x),
            Err(_) => (),
        }
        context.fn_stats_store.add(key.clone(), value.clone()).unwrap();

        let fn_ty = Ty::new_or_get_function_from_key(context, key)?;
        Var::force_new(context, qual, name, fn_ty, false, None);
        Ok(value)
    }

    pub fn get(
        context: &Context<'input>,
        qual: QualKey,
        name: String,
    ) -> Result<Rc<Self>, ElementError> {
        FnKey::new(qual, name).get_value(context)
    }
}

impl FnKey {
    pub fn new(qual: QualKey, name: String) -> Self {
        Self {
            qual,
            name,
        }
    }
}
