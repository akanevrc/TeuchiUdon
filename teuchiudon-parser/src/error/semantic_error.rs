use crate::semantics::SemanticError;
use super::{
    NEWLINE,
    char_caret,
    line_infoes,
};

pub fn convert_semantic_error(input: &str, es: Vec<SemanticError>) -> Vec<String> {
    let infoes = line_infoes(input, &es);
    let mut messages = Vec::new();
    for (l_c_ls, slice, context) in infoes {
        if let Some((line, ch, line_slice)) = l_c_ls {
            let mes = format!(
                "({}, {}): {}{}{}{}{}",
                line,
                ch,
                context,
                NEWLINE,
                line_slice,
                NEWLINE,
                char_caret(ch, line_slice, slice)
            );
            messages.push(mes);
        }
        else {
            messages.push(context.clone());
        }
    }
    messages
}
