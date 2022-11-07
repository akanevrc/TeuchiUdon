pub mod json;
pub mod keyword;
pub mod op_code;
pub mod semantic_op;
pub mod semantic_ty_op;
pub mod store;
pub mod ty_store;

use self::{
    json::register_from_json,
    keyword::KeywordContext,
    op_code::OpCodeContext,
    semantic_op::SemanticOpContext,
    semantic_ty_op::SemanticTyOpContext,
    store::Store,
    ty_store::register_default_tys,
};
use crate::semantics::elements::{
    ElementError,
    ty::{
        BaseTy,
        BaseTyKey,
    },
    literal::{
        Literal,
        LiteralKey,
    },
    var::{
        Var,
        VarKey,
    },
};

pub struct Context {
    pub keyword: KeywordContext,
    pub op_code: OpCodeContext,
    pub semantic_op: SemanticOpContext,
    pub semantic_ty_op: SemanticTyOpContext,
    pub literal_store: Store<LiteralKey, Literal>,
    pub ty_store: Store<BaseTyKey, BaseTy>,
    pub var_store: Store<VarKey, Var>,
}

impl Context {
    pub fn new() -> Self {
        let context = Self {
            keyword: KeywordContext::new(),
            op_code: OpCodeContext::new(),
            semantic_op: SemanticOpContext::new(),
            semantic_ty_op: SemanticTyOpContext::new(),
            literal_store: Store::new(ElementError::LiteralNotFound),
            ty_store: Store::new(ElementError::TyNotFound),
            var_store: Store::new(ElementError::VarNotFound),
        };
        register_default_tys(&context);
        context
    }

    pub fn new_with_json(json: String) -> Result<Self, Vec<String>> {
        let context = Self::new();
        register_from_json(&context, json)?;
        Ok(context)
    }
}
