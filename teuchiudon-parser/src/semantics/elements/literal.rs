use std::rc::Rc;
use crate::impl_key_value_elements;
use crate::context::Context;
use super::{
    ElementError,
    base_ty::BaseTy,
    element::{
        SemanticElement,
        ValueElement,
    },
    ty::{
        Ty,
        TyLogicalKey,
    },
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
    pub ty: TyLogicalKey,
}

impl_key_value_elements!(
    LiteralKey,
    Literal,
    LiteralKey {
        text: self.text.clone(),
        ty: self.ty.to_key()
    },
    literal_store
);

impl SemanticElement for LiteralKey {
    fn description(&self) -> String {
        format!(
            "'{}': {}",
            self.text.description(),
            self.ty.description()
        )
    }

    fn logical_name(&self) -> String {
        format!(
            "literal[{}][{}]",
            self.ty.logical_name(),
            self.text.logical_name()
        )
    }
}

impl Literal {
    pub fn new<'input>(
        context: &Context<'input>,
        text: String,
        ty: Rc<Ty>
    ) -> Result<Rc<Self>, ElementError> {
        let value = Rc::new(Self {
            id: context.literal_store.next_id(),
            text,
            ty,
        });
        let key = value.to_key();
        context.literal_store.add(key, value.clone())?;
        Ok(value)
    }

    pub fn new_unit<'input>(
        context: &Context<'input>
    ) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "unit")?.new_or_get_applied_zero(context)?;
        Self::new(context, "()".to_owned(), ty)
    }

    pub fn new_null<'input>(
        context: &Context<'input>
    ) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "nulltype")?.new_or_get_applied_zero(context)?;
        Self::new(context, "null".to_owned(), ty)
    }

    pub fn new_bool<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "bool")?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_pure_integer<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "int")?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_dec_integer<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_integer_text(text);
        let ty = BaseTy::get_from_name(context, ty_name)?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_hex_integer<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_integer_text(text);
        let ty = BaseTy::get_from_name(context, ty_name)?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_bin_integer<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_integer_text(text);
        let ty = BaseTy::get_from_name(context, ty_name)?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_real_number<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_real_number_text(text);
        let ty = BaseTy::get_from_name(context, ty_name)?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_character<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "char")?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_regular_string<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "string")?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    pub fn new_verbatium_string<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = BaseTy::get_from_name(context, "string")?.new_or_get_applied_zero(context)?;
        Self::new(context, text, ty)
    }

    fn trim_integer_text(text: String) -> (String, &'static str) {
        let text = Self::formatted_number(text);
        match &text.chars().collect::<Vec<_>>()[..] {
            [.., x, y] if *x == 'L' && *y == 'U' || *x == 'U' && *y == 'L' =>
                (text[0..text.len() - 2].to_owned(), "ulong"),
            [.., x] if *x == 'L' =>
                (text[0..text.len() - 1].to_owned(), "long"),
            [.., x] if *x == 'U' =>
                (text[0..text.len() - 1].to_owned(), "uint"),
            _ =>
                (text, "int"),
        }
    }

    fn trim_real_number_text(text: String) -> (String, &'static str) {
        let text = Self::formatted_number(text);
        match &text.chars().collect::<Vec<_>>()[..] {
            [.., x] if *x == 'F' =>
                (text[0..text.len() - 1].to_owned(), "float"),
            [.., x] if *x == 'D' =>
                (text[0..text.len() - 1].to_owned(), "double"),
            [.., x] if *x == 'M' =>
                (text[0..text.len() - 1].to_owned(), "decimal"),
            _ =>
                (text, "float"),
        }
    }

    fn formatted_number(text: String) -> String {
        text.to_uppercase().replace("_", "")
    }
}
