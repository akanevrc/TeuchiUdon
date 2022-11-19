use itertools::Itertools;
use crate::context::Context;
use crate::semantics::elements::{
    element::ValueElement,
    named_methods::{
        NamedMethods,
        NamedMethodsKey,
    },
};

impl Context {
    pub fn register_named_methods(&self) -> Result<(), Vec<String>> {
        let kv =
            self.method_store.values()
            .into_group_map_by(|x| NamedMethodsKey::new(x.ty.to_key(), x.name.clone()));
        for methods in kv.into_values() {
            NamedMethods::from_methods(self, methods)
                .map_err(|x| vec![x.message])?;
        }
        Ok(())
    }
}
