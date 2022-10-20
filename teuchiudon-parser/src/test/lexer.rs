use crate::context::Context;
use crate::lexer::{
    ast,
    lex,
    byte_order_mark,
    delimited_comment,
    line_comment,
    newline,
    whitespace0,
    whitespace1,
    keyword,
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
    this_literal,
    interpolated_string,
    eof,
};
use crate::parser;

#[test]
fn test_lex() {
    let context = Context::new();
    assert_eq!(lex(keyword(&context, "as"))("as xxx"), Ok((" xxx", ast::Keyword::As)));
    assert_eq!(lex(keyword(&context, "as"))("  as xxx"), Ok((" xxx", ast::Keyword::As)));
    assert_eq!(lex(keyword(&context, "as"))(" {/ comment /} // comment\nas xxx"), Ok((" xxx", ast::Keyword::As)));
}

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
    assert!(whitespace1("xxx").is_err());
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
    assert!(delimited_comment("{/ this is a comment. {/ xxx this is also a comment. /}xxx").is_err());
}

#[test]
fn test_keyword_as() {
    let context = Context::new();
    assert_eq!(keyword(&context, "as")("as xxx").0, Ok((" xxx", ast::Keyword::As)));
}

#[test]
fn test_keyword_as_error() {
    let context = Context::new();
    assert!(keyword(&context, "as")("asxxx").0.is_err());
}

#[test]
fn test_op_code_open_brace() {
    let context = Context::new();
    assert_eq!(op_code(&context, "{")("{xxx").0, Ok(("xxx", ast::OpCode::OpenBrace)));
}

#[test]
fn test_op_code_comma() {
    let context = Context::new();
    assert_eq!(op_code(&context, ",")(",xxx").0, Ok(("xxx", ast::OpCode::Comma)));
}

#[test]
fn test_op_code_semicolon() {
    let context = Context::new();
    assert_eq!(op_code(&context, ";")(";xxx").0, Ok(("xxx", ast::OpCode::Semicolon)));
}

#[test]
fn test_op_code_dot() {
    let context = Context::new();
    assert_eq!(op_code(&context, ".")(".xxx").0, Ok(("xxx", ast::OpCode::Dot)));
}

#[test]
fn test_ident() {
    let context = Context::new();
    assert_eq!(ident(&context)("A xxx").0, Ok((" xxx", ast::Ident("A".to_owned()))));
    assert_eq!(ident(&context)("a xxx").0, Ok((" xxx", ast::Ident("a".to_owned()))));
    assert_eq!(ident(&context)("AbC xxx").0, Ok((" xxx", ast::Ident("AbC".to_owned()))));
    assert_eq!(ident(&context)("abc xxx").0, Ok((" xxx", ast::Ident("abc".to_owned()))));
    assert_eq!(ident(&context)("ab1 xxx").0, Ok((" xxx", ast::Ident("ab1".to_owned()))));
    assert!(ident(&context)("1ab xxx").0.is_err());
    assert_eq!(ident(&context)("a_b xxx").0, Ok((" xxx", ast::Ident("a_b".to_owned()))));
    assert!(ident(&context)("_ab xxx").0.is_err());
}

#[test]
fn test_unit_literal() {
    let context = Context::new();
    assert_eq!(unit_literal(&context)("()xxx").0, Ok(("xxx", ast::Literal::Unit)));
    assert_eq!(unit_literal(&context)("( )xxx").0, Ok(("xxx", ast::Literal::Unit)));
}

#[test]
fn test_null_literal() {
    let context = Context::new();
    assert_eq!(null_literal(&context)("null xxx").0, Ok((" xxx", ast::Literal::Null(ast::Keyword::Null))));
    assert!(null_literal(&context)("nullxxx").0.is_err());
}

#[test]
fn test_bool_literal() {
    let context = Context::new();
    assert_eq!(bool_literal(&context)("true xxx").0, Ok((" xxx", ast::Literal::Bool(ast::Keyword::True))));
    assert_eq!(bool_literal(&context)("false xxx").0, Ok((" xxx", ast::Literal::Bool(ast::Keyword::False))));
    assert!(bool_literal(&context)("truexxx").0.is_err());
}

#[test]
fn test_integer_literal() {
    assert_eq!(integer_literal("123 xxx").0, Ok((" xxx", ast::Literal::PureInteger("123".to_owned()))));
    assert_eq!(integer_literal("1_2__3 xxx").0, Ok((" xxx", ast::Literal::PureInteger("123".to_owned()))));
    assert_eq!(integer_literal("123L xxx").0, Ok((" xxx", ast::Literal::DecInteger("123L".to_owned()))));
    assert_eq!(integer_literal("123U xxx").0, Ok((" xxx", ast::Literal::DecInteger("123U".to_owned()))));
    assert_eq!(integer_literal("123LU xxx").0, Ok((" xxx", ast::Literal::DecInteger("123LU".to_owned()))));
    assert_eq!(integer_literal("123UL xxx").0, Ok((" xxx", ast::Literal::DecInteger("123UL".to_owned()))));
    assert!(integer_literal("123xxx").0.is_err());
    assert!(integer_literal("_123 xxx").0.is_err());
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
    assert!(hex_integer_literal("0xFA3xxx").0.is_err());
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
    assert!(bin_integer_literal("0b101xxx").0.is_err());
    assert!(bin_integer_literal("0b123 xxx").0.is_err());
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
    assert!(real_number_literal("12.345xxx").0.is_err());
    assert!(real_number_literal("123Fxxx").0.is_err());
    assert!(real_number_literal("123 xxx").0.is_err());
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
    assert!(character_literal("'ab'xxx").0.is_err());
    assert!(character_literal("' a'xxx").0.is_err());
    assert!(character_literal("'\\1'xxx").0.is_err());
    assert!(character_literal("'\\xFFFFF'xxx").0.is_err());
    assert!(character_literal("'\\uFFFFFFFF'xxx").0.is_err());
    assert!(character_literal("'\\UFFFF'xxx").0.is_err());
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
    assert!(verbatium_string_literal("@\"\"\"xxx").0.is_err());
}

#[test]
fn test_this_literal() {
    let context = Context::new();
    assert_eq!(this_literal(&context)("this xxx").0, Ok((" xxx", ast::Literal::This(ast::Keyword::This))));
    assert!(this_literal(&context)("thisxxx").0.is_err());
}

#[test]
fn test_interpolated_string() {
    let context = Context::new();
    assert_eq!(
        interpolated_string(&context)("$\"abc{123}def{val}ghi\"xxx").0,
        Ok(("xxx", ast::InterpolatedString(
            vec!["abc".to_owned(), "def".to_owned(), "ghi".to_owned()],
            vec![
                parser::ast::Expr(parser::ast::Term::Literal(ast::Literal::PureInteger("123".to_owned())), Vec::new()),
                parser::ast::Expr(parser::ast::Term::EvalVar(ast::Ident("val".to_owned())), Vec::new()),
            ],
        ))),
    );
    assert!(interpolated_string(&context)("$\"abc{123\"xxx").0.is_err());
}

#[test]
fn test_eof() {
    assert_eq!(eof("").0, Ok(("", ())));
    assert!(eof("xxx").0.is_err());
}
