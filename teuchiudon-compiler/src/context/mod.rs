pub mod json;

use std::{
    collections::HashMap,
    rc::Rc,
};
use teuchiudon_parser::semantics::elements::{
    element::ValueElement,
    ev::Ev,
    ev_stats::EvStats,
    label::{
        CodeLabel,
        CodeLabelKind,
        DataLabel,
        DataLabelKind,
        ExternLabel,
        ExternLabelKind,
        TyLabel,
        TyLabelKind,
    },
    literal::Literal,
    method::Method,
    top_stat::TopStat,
    ty::Ty,
    valued_var::ValuedVar,
    var::Var,
};

pub struct Context<'input> {
    pub ty_labels: HashMap<Rc<Ty>, Rc<TyLabel>>,
    pub ev_labels: HashMap<Rc<Ev>, Rc<CodeLabel>>,
    pub literal_labels: HashMap<Rc<Literal>, Rc<DataLabel>>,
    pub method_labels: HashMap<Rc<Method>, Rc<ExternLabel>>,
    pub var_labels: HashMap<Rc<Var>, Rc<DataLabel>>,
    pub top_stats: Vec<Rc<TopStat<'input>>>,
    pub ev_stats: HashMap<Rc<Ev>, Rc<EvStats<'input>>>,
    pub valued_vars: HashMap<Rc<Var>, Rc<ValuedVar>>,
}

impl<'input> Context<'input> {
    pub fn convert(context: &teuchiudon_parser::context::Context<'input>) -> Self {
        Self {
            ty_labels:
                context.ty_store.values()
                .map(|x| (x.clone(), TyLabel::new(TyLabelKind::Ty(x.clone()))))
                .collect(),
            ev_labels:
                context.ev_store.values()
                .map(|x| (x.clone(), CodeLabel::new(CodeLabelKind::Ev(x.clone()))))
                .collect(),
            literal_labels:
                context.literal_store.values()
                .map(|x| (x.clone(), DataLabel::new(DataLabelKind::Literal(x.clone()))))
                .collect(),
            method_labels:
                context.method_store.values()
                .map(|x| (x.clone(), ExternLabel::new(ExternLabelKind::Method(x.clone()))))
                .collect(),
            var_labels:
                context.var_store.values()
                .map(|x| (x.clone(), DataLabel::new(DataLabelKind::Var(x.clone()))))
                .collect(),
            top_stats:
                context.top_stat_store.values()
                .collect(),
            ev_stats:
                context.ev_stats_store.values()
                .map(|x| (Ev::get(context, x.name.clone()).unwrap(), x.clone()))
                .collect(),
            valued_vars:
                context.valued_var_store.values()
                .map(|x| (Var::get(context, x.qual.to_key(), x.name.clone()).unwrap(), x.clone()))
                .collect(),
        }
    }
}
