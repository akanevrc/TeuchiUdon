use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    base_ty::BaseTy,
    element::ValueElement,
    ty::Ty,
};

#[derive(Clone, Debug)]
pub struct Literal {
    pub id: usize,
    pub text: String,
    pub ty: Rc<Ty>,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct LiteralKey {
    pub text: String,
    pub ty: Rc<Ty>,
}

impl_key_value_elements!(
    LiteralKey,
    Literal,
    LiteralKey {
        text: self.text.clone(),
        ty: self.ty.clone()
    },
    format!(
        "'{}': {}",
        self.text.description(),
        self.ty.description()
    ),
    literal_store
);

impl Literal {
    pub fn new(context: &Context, text: String, ty: Rc<Ty>) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.literal_store.next_id(),
            text,
            ty,
        });
        let key = value.to_key();
        context.literal_store.add(key, value.clone())?;
        Ok(value)
    }

    pub fn new_unit(context: &Context) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "unit".to_owned())?.new_or_get_applied_zero(context)?;
        Self::new(context, "()".to_owned(), ty)
    }

    pub fn new_null(context: &Context) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "nulltype".to_owned())?.new_or_get_applied_zero(context)?;
        Self::new(context, "null".to_owned(), ty)
    }

    pub fn new_bool(context: &Context, text: String) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "bool".to_owned())?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_pure_integer(context: &Context, text: String) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "int".to_owned())?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_dec_integer(context: &Context, text: String) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_integer_text(text);
        let ty = BaseTy::get_from_name(context, ty_name)?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_hex_integer(context: &Context, text: String) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_integer_text(text);
        let ty = BaseTy::get_from_name(context, ty_name)?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_bin_integer(context: &Context, text: String) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_integer_text(text);
        let ty = BaseTy::get_from_name(context, ty_name)?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_real_number(context: &Context, text: String) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_real_number_text(text);
        let ty = BaseTy::get_from_name(context, ty_name)?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_character(context: &Context, text: String) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "char".to_owned())?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_regular_string(context: &Context, text: String) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "string".to_owned())?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_verbatium_string(context: &Context, text: String) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "string".to_owned())?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    fn trim_integer_text(text: String) -> (String, String) {
        let text = Self::formatted_number(text);
        match &text.chars().collect::<Vec<_>>()[..] {
            [.., x, y] if *x == 'L' && *y == 'U' || *x == 'U' && *y == 'L' =>
                (text[0..text.len() - 2].to_owned(), "ulong".to_owned()),
            [.., x] if *x == 'L' =>
                (text[0..text.len() - 1].to_owned(), "long".to_owned()),
            [.., x] if *x == 'U' =>
                (text[0..text.len() - 1].to_owned(), "uint".to_owned()),
            _ =>
                (text, "int".to_owned()),
        }
    }

    fn trim_real_number_text(text: String) -> (String, String) {
        let text = Self::formatted_number(text);
        match &text.chars().collect::<Vec<_>>()[..] {
            [.., x] if *x == 'F' =>
                (text[0..text.len() - 1].to_owned(), "float".to_owned()),
            [.., x] if *x == 'D' =>
                (text[0..text.len() - 1].to_owned(), "double".to_owned()),
            [.., x] if *x == 'M' =>
                (text[0..text.len() - 1].to_owned(), "decimal".to_owned()),
            _ =>
                (text, "float".to_owned()),
        }
    }

    fn formatted_number(text: String) -> String {
        text.to_uppercase().replace("_", "")
    }
}
