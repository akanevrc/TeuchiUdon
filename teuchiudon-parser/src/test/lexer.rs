use crate::lexer::{
    ast as ast,
    byte_order_mark,
    delimited_comment,
    line_comment,
    newline,
    whitespace0,
    control,
    encloser,
    delimiter,
    end,
    op_code,
    ident,
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

#[test]
fn test_control_as() {
    assert_eq!(control("as")("as xxx"), Ok((" xxx", ast::Control::As)));
}

#[test]
fn test_control_as_error() {
    assert_eq!(control("as")("asxxx").map_err(|_| ()), Err(()));
}

#[test]
fn test_encloser_open_brace() {
    assert_eq!(encloser("{")("{xxx"), Ok(("xxx", ast::Encloser::OpenBrace)));
}

#[test]
fn test_delimiter_comma() {
    assert_eq!(delimiter(",")(",xxx"), Ok(("xxx", ast::Delimiter::Comma)));
}

#[test]
fn test_end_semicolon() {
    assert_eq!(end(";")(";xxx"), Ok(("xxx", ast::End::Semicolon)));
}

#[test]
fn test_op_code_dot() {
    assert_eq!(op_code(".")(".xxx"), Ok(("xxx", ast::OpCode::Dot)));
}

#[test]
fn test_ident() {
    assert_eq!(ident("A xxx"), Ok((" xxx", ast::Ident { name: "A".to_owned() })));
    assert_eq!(ident("a xxx"), Ok((" xxx", ast::Ident { name: "a".to_owned() })));
    assert_eq!(ident("AbC xxx"), Ok((" xxx", ast::Ident { name: "AbC".to_owned() })));
    assert_eq!(ident("abc xxx"), Ok((" xxx", ast::Ident { name: "abc".to_owned() })));
    assert_eq!(ident("ab1 xxx"), Ok((" xxx", ast::Ident { name: "ab1".to_owned() })));
    assert_eq!(ident("1ab xxx").map_err(|_| ()), Err(()));
    assert_eq!(ident("a_b xxx"), Ok((" xxx", ast::Ident { name: "a_b".to_owned() })));
    assert_eq!(ident("_ab xxx").map_err(|_| ()), Err(()));
}
