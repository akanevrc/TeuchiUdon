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
    ty::{
        Ty,
        TyKey,
        TyRealKey,
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
    pub base_ty_store: Store<BaseTyKey, BaseTy>,
    pub base_ty_logical_store: Store<BaseTyLogicalKey, BaseTy>,
    pub literal_store: Store<LiteralKey, Literal>,
    pub ty_store: Store<TyKey, Ty>,
    pub ty_real_store: Store<TyRealKey, Ty>,
    pub var_store: Store<VarKey, Var>,
}

impl Context {
    pub fn new() -> Result<Self, Vec<String>> {
        let context = Self {
            keyword: KeywordContext::new(),
            op_code: OpCodeContext::new(),
            semantic_op: SemanticOpContext::new(),
            semantic_ty_op: SemanticTyOpContext::new(),
            base_ty_store: Store::new(|x| format!("Specific type `{}` not found", x.description())),
            base_ty_logical_store: Store::new(|x| format!("Specific type `{}` not found", x.description())),
            literal_store: Store::new(|x| format!("Specific literal `{}` not found", x.description())),
            ty_store: Store::new(|x| format!("Specific type `{}` not found", x.description())),
            ty_real_store: Store::new(|x| format!("Specific type `{}` not found", x.description())),
            var_store: Store::new(|x| format!("Specific variable `{}` not found", x.description())),
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
