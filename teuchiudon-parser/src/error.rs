use std::{
    collections::HashMap,
    fmt::Write,
};
use itertools::Itertools;
use nom::error::{
    VerboseError,
    VerboseErrorKind,
};

#[cfg(windows)]
const NEWLINE: &'static str = "\r\n";
#[cfg(not(windows))]
const NEWLINE: &'static str = "\n";

pub fn convert_error(input: &str, e: VerboseError<&str>) -> String {
    let es = line_char_numbers(input, e);
    let mut message = String::new();
    for (line, ch, slice, kind) in es {
        match kind {
            VerboseErrorKind::Context(x) => write!(&mut message, "({}, {}): in {}{}{}{}", line, ch, x, NEWLINE, slice, NEWLINE).unwrap(),
            VerboseErrorKind::Char(x) => write!(&mut message, "({}, {}): in {}{}{}{}", line, ch, x, NEWLINE, slice, NEWLINE).unwrap(),
            VerboseErrorKind::Nom(x) => write!(&mut message, "({}, {}): in {:?}{}{}{}", line, ch, x, NEWLINE, slice, NEWLINE).unwrap(),
        }
    }
    message
}

pub fn line_char_numbers<'input>(input: &'input str, e: VerboseError<&'input str>) -> Vec<(usize, usize, &'input str, VerboseErrorKind)> {
    let sorted = e.errors.iter()
        .map(|x| x.0)
        .sorted_by(|&x, &y| x.as_ptr().cmp(&y.as_ptr()));
    let input_ptr = input.as_ptr() as usize;
    let input_bytes = input.as_bytes();
    let mut current_line = if input.len() > 0 && input_bytes[0] == b'\n' { 2 } else { 1 };
    let mut current_char = if input.len() > 0 && input_bytes[0] == b'\n' { 1 } else { 2 };
    let mut line_char_numbers = HashMap::<usize, (usize, usize)>::new();
    let mut prev_offset = 1usize;
    for s in sorted {
        let ptr = s.as_ptr() as usize;
        let offset = ptr - input_ptr;
        if offset == 0 {
            line_char_numbers.insert(ptr, (1, 1));
            continue;
        }
        let iter_1 = input_bytes[prev_offset - 1..offset - 1].iter();
        let iter = input_bytes[prev_offset..offset].iter();
        let line = iter_1.zip(iter).filter(|(&x, &y)| x == b'\r' || y == b'\n').count();
        let ch = input_bytes[prev_offset..offset].iter().rev().position(|&x| x == b'\r' || x == b'\n');
        current_line += line;
        current_char = ch.map_or(current_char + offset - prev_offset, |x| x + 1);
        line_char_numbers.insert(ptr, (current_line, current_char));
        prev_offset = offset;
    }
    e.errors.into_iter().map(|(s, k)| {
        let ptr = s.as_ptr() as usize;
        let (l, c) = line_char_numbers.get(&ptr).unwrap();
        (*l, *c, s, k)
    }).collect()
}
