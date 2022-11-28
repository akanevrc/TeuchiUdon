mod json;

#[cfg(test)]
mod tests;

use std::{
    env,
    fs,
};
use teuchiudon_compiler::compile;

fn main() {
    if env::args().len() != 2 {
        panic!("Please specify .teuchi file path");
    }

    let path = env::args().last().unwrap();
    let input = fs::read_to_string(path).unwrap();
    let json = fs::read_to_string("./src/tests/teuchi/udon-symbols.json").unwrap();
    let output = compile(input.as_str(), json.as_str());
    let compiled = json::from_json(&output);

    if compiled.errors.len() == 0 {
        println!("{}", compiled.output);
        println!("{:#?}", compiled.default_values);
    }
    else {
        println!("{}", compiled.errors.join("\n"));
    }
}
