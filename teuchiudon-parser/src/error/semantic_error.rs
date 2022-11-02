use crate::semantics::SemanticError;
use super::{
    NEWLINE,
    char_caret,
    line_infoes,
};

pub fn convert_semantic_error(input: &str, es: Vec<SemanticError>) -> Vec<String> {
    let infoes = line_infoes(input, &es);
    let mut messages = Vec::new();
    for (line, ch, line_slice, slice, message) in infoes {
        let mes = format!(
            "({}, {}): {}{}{}{}{}",
            line,
            ch,
            message,
            NEWLINE,
            line_slice,
            NEWLINE,
            char_caret(ch, line_slice, slice)
        );
        messages.push(mes);
    }
    messages
}
