use crate::lexer::{
    byte_order_mark,
    delimited_comment,
    line_comment,
    newline,
    whitespace0,
};

#[test]
fn test_byte_order_mark() {
    assert_eq!(byte_order_mark("\u{EF}\u{BB}\u{BF}xxx"), Ok(("xxx", ())));
}

#[test]
fn test_whitespace0() {
    assert_eq!(whitespace0(" \t\r\nxxx"), Ok(("xxx", ())));
}

#[test]
fn test_newline() {
    assert_eq!(newline("\r\nxxx"), Ok(("xxx", ())));
    assert_eq!(newline("\rxxx"), Ok(("xxx", ())));
    assert_eq!(newline("\nxxx"), Ok(("xxx", ())));
}

#[test]
fn test_line_comment() {
    assert_eq!(line_comment("// this is a comment.\r\nxxx"), Ok(("xxx", ())));
}

#[test]
fn test_delimited_comment() {
    assert_eq!(delimited_comment("{/ this is a comment.\r\n this is also a comment. /}xxx"), Ok(("xxx", ())));
}

#[test]
fn test_delimited_comment_nested() {
    assert_eq!(delimited_comment("{/ this is a comment. {/ xxx /} this is also a comment. /}xxx"), Ok(("xxx", ())));
}

#[test]
fn test_delimited_comment_error() {
    assert_eq!(delimited_comment("{/ this is a comment. {/ xxx this is also a comment. /}xxx").map_err(|_| ()), Err(()));
}
