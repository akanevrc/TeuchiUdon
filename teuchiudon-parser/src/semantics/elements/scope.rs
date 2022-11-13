use super::element::SemanticElement;

#[derive(Clone, Debug, Eq, Hash, PartialEq)]
pub enum Scope {
    Block,
    FnBlock,
    LoopBlock,
    For,
    Fn,
    Closure,
    LetIn,
    Qual(String),
    VarBind,
}

impl SemanticElement for Scope {
    fn description(&self) -> String {
        match self {
            Self::Qual(x) =>
                x.description(),
            _ =>
                panic!("Not implemented error occured"),
        }
    }

    fn logical_name(&self) -> String {
        match self {
            Self::Qual(x) =>
                x.logical_name(),
            _ =>
                panic!("Not implemented error occured"),
        }
    }
}
