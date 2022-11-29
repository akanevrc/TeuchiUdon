use super::element::SemanticElement;

#[derive(Clone, Debug, Eq, Hash, Ord, PartialEq, PartialOrd)]
pub enum Scope {
    Block(usize),
    Loop(usize),
    LetIn(usize),
    Fn(usize),
    Qual(String),
}

impl SemanticElement for Scope {
    fn description(&self) -> String {
        match self {
            Self::Block(x) =>
                format!("block[{}]", x),
            Self::Loop(x) =>
                format!("loop[{}]", x),
            Self::LetIn(x) =>
                format!("letin[{}]", x),
            Self::Fn(x) =>
                format!("fn[{}]", x),
            Self::Qual(x) =>
                x.description(),
        }
    }

    fn logical_name(&self) -> String {
        match self {
            Self::Block(x) =>
                format!("block[{}]", x),
            Self::Loop(x) =>
                format!("loop[{}]", x),
            Self::LetIn(x) =>
                format!("letin[{}]", x),
            Self::Fn(x) =>
                format!("fn[{}]", x),
            Self::Qual(x) =>
                x.logical_name(),
        }
    }
}
