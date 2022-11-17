use std::{
    collections::HashMap,
    rc::Rc,
};
use teuchiudon_parser::semantics::elements::{
    label::{
        DataLabel,
        DataLabelKind,
        TyLabel,
        TyLabelKind,
    },
    literal::Literal,
    ty::Ty,
    var::Var,
};

pub struct Context {
    pub literal_labels: HashMap<Rc<Literal>, Rc<DataLabel>>,
    pub ty_labels: HashMap<Rc<Ty>, Rc<TyLabel>>,
    pub var_labels: HashMap<Rc<Var>, Rc<DataLabel>>,
}

impl Context {
    pub fn convert(context: &teuchiudon_parser::context::Context) -> Self {
        Self {
            literal_labels:
                context.literal_store.values()
                .map(|x| (x.clone(), Rc::new(DataLabel::new(DataLabelKind::Literal(x.clone())))))
                .collect(),
            ty_labels:
                context.ty_store.values()
                .map(|x| (x.clone(), Rc::new(TyLabel::new(TyLabelKind::Ty(x.clone())))))
                .collect(),
            var_labels:
                context.var_store.values()
                .map(|x| (x.clone(), Rc::new(DataLabel::new(DataLabelKind::Var(x.clone())))))
                .collect(),
        }
    }
}
