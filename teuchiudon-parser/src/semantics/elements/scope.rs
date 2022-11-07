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
            Self::Qual(name) =>
                name.clone(),
            _ =>
                "Not implemented error occured".to_owned(),
        }
    }
}
