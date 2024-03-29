pub mod assembly;
pub mod compiler;
pub mod context;

use std::collections::HashSet;
use teuchiudon_parser::{
    context::Context as ParserContext,
    analize,
    parse,
};
use self::{
    assembly::container::AsmContainer,
    compiler::generator::{
        generate_data_part,
        generate_code_part
    },
    context::Context as CompilerContext,
};

pub fn compile(input: &str, json: &str) -> String {
    match compile_result(input, json) {
        Ok((context, output, used_data)) =>
            context.output_to_json(output, used_data),
        Err(errors) =>
            CompilerContext::errors_to_json(errors),
    }
}

fn compile_result<'input>(input: &'input str, json: &'input str) -> Result<(CompilerContext<'input>, String, HashSet<String>), Vec<String>> {
    let parser_context = ParserContext::new_with_json(json.to_owned())?;
    let parsed = parse(&parser_context, input)?;
    analize(&parser_context, input, parsed)?;
    
    let compiler_context = CompilerContext::convert(&parser_context);
    let mut asm_container = AsmContainer::new();
    asm_container.push_data_part(generate_data_part(&compiler_context));
    asm_container.push_code_part(generate_code_part(&compiler_context));
    asm_container.prepare();
    Ok((compiler_context, asm_container.to_string(), asm_container.used_data()))
}
