use teuchiudon_parser::{
    context::Context,
    parse,
};

pub fn compile(input: &str) -> Result<String, String> {
    let context = Context::new();
    parse(&context, input).map(|_| "Compile succeeded".to_owned())
}
