pub mod parsed_error;
pub mod semantic_error;
pub mod context_iter;

use std::{
    collections::HashMap,
    cmp::min,
    error::Error,
    iter::repeat,
};
use itertools::Itertools;
use nom_supreme::error::{
    GenericErrorTree,
};
use context_iter::HasContextIter;

#[cfg(windows)]
const NEWLINE: &'static str = "\r\n";
#[cfg(not(windows))]
const NEWLINE: &'static str = "\n";

pub type ErrorTree<'input> = GenericErrorTree<&'input str, &'static str, String, Box<dyn Error + Send + Sync + 'static>>;

#[derive(Clone, Debug)]
pub struct ErrorTreeIter<'input: 'error, 'error> {
    stack: Vec<&'error ErrorTree<'input>>,
}

impl<'input: 'error, 'error> ErrorTreeIter<'input, 'error> {
    pub fn new(error: &'error ErrorTree<'input>) -> Self {
        let mut stack = Vec::new();
        stack.push(error);
        Self { stack }
    }
}

impl<'input: 'error, 'error> Iterator for ErrorTreeIter<'input, 'error> {
    type Item = &'error ErrorTree<'input>;

    fn next(&mut self) -> Option<Self::Item> {
        match self.stack.pop() {
            Some(error) => {
                match error {
                    GenericErrorTree::Base { location: _, kind: _ } => (),
                    GenericErrorTree::Stack { base, contexts: _ } => {
                        self.stack.push(base);
                    },
                    GenericErrorTree::Alt(v) => {
                        for e in v.iter().rev() {
                            self.stack.push(e);
                        }
                    }
                }
                Some(error)
            },
            None => None,
        }
    }
}

pub fn line_infoes<'input: 'error, 'error, E>(
    input: &'input str,
    e: &'error E,
) -> Vec<(usize, usize, &'input str, &'input str, &'error String)>
where
    E: HasContextIter<'input, 'error>,
{
    let sorted = e.context_iter()
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
    e.context_iter()
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
