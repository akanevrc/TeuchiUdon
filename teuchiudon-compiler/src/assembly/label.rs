use teuchiudon_parser::semantics::elements::{
    label::{
        CodeLabel,
        DataLabel,
        DataLabelKind,
        TyLabel,
    },
    literal::Literal,
    qual::Qual,
    scope::Scope,
    ty::Ty,
    var::Var,
};

pub trait HasLabel {
    fn part_name(&self) -> String;
    fn full_name(&self) -> String;
}

impl HasLabel for DataLabel {
    fn part_name(&self) -> String {
        match &self.kind {
            DataLabelKind::Literal(x) => x.part_name(),
            DataLabelKind::Var(x) => x.part_name(),
        }
    }

    fn full_name(&self) -> String {
        match &self.kind {
            DataLabelKind::Literal(x) => x.full_name(),
            DataLabelKind::Var(x) => x.full_name(),
        }
    }
}

impl HasLabel for CodeLabel {
    fn part_name(&self) -> String {
        "".to_owned()
    }

    fn full_name(&self) -> String {
        "".to_owned()
    }
}

impl HasLabel for TyLabel {
    fn part_name(&self) -> String {
        self.ty.part_name()
    }

    fn full_name(&self) -> String {
        self.ty.full_name()
    }
}

impl HasLabel for Literal {
    fn part_name(&self) -> String {
        format!("literal[{}]", self.id)
    }

    fn full_name(&self) -> String {
        format!("literal[{}]", self.id)
    }
}

impl HasLabel for Qual {
    fn part_name(&self) -> String {
        self.qualify(">")
    }

    fn full_name(&self) -> String {
        self.qualify(">")
    }
}

impl HasLabel for Scope {
    fn part_name(&self) -> String {
        match self {
            Self::Ty(x) => x.part_name(),
            _ => "".to_owned(),
        }
    }

    fn full_name(&self) -> String {
        match self {
            Self::Ty(x) => x.full_name(),
            _ => "".to_owned(),
        }
    }
}

impl HasLabel for Ty {
    fn part_name(&self) -> String {
        self.real_name.clone()
    }

    fn full_name(&self) -> String {
        self.real_name.clone()
    }
}

impl HasLabel for Var {
    fn part_name(&self) -> String {
        format!("var[{}]", self.name)
    }

    fn full_name(&self) -> String {
        format!("var[{}{}]", self.qual.full_name(), self.name)
    }
}
