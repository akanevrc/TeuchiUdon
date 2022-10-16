use crate::lexer::{
    ast as ast,
    byte_order_mark,
    delimited_comment,
    line_comment,
    newline,
    whitespace0,
    whitespace1,
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
    interpolated_string,
};

#[test]
fn test_byte_order_mark() {
    assert_eq!(byte_order_mark("\u{EF}\u{BB}\u{BF}xxx"), Ok(("xxx", ())));
}

#[test]
fn test_whitespace0() {
    assert_eq!(whitespace0("xxx"), Ok(("xxx", ())));
    assert_eq!(whitespace0(" \t\r\nxxx"), Ok(("xxx", ())));
}

#[test]
fn test_whitespace1() {
    assert_eq!(whitespace1(" \t\r\nxxx"), Ok(("xxx", ())));
    assert_eq!(whitespace1("xxx").map_err(|_| ()), Err(()));
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
    assert_eq!(control("as")("as xxx").0, Ok((" xxx", ast::Control::As)));
}

#[test]
fn test_control_as_error() {
    assert_eq!(control("as")("asxxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_encloser_open_brace() {
    assert_eq!(encloser("{")("{xxx").0, Ok(("xxx", ast::Encloser::OpenBrace)));
}

#[test]
fn test_delimiter_comma() {
    assert_eq!(delimiter(",")(",xxx").0, Ok(("xxx", ast::Delimiter::Comma)));
}

#[test]
fn test_end_semicolon() {
    assert_eq!(end(";")(";xxx").0, Ok(("xxx", ast::End::Semicolon)));
}

#[test]
fn test_op_code_dot() {
    assert_eq!(op_code(".")(".xxx").0, Ok(("xxx", ast::OpCode::Dot)));
}

#[test]
fn test_ident() {
    assert_eq!(ident("A xxx").0, Ok((" xxx", ast::Ident { name: "A".to_owned() })));
    assert_eq!(ident("a xxx").0, Ok((" xxx", ast::Ident { name: "a".to_owned() })));
    assert_eq!(ident("AbC xxx").0, Ok((" xxx", ast::Ident { name: "AbC".to_owned() })));
    assert_eq!(ident("abc xxx").0, Ok((" xxx", ast::Ident { name: "abc".to_owned() })));
    assert_eq!(ident("ab1 xxx").0, Ok((" xxx", ast::Ident { name: "ab1".to_owned() })));
    assert_eq!(ident("1ab xxx").0.map_err(|_| ()), Err(()));
    assert_eq!(ident("a_b xxx").0, Ok((" xxx", ast::Ident { name: "a_b".to_owned() })));
    assert_eq!(ident("_ab xxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_unit_literal() {
    assert_eq!(unit_literal("()xxx").0, Ok(("xxx", ast::Literal::Unit)));
    assert_eq!(unit_literal("( )xxx").0, Ok(("xxx", ast::Literal::Unit)));
}

#[test]
fn test_null_literal() {
    assert_eq!(null_literal("null xxx").0, Ok((" xxx", ast::Literal::Null)));
    assert_eq!(null_literal("nullxxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_bool_literal() {
    assert_eq!(bool_literal("true xxx").0, Ok((" xxx", ast::Literal::Bool("true".to_owned()))));
    assert_eq!(bool_literal("false xxx").0, Ok((" xxx", ast::Literal::Bool("false".to_owned()))));
    assert_eq!(bool_literal("truexxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_integer_literal() {
    assert_eq!(integer_literal("123 xxx").0, Ok((" xxx", ast::Literal::Integer("123".to_owned()))));
    assert_eq!(integer_literal("1_2__3 xxx").0, Ok((" xxx", ast::Literal::Integer("123".to_owned()))));
    assert_eq!(integer_literal("123L xxx").0, Ok((" xxx", ast::Literal::Integer("123L".to_owned()))));
    assert_eq!(integer_literal("123U xxx").0, Ok((" xxx", ast::Literal::Integer("123U".to_owned()))));
    assert_eq!(integer_literal("123LU xxx").0, Ok((" xxx", ast::Literal::Integer("123LU".to_owned()))));
    assert_eq!(integer_literal("123UL xxx").0, Ok((" xxx", ast::Literal::Integer("123UL".to_owned()))));
    assert_eq!(integer_literal("123xxx").0.map_err(|_| ()), Err(()));
    assert_eq!(integer_literal("_123 xxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_hex_integer_literal() {
    assert_eq!(hex_integer_literal("0xFA3 xxx").0, Ok((" xxx", ast::Literal::HexInteger("FA3".to_owned()))));
    assert_eq!(hex_integer_literal("0XFA3 xxx").0, Ok((" xxx", ast::Literal::HexInteger("FA3".to_owned()))));
    assert_eq!(hex_integer_literal("0xfa3 xxx").0, Ok((" xxx", ast::Literal::HexInteger("fa3".to_owned()))));
    assert_eq!(hex_integer_literal("0x_F_A__3 xxx").0, Ok((" xxx", ast::Literal::HexInteger("FA3".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3L xxx").0, Ok((" xxx", ast::Literal::HexInteger("FA3L".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3U xxx").0, Ok((" xxx", ast::Literal::HexInteger("FA3U".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3LU xxx").0, Ok((" xxx", ast::Literal::HexInteger("FA3LU".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3UL xxx").0, Ok((" xxx", ast::Literal::HexInteger("FA3UL".to_owned()))));
    assert_eq!(hex_integer_literal("0xFA3xxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_bin_integer_literal() {
    assert_eq!(bin_integer_literal("0b101 xxx").0, Ok((" xxx", ast::Literal::BinInteger("101".to_owned()))));
    assert_eq!(bin_integer_literal("0B101 xxx").0, Ok((" xxx", ast::Literal::BinInteger("101".to_owned()))));
    assert_eq!(bin_integer_literal("0b_1_0__1 xxx").0, Ok((" xxx", ast::Literal::BinInteger("101".to_owned()))));
    assert_eq!(bin_integer_literal("0b101L xxx").0, Ok((" xxx", ast::Literal::BinInteger("101L".to_owned()))));
    assert_eq!(bin_integer_literal("0b101U xxx").0, Ok((" xxx", ast::Literal::BinInteger("101U".to_owned()))));
    assert_eq!(bin_integer_literal("0b101LU xxx").0, Ok((" xxx", ast::Literal::BinInteger("101LU".to_owned()))));
    assert_eq!(bin_integer_literal("0b101UL xxx").0, Ok((" xxx", ast::Literal::BinInteger("101UL".to_owned()))));
    assert_eq!(bin_integer_literal("0b101xxx").0.map_err(|_| ()), Err(()));
    assert_eq!(bin_integer_literal("0b123 xxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_real_number_literal() {
    assert_eq!(real_number_literal("12.345 xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345".to_owned()))));
    assert_eq!(real_number_literal("1__2.3_4__5 xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345".to_owned()))));
    assert_eq!(real_number_literal("12.345E67 xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345E67".to_owned()))));
    assert_eq!(real_number_literal("12.345e67 xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345e67".to_owned()))));
    assert_eq!(real_number_literal("12.345E+67 xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345E+67".to_owned()))));
    assert_eq!(real_number_literal("12.345E-67 xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345E-67".to_owned()))));
    assert_eq!(real_number_literal("12.345F xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345F".to_owned()))));
    assert_eq!(real_number_literal("12.345D xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345D".to_owned()))));
    assert_eq!(real_number_literal("12.345M xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345M".to_owned()))));
    assert_eq!(real_number_literal("12.345E-67D xxx").0, Ok((" xxx", ast::Literal::RealNumber("12.345E-67D".to_owned()))));
    assert_eq!(real_number_literal("123E45 xxx").0, Ok((" xxx", ast::Literal::RealNumber("123E45".to_owned()))));
    assert_eq!(real_number_literal("123e45 xxx").0, Ok((" xxx", ast::Literal::RealNumber("123e45".to_owned()))));
    assert_eq!(real_number_literal("123E+45 xxx").0, Ok((" xxx", ast::Literal::RealNumber("123E+45".to_owned()))));
    assert_eq!(real_number_literal("123E-45 xxx").0, Ok((" xxx", ast::Literal::RealNumber("123E-45".to_owned()))));
    assert_eq!(real_number_literal("123F xxx").0, Ok((" xxx", ast::Literal::RealNumber("123F".to_owned()))));
    assert_eq!(real_number_literal("123D xxx").0, Ok((" xxx", ast::Literal::RealNumber("123D".to_owned()))));
    assert_eq!(real_number_literal("123M xxx").0, Ok((" xxx", ast::Literal::RealNumber("123M".to_owned()))));
    assert_eq!(real_number_literal("123E-45D xxx").0, Ok((" xxx", ast::Literal::RealNumber("123E-45D".to_owned()))));
    assert_eq!(real_number_literal("12.345xxx").0.map_err(|_| ()), Err(()));
    assert_eq!(real_number_literal("123Fxxx").0.map_err(|_| ()), Err(()));
    assert_eq!(real_number_literal("123 xxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_character_literal() {
    assert_eq!(character_literal("'a'xxx").0, Ok(("xxx", ast::Literal::Character("a".to_owned()))));
    assert_eq!(character_literal("' 'xxx").0, Ok(("xxx", ast::Literal::Character(" ".to_owned()))));
    assert_eq!(character_literal("'\"'xxx").0, Ok(("xxx", ast::Literal::Character("\"".to_owned()))));
    assert_eq!(character_literal("'\\''xxx").0, Ok(("xxx", ast::Literal::Character("\\'".to_owned()))));
    assert_eq!(character_literal("'\\\\'xxx").0, Ok(("xxx", ast::Literal::Character("\\\\".to_owned()))));
    assert_eq!(character_literal("'\\n'xxx").0, Ok(("xxx", ast::Literal::Character("\\n".to_owned()))));
    assert_eq!(character_literal("'\\xF'xxx").0, Ok(("xxx", ast::Literal::Character("\\xF".to_owned()))));
    assert_eq!(character_literal("'\\xFFFF'xxx").0, Ok(("xxx", ast::Literal::Character("\\xFFFF".to_owned()))));
    assert_eq!(character_literal("'\\uFFFF'xxx").0, Ok(("xxx", ast::Literal::Character("\\uFFFF".to_owned()))));
    assert_eq!(character_literal("'\\UFFFFFFFF'xxx").0, Ok(("xxx", ast::Literal::Character("\\UFFFFFFFF".to_owned()))));
    assert_eq!(character_literal("'ab'xxx").0.map_err(|_| ()), Err(()));
    assert_eq!(character_literal("' a'xxx").0.map_err(|_| ()), Err(()));
    assert_eq!(character_literal("'\\1'xxx").0.map_err(|_| ()), Err(()));
    assert_eq!(character_literal("'\\xFFFFF'xxx").0.map_err(|_| ()), Err(()));
    assert_eq!(character_literal("'\\uFFFFFFFF'xxx").0.map_err(|_| ()), Err(()));
    assert_eq!(character_literal("'\\UFFFF'xxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_regular_string_literal() {
    assert_eq!(regular_string_literal("\"\"xxx").0, Ok(("xxx", ast::Literal::RegularString("".to_owned()))));
    assert_eq!(regular_string_literal("\"abc\"xxx").0, Ok(("xxx", ast::Literal::RegularString("abc".to_owned()))));
    assert_eq!(regular_string_literal("\"'\"xxx").0, Ok(("xxx", ast::Literal::RegularString("'".to_owned()))));
    assert_eq!(regular_string_literal("\"\\\"\"xxx").0, Ok(("xxx", ast::Literal::RegularString("\\\"".to_owned()))));
}

#[test]
fn test_verbatium_string_literal() {
    assert_eq!(verbatium_string_literal("@\"\"xxx").0, Ok(("xxx", ast::Literal::VerbatiumString("".to_owned()))));
    assert_eq!(verbatium_string_literal("@\"abc\"xxx").0, Ok(("xxx", ast::Literal::VerbatiumString("abc".to_owned()))));
    assert_eq!(verbatium_string_literal("@\"\"\"\"xxx").0, Ok(("xxx", ast::Literal::VerbatiumString("\"\"".to_owned()))));
    assert_eq!(verbatium_string_literal("@\"\"\"xxx").0.map_err(|_| ()), Err(()));
}

#[test]
fn test_interpolated_string() {
    assert_eq!(
        interpolated_string("$\"abc{expr}def{expr}ghi\"xxx").0,
        Ok(("xxx", ast::InterpolatedString { string_parts: vec!["abc".to_owned(), "def".to_owned(), "ghi".to_owned()] }))
    );
    assert_eq!(interpolated_string("$\"abc{expr\"xxx").0.map_err(|_| ()), Err(()));
}
