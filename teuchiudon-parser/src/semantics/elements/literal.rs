use std::{
    collections::HashMap,
    rc::Rc,
};
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

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
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
    pub fn new_or_get<'input>(
        context: &Context<'input>,
        text: String,
        ty: Rc<Ty>
    ) -> Rc<Self> {
        let value = Rc::new(Self {
            id: context.literal_store.next_id(),
            text,
            ty,
        });

        let key = value.to_key();
        match key.clone().get_value(context) {
            Ok(x) => return x,
            Err(_) => (),
        }

        context.literal_store.add(key, value.clone()).unwrap();
        value
    }

    pub fn new_or_get_unit<'input>(
        context: &Context<'input>
    ) -> Result<Rc<Self>, ElementError> {
        let ty = Ty::get_from_name(context, "unit")?;
        Ok(Self::new_or_get(context, "()".to_owned(), ty))
    }

    pub fn new_or_get_null<'input>(
        context: &Context<'input>
    ) -> Result<Rc<Self>, ElementError> {
        let ty = Ty::get_from_name(context, "nulltype")?;
        Ok(Self::new_or_get(context, "null".to_owned(), ty))
    }

    pub fn new_or_get_bool<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = Ty::get_from_name(context, "bool")?;
        Ok(Self::new_or_get(context, text, ty))
    }

    pub fn new_or_get_pure_integer<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = Ty::get_from_name(context, "int")?;
        Ok(Self::new_or_get(context, text, ty))
    }

    pub fn new_or_get_dec_integer<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_integer_text(text);
        let ty = Ty::get_from_name(context, ty_name)?;
        Ok(Self::new_or_get(context, text, ty))
    }

    pub fn new_or_get_hex_integer<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_integer_text(text);
        let ty = Ty::get_from_name(context, ty_name)?;
        Ok(Self::new_or_get(context, text, ty))
    }

    pub fn new_or_get_bin_integer<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_integer_text(text);
        let ty = Ty::get_from_name(context, ty_name)?;
        Ok(Self::new_or_get(context, text, ty))
    }

    pub fn new_or_get_real_number<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let (text, ty_name) = Self::trim_real_number_text(text);
        let ty = Ty::get_from_name(context, ty_name)?;
        Ok(Self::new_or_get(context, text, ty))
    }

    pub fn new_or_get_character<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = Ty::get_from_name(context, "char")?;
        Ok(Self::new_or_get(context, text, ty))
    }

    pub fn new_or_get_regular_string<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = Ty::get_from_name(context, "string")?;
        let text = format!("\"{}\"", text);
        Ok(Self::new_or_get(context, text, ty))
    }

    pub fn new_or_get_verbatium_string<'input>(
        context: &Context<'input>,
        text: String
    ) -> Result<Rc<Self>, ElementError> {
        let ty = Ty::get_from_name(context, "string")?;
        let text = format!("@\"{}\"", text);
        Ok(Self::new_or_get(context, text, ty))
    }

    fn trim_integer_text(text: String) -> (String, &'static str) {
        let text = Self::formatted_number(text);
        match &text.chars().collect::<Vec<_>>()[..] {
            [.., x, y]
                if
                    x.to_uppercase().to_string().as_str() == "L" && y.to_uppercase().to_string().as_str() == "U" ||
                    x.to_uppercase().to_string().as_str() == "U" && y.to_uppercase().to_string().as_str() == "L"
                =>
                    (text[0..text.len() - 2].to_owned(), "ulong"),
            [.., x] if x.to_uppercase().to_string().as_str() == "L" =>
                (text[0..text.len() - 1].to_owned(), "long"),
            [.., x] if x.to_uppercase().to_string().as_str() == "U" =>
                (text[0..text.len() - 1].to_owned(), "uint"),
            _ =>
                (text, "int"),
        }
    }

    fn trim_real_number_text(text: String) -> (String, &'static str) {
        let text = Self::formatted_number(text);
        match &text.chars().collect::<Vec<_>>()[..] {
            [.., x] if x.to_uppercase().to_string().as_str() == "F" =>
                (text[0..text.len() - 1].to_owned(), "float"),
            [.., x] if x.to_uppercase().to_string().as_str() == "D" =>
                (text[0..text.len() - 1].to_owned(), "double"),
            [.., x] if x.to_uppercase().to_string().as_str() == "M" =>
                (text[0..text.len() - 1].to_owned(), "decimal"),
            _ =>
                (text, "float"),
        }
    }

    fn formatted_number(text: String) -> String {
        text.replace("_", "")
    }

    pub fn new_or_get_term_prefix_op_literals<'input>(
        context: &Context<'input>,
        op: &ast::TermPrefixOp,
        ty: Rc<Ty>
    ) -> Result<HashMap<&'static str, Rc<Self>>, ElementError> {
        match op {
            ast::TermPrefixOp::Tilde =>
                Ok(vec![("mask", Self::new_or_get_mask(context, ty)?)].into_iter().collect()),
            _ =>
                Ok(HashMap::new()),
        }
    }

    pub fn get_term_infix_op_literals<'input>(
        _context: &Context<'input>,
        op: &ast::TermInfixOp,
        _left_ty: Rc<Ty>,
        _right_ty: Rc<Ty>
    ) -> Result<HashMap<&'static str, Rc<Self>>, ElementError> {
        match op {
            ast::TermInfixOp::Mul |
            ast::TermInfixOp::Div |
            ast::TermInfixOp::Mod |
            ast::TermInfixOp::Add |
            ast::TermInfixOp::Sub |
            ast::TermInfixOp::LeftShift |
            ast::TermInfixOp::RightShift =>
                Ok(HashMap::new()),
            _ =>
                panic!("Not implemented"),
        }
    }

    pub fn get_factor_infix_op_literals<'input>(
        _context: &Context<'input>,
        op: &ast::FactorInfixOp,
        _left_ty: Rc<Ty>,
        _right_ty: Rc<Ty>
    ) -> Result<HashMap<&'static str, Rc<Self>>, ElementError> {
        match op {
            ast::FactorInfixOp::TyAccess |
            ast::FactorInfixOp::Access |
            ast::FactorInfixOp::EvalFn =>
                Ok(HashMap::new()),
            _ =>
                panic!("Not implemented"),
        }
    }

    fn new_or_get_mask(context: &Context, ty: Rc<Ty>) -> Result<Rc<Self>, ElementError> {
        let text = match ty.logical_name.as_str() {
            "SystemBoolean" => "true",
            "SystemByte" => "0xFF",
            "SystemSByte" => "0xFF",
            "SystemInt16" => "0xFFFF",
            "SystemUInt16" => "0xFFFF",
            "SystemInt32" => "0xFFFFFFFF",
            "SystemUInt32" => "0xFFFFFFFF",
            "SystemInt64" => "0xFFFFFFFFFFFFFFFF",
            "SystemUInt64" => "0xFFFFFFFFFFFFFFFF",
            _ => return Err(ElementError::new("Type `{}` is not compatible with this operation".to_owned()))
        };
        Ok(Self::new_or_get(context, text.to_owned(), ty))
    }
}
