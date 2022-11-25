use serde::Serialize;
use serde_json;
use super::Context;
use crate::assembly::label::EvalLabel;

#[derive(Serialize)]
struct Compiled {
    output: String,
    errors: Vec<String>,
    default_values: Vec<DefaultValue>,
}

#[derive(Serialize)]
struct DefaultValue {
    name: String,
    ty: String,
    value: String,
}

impl<'input> Context<'input> {
    pub fn output_to_json(&self, output: String) -> String {
        let default_values =
            self.valued_vars.iter()
            .map(|(var, pub_var)|
                DefaultValue {
                    name: var.name.clone(),
                    ty: self.ty_labels[var.ty.borrow().as_ref()].to_name()[0].real_name.clone(),
                    value: self.literal_labels[pub_var.literal.as_ref()].to_name()[0].real_name.clone(),
                }
            )
            .chain(
                self.literal_labels.iter()
                .map(|(literal, data)|
                    DefaultValue {
                        name: data.to_name()[0].real_name.clone(),
                        ty: self.ty_labels[literal.ty.as_ref()].to_name()[0].real_name.clone(),
                        value: literal.text.clone(),
                    }
                )
            )
            .collect();
        let compiled = Compiled {
            output,
            errors: Vec::new(),
            default_values,
        };
        serde_json::to_string(&compiled).unwrap()
    }

    pub fn errors_to_json(errors: Vec<String>) -> String {
        let compiled = Compiled {
            output: String::new(),
            errors,
            default_values: Vec::new(),
        };
        serde_json::to_string(&compiled).unwrap()
    }
}
