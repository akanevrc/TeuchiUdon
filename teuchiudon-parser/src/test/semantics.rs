use std::rc::Rc;
use crate::context::Context;
use crate::lexer;
use crate::parser;
use crate::semantics::{
    analyzer,
    ast,
};

#[test]
fn test_type_expr() {
    let context = Context::new();
    let parsed = parser::type_expr(&context)("T::U::V").unwrap().1;
    assert_eq!(
        analyzer::type_expr(&context, &parsed).ok(),
        Some(Rc::new(ast::TypeExpr::InfixOp {
            parsed: Some(&parsed),
            left: Rc::new(ast::TypeExpr::InfixOp {
                parsed: Some(&parsed),
                left: Rc::new(ast::TypeExpr::Term {
                    parsed: Some(&parsed),
                    term: Rc::new(ast::TypeTerm::EvalType {
                        parsed: Some(&parsed.0),
                        ident: ast::Ident {
                            parsed: Some(&lexer::ast::Ident("T")),
                            name: "T".to_owned(),
                        },
                    }),
                }),
                op: ast::TypeOp::Access,
                right: Rc::new(ast::TypeExpr::Term {
                    parsed: Some(&parsed),
                    term: Rc::new(ast::TypeTerm::EvalType {
                        parsed: match &parsed.1[0] { parser::ast::TypeOp::Access(_, x) => Some(x) },
                        ident: ast::Ident {
                            parsed: Some(&lexer::ast::Ident("U")),
                            name: "U".to_owned(),
                        },
                    }),
                }),
            }),
            op: ast::TypeOp::Access,
            right: Rc::new(ast::TypeExpr::Term {
                parsed: Some(&parsed),
                term: Rc::new(ast::TypeTerm::EvalType {
                    parsed: match &parsed.1[1] { parser::ast::TypeOp::Access(_, x) => Some(x) },
                    ident: ast::Ident {
                        parsed: Some(&lexer::ast::Ident("V")),
                        name: "V".to_owned(),
                    },
                }),
            }),
        })),
    )
}
