use std::{
    cell::RefCell,
    rc::Rc,
};
use crate::context::Context;
use crate::lexer;
use crate::parser;
use crate::semantics::elements::element::KeyElement;
use crate::semantics::{
    analyzer,
    ast,
    elements::{
        self,
        base_ty:: BaseTy,
        element::ValueElement,
        qual::{
            Qual,
            QualKey,
        },
        ty::Ty,
        var::VarKey,
    },
};

#[test]
fn test_ty_expr() {
    let context = Context::new().unwrap();
    let qual_t = Qual::new_or_get_quals(&context, vec!["T".to_owned()]).unwrap();
    let qual_u = Qual::new_or_get_quals(&context, vec!["T".to_owned(), "U".to_owned()]).unwrap();
    let ty_t = Ty::new_or_get_qual_from_key(&context, qual_t.to_key()).unwrap();
    let ty_u = Ty::new_or_get_qual_from_key(&context, qual_u.to_key()).unwrap();
    let ty_v = Ty::new_or_get_type_from_key(
        &context,
        Ty::new(
            &context,
            BaseTy::new(
                &context,
                qual_u.clone(),
                "V".to_owned(),
                "TUV".to_owned(),
            ).unwrap(),
            Vec::new(),
        ).unwrap().to_key(),
    ).unwrap();
    let ty_unknown = elements::ty::Ty::get_from_name(&context, "unknown".to_owned()).unwrap();
    let var_t = VarKey::new(QualKey::top(), "T".to_owned()).get_value(&context).unwrap();
    let var_u = VarKey::new(qual_t.to_key(), "U".to_owned()).get_value(&context).unwrap();
    let var_v = VarKey::new(qual_u.to_key(), "V".to_owned()).get_value(&context).unwrap();
    let parsed = parser::ty_expr(&context)("T::U::V").unwrap().1;
    assert_eq!(
        analyzer::ty_expr(&context, &parsed).ok(),
        Some(Rc::new(ast::TyExpr {
            parsed: Some(&parsed),
            detail: ast::TyExprDetail::InfixOp {
                left: Rc::new(ast::TyExpr {
                    parsed: Some(&parsed),
                    detail: ast::TyExprDetail::InfixOp {
                        left: Rc::new(ast::TyExpr {
                            parsed: Some(&parsed),
                            detail: ast::TyExprDetail::Term {
                                term: Rc::new(ast::TyTerm {
                                    parsed: Some(&parsed.ty_term),
                                    detail: ast::TyTermDetail::EvalTy {
                                        ident: ast::Ident {
                                            parsed: Some(&lexer::ast::Ident { slice: "T" }),
                                            name: "T".to_owned(),
                                        },
                                        var: RefCell::new(var_t),
                                    },
                                    ty: ty_t.clone(),
                                }),
                            },
                            ty: ty_t.clone(),
                        }),
                        op: ast::TyOp::Access,
                        right: Rc::new(ast::TyExpr {
                            parsed: Some(&parsed),
                            detail: ast::TyExprDetail::Term {
                                term: Rc::new(ast::TyTerm {
                                    parsed: match &parsed.ty_ops[0].kind {
                                        parser::ast::TyOpKind::Access { op_code: _, ty_term } => Some(ty_term),
                                    },
                                    detail: ast::TyTermDetail::EvalTy {
                                        ident: ast::Ident {
                                            parsed: Some(&lexer::ast::Ident { slice: "U" }),
                                            name: "U".to_owned(),
                                        },
                                        var: RefCell::new(var_u),
                                    },
                                    ty: ty_unknown.clone(),
                                }),
                            },
                            ty: ty_unknown.clone(),
                        }),
                    },
                    ty: ty_u.clone(),
                }),
                op: ast::TyOp::Access,
                right: Rc::new(ast::TyExpr {
                    parsed: Some(&parsed),
                    detail: ast::TyExprDetail::Term {
                        term: Rc::new(ast::TyTerm {
                            parsed: match &parsed.ty_ops[1].kind {
                                parser::ast::TyOpKind::Access { op_code: _, ty_term } => Some(ty_term),
                            },
                            detail: ast::TyTermDetail::EvalTy {
                                ident: ast::Ident {
                                    parsed: Some(&lexer::ast::Ident { slice: "V" }),
                                    name: "V".to_owned(),
                                },
                                var: RefCell::new(var_v),
                            },
                            ty: ty_unknown.clone(),
                        }),
                    },
                    ty: ty_unknown.clone(),
                }),
            },
            ty: ty_v.clone(),
        })),
    )
}
