use std::rc::Rc;
use crate::context::Context;
use crate::lexer;
use crate::parser::{
    self,
    ast,
};

#[test]
fn test_target() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::target(&context)("pub let x = 123; pub fn f() {};").ok(),
        Some(("", Rc::new(ast::Target {
            slice: "pub let x = 123; pub fn f() {};",
            body: Some(Rc::new(ast::Body {
                slice: "pub let x = 123; pub fn f() {};",
                top_stats: vec![
                    Rc::new(ast::TopStat {
                        slice: "pub let x = 123;",
                        kind: Rc::new(ast::TopStatKind::VarBind {
                            access_attr: Some(Rc::new(ast::AccessAttr { slice: "pub", attr: Rc::new(lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub }) })),
                            sync_attr: None,
                            var_bind: Rc::new(ast::VarBind {
                                slice: " let x = 123",
                                let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                                var_decl: Rc::new(ast::VarDecl {
                                    slice: " x",
                                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                                        ty_expr: None,
                                    }),
                                }),
                                expr: Rc::new(ast::Expr {
                                    slice: " 123",
                                    term: Rc::new(ast::Term {
                                        slice: " 123",
                                        kind: Rc::new(ast::TermKind::Literal {
                                            literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                                        }),
                                    }),
                                    ops: vec![]
                                }),
                            }),
                        }),
                    }),
                    Rc::new(ast::TopStat {
                        slice: " pub fn f() {};",
                        kind: Rc::new(ast::TopStatKind::FnBind {
                            access_attr: Some(Rc::new(ast::AccessAttr { slice: " pub", attr: Rc::new(lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub }) })),
                            fn_bind: Rc::new(ast::FnBind {
                                slice: " fn f() {}",
                                fn_keyword: Rc::new(lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn }),
                                fn_decl: Rc::new(ast::FnDecl {
                                    slice: " f()",
                                    ident: Rc::new(lexer::ast::Ident { slice: "f" }),
                                    var_decl: Rc::new(ast::VarDecl {
                                        slice: "()",
                                        kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![] }),
                                    }),
                                    ty_expr: None,
                                }),
                                stats_block: Rc::new(ast::StatsBlock {
                                    slice: " {}",
                                    stats: vec![],
                                    ret: None
                                }),
                            }),
                        }),
                    }),
                ],
            })),
        }))),
    );
}

#[test]
fn test_body() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::body(&context)("pub let x = 123; pub fn f() {};").ok(),
        Some(("", Rc::new(ast::Body {
            slice: "pub let x = 123; pub fn f() {};",
            top_stats: vec![
                Rc::new(ast::TopStat {
                    slice: "pub let x = 123;",
                    kind: Rc::new(ast::TopStatKind::VarBind {
                        access_attr: Some(Rc::new(ast::AccessAttr { slice: "pub", attr: Rc::new(lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub }) })),
                        sync_attr: None,
                        var_bind: Rc::new(ast::VarBind {
                            slice: " let x = 123",
                            let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                            var_decl: Rc::new(ast::VarDecl {
                                slice: " x",
                                kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                    mut_attr: None,
                                    ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                                    ty_expr: None,
                                }),
                            }),
                            expr: Rc::new(ast::Expr {
                                slice: " 123",
                                term: Rc::new(ast::Term {
                                    slice: " 123",
                                    kind: Rc::new(ast::TermKind::Literal {
                                        literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                                    }),
                                }),
                                ops: vec![],
                            }),
                        }),
                    }),
                }),
                Rc::new(ast::TopStat {
                    slice: " pub fn f() {};",
                    kind: Rc::new(ast::TopStatKind::FnBind {
                        access_attr: Some(Rc::new(ast::AccessAttr { slice: " pub", attr: Rc::new(lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub }) })),
                        fn_bind: Rc::new(ast::FnBind {
                            slice: " fn f() {}",
                            fn_keyword: Rc::new(lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn }),
                            fn_decl: Rc::new(ast::FnDecl {
                                slice: " f()",
                                ident: Rc::new(lexer::ast::Ident { slice: "f" }),
                                var_decl: Rc::new(ast::VarDecl {
                                    slice: "()",
                                    kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![] }),
                                }),
                                ty_expr: None,
                            }),
                            stats_block: Rc::new(ast::StatsBlock {
                                slice: " {}",
                                stats: vec![],
                                ret: None
                            }),
                        }),
                    }),
                }),
            ],
        }))),
    );
}

#[test]
fn test_var_bind_top_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::top_stat(&context)("pub sync let mut x = 123;").ok(),
        Some(("", Rc::new(ast::TopStat {
            slice: "pub sync let mut x = 123;",
            kind: Rc::new(ast::TopStatKind::VarBind {
                access_attr: Some(Rc::new(ast::AccessAttr { slice: "pub", attr: Rc::new(lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub }) })),
                sync_attr: Some(Rc::new(ast::SyncAttr { slice: " sync", attr: Rc::new(lexer::ast::Keyword { slice: "sync", kind: lexer::ast::KeywordKind::Sync }) })),
                var_bind: Rc::new(ast::VarBind {
                    slice: " let mut x = 123",
                    let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                    var_decl: Rc::new(ast::VarDecl {
                        slice: " mut x",
                        kind: Rc::new(ast::VarDeclKind::SingleDecl {
                            mut_attr: Some(Rc::new(ast::MutAttr { slice: " mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                            ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                            ty_expr: None,
                        }),
                    }),
                    expr: Rc::new(ast::Expr {
                        slice: " 123",
                        term: Rc::new(ast::Term {
                            slice: " 123",
                            kind: Rc::new(ast::TermKind::Literal {
                                literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                            }),
                        }),
                        ops: vec![],
                    }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_fn_bind_top_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::top_stat(&context)("pub fn f(x: int) -> int { x };").ok(),
        Some(("", Rc::new(ast::TopStat {
            slice: "pub fn f(x: int) -> int { x };",
            kind: Rc::new(ast::TopStatKind::FnBind {
                access_attr: Some(Rc::new(ast::AccessAttr { slice: "pub", attr: Rc::new(lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub })})),
                fn_bind: Rc::new(ast::FnBind {
                    slice: " fn f(x: int) -> int { x }",
                    fn_keyword: Rc::new(lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn }),
                    fn_decl: Rc::new(ast::FnDecl {
                        slice: " f(x: int) -> int",
                        ident: Rc::new(lexer::ast::Ident { slice: "f" }),
                        var_decl: Rc::new(ast::VarDecl {
                            slice: "(x: int)",
                            kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                                Rc::new(ast::VarDecl {
                                    slice: "x: int",
                                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                                        ty_expr: Some(Rc::new(ast::TyExpr {
                                            slice: " int",
                                            ty_term: Rc::new(ast::TyTerm {
                                                slice: " int",
                                                kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                                            }),
                                            ty_ops: vec![]
                                        })),
                                    }),
                                }),
                            ]}),
                        }),
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                            }),
                            ty_ops: vec![],
                        })),
                    }),
                    stats_block: Rc::new(ast::StatsBlock {
                        slice: " { x }",
                        stats: vec![],
                        ret: Some(Rc::new(ast::Expr {
                            slice: " x",
                            term: Rc::new(ast::Term {
                                slice: " x", kind:
                                Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                            }),
                            ops: vec![],
                        })),
                    }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_stat_top_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::top_stat(&context)("f();").ok(),
        Some(("", Rc::new(ast::TopStat {
            slice: "f();",
            kind: Rc::new(ast::TopStatKind::Stat {
                stat: Rc::new(ast::Stat {
                    slice: "f()",
                    kind: Rc::new(ast::StatKind::Expr {
                        expr: Rc::new(ast::Expr {
                            slice: "f()",
                            term: Rc::new(ast::Term {
                                slice: "f",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                            }),
                            ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                        }),
                    }),
                })
            }),
        }))),
    );
}

#[test]
fn test_var_bind() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::var_bind(&context)("let mut x: int = 123").ok(),
        Some(("", Rc::new(ast::VarBind {
            slice: "let mut x: int = 123",
            let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
            var_decl: Rc::new(ast::VarDecl {
                slice: " mut x: int",
                kind: Rc::new(ast::VarDeclKind::SingleDecl {
                    mut_attr: Some(Rc::new(ast::MutAttr { slice: " mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                    ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                    ty_expr: Some(Rc::new(ast::TyExpr {
                        slice: " int",
                        ty_term: Rc::new(ast::TyTerm {
                            slice: " int",
                            kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                        }),
                        ty_ops: vec![],
                    })),
                }),
            }),
            expr: Rc::new(ast::Expr {
                slice: " 123",
                term: Rc::new(ast::Term {
                    slice: " 123",
                    kind: Rc::new(ast::TermKind::Literal {
                        literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                    }),
                }),
                ops: vec![],
            }),
        }))),
    );
}

#[test]
fn test_single_var_decl() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::var_decl(&context)("x").ok(),
        Some(("", Rc::new(ast::VarDecl {
            slice: "x",
            kind: Rc::new(ast::VarDeclKind::SingleDecl {
                mut_attr: None,
                ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                ty_expr: None,
            }),
        }))),
    );
    assert_eq!(
        parser::var_decl(&context)("mut x").ok(),
        Some(("", Rc::new(ast::VarDecl {
            slice: "mut x",
            kind: Rc::new(ast::VarDeclKind::SingleDecl {
                mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                ty_expr: None,
            }),
        }))),
    );
    assert_eq!(
        parser::var_decl(&context)("mut x: int").ok(),
        Some(("", Rc::new(ast::VarDecl {
            slice: "mut x: int",
            kind: Rc::new(ast::VarDeclKind::SingleDecl {
                mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                ty_expr: Some(Rc::new(ast::TyExpr {
                    slice: " int",
                    ty_term: Rc::new(ast::TyTerm {
                        slice: " int",
                        kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                    }),
                    ty_ops: vec![],
                })),
            })
        }))),
    );
}

#[test]
fn test_tuple_var_decl() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::var_decl(&context)("()").ok(),
        Some(("", Rc::new(ast::VarDecl {
            slice: "()",
            kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![] })
        }))),
    );
    assert_eq!(
        parser::var_decl(&context)("(x)").ok(),
        Some(("", Rc::new(ast::VarDecl {
            slice: "(x)",
            kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                Rc::new(ast::VarDecl {
                    slice: "x",
                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                        mut_attr: None,
                        ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                        ty_expr: None
                    }),
                }),
            ]}),
        }))),
    );
    assert_eq!(
        parser::var_decl(&context)("(mut x: int, y)").ok(),
        Some(("", Rc::new(ast::VarDecl {
            slice: "(mut x: int, y)",
            kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                Rc::new(ast::VarDecl {
                    slice: "mut x: int",
                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                        mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                        ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                            }),
                            ty_ops: vec![],
                        })),
                    }),
                }),
                Rc::new(ast::VarDecl {
                    slice: " y",
                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                        mut_attr: None,
                        ident: Rc::new(lexer::ast::Ident { slice: "y" }),
                        ty_expr: None,
                    }),
                }),
            ]}),
        }))),
    );
    assert_eq!(
        parser::var_decl(&context)("(mut x: int, y,)").ok(),
        Some(("", Rc::new(ast::VarDecl {
            slice: "(mut x: int, y,)",
            kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                Rc::new(ast::VarDecl {
                    slice: "mut x: int",
                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                        mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                        ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                            }),
                            ty_ops: vec![],
                        })),
                    }),
                }),
                Rc::new(ast::VarDecl {
                    slice: " y",
                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                        mut_attr: None,
                        ident: Rc::new(lexer::ast::Ident { slice: "y" }),
                        ty_expr: None,
                    }),
                }),
            ]}),
        }))),
    );
}

#[test]
fn test_fn_bind() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::fn_bind(&context)("fn f(mut x: int, y) -> int { g(); x }").ok(),
        Some(("", Rc::new(ast::FnBind {
            slice: "fn f(mut x: int, y) -> int { g(); x }",
            fn_keyword: Rc::new(lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn }),
            fn_decl: Rc::new(ast::FnDecl {
                slice: " f(mut x: int, y) -> int",
                ident: Rc::new(lexer::ast::Ident { slice: "f" }),
                var_decl: Rc::new(ast::VarDecl {
                    slice: "(mut x: int, y)",
                    kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                        Rc::new(ast::VarDecl {
                            slice: "mut x: int",
                            kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                                ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                                ty_expr: Some(Rc::new(ast::TyExpr {
                                    slice: " int",
                                    ty_term: Rc::new(ast::TyTerm {
                                        slice: " int",
                                        kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                                    }),
                                    ty_ops: vec![],
                                })),
                            }),
                        }),
                        Rc::new(ast::VarDecl {
                            slice: " y",
                            kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                mut_attr: None,
                                ident: Rc::new(lexer::ast::Ident { slice: "y" }),
                                ty_expr: None
                            }),
                        }),
                    ]}),
                }),
                ty_expr: Some(Rc::new(ast::TyExpr {
                    slice: " int",
                    ty_term: Rc::new(ast::TyTerm {
                        slice: " int",
                        kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                    }),
                    ty_ops: vec![],
                })),
            }),
            stats_block: Rc::new(ast::StatsBlock {
                slice: " { g(); x }",
                stats: vec![
                    Rc::new(ast::Stat {
                        slice: " g()",
                        kind: Rc::new(ast::StatKind::Expr {
                            expr: Rc::new(ast::Expr {
                                slice: " g()",
                                term: Rc::new(ast::Term {
                                    slice: " g",
                                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "g" }) }),
                                }),
                                ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                            }),
                        }),
                    }),
                ],
                ret: Some(Rc::new(ast::Expr {
                    slice: " x",
                    term: Rc::new(ast::Term {
                        slice: " x",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                    }),
                    ops: vec![],
                })),
            }),
        }))),
    );
}

#[test]
fn test_fn_decl() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::fn_decl(&context)("f()").ok(),
        Some(("", Rc::new(ast::FnDecl {
            slice: "f()",
            ident: Rc::new(lexer::ast::Ident { slice: "f" }),
            var_decl: Rc::new(ast::VarDecl {
                slice: "()",
                kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![] }),
            }),
            ty_expr: None,
        }))),
    );
    assert_eq!(
        parser::fn_decl(&context)("f(mut x: int, y) -> int").ok(),
        Some(("", Rc::new(ast::FnDecl {
            slice: "f(mut x: int, y) -> int",
            ident: Rc::new(lexer::ast::Ident { slice: "f" }),
            var_decl: Rc::new(ast::VarDecl {
                slice: "(mut x: int, y)",
                kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                    Rc::new(ast::VarDecl {
                        slice: "mut x: int",
                        kind: Rc::new(ast::VarDeclKind::SingleDecl {
                            mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                            ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                            ty_expr: Some(Rc::new(ast::TyExpr {
                                slice: " int",
                                ty_term: Rc::new(ast::TyTerm {
                                    slice: " int",
                                    kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                                }),
                                ty_ops: vec![],
                            })),
                        }),
                    }),
                    Rc::new(ast::VarDecl {
                        slice: " y",
                        kind: Rc::new(ast::VarDeclKind::SingleDecl {
                            mut_attr: None,
                            ident: Rc::new(lexer::ast::Ident { slice: "y" }),
                            ty_expr: None
                        }),
                    }),
                ]}),
            }),
            ty_expr: Some(Rc::new(ast::TyExpr {
                slice: " int",
                ty_term: Rc::new(ast::TyTerm {
                    slice: " int",
                    kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                }),
                ty_ops: vec![],
            })),
        }))),
    );
}

#[test]
fn test_ty_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::ty_expr(&context)("T::U::V").ok(),
        Some(("", Rc::new(ast::TyExpr {
            slice: "T::U::V",
            ty_term: Rc::new(ast::TyTerm {
                slice: "T",
                kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "T" }) }),
            }),
            ty_ops: vec![
                Rc::new(ast::TyOp {
                    slice: "::U",
                    kind: Rc::new(ast::TyOpKind::Access {
                        op_code: Rc::new(lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon }),
                        ty_term: Rc::new(ast::TyTerm {
                            slice: "U",
                            kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "U" }) }),
                        }),
                    }),
                }),
                Rc::new(ast::TyOp {
                    slice: "::V",
                    kind: Rc::new(ast::TyOpKind::Access {
                        op_code: Rc::new(lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon }),
                        ty_term: Rc::new(ast::TyTerm {
                            slice: "V",
                            kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "V" }) }),
                        }),
                    }),
                }),
            ],
        }))),
    );
}

#[test]
fn test_ty_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::ty_op(&context)("::T").ok(),
        Some(("", Rc::new(ast::TyOp {
            slice: "::T",
            kind: Rc::new(ast::TyOpKind::Access {
                op_code: Rc::new(lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon }),
                ty_term: Rc::new(ast::TyTerm {
                    slice: "T",
                    kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "T" }) }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_ty_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::ty_term(&context)("string").ok(),
        Some(("", Rc::new(ast::TyTerm {
            slice: "string",
            kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "string" }) }),
        }))),
    );
}

#[test]
fn test_stats_block() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stats_block(&context)("{}").ok(),
        Some(("", Rc::new(ast::StatsBlock {
            slice: "{}",
            stats: vec![],
            ret: None
        }))),
    );
    assert_eq!(
        parser::stats_block(&context)("{ f(); x }").ok(),
        Some(("", Rc::new(ast::StatsBlock {
            slice: "{ f(); x }",
            stats: vec![
                Rc::new(ast::Stat {
                    slice: " f()",
                    kind: Rc::new(ast::StatKind::Expr {
                        expr: Rc::new(ast::Expr {
                            slice: " f()",
                            term: Rc::new(ast::Term {
                                slice: " f",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                            }),
                            ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                        }),
                    }),
                }),
            ],
            ret: Some(Rc::new(ast::Expr {
                slice: " x",
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
                ops: vec![],
            })),
        }))),
    );
    assert_eq!(
        parser::stats_block(&context)("{ f(); x; }").ok(),
        Some(("", Rc::new(ast::StatsBlock {
            slice: "{ f(); x; }",
            stats: vec![
                Rc::new(ast::Stat {
                    slice: " f()",
                    kind: Rc::new(ast::StatKind::Expr {
                        expr: Rc::new(ast::Expr {
                            slice: " f()",
                            term: Rc::new(ast::Term {
                                slice: " f",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                            }),
                            ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                        }),
                    }),
                }),
                Rc::new(ast::Stat {
                    slice: " x",
                    kind: Rc::new(ast::StatKind::Expr {
                        expr: Rc::new(ast::Expr {
                            slice: " x",
                            term: Rc::new(ast::Term {
                                slice: " x",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                            }),
                            ops: vec![],
                        }),
                    }),
                }),
            ],
            ret: None,
        }))),
    );
}

#[test]
fn test_return_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("return;").ok(),
        Some(("", Rc::new(ast::Stat {
            slice: "return",
            kind: Rc::new(ast::StatKind::Return {
                return_keyword: Rc::new(lexer::ast::Keyword { slice: "return", kind: lexer::ast::KeywordKind::Return }),
                expr: None
            })
        }))),
    );
    assert_eq!(
        parser::stat(&context)("return x;").ok(),
        Some(("", Rc::new(ast::Stat {
            slice: "return x",
            kind: Rc::new(ast::StatKind::Return {
                return_keyword: Rc::new(lexer::ast::Keyword { slice: "return", kind: lexer::ast::KeywordKind::Return }),
                expr: Some(Rc::new(ast::Expr {
                    slice: " x",
                    term: Rc::new(ast::Term {
                        slice: " x",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                    }),
                    ops: vec![],
                })),
            }),
        }))),
    );
}

#[test]
fn test_continue_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("continue;").ok(),
        Some(("", Rc::new(ast::Stat {
            slice: "continue",
            kind: Rc::new(ast::StatKind::Continue {
                continue_keyword: Rc::new(lexer::ast::Keyword { slice: "continue", kind: lexer::ast::KeywordKind::Continue }),
            }),
        }))),
    );
}

#[test]
fn test_break_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("break;").ok(),
        Some(("", Rc::new(ast::Stat {
            slice: "break",
            kind: Rc::new(ast::StatKind::Break {
                break_keyword: Rc::new(lexer::ast::Keyword { slice: "break", kind: lexer::ast::KeywordKind::Break }),
            }),
        }))),
    );
}

#[test]
fn test_var_bind_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("let x = 123;").ok(),
        Some(("", Rc::new(ast::Stat {
            slice: "let x = 123",
            kind: Rc::new(ast::StatKind::VarBind {
                var_bind: Rc::new(ast::VarBind {
                    slice: "let x = 123",
                    let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                    var_decl: Rc::new(ast::VarDecl {
                        slice: " x",
                        kind: Rc::new(ast::VarDeclKind::SingleDecl {
                            mut_attr: None,
                            ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                            ty_expr: None,
                        }),
                    }),
                    expr: Rc::new(ast::Expr {
                        slice: " 123",
                        term: Rc::new(ast::Term {
                            slice: " 123",
                            kind: Rc::new(ast::TermKind::Literal {
                                literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                            }),
                        }),
                        ops: vec![]
                    }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::stat(&context)("let mut x: int = 123;").ok(),
        Some(("", Rc::new(ast::Stat {
            slice: "let mut x: int = 123",
            kind: Rc::new(ast::StatKind::VarBind {
                var_bind: Rc::new(ast::VarBind {
                    slice: "let mut x: int = 123",
                    let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                    var_decl: Rc::new(ast::VarDecl {
                        slice: " mut x: int",
                        kind: Rc::new(ast::VarDeclKind::SingleDecl {
                            mut_attr: Some(Rc::new(ast::MutAttr { slice: " mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                            ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                            ty_expr: Some(Rc::new(ast::TyExpr {
                                slice: " int",
                                ty_term: Rc::new(ast::TyTerm {
                                    slice: " int",
                                    kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                                }),
                                ty_ops: vec![],
                            })),
                        }),
                    }),
                    expr: Rc::new(ast::Expr {
                        slice: " 123",
                        term: Rc::new(ast::Term {
                            slice: " 123",
                            kind: Rc::new(ast::TermKind::Literal {
                                literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                            }),
                        }),
                        ops: vec![],
                    }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::stat(&context)("let (mut x: int, y) = (123, 456);").ok(),
        Some(("", Rc::new(ast::Stat {
            slice: "let (mut x: int, y) = (123, 456)",
            kind: Rc::new(ast::StatKind::VarBind {
                var_bind: Rc::new(ast::VarBind {
                    slice: "let (mut x: int, y) = (123, 456)",
                    let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                    var_decl: Rc::new(ast::VarDecl {
                        slice: " (mut x: int, y)",
                        kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                            Rc::new(ast::VarDecl {
                                slice: "mut x: int",
                                kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                    mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                                    ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                                    ty_expr: Some(Rc::new(ast::TyExpr {
                                        slice: " int",
                                        ty_term: Rc::new(ast::TyTerm {
                                            slice: " int",
                                            kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                                        }),
                                        ty_ops: vec![],
                                    })),
                                }),
                            }),
                            Rc::new(ast::VarDecl {
                                slice: " y",
                                kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                    mut_attr: None,
                                    ident: Rc::new(lexer::ast::Ident { slice: "y" }),
                                    ty_expr: None
                                }),
                            }),
                        ]}),
                    }),
                    expr: Rc::new(ast::Expr {
                        slice: " (123, 456)",
                        term: Rc::new(ast::Term {
                            slice: " (123, 456)",
                            kind: Rc::new(ast::TermKind::Tuple { exprs: vec![
                                Rc::new(ast::Expr {
                                    slice: "123",
                                    term: Rc::new(ast::Term {
                                        slice: "123",
                                        kind: Rc::new(ast::TermKind::Literal {
                                            literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                                        }),
                                    }),
                                    ops: vec![],
                                }),
                                Rc::new(ast::Expr {
                                    slice: " 456",
                                    term: Rc::new(ast::Term {
                                        slice: " 456",
                                        kind: Rc::new(ast::TermKind::Literal {
                                            literal: Rc::new(lexer::ast::Literal { slice: "456", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "456" }) }),
                                        }),
                                    }),
                                    ops: vec![],
                                }),
                            ]}),
                        }),
                        ops: vec![]
                    }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_fn_bind_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("fn f(x: int) -> int { x };").ok(),
        Some(("", Rc::new(ast::Stat {
            slice: "fn f(x: int) -> int { x }",
            kind: Rc::new(ast::StatKind::FnBind {
                fn_bind: Rc::new(ast::FnBind {
                    slice: "fn f(x: int) -> int { x }",
                    fn_keyword: Rc::new(lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn }),
                    fn_decl: Rc::new(ast::FnDecl {
                        slice: " f(x: int) -> int",
                        ident: Rc::new(lexer::ast::Ident { slice: "f" }),
                        var_decl: Rc::new(ast::VarDecl {
                            slice: "(x: int)",
                            kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                                Rc::new(ast::VarDecl {
                                    slice: "x: int",
                                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                                        ty_expr: Some(Rc::new(ast::TyExpr {
                                            slice: " int",
                                            ty_term: Rc::new(ast::TyTerm {
                                                slice: " int",
                                                kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                                            }),
                                            ty_ops: vec![],
                                        })),
                                    }),
                                }),
                            ]}),
                        }),
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                            }),
                            ty_ops: vec![],
                        })),
                    }),
                    stats_block: Rc::new(ast::StatsBlock {
                        slice: " { x }",
                        stats: vec![],
                        ret: Some(Rc::new(ast::Expr {
                            slice: " x",
                            term: Rc::new(ast::Term {
                                slice: " x",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                            }),
                            ops: vec![]
                        })),
                    }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_expr_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("x = 123;").ok(),
        Some(("", Rc::new(ast::Stat {
            slice: "x = 123",
            kind: Rc::new(ast::StatKind::Expr {
                expr: Rc::new(ast::Expr {
                    slice: "x = 123",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                    }),
                    ops: vec![
                        Rc::new(ast::Op {
                            slice: " = 123",
                            kind: Rc::new(ast::OpKind::Assign {
                                term: Rc::new(ast::Term {
                                    slice: " 123",
                                    kind: Rc::new(ast::TermKind::Literal {
                                        literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                                    }),
                                }),
                            }),
                        }),
                    ],
                }),
            }),
        }))),
    );
}

#[test]
fn test_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::expr(&context)("x = T::f(1, 2).t + a.g(...b)[y]").ok(),
        Some(("", Rc::new(ast::Expr {
            slice: "x = T::f(1, 2).t + a.g(...b)[y]",
            term: Rc::new(ast::Term {
                slice: "x",
                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
            }),
            ops: vec![
                Rc::new(ast::Op {
                    slice: " = T",
                    kind: Rc::new(ast::OpKind::Assign {
                        term: Rc::new(ast::Term {
                            slice: " T",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "T" }) }),
                        }),
                    }),
                }),
                Rc::new(ast::Op {
                    slice: "::f",
                    kind: Rc::new(ast::OpKind::TyAccess {
                        op_code: Rc::new(lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon }),
                        term: Rc::new(ast::Term {
                            slice: "f",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                        }),
                    }),
                }),
                Rc::new(ast::Op {
                    slice: "(1, 2)",
                    kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![
                        Rc::new(ast::ArgExpr {
                            slice: "1",
                            mut_attr: None,
                            expr: Rc::new(ast::Expr {
                                slice: "1",
                                term: Rc::new(ast::Term {
                                    slice: "1",
                                    kind: Rc::new(ast::TermKind::Literal {
                                        literal: Rc::new(lexer::ast::Literal { slice: "1", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "1" }) }),
                                    }),
                                }),
                                ops: vec![],
                            }),
                        }),
                        Rc::new(ast::ArgExpr {
                            slice: " 2",
                            mut_attr: None,
                            expr: Rc::new(ast::Expr {
                                slice: " 2",
                                term: Rc::new(ast::Term {
                                    slice: " 2",
                                    kind: Rc::new(ast::TermKind::Literal {
                                        literal: Rc::new(lexer::ast::Literal { slice: "2", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "2" }) }),
                                    }),
                                }),
                                ops: vec![],
                            }),
                        }),
                    ]}),
                }),
                Rc::new(ast::Op {
                    slice: ".t",
                    kind: Rc::new(ast::OpKind::Access {
                        op_code: Rc::new(lexer::ast::OpCode { slice: ".", kind: lexer::ast::OpCodeKind::Dot }),
                        term: Rc::new(ast::Term {
                            slice: "t",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "t" }) }),
                        }),
                    }),
                }),
                Rc::new(ast::Op {
                    slice: " + a",
                    kind: Rc::new(ast::OpKind::InfixOp {
                        op_code: Rc::new(lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus }),
                        term: Rc::new(ast::Term {
                            slice: " a",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "a" }) }),
                        }),
                    }),
                }),
                Rc::new(ast::Op {
                    slice: ".g",
                    kind: Rc::new(ast::OpKind::Access {
                        op_code: Rc::new(lexer::ast::OpCode { slice: ".", kind: lexer::ast::OpCodeKind::Dot }),
                        term: Rc::new(ast::Term {
                            slice: "g",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "g" }) }),
                        }),
                    }),
                }),
                Rc::new(ast::Op {
                    slice: "(...b)",
                    kind: Rc::new(ast::OpKind::EvalSpreadFn {
                        expr: Rc::new(ast::Expr {
                            slice: "b",
                            term: Rc::new(ast::Term {
                                slice: "b",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "b" }) }),
                            }),
                            ops: vec![],
                        }),
                    }),
                }),
                Rc::new(ast::Op {
                    slice: "[y]",
                    kind: Rc::new(ast::OpKind::EvalKey {
                        expr: Rc::new(ast::Expr {
                            slice: "y",
                            term: Rc::new(ast::Term {
                                slice: "y",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "y" }) }),
                            }),
                            ops: vec![]
                        }),
                    }),
                }),
            ]
        }))),
    );
}

#[test]
fn test_ty_access_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("::x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "::x",
            kind: Rc::new(ast::OpKind::TyAccess {
                op_code: Rc::new(lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon }),
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_access_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)(".x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: ".x",
            kind: Rc::new(ast::OpKind::Access {
                op_code: Rc::new(lexer::ast::OpCode { slice: ".", kind: lexer::ast::OpCodeKind::Dot }),
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("?.x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "?.x",
            kind: Rc::new(ast::OpKind::Access {
                op_code: Rc::new(lexer::ast::OpCode { slice: "?.", kind: lexer::ast::OpCodeKind::CoalescingAccess }),
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_eval_fn_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("()").ok(),
        Some(("", Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) }))),
    );
    assert_eq!(
        parser::op(&context)("(mut x, y, z)").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "(mut x, y, z)",
            kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![
                Rc::new(ast::ArgExpr {
                    slice: "mut x",
                    mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                    expr: Rc::new(ast::Expr {
                        slice: " x",
                        term: Rc::new(ast::Term {
                            slice: " x",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                        }),
                        ops: vec![],
                    }),
                }),
                Rc::new(ast::ArgExpr {
                    slice: " y",
                    mut_attr: None,
                    expr: Rc::new(ast::Expr {
                        slice: " y",
                        term: Rc::new(ast::Term {
                            slice: " y",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "y" }) }),
                        }),
                        ops: vec![],
                    }),
                }),
                Rc::new(ast::ArgExpr {
                    slice: " z",
                    mut_attr: None,
                    expr: Rc::new(ast::Expr {
                        slice: " z",
                        term: Rc::new(ast::Term {
                            slice: " z",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "z" }) }),
                        }),
                        ops: vec![],
                    }),
                }),
            ]}),
        }))),
    );
    assert_eq!(
        parser::op(&context)("(mut x, y, z,)").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "(mut x, y, z,)",
            kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![
                Rc::new(ast::ArgExpr {
                    slice: "mut x",
                    mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                    expr: Rc::new(ast::Expr {
                        slice: " x",
                        term: Rc::new(ast::Term {
                            slice: " x",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                        }),
                        ops: vec![],
                    }),
                }),
                Rc::new(ast::ArgExpr {
                    slice: " y",
                    mut_attr: None,
                    expr: Rc::new(ast::Expr {
                        slice: " y",
                        term: Rc::new(ast::Term {
                            slice: " y",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "y" }) }),
                        }),
                        ops: vec![],
                    }),
                }),
                Rc::new(ast::ArgExpr {
                    slice: " z",
                    mut_attr: None,
                    expr: Rc::new(ast::Expr {
                        slice: " z",
                        term: Rc::new(ast::Term {
                            slice: " z",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "z" }) }),
                        }),
                        ops: vec![],
                    }),
                }),
            ]}),
        }))),
    );
}

#[test]
fn test_eval_spread_fn_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("(...x)").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "(...x)",
            kind: Rc::new(ast::OpKind::EvalSpreadFn {
                expr: Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
}

#[test]
fn test_eval_key_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("[x]").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "[x]",
            kind: Rc::new(ast::OpKind::EvalKey {
                expr: Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
}

#[test]
fn test_cast_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("as T").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "as T",
            kind: Rc::new(ast::OpKind::CastOp {
                as_keyword: Rc::new(lexer::ast::Keyword { slice: "as", kind: lexer::ast::KeywordKind::As }),
                ty_expr: Rc::new(ast::TyExpr {
                    slice: " T",
                    ty_term: Rc::new(ast::TyTerm {
                        slice: " T",
                        kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "T" }) }),
                    }),
                    ty_ops: vec![],
                }),
            }),
        }))),
    );
}

#[test]
fn test_infix_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("* x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "* x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "*", kind: lexer::ast::OpCodeKind::Star }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("/ x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "/ x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "/", kind: lexer::ast::OpCodeKind::Div }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("% x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "% x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "%", kind: lexer::ast::OpCodeKind::Percent }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("+ x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "+ x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("- x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "- x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "-", kind: lexer::ast::OpCodeKind::Minus }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("<< x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "<< x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "<<", kind: lexer::ast::OpCodeKind::LeftShift }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)(">> x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: ">> x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: ">>", kind: lexer::ast::OpCodeKind::RightShift }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("< x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "< x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "<", kind: lexer::ast::OpCodeKind::Lt }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("> x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "> x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: ">", kind: lexer::ast::OpCodeKind::Gt }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("<= x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "<= x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "<=", kind: lexer::ast::OpCodeKind::Le }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)(">= x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: ">= x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: ">=", kind: lexer::ast::OpCodeKind::Ge }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("== x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "== x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("!= x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "!= x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "!=", kind: lexer::ast::OpCodeKind::Ne }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("& x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "& x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "&", kind: lexer::ast::OpCodeKind::Amp }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("^ x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "^ x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "^", kind: lexer::ast::OpCodeKind::Caret }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("| x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "| x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "|", kind: lexer::ast::OpCodeKind::Pipe }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("&& x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "&& x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "&&", kind: lexer::ast::OpCodeKind::And }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("|| x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "|| x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "||", kind: lexer::ast::OpCodeKind::Or }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("?? x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "?? x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "??", kind: lexer::ast::OpCodeKind::Coalescing }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("|> x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "|> x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "|>", kind: lexer::ast::OpCodeKind::RightPipeline }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::op(&context)("<| x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "<| x",
            kind: Rc::new(ast::OpKind::InfixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "<|", kind: lexer::ast::OpCodeKind::LeftPipeline }),
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_assign_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("= x").ok(),
        Some(("", Rc::new(ast::Op {
            slice: "= x",
            kind: Rc::new(ast::OpKind::Assign {
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_prefix_op_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("+x").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "+x",
            kind: Rc::new(ast::TermKind::PrefixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus }),
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("-123").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "-123",
            kind: Rc::new(ast::TermKind::PrefixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "-", kind: lexer::ast::OpCodeKind::Minus }),
                term: Rc::new(ast::Term {
                    slice: "123",
                    kind: Rc::new(ast::TermKind::Literal {
                        literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                    }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("!false").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "!false",
            kind: Rc::new(ast::TermKind::PrefixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "!", kind: lexer::ast::OpCodeKind::Bang }),
                term: Rc::new(ast::Term {
                    slice: "false",
                    kind: Rc::new(ast::TermKind::Literal {
                        literal: Rc::new(lexer::ast::Literal {
                            slice: "false",
                            kind: Rc::new(lexer::ast::LiteralKind::Bool { keyword: Rc::new(lexer::ast::Keyword { slice: "false", kind: lexer::ast::KeywordKind::False }) }),
                        }),
                    }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("~0xFFFF").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "~0xFFFF",
            kind: Rc::new(ast::TermKind::PrefixOp {
                op_code: Rc::new(lexer::ast::OpCode { slice: "~", kind: lexer::ast::OpCodeKind::Tilde }),
                term: Rc::new(ast::Term {
                    slice: "0xFFFF",
                    kind: Rc::new(ast::TermKind::Literal {
                        literal: Rc::new(lexer::ast::Literal { slice: "0xFFFF", kind: Rc::new(lexer::ast::LiteralKind::HexInteger { slice: "0xFFFF" }) })
                    })
                }),
            }),
        }))),
    );
}

#[test]
fn test_block_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("{ f(); g(); x }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "{ f(); g(); x }",
            kind: Rc::new(ast::TermKind::Block {
                stats: Rc::new(ast::StatsBlock {
                    slice: "{ f(); g(); x }",
                    stats: vec![
                        Rc::new(ast::Stat {
                            slice: " f()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                        Rc::new(ast::Stat {
                            slice: " g()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " g()",
                                    term: Rc::new(ast::Term {
                                        slice: " g",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "g" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                    ],
                    ret: Some(Rc::new(ast::Expr {
                        slice: " x",
                        term: Rc::new(ast::Term {
                            slice: " x",
                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                        }),
                        ops: vec![],
                    })),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("{ f(); g(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "{ f(); g(); }",
            kind: Rc::new(ast::TermKind::Block {
                stats: Rc::new(ast::StatsBlock {
                    slice: "{ f(); g(); }",
                    stats: vec![
                        Rc::new(ast::Stat {
                            slice: " f()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                        Rc::new(ast::Stat {
                            slice: " g()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " g()",
                                    term: Rc::new(ast::Term {
                                        slice: " g",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "g" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                    ],
                    ret: None,
                }),
            }),
        }))),
    );
}

#[test]
fn test_paren_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("(x)").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "(x)",
            kind: Rc::new(ast::TermKind::Paren {
                expr: Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
}

#[test]
fn test_tuple_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("(1, 2, 3)").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "(1, 2, 3)",
            kind: Rc::new(ast::TermKind::Tuple { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "1",
                    term: Rc::new(ast::Term {
                        slice: "1",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "1", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "1" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " 2",
                    term: Rc::new(ast::Term {
                        slice: " 2",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "2", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "2" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " 3",
                    term: Rc::new(ast::Term {
                        slice: " 3",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "3", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "3" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
            ]}),
        }))),
    );
    assert_eq!(
        parser::term(&context)("(1, 2, 3,)").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "(1, 2, 3,)",
            kind: Rc::new(ast::TermKind::Tuple { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "1",
                    term: Rc::new(ast::Term {
                        slice: "1",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "1", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "1" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " 2",
                    term: Rc::new(ast::Term {
                        slice: " 2",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "2", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "2" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " 3",
                    term: Rc::new(ast::Term {
                        slice: " 3",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "3", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "3" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
            ]}),
        }))),
    );
    assert_eq!(
        parser::term(&context)("(1,)").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "(1,)",
            kind: Rc::new(ast::TermKind::Tuple { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "1",
                    term: Rc::new(ast::Term {
                        slice: "1",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "1", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "1" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
            ]}),
        }))),
    );
}

#[test]
fn test_array_ctor_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("[]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[]",
            kind: Rc::new(ast::TermKind::ArrayCtor { iter_expr: None }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("[0..10]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[0..10]",
            kind: Rc::new(ast::TermKind::ArrayCtor {
                iter_expr: Some(Rc::new(ast::IterExpr {
                    slice: "0..10",
                    kind: Rc::new(ast::IterExprKind::Range {
                        left: Rc::new(ast::Expr {
                            slice: "0",
                            term: Rc::new(ast::Term {
                                slice: "0",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                        right: Rc::new(ast::Expr {
                            slice: "10",
                            term: Rc::new(ast::Term {
                                slice: "10",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "10", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "10" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                    }),
                })),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("[0..10..2]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[0..10..2]",
            kind: Rc::new(ast::TermKind::ArrayCtor {
                iter_expr: Some(Rc::new(ast::IterExpr {
                    slice: "0..10..2",
                    kind: Rc::new(ast::IterExprKind::SteppedRange {
                        left: Rc::new(ast::Expr {
                            slice: "0",
                            term: Rc::new(ast::Term {
                                slice: "0",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                        right: Rc::new(ast::Expr {
                            slice: "10",
                            term: Rc::new(ast::Term {
                                slice: "10",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "10", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "10" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                        step: Rc::new(ast::Expr {
                            slice: "2",
                            term: Rc::new(ast::Term {
                                slice: "2",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "2", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "2" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                    }),
                })),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("[...x]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[...x]",
            kind: Rc::new(ast::TermKind::ArrayCtor {
                iter_expr: Some(Rc::new(ast::IterExpr {
                    slice: "...x",
                    kind: Rc::new(ast::IterExprKind::Spread {
                        expr: Rc::new(ast::Expr {
                            slice: "x",
                            term: Rc::new(ast::Term {
                                slice: "x",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                            }),
                            ops: vec![],
                        }),
                    }),
                })),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[1, 2, 3]",
            kind: Rc::new(ast::TermKind::ArrayCtor {
                iter_expr: Some(Rc::new(ast::IterExpr {
                    slice: "1, 2, 3",
                    kind: Rc::new(ast::IterExprKind::Elements { exprs: vec![
                        Rc::new(ast::Expr {
                            slice: "1",
                            term: Rc::new(ast::Term {
                                slice: "1",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "1", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "1" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                        Rc::new(ast::Expr {
                            slice: " 2",
                            term: Rc::new(ast::Term {
                                slice: " 2",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "2", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "2" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                        Rc::new(ast::Expr {
                            slice: " 3",
                            term: Rc::new(ast::Term {
                                slice: " 3",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "3", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "3" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                    ]}),
                })),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3,]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[1, 2, 3,]",
            kind: Rc::new(ast::TermKind::ArrayCtor {
                iter_expr: Some(Rc::new(ast::IterExpr {
                    slice: "1, 2, 3,",
                    kind: Rc::new(ast::IterExprKind::Elements { exprs: vec![
                        Rc::new(ast::Expr {
                            slice: "1",
                            term: Rc::new(ast::Term {
                                slice: "1",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "1", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "1" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                        Rc::new(ast::Expr {
                            slice: " 2",
                            term: Rc::new(ast::Term {
                                slice: " 2",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "2", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "2" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                        Rc::new(ast::Expr {
                            slice: " 3",
                            term: Rc::new(ast::Term {
                                slice: " 3",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "3", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "3" }) }),
                                }),
                            }),
                            ops: vec![],
                        }),
                    ]}),
                })),
            }),
        }))),
    );
}

#[test]
fn test_literal_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("()").ok(),
        Some((
            "",
            Rc::new(ast::Term {
                slice: "()",
                kind: Rc::new(ast::TermKind::Literal {
                    literal: Rc::new(lexer::ast::Literal {
                        slice: "()",
                        kind: Rc::new(lexer::ast::LiteralKind::Unit {
                            left: Rc::new(lexer::ast::OpCode { slice: "(", kind: lexer::ast::OpCodeKind::OpenParen }),
                            right: Rc::new(lexer::ast::OpCode { slice: ")", kind: lexer::ast::OpCodeKind::CloseParen }),
                        }),
                    }),
                }),
            }),
        )),
    );
    assert_eq!(
        parser::term(&context)("null").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "null",
            kind: Rc::new(ast::TermKind::Literal {
                literal: Rc::new(lexer::ast::Literal {
                    slice: "null",
                    kind: Rc::new(lexer::ast::LiteralKind::Null { keyword: Rc::new(lexer::ast::Keyword { slice: "null", kind: lexer::ast::KeywordKind::Null }) })
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("true").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "true",
            kind: Rc::new(ast::TermKind::Literal {
                literal: Rc::new(lexer::ast::Literal {
                    slice: "true",
                    kind: Rc::new(lexer::ast::LiteralKind::Bool { keyword: Rc::new(lexer::ast::Keyword { slice: "true", kind: lexer::ast::KeywordKind::True }) }),
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("false").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "false",
            kind: Rc::new(ast::TermKind::Literal {
                literal: Rc::new(lexer::ast::Literal {
                    slice: "false",
                    kind: Rc::new(lexer::ast::LiteralKind::Bool { keyword: Rc::new(lexer::ast::Keyword { slice: "false", kind: lexer::ast::KeywordKind::False }) }),
                }),
            }),
        }))),
    );
    assert_eq!(parser::term(&context)("123.45").ok(), Some(("", Rc::new(ast::Term {
        slice: "123.45",
        kind: Rc::new(ast::TermKind::Literal { literal: Rc::new(lexer::ast::Literal { slice: "123.45", kind: Rc::new(lexer::ast::LiteralKind::RealNumber { slice: "123.45" }) }) })
    }))));
    assert_eq!(parser::term(&context)("0x1AF").ok(), Some(("", Rc::new(ast::Term {
        slice: "0x1AF",
        kind: Rc::new(ast::TermKind::Literal { literal: Rc::new(lexer::ast::Literal { slice: "0x1AF", kind: Rc::new(lexer::ast::LiteralKind::HexInteger { slice: "0x1AF" }) }) })
    }))));
    assert_eq!(parser::term(&context)("0b101").ok(), Some(("", Rc::new(ast::Term {
        slice: "0b101",
        kind: Rc::new(ast::TermKind::Literal { literal: Rc::new(lexer::ast::Literal { slice: "0b101", kind: Rc::new(lexer::ast::LiteralKind::BinInteger { slice: "0b101" }) }) })
    }))));
    assert_eq!(parser::term(&context)("123").ok(), Some(("", Rc::new(ast::Term {
        slice: "123",
        kind: Rc::new(ast::TermKind::Literal {
            literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
        }),
    }))));
    assert_eq!(parser::term(&context)("'a'").ok(), Some(("", Rc::new(ast::Term {
        slice: "'a'",
        kind: Rc::new(ast::TermKind::Literal { literal: Rc::new(lexer::ast::Literal { slice: "a", kind: Rc::new(lexer::ast::LiteralKind::Character { slice: "a" }) }) })
    }))));
    assert_eq!(parser::term(&context)("\"abc\"").ok(), Some(("", Rc::new(ast::Term {
        slice: "\"abc\"",
        kind: Rc::new(ast::TermKind::Literal { literal: Rc::new(lexer::ast::Literal { slice: "abc", kind: Rc::new(lexer::ast::LiteralKind::RegularString { slice: "abc" }) }) })
    }))));
    assert_eq!(parser::term(&context)("@\"\\abc\"").ok(), Some(("", Rc::new(ast::Term {
        slice: "@\"\\abc\"",
        kind: Rc::new(ast::TermKind::Literal { literal: Rc::new(lexer::ast::Literal { slice: "\\abc", kind: Rc::new(lexer::ast::LiteralKind::VerbatiumString { slice: "\\abc" }) }) })
    }))));
}

#[test]
fn test_this_literal_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("this").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "this",
            kind: Rc::new(ast::TermKind::ThisLiteral {
                literal: Rc::new(lexer::ast::Literal {
                    slice: "this",
                    kind: Rc::new(lexer::ast::LiteralKind::This { keyword: Rc::new(lexer::ast::Keyword { slice: "this", kind: lexer::ast::KeywordKind::This }) })
                }),
            }),
        }))),
    );
}

#[test]
fn test_interpolated_string_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("$\"abc{123}{x}def\"").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "$\"abc{123}{x}def\"",
            kind: Rc::new(ast::TermKind::InterpolatedString {
                interpolated_string: Rc::new(lexer::ast::InterpolatedString {
                    slice: "abc{123}{x}def",
                    string_parts: vec![
                        "abc",
                        "",
                        "def",
                    ],
                    exprs: vec![
                        Rc::new(ast::Expr {
                            slice: "123",
                            term: Rc::new(ast::Term {
                                slice: "123",
                                kind: Rc::new(ast::TermKind::Literal {
                                    literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                                }),
                            }),
                            ops: vec![]
                        }),
                        Rc::new(ast::Expr {
                            slice: "x",
                            term: Rc::new(ast::Term {
                                slice: "x",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                            }),
                            ops: vec![],
                        }),
                    ],
                }),
            }),
        }))),
    );
}

#[test]
fn test_eval_var_term() {
    let context = Context::new().unwrap();
    assert_eq!(parser::term(&context)("someVar").ok(), Some(("", Rc::new(ast::Term {
        slice: "someVar",
        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "someVar" }) }),
    }))));
    assert_eq!(parser::term(&context)("some_var").ok(), Some(("", Rc::new(ast::Term {
        slice: "some_var",
        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "some_var" }) }),
    }))));
}

#[test]
fn test_let_in_bind_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("let mut i: int = 123 in i + 1").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "let mut i: int = 123 in i + 1",
            kind: Rc::new(ast::TermKind::LetInBind {
                var_bind: Rc::new(ast::VarBind {
                    slice: "let mut i: int = 123",
                    let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                    var_decl: Rc::new(ast::VarDecl {
                        slice: " mut i: int",
                        kind: Rc::new(ast::VarDeclKind::SingleDecl {
                            mut_attr: Some(Rc::new(ast::MutAttr { slice: " mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
                            ident: Rc::new(lexer::ast::Ident { slice: "i" }),
                            ty_expr: Some(Rc::new(ast::TyExpr {
                                slice: " int",
                                ty_term: Rc::new(ast::TyTerm {
                                    slice: " int",
                                    kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                                }),
                                ty_ops: vec![],
                            })),
                        }),
                    }),
                    expr: Rc::new(ast::Expr {
                        slice: " 123",
                        term: Rc::new(ast::Term {
                            slice: " 123",
                            kind: Rc::new(ast::TermKind::Literal {
                                literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                            }),
                        }),
                        ops: vec![]
                    }),
                }),
                in_keyword: Rc::new(lexer::ast::Keyword { slice: "in", kind: lexer::ast::KeywordKind::In }),
                expr: Rc::new(ast::Expr {
                    slice: " i + 1",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "i" }) }),
                    }),
                    ops: vec![
                        Rc::new(ast::Op {
                            slice: " + 1",
                            kind: Rc::new(ast::OpKind::InfixOp {
                                op_code: Rc::new(lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus }),
                                term: Rc::new(ast::Term {
                                    slice: " 1",
                                    kind: Rc::new(ast::TermKind::Literal {
                                        literal: Rc::new(lexer::ast::Literal { slice: "1", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "1" }) }),
                                    }),
                                }),
                            }),
                        }),
                    ],
                }),
            }),
        }))),
    );
}

#[test]
fn test_if_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "if i == 0 { f(); }",
            kind: Rc::new(ast::TermKind::If {
                if_keyword: Rc::new(lexer::ast::Keyword { slice: "if", kind: lexer::ast::KeywordKind::If }),
                condition: Rc::new(ast::Expr {
                    slice: " i == 0",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "i" }) }),
                    }),
                    ops: vec![
                        Rc::new(ast::Op {
                            slice: " == 0",
                            kind: Rc::new(ast::OpKind::InfixOp {
                                op_code: Rc::new(lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq }),
                                term: Rc::new(ast::Term {
                                    slice: " 0",
                                    kind: Rc::new(ast::TermKind::Literal {
                                        literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                                    }),
                                }),
                            }),
                        }),
                    ],
                }),
                if_part: Rc::new(ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        Rc::new(ast::Stat {
                            slice: " f()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                    ],
                    ret: None,
                }),
                else_part: None,
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); } else { g(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "if i == 0 { f(); } else { g(); }",
            kind: Rc::new(ast::TermKind::If {
                if_keyword: Rc::new(lexer::ast::Keyword { slice: "if", kind: lexer::ast::KeywordKind::If }),
                condition: Rc::new(ast::Expr {
                    slice: " i == 0",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "i" }) }),
                    }),
                    ops: vec![
                        Rc::new(ast::Op {
                            slice: " == 0",
                            kind: Rc::new(ast::OpKind::InfixOp {
                                op_code: Rc::new(lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq }),
                                term: Rc::new(ast::Term {
                                    slice: " 0",
                                    kind: Rc::new(ast::TermKind::Literal {
                                        literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                                    }),
                                }),
                            }),
                        }),
                    ],
                }),
                if_part: Rc::new(ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        Rc::new(ast::Stat {
                            slice: " f()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                    ],
                    ret: None,
                }),
                else_part: Some((
                    Rc::new(lexer::ast::Keyword { slice: "else", kind: lexer::ast::KeywordKind::Else }),
                    Rc::new(ast::StatsBlock {
                        slice: " { g(); }",
                        stats: vec![
                            Rc::new(ast::Stat {
                                slice: " g()",
                                kind: Rc::new(ast::StatKind::Expr {
                                    expr: Rc::new(ast::Expr {
                                        slice: " g()",
                                        term: Rc::new(ast::Term {
                                            slice: " g",
                                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "g" }) }),
                                        }),
                                        ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                    }),
                                }),
                            }),
                        ],
                        ret: None,
                    }),
                )),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); } else if j == 0 { g(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "if i == 0 { f(); } else if j == 0 { g(); }",
            kind: Rc::new(ast::TermKind::If {
                if_keyword: Rc::new(lexer::ast::Keyword { slice: "if", kind: lexer::ast::KeywordKind::If }),
                condition: Rc::new(ast::Expr {
                    slice: " i == 0",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "i" }) }),
                    }),
                    ops: vec![
                        Rc::new(ast::Op {
                            slice: " == 0",
                            kind: Rc::new(ast::OpKind::InfixOp {
                                op_code: Rc::new(lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq }),
                                term: Rc::new(ast::Term {
                                    slice: " 0",
                                    kind: Rc::new(ast::TermKind::Literal {
                                        literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                                    }),
                                }),
                            }),
                        }),
                    ]
                }),
                if_part: Rc::new(ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        Rc::new(ast::Stat {
                            slice: " f()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                    ],
                    ret: None,
                }),
                else_part: Some((
                    Rc::new(lexer::ast::Keyword { slice: "else", kind: lexer::ast::KeywordKind::Else }),
                    Rc::new(ast::StatsBlock {
                        slice: " if j == 0 { g(); }",
                        stats: vec![],
                        ret: Some(Rc::new(ast::Expr {
                            slice: " if j == 0 { g(); }",
                            term: Rc::new(ast::Term {
                                slice: " if j == 0 { g(); }",
                                kind: Rc::new(ast::TermKind::If {
                                    if_keyword: Rc::new(lexer::ast::Keyword { slice: "if", kind: lexer::ast::KeywordKind::If }),
                                    condition: Rc::new(ast::Expr {
                                        slice: " j == 0",
                                        term: Rc::new(ast::Term {
                                            slice: " j",
                                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "j" }) }),
                                        }),
                                        ops: vec![
                                            Rc::new(ast::Op {
                                                slice: " == 0",
                                                kind: Rc::new(ast::OpKind::InfixOp {
                                                    op_code: Rc::new(lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq }),
                                                    term: Rc::new(ast::Term {
                                                        slice: " 0",
                                                        kind: Rc::new(ast::TermKind::Literal {
                                                            literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                                                        }),
                                                    }),
                                                }),
                                            }),
                                        ],
                                    }),
                                    if_part: Rc::new(ast::StatsBlock {
                                        slice: " { g(); }",
                                        stats: vec![
                                            Rc::new(ast::Stat {
                                                slice: " g()",
                                                kind: Rc::new(ast::StatKind::Expr {
                                                    expr: Rc::new(ast::Expr {
                                                        slice: " g()",
                                                        term: Rc::new(ast::Term {
                                                            slice: " g",
                                                            kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "g" }) }),
                                                        }),
                                                        ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                                    }),
                                                }),
                                            }),
                                        ],
                                        ret: None,
                                    }),
                                    else_part: None,
                                }),
                            }),
                            ops: vec![],
                        })),
                    }),
                )),
            }),
        }))),
    );
}

#[test]
fn test_while_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("while i == 0 { f(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "while i == 0 { f(); }",
            kind: Rc::new(ast::TermKind::While {
                while_keyword: Rc::new(lexer::ast::Keyword { slice: "while", kind: lexer::ast::KeywordKind::While }),
                condition: Rc::new(ast::Expr {
                    slice: " i == 0",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "i" }) }),
                    }),
                    ops: vec![
                        Rc::new(ast::Op {
                            slice: " == 0",
                            kind: Rc::new(ast::OpKind::InfixOp {
                                op_code: Rc::new(lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq }),
                                term: Rc::new(ast::Term {
                                    slice: " 0",
                                    kind: Rc::new(ast::TermKind::Literal {
                                        literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                                    }),
                                }),
                            }),
                        }),
                    ],
                }),
                stats: Rc::new(ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        Rc::new(ast::Stat {
                            slice: " f()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                    ],
                    ret: None,
                }),
            }),
        }))),
    );
}

#[test]
fn test_loop_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("loop { f(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "loop { f(); }",
            kind: Rc::new(ast::TermKind::Loop {
                loop_keyword: Rc::new(lexer::ast::Keyword { slice: "loop", kind: lexer::ast::KeywordKind::Loop }),
                stats: Rc::new(ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        Rc::new(ast::Stat {
                            slice: " f()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                    ],
                    ret: None,
                }),
            }),
        }))),
    );
}

#[test]
fn test_for_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("for let i <- 0..10 for let j <- 0..10..2 for k <- arr { f(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "for let i <- 0..10 for let j <- 0..10..2 for k <- arr { f(); }",
            kind: Rc::new(ast::TermKind::For {
                for_binds: vec![
                    (
                        Rc::new(lexer::ast::Keyword { slice: "for", kind: lexer::ast::KeywordKind::For }),
                        Rc::new(ast::ForBind {
                            slice: " let i <- 0..10",
                            kind: Rc::new(ast::ForBindKind::Let {
                                let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                                var_decl: Rc::new(ast::VarDecl {
                                    slice: " i",
                                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: Rc::new(lexer::ast::Ident { slice: "i" }),
                                        ty_expr: None
                                    }),
                                }),
                                for_iter_expr: Rc::new(ast::ForIterExpr {
                                    slice: " 0..10",
                                    kind: Rc::new(ast::ForIterExprKind::Range {
                                        left: Rc::new(ast::Expr {
                                            slice: " 0",
                                            term: Rc::new(ast::Term {
                                                slice: " 0",
                                                kind: Rc::new(ast::TermKind::Literal {
                                                    literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                                                }),
                                            }),
                                            ops: vec![],
                                        }),
                                        right: Rc::new(ast::Expr {
                                            slice: "10",
                                            term: Rc::new(ast::Term {
                                                slice: "10",
                                                kind: Rc::new(ast::TermKind::Literal {
                                                    literal: Rc::new(lexer::ast::Literal { slice: "10", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "10" }) }),
                                                }),
                                            }),
                                            ops: vec![],
                                        }),
                                    }),
                                }),
                            }),
                        }),
                    ),
                    (
                        Rc::new(lexer::ast::Keyword { slice: "for", kind: lexer::ast::KeywordKind::For }),
                        Rc::new(ast::ForBind {
                            slice: " let j <- 0..10..2",
                            kind: Rc::new(ast::ForBindKind::Let {
                                let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                                var_decl: Rc::new(ast::VarDecl {
                                    slice: " j",
                                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: Rc::new(lexer::ast::Ident { slice: "j" }),
                                        ty_expr: None,
                                    }),
                                }),
                                for_iter_expr: Rc::new(ast::ForIterExpr {
                                    slice: " 0..10..2",
                                    kind: Rc::new(ast::ForIterExprKind::SteppedRange {
                                        left: Rc::new(ast::Expr {
                                            slice: " 0",
                                            term: Rc::new(ast::Term {
                                                slice: " 0",
                                                kind: Rc::new(ast::TermKind::Literal {
                                                    literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                                                }),
                                            }),
                                            ops: vec![],
                                        }),
                                        right: Rc::new(ast::Expr {
                                            slice: "10",
                                            term: Rc::new(ast::Term {
                                                slice: "10",
                                                kind: Rc::new(ast::TermKind::Literal {
                                                    literal: Rc::new(lexer::ast::Literal { slice: "10", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "10" }) }),
                                                }),
                                            }),
                                            ops: vec![],
                                        }),
                                        step: Rc::new(ast::Expr {
                                            slice: "2",
                                            term: Rc::new(ast::Term {
                                                slice: "2",
                                                kind: Rc::new(ast::TermKind::Literal {
                                                    literal: Rc::new(lexer::ast::Literal { slice: "2", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "2" }) }),
                                                }),
                                            }),
                                            ops: vec![],
                                        }),
                                    }),
                                }),
                            }),
                        }),
                    ),
                    (
                        Rc::new(lexer::ast::Keyword { slice: "for", kind: lexer::ast::KeywordKind::For }),
                        Rc::new(ast::ForBind {
                            slice: " k <- arr",
                            kind: Rc::new(ast::ForBindKind::Assign {
                                left: Rc::new(ast::Expr {
                                    slice: " k",
                                    term: Rc::new(ast::Term {
                                        slice: " k",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "k" }) }),
                                    }),
                                    ops: vec![],
                                }),
                                for_iter_expr: Rc::new(ast::ForIterExpr {
                                    slice: " arr",
                                    kind: Rc::new(ast::ForIterExprKind::Spread {
                                        expr: Rc::new(ast::Expr {
                                            slice: " arr",
                                            term: Rc::new(ast::Term {
                                                slice: " arr",
                                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "arr" }) }),
                                            }),
                                            ops: vec![],
                                        }),
                                    }),
                                }),
                            }),
                        }),
                    ),
                ],
                stats: Rc::new(ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        Rc::new(ast::Stat {
                            slice: " f()",
                            kind: Rc::new(ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "f" }) }),
                                    }),
                                    ops: vec![Rc::new(ast::Op { slice: "()", kind: Rc::new(ast::OpKind::EvalFn { arg_exprs: vec![] }) })],
                                }),
                            }),
                        }),
                    ],
                    ret: None,
                }),
            }),
        }))),
    );
}

#[test]
fn test_closure_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("|| 123").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "|| 123",
            kind: Rc::new(ast::TermKind::Closure {
                var_decl: Rc::new(ast::VarDecl {
                    slice: "||",
                    kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![] }),
                }),
                expr: Rc::new(ast::Expr {
                    slice: " 123",
                    term: Rc::new(ast::Term {
                        slice: " 123",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "123", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "123" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("|x| x").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "|x| x",
            kind: Rc::new(ast::TermKind::Closure {
                var_decl: Rc::new(ast::VarDecl {
                    slice: "|x|",
                    kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                        Rc::new(ast::VarDecl {
                            slice: "x",
                            kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                mut_attr: None,
                                ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                                ty_expr: None
                            }),
                        }),
                    ]}),
                }),
                expr: Rc::new(ast::Expr {
                    slice: " x",
                    term: Rc::new(ast::Term {
                        slice: " x",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                    }),
                    ops: vec![]
                }),
            }),
        }))),
    );
    assert_eq!(
        parser::term(&context)("|x: int, y: int| { x + y }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "|x: int, y: int| { x + y }",
            kind: Rc::new(ast::TermKind::Closure {
                var_decl: Rc::new(ast::VarDecl {
                    slice: "|x: int, y: int|",
                    kind: Rc::new(ast::VarDeclKind::TupleDecl { var_decls: vec![
                        Rc::new(ast::VarDecl {
                            slice: "x: int",
                            kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                mut_attr: None,
                                ident: Rc::new(lexer::ast::Ident { slice: "x" }),
                                ty_expr: Some(Rc::new(ast::TyExpr {
                                    slice: " int",
                                    ty_term: Rc::new(ast::TyTerm {
                                        slice: " int",
                                        kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                                    }),
                                    ty_ops: vec![],
                                })),
                            }),
                        }),
                        Rc::new(ast::VarDecl {
                            slice: " y: int",
                            kind: Rc::new(ast::VarDeclKind::SingleDecl {
                                mut_attr: None,
                                ident: Rc::new(lexer::ast::Ident { slice: "y" }),
                                ty_expr: Some(Rc::new(ast::TyExpr {
                                    slice: " int",
                                    ty_term: Rc::new(ast::TyTerm {
                                        slice: " int",
                                        kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                                    }),
                                    ty_ops: vec![],
                                })),
                            }),
                        }),
                    ]}),
                }),
                expr: Rc::new(ast::Expr {
                    slice: " { x + y }",
                    term: Rc::new(ast::Term {
                        slice: " { x + y }",
                        kind: Rc::new(ast::TermKind::Block {
                            stats: Rc::new(ast::StatsBlock {
                                slice: " { x + y }",
                                stats: vec![],
                                ret: Some(Rc::new(ast::Expr {
                                    slice: " x + y",
                                    term: Rc::new(ast::Term {
                                        slice: " x",
                                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                                    }),
                                    ops: vec![
                                        Rc::new(ast::Op {
                                            slice: " + y",
                                            kind: Rc::new(ast::OpKind::InfixOp {
                                                op_code: Rc::new(lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus }),
                                                term: Rc::new(ast::Term {
                                                    slice: " y",
                                                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "y" }) }),
                                                }),
                                            }),
                                        }),
                                    ],
                                })),
                            }),
                        }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
}

#[test]
fn test_range_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::iter_expr(&context)("0..10").ok(),
        Some(("", Rc::new(ast::IterExpr {
            slice: "0..10",
            kind: Rc::new(ast::IterExprKind::Range {
                left: Rc::new(ast::Expr {
                    slice: "0",
                    term: Rc::new(ast::Term {
                        slice: "0",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                right: Rc::new(ast::Expr {
                    slice: "10",
                    term: Rc::new(ast::Term {
                        slice: "10",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "10", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "10" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
}

#[test]
fn test_stepped_range_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::iter_expr(&context)("0..10..2").ok(),
        Some(("", Rc::new(ast::IterExpr {
            slice: "0..10..2",
            kind: Rc::new(ast::IterExprKind::SteppedRange {
                left: Rc::new(ast::Expr {
                    slice: "0",
                    term: Rc::new(ast::Term {
                        slice: "0",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                right: Rc::new(ast::Expr {
                    slice: "10",
                    term: Rc::new(ast::Term {
                        slice: "10",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "10", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "10" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                step: Rc::new(ast::Expr {
                    slice: "2",
                    term: Rc::new(ast::Term {
                        slice: "2",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "2", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "2" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
}

#[test]
fn test_spread_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::iter_expr(&context)("...arr").ok(),
        Some(("", Rc::new(ast::IterExpr {
            slice: "...arr",
            kind: Rc::new(ast::IterExprKind::Spread {
                expr: Rc::new(ast::Expr {
                    slice: "arr",
                    term: Rc::new(ast::Term {
                        slice: "arr",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "arr" }) }),
                    }),
                    ops: vec![]
                }),
            }),
        }))),
    );
}

#[test]
fn test_elements_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::iter_expr(&context)("x, y").ok(),
        Some(("", Rc::new(ast::IterExpr {
            slice: "x, y",
            kind: Rc::new(ast::IterExprKind::Elements { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " y",
                    term: Rc::new(ast::Term {
                        slice: " y",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "y" }) }),
                    }),
                    ops: vec![],
                }),
            ]}),
        }))),
    );
    assert_eq!(
        parser::iter_expr(&context)("x, y,").ok(),
        Some(("", Rc::new(ast::IterExpr {
            slice: "x, y,",
            kind: Rc::new(ast::IterExprKind::Elements { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " y",
                    term: Rc::new(ast::Term {
                        slice: " y",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "y" }) }),
                    }),
                    ops: vec![],
                }),
            ]}),
        }))),
    );
}

#[test]
fn test_arg_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::arg_expr(&context)("x").ok(),
        Some(("", Rc::new(ast::ArgExpr {
            slice: "x",
            mut_attr: None,
            expr: Rc::new(ast::Expr {
                slice: "x",
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
                ops: vec![],
            }),
        }))),
    );
    assert_eq!(
        parser::arg_expr(&context)("mut x").ok(),
        Some(("", Rc::new(ast::ArgExpr {
            slice: "mut x",
            mut_attr: Some(Rc::new(ast::MutAttr { slice: "mut", attr: Rc::new(lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut }) })),
            expr: Rc::new(ast::Expr {
                slice: " x",
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "x" }) }),
                }),
                ops: vec![],
            }),
        }))),
    );
}

#[test]
fn test_let_for_bind() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_bind(&context)("let i: int <- arr").ok(),
        Some(("", Rc::new(ast::ForBind {
            slice: "let i: int <- arr",
            kind: Rc::new(ast::ForBindKind::Let {
                let_keyword: Rc::new(lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let }),
                var_decl: Rc::new(ast::VarDecl {
                    slice: " i: int",
                    kind: Rc::new(ast::VarDeclKind::SingleDecl {
                        mut_attr: None,
                        ident: Rc::new(lexer::ast::Ident { slice: "i" }),
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: Rc::new(ast::TyTermKind::EvalTy { ident: Rc::new(lexer::ast::Ident { slice: "int" }) }),
                            }),
                            ty_ops: vec![],
                        })),
                    }),
                }),
                for_iter_expr: Rc::new(ast::ForIterExpr {
                    slice: " arr",
                    kind: Rc::new(ast::ForIterExprKind::Spread {
                        expr: Rc::new(ast::Expr {
                            slice: " arr",
                            term: Rc::new(ast::Term {
                                slice: " arr",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "arr" }) }),
                            }),
                            ops: vec![],
                        }),
                    }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_assign_for_bind() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_bind(&context)("i <- arr").ok(),
        Some(("", Rc::new(ast::ForBind {
            slice: "i <- arr",
            kind: Rc::new(ast::ForBindKind::Assign {
                left: Rc::new(ast::Expr {
                    slice: "i",
                    term: Rc::new(ast::Term {
                        slice: "i",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "i" }) }),
                    }),
                    ops: vec![],
                }),
                for_iter_expr: Rc::new(ast::ForIterExpr {
                    slice: " arr",
                    kind: Rc::new(ast::ForIterExprKind::Spread {
                        expr: Rc::new(ast::Expr {
                            slice: " arr",
                            term: Rc::new(ast::Term {
                                slice: " arr",
                                kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "arr" }) }),
                            }),
                            ops: vec![],
                        }),
                    }),
                }),
            }),
        }))),
    );
}

#[test]
fn test_range_for_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_iter_expr(&context)("0..10").ok(),
        Some(("", Rc::new(ast::ForIterExpr {
            slice: "0..10",
            kind: Rc::new(ast::ForIterExprKind::Range {
                left: Rc::new(ast::Expr {
                    slice: "0",
                    term: Rc::new(ast::Term {
                        slice: "0",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                right: Rc::new(ast::Expr {
                    slice: "10",
                    term: Rc::new(ast::Term {
                        slice: "10",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "10", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "10" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
}

#[test]
fn test_stepped_range_for_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_iter_expr(&context)("0..10..2").ok(),
        Some(("", Rc::new(ast::ForIterExpr {
            slice: "0..10..2",
            kind: Rc::new(ast::ForIterExprKind::SteppedRange {
                left: Rc::new(ast::Expr {
                    slice: "0",
                    term: Rc::new(ast::Term {
                        slice: "0",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "0", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "0" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                right: Rc::new(ast::Expr {
                    slice: "10",
                    term: Rc::new(ast::Term {
                        slice: "10",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "10", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "10" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
                step: Rc::new(ast::Expr {
                    slice: "2",
                    term: Rc::new(ast::Term {
                        slice: "2",
                        kind: Rc::new(ast::TermKind::Literal {
                            literal: Rc::new(lexer::ast::Literal { slice: "2", kind: Rc::new(lexer::ast::LiteralKind::PureInteger { slice: "2" }) }),
                        }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
}

#[test]
fn test_spread_for_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_iter_expr(&context)("arr").ok(),
        Some(("", Rc::new(ast::ForIterExpr {
            slice: "arr",
            kind: Rc::new(ast::ForIterExprKind::Spread {
                expr: Rc::new(ast::Expr {
                    slice: "arr",
                    term: Rc::new(ast::Term {
                        slice: "arr",
                        kind: Rc::new(ast::TermKind::EvalVar { ident: Rc::new(lexer::ast::Ident { slice: "arr" }) }),
                    }),
                    ops: vec![],
                }),
            }),
        }))),
    );
}
