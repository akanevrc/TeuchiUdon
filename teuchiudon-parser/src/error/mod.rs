pub(crate) mod context_iter;
pub(crate) mod parsed_error;
pub(crate) mod semantic_error;

use std::{
    collections::HashMap,
    cmp,
    error::Error,
    iter,
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
) -> Vec<(Option<(usize, usize, &'input str)>, Option<&'input str>, &'error String)>
where
    E: HasContextIter<'input, 'error>,
{
    let sorted = e.context_iter()
        .map(|x| x.0)
        .sorted_by(|x, y| option_str_ptr(*x).cmp(&option_str_ptr(*y)));
    let filtered = filter_slices(sorted);
    let input_ptr = input.as_ptr() as usize;
    let input_bytes = input.as_bytes();
    let mut current_line = if input.len() > 0 && input_bytes[0] == b'\n' { 2 } else { 1 };
    let mut current_char = if input.len() > 0 && input_bytes[0] == b'\n' { 1 } else { 2 };
    let mut line_char_slice = HashMap::<usize, Option<(usize, usize, &str)>>::new();
    let mut prev_offset = 1usize;
    for s in filtered {
        let ptr = option_str_ptr(s);
        let offset = s.map_or(0, |x| x.trim().as_ptr() as usize - input_ptr);
        if offset == 0 {
            line_char_slice.insert(ptr, None);
            continue;
        }
        let iter_1 = input_bytes[prev_offset - 1..offset - 1].iter();
        let iter = input_bytes[prev_offset..offset].iter();
        let line = iter_1.zip(iter).filter(|(x, y)| **x == b'\r' || **y == b'\n').count();
        let ch = input_bytes[prev_offset..offset].iter().rev().position(|x| *x == b'\r' || *x == b'\n');
        current_line += line;
        current_char = ch.map_or(current_char + offset - prev_offset, |x| x + 1);
        let line_slice = input[offset + 1 - current_char..].lines().next().unwrap_or(&input[offset + 1 - current_char..]);
        line_char_slice.insert(ptr, Some((current_line, current_char, line_slice)));
        prev_offset = offset;
    }
    e.context_iter()
    .collect::<Vec<_>>()
    .into_iter()
    .rev()
    .unique_by(|(s, context)| (option_str_ptr(*s), option_str_len(*s), *context))
    .rev()
    .filter_map(|(s, context)| {
        let ptr = option_str_ptr(s);
        let l_c_ls = line_char_slice.get(&ptr);
        if let Some(Some((l, c, ls))) = l_c_ls {
            Some((Some((*l, *c, *ls)), s, context))
        }
        else if let Some(None) = l_c_ls {
            Some((None, s, context))
        }
        else {
            None
        }
    }).collect()
}

fn filter_slices<'input>(
    iter: impl Iterator<Item = Option<&'input str>>
) -> Vec<Option<&'input str>> {
    let mut v = Vec::<Option<&str>>::new();
    let mut v_keep = Vec::new();
    for slice in iter {
        let slice_head = option_str_ptr(slice);
        let slice_tail = slice_head + option_str_len(slice);
        let mut slice_keep = true;
        for (i, &s) in v.iter().enumerate() {
            let s_head = option_str_ptr(s);
            let s_tail = s_head + option_str_len(s);
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
        v_keep = iter::repeat(true).take(v.len()).collect();
    }
    v
}

fn char_caret(ch: usize, line_slice: &str, slice: Option<&str>) -> String {
    format!("{}{}", " ".repeat(ch - 1), "^".repeat(cmp::min(line_slice.len() + 1 - ch, option_str_len(slice))))
}

fn option_str_ptr(s: Option<&str>) -> usize {
    s.map_or(0, |x| x.as_ptr() as usize)
}

fn option_str_len(s: Option<&str>) -> usize {
    s.map_or(0, |x| x.len() as usize)
}
