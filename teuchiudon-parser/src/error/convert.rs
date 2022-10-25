use std::{
    collections::HashMap,
    fmt::Write,
    cmp::min,
    iter::repeat,
};
use itertools::Itertools;
use nom_supreme::error::{
    GenericErrorTree,
    StackContext,
};
use super::{
    ErrorTree,
    ErrorTreeIter,
};

#[cfg(windows)]
const NEWLINE: &'static str = "\r\n";
#[cfg(not(windows))]
const NEWLINE: &'static str = "\n";

pub fn convert_error(input: &str, e: ErrorTree) -> String {
    let infoes = line_infoes(input, &e);
    let mut message = String::new();
    for (line, ch, line_slice, slice, context) in infoes {
        writeln!(
            &mut message,
            "({}, {}): Parse error, expected {}{}{}{}{}",
            line,
            ch,
            context,
            NEWLINE,
            line_slice,
            NEWLINE,
            char_caret(ch, line_slice, slice)
        ).unwrap();
    }
    message
}

pub fn line_infoes<'input: 'error, 'error>(
    input: &'input str,
    e: &'error ErrorTree<'input>
) -> Vec<(usize, usize, &'input str, &'input str, &'error String)> {
    let sorted = context_iter(e)
        .map(|x| x.0)
        .sorted_by(|x, y| x.as_ptr().cmp(&y.as_ptr()));
    let filtered = filter_slices(sorted);
    let input_ptr = input.as_ptr() as usize;
    let input_bytes = input.as_bytes();
    let mut current_line = if input.len() > 0 && input_bytes[0] == b'\n' { 2 } else { 1 };
    let mut current_char = if input.len() > 0 && input_bytes[0] == b'\n' { 1 } else { 2 };
    let mut line_char_slice = HashMap::<usize, (usize, usize, &str)>::new();
    let mut prev_offset = 1usize;
    for s in filtered {
        let ptr = s.as_ptr() as usize;
        let offset = ptr - input_ptr;
        if offset == 0 {
            let line_slice = input.lines().next().unwrap_or("");
            line_char_slice.insert(ptr, (1, 1, line_slice));
            continue;
        }
        let iter_1 = input_bytes[prev_offset - 1..offset - 1].iter();
        let iter = input_bytes[prev_offset..offset].iter();
        let line = iter_1.zip(iter).filter(|(x, y)| **x == b'\r' || **y == b'\n').count();
        let ch = input_bytes[prev_offset..offset].iter().rev().position(|x| *x == b'\r' || *x == b'\n');
        current_line += line;
        current_char = ch.map_or(current_char + offset - prev_offset, |x| x + 1);
        let line_slice = input[offset + 1 - current_char..].lines().next().unwrap();
        line_char_slice.insert(ptr, (current_line, current_char, line_slice));
        prev_offset = offset;
    }
    context_iter(e)
    .collect::<Vec<_>>()
    .into_iter()
    .rev()
    .unique_by(|(s, context)| (s.as_ptr(), s.len(), *context))
    .rev()
    .filter_map(|(s, context)| {
        let ptr = s.as_ptr() as usize;
        if let Some((l, c, ls)) = line_char_slice.get(&ptr) {
            Some((*l, *c, *ls, s, context))
        }
        else {
            None
        }
    }).collect()
}

fn context_iter<'input: 'error, 'error>(e: &'error ErrorTree<'input>) -> impl Iterator<Item = (&'input str, &'error String)> {
    ErrorTreeIter::new(e)
    .filter_map(|x|
        if let GenericErrorTree::Stack { base: _, contexts } = x { Some(contexts) } else { None }
    )
    .flat_map(|x| x)
    .filter_map(|x|
        if let StackContext::Context(s) = &x.1 { Some((x.0, s)) } else { None }
    )
}

fn filter_slices<'input>(
    iter: impl Iterator<Item = &'input str>
) -> Vec<&'input str> {
    let mut v = Vec::<&str>::new();
    let mut v_keep = Vec::<bool>::new();
    for slice in iter {
        let slice_head = slice.as_ptr() as usize;
        let slice_tail = slice_head + slice.len();
        let mut slice_keep = true;
        for (i, &s) in v.iter().enumerate() {
            let s_head = s.as_ptr() as usize;
            let s_tail = s_head + s.len();
            if s_head <= slice_head && slice_tail <= s_tail {
                v_keep[i] = false;
            }
            else if slice_head <= s_head && s_tail <= slice_tail {
                slice_keep = false;
            }
        }
        v = v.iter()
            .zip(v_keep.iter())
            .filter_map(|(s, k)| if *k { Some(*s) } else { None })
            .collect();
        if slice_keep { v.push(slice); }
        v_keep = repeat(true).take(v.len()).collect();
    }
    v
}

fn char_caret(ch: usize, line_slice: &str, slice: &str) -> String {
    format!("{}{}", " ".repeat(ch - 1), "^".repeat(min(line_slice.len() + 1 - ch, slice.len())))
}
