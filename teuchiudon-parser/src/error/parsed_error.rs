use super::{
    NEWLINE,
    ErrorTree,
    char_caret,
    line_infoes,
};

pub fn convert_parsed_error(input: &str, e: ErrorTree) -> Vec<String> {
    let infoes = line_infoes(input, &e);
    let mut messages = Vec::new();
    for (line, ch, line_slice, slice, context) in infoes {
        let mes = format!(
            "({}, {}): Parse error, expected {}{}{}{}{}",
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
    messages
}
