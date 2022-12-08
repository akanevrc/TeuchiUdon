use super::{
    NEWLINE,
    ErrorTree,
    char_caret,
    line_infoes,
};

pub fn convert_parsed_error(input: &str, e: ErrorTree) -> Vec<String> {
    let infoes = line_infoes(input, &e);
    let mut messages = Vec::new();
    for (l_c_ls, slice, context) in infoes {
        if let Some((line, ch, line_slice)) = l_c_ls {
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
        else {
            messages.push(context.clone());
        }
    }
    messages
}
