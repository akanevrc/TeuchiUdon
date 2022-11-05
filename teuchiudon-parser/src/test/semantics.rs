use std::rc::Rc;
use crate::context::Context;
use crate::lexer;
use crate::parser;
use crate::semantics::{
    analyzer,
    ast,
    elements,
};

#[test]
fn test_type_expr() {
    let context = Context::new();
    let ty_int = elements::ty::BaseTy::new(
        &context,
        elements::qual::Qual::TOP,
        "int".to_owned(),
        "SystemInt32".to_owned(),
        Some("SystemInt32".to_owned()),
    ).direct();
    let parsed = parser::type_expr(&context)("T::U::V").unwrap().1;
    assert_eq!(
        analyzer::type_expr(&context, &parsed).ok(),
        Some(Rc::new(ast::TypeExpr {
            detail: ast::TypeExprDetail::InfixOp {
                parsed: Some(&parsed),
                left: Rc::new(ast::TypeExpr {
                    detail: ast::TypeExprDetail::InfixOp {
                        parsed: Some(&parsed),
                        left: Rc::new(ast::TypeExpr {
                            detail: ast::TypeExprDetail::Term {
                                parsed: Some(&parsed),
                                term: Rc::new(ast::TypeTerm {
                                    detail: ast::TypeTermDetail::EvalType {
                                        parsed: Some(&parsed.0),
                                        ident: ast::Ident {
                                            parsed: Some(&lexer::ast::Ident("T")),
                                            name: "T".to_owned(),
                                        },
                                    },
                                    ty: ty_int.clone(),
                                }),
                            },
                            ty: ty_int.clone(),
                        }),
                        op: ast::TypeOp::Access,
                        right: Rc::new(ast::TypeExpr {
                            detail: ast::TypeExprDetail::Term {
                                parsed: Some(&parsed),
                                term: Rc::new(ast::TypeTerm {
                                    detail: ast::TypeTermDetail::EvalType {
                                        parsed: match &parsed.1[0] { parser::ast::TypeOp::Access(_, x) => Some(x) },
                                        ident: ast::Ident {
                                            parsed: Some(&lexer::ast::Ident("U")),
                                            name: "U".to_owned(),
                                        },
                                    },
                                    ty: ty_int.clone(),
                                }),
                            },
                            ty: ty_int.clone(),
                        }),
                    },
                    ty: ty_int.clone(),
                }),
                op: ast::TypeOp::Access,
                right: Rc::new(ast::TypeExpr {
                    detail: ast::TypeExprDetail::Term {
                        parsed: Some(&parsed),
                        term: Rc::new(ast::TypeTerm {
                            detail: ast::TypeTermDetail::EvalType {
                                parsed: match &parsed.1[1] { parser::ast::TypeOp::Access(_, x) => Some(x) },
                                ident: ast::Ident {
                                    parsed: Some(&lexer::ast::Ident("V")),
                                    name: "V".to_owned(),
                                },
                            },
                            ty: ty_int.clone(),
                        }),
                    },
                    ty: ty_int.clone(),
                }),
            },
            ty: ty_int.clone(),
        })),
    )
}
