use std::hash::Hash;
use teuchiudon_parser::semantics::elements::{
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
    ev::Ev,
    literal::Literal,
    method::Method,
    qual::Qual,
    scope::Scope,
    ty::{
        Ty,
        TyInstance,
    },
    var::Var,
};

pub trait EvalLabel<T>
{
    fn to_name(&self) -> T;
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct DataName {
    pub real_name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct CodeName {
    pub real_name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct TyName {
    pub real_name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub struct ExternName {
    pub real_name: String,
}

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum TyElem {
    This { ty: TyName },
    Single { elem: DataName, ty: TyName }
}

impl From<String> for DataName {
    fn from(x: String) -> Self {
        Self {
            real_name: x,
        }
    }
}

impl From<String> for CodeName {
    fn from(x: String) -> Self {
        Self {
            real_name: x,
        }
    }
}

impl From<String> for TyName {
    fn from(x: String) -> Self {
        Self {
            real_name: x,
        }
    }
}

impl From<String> for ExternName {
    fn from(x: String) -> Self {
        Self {
            real_name: x,
        }
    }
}

impl EvalLabel<String> for DataName {
    fn to_name(&self) -> String {
        self.real_name.clone()
    }
}

impl EvalLabel<String> for CodeName {
    fn to_name(&self) -> String {
        self.real_name.clone()
    }
}

impl EvalLabel<String> for TyName {
    fn to_name(&self) -> String {
        self.real_name.clone()
    }
}

impl EvalLabel<String> for ExternName {
    fn to_name(&self) -> String {
        self.real_name.clone()
    }
}

impl EvalLabel<Vec<DataName>> for DataLabel {
    fn to_name(&self) -> Vec<DataName> {
        match &self.kind {
            DataLabelKind::Literal(x) => vec![x.to_name()],
            DataLabelKind::Var(x) => x.to_name(),
            DataLabelKind::Indirect(x, _) => vec![x.to_name().to_indirect()],
        }
    }
}

impl EvalLabel<CodeName> for CodeLabel {
    fn to_name(&self) -> CodeName {
        match &self.kind {
            CodeLabelKind::Ev(x) => x.to_name(),
        }
    }
}

impl EvalLabel<Vec<TyName>> for TyLabel {
    fn to_name(&self) -> Vec<TyName> {
        match &self.kind {
            TyLabelKind::Ty(ty) => ty.to_name(),
            TyLabelKind::Addr => vec![TyName::indirect()],
        }
    }
}

impl EvalLabel<ExternName> for ExternLabel {
    fn to_name(&self) -> ExternName {
        match &self.kind {
            ExternLabelKind::Method(x) => x.to_name(),
        }
    }
}

impl EvalLabel<String> for Qual {
    fn to_name(&self) -> String {
        self.qualify_logical_name(">")
    }
}

impl EvalLabel<String> for Scope {
    fn to_name(&self) -> String {
        match self {
            Self::Qual(x) => x.clone(),
            _ => "".to_owned(),
        }
    }
}

impl EvalLabel<Vec<TyName>> for Ty {
    fn to_name(&self) -> Vec<TyName> {
        match &self.instance {
            Some(instance) =>
                instance.to_name().into_iter()
                .map(|x| match x {
                    TyElem::This { ty } => ty,
                    TyElem::Single { elem: _, ty } => ty,
                })
                .collect(),
            None =>
                panic!("Illegal state"),
        }
    }
}

impl EvalLabel<Vec<TyElem>> for TyInstance {
    fn to_name(&self) -> Vec<TyElem> {
        match self {
            TyInstance::Unit =>
                Vec::new(),
            TyInstance::Single { elem_name, ty_name } =>
                vec![TyElem::Single { elem: DataName::from(elem_name.clone().unwrap_or(String::new())), ty: TyName::from(ty_name.clone()) }],
            TyInstance::Tuple { elem_name, instances } =>
                instances.iter()
                .flat_map(|x| x.to_name().into_iter())
                .map(|x| match x {
                    TyElem::This { ty } => {
                        let elem = DataName::from(elem_name.clone().unwrap_or(String::new()));
                        TyElem::Single { elem, ty }
                    },
                    TyElem::Single { elem, ty } => {
                        let elem = DataName::from(format!("[{}]{}", elem_name.clone().unwrap_or(String::new()), elem.real_name));
                        TyElem::Single { elem, ty }
                    },
                })
                .collect()
        }
    }
}

impl EvalLabel<CodeName> for Ev {
    fn to_name(&self) -> CodeName {
        CodeName::from(self.real_name.clone())
    }
}

impl EvalLabel<DataName> for Literal {
    fn to_name(&self) -> DataName {
        DataName::from(format!("literal[{}]", self.id))
    }
}

impl EvalLabel<ExternName> for Method {
    fn to_name(&self) -> ExternName {
        ExternName::from(self.real_name.clone())
    }
}

impl EvalLabel<Vec<DataName>> for Var {
    fn to_name(&self) -> Vec<DataName> {
        match &self.ty.borrow().instance {
            Some(instance) =>
                instance.to_name().into_iter()
                .map(|x| match x {
                    TyElem::This { ty: _ } =>
                        DataName::from(format!("var[{}{}]", self.qual.to_name(), self.name)),
                    TyElem::Single { elem, ty: _ } =>
                        DataName::from(format!("var[{}{}][{}]", self.qual.to_name(), self.name, elem.real_name)),
                })
                .collect(),
            None =>
                panic!("Illegal state"),
        }
    }
}

impl CodeName {
    pub fn to_indirect(&self) -> DataName {
        DataName::from(format!("indirect[{}]", self.real_name))
    }
}

impl TyName {
    pub fn indirect() -> Self {
        Self::from("SysytemUInt32".to_owned())
    }
}
