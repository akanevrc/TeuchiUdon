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
    let context = Context::new().unwrap();
    let ty_int = elements::ty::Ty::get_from_name(&context, "int").unwrap();
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
                                        parsed: Some(&parsed.ty_term),
                                        ident: ast::Ident {
                                            parsed: Some(&lexer::ast::Ident { slice: "T" }),
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
                                        parsed: match &parsed.ty_ops[0].kind {
                                            parser::ast::TyOpKind::Access { op_code: _, ty_term } => Some(ty_term),
                                        },
                                        ident: ast::Ident {
                                            parsed: Some(&lexer::ast::Ident { slice: "U" }),
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
                                parsed: match &parsed.ty_ops[1].kind {
                                    parser::ast::TyOpKind::Access { op_code: _, ty_term } => Some(ty_term),
                                },
                                ident: ast::Ident {
                                    parsed: Some(&lexer::ast::Ident { slice: "V" }),
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
