pub mod id_factory;
pub mod keyword;
pub mod op_code;
pub mod qual_stack;
pub mod semantic_op;
pub mod semantic_ty_op;
pub mod store;
pub mod vec_store;
mod json;

use self::{
    id_factory::IdFactory,
    keyword::KeywordContext,
    op_code::OpCodeContext,
    qual_stack::QualStack,
    semantic_op::SemanticOpContext,
    semantic_ty_op::SemanticTyOpContext,
    store::Store,
    vec_store::VecStore,
};
use crate::semantics::elements::{
    base_ty::{
        BaseTy,
        BaseTyKey,
        BaseTyLogicalKey,
    },
    element::SemanticElement,
    ev::{
        Ev,
        EvKey,
    },
    ev_stats::EvStats,
    eval_fn::{
        EvalFn,
        EvalFnKey,
    },
    fn_stats::{
        FnKey,
        FnStats,
    },
    literal::{
        Literal,
        LiteralKey,
    },
    method::{
        Method,
        MethodKey,
    },
    named_methods::{
        NamedMethods,
        NamedMethodsKey,
    },
    qual::{
        Qual,
        QualKey,
    },
    this_literal::{
        ThisLiteral,
        ThisLiteralKey,
    },
    top_stat::TopStat,
    ty::{
        Ty,
        TyKey,
        TyLogicalKey,
    },
    valued_var::ValuedVar,
    var::{
        Var,
        VarKey,
    },
};

pub struct Context<'input> {
    pub keyword: KeywordContext,
    pub op_code: OpCodeContext,
    pub semantic_op: SemanticOpContext,
    pub semantic_ty_op: SemanticTyOpContext,
    pub block_id_factory: IdFactory,
    pub loop_id_factory: IdFactory,
    pub let_in_id_factory: IdFactory,
    pub qual_stack: QualStack,
    pub qual_store: Store<QualKey, Qual>,
    pub base_ty_store: Store<BaseTyKey, BaseTy>,
    pub base_ty_logical_store: Store<BaseTyLogicalKey, BaseTy>,
    pub ty_store: Store<TyKey, Ty>,
    pub ty_logical_store: Store<TyLogicalKey, Ty>,
    pub ev_store: Store<EvKey, Ev>,
    pub literal_store: Store<LiteralKey, Literal>,
    pub this_literal_store: Store<ThisLiteralKey, ThisLiteral>,
    pub method_store: Store<MethodKey, Method>,
    pub named_methods_store: Store<NamedMethodsKey, NamedMethods>,
    pub var_store: Store<VarKey, Var>,
    pub top_stat_store: VecStore<TopStat<'input>>,
    pub ev_stats_store: Store<EvKey, EvStats<'input>>,
    pub fn_stats_store: Store<FnKey, FnStats<'input>>,
    pub eval_fn_store: Store<EvalFnKey, EvalFn<'input>>,
    pub valued_var_store: Store<VarKey, ValuedVar>,
}

impl<'input> Context<'input> {
    pub fn new() -> Result<Self, Vec<String>> {
        let context = Self {
            keyword: KeywordContext::new(),
            op_code: OpCodeContext::new(),
            semantic_op: SemanticOpContext::new(),
            semantic_ty_op: SemanticTyOpContext::new(),
            block_id_factory: IdFactory::new(),
            loop_id_factory: IdFactory::new(),
            let_in_id_factory: IdFactory::new(),
            qual_stack: QualStack::new(),
            qual_store: Store::new(|x| format!("Specified qualifier `{}` not found", x.description())),
            base_ty_store: Store::new(|x| format!("Specified type `{}` not found", x.description())),
            base_ty_logical_store: Store::new(|x| format!("Specified type `{}` not found", x.description())),
            ty_store: Store::new(|x| format!("Specified type `{}` not found", x.description())),
            ty_logical_store: Store::new(|x| format!("Specified type `{}` not found", x.description())),
            ev_store: Store::new(|x| format!("Specified event `{}` not found", x.description())),
            literal_store: Store::new(|x| format!("Specified literal `{}` not found", x.description())),
            this_literal_store: Store::new(|x| format!("Specified literal `{}` not found", x.description())),
            method_store: Store::new(|x| format!("Specified method `{}` not found", x.description())),
            named_methods_store: Store::new(|x| format!("Specified method `{}` not found", x.description())),
            var_store: Store::new(|x| format!("Specified variable `{}` not found", x.description())),
            top_stat_store: VecStore::new(),
            ev_stats_store: Store::new(|x| format!("Specified event `{}` not found", x.description())),
            fn_stats_store: Store::new(|x| format!("Specified function `{}` not found", x.description())),
            eval_fn_store: Store::new(|x| format!("Specified function evaluation `{}` not found", x.description())),
            valued_var_store: Store::new(|x| format!("Specified variable `{}` not found", x.description())),
        };
        context.register_default_tys()?;
        Ok(context)
    }

    pub fn new_with_json(json: String) -> Result<Self, Vec<String>> {
        let context = Self::new()?;
        context.register_from_json(json)?;
        context.register_named_methods()?;
        Ok(context)
    }
}
