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
        Some(("", ast::Target {
            slice: "pub let x = 123; pub fn f() {};",
            body: Some(ast::Body {
                slice: "pub let x = 123; pub fn f() {};",
                top_stats: vec![
                    ast::TopStat {
                        slice: "pub let x = 123;",
                        kind: ast::TopStatKind::VarBind {
                            access_attr: Some(ast::AccessAttr { slice: "pub", attr: lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub } }),
                            sync_attr: None,
                            var_bind: ast::VarBind {
                                slice: " let x = 123",
                                let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                                var_decl: ast::VarDecl {
                                    slice: " x",
                                    kind: ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: lexer::ast::Ident { slice: "x" },
                                        ty_expr: None,
                                    },
                                },
                                expr: Rc::new(ast::Expr {
                                    slice: " 123",
                                    term: Rc::new(ast::Term {
                                        slice: " 123",
                                        kind: ast::TermKind::Literal {
                                            literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                                        },
                                    }),
                                    ops: vec![]
                                }),
                            },
                        },
                    },
                    ast::TopStat {
                        slice: " pub fn f() {};",
                        kind: ast::TopStatKind::FnBind {
                            access_attr: Some(ast::AccessAttr { slice: " pub", attr: lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub } }),
                            fn_bind: ast::FnBind {
                                slice: " fn f() {}",
                                fn_keyword: lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn },
                                fn_decl: ast::FnDecl {
                                    slice: " f()",
                                    ident: lexer::ast::Ident { slice: "f" },
                                    var_decl: ast::VarDecl {
                                        slice: "()",
                                        kind: ast::VarDeclKind::TupleDecl { var_decls: vec![] },
                                    },
                                    ty_expr: None,
                                },
                                stats_block: ast::StatsBlock {
                                    slice: " {}",
                                    stats: vec![],
                                    ret: None
                                },
                            },
                        },
                    },
                ],
            }),
        })),
    );
}

#[test]
fn test_body() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::body(&context)("pub let x = 123; pub fn f() {};").ok(),
        Some(("", ast::Body {
            slice: "pub let x = 123; pub fn f() {};",
            top_stats: vec![
                ast::TopStat {
                    slice: "pub let x = 123;",
                    kind: ast::TopStatKind::VarBind {
                        access_attr: Some(ast::AccessAttr { slice: "pub", attr: lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub } }),
                        sync_attr: None,
                        var_bind: ast::VarBind {
                            slice: " let x = 123",
                            let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                            var_decl: ast::VarDecl {
                                slice: " x",
                                kind: ast::VarDeclKind::SingleDecl {
                                    mut_attr: None,
                                    ident: lexer::ast::Ident { slice: "x" },
                                    ty_expr: None,
                                },
                            },
                            expr: Rc::new(ast::Expr {
                                slice: " 123",
                                term: Rc::new(ast::Term {
                                    slice: " 123",
                                    kind: ast::TermKind::Literal {
                                        literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                                    },
                                }),
                                ops: vec![],
                            }),
                        },
                    },
                },
                ast::TopStat {
                    slice: " pub fn f() {};",
                    kind: ast::TopStatKind::FnBind {
                        access_attr: Some(ast::AccessAttr { slice: " pub", attr: lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub } }),
                        fn_bind: ast::FnBind {
                            slice: " fn f() {}",
                            fn_keyword: lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn },
                            fn_decl: ast::FnDecl {
                                slice: " f()",
                                ident: lexer::ast::Ident { slice: "f" },
                                var_decl: ast::VarDecl {
                                    slice: "()",
                                    kind: ast::VarDeclKind::TupleDecl { var_decls: vec![] },
                                },
                                ty_expr: None,
                            },
                            stats_block: ast::StatsBlock {
                                slice: " {}",
                                stats: vec![],
                                ret: None
                            },
                        },
                    },
                },
            ],
        })),
    );
}

#[test]
fn test_var_bind_top_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::top_stat(&context)("pub sync let mut x = 123;").ok(),
        Some(("", ast::TopStat {
            slice: "pub sync let mut x = 123;",
            kind: ast::TopStatKind::VarBind {
                access_attr: Some(ast::AccessAttr { slice: "pub", attr: lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub } }),
                sync_attr: Some(ast::SyncAttr { slice: " sync", attr: lexer::ast::Keyword { slice: "sync", kind: lexer::ast::KeywordKind::Sync } }),
                var_bind: ast::VarBind {
                    slice: " let mut x = 123",
                    let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                    var_decl: ast::VarDecl {
                        slice: " mut x",
                        kind: ast::VarDeclKind::SingleDecl {
                            mut_attr: Some(ast::MutAttr { slice: " mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                            ident: lexer::ast::Ident { slice: "x" },
                            ty_expr: None,
                        },
                    },
                    expr: Rc::new(ast::Expr {
                        slice: " 123",
                        term: Rc::new(ast::Term {
                            slice: " 123",
                            kind: ast::TermKind::Literal {
                                literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                            },
                        }),
                        ops: vec![],
                    }),
                },
            },
        })),
    );
}

#[test]
fn test_fn_bind_top_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::top_stat(&context)("pub fn f(x: int) -> int { x };").ok(),
        Some(("", ast::TopStat {
            slice: "pub fn f(x: int) -> int { x };",
            kind: ast::TopStatKind::FnBind {
                access_attr: Some(ast::AccessAttr { slice: "pub", attr: lexer::ast::Keyword { slice: "pub", kind: lexer::ast::KeywordKind::Pub }}),
                fn_bind: ast::FnBind {
                    slice: " fn f(x: int) -> int { x }",
                    fn_keyword: lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn },
                    fn_decl: ast::FnDecl {
                        slice: " f(x: int) -> int",
                        ident: lexer::ast::Ident { slice: "f" },
                        var_decl: ast::VarDecl {
                            slice: "(x: int)",
                            kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                                ast::VarDecl {
                                    slice: "x: int",
                                    kind: ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: lexer::ast::Ident { slice: "x" },
                                        ty_expr: Some(Rc::new(ast::TyExpr {
                                            slice: " int",
                                            ty_term: Rc::new(ast::TyTerm {
                                                slice: " int",
                                                kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                                            }),
                                            ty_ops: vec![]
                                        })),
                                    },
                                },
                            ]},
                        },
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                            }),
                            ty_ops: vec![],
                        })),
                    },
                    stats_block: ast::StatsBlock {
                        slice: " { x }",
                        stats: vec![],
                        ret: Some(Rc::new(ast::Expr {
                            slice: " x",
                            term: Rc::new(ast::Term {
                                slice: " x", kind:
                                ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                            }),
                            ops: vec![],
                        })),
                    },
                },
            },
        })),
    );
}

#[test]
fn test_stat_top_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::top_stat(&context)("f();").ok(),
        Some(("", ast::TopStat {
            slice: "f();",
            kind: ast::TopStatKind::Stat {
                stat: ast::Stat {
                    slice: "f()",
                    kind: ast::StatKind::Expr {
                        expr: Rc::new(ast::Expr {
                            slice: "f()",
                            term: Rc::new(ast::Term {
                                slice: "f",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                            }),
                            ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                        }),
                    },
                }
            },
        })),
    );
}

#[test]
fn test_var_bind() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::var_bind(&context)("let mut x: int = 123").ok(),
        Some(("", ast::VarBind {
            slice: "let mut x: int = 123",
            let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
            var_decl: ast::VarDecl {
                slice: " mut x: int",
                kind: ast::VarDeclKind::SingleDecl {
                    mut_attr: Some(ast::MutAttr { slice: " mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                    ident: lexer::ast::Ident { slice: "x" },
                    ty_expr: Some(Rc::new(ast::TyExpr {
                        slice: " int",
                        ty_term: Rc::new(ast::TyTerm {
                            slice: " int",
                            kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                        }),
                        ty_ops: vec![],
                    })),
                },
            },
            expr: Rc::new(ast::Expr {
                slice: " 123",
                term: Rc::new(ast::Term {
                    slice: " 123",
                    kind: ast::TermKind::Literal {
                        literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                    },
                }),
                ops: vec![],
            }),
        })),
    );
}

#[test]
fn test_single_var_decl() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::var_decl(&context)("x").ok(),
        Some(("", ast::VarDecl {
            slice: "x",
            kind: ast::VarDeclKind::SingleDecl {
                mut_attr: None,
                ident: lexer::ast::Ident { slice: "x" },
                ty_expr: None,
            },
        })),
    );
    assert_eq!(
        parser::var_decl(&context)("mut x").ok(),
        Some(("", ast::VarDecl {
            slice: "mut x",
            kind: ast::VarDeclKind::SingleDecl {
                mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                ident: lexer::ast::Ident { slice: "x" },
                ty_expr: None,
            },
        })),
    );
    assert_eq!(
        parser::var_decl(&context)("mut x: int").ok(),
        Some(("", ast::VarDecl {
            slice: "mut x: int",
            kind: ast::VarDeclKind::SingleDecl {
                mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                ident: lexer::ast::Ident { slice: "x" },
                ty_expr: Some(Rc::new(ast::TyExpr {
                    slice: " int",
                    ty_term: Rc::new(ast::TyTerm {
                        slice: " int",
                        kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                    }),
                    ty_ops: vec![],
                })),
            }
        })),
    );
}

#[test]
fn test_tuple_var_decl() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::var_decl(&context)("()").ok(),
        Some(("", ast::VarDecl {
            slice: "()",
            kind: ast::VarDeclKind::TupleDecl { var_decls: vec![] }
        })),
    );
    assert_eq!(
        parser::var_decl(&context)("(x)").ok(),
        Some(("", ast::VarDecl {
            slice: "(x)",
            kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                ast::VarDecl {
                    slice: "x",
                    kind: ast::VarDeclKind::SingleDecl {
                        mut_attr: None,
                        ident: lexer::ast::Ident { slice: "x" },
                        ty_expr: None
                    },
                },
            ]},
        })),
    );
    assert_eq!(
        parser::var_decl(&context)("(mut x: int, y)").ok(),
        Some(("", ast::VarDecl {
            slice: "(mut x: int, y)",
            kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                ast::VarDecl {
                    slice: "mut x: int",
                    kind: ast::VarDeclKind::SingleDecl {
                        mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                        ident: lexer::ast::Ident { slice: "x" },
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                            }),
                            ty_ops: vec![],
                        })),
                    },
                },
                ast::VarDecl {
                    slice: " y",
                    kind: ast::VarDeclKind::SingleDecl {
                        mut_attr: None,
                        ident: lexer::ast::Ident { slice: "y" },
                        ty_expr: None,
                    },
                },
            ]},
        })),
    );
    assert_eq!(
        parser::var_decl(&context)("(mut x: int, y,)").ok(),
        Some(("", ast::VarDecl {
            slice: "(mut x: int, y,)",
            kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                ast::VarDecl {
                    slice: "mut x: int",
                    kind: ast::VarDeclKind::SingleDecl {
                        mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                        ident: lexer::ast::Ident { slice: "x" },
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                            }),
                            ty_ops: vec![],
                        })),
                    },
                },
                ast::VarDecl {
                    slice: " y",
                    kind: ast::VarDeclKind::SingleDecl {
                        mut_attr: None,
                        ident: lexer::ast::Ident { slice: "y" },
                        ty_expr: None,
                    },
                },
            ]},
        })),
    );
}

#[test]
fn test_fn_bind() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::fn_bind(&context)("fn f(mut x: int, y) -> int { g(); x }").ok(),
        Some(("", ast::FnBind {
            slice: "fn f(mut x: int, y) -> int { g(); x }",
            fn_keyword: lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn },
            fn_decl: ast::FnDecl {
                slice: " f(mut x: int, y) -> int",
                ident: lexer::ast::Ident { slice: "f" },
                var_decl: ast::VarDecl {
                    slice: "(mut x: int, y)",
                    kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                        ast::VarDecl {
                            slice: "mut x: int",
                            kind: ast::VarDeclKind::SingleDecl {
                                mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                                ident: lexer::ast::Ident { slice: "x" },
                                ty_expr: Some(Rc::new(ast::TyExpr {
                                    slice: " int",
                                    ty_term: Rc::new(ast::TyTerm {
                                        slice: " int",
                                        kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                                    }),
                                    ty_ops: vec![],
                                })),
                            },
                        },
                        ast::VarDecl {
                            slice: " y",
                            kind: ast::VarDeclKind::SingleDecl {
                                mut_attr: None,
                                ident: lexer::ast::Ident { slice: "y" },
                                ty_expr: None
                            },
                        },
                    ]},
                },
                ty_expr: Some(Rc::new(ast::TyExpr {
                    slice: " int",
                    ty_term: Rc::new(ast::TyTerm {
                        slice: " int",
                        kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                    }),
                    ty_ops: vec![],
                })),
            },
            stats_block: ast::StatsBlock {
                slice: " { g(); x }",
                stats: vec![
                    ast::Stat {
                        slice: " g()",
                        kind: ast::StatKind::Expr {
                            expr: Rc::new(ast::Expr {
                                slice: " g()",
                                term: Rc::new(ast::Term {
                                    slice: " g",
                                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "g" } },
                                }),
                                ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                            }),
                        },
                    },
                ],
                ret: Some(Rc::new(ast::Expr {
                    slice: " x",
                    term: Rc::new(ast::Term {
                        slice: " x",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                    }),
                    ops: vec![],
                })),
            },
        })),
    );
}

#[test]
fn test_fn_decl() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::fn_decl(&context)("f()").ok(),
        Some(("", ast::FnDecl {
            slice: "f()",
            ident: lexer::ast::Ident { slice: "f" },
            var_decl: ast::VarDecl {
                slice: "()",
                kind: ast::VarDeclKind::TupleDecl { var_decls: vec![] },
            },
            ty_expr: None,
        })),
    );
    assert_eq!(
        parser::fn_decl(&context)("f(mut x: int, y) -> int").ok(),
        Some(("", ast::FnDecl {
            slice: "f(mut x: int, y) -> int",
            ident: lexer::ast::Ident { slice: "f" },
            var_decl: ast::VarDecl {
                slice: "(mut x: int, y)",
                kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                    ast::VarDecl {
                        slice: "mut x: int",
                        kind: ast::VarDeclKind::SingleDecl {
                            mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                            ident: lexer::ast::Ident { slice: "x" },
                            ty_expr: Some(Rc::new(ast::TyExpr {
                                slice: " int",
                                ty_term: Rc::new(ast::TyTerm {
                                    slice: " int",
                                    kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                                }),
                                ty_ops: vec![],
                            })),
                        },
                    },
                    ast::VarDecl {
                        slice: " y",
                        kind: ast::VarDeclKind::SingleDecl {
                            mut_attr: None,
                            ident: lexer::ast::Ident { slice: "y" },
                            ty_expr: None
                        },
                    },
                ]},
            },
            ty_expr: Some(Rc::new(ast::TyExpr {
                slice: " int",
                ty_term: Rc::new(ast::TyTerm {
                    slice: " int",
                    kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                }),
                ty_ops: vec![],
            })),
        })),
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
                kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "T" } },
            }),
            ty_ops: vec![
                ast::TyOp {
                    slice: "::U",
                    kind: ast::TyOpKind::Access {
                        op_code: lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon },
                        ty_term: Rc::new(ast::TyTerm {
                            slice: "U",
                            kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "U" } },
                        }),
                    },
                },
                ast::TyOp {
                    slice: "::V",
                    kind: ast::TyOpKind::Access {
                        op_code: lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon },
                        ty_term: Rc::new(ast::TyTerm {
                            slice: "V",
                            kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "V" } },
                        }),
                    },
                },
            ],
        }))),
    );
}

#[test]
fn test_ty_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::ty_op(&context)("::T").ok(),
        Some(("", ast::TyOp {
            slice: "::T",
            kind: ast::TyOpKind::Access {
                op_code: lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon },
                ty_term: Rc::new(ast::TyTerm {
                    slice: "T",
                    kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "T" } },
                }),
            },
        })),
    );
}

#[test]
fn test_ty_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::ty_term(&context)("string").ok(),
        Some(("", Rc::new(ast::TyTerm {
            slice: "string",
            kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "string" } },
        }))),
    );
}

#[test]
fn test_stats_block() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stats_block(&context)("{}").ok(),
        Some(("", ast::StatsBlock {
            slice: "{}",
            stats: vec![],
            ret: None
        })),
    );
    assert_eq!(
        parser::stats_block(&context)("{ f(); x }").ok(),
        Some(("", ast::StatsBlock {
            slice: "{ f(); x }",
            stats: vec![
                ast::Stat {
                    slice: " f()",
                    kind: ast::StatKind::Expr {
                        expr: Rc::new(ast::Expr {
                            slice: " f()",
                            term: Rc::new(ast::Term {
                                slice: " f",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                            }),
                            ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                        }),
                    },
                },
            ],
            ret: Some(Rc::new(ast::Expr {
                slice: " x",
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
                ops: vec![],
            })),
        })),
    );
    assert_eq!(
        parser::stats_block(&context)("{ f(); x; }").ok(),
        Some(("", ast::StatsBlock {
            slice: "{ f(); x; }",
            stats: vec![
                ast::Stat {
                    slice: " f()",
                    kind: ast::StatKind::Expr {
                        expr: Rc::new(ast::Expr {
                            slice: " f()",
                            term: Rc::new(ast::Term {
                                slice: " f",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                            }),
                            ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                        }),
                    },
                },
                ast::Stat {
                    slice: " x",
                    kind: ast::StatKind::Expr {
                        expr: Rc::new(ast::Expr {
                            slice: " x",
                            term: Rc::new(ast::Term {
                                slice: " x",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                            }),
                            ops: vec![],
                        }),
                    },
                },
            ],
            ret: None,
        })),
    );
}

#[test]
fn test_return_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("return;").ok(),
        Some(("", ast::Stat {
            slice: "return",
            kind: ast::StatKind::Return {
                return_keyword: lexer::ast::Keyword { slice: "return", kind: lexer::ast::KeywordKind::Return },
                expr: None
            }
        })),
    );
    assert_eq!(
        parser::stat(&context)("return x;").ok(),
        Some(("", ast::Stat {
            slice: "return x",
            kind: ast::StatKind::Return {
                return_keyword: lexer::ast::Keyword { slice: "return", kind: lexer::ast::KeywordKind::Return },
                expr: Some(Rc::new(ast::Expr {
                    slice: " x",
                    term: Rc::new(ast::Term {
                        slice: " x",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                    }),
                    ops: vec![],
                })),
            },
        })),
    );
}

#[test]
fn test_continue_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("continue;").ok(),
        Some(("", ast::Stat {
            slice: "continue",
            kind: ast::StatKind::Continue {
                continue_keyword: lexer::ast::Keyword { slice: "continue", kind: lexer::ast::KeywordKind::Continue },
            },
        })),
    );
}

#[test]
fn test_break_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("break;").ok(),
        Some(("", ast::Stat {
            slice: "break",
            kind: ast::StatKind::Break {
                break_keyword: lexer::ast::Keyword { slice: "break", kind: lexer::ast::KeywordKind::Break },
            },
        })),
    );
}

#[test]
fn test_var_bind_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("let x = 123;").ok(),
        Some(("", ast::Stat {
            slice: "let x = 123",
            kind: ast::StatKind::VarBind {
                var_bind: ast::VarBind {
                    slice: "let x = 123",
                    let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                    var_decl: ast::VarDecl {
                        slice: " x",
                        kind: ast::VarDeclKind::SingleDecl {
                            mut_attr: None,
                            ident: lexer::ast::Ident { slice: "x" },
                            ty_expr: None,
                        },
                    },
                    expr: Rc::new(ast::Expr {
                        slice: " 123",
                        term: Rc::new(ast::Term {
                            slice: " 123",
                            kind: ast::TermKind::Literal {
                                literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                            },
                        }),
                        ops: vec![]
                    }),
                },
            },
        })),
    );
    assert_eq!(
        parser::stat(&context)("let mut x: int = 123;").ok(),
        Some(("", ast::Stat {
            slice: "let mut x: int = 123",
            kind: ast::StatKind::VarBind {
                var_bind: ast::VarBind {
                    slice: "let mut x: int = 123",
                    let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                    var_decl: ast::VarDecl {
                        slice: " mut x: int",
                        kind: ast::VarDeclKind::SingleDecl {
                            mut_attr: Some(ast::MutAttr { slice: " mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                            ident: lexer::ast::Ident { slice: "x" },
                            ty_expr: Some(Rc::new(ast::TyExpr {
                                slice: " int",
                                ty_term: Rc::new(ast::TyTerm {
                                    slice: " int",
                                    kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                                }),
                                ty_ops: vec![],
                            })),
                        },
                    },
                    expr: Rc::new(ast::Expr {
                        slice: " 123",
                        term: Rc::new(ast::Term {
                            slice: " 123",
                            kind: ast::TermKind::Literal {
                                literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                            },
                        }),
                        ops: vec![],
                    }),
                },
            },
        })),
    );
    assert_eq!(
        parser::stat(&context)("let (mut x: int, y) = (123, 456);").ok(),
        Some(("", ast::Stat {
            slice: "let (mut x: int, y) = (123, 456)",
            kind: ast::StatKind::VarBind {
                var_bind: ast::VarBind {
                    slice: "let (mut x: int, y) = (123, 456)",
                    let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                    var_decl: ast::VarDecl {
                        slice: " (mut x: int, y)",
                        kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                            ast::VarDecl {
                                slice: "mut x: int",
                                kind: ast::VarDeclKind::SingleDecl {
                                    mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                                    ident: lexer::ast::Ident { slice: "x" },
                                    ty_expr: Some(Rc::new(ast::TyExpr {
                                        slice: " int",
                                        ty_term: Rc::new(ast::TyTerm {
                                            slice: " int",
                                            kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                                        }),
                                        ty_ops: vec![],
                                    })),
                                },
                            },
                            ast::VarDecl {
                                slice: " y",
                                kind: ast::VarDeclKind::SingleDecl {
                                    mut_attr: None,
                                    ident: lexer::ast::Ident { slice: "y" },
                                    ty_expr: None
                                },
                            },
                        ]},
                    },
                    expr: Rc::new(ast::Expr {
                        slice: " (123, 456)",
                        term: Rc::new(ast::Term {
                            slice: " (123, 456)",
                            kind: ast::TermKind::Tuple { exprs: vec![
                                Rc::new(ast::Expr {
                                    slice: "123",
                                    term: Rc::new(ast::Term {
                                        slice: "123",
                                        kind: ast::TermKind::Literal {
                                            literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                                        },
                                    }),
                                    ops: vec![],
                                }),
                                Rc::new(ast::Expr {
                                    slice: " 456",
                                    term: Rc::new(ast::Term {
                                        slice: " 456",
                                        kind: ast::TermKind::Literal {
                                            literal: lexer::ast::Literal { slice: "456", kind: lexer::ast::LiteralKind::PureInteger { slice: "456" } },
                                        },
                                    }),
                                    ops: vec![],
                                }),
                            ]},
                        }),
                        ops: vec![]
                    }),
                },
            },
        })),
    );
}

#[test]
fn test_fn_bind_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("fn f(x: int) -> int { x };").ok(),
        Some(("", ast::Stat {
            slice: "fn f(x: int) -> int { x }",
            kind: ast::StatKind::FnBind {
                fn_bind: ast::FnBind {
                    slice: "fn f(x: int) -> int { x }",
                    fn_keyword: lexer::ast::Keyword { slice: "fn", kind: lexer::ast::KeywordKind::Fn },
                    fn_decl: ast::FnDecl {
                        slice: " f(x: int) -> int",
                        ident: lexer::ast::Ident { slice: "f" },
                        var_decl: ast::VarDecl {
                            slice: "(x: int)",
                            kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                                ast::VarDecl {
                                    slice: "x: int",
                                    kind: ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: lexer::ast::Ident { slice: "x" },
                                        ty_expr: Some(Rc::new(ast::TyExpr {
                                            slice: " int",
                                            ty_term: Rc::new(ast::TyTerm {
                                                slice: " int",
                                                kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                                            }),
                                            ty_ops: vec![],
                                        })),
                                    },
                                },
                            ]},
                        },
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                            }),
                            ty_ops: vec![],
                        })),
                    },
                    stats_block: ast::StatsBlock {
                        slice: " { x }",
                        stats: vec![],
                        ret: Some(Rc::new(ast::Expr {
                            slice: " x",
                            term: Rc::new(ast::Term {
                                slice: " x",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                            }),
                            ops: vec![]
                        })),
                    },
                },
            },
        })),
    );
}

#[test]
fn test_expr_stat() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::stat(&context)("x = 123;").ok(),
        Some(("", ast::Stat {
            slice: "x = 123",
            kind: ast::StatKind::Expr {
                expr: Rc::new(ast::Expr {
                    slice: "x = 123",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                    }),
                    ops: vec![
                        ast::Op {
                            slice: " = 123",
                            kind: ast::OpKind::Assign {
                                term: Rc::new(ast::Term {
                                    slice: " 123",
                                    kind: ast::TermKind::Literal {
                                        literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                                    },
                                }),
                            },
                        },
                    ],
                }),
            },
        })),
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
                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
            }),
            ops: vec![
                ast::Op {
                    slice: " = T",
                    kind: ast::OpKind::Assign {
                        term: Rc::new(ast::Term {
                            slice: " T",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "T" } },
                        }),
                    },
                },
                ast::Op {
                    slice: "::f",
                    kind: ast::OpKind::TyAccess {
                        op_code: lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon },
                        term: Rc::new(ast::Term {
                            slice: "f",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                        }),
                    },
                },
                ast::Op {
                    slice: "(1, 2)",
                    kind: ast::OpKind::EvalFn { arg_exprs: vec![
                        ast::ArgExpr {
                            slice: "1",
                            mut_attr: None,
                            expr: Rc::new(ast::Expr {
                                slice: "1",
                                term: Rc::new(ast::Term {
                                    slice: "1",
                                    kind: ast::TermKind::Literal {
                                        literal: lexer::ast::Literal { slice: "1", kind: lexer::ast::LiteralKind::PureInteger { slice: "1" } },
                                    },
                                }),
                                ops: vec![],
                            }),
                        },
                        ast::ArgExpr {
                            slice: " 2",
                            mut_attr: None,
                            expr: Rc::new(ast::Expr {
                                slice: " 2",
                                term: Rc::new(ast::Term {
                                    slice: " 2",
                                    kind: ast::TermKind::Literal {
                                        literal: lexer::ast::Literal { slice: "2", kind: lexer::ast::LiteralKind::PureInteger { slice: "2" } },
                                    },
                                }),
                                ops: vec![],
                            }),
                        },
                    ]},
                },
                ast::Op {
                    slice: ".t",
                    kind: ast::OpKind::Access {
                        op_code: lexer::ast::OpCode { slice: ".", kind: lexer::ast::OpCodeKind::Dot },
                        term: Rc::new(ast::Term {
                            slice: "t",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "t" } },
                        }),
                    },
                },
                ast::Op {
                    slice: " + a",
                    kind: ast::OpKind::InfixOp {
                        op_code: lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus },
                        term: Rc::new(ast::Term {
                            slice: " a",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "a" } },
                        }),
                    },
                },
                ast::Op {
                    slice: ".g",
                    kind: ast::OpKind::Access {
                        op_code: lexer::ast::OpCode { slice: ".", kind: lexer::ast::OpCodeKind::Dot },
                        term: Rc::new(ast::Term {
                            slice: "g",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "g" } },
                        }),
                    },
                },
                ast::Op {
                    slice: "(...b)",
                    kind: ast::OpKind::EvalSpreadFn {
                        expr: Rc::new(ast::Expr {
                            slice: "b",
                            term: Rc::new(ast::Term {
                                slice: "b",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "b" } },
                            }),
                            ops: vec![],
                        }),
                    },
                },
                ast::Op {
                    slice: "[y]",
                    kind: ast::OpKind::EvalKey {
                        expr: Rc::new(ast::Expr {
                            slice: "y",
                            term: Rc::new(ast::Term {
                                slice: "y",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "y" } },
                            }),
                            ops: vec![]
                        }),
                    },
                },
            ]
        }))),
    );
}

#[test]
fn test_ty_access_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("::x").ok(),
        Some(("", ast::Op {
            slice: "::x",
            kind: ast::OpKind::TyAccess {
                op_code: lexer::ast::OpCode { slice: "::", kind: lexer::ast::OpCodeKind::DoubleColon },
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
}

#[test]
fn test_access_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)(".x").ok(),
        Some(("", ast::Op {
            slice: ".x",
            kind: ast::OpKind::Access {
                op_code: lexer::ast::OpCode { slice: ".", kind: lexer::ast::OpCodeKind::Dot },
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("?.x").ok(),
        Some(("", ast::Op {
            slice: "?.x",
            kind: ast::OpKind::Access {
                op_code: lexer::ast::OpCode { slice: "?.", kind: lexer::ast::OpCodeKind::CoalescingAccess },
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
}

#[test]
fn test_eval_fn_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("()").ok(),
        Some(("", ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } })),
    );
    assert_eq!(
        parser::op(&context)("(mut x, y, z)").ok(),
        Some(("", ast::Op {
            slice: "(mut x, y, z)",
            kind: ast::OpKind::EvalFn { arg_exprs: vec![
                ast::ArgExpr {
                    slice: "mut x",
                    mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                    expr: Rc::new(ast::Expr {
                        slice: " x",
                        term: Rc::new(ast::Term {
                            slice: " x",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                        }),
                        ops: vec![],
                    }),
                },
                ast::ArgExpr {
                    slice: " y",
                    mut_attr: None,
                    expr: Rc::new(ast::Expr {
                        slice: " y",
                        term: Rc::new(ast::Term {
                            slice: " y",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "y" } },
                        }),
                        ops: vec![],
                    }),
                },
                ast::ArgExpr {
                    slice: " z",
                    mut_attr: None,
                    expr: Rc::new(ast::Expr {
                        slice: " z",
                        term: Rc::new(ast::Term {
                            slice: " z",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "z" } },
                        }),
                        ops: vec![],
                    }),
                },
            ]},
        })),
    );
    assert_eq!(
        parser::op(&context)("(mut x, y, z,)").ok(),
        Some(("", ast::Op {
            slice: "(mut x, y, z,)",
            kind: ast::OpKind::EvalFn { arg_exprs: vec![
                ast::ArgExpr {
                    slice: "mut x",
                    mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                    expr: Rc::new(ast::Expr {
                        slice: " x",
                        term: Rc::new(ast::Term {
                            slice: " x",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                        }),
                        ops: vec![],
                    }),
                },
                ast::ArgExpr {
                    slice: " y",
                    mut_attr: None,
                    expr: Rc::new(ast::Expr {
                        slice: " y",
                        term: Rc::new(ast::Term {
                            slice: " y",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "y" } },
                        }),
                        ops: vec![],
                    }),
                },
                ast::ArgExpr {
                    slice: " z",
                    mut_attr: None,
                    expr: Rc::new(ast::Expr {
                        slice: " z",
                        term: Rc::new(ast::Term {
                            slice: " z",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "z" } },
                        }),
                        ops: vec![],
                    }),
                },
            ]},
        })),
    );
}

#[test]
fn test_eval_spread_fn_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("(...x)").ok(),
        Some(("", ast::Op {
            slice: "(...x)",
            kind: ast::OpKind::EvalSpreadFn {
                expr: Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                    }),
                    ops: vec![],
                }),
            },
        })),
    );
}

#[test]
fn test_eval_key_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("[x]").ok(),
        Some(("", ast::Op {
            slice: "[x]",
            kind: ast::OpKind::EvalKey {
                expr: Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                    }),
                    ops: vec![],
                }),
            },
        })),
    );
}

#[test]
fn test_cast_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("as T").ok(),
        Some(("", ast::Op {
            slice: "as T",
            kind: ast::OpKind::CastOp {
                as_keyword: lexer::ast::Keyword { slice: "as", kind: lexer::ast::KeywordKind::As },
                ty_expr: Rc::new(ast::TyExpr {
                    slice: " T",
                    ty_term: Rc::new(ast::TyTerm {
                        slice: " T",
                        kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "T" } },
                    }),
                    ty_ops: vec![],
                }),
            },
        })),
    );
}

#[test]
fn test_infix_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("* x").ok(),
        Some(("", ast::Op {
            slice: "* x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "*", kind: lexer::ast::OpCodeKind::Star },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("/ x").ok(),
        Some(("", ast::Op {
            slice: "/ x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "/", kind: lexer::ast::OpCodeKind::Div },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("% x").ok(),
        Some(("", ast::Op {
            slice: "% x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "%", kind: lexer::ast::OpCodeKind::Percent },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("+ x").ok(),
        Some(("", ast::Op {
            slice: "+ x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("- x").ok(),
        Some(("", ast::Op {
            slice: "- x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "-", kind: lexer::ast::OpCodeKind::Minus },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("<< x").ok(),
        Some(("", ast::Op {
            slice: "<< x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "<<", kind: lexer::ast::OpCodeKind::LeftShift },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)(">> x").ok(),
        Some(("", ast::Op {
            slice: ">> x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: ">>", kind: lexer::ast::OpCodeKind::RightShift },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("< x").ok(),
        Some(("", ast::Op {
            slice: "< x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "<", kind: lexer::ast::OpCodeKind::Lt },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("> x").ok(),
        Some(("", ast::Op {
            slice: "> x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: ">", kind: lexer::ast::OpCodeKind::Gt },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("<= x").ok(),
        Some(("", ast::Op {
            slice: "<= x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "<=", kind: lexer::ast::OpCodeKind::Le },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)(">= x").ok(),
        Some(("", ast::Op {
            slice: ">= x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: ">=", kind: lexer::ast::OpCodeKind::Ge },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("== x").ok(),
        Some(("", ast::Op {
            slice: "== x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("!= x").ok(),
        Some(("", ast::Op {
            slice: "!= x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "!=", kind: lexer::ast::OpCodeKind::Ne },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("& x").ok(),
        Some(("", ast::Op {
            slice: "& x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "&", kind: lexer::ast::OpCodeKind::Amp },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("^ x").ok(),
        Some(("", ast::Op {
            slice: "^ x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "^", kind: lexer::ast::OpCodeKind::Caret },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("| x").ok(),
        Some(("", ast::Op {
            slice: "| x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "|", kind: lexer::ast::OpCodeKind::Pipe },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("&& x").ok(),
        Some(("", ast::Op {
            slice: "&& x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "&&", kind: lexer::ast::OpCodeKind::And },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("|| x").ok(),
        Some(("", ast::Op {
            slice: "|| x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "||", kind: lexer::ast::OpCodeKind::Or },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("?? x").ok(),
        Some(("", ast::Op {
            slice: "?? x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "??", kind: lexer::ast::OpCodeKind::Coalescing },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("|> x").ok(),
        Some(("", ast::Op {
            slice: "|> x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "|>", kind: lexer::ast::OpCodeKind::RightPipeline },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
    assert_eq!(
        parser::op(&context)("<| x").ok(),
        Some(("", ast::Op {
            slice: "<| x",
            kind: ast::OpKind::InfixOp {
                op_code: lexer::ast::OpCode { slice: "<|", kind: lexer::ast::OpCodeKind::LeftPipeline },
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
}

#[test]
fn test_assign_op() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::op(&context)("= x").ok(),
        Some(("", ast::Op {
            slice: "= x",
            kind: ast::OpKind::Assign {
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        })),
    );
}

#[test]
fn test_prefix_op_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("+x").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "+x",
            kind: ast::TermKind::PrefixOp {
                op_code: lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus },
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("-123").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "-123",
            kind: ast::TermKind::PrefixOp {
                op_code: lexer::ast::OpCode { slice: "-", kind: lexer::ast::OpCodeKind::Minus },
                term: Rc::new(ast::Term {
                    slice: "123",
                    kind: ast::TermKind::Literal {
                        literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                    },
                }),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("!false").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "!false",
            kind: ast::TermKind::PrefixOp {
                op_code: lexer::ast::OpCode { slice: "!", kind: lexer::ast::OpCodeKind::Bang },
                term: Rc::new(ast::Term {
                    slice: "false",
                    kind: ast::TermKind::Literal {
                        literal: lexer::ast::Literal {
                            slice: "false",
                            kind: lexer::ast::LiteralKind::Bool { keyword: lexer::ast::Keyword { slice: "false", kind: lexer::ast::KeywordKind::False } },
                        },
                    },
                }),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("~0xFFFF").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "~0xFFFF",
            kind: ast::TermKind::PrefixOp {
                op_code: lexer::ast::OpCode { slice: "~", kind: lexer::ast::OpCodeKind::Tilde },
                term: Rc::new(ast::Term {
                    slice: "0xFFFF",
                    kind: ast::TermKind::Literal {
                        literal: lexer::ast::Literal { slice: "0xFFFF", kind: lexer::ast::LiteralKind::HexInteger { slice: "0xFFFF" } }
                    }
                }),
            },
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
            kind: ast::TermKind::Block {
                stats: ast::StatsBlock {
                    slice: "{ f(); g(); x }",
                    stats: vec![
                        ast::Stat {
                            slice: " f()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                        ast::Stat {
                            slice: " g()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " g()",
                                    term: Rc::new(ast::Term {
                                        slice: " g",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "g" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                    ],
                    ret: Some(Rc::new(ast::Expr {
                        slice: " x",
                        term: Rc::new(ast::Term {
                            slice: " x",
                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                        }),
                        ops: vec![],
                    })),
                },
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("{ f(); g(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "{ f(); g(); }",
            kind: ast::TermKind::Block {
                stats: ast::StatsBlock {
                    slice: "{ f(); g(); }",
                    stats: vec![
                        ast::Stat {
                            slice: " f()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                        ast::Stat {
                            slice: " g()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " g()",
                                    term: Rc::new(ast::Term {
                                        slice: " g",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "g" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                    ],
                    ret: None,
                },
            },
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
            kind: ast::TermKind::Paren {
                expr: Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                    }),
                    ops: vec![],
                }),
            },
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
            kind: ast::TermKind::Tuple { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "1",
                    term: Rc::new(ast::Term {
                        slice: "1",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "1", kind: lexer::ast::LiteralKind::PureInteger { slice: "1" } },
                        },
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " 2",
                    term: Rc::new(ast::Term {
                        slice: " 2",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "2", kind: lexer::ast::LiteralKind::PureInteger { slice: "2" } },
                        },
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " 3",
                    term: Rc::new(ast::Term {
                        slice: " 3",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "3", kind: lexer::ast::LiteralKind::PureInteger { slice: "3" } },
                        },
                    }),
                    ops: vec![],
                }),
            ]},
        }))),
    );
    assert_eq!(
        parser::term(&context)("(1, 2, 3,)").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "(1, 2, 3,)",
            kind: ast::TermKind::Tuple { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "1",
                    term: Rc::new(ast::Term {
                        slice: "1",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "1", kind: lexer::ast::LiteralKind::PureInteger { slice: "1" } },
                        },
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " 2",
                    term: Rc::new(ast::Term {
                        slice: " 2",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "2", kind: lexer::ast::LiteralKind::PureInteger { slice: "2" } },
                        },
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " 3",
                    term: Rc::new(ast::Term {
                        slice: " 3",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "3", kind: lexer::ast::LiteralKind::PureInteger { slice: "3" } },
                        },
                    }),
                    ops: vec![],
                }),
            ]},
        }))),
    );
    assert_eq!(
        parser::term(&context)("(1,)").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "(1,)",
            kind: ast::TermKind::Tuple { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "1",
                    term: Rc::new(ast::Term {
                        slice: "1",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "1", kind: lexer::ast::LiteralKind::PureInteger { slice: "1" } },
                        },
                    }),
                    ops: vec![],
                }),
            ]},
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
            kind: ast::TermKind::ArrayCtor { iter_expr: None },
        }))),
    );
    assert_eq!(
        parser::term(&context)("[0..10]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[0..10]",
            kind: ast::TermKind::ArrayCtor {
                iter_expr: Some(ast::IterExpr {
                    slice: "0..10",
                    kind: ast::IterExprKind::Range {
                        left: Rc::new(ast::Expr {
                            slice: "0",
                            term: Rc::new(ast::Term {
                                slice: "0",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                                },
                            }),
                            ops: vec![],
                        }),
                        right: Rc::new(ast::Expr {
                            slice: "10",
                            term: Rc::new(ast::Term {
                                slice: "10",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "10", kind: lexer::ast::LiteralKind::PureInteger { slice: "10" } },
                                },
                            }),
                            ops: vec![],
                        }),
                    },
                }),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("[0..10..2]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[0..10..2]",
            kind: ast::TermKind::ArrayCtor {
                iter_expr: Some(ast::IterExpr {
                    slice: "0..10..2",
                    kind: ast::IterExprKind::SteppedRange {
                        left: Rc::new(ast::Expr {
                            slice: "0",
                            term: Rc::new(ast::Term {
                                slice: "0",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                                },
                            }),
                            ops: vec![],
                        }),
                        right: Rc::new(ast::Expr {
                            slice: "10",
                            term: Rc::new(ast::Term {
                                slice: "10",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "10", kind: lexer::ast::LiteralKind::PureInteger { slice: "10" } },
                                },
                            }),
                            ops: vec![],
                        }),
                        step: Rc::new(ast::Expr {
                            slice: "2",
                            term: Rc::new(ast::Term {
                                slice: "2",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "2", kind: lexer::ast::LiteralKind::PureInteger { slice: "2" } },
                                },
                            }),
                            ops: vec![],
                        }),
                    },
                }),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("[...x]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[...x]",
            kind: ast::TermKind::ArrayCtor {
                iter_expr: Some(ast::IterExpr {
                    slice: "...x",
                    kind: ast::IterExprKind::Spread {
                        expr: Rc::new(ast::Expr {
                            slice: "x",
                            term: Rc::new(ast::Term {
                                slice: "x",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                            }),
                            ops: vec![],
                        }),
                    },
                }),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[1, 2, 3]",
            kind: ast::TermKind::ArrayCtor {
                iter_expr: Some(ast::IterExpr {
                    slice: "1, 2, 3",
                    kind: ast::IterExprKind::Elements { exprs: vec![
                        Rc::new(ast::Expr {
                            slice: "1",
                            term: Rc::new(ast::Term {
                                slice: "1",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "1", kind: lexer::ast::LiteralKind::PureInteger { slice: "1" } },
                                },
                            }),
                            ops: vec![],
                        }),
                        Rc::new(ast::Expr {
                            slice: " 2",
                            term: Rc::new(ast::Term {
                                slice: " 2",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "2", kind: lexer::ast::LiteralKind::PureInteger { slice: "2" } },
                                },
                            }),
                            ops: vec![],
                        }),
                        Rc::new(ast::Expr {
                            slice: " 3",
                            term: Rc::new(ast::Term {
                                slice: " 3",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "3", kind: lexer::ast::LiteralKind::PureInteger { slice: "3" } },
                                },
                            }),
                            ops: vec![],
                        }),
                    ]},
                }),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3,]").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "[1, 2, 3,]",
            kind: ast::TermKind::ArrayCtor {
                iter_expr: Some(ast::IterExpr {
                    slice: "1, 2, 3,",
                    kind: ast::IterExprKind::Elements { exprs: vec![
                        Rc::new(ast::Expr {
                            slice: "1",
                            term: Rc::new(ast::Term {
                                slice: "1",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "1", kind: lexer::ast::LiteralKind::PureInteger { slice: "1" } },
                                },
                            }),
                            ops: vec![],
                        }),
                        Rc::new(ast::Expr {
                            slice: " 2",
                            term: Rc::new(ast::Term {
                                slice: " 2",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "2", kind: lexer::ast::LiteralKind::PureInteger { slice: "2" } },
                                },
                            }),
                            ops: vec![],
                        }),
                        Rc::new(ast::Expr {
                            slice: " 3",
                            term: Rc::new(ast::Term {
                                slice: " 3",
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "3", kind: lexer::ast::LiteralKind::PureInteger { slice: "3" } },
                                },
                            }),
                            ops: vec![],
                        }),
                    ]},
                }),
            },
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
                kind: ast::TermKind::Literal {
                    literal: lexer::ast::Literal {
                        slice: "()",
                        kind: lexer::ast::LiteralKind::Unit {
                            left: lexer::ast::OpCode { slice: "(", kind: lexer::ast::OpCodeKind::OpenParen },
                            right: lexer::ast::OpCode { slice: ")", kind: lexer::ast::OpCodeKind::CloseParen },
                        },
                    },
                },
            }),
        )),
    );
    assert_eq!(
        parser::term(&context)("null").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "null",
            kind: ast::TermKind::Literal {
                literal: lexer::ast::Literal {
                    slice: "null",
                    kind: lexer::ast::LiteralKind::Null { keyword: lexer::ast::Keyword { slice: "null", kind: lexer::ast::KeywordKind::Null } }
                },
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("true").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "true",
            kind: ast::TermKind::Literal {
                literal: lexer::ast::Literal {
                    slice: "true",
                    kind: lexer::ast::LiteralKind::Bool { keyword: lexer::ast::Keyword { slice: "true", kind: lexer::ast::KeywordKind::True } },
                },
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("false").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "false",
            kind: ast::TermKind::Literal {
                literal: lexer::ast::Literal {
                    slice: "false",
                    kind: lexer::ast::LiteralKind::Bool { keyword: lexer::ast::Keyword { slice: "false", kind: lexer::ast::KeywordKind::False } },
                },
            },
        }))),
    );
    assert_eq!(parser::term(&context)("123.45").ok(), Some(("", Rc::new(ast::Term {
        slice: "123.45",
        kind: ast::TermKind::Literal { literal: lexer::ast::Literal { slice: "123.45", kind: lexer::ast::LiteralKind::RealNumber { slice: "123.45" } } }
    }))));
    assert_eq!(parser::term(&context)("0x1AF").ok(), Some(("", Rc::new(ast::Term {
        slice: "0x1AF",
        kind: ast::TermKind::Literal { literal: lexer::ast::Literal { slice: "0x1AF", kind: lexer::ast::LiteralKind::HexInteger { slice: "0x1AF" } } }
    }))));
    assert_eq!(parser::term(&context)("0b101").ok(), Some(("", Rc::new(ast::Term {
        slice: "0b101",
        kind: ast::TermKind::Literal { literal: lexer::ast::Literal { slice: "0b101", kind: lexer::ast::LiteralKind::BinInteger { slice: "0b101" } } }
    }))));
    assert_eq!(parser::term(&context)("123").ok(), Some(("", Rc::new(ast::Term {
        slice: "123",
        kind: ast::TermKind::Literal {
            literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
        },
    }))));
    assert_eq!(parser::term(&context)("'a'").ok(), Some(("", Rc::new(ast::Term {
        slice: "'a'",
        kind: ast::TermKind::Literal { literal: lexer::ast::Literal { slice: "a", kind: lexer::ast::LiteralKind::Character { slice: "a" } } }
    }))));
    assert_eq!(parser::term(&context)("\"abc\"").ok(), Some(("", Rc::new(ast::Term {
        slice: "\"abc\"",
        kind: ast::TermKind::Literal { literal: lexer::ast::Literal { slice: "abc", kind: lexer::ast::LiteralKind::RegularString { slice: "abc" } } }
    }))));
    assert_eq!(parser::term(&context)("@\"\\abc\"").ok(), Some(("", Rc::new(ast::Term {
        slice: "@\"\\abc\"",
        kind: ast::TermKind::Literal { literal: lexer::ast::Literal { slice: "\\abc", kind: lexer::ast::LiteralKind::VerbatiumString { slice: "\\abc" } } }
    }))));
}

#[test]
fn test_this_literal_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("this").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "this",
            kind: ast::TermKind::ThisLiteral {
                literal: lexer::ast::Literal {
                    slice: "this",
                    kind: lexer::ast::LiteralKind::This { keyword: lexer::ast::Keyword { slice: "this", kind: lexer::ast::KeywordKind::This } }
                },
            },
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
            kind: ast::TermKind::InterpolatedString {
                interpolated_string: lexer::ast::InterpolatedString {
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
                                kind: ast::TermKind::Literal {
                                    literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                                },
                            }),
                            ops: vec![]
                        }),
                        Rc::new(ast::Expr {
                            slice: "x",
                            term: Rc::new(ast::Term {
                                slice: "x",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                            }),
                            ops: vec![],
                        }),
                    ],
                },
            },
        }))),
    );
}

#[test]
fn test_eval_var_term() {
    let context = Context::new().unwrap();
    assert_eq!(parser::term(&context)("someVar").ok(), Some(("", Rc::new(ast::Term {
        slice: "someVar",
        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "someVar" } },
    }))));
    assert_eq!(parser::term(&context)("some_var").ok(), Some(("", Rc::new(ast::Term {
        slice: "some_var",
        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "some_var" } },
    }))));
}

#[test]
fn test_let_in_bind_term() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::term(&context)("let mut i: int = 123 in i + 1").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "let mut i: int = 123 in i + 1",
            kind: ast::TermKind::LetInBind {
                var_bind: ast::VarBind {
                    slice: "let mut i: int = 123",
                    let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                    var_decl: ast::VarDecl {
                        slice: " mut i: int",
                        kind: ast::VarDeclKind::SingleDecl {
                            mut_attr: Some(ast::MutAttr { slice: " mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
                            ident: lexer::ast::Ident { slice: "i" },
                            ty_expr: Some(Rc::new(ast::TyExpr {
                                slice: " int",
                                ty_term: Rc::new(ast::TyTerm {
                                    slice: " int",
                                    kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                                }),
                                ty_ops: vec![],
                            })),
                        },
                    },
                    expr: Rc::new(ast::Expr {
                        slice: " 123",
                        term: Rc::new(ast::Term {
                            slice: " 123",
                            kind: ast::TermKind::Literal {
                                literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                            },
                        }),
                        ops: vec![]
                    }),
                },
                in_keyword: lexer::ast::Keyword { slice: "in", kind: lexer::ast::KeywordKind::In },
                expr: Rc::new(ast::Expr {
                    slice: " i + 1",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "i" } },
                    }),
                    ops: vec![
                        ast::Op {
                            slice: " + 1",
                            kind: ast::OpKind::InfixOp {
                                op_code: lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus },
                                term: Rc::new(ast::Term {
                                    slice: " 1",
                                    kind: ast::TermKind::Literal {
                                        literal: lexer::ast::Literal { slice: "1", kind: lexer::ast::LiteralKind::PureInteger { slice: "1" } },
                                    },
                                }),
                            },
                        },
                    ],
                }),
            },
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
            kind: ast::TermKind::If {
                if_keyword: lexer::ast::Keyword { slice: "if", kind: lexer::ast::KeywordKind::If },
                condition: Rc::new(ast::Expr {
                    slice: " i == 0",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "i" } },
                    }),
                    ops: vec![
                        ast::Op {
                            slice: " == 0",
                            kind: ast::OpKind::InfixOp {
                                op_code: lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq },
                                term: Rc::new(ast::Term {
                                    slice: " 0",
                                    kind: ast::TermKind::Literal {
                                        literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                                    },
                                }),
                            },
                        },
                    ],
                }),
                if_part: ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        ast::Stat {
                            slice: " f()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                    ],
                    ret: None,
                },
                else_part: None,
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); } else { g(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "if i == 0 { f(); } else { g(); }",
            kind: ast::TermKind::If {
                if_keyword: lexer::ast::Keyword { slice: "if", kind: lexer::ast::KeywordKind::If },
                condition: Rc::new(ast::Expr {
                    slice: " i == 0",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "i" } },
                    }),
                    ops: vec![
                        ast::Op {
                            slice: " == 0",
                            kind: ast::OpKind::InfixOp {
                                op_code: lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq },
                                term: Rc::new(ast::Term {
                                    slice: " 0",
                                    kind: ast::TermKind::Literal {
                                        literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                                    },
                                }),
                            },
                        },
                    ],
                }),
                if_part: ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        ast::Stat {
                            slice: " f()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                    ],
                    ret: None,
                },
                else_part: Some((
                    lexer::ast::Keyword { slice: "else", kind: lexer::ast::KeywordKind::Else },
                    ast::StatsBlock {
                        slice: " { g(); }",
                        stats: vec![
                            ast::Stat {
                                slice: " g()",
                                kind: ast::StatKind::Expr {
                                    expr: Rc::new(ast::Expr {
                                        slice: " g()",
                                        term: Rc::new(ast::Term {
                                            slice: " g",
                                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "g" } },
                                        }),
                                        ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                    }),
                                },
                            },
                        ],
                        ret: None,
                    },
                )),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); } else if j == 0 { g(); }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "if i == 0 { f(); } else if j == 0 { g(); }",
            kind: ast::TermKind::If {
                if_keyword: lexer::ast::Keyword { slice: "if", kind: lexer::ast::KeywordKind::If },
                condition: Rc::new(ast::Expr {
                    slice: " i == 0",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "i" } },
                    }),
                    ops: vec![
                        ast::Op {
                            slice: " == 0",
                            kind: ast::OpKind::InfixOp {
                                op_code: lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq },
                                term: Rc::new(ast::Term {
                                    slice: " 0",
                                    kind: ast::TermKind::Literal {
                                        literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                                    },
                                }),
                            },
                        },
                    ]
                }),
                if_part: ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        ast::Stat {
                            slice: " f()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                    ],
                    ret: None,
                },
                else_part: Some((
                    lexer::ast::Keyword { slice: "else", kind: lexer::ast::KeywordKind::Else },
                    ast::StatsBlock {
                        slice: " if j == 0 { g(); }",
                        stats: vec![],
                        ret: Some(Rc::new(ast::Expr {
                            slice: " if j == 0 { g(); }",
                            term: Rc::new(ast::Term {
                                slice: " if j == 0 { g(); }",
                                kind: ast::TermKind::If {
                                    if_keyword: lexer::ast::Keyword { slice: "if", kind: lexer::ast::KeywordKind::If },
                                    condition: Rc::new(ast::Expr {
                                        slice: " j == 0",
                                        term: Rc::new(ast::Term {
                                            slice: " j",
                                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "j" } },
                                        }),
                                        ops: vec![
                                            ast::Op {
                                                slice: " == 0",
                                                kind: ast::OpKind::InfixOp {
                                                    op_code: lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq },
                                                    term: Rc::new(ast::Term {
                                                        slice: " 0",
                                                        kind: ast::TermKind::Literal {
                                                            literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                                                        },
                                                    }),
                                                },
                                            },
                                        ],
                                    }),
                                    if_part: ast::StatsBlock {
                                        slice: " { g(); }",
                                        stats: vec![
                                            ast::Stat {
                                                slice: " g()",
                                                kind: ast::StatKind::Expr {
                                                    expr: Rc::new(ast::Expr {
                                                        slice: " g()",
                                                        term: Rc::new(ast::Term {
                                                            slice: " g",
                                                            kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "g" } },
                                                        }),
                                                        ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                                    }),
                                                },
                                            },
                                        ],
                                        ret: None,
                                    },
                                    else_part: None,
                                },
                            }),
                            ops: vec![],
                        })),
                    },
                )),
            },
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
            kind: ast::TermKind::While {
                while_keyword: lexer::ast::Keyword { slice: "while", kind: lexer::ast::KeywordKind::While },
                condition: Rc::new(ast::Expr {
                    slice: " i == 0",
                    term: Rc::new(ast::Term {
                        slice: " i",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "i" } },
                    }),
                    ops: vec![
                        ast::Op {
                            slice: " == 0",
                            kind: ast::OpKind::InfixOp {
                                op_code: lexer::ast::OpCode { slice: "==", kind: lexer::ast::OpCodeKind::Eq },
                                term: Rc::new(ast::Term {
                                    slice: " 0",
                                    kind: ast::TermKind::Literal {
                                        literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                                    },
                                }),
                            },
                        },
                    ],
                }),
                stats: ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        ast::Stat {
                            slice: " f()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                    ],
                    ret: None,
                },
            },
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
            kind: ast::TermKind::Loop {
                loop_keyword: lexer::ast::Keyword { slice: "loop", kind: lexer::ast::KeywordKind::Loop },
                stats: ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        ast::Stat {
                            slice: " f()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                    ],
                    ret: None,
                },
            },
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
            kind: ast::TermKind::For {
                for_binds: vec![
                    (
                        lexer::ast::Keyword { slice: "for", kind: lexer::ast::KeywordKind::For },
                        ast::ForBind {
                            slice: " let i <- 0..10",
                            kind: ast::ForBindKind::Let {
                                let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                                var_decl: ast::VarDecl {
                                    slice: " i",
                                    kind: ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: lexer::ast::Ident { slice: "i" },
                                        ty_expr: None
                                    },
                                },
                                for_iter_expr: ast::ForIterExpr {
                                    slice: " 0..10",
                                    kind: ast::ForIterExprKind::Range {
                                        left: Rc::new(ast::Expr {
                                            slice: " 0",
                                            term: Rc::new(ast::Term {
                                                slice: " 0",
                                                kind: ast::TermKind::Literal {
                                                    literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                                                },
                                            }),
                                            ops: vec![],
                                        }),
                                        right: Rc::new(ast::Expr {
                                            slice: "10",
                                            term: Rc::new(ast::Term {
                                                slice: "10",
                                                kind: ast::TermKind::Literal {
                                                    literal: lexer::ast::Literal { slice: "10", kind: lexer::ast::LiteralKind::PureInteger { slice: "10" } },
                                                },
                                            }),
                                            ops: vec![],
                                        }),
                                    },
                                },
                            },
                        },
                    ),
                    (
                        lexer::ast::Keyword { slice: "for", kind: lexer::ast::KeywordKind::For },
                        ast::ForBind {
                            slice: " let j <- 0..10..2",
                            kind: ast::ForBindKind::Let {
                                let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                                var_decl: ast::VarDecl {
                                    slice: " j",
                                    kind: ast::VarDeclKind::SingleDecl {
                                        mut_attr: None,
                                        ident: lexer::ast::Ident { slice: "j" },
                                        ty_expr: None,
                                    },
                                },
                                for_iter_expr: ast::ForIterExpr {
                                    slice: " 0..10..2",
                                    kind: ast::ForIterExprKind::SteppedRange {
                                        left: Rc::new(ast::Expr {
                                            slice: " 0",
                                            term: Rc::new(ast::Term {
                                                slice: " 0",
                                                kind: ast::TermKind::Literal {
                                                    literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                                                },
                                            }),
                                            ops: vec![],
                                        }),
                                        right: Rc::new(ast::Expr {
                                            slice: "10",
                                            term: Rc::new(ast::Term {
                                                slice: "10",
                                                kind: ast::TermKind::Literal {
                                                    literal: lexer::ast::Literal { slice: "10", kind: lexer::ast::LiteralKind::PureInteger { slice: "10" } },
                                                },
                                            }),
                                            ops: vec![],
                                        }),
                                        step: Rc::new(ast::Expr {
                                            slice: "2",
                                            term: Rc::new(ast::Term {
                                                slice: "2",
                                                kind: ast::TermKind::Literal {
                                                    literal: lexer::ast::Literal { slice: "2", kind: lexer::ast::LiteralKind::PureInteger { slice: "2" } },
                                                },
                                            }),
                                            ops: vec![],
                                        }),
                                    },
                                },
                            },
                        },
                    ),
                    (
                        lexer::ast::Keyword { slice: "for", kind: lexer::ast::KeywordKind::For },
                        ast::ForBind {
                            slice: " k <- arr",
                            kind: ast::ForBindKind::Assign {
                                left: Rc::new(ast::Expr {
                                    slice: " k",
                                    term: Rc::new(ast::Term {
                                        slice: " k",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "k" } },
                                    }),
                                    ops: vec![],
                                }),
                                for_iter_expr: ast::ForIterExpr {
                                    slice: " arr",
                                    kind: ast::ForIterExprKind::Spread {
                                        expr: Rc::new(ast::Expr {
                                            slice: " arr",
                                            term: Rc::new(ast::Term {
                                                slice: " arr",
                                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "arr" } },
                                            }),
                                            ops: vec![],
                                        }),
                                    },
                                },
                            },
                        },
                    ),
                ],
                stats: ast::StatsBlock {
                    slice: " { f(); }",
                    stats: vec![
                        ast::Stat {
                            slice: " f()",
                            kind: ast::StatKind::Expr {
                                expr: Rc::new(ast::Expr {
                                    slice: " f()",
                                    term: Rc::new(ast::Term {
                                        slice: " f",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "f" } },
                                    }),
                                    ops: vec![ast::Op { slice: "()", kind: ast::OpKind::EvalFn { arg_exprs: vec![] } }],
                                }),
                            },
                        },
                    ],
                    ret: None,
                },
            },
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
            kind: ast::TermKind::Closure {
                var_decl: ast::VarDecl {
                    slice: "||",
                    kind: ast::VarDeclKind::TupleDecl { var_decls: vec![] },
                },
                expr: Rc::new(ast::Expr {
                    slice: " 123",
                    term: Rc::new(ast::Term {
                        slice: " 123",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "123", kind: lexer::ast::LiteralKind::PureInteger { slice: "123" } },
                        },
                    }),
                    ops: vec![],
                }),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("|x| x").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "|x| x",
            kind: ast::TermKind::Closure {
                var_decl: ast::VarDecl {
                    slice: "|x|",
                    kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                        ast::VarDecl {
                            slice: "x",
                            kind: ast::VarDeclKind::SingleDecl {
                                mut_attr: None,
                                ident: lexer::ast::Ident { slice: "x" },
                                ty_expr: None
                            },
                        },
                    ]},
                },
                expr: Rc::new(ast::Expr {
                    slice: " x",
                    term: Rc::new(ast::Term {
                        slice: " x",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                    }),
                    ops: vec![]
                }),
            },
        }))),
    );
    assert_eq!(
        parser::term(&context)("|x: int, y: int| { x + y }").ok(),
        Some(("", Rc::new(ast::Term {
            slice: "|x: int, y: int| { x + y }",
            kind: ast::TermKind::Closure {
                var_decl: ast::VarDecl {
                    slice: "|x: int, y: int|",
                    kind: ast::VarDeclKind::TupleDecl { var_decls: vec![
                        ast::VarDecl {
                            slice: "x: int",
                            kind: ast::VarDeclKind::SingleDecl {
                                mut_attr: None,
                                ident: lexer::ast::Ident { slice: "x" },
                                ty_expr: Some(Rc::new(ast::TyExpr {
                                    slice: " int",
                                    ty_term: Rc::new(ast::TyTerm {
                                        slice: " int",
                                        kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                                    }),
                                    ty_ops: vec![],
                                })),
                            },
                        },
                        ast::VarDecl {
                            slice: " y: int",
                            kind: ast::VarDeclKind::SingleDecl {
                                mut_attr: None,
                                ident: lexer::ast::Ident { slice: "y" },
                                ty_expr: Some(Rc::new(ast::TyExpr {
                                    slice: " int",
                                    ty_term: Rc::new(ast::TyTerm {
                                        slice: " int",
                                        kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                                    }),
                                    ty_ops: vec![],
                                })),
                            },
                        },
                    ]},
                },
                expr: Rc::new(ast::Expr {
                    slice: " { x + y }",
                    term: Rc::new(ast::Term {
                        slice: " { x + y }",
                        kind: ast::TermKind::Block {
                            stats: ast::StatsBlock {
                                slice: " { x + y }",
                                stats: vec![],
                                ret: Some(Rc::new(ast::Expr {
                                    slice: " x + y",
                                    term: Rc::new(ast::Term {
                                        slice: " x",
                                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                                    }),
                                    ops: vec![
                                        ast::Op {
                                            slice: " + y",
                                            kind: ast::OpKind::InfixOp {
                                                op_code: lexer::ast::OpCode { slice: "+", kind: lexer::ast::OpCodeKind::Plus },
                                                term: Rc::new(ast::Term {
                                                    slice: " y",
                                                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "y" } },
                                                }),
                                            },
                                        },
                                    ],
                                })),
                            },
                        },
                    }),
                    ops: vec![],
                }),
            },
        }))),
    );
}

#[test]
fn test_range_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::iter_expr(&context)("0..10").ok(),
        Some(("", ast::IterExpr {
            slice: "0..10",
            kind: ast::IterExprKind::Range {
                left: Rc::new(ast::Expr {
                    slice: "0",
                    term: Rc::new(ast::Term {
                        slice: "0",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                        },
                    }),
                    ops: vec![],
                }),
                right: Rc::new(ast::Expr {
                    slice: "10",
                    term: Rc::new(ast::Term {
                        slice: "10",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "10", kind: lexer::ast::LiteralKind::PureInteger { slice: "10" } },
                        },
                    }),
                    ops: vec![],
                }),
            },
        })),
    );
}

#[test]
fn test_stepped_range_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::iter_expr(&context)("0..10..2").ok(),
        Some(("", ast::IterExpr {
            slice: "0..10..2",
            kind: ast::IterExprKind::SteppedRange {
                left: Rc::new(ast::Expr {
                    slice: "0",
                    term: Rc::new(ast::Term {
                        slice: "0",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                        },
                    }),
                    ops: vec![],
                }),
                right: Rc::new(ast::Expr {
                    slice: "10",
                    term: Rc::new(ast::Term {
                        slice: "10",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "10", kind: lexer::ast::LiteralKind::PureInteger { slice: "10" } },
                        },
                    }),
                    ops: vec![],
                }),
                step: Rc::new(ast::Expr {
                    slice: "2",
                    term: Rc::new(ast::Term {
                        slice: "2",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "2", kind: lexer::ast::LiteralKind::PureInteger { slice: "2" } },
                        },
                    }),
                    ops: vec![],
                }),
            },
        })),
    );
}

#[test]
fn test_spread_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::iter_expr(&context)("...arr").ok(),
        Some(("", ast::IterExpr {
            slice: "...arr",
            kind: ast::IterExprKind::Spread {
                expr: Rc::new(ast::Expr {
                    slice: "arr",
                    term: Rc::new(ast::Term {
                        slice: "arr",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "arr" } },
                    }),
                    ops: vec![]
                }),
            },
        })),
    );
}

#[test]
fn test_elements_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::iter_expr(&context)("x, y").ok(),
        Some(("", ast::IterExpr {
            slice: "x, y",
            kind: ast::IterExprKind::Elements { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " y",
                    term: Rc::new(ast::Term {
                        slice: " y",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "y" } },
                    }),
                    ops: vec![],
                }),
            ]},
        })),
    );
    assert_eq!(
        parser::iter_expr(&context)("x, y,").ok(),
        Some(("", ast::IterExpr {
            slice: "x, y,",
            kind: ast::IterExprKind::Elements { exprs: vec![
                Rc::new(ast::Expr {
                    slice: "x",
                    term: Rc::new(ast::Term {
                        slice: "x",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                    }),
                    ops: vec![],
                }),
                Rc::new(ast::Expr {
                    slice: " y",
                    term: Rc::new(ast::Term {
                        slice: " y",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "y" } },
                    }),
                    ops: vec![],
                }),
            ]},
        })),
    );
}

#[test]
fn test_arg_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::arg_expr(&context)("x").ok(),
        Some(("", ast::ArgExpr {
            slice: "x",
            mut_attr: None,
            expr: Rc::new(ast::Expr {
                slice: "x",
                term: Rc::new(ast::Term {
                    slice: "x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
                ops: vec![],
            }),
        })),
    );
    assert_eq!(
        parser::arg_expr(&context)("mut x").ok(),
        Some(("", ast::ArgExpr {
            slice: "mut x",
            mut_attr: Some(ast::MutAttr { slice: "mut", attr: lexer::ast::Keyword { slice: "mut", kind: lexer::ast::KeywordKind::Mut } }),
            expr: Rc::new(ast::Expr {
                slice: " x",
                term: Rc::new(ast::Term {
                    slice: " x",
                    kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "x" } },
                }),
                ops: vec![],
            }),
        })),
    );
}

#[test]
fn test_let_for_bind() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_bind(&context)("let i: int <- arr").ok(),
        Some(("", ast::ForBind {
            slice: "let i: int <- arr",
            kind: ast::ForBindKind::Let {
                let_keyword: lexer::ast::Keyword { slice: "let", kind: lexer::ast::KeywordKind::Let },
                var_decl: ast::VarDecl {
                    slice: " i: int",
                    kind: ast::VarDeclKind::SingleDecl {
                        mut_attr: None,
                        ident: lexer::ast::Ident { slice: "i" },
                        ty_expr: Some(Rc::new(ast::TyExpr {
                            slice: " int",
                            ty_term: Rc::new(ast::TyTerm {
                                slice: " int",
                                kind: ast::TyTermKind::EvalTy { ident: lexer::ast::Ident { slice: "int" } },
                            }),
                            ty_ops: vec![],
                        })),
                    },
                },
                for_iter_expr: ast::ForIterExpr {
                    slice: " arr",
                    kind: ast::ForIterExprKind::Spread {
                        expr: Rc::new(ast::Expr {
                            slice: " arr",
                            term: Rc::new(ast::Term {
                                slice: " arr",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "arr" } },
                            }),
                            ops: vec![],
                        }),
                    },
                },
            },
        })),
    );
}

#[test]
fn test_assign_for_bind() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_bind(&context)("i <- arr").ok(),
        Some(("", ast::ForBind {
            slice: "i <- arr",
            kind: ast::ForBindKind::Assign {
                left: Rc::new(ast::Expr {
                    slice: "i",
                    term: Rc::new(ast::Term {
                        slice: "i",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "i" } },
                    }),
                    ops: vec![],
                }),
                for_iter_expr: ast::ForIterExpr {
                    slice: " arr",
                    kind: ast::ForIterExprKind::Spread {
                        expr: Rc::new(ast::Expr {
                            slice: " arr",
                            term: Rc::new(ast::Term {
                                slice: " arr",
                                kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "arr" } },
                            }),
                            ops: vec![],
                        }),
                    },
                },
            },
        })),
    );
}

#[test]
fn test_range_for_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_iter_expr(&context)("0..10").ok(),
        Some(("", ast::ForIterExpr {
            slice: "0..10",
            kind: ast::ForIterExprKind::Range {
                left: Rc::new(ast::Expr {
                    slice: "0",
                    term: Rc::new(ast::Term {
                        slice: "0",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                        },
                    }),
                    ops: vec![],
                }),
                right: Rc::new(ast::Expr {
                    slice: "10",
                    term: Rc::new(ast::Term {
                        slice: "10",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "10", kind: lexer::ast::LiteralKind::PureInteger { slice: "10" } },
                        },
                    }),
                    ops: vec![],
                }),
            },
        })),
    );
}

#[test]
fn test_stepped_range_for_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_iter_expr(&context)("0..10..2").ok(),
        Some(("", ast::ForIterExpr {
            slice: "0..10..2",
            kind: ast::ForIterExprKind::SteppedRange {
                left: Rc::new(ast::Expr {
                    slice: "0",
                    term: Rc::new(ast::Term {
                        slice: "0",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "0", kind: lexer::ast::LiteralKind::PureInteger { slice: "0" } },
                        },
                    }),
                    ops: vec![],
                }),
                right: Rc::new(ast::Expr {
                    slice: "10",
                    term: Rc::new(ast::Term {
                        slice: "10",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "10", kind: lexer::ast::LiteralKind::PureInteger { slice: "10" } },
                        },
                    }),
                    ops: vec![],
                }),
                step: Rc::new(ast::Expr {
                    slice: "2",
                    term: Rc::new(ast::Term {
                        slice: "2",
                        kind: ast::TermKind::Literal {
                            literal: lexer::ast::Literal { slice: "2", kind: lexer::ast::LiteralKind::PureInteger { slice: "2" } },
                        },
                    }),
                    ops: vec![],
                }),
            },
        })),
    );
}

#[test]
fn test_spread_for_iter_expr() {
    let context = Context::new().unwrap();
    assert_eq!(
        parser::for_iter_expr(&context)("arr").ok(),
        Some(("", ast::ForIterExpr {
            slice: "arr",
            kind: ast::ForIterExprKind::Spread {
                expr: Rc::new(ast::Expr {
                    slice: "arr",
                    term: Rc::new(ast::Term {
                        slice: "arr",
                        kind: ast::TermKind::EvalVar { ident: lexer::ast::Ident { slice: "arr" } },
                    }),
                    ops: vec![],
                }),
            },
        })),
    );
}
