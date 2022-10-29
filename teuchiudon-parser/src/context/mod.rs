pub mod keyword;
pub mod op_code;
pub mod semantic_op;
pub mod semantic_type_op;

use self::{
    keyword::KeywordContext,
    op_code::OpCodeContext,
    semantic_op::SemanticOpContext,
    semantic_type_op::SemanticTypeOpContext,
};

pub struct Context {
    pub keyword: KeywordContext,
    pub op_code: OpCodeContext,
    pub semantic_op: SemanticOpContext,
    pub semantic_type_op: SemanticTypeOpContext,
}

impl Context {
    pub fn new() -> Self {
        Self {
            keyword: KeywordContext::new(),
            op_code: OpCodeContext::new(),
            semantic_op: SemanticOpContext::new(),
            semantic_type_op: SemanticTypeOpContext::new(),
        }
    }
}
