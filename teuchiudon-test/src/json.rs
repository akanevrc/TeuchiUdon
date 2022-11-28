use serde::Deserialize;
use serde_json;

#[derive(Debug, Deserialize)]
pub struct Compiled {
    pub output: String,
    pub errors: Vec<String>,
    pub default_values: Vec<DefaultValue>,
}

#[derive(Debug, Deserialize)]
pub struct DefaultValue {
    pub name: String,
    pub ty: String,
    pub value: String,
}

pub fn from_json(json: &str) -> Compiled {
    serde_json::from_str(json).unwrap()
}
