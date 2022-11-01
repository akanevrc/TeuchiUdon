use super::Context;
use crate::semantics::elements::{
    element::ValueElement,
    qual::Qual,
    ty::Ty,
};

pub fn register_default_tys(context: &Context) {
    let value = Ty::new(
        context,
        Qual::TOP,
        "int".to_owned(),
        Vec::new(),
        "SystemInt32".to_owned(),
        "SystemInt32".to_owned()
    );
    let key = value.to_key();
    context.ty_store.add(key, value);
}
