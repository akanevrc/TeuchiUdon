use crate::lexer::{
    ast::{self as ast, Literal},
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
    unit_literal,
    null_literal,
    bool_literal,
    integer_literal,
    hex_integer_literal,
    bin_integer_literal,
    real_number_literal,
    character_literal,
    regular_string_literal,
    verbatium_string_literal,
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

#[test]
fn test_unit_literal() {
    assert_eq!(unit_literal("()xxx"), Ok(("xxx", ast::Literal::Unit)));
    assert_eq!(unit_literal("( )xxx"), Ok(("xxx", ast::Literal::Unit)));
}

#[test]
fn test_null_literal() {
    assert_eq!(null_literal("null xxx"), Ok((" xxx", ast::Literal::Null)));
    assert_eq!(null_literal("nullxxx").map_err(|_| ()), Err(()));
}

#[test]
fn test_bool_literal() {
    assert_eq!(bool_literal("true xxx"), Ok((" xxx", ast::Literal::Bool("true".to_owned()))));
    assert_eq!(bool_literal("false xxx"), Ok((" xxx", ast::Literal::Bool("false".to_owned()))));
    assert_eq!(bool_literal("truexxx").map_err(|_| ()), Err(()));
}

#[test]
fn test_integer_literal() {
    assert_eq!(integer_literal("123 xxx"), Ok((" xxx", ast::Literal::Integer("123".to_owned()))));
    assert_eq!(integer_literal("1_2__3 xxx"), Ok((" xxx", ast::Literal::Integer("123".to_owned()))));
    assert_eq!(integer_literal("123L xxx"), Ok((" xxx", ast::Literal::Integer("123L".to_owned()))));
    assert_eq!(integer_literal("123U xxx"), Ok((" xxx", ast::Literal::Integer("123U".to_owned()))));
    assert_eq!(integer_literal("123LU xxx"), Ok((" xxx", ast::Literal::Integer("123LU".to_owned()))));
    assert_eq!(integer_literal("123UL xxx"), Ok((" xxx", ast::Literal::Integer("123UL".to_owned()))));
    assert_eq!(integer_literal("123xxx").map_err(|_| ()), Err(()));
    assert_eq!(integer_literal("_123 xxx").map_err(|_| ()), Err(()));
}

#[test]
fn test_hex_integer_literal() {
    assert_eq!(hex_integer_literal("0xFA3 xxx"), Ok((" xxx", ast::Literal::HexInteger("FA3".to_owned()))));
    assert_eq!(hex_integer_literal("0XFA3 xxx"), Ok((" xxx", ast::Literal::HexInteger("FA3".to_owned()))));
    assert_eq!(hex_integer_literal("0xfa3 xxx"), Ok((" xxx", ast::Literal::HexInteger("fa3".to_owned()))));
    assert_eq!(hex_integer_literal("0x_F_A__3 xxx"), Ok((" xxx", ast::Literal::HexInteger("FA3".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3L xxx"), Ok((" xxx", ast::Literal::HexInteger("FA3L".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3U xxx"), Ok((" xxx", ast::Literal::HexInteger("FA3U".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3LU xxx"), Ok((" xxx", ast::Literal::HexInteger("FA3LU".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3UL xxx"), Ok((" xxx", ast::Literal::HexInteger("FA3UL".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3xxx").map_err(|_| ()), Err(()));
}

#[test]
fn test_bin_integer_literal() {
    assert_eq!(bin_integer_literal("0b101 xxx"), Ok((" xxx", ast::Literal::BinInteger("101".to_owned()))));
    assert_eq!(bin_integer_literal("0B101 xxx"), Ok((" xxx", ast::Literal::BinInteger("101".to_owned()))));
    assert_eq!(bin_integer_literal("0b_1_0__1 xxx"), Ok((" xxx", ast::Literal::BinInteger("101".to_owned()))));
    assert_eq!(bin_integer_literal("0b101L xxx"), Ok((" xxx", ast::Literal::BinInteger("101L".to_owned()))));
    assert_eq!(bin_integer_literal("0b101U xxx"), Ok((" xxx", ast::Literal::BinInteger("101U".to_owned()))));
    assert_eq!(bin_integer_literal("0b101LU xxx"), Ok((" xxx", ast::Literal::BinInteger("101LU".to_owned()))));
    assert_eq!(bin_integer_literal("0b101UL xxx"), Ok((" xxx", ast::Literal::BinInteger("101UL".to_owned()))));
    assert_eq!(bin_integer_literal("0b101xxx").map_err(|_| ()), Err(()));
    assert_eq!(bin_integer_literal("0b123 xxx").map_err(|_| ()), Err(()));
}

#[test]
fn test_real_number_literal() {
    assert_eq!(real_number_literal("12.345 xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345".to_owned()))));
    assert_eq!(real_number_literal("1__2.3_4__5 xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345".to_owned()))));
    assert_eq!(real_number_literal("12.345E67 xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345E67".to_owned()))));
    assert_eq!(real_number_literal("12.345e67 xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345e67".to_owned()))));
    assert_eq!(real_number_literal("12.345E+67 xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345E+67".to_owned()))));
    assert_eq!(real_number_literal("12.345E-67 xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345E-67".to_owned()))));
    assert_eq!(real_number_literal("12.345F xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345F".to_owned()))));
    assert_eq!(real_number_literal("12.345D xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345D".to_owned()))));
    assert_eq!(real_number_literal("12.345M xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345M".to_owned()))));
    assert_eq!(real_number_literal("12.345E-67D xxx"), Ok((" xxx", ast::Literal::RealNumber("12.345E-67D".to_owned()))));
    assert_eq!(real_number_literal("123E45 xxx"), Ok((" xxx", ast::Literal::RealNumber("123E45".to_owned()))));
    assert_eq!(real_number_literal("123e45 xxx"), Ok((" xxx", ast::Literal::RealNumber("123e45".to_owned()))));
    assert_eq!(real_number_literal("123E+45 xxx"), Ok((" xxx", ast::Literal::RealNumber("123E+45".to_owned()))));
    assert_eq!(real_number_literal("123E-45 xxx"), Ok((" xxx", ast::Literal::RealNumber("123E-45".to_owned()))));
    assert_eq!(real_number_literal("123F xxx"), Ok((" xxx", ast::Literal::RealNumber("123F".to_owned()))));
    assert_eq!(real_number_literal("123D xxx"), Ok((" xxx", ast::Literal::RealNumber("123D".to_owned()))));
    assert_eq!(real_number_literal("123M xxx"), Ok((" xxx", ast::Literal::RealNumber("123M".to_owned()))));
    assert_eq!(real_number_literal("123E-45D xxx"), Ok((" xxx", ast::Literal::RealNumber("123E-45D".to_owned()))));
    assert_eq!(real_number_literal("12.345xxx").map_err(|_| ()), Err(()));
    assert_eq!(real_number_literal("123Fxxx").map_err(|_| ()), Err(()));
    assert_eq!(real_number_literal("123 xxx").map_err(|_| ()), Err(()));
}

#[test]
fn test_character_literal() {
    assert_eq!(character_literal("'a'xxx"), Ok(("xxx", Literal::Character("a".to_owned()))));
    assert_eq!(character_literal("' 'xxx"), Ok(("xxx", Literal::Character(" ".to_owned()))));
    assert_eq!(character_literal("'\"'xxx"), Ok(("xxx", Literal::Character("\"".to_owned()))));
    assert_eq!(character_literal("'\\''xxx"), Ok(("xxx", Literal::Character("\\'".to_owned()))));
    assert_eq!(character_literal("'\\\\'xxx"), Ok(("xxx", Literal::Character("\\\\".to_owned()))));
    assert_eq!(character_literal("'\\n'xxx"), Ok(("xxx", Literal::Character("\\n".to_owned()))));
    assert_eq!(character_literal("'\\xF'xxx"), Ok(("xxx", Literal::Character("\\xF".to_owned()))));
    assert_eq!(character_literal("'\\xFFFF'xxx"), Ok(("xxx", Literal::Character("\\xFFFF".to_owned()))));
    assert_eq!(character_literal("'\\uFFFF'xxx"), Ok(("xxx", Literal::Character("\\uFFFF".to_owned()))));
    assert_eq!(character_literal("'\\UFFFFFFFF'xxx"), Ok(("xxx", Literal::Character("\\UFFFFFFFF".to_owned()))));
    assert_eq!(character_literal("'ab'xxx").map_err(|_| ()), Err(()));
    assert_eq!(character_literal("' a'xxx").map_err(|_| ()), Err(()));
    assert_eq!(character_literal("'\\1'xxx").map_err(|_| ()), Err(()));
    assert_eq!(character_literal("'\\xFFFFF'xxx").map_err(|_| ()), Err(()));
    assert_eq!(character_literal("'\\uFFFFFFFF'xxx").map_err(|_| ()), Err(()));
    assert_eq!(character_literal("'\\UFFFF'xxx").map_err(|_| ()), Err(()));
}

#[test]
fn test_regular_string_literal() {
    assert_eq!(regular_string_literal("\"\"xxx"), Ok(("xxx", Literal::RegularString("".to_owned()))));
    assert_eq!(regular_string_literal("\"abc\"xxx"), Ok(("xxx", Literal::RegularString("abc".to_owned()))));
    assert_eq!(regular_string_literal("\"'\"xxx"), Ok(("xxx", Literal::RegularString("'".to_owned()))));
    assert_eq!(regular_string_literal("\"\\\"\"xxx"), Ok(("xxx", Literal::RegularString("\\\"".to_owned()))));
}

#[test]
fn test_verbatium_string_literal() {
    assert_eq!(verbatium_string_literal("@\"\"xxx"), Ok(("xxx", Literal::VerbatiumString("".to_owned()))));
    assert_eq!(verbatium_string_literal("@\"abc\"xxx"), Ok(("xxx", Literal::VerbatiumString("abc".to_owned()))));
    assert_eq!(verbatium_string_literal("@\"\"\"\"xxx"), Ok(("xxx", Literal::VerbatiumString("\"\"".to_owned()))));
    assert_eq!(verbatium_string_literal("@\"\"\"xxx").map_err(|_| ()), Err(()));
}
