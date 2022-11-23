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
    let context = Context::new().unwrap();
    assert_eq!(lex(lexer::keyword(&context, "as"))("as xxx").ok(), Some((" xxx", Rc::new(ast::Keyword { slice: "as", kind: ast::KeywordKind::As }))));
    assert_eq!(lex(lexer::keyword(&context, "as"))("  as xxx").ok(), Some((" xxx", Rc::new(ast::Keyword { slice: "as", kind: ast::KeywordKind::As }))));
    assert_eq!(lex(lexer::keyword(&context, "as"))(" {/ comment /} // comment\nas xxx").ok(), Some((" xxx", Rc::new(ast::Keyword { slice: "as", kind: ast::KeywordKind::As }))));
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
    let context = Context::new().unwrap();
    assert_eq!(lexer::keyword(&context, "as")("as xxx").0.ok(), Some((" xxx", Rc::new(ast::Keyword { slice: "as", kind: ast::KeywordKind::As }))));
    assert_eq!(lexer::keyword(&context, "as")("asxxx").0.ok(), None);
}

#[test]
fn test_op_code() {
    let context = Context::new().unwrap();
    assert_eq!(lexer::op_code(&context, "{")("{xxx").0.ok(), Some(("xxx", Rc::new(ast::OpCode { slice: "{", kind: ast::OpCodeKind::OpenBrace }))));
    assert_eq!(lexer::op_code(&context, ",")(",xxx").0.ok(), Some(("xxx", Rc::new(ast::OpCode { slice: ",", kind: ast::OpCodeKind::Comma }))));
    assert_eq!(lexer::op_code(&context, ";")(";xxx").0.ok(), Some(("xxx", Rc::new(ast::OpCode { slice: ";", kind: ast::OpCodeKind::Semicolon }))));
    assert_eq!(lexer::op_code(&context, ".")(".xxx").0.ok(), Some(("xxx", Rc::new(ast::OpCode { slice: ".", kind: ast::OpCodeKind::Dot }))));
    assert_eq!(lexer::op_code(&context, "<")("<xxx").0.ok(), Some(("xxx", Rc::new(ast::OpCode { slice: "<", kind: ast::OpCodeKind::Lt }))));
    assert_eq!(lexer::op_code(&context, "<=")("<==xxx").0.ok(), Some(("=xxx", Rc::new(ast::OpCode { slice: "<=", kind: ast::OpCodeKind::Le }))));
    assert_eq!(lexer::op_code(&context, "<")("<=xxx").0.ok(), None);
}

#[test]
fn test_ident() {
    let context = Context::new().unwrap();
    assert_eq!(lexer::ident(&context)("A xxx").0.ok(), Some((" xxx", Rc::new(ast::Ident { slice: "A" }))));
    assert_eq!(lexer::ident(&context)("a xxx").0.ok(), Some((" xxx", Rc::new(ast::Ident { slice: "a" }))));
    assert_eq!(lexer::ident(&context)("AbC xxx").0.ok(), Some((" xxx", Rc::new(ast::Ident { slice: "AbC" }))));
    assert_eq!(lexer::ident(&context)("abc xxx").0.ok(), Some((" xxx", Rc::new(ast::Ident { slice: "abc" }))));
    assert_eq!(lexer::ident(&context)("ab1 xxx").0.ok(), Some((" xxx", Rc::new(ast::Ident { slice: "ab1" }))));
    assert_eq!(lexer::ident(&context)("1ab xxx").0.ok(), None);
    assert_eq!(lexer::ident(&context)("a_b xxx").0.ok(), Some((" xxx", Rc::new(ast::Ident { slice: "a_b" }))));
    assert_eq!(lexer::ident(&context)("_ab xxx").0.ok(), None);
}

#[test]
fn test_unit_literal() {
    let context = Context::new().unwrap();
    assert_eq!(
        lexer::unit_literal(&context)("()xxx").0.ok(),
        Some(("xxx", Rc::new(ast::Literal { slice: "()", kind: Rc::new(ast::LiteralKind::Unit { left: Rc::new(ast::OpCode { slice: "(", kind: ast::OpCodeKind::OpenParen }), right: Rc::new(ast::OpCode { slice: ")", kind: ast::OpCodeKind::CloseParen }) }) })))
    );
    assert_eq!(
        lexer::unit_literal(&context)("( )xxx").0.ok(),
        Some(("xxx", Rc::new(ast::Literal { slice: "( )", kind: Rc::new(ast::LiteralKind::Unit { left: Rc::new(ast::OpCode { slice: "(", kind: ast::OpCodeKind::OpenParen }), right: Rc::new(ast::OpCode { slice: ")", kind: ast::OpCodeKind::CloseParen }) }) })))
    );
}

#[test]
fn test_null_literal() {
    let context = Context::new().unwrap();
    assert_eq!(lexer::null_literal(&context)("null xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "null", kind: Rc::new(ast::LiteralKind::Null { keyword: Rc::new(ast::Keyword { slice: "null", kind: ast::KeywordKind::Null }) }) }))));
    assert_eq!(lexer::null_literal(&context)("nullxxx").0.ok(), None);
}

#[test]
fn test_bool_literal() {
    let context = Context::new().unwrap();
    assert_eq!(lexer::bool_literal(&context)("true xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "true", kind: Rc::new(ast::LiteralKind::Bool { keyword: Rc::new(ast::Keyword { slice: "true", kind: ast::KeywordKind::True }) }) }))));
    assert_eq!(lexer::bool_literal(&context)("false xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "false", kind: Rc::new(ast::LiteralKind::Bool { keyword: Rc::new(ast::Keyword { slice: "false", kind: ast::KeywordKind::False }) }) }))));
    assert_eq!(lexer::bool_literal(&context)("truexxx").0.ok(), None);
}

#[test]
fn test_integer_literal() {
    assert_eq!(lexer::integer_literal("123 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123", kind: Rc::new(ast::LiteralKind::PureInteger { slice: "123" }) }))));
    assert_eq!(lexer::integer_literal("1_2__3 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "1_2__3", kind: Rc::new(ast::LiteralKind::PureInteger { slice: "1_2__3" }) }))));
    assert_eq!(lexer::integer_literal("123L xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123L", kind: Rc::new(ast::LiteralKind::DecInteger { slice: "123L" }) }))));
    assert_eq!(lexer::integer_literal("123U xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123U", kind: Rc::new(ast::LiteralKind::DecInteger { slice: "123U" }) }))));
    assert_eq!(lexer::integer_literal("123LU xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123LU", kind: Rc::new(ast::LiteralKind::DecInteger { slice: "123LU" }) }))));
    assert_eq!(lexer::integer_literal("123UL xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123UL", kind: Rc::new(ast::LiteralKind::DecInteger { slice: "123UL" }) }))));
    assert_eq!(lexer::integer_literal("123xxx").0.ok(), None);
    assert_eq!(lexer::integer_literal("_123 xxx").0.ok(), None);
}

#[test]
fn test_hex_integer_literal() {
    assert_eq!(lexer::hex_integer_literal("0xFA3 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0xFA3", kind: Rc::new(ast::LiteralKind::HexInteger { slice: "0xFA3" }) }))));
    assert_eq!(lexer::hex_integer_literal("0XFA3 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0XFA3", kind: Rc::new(ast::LiteralKind::HexInteger { slice: "0XFA3" }) }))));
    assert_eq!(lexer::hex_integer_literal("0xfa3 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0xfa3", kind: Rc::new(ast::LiteralKind::HexInteger { slice: "0xfa3" }) }))));
    assert_eq!(lexer::hex_integer_literal("0x_F_A__3 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0x_F_A__3", kind: Rc::new(ast::LiteralKind::HexInteger { slice: "0x_F_A__3" }) }))));
    assert_eq!(lexer::hex_integer_literal("0xFA3L xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0xFA3L", kind: Rc::new(ast::LiteralKind::HexInteger { slice: "0xFA3L" }) }))));
    assert_eq!(lexer::hex_integer_literal("0xFA3U xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0xFA3U", kind: Rc::new(ast::LiteralKind::HexInteger { slice: "0xFA3U" }) }))));
    assert_eq!(lexer::hex_integer_literal("0xFA3LU xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0xFA3LU", kind: Rc::new(ast::LiteralKind::HexInteger { slice: "0xFA3LU" }) }))));
    assert_eq!(lexer::hex_integer_literal("0xFA3UL xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0xFA3UL", kind: Rc::new(ast::LiteralKind::HexInteger { slice: "0xFA3UL" }) }))));
    assert_eq!(lexer::hex_integer_literal("0xFA3xxx").0.ok(), None);
}

#[test]
fn test_bin_integer_literal() {
    assert_eq!(lexer::bin_integer_literal("0b101 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0b101", kind: Rc::new(ast::LiteralKind::BinInteger { slice: "0b101" }) }))));
    assert_eq!(lexer::bin_integer_literal("0B101 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0B101", kind: Rc::new(ast::LiteralKind::BinInteger { slice: "0B101" }) }))));
    assert_eq!(lexer::bin_integer_literal("0b_1_0__1 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0b_1_0__1", kind: Rc::new(ast::LiteralKind::BinInteger { slice: "0b_1_0__1" }) }))));
    assert_eq!(lexer::bin_integer_literal("0b101L xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0b101L", kind: Rc::new(ast::LiteralKind::BinInteger { slice: "0b101L" }) }))));
    assert_eq!(lexer::bin_integer_literal("0b101U xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0b101U", kind: Rc::new(ast::LiteralKind::BinInteger { slice: "0b101U" }) }))));
    assert_eq!(lexer::bin_integer_literal("0b101LU xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0b101LU", kind: Rc::new(ast::LiteralKind::BinInteger { slice: "0b101LU" }) }))));
    assert_eq!(lexer::bin_integer_literal("0b101UL xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "0b101UL", kind: Rc::new(ast::LiteralKind::BinInteger { slice: "0b101UL" }) }))));
    assert_eq!(lexer::bin_integer_literal("0b101xxx").0.ok(), None);
    assert_eq!(lexer::bin_integer_literal("0b123 xxx").0.ok(), None);
}

#[test]
fn test_real_number_literal() {
    assert_eq!(lexer::real_number_literal("12.345 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "12.345", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "12.345" }) }))));
    assert_eq!(lexer::real_number_literal("1__2.3_4__5 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "1__2.3_4__5", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "1__2.3_4__5" }) }))));
    assert_eq!(lexer::real_number_literal("12.345E67 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "12.345E67", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "12.345E67" }) }))));
    assert_eq!(lexer::real_number_literal("12.345e67 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "12.345e67", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "12.345e67" }) }))));
    assert_eq!(lexer::real_number_literal("12.345E+67 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "12.345E+67", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "12.345E+67" }) }))));
    assert_eq!(lexer::real_number_literal("12.345E-67 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "12.345E-67", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "12.345E-67" }) }))));
    assert_eq!(lexer::real_number_literal("12.345F xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "12.345F", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "12.345F" }) }))));
    assert_eq!(lexer::real_number_literal("12.345D xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "12.345D", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "12.345D" }) }))));
    assert_eq!(lexer::real_number_literal("12.345M xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "12.345M", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "12.345M" }) }))));
    assert_eq!(lexer::real_number_literal("12.345E-67D xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "12.345E-67D", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "12.345E-67D" }) }))));
    assert_eq!(lexer::real_number_literal("123E45 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123E45", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "123E45" }) }))));
    assert_eq!(lexer::real_number_literal("123e45 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123e45", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "123e45" }) }))));
    assert_eq!(lexer::real_number_literal("123E+45 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123E+45", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "123E+45" }) }))));
    assert_eq!(lexer::real_number_literal("123E-45 xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123E-45", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "123E-45" }) }))));
    assert_eq!(lexer::real_number_literal("123F xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123F", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "123F" }) }))));
    assert_eq!(lexer::real_number_literal("123D xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123D", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "123D" }) }))));
    assert_eq!(lexer::real_number_literal("123M xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123M", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "123M" }) }))));
    assert_eq!(lexer::real_number_literal("123E-45D xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "123E-45D", kind: Rc::new(ast::LiteralKind::RealNumber { slice: "123E-45D" }) }))));
    assert_eq!(lexer::real_number_literal("12.345xxx").0.ok(), None);
    assert_eq!(lexer::real_number_literal("123Fxxx").0.ok(), None);
    assert_eq!(lexer::real_number_literal("123 xxx").0.ok(), None);
}

#[test]
fn test_character_literal() {
    assert_eq!(lexer::character_literal("'a'xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "a", kind: Rc::new(ast::LiteralKind::Character { slice: "a" }) }))));
    assert_eq!(lexer::character_literal("' 'xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: " ", kind: Rc::new(ast::LiteralKind::Character { slice: " " }) }))));
    assert_eq!(lexer::character_literal("'\"'xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\"", kind: Rc::new(ast::LiteralKind::Character { slice: "\"" }) }))));
    assert_eq!(lexer::character_literal("'\\''xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\\'", kind: Rc::new(ast::LiteralKind::Character { slice: "\\'" }) }))));
    assert_eq!(lexer::character_literal("'\\\\'xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\\\\", kind: Rc::new(ast::LiteralKind::Character { slice: "\\\\" }) }))));
    assert_eq!(lexer::character_literal("'\\n'xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\\n", kind: Rc::new(ast::LiteralKind::Character { slice: "\\n" }) }))));
    assert_eq!(lexer::character_literal("'\\xF'xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\\xF", kind: Rc::new(ast::LiteralKind::Character { slice: "\\xF" }) }))));
    assert_eq!(lexer::character_literal("'\\xFFFF'xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\\xFFFF", kind: Rc::new(ast::LiteralKind::Character { slice: "\\xFFFF" }) }))));
    assert_eq!(lexer::character_literal("'\\uFFFF'xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\\uFFFF", kind: Rc::new(ast::LiteralKind::Character { slice: "\\uFFFF" }) }))));
    assert_eq!(lexer::character_literal("'\\UFFFFFFFF'xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\\UFFFFFFFF", kind: Rc::new(ast::LiteralKind::Character { slice: "\\UFFFFFFFF" }) }))));
    assert_eq!(lexer::character_literal("'ab'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("' a'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("'\\1'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("'\\xFFFFF'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("'\\uFFFFFFFF'xxx").0.ok(), None);
    assert_eq!(lexer::character_literal("'\\UFFFF'xxx").0.ok(), None);
}

#[test]
fn test_regular_string_literal() {
    assert_eq!(lexer::regular_string_literal("\"\"xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "", kind: Rc::new(ast::LiteralKind::RegularString { slice: "" }) }))));
    assert_eq!(lexer::regular_string_literal("\"abc\"xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "abc", kind: Rc::new(ast::LiteralKind::RegularString { slice: "abc" }) }))));
    assert_eq!(lexer::regular_string_literal("\"'\"xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "'", kind: Rc::new(ast::LiteralKind::RegularString { slice: "'" }) }))));
    assert_eq!(lexer::regular_string_literal("\"\\\"\"xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\\\"", kind: Rc::new(ast::LiteralKind::RegularString { slice: "\\\"" }) }))));
}

#[test]
fn test_verbatium_string_literal() {
    assert_eq!(lexer::verbatium_string_literal("@\"\"xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "", kind: Rc::new(ast::LiteralKind::VerbatiumString { slice: "" }) }))));
    assert_eq!(lexer::verbatium_string_literal("@\"abc\"xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "abc", kind: Rc::new(ast::LiteralKind::VerbatiumString { slice: "abc" }) }))));
    assert_eq!(lexer::verbatium_string_literal("@\"\"\"\"xxx").0.ok(), Some(("xxx", Rc::new(ast::Literal { slice: "\"\"", kind: Rc::new(ast::LiteralKind::VerbatiumString { slice: "\"\"" }) }))));
    assert_eq!(lexer::verbatium_string_literal("@\"\"\"xxx").0.ok(), None);
}

#[test]
fn test_this_literal() {
    let context = Context::new().unwrap();
    assert_eq!(lexer::this_literal(&context)("this xxx").0.ok(), Some((" xxx", Rc::new(ast::Literal { slice: "this", kind: Rc::new(ast::LiteralKind::This { keyword: Rc::new(ast::Keyword { slice: "this", kind: ast::KeywordKind::This }) }) }))));
    assert_eq!(lexer::this_literal(&context)("thisxxx").0.ok(), None);
}

#[test]
fn test_interpolated_string() {
    let context = Context::new().unwrap();
    assert_eq!(
        lexer::interpolated_string(&context)("$\"abc{123}def{val}ghi\"xxx").0.ok(),
        Some(("xxx", Rc::new(ast::InterpolatedString {
            slice: "abc{123}def{val}ghi",
            string_parts: vec!["abc", "def", "ghi"],
            exprs: vec![
                Rc::new(parser::ast::Expr {
                    slice: "123",
                    term: Rc::new(parser::ast::Term {
                        slice: "123",
                        kind: Rc::new(parser::ast::TermKind::Literal {
                            literal: Rc::new(ast::Literal { slice: "123", kind: Rc::new(ast::LiteralKind::PureInteger { slice: "123" }) })
                        }),
                    }),
                    ops: vec![],
                }),
                Rc::new(parser::ast::Expr {
                    slice: "val",
                    term: Rc::new(parser::ast::Term {
                        slice: "val",
                        kind: Rc::new(parser::ast::TermKind::EvalVar { ident: Rc::new(ast::Ident { slice: "val" }) }),
                    }),
                    ops: vec![],
                }),
            ],
        }))),
    );
    assert_eq!(lexer::interpolated_string(&context)("$\"abc{123\"xxx").0.ok(), None);
}

#[test]
fn test_eof() {
    assert_eq!(lexer::eof("").0.ok(), Some(("", ())));
    assert_eq!(lexer::eof("xxx").0.ok(), None);
}
