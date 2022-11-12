pub mod keyword;
pub mod op_code;
pub mod semantic_op;
pub mod semantic_ty_op;
pub mod store;
mod json;
mod ty_store;

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
    base_ty::{
        BaseTy,
        BaseTyKey,
        BaseTyLogicalKey,
    },
    element::SemanticElement,
    literal::{
        Literal,
        LiteralKey,
    },
    qual::{
        Qual,
        QualKey,
    },
    ty::{
        Ty,
        TyKey,
        TyLogicalKey,
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
    pub qual_store: Store<QualKey, Qual>,
    pub base_ty_store: Store<BaseTyKey, BaseTy>,
    pub base_ty_logical_store: Store<BaseTyLogicalKey, BaseTy>,
    pub ty_store: Store<TyKey, Ty>,
    pub ty_logical_store: Store<TyLogicalKey, Ty>,
    pub literal_store: Store<LiteralKey, Literal>,
    pub var_store: Store<VarKey, Var>,
}

impl Context {
    pub fn new() -> Result<Self, Vec<String>> {
        let context = Self {
            keyword: KeywordContext::new(),
            op_code: OpCodeContext::new(),
            semantic_op: SemanticOpContext::new(),
            semantic_ty_op: SemanticTyOpContext::new(),
            qual_store: Store::new(|x| format!("Specified qualifier `{}` not found", x.description())),
            base_ty_store: Store::new(|x| format!("Specified type `{}` not found", x.description())),
            base_ty_logical_store: Store::new(|x| format!("Specified type `{}` not found", x.description())),
            ty_store: Store::new(|x| format!("Specified type `{}` not found", x.description())),
            ty_logical_store: Store::new(|x| format!("Specified type `{}` not found", x.description())),
            literal_store: Store::new(|x| format!("Specified literal `{}` not found", x.description())),
            var_store: Store::new(|x| format!("Specified variable `{}` not found", x.description())),
        };
        register_default_tys(&context)?;
        Ok(context)
    }

    pub fn new_with_json(json: String) -> Result<Self, Vec<String>> {
        let context = Self::new()?;
        register_from_json(&context, json)?;
        Ok(context)
    }
}
