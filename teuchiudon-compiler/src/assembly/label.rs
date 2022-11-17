use teuchiudon_parser::semantics::elements::{
    label::{
        CodeLabel,
        DataLabel,
        DataLabelKind,
        TyLabel,
        TyLabelKind,
    },
    literal::Literal,
    qual::Qual,
    scope::Scope,
    ty::Ty,
    var::Var,
};

pub trait Label {
    fn part_name(&self) -> String;
    fn full_name(&self) -> String;
}

impl Label for DataLabel {
    fn part_name(&self) -> String {
        match &self.kind {
            DataLabelKind::Literal(x) => x.part_name(),
            DataLabelKind::Var(x) => x.part_name(),
            DataLabelKind::Indirect(x, _) => format!("indirect[{}]", x.part_name()),
        }
    }

    fn full_name(&self) -> String {
        match &self.kind {
            DataLabelKind::Literal(x) => x.full_name(),
            DataLabelKind::Var(x) => x.full_name(),
            DataLabelKind::Indirect(x, _) => format!("indirect[{}]", x.full_name()),
        }
    }
}

impl Label for CodeLabel {
    fn part_name(&self) -> String {
        "".to_owned()
    }

    fn full_name(&self) -> String {
        "".to_owned()
    }
}

impl Label for TyLabel {
    fn part_name(&self) -> String {
        match &self.kind {
            TyLabelKind::Ty(ty) => ty.part_name(),
            TyLabelKind::Addr => "SysytemUInt32".to_owned(),
        }
    }

    fn full_name(&self) -> String {
        match &self.kind {
            TyLabelKind::Ty(ty) => ty.full_name(),
            TyLabelKind::Addr => "SysytemUInt32".to_owned(),
        }
    }
}

impl Label for Literal {
    fn part_name(&self) -> String {
        format!("literal[{}]", self.id)
    }

    fn full_name(&self) -> String {
        format!("literal[{}]", self.id)
    }
}

impl Label for Qual {
    fn part_name(&self) -> String {
        self.qualify_logical_name(">")
    }

    fn full_name(&self) -> String {
        self.qualify_logical_name(">")
    }
}

impl Label for Scope {
    fn part_name(&self) -> String {
        match self {
            Self::Qual(x) => x.clone(),
            _ => "".to_owned(),
        }
    }

    fn full_name(&self) -> String {
        match self {
            Self::Qual(x) => x.clone(),
            _ => "".to_owned(),
        }
    }
}

impl Label for Ty {
    fn part_name(&self) -> String {
        if let Some(real_name) = &self.real_name {
            real_name.clone()
        }
        else {
            panic!("Illegal state")
        }
    }

    fn full_name(&self) -> String {
        if let Some(real_name) = &self.real_name {
            real_name.clone()
        }
        else {
            panic!("Illegal state")
        }
    }
}

impl Label for Var {
    fn part_name(&self) -> String {
        format!("var[{}]", self.name)
    }

    fn full_name(&self) -> String {
        format!("var[{}{}]", self.qual.full_name(), self.name)
    }
}
