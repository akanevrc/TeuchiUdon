use std::rc::Rc;
use crate::context::Context;
use crate::lexer::{
    self,
    ast,
    lex,
};
use crate::parser;

#[test]
fn test_lex() {
    let context = Context::new();
    assert_eq!(lex(lexer::keyword(&context, "as"))("as xxx").ok(), Some((" xxx", ast::Keyword("as", ast::KeywordKind::As))));
    assert_eq!(lex(lexer::keyword(&context, "as"))("  as xxx").ok(), Some((" xxx", ast::Keyword("as", ast::KeywordKind::As))));
    assert_eq!(lex(lexer::keyword(&context, "as"))(" {/ comment /} // comment\nas xxx").ok(), Some((" xxx", ast::Keyword("as", ast::KeywordKind::As))));
}

#[test]
fn test_byte_order_mark() {
    assert_eq!(lexer::byte_order_mark("\u{EF}\u{BB}\u{BF}xxx").0.ok(), Some(("xxx", ())));
}

#[test]
fn test_whitespace0() {
    assert_eq!(lexer::whitespace0("xxx").ok(), Some(("xxx", ())));
    assert_eq!(lexer::whitespace0(" \t\r\nxxx").ok(), Some(("xxx", ())));
}

#[test]
fn test_whitespace1() {
    assert_eq!(lexer::whitespace1(" \t\r\nxxx").ok(), Some(("xxx", ())));
    assert_eq!(lexer::whitespace1("xxx").ok(), None);
}

#[test]
fn test_newline() {
    assert_eq!(lexer::newline("\r\nxxx").ok(), Some(("xxx", ())));
    assert_eq!(lexer::newline("\rxxx").ok(), Some(("xxx", ())));
    assert_eq!(lexer::newline("\nxxx").ok(), Some(("xxx", ())));
}

#[test]
fn test_line_comment() {
    assert_eq!(lexer::line_comment("// this is a comment.\r\nxxx").ok(), Some(("xxx", ())));
}

#[test]
fn test_delimited_comment() {
    assert_eq!(lexer::delimited_comment("{/ this is a comment.\r\n this is also a comment. /}xxx").ok(), Some(("xxx", ())));
    assert_eq!(lexer::delimited_comment("{/ this is a comment. {/ xxx /} this is also a comment. /}xxx").ok(), Some(("xxx", ())));
    assert_eq!(lexer::delimited_comment("{/ this is a comment. {/ xxx this is also a comment. /}xxx").ok(), None);
}

#[test]
fn test_keyword() {
    let context = Context::new();
    assert_eq!(lexer::keyword(&context, "as")("as xxx").0.ok(), Some((" xxx", ast::Keyword("as", ast::KeywordKind::As))));
    assert_eq!(lexer::keyword(&context, "as")("asxxx").0.ok(), None);
}

#[test]
fn test_op_code() {
    let context = Context::new();
    assert_eq!(lexer::op_code(&context, "{")("{xxx").0.ok(), Some(("xxx", ast::OpCode("{", ast::OpCodeKind::OpenBrace))));
    assert_eq!(lexer::op_code(&context, ",")(",xxx").0.ok(), Some(("xxx", ast::OpCode(",", ast::OpCodeKind::Comma))));
    assert_eq!(lexer::op_code(&context, ";")(";xxx").0.ok(), Some(("xxx", ast::OpCode(";", ast::OpCodeKind::Semicolon))));
    assert_eq!(lexer::op_code(&context, ".")(".xxx").0.ok(), Some(("xxx", ast::OpCode(".", ast::OpCodeKind::Dot))));
    assert_eq!(lexer::op_code(&context, "<")("<xxx").0.ok(), Some(("xxx", ast::OpCode("<", ast::OpCodeKind::Lt))));
    assert_eq!(lexer::op_code(&context, "<=")("<==xxx").0.ok(), Some(("=xxx", ast::OpCode("<=", ast::OpCodeKind::Le))));
    assert_eq!(lexer::op_code(&context, "<")("<=xxx").0.ok(), None);
}

#[test]
fn test_ident() {
    let context = Context::new();
    assert_eq!(lexer::ident(&context)("A xxx").0.ok(), Some((" xxx", ast::Ident("A"))));
    assert_eq!(lexer::ident(&context)("a xxx").0.ok(), Some((" xxx", ast::Ident("a"))));
    assert_eq!(lexer::ident(&context)("AbC xxx").0.ok(), Some((" xxx", ast::Ident("AbC"))));
    assert_eq!(lexer::ident(&context)("abc xxx").0.ok(), Some((" xxx", ast::Ident("abc"))));
    assert_eq!(lexer::ident(&context)("ab1 xxx").0.ok(), Some((" xxx", ast::Ident("ab1"))));
    assert_eq!(lexer::ident(&context)("1ab xxx").0.ok(), None);
    assert_eq!(lexer::ident(&context)("a_b xxx").0.ok(), Some((" xxx", ast::Ident("a_b"))));
    assert_eq!(lexer::ident(&context)("_ab xxx").0.ok(), None);
}

#[test]
fn test_unit_literal() {
    let context = Context::new();
    assert_eq!(
        lexer::unit_literal(&context)("()xxx").0.ok(),
        Some(("xxx", ast::Literal::Unit(ast::OpCode("(", ast::OpCodeKind::OpenParen), ast::OpCode(")", ast::OpCodeKind::CloseParen))))
    );
    assert_eq!(
        lexer::unit_literal(&context)("( )xxx").0.ok(),
        Some(("xxx", ast::Literal::Unit(ast::OpCode("(", ast::OpCodeKind::OpenParen), ast::OpCode(")", ast::OpCodeKind::CloseParen))))
    );
}

#[test]
fn test_null_literal() {
    let context = Context::new();
    assert_eq!(lexer::null_literal(&context)("null xxx").0.ok(), Some((" xxx", ast::Literal::Null(ast::Keyword("null", ast::KeywordKind::Null)))));
    assert_eq!(lexer::null_literal(&context)("nullxxx").0.ok(), None);
}

#[test]
fn test_bool_literal() {
    let context = Context::new();
    assert_eq!(lexer::bool_literal(&context)("true xxx").0.ok(), Some((" xxx", ast::Literal::Bool(ast::Keyword("true", ast::KeywordKind::True)))));
    assert_eq!(lexer::bool_literal(&context)("false xxx").0.ok(), Some((" xxx", ast::Literal::Bool(ast::Keyword("false", ast::KeywordKind::False)))));
    assert_eq!(lexer::bool_literal(&context)("truexxx").0.ok(), None);
}

#[test]
fn test_integer_literal() {
    assert_eq!(lexer::integer_literal("123 xxx").0.ok(), Some((" xxx", ast::Literal::PureInteger("123"))));
    assert_eq!(lexer::integer_literal("1_2__3 xxx").0.ok(), Some((" xxx", ast::Literal::PureInteger("1_2__3"))));
    assert_eq!(lexer::integer_literal("123L xxx").0.ok(), Some((" xxx", ast::Literal::DecInteger("123L"))));
    assert_eq!(lexer::integer_literal("123U xxx").0.ok(), Some((" xxx", ast::Literal::DecInteger("123U"))));
    assert_eq!(lexer::integer_literal("123LU xxx").0.ok(), Some((" xxx", ast::Literal::DecInteger("123LU"))));
    assert_eq!(lexer::integer_literal("123UL xxx").0.ok(), Some((" xxx", ast::Literal::DecInteger("123UL"))));
    assert_eq!(lexer::integer_literal("123xxx").0.ok(), None);
    assert_eq!(lexer::integer_literal("_123 xxx").0.ok(), None);
}

#[test]
fn test_hex_integer_literal() {
    assert_eq!(lexer::hex_integer_literal("0xFA3 xxx").0.ok(), Some((" xxx", ast::Literal::HexInteger("0xFA3"))));
    assert_eq!(lexer::hex_integer_literal("0XFA3 xxx").0.ok(), Some((" xxx", ast::Literal::HexInteger("0XFA3"))));
    assert_eq!(lexer::hex_integer_literal("0xfa3 xxx").0.ok(), Some((" xxx", ast::Literal::HexInteger("0xfa3"))));
    assert_eq!(lexer::hex_integer_literal("0x_F_A__3 xxx").0.ok(), Some((" xxx", ast::Literal::HexInteger("0x_F_A__3"))));
    assert_eq!(lexer::hex_integer_literal("0xFA3L xxx").0.ok(), Some((" xxx", ast::Literal::HexInteger("0xFA3L"))));
    assert_eq!(lexer::hex_integer_literal("0xFA3U xxx").0.ok(), Some((" xxx", ast::Literal::HexInteger("0xFA3U"))));
    assert_eq!(lexer::hex_integer_literal("0xFA3LU xxx").0.ok(), Some((" xxx", ast::Literal::HexInteger("0xFA3LU"))));
    assert_eq!(lexer::hex_integer_literal("0xFA3UL xxx").0.ok(), Some((" xxx", ast::Literal::HexInteger("0xFA3UL"))));
    assert_eq!(lexer::hex_integer_literal("0xFA3xxx").0.ok(), None);
}

#[test]
fn test_bin_integer_literal() {
    assert_eq!(lexer::bin_integer_literal("0b101 xxx").0.ok(), Some((" xxx", ast::Literal::BinInteger("0b101"))));
    assert_eq!(lexer::bin_integer_literal("0B101 xxx").0.ok(), Some((" xxx", ast::Literal::BinInteger("0B101"))));
    assert_eq!(lexer::bin_integer_literal("0b_1_0__1 xxx").0.ok(), Some((" xxx", ast::Literal::BinInteger("0b_1_0__1"))));
    assert_eq!(lexer::bin_integer_literal("0b101L xxx").0.ok(), Some((" xxx", ast::Literal::BinInteger("0b101L"))));
    assert_eq!(lexer::bin_integer_literal("0b101U xxx").0.ok(), Some((" xxx", ast::Literal::BinInteger("0b101U"))));
    assert_eq!(lexer::bin_integer_literal("0b101LU xxx").0.ok(), Some((" xxx", ast::Literal::BinInteger("0b101LU"))));
    assert_eq!(lexer::bin_integer_literal("0b101UL xxx").0.ok(), Some((" xxx", ast::Literal::BinInteger("0b101UL"))));
    assert_eq!(lexer::bin_integer_literal("0b101xxx").0.ok(), None);
    assert_eq!(lexer::bin_integer_literal("0b123 xxx").0.ok(), None);
}

#[test]
fn test_real_number_literal() {
    assert_eq!(lexer::real_number_literal("12.345 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("12.345"))));
    assert_eq!(lexer::real_number_literal("1__2.3_4__5 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("1__2.3_4__5"))));
    assert_eq!(lexer::real_number_literal("12.345E67 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("12.345E67"))));
    assert_eq!(lexer::real_number_literal("12.345e67 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("12.345e67"))));
    assert_eq!(lexer::real_number_literal("12.345E+67 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("12.345E+67"))));
    assert_eq!(lexer::real_number_literal("12.345E-67 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("12.345E-67"))));
    assert_eq!(lexer::real_number_literal("12.345F xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("12.345F"))));
    assert_eq!(lexer::real_number_literal("12.345D xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("12.345D"))));
    assert_eq!(lexer::real_number_literal("12.345M xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("12.345M"))));
    assert_eq!(lexer::real_number_literal("12.345E-67D xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("12.345E-67D"))));
    assert_eq!(lexer::real_number_literal("123E45 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("123E45"))));
    assert_eq!(lexer::real_number_literal("123e45 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("123e45"))));
    assert_eq!(lexer::real_number_literal("123E+45 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("123E+45"))));
    assert_eq!(lexer::real_number_literal("123E-45 xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("123E-45"))));
    assert_eq!(lexer::real_number_literal("123F xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("123F"))));
    assert_eq!(lexer::real_number_literal("123D xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("123D"))));
    assert_eq!(lexer::real_number_literal("123M xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("123M"))));
    assert_eq!(lexer::real_number_literal("123E-45D xxx").0.ok(), Some((" xxx", ast::Literal::RealNumber("123E-45D"))));
    assert_eq!(lexer::real_number_literal("12.345xxx").0.ok(), None);
    assert_eq!(lexer::real_number_literal("123Fxxx").0.ok(), None);
    assert_eq!(lexer::real_number_literal("123 xxx").0.ok(), None);
}

#[test]
fn test_character_literal() {
    assert_eq!(lexer::character_literal("'a'xxx").0.ok(), Some(("xxx", ast::Literal::Character("a"))));
    assert_eq!(lexer::character_literal("' 'xxx").0.ok(), Some(("xxx", ast::Literal::Character(" "))));
    assert_eq!(lexer::character_literal("'\"'xxx").0.ok(), Some(("xxx", ast::Literal::Character("\""))));
    assert_eq!(lexer::character_literal("'\\''xxx").0.ok(), Some(("xxx", ast::Literal::Character("\\'"))));
    assert_eq!(lexer::character_literal("'\\\\'xxx").0.ok(), Some(("xxx", ast::Literal::Character("\\\\"))));
    assert_eq!(lexer::character_literal("'\\n'xxx").0.ok(), Some(("xxx", ast::Literal::Character("\\n"))));
    assert_eq!(lexer::character_literal("'\\xF'xxx").0.ok(), Some(("xxx", ast::Literal::Character("\\xF"))));
    assert_eq!(lexer::character_literal("'\\xFFFF'xxx").0.ok(), Some(("xxx", ast::Literal::Character("\\xFFFF"))));
    assert_eq!(lexer::character_literal("'\\uFFFF'xxx").0.ok(), Some(("xxx", ast::Literal::Character("\\uFFFF"))));
    assert_eq!(lexer::character_literal("'\\UFFFFFFFF'xxx").0.ok(), Some(("xxx", ast::Literal::Character("\\UFFFFFFFF"))));
    assert_eq!(lexer::character_literal("'ab'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("' a'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("'\\1'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("'\\xFFFFF'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("'\\uFFFFFFFF'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("'\\UFFFF'xxx").0.ok(), None);
}

#[test]
fn test_regular_string_literal() {
    assert_eq!(lexer::regular_string_literal("\"\"xxx").0.ok(), Some(("xxx", ast::Literal::RegularString(""))));
    assert_eq!(lexer::regular_string_literal("\"abc\"xxx").0.ok(), Some(("xxx", ast::Literal::RegularString("abc"))));
    assert_eq!(lexer::regular_string_literal("\"'\"xxx").0.ok(), Some(("xxx", ast::Literal::RegularString("'"))));
    assert_eq!(lexer::regular_string_literal("\"\\\"\"xxx").0.ok(), Some(("xxx", ast::Literal::RegularString("\\\""))));
}

#[test]
fn test_verbatium_string_literal() {
    assert_eq!(lexer::verbatium_string_literal("@\"\"xxx").0.ok(), Some(("xxx", ast::Literal::VerbatiumString(""))));
    assert_eq!(lexer::verbatium_string_literal("@\"abc\"xxx").0.ok(), Some(("xxx", ast::Literal::VerbatiumString("abc"))));
    assert_eq!(lexer::verbatium_string_literal("@\"\"\"\"xxx").0.ok(), Some(("xxx", ast::Literal::VerbatiumString("\"\""))));
    assert_eq!(lexer::verbatium_string_literal("@\"\"\"xxx").0.ok(), None);
}

#[test]
fn test_this_literal() {
    let context = Context::new();
    assert_eq!(lexer::this_literal(&context)("this xxx").0.ok(), Some((" xxx", ast::Literal::This(ast::Keyword("this", ast::KeywordKind::This)))));
    assert_eq!(lexer::this_literal(&context)("thisxxx").0.ok(), None);
}

#[test]
fn test_interpolated_string() {
    let context = Context::new();
    assert_eq!(
        lexer::interpolated_string(&context)("$\"abc{123}def{val}ghi\"xxx").0.ok(),
        Some(("xxx", ast::InterpolatedString(
            vec!["abc", "def", "ghi"],
            vec![
                Rc::new(parser::ast::Expr(Rc::new(parser::ast::Term::Literal(ast::Literal::PureInteger("123"))), vec![])),
                Rc::new(parser::ast::Expr(Rc::new(parser::ast::Term::EvalVar(ast::Ident("val"))), vec![])),
            ],
        ))),
    );
    assert_eq!(lexer::interpolated_string(&context)("$\"abc{123\"xxx").0.ok(), None);
}

#[test]
fn test_eof() {
    assert_eq!(lexer::eof("").0.ok(), Some(("", ())));
    assert_eq!(lexer::eof("xxx").0.ok(), None);
}
