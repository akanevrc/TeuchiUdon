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
fn test_ty_expr() {
    let context = Context::new();
    let ty_int = elements::ty::Ty::new(
        &context,
        elements::base_ty::BaseTy::new(
            &context,
            elements::qual::Qual::TOP,
            "int".to_owned(),
            "SystemInt32".to_owned(),
        ),
        Vec::new(),
        "SystemInt32".to_owned(),
    );
    let parsed = parser::ty_expr(&context)("T::U::V").unwrap().1;
    assert_eq!(
        analyzer::ty_expr(&context, &parsed).ok(),
        Some(Rc::new(ast::TyExpr {
            detail: ast::TyExprDetail::InfixOp {
                parsed: Some(&parsed),
                left: Rc::new(ast::TyExpr {
                    detail: ast::TyExprDetail::InfixOp {
                        parsed: Some(&parsed),
                        left: Rc::new(ast::TyExpr {
                            detail: ast::TyExprDetail::Term {
                                parsed: Some(&parsed),
                                term: Rc::new(ast::TyTerm {
                                    detail: ast::TyTermDetail::EvalTy {
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
                        op: ast::TyOp::Access,
                        right: Rc::new(ast::TyExpr {
                            detail: ast::TyExprDetail::Term {
                                parsed: Some(&parsed),
                                term: Rc::new(ast::TyTerm {
                                    detail: ast::TyTermDetail::EvalTy {
                                        parsed: match &parsed.1[0] { parser::ast::TyOp::Access(_, x) => Some(x) },
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
                op: ast::TyOp::Access,
                right: Rc::new(ast::TyExpr {
                    detail: ast::TyExprDetail::Term {
                        parsed: Some(&parsed),
                        term: Rc::new(ast::TyTerm {
                            detail: ast::TyTermDetail::EvalTy {
                                parsed: match &parsed.1[1] { parser::ast::TyOp::Access(_, x) => Some(x) },
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
