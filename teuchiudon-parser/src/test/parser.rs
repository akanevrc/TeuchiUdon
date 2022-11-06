use std::rc::Rc;
use crate::context::Context;
use crate::lexer;
use crate::parser::{
    self,
    ast,
};

#[test]
fn test_target() {
    let context = Context::new();
    assert_eq!(
        parser::target(&context)("pub let x = 123; pub fn f() {};").ok(),
        Some(("", ast::Target(
            Some(ast::Body(vec![
                ast::TopStat::VarBind(
                    Some(ast::AccessAttr(lexer::ast::Keyword("pub", lexer::ast::KeywordKind::Pub))),
                    None,
                    ast::VarBind(
                        lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
                        Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
                    ),
                ),
                ast::TopStat::FnBind(
                    Some(ast::AccessAttr(lexer::ast::Keyword("pub", lexer::ast::KeywordKind::Pub))),
                    ast::FnBind(
                        lexer::ast::Keyword("fn", lexer::ast::KeywordKind::Fn),
                        ast::FnDecl(
                            lexer::ast::Ident("f"),
                            ast::VarDecl::TupleDecl(vec![]),
                            None,
                        ),
                        ast::StatsBlock(vec![], None),
                    ),
                ),
            ]),
        )))),
    );
}

#[test]
fn test_body() {
    let context = Context::new();
    assert_eq!(
        parser::body(&context)("pub let x = 123; pub fn f() {};").ok(),
        Some(("", ast::Body(vec![
            ast::TopStat::VarBind(
                Some(ast::AccessAttr(lexer::ast::Keyword("pub", lexer::ast::KeywordKind::Pub))),
                None,
                ast::VarBind(
                    lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
                    ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
                    Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
                ),
            ),
            ast::TopStat::FnBind(
                Some(ast::AccessAttr(lexer::ast::Keyword("pub", lexer::ast::KeywordKind::Pub))),
                ast::FnBind(
                    lexer::ast::Keyword("fn", lexer::ast::KeywordKind::Fn),
                    ast::FnDecl(
                        lexer::ast::Ident("f"),
                        ast::VarDecl::TupleDecl(vec![]),
                        None,
                    ),
                    ast::StatsBlock(vec![], None),
                ),
            ),
        ]))),
    );
}

#[test]
fn test_var_bind_top_stat() {
    let context = Context::new();
    assert_eq!(
        parser::top_stat(&context)("pub sync let mut x = 123;").ok(),
        Some(("", ast::TopStat::VarBind(
            Some(ast::AccessAttr(lexer::ast::Keyword("pub", lexer::ast::KeywordKind::Pub))),
            Some(ast::SyncAttr(lexer::ast::Keyword("sync", lexer::ast::KeywordKind::Sync))),
            ast::VarBind(
                lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                    lexer::ast::Ident("x"),
                    None,
                ),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
            ),
        ))),
    );
}

#[test]
fn test_fn_bind_top_stat() {
    let context = Context::new();
    assert_eq!(
        parser::top_stat(&context)("pub fn f(x: int) -> int { x };").ok(),
        Some(("", ast::TopStat::FnBind(
            Some(ast::AccessAttr(lexer::ast::Keyword("pub", lexer::ast::KeywordKind::Pub))),
            ast::FnBind(
                lexer::ast::Keyword("fn", lexer::ast::KeywordKind::Fn),
                ast::FnDecl(
                    lexer::ast::Ident("f"),
                    ast::VarDecl::TupleDecl(vec![
                        ast::VarDecl::SingleDecl(
                            None,
                            lexer::ast::Ident("x"),
                            Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                        ),
                    ]),
                    Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                ),
                ast::StatsBlock(
                    vec![],
                    Some(Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
                ),
            ),
        ))),
    );
}

#[test]
fn test_stat_top_stat() {
    let context = Context::new();
    assert_eq!(
        parser::top_stat(&context)("f();").ok(),
        Some(("", ast::TopStat::Stat(
            ast::Stat::Expr(
                Rc::new(ast::Expr(
                    Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                    vec![ast::Op::EvalFn(vec![])],
                )),
            ),
        ))),
    );
}

#[test]
fn test_var_bind() {
    let context = Context::new();
    assert_eq!(
        parser::var_bind(&context)("let mut x: int = 123").ok(),
        Some(("", ast::VarBind(
            lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
            ast::VarDecl::SingleDecl(
                Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                lexer::ast::Ident("x"),
                Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
            ),
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
        ))),
    );
}

#[test]
fn test_single_var_decl() {
    let context = Context::new();
    assert_eq!(
        parser::var_decl(&context)("x").ok(),
        Some(("", ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None))),
    );
    assert_eq!(
        parser::var_decl(&context)("mut x").ok(),
        Some(("", ast::VarDecl::SingleDecl(
            Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
            lexer::ast::Ident("x"),
            None,
        ))),
    );
    assert_eq!(
        parser::var_decl(&context)("mut x: int").ok(),
        Some(("", ast::VarDecl::SingleDecl(
            Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
            lexer::ast::Ident("x"),
            Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
        ))),
    );
}

#[test]
fn test_tuple_var_decl() {
    let context = Context::new();
    assert_eq!(
        parser::var_decl(&context)("()").ok(),
        Some(("", ast::VarDecl::TupleDecl(vec![]))),
    );
    assert_eq!(
        parser::var_decl(&context)("(x)").ok(),
        Some(("", ast::VarDecl::TupleDecl(vec![
            ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
        ]))),
    );
    assert_eq!(
        parser::var_decl(&context)("(mut x: int, y)").ok(),
        Some(("", ast::VarDecl::TupleDecl(vec![
            ast::VarDecl::SingleDecl(
                Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                lexer::ast::Ident("x"),
                Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
            ),
            ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
        ]))),
    );
    assert_eq!(
        parser::var_decl(&context)("(mut x: int, y,)").ok(),
        Some(("", ast::VarDecl::TupleDecl(vec![
            ast::VarDecl::SingleDecl(
                Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                lexer::ast::Ident("x"),
                Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
            ),
            ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
        ]))),
    );
}

#[test]
fn test_fn_bind() {
    let context = Context::new();
    assert_eq!(
        parser::fn_bind(&context)("fn f(mut x: int, y) -> int { g(); x }").ok(),
        Some(("", ast::FnBind(
            lexer::ast::Keyword("fn", lexer::ast::KeywordKind::Fn),
            ast::FnDecl(
                lexer::ast::Ident("f"),
                ast::VarDecl::TupleDecl(vec![
                    ast::VarDecl::SingleDecl(
                        Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                        lexer::ast::Ident("x"),
                        Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                    ),
                    ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
                ]),
                Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
            ),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                            vec![ast::Op::EvalFn(vec![])]
                        )),
                    ),
                ],
                Some(Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
            ),
        ))),
    );
}

#[test]
fn test_fn_decl() {
    let context = Context::new();
    assert_eq!(
        parser::fn_decl(&context)("f()").ok(),
        Some(("", ast::FnDecl(
            lexer::ast::Ident("f"),
            ast::VarDecl::TupleDecl(vec![]),
            None,
        ))),
    );
    assert_eq!(
        parser::fn_decl(&context)("f(mut x: int, y) -> int").ok(),
        Some(("", ast::FnDecl(
            lexer::ast::Ident("f"),
            ast::VarDecl::TupleDecl(vec![
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                    lexer::ast::Ident("x"),
                    Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                ),
                ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
            ]),
            Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
        ))),
    );
}

#[test]
fn test_ty_expr() {
    let context = Context::new();
    assert_eq!(
        parser::ty_expr(&context)("T::U::V").ok(),
        Some(("", Rc::new(ast::TyExpr(
            Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("T"))),
            vec![
                ast::TyOp::Access(
                    lexer::ast::OpCode("::", lexer::ast::OpCodeKind::DoubleColon),
                    Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("U"))),
                ),
                ast::TyOp::Access(
                    lexer::ast::OpCode("::", lexer::ast::OpCodeKind::DoubleColon),
                    Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("V"))),
                ),
            ],
        )))),
    );
}

#[test]
fn test_ty_op() {
    let context = Context::new();
    assert_eq!(
        parser::ty_op(&context)("::T").ok(),
        Some(("", ast::TyOp::Access(
            lexer::ast::OpCode("::", lexer::ast::OpCodeKind::DoubleColon),
            Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("T"))),
        ))),
    );
}

#[test]
fn test_ty_term() {
    let context = Context::new();
    assert_eq!(
        parser::ty_term(&context)("string").ok(),
        Some(("", Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("string"))))),
    );
}

#[test]
fn test_stats_block() {
    let context = Context::new();
    assert_eq!(
        parser::stats_block(&context)("{}").ok(),
        Some(("", ast::StatsBlock(vec![], None))),
    );
    assert_eq!(
        parser::stats_block(&context)("{ f(); x }").ok(),
        Some(("", ast::StatsBlock(
            vec![
                ast::Stat::Expr(
                    Rc::new(ast::Expr(
                        Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                        vec![ast::Op::EvalFn(vec![])]
                    )),
                ),
            ],
            Some(Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
        ))),
    );
    assert_eq!(
        parser::stats_block(&context)("{ f(); x; }").ok(),
        Some(("", ast::StatsBlock(
            vec![
                ast::Stat::Expr(
                    Rc::new(ast::Expr(
                        Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                        vec![ast::Op::EvalFn(vec![])]
                    )),
                ),
                ast::Stat::Expr(Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])))
            ],
            None,
        ))),
    );
}

#[test]
fn test_return_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("return;").ok(),
        Some(("", ast::Stat::Return(lexer::ast::Keyword("return", lexer::ast::KeywordKind::Return), None))),
    );
    assert_eq!(
        parser::stat(&context)("return x;").ok(),
        Some(("", ast::Stat::Return(
            lexer::ast::Keyword("return", lexer::ast::KeywordKind::Return),
            Some(Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
        ))),
    );
}

#[test]
fn test_continue_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("continue;").ok(),
        Some(("", ast::Stat::Continue(lexer::ast::Keyword("continue", lexer::ast::KeywordKind::Continue)))),
    );
}

#[test]
fn test_break_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("break;").ok(),
        Some(("", ast::Stat::Break(lexer::ast::Keyword("break", lexer::ast::KeywordKind::Break)))),
    );
}

#[test]
fn test_var_bind_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("let x = 123;").ok(),
        Some(("", ast::Stat::VarBind(
            ast::VarBind(
                lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
                ast::VarDecl::SingleDecl(
                    None,
                    lexer::ast::Ident("x"),
                    None,
                ),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
            ),
        ))),
    );
    assert_eq!(
        parser::stat(&context)("let mut x: int = 123;").ok(),
        Some(("", ast::Stat::VarBind(
            ast::VarBind(
                lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                    lexer::ast::Ident("x"),
                    Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                ),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
            ),
        ))),
    );
    assert_eq!(
        parser::stat(&context)("let (mut x: int, y) = (123, 456);").ok(),
        Some(("", ast::Stat::VarBind(
            ast::VarBind(
                lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
                ast::VarDecl::TupleDecl(
                    vec![
                        ast::VarDecl::SingleDecl(
                            Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                            lexer::ast::Ident("x"),
                            Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                        ),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
                    ],
                ),
                Rc::new(ast::Expr(
                    Rc::new(ast::Term::Tuple(
                        vec![
                            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
                            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("456"))), vec![])),
                        ],
                    )),
                    vec![]
                )),
            ),
        ))),
    );
}

#[test]
fn test_fn_bind_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("fn f(x: int) -> int { x };").ok(),
        Some(("", ast::Stat::FnBind(
            ast::FnBind(
                lexer::ast::Keyword("fn", lexer::ast::KeywordKind::Fn),
                ast::FnDecl(
                    lexer::ast::Ident("f"),
                    ast::VarDecl::TupleDecl(vec![
                        ast::VarDecl::SingleDecl(
                            None,
                            lexer::ast::Ident("x"),
                            Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                        ),
                    ]),
                    Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                ),
                ast::StatsBlock(
                    vec![],
                    Some(Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
                ),
            ),
        ))),
    );
}

#[test]
fn test_expr_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("x = 123;").ok(),
        Some(("", ast::Stat::Expr(
            Rc::new(ast::Expr(
                Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
                vec![
                    ast::Op::Assign(
                        Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))),
                    )
                ],
            ))
        ))),
    );
}

#[test]
fn test_expr() {
    let context = Context::new();
    assert_eq!(
        parser::expr(&context)("x = T::f(1, 2).t + a.g(...b)[y]").ok(),
        Some(("", Rc::new(ast::Expr(
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
            vec![
                ast::Op::Assign(
                    Rc::new(ast::Term::EvalVar(lexer::ast::Ident("T"))),
                ),
                ast::Op::TyAccess(
                    lexer::ast::OpCode("::", lexer::ast::OpCodeKind::DoubleColon),
                    Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                ),
                ast::Op::EvalFn(vec![
                    ast::ArgExpr(
                        None,
                        Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![])),
                    ),
                    ast::ArgExpr(
                        None,
                        Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
                    )
                ]),
                ast::Op::Access(
                    lexer::ast::OpCode(".", lexer::ast::OpCodeKind::Dot),
                    Rc::new(ast::Term::EvalVar(lexer::ast::Ident("t"))),
                ),
                ast::Op::InfixOp(
                    lexer::ast::OpCode("+", lexer::ast::OpCodeKind::Plus),
                    Rc::new(ast::Term::EvalVar(lexer::ast::Ident("a"))),
                ),
                ast::Op::Access(
                    lexer::ast::OpCode(".", lexer::ast::OpCodeKind::Dot),
                    Rc::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                ),
                ast::Op::EvalSpreadFn(
                    Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("b"))), vec![])),
                ),
                ast::Op::EvalKey(
                    Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![])),
                ),
            ]
        )))),
    );
}

#[test]
fn test_ty_access_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("::x").ok(),
        Some(("", ast::Op::TyAccess(
            lexer::ast::OpCode("::", lexer::ast::OpCodeKind::DoubleColon),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
}

#[test]
fn test_access_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)(".x").ok(),
        Some(("", ast::Op::Access(
            lexer::ast::OpCode(".", lexer::ast::OpCodeKind::Dot),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("?.x").ok(),
        Some(("", ast::Op::Access(
            lexer::ast::OpCode("?.", lexer::ast::OpCodeKind::CoalescingAccess),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
}

#[test]
fn test_eval_fn_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("()").ok(),
        Some(("", ast::Op::EvalFn(vec![]))),
    );
    assert_eq!(
        parser::op(&context)("(mut x, y, z)").ok(),
        Some(("", ast::Op::EvalFn(vec![
            ast::ArgExpr(
                Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))
            ),
            ast::ArgExpr(
                None,
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![]))
            ),
            ast::ArgExpr(
                None,
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("z"))), vec![]))
            ),
        ]))),
    );
    assert_eq!(
        parser::op(&context)("(mut x, y, z,)").ok(),
        Some(("", ast::Op::EvalFn(vec![
            ast::ArgExpr(
                Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))
            ),
            ast::ArgExpr(
                None,
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![]))
            ),
            ast::ArgExpr(
                None,
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("z"))), vec![]))
            ),
        ]))),
    );
}

#[test]
fn test_eval_spread_fn_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("(...x)").ok(),
        Some(("", ast::Op::EvalSpreadFn(
            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
}

#[test]
fn test_eval_key_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("[x]").ok(),
        Some(("", ast::Op::EvalKey(
            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
}

#[test]
fn test_cast_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("as T").ok(),
        Some(("", ast::Op::CastOp(
            lexer::ast::Keyword("as", lexer::ast::KeywordKind::As),
            Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("T"))), vec![])),
        ))),
    );
}

#[test]
fn test_infix_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("* x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("*", lexer::ast::OpCodeKind::Star),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("/ x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("/", lexer::ast::OpCodeKind::Div),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("% x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("%", lexer::ast::OpCodeKind::Percent),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("+ x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("+", lexer::ast::OpCodeKind::Plus),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("- x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("-", lexer::ast::OpCodeKind::Minus),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("<< x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("<<", lexer::ast::OpCodeKind::LeftShift),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)(">> x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode(">>", lexer::ast::OpCodeKind::RightShift),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("< x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("<", lexer::ast::OpCodeKind::Lt),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("> x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode(">", lexer::ast::OpCodeKind::Gt),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("<= x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("<=", lexer::ast::OpCodeKind::Le),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)(">= x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode(">=", lexer::ast::OpCodeKind::Ge),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("== x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("==", lexer::ast::OpCodeKind::Eq),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("!= x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("!=", lexer::ast::OpCodeKind::Ne),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("& x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("&", lexer::ast::OpCodeKind::Amp),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("^ x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("^", lexer::ast::OpCodeKind::Caret),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("| x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("|", lexer::ast::OpCodeKind::Pipe),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("&& x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("&&", lexer::ast::OpCodeKind::And),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("|| x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("||", lexer::ast::OpCodeKind::Or),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("?? x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("??", lexer::ast::OpCodeKind::Coalescing),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("|> x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("|>", lexer::ast::OpCodeKind::RightPipeline),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("<| x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode("<|", lexer::ast::OpCodeKind::LeftPipeline),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
}

#[test]
fn test_assign_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("= x").ok(),
        Some(("", ast::Op::Assign(
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
}

#[test]
fn test_prefix_op_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("+x").ok(),
        Some(("", Rc::new(ast::Term::PrefixOp(
            lexer::ast::OpCode("+", lexer::ast::OpCodeKind::Plus),
            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        )))),
    );
    assert_eq!(
        parser::term(&context)("-123").ok(),
        Some(("", Rc::new(ast::Term::PrefixOp(
            lexer::ast::OpCode("-", lexer::ast::OpCodeKind::Minus),
            Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))),
        )))),
    );
    assert_eq!(
        parser::term(&context)("!false").ok(),
        Some(("", Rc::new(ast::Term::PrefixOp(
            lexer::ast::OpCode("!", lexer::ast::OpCodeKind::Bang),
            Rc::new(ast::Term::Literal(lexer::ast::Literal::Bool(lexer::ast::Keyword("false", lexer::ast::KeywordKind::False)))),
        )))),
    );
    assert_eq!(
        parser::term(&context)("~0xFFFF").ok(),
        Some(("", Rc::new(ast::Term::PrefixOp(
            lexer::ast::OpCode("~", lexer::ast::OpCodeKind::Tilde),
            Rc::new(ast::Term::Literal(lexer::ast::Literal::HexInteger("0xFFFF"))),
        )))),
    );
}

#[test]
fn test_block_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("{ f(); g(); x }").ok(),
        Some(("", Rc::new(ast::Term::Block(
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                Some(Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
            ),
        )))),
    );
    assert_eq!(
        parser::term(&context)("{ f(); g(); }").ok(),
        Some(("", Rc::new(ast::Term::Block(
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
        )))),
    );
}

#[test]
fn test_paren_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("(x)").ok(),
        Some(("", Rc::new(ast::Term::Paren(
            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        )))),
    );
}

#[test]
fn test_tuple_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("(1, 2, 3)").ok(),
        Some(("", Rc::new(ast::Term::Tuple(
            vec![
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("3"))), vec![])),
            ],
        )))),
    );
    assert_eq!(
        parser::term(&context)("(1, 2, 3,)").ok(),
        Some(("", Rc::new(ast::Term::Tuple(
            vec![
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("3"))), vec![])),
            ],
        )))),
    );
    assert_eq!(
        parser::term(&context)("(1,)").ok(),
        Some(("", Rc::new(ast::Term::Tuple(
            vec![
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![])),
            ],
        )))),
    );
}

#[test]
fn test_array_ctor_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("[]").ok(),
        Some(("", Rc::new(ast::Term::ArrayCtor(None)))),
    );
    assert_eq!(
        parser::term(&context)("[0..10]").ok(),
        Some(("", Rc::new(ast::Term::ArrayCtor(
            Some(ast::IterExpr::Range(
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
            )),
        )))),
    );
    assert_eq!(
        parser::term(&context)("[0..10..2]").ok(),
        Some(("", Rc::new(ast::Term::ArrayCtor(
            Some(ast::IterExpr::SteppedRange(
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
            )),
        )))),
    );
    assert_eq!(
        parser::term(&context)("[...x]").ok(),
        Some(("", Rc::new(ast::Term::ArrayCtor(
            Some(ast::IterExpr::Spread(
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
            )),
        )))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3]").ok(),
        Some(("", Rc::new(ast::Term::ArrayCtor(
            Some(ast::IterExpr::Elements(vec![
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("3"))), vec![])),
            ])),
        )))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3,]").ok(),
        Some(("", Rc::new(ast::Term::ArrayCtor(
            Some(ast::IterExpr::Elements(vec![
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("3"))), vec![])),
            ])),
        )))),
    );
}

#[test]
fn test_literal_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("()").ok(),
        Some((
            "",
            Rc::new(ast::Term::Literal(lexer::ast::Literal::Unit(
                lexer::ast::OpCode("(",lexer::ast::OpCodeKind::OpenParen),
                lexer::ast::OpCode(")", lexer::ast::OpCodeKind::CloseParen)
            )))
        )),
    );
    assert_eq!(
        parser::term(&context)("null").ok(),
        Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::Null(lexer::ast::Keyword("null", lexer::ast::KeywordKind::Null))))))
    );
    assert_eq!(
        parser::term(&context)("true").ok(),
        Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::Bool(lexer::ast::Keyword("true", lexer::ast::KeywordKind::True))))))
    );
    assert_eq!(
        parser::term(&context)("false").ok(),
        Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::Bool(lexer::ast::Keyword("false", lexer::ast::KeywordKind::False))))))
    );
    assert_eq!(parser::term(&context)("123.45").ok(), Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::RealNumber("123.45"))))));
    assert_eq!(parser::term(&context)("0x1AF").ok(), Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::HexInteger("0x1AF"))))));
    assert_eq!(parser::term(&context)("0b101").ok(), Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::BinInteger("0b101"))))));
    assert_eq!(parser::term(&context)("123").ok(), Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))))));
    assert_eq!(parser::term(&context)("'a'").ok(), Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::Character("a"))))));
    assert_eq!(parser::term(&context)("\"abc\"").ok(), Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::RegularString("abc"))))));
    assert_eq!(parser::term(&context)("@\"\\abc\"").ok(), Some(("", Rc::new(ast::Term::Literal(lexer::ast::Literal::VerbatiumString("\\abc"))))));
}

#[test]
fn test_this_literal_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("this").ok(),
        Some(("", Rc::new(ast::Term::ThisLiteral(lexer::ast::Literal::This(lexer::ast::Keyword("this", lexer::ast::KeywordKind::This))))))
    );
}

#[test]
fn test_interpolated_string_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("$\"abc{123}{x}def\"").ok(),
        Some(("", Rc::new(ast::Term::InterpolatedString(
            lexer::ast::InterpolatedString(
                vec![
                    "abc",
                    "",
                    "def",
                ],
                vec![
                    Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
                    Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
                ],
            )
        )))),
    );
}

#[test]
fn test_eval_var_term() {
    let context = Context::new();
    assert_eq!(parser::term(&context)("someVar").ok(), Some(("", Rc::new(ast::Term::EvalVar(lexer::ast::Ident("someVar"))))));
    assert_eq!(parser::term(&context)("some_var").ok(), Some(("", Rc::new(ast::Term::EvalVar(lexer::ast::Ident("some_var"))))));
}

#[test]
fn test_let_in_bind_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("let mut i: int = 123 in i + 1").ok(),
        Some(("", Rc::new(ast::Term::LetInBind(
            ast::VarBind(
                lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
                    lexer::ast::Ident("i"),
                    Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                ),
                Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
            ),
            lexer::ast::Keyword("in", lexer::ast::KeywordKind::In),
            Rc::new(ast::Expr(
                Rc::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode("+", lexer::ast::OpCodeKind::Plus),
                        Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))),
                    ),
                ],
            )),
        )))),
    );
}

#[test]
fn test_if_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); }").ok(),
        Some(("", Rc::new(ast::Term::If(
            lexer::ast::Keyword("if", lexer::ast::KeywordKind::If),
            Rc::new(ast::Expr(
                Rc::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode("==", lexer::ast::OpCodeKind::Eq),
                        Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                    ),
                ],
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
            None,
        )))),
    );
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); } else { g(); }").ok(),
        Some(("", Rc::new(ast::Term::If(
            lexer::ast::Keyword("if", lexer::ast::KeywordKind::If),
            Rc::new(ast::Expr(
                Rc::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode("==", lexer::ast::OpCodeKind::Eq),
                        Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                    ),
                ],
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
            Some((
                lexer::ast::Keyword("else", lexer::ast::KeywordKind::Else),
                ast::StatsBlock(
                    vec![
                        ast::Stat::Expr(
                            Rc::new(ast::Expr(
                                Rc::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                                vec![ast::Op::EvalFn(vec![])],
                            )),
                        ),
                    ],
                    None,
                ),
            )),
        )))),
    );
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); } else if j == 0 { g(); }").ok(),
        Some(("", Rc::new(ast::Term::If(
            lexer::ast::Keyword("if", lexer::ast::KeywordKind::If),
            Rc::new(ast::Expr(
                Rc::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode("==", lexer::ast::OpCodeKind::Eq),
                        Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                    ),
                ]
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
            Some((
                lexer::ast::Keyword("else", lexer::ast::KeywordKind::Else),
                ast::StatsBlock(
                    vec![],
                    Some(Rc::new(ast::Expr(
                        Rc::new(ast::Term::If(
                            lexer::ast::Keyword("if", lexer::ast::KeywordKind::If),
                            Rc::new(ast::Expr(
                                Rc::new(ast::Term::EvalVar(lexer::ast::Ident("j"))),
                                vec![
                                    ast::Op::InfixOp(
                                        lexer::ast::OpCode("==", lexer::ast::OpCodeKind::Eq),
                                        Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                                    ),
                                ],
                            )),
                            ast::StatsBlock(
                                vec![
                                    ast::Stat::Expr(
                                        Rc::new(ast::Expr(
                                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                                            vec![ast::Op::EvalFn(vec![])],
                                        )),
                                    ),
                                ],
                                None,
                            ),
                            None,
                        )),
                        vec![],
                    ))),
                ),
            )),
        )))),
    );
}

#[test]
fn test_while_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("while i == 0 { f(); }").ok(),
        Some(("", Rc::new(ast::Term::While(
            lexer::ast::Keyword("while", lexer::ast::KeywordKind::While),
            Rc::new(ast::Expr(
                Rc::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode("==", lexer::ast::OpCodeKind::Eq),
                        Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                    ),
                ],
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
        )))),
    );
}

#[test]
fn test_loop_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("loop { f(); }").ok(),
        Some(("", Rc::new(ast::Term::Loop(
            lexer::ast::Keyword("loop", lexer::ast::KeywordKind::Loop),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
        )))),
    );
}

#[test]
fn test_for_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("for let i <- 0..10 for let j <- 0..10..2 for k <- arr { f(); }").ok(),
        Some(("", Rc::new(ast::Term::For(
            vec![
                (
                    lexer::ast::Keyword("for", lexer::ast::KeywordKind::For),
                    ast::ForBind::Let(
                        lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("i"), None),
                        ast::ForIterExpr::Range(
                            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
                            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
                        ),
                    ),
                ),
                (
                    lexer::ast::Keyword("for", lexer::ast::KeywordKind::For),
                    ast::ForBind::Let(
                        lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("j"), None),
                        ast::ForIterExpr::SteppedRange(
                            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
                            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
                            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
                        ),
                    ),
                ),
                (
                    lexer::ast::Keyword("for", lexer::ast::KeywordKind::For),
                    ast::ForBind::Assign(
                        Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("k"))), vec![])),
                        ast::ForIterExpr::Spread(
                            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
                        ),
                    ),
                ),
            ],
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
        )))),
    );
}

#[test]
fn test_closure_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("|| 123").ok(),
        Some(("", Rc::new(ast::Term::Closure(
            ast::VarDecl::TupleDecl(vec![]),
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
        )))),
    );
    assert_eq!(
        parser::term(&context)("|x| x").ok(),
        Some(("", Rc::new(ast::Term::Closure(
            ast::VarDecl::TupleDecl(vec![
                ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
            ]),
            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        )))),
    );
    assert_eq!(
        parser::term(&context)("|x: int, y: int| { x + y }").ok(),
        Some(("", Rc::new(ast::Term::Closure(
            ast::VarDecl::TupleDecl(vec![
                ast::VarDecl::SingleDecl(
                    None,
                    lexer::ast::Ident("x"),
                    Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                ),
                ast::VarDecl::SingleDecl(
                    None,
                    lexer::ast::Ident("y"),
                    Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
                ),
            ]),
            Rc::new(ast::Expr(
                Rc::new(ast::Term::Block(
                    ast::StatsBlock(
                        vec![],
                        Some(Rc::new(ast::Expr(
                            Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
                            vec![
                                ast::Op::InfixOp(
                                    lexer::ast::OpCode("+", lexer::ast::OpCodeKind::Plus),
                                    Rc::new(ast::Term::EvalVar(lexer::ast::Ident("y"))),
                                ),
                            ],
                        )))
                    )
                )),
                vec![],
            )),
        )))),
    );
}

#[test]
fn test_range_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("0..10").ok(),
        Some(("", ast::IterExpr::Range(
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
        ))),
    );
}

#[test]
fn test_stepped_range_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("0..10..2").ok(),
        Some(("", ast::IterExpr::SteppedRange(
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
        ))),
    );
}

#[test]
fn test_spread_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("...arr").ok(),
        Some(("", ast::IterExpr::Spread(
            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
        ))),
    );
}

#[test]
fn test_elements_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("x, y").ok(),
        Some(("", ast::IterExpr::Elements(
            vec![
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![])),
            ]
        ))),
    );
    assert_eq!(
        parser::iter_expr(&context)("x, y,").ok(),
        Some(("", ast::IterExpr::Elements(
            vec![
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![])),
            ]
        ))),
    );
}

#[test]
fn test_arg_expr() {
    let context = Context::new();
    assert_eq!(
        parser::arg_expr(&context)("x").ok(),
        Some(("", ast::ArgExpr(
            None,
            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
    assert_eq!(
        parser::arg_expr(&context)("mut x").ok(),
        Some(("", ast::ArgExpr(
            Some(ast::MutAttr(lexer::ast::Keyword("mut", lexer::ast::KeywordKind::Mut))),
            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
}

#[test]
fn test_let_for_bind() {
    let context = Context::new();
    assert_eq!(
        parser::for_bind(&context)("let i: int <- arr").ok(),
        Some(("", ast::ForBind::Let(
            lexer::ast::Keyword("let", lexer::ast::KeywordKind::Let),
            ast::VarDecl::SingleDecl(
                None,
                lexer::ast::Ident("i"),
                Some(Rc::new(ast::TyExpr(Rc::new(ast::TyTerm::EvalTy(lexer::ast::Ident("int"))), vec![]))),
            ),
            ast::ForIterExpr::Spread(
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
            ),
        ))),
    );
}

#[test]
fn test_assign_for_bind() {
    let context = Context::new();
    assert_eq!(
        parser::for_bind(&context)("i <- arr").ok(),
        Some(("", ast::ForBind::Assign(
            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("i"))), vec![])),
            ast::ForIterExpr::Spread(
                Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
            ),
        ))),
    );
}

#[test]
fn test_range_for_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::for_iter_expr(&context)("0..10").ok(),
        Some(("", ast::ForIterExpr::Range(
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
        ))),
    );
}

#[test]
fn test_stepped_range_for_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::for_iter_expr(&context)("0..10..2").ok(),
        Some(("", ast::ForIterExpr::SteppedRange(
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
            Rc::new(ast::Expr(Rc::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
        ))),
    );
}

#[test]
fn test_spread_for_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::for_iter_expr(&context)("arr").ok(),
        Some(("", ast::ForIterExpr::Spread(
            Rc::new(ast::Expr(Rc::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
        ))),
    );
}
