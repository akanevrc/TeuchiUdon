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
                    Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
                    None,
                    ast::VarBind(
                        lexer::ast::Keyword::Let("let"),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
                        Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
                    ),
                ),
                ast::TopStat::FnBind(
                    Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
                    ast::FnBind(
                        lexer::ast::Keyword::Fn("fn"),
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
                Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
                None,
                ast::VarBind(
                    lexer::ast::Keyword::Let("let"),
                    ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
                    Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
                ),
            ),
            ast::TopStat::FnBind(
                Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
                ast::FnBind(
                    lexer::ast::Keyword::Fn("fn"),
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
            Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
            Some(ast::SyncAttr(lexer::ast::Keyword::Sync("sync"))),
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                    lexer::ast::Ident("x"),
                    None,
                ),
                Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
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
            Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
            ast::FnBind(
                lexer::ast::Keyword::Fn("fn"),
                ast::FnDecl(
                    lexer::ast::Ident("f"),
                    ast::VarDecl::TupleDecl(vec![
                        ast::VarDecl::SingleDecl(
                            None,
                            lexer::ast::Ident("x"),
                            Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                        ),
                    ]),
                    Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                ),
                ast::StatsBlock(
                    vec![],
                    Some(Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
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
                Box::new(ast::Expr(
                    Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
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
            lexer::ast::Keyword::Let("let"),
            ast::VarDecl::SingleDecl(
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                lexer::ast::Ident("x"),
                Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
            ),
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
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
            Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
            lexer::ast::Ident("x"),
            None,
        ))),
    );
    assert_eq!(
        parser::var_decl(&context)("mut x: int").ok(),
        Some(("", ast::VarDecl::SingleDecl(
            Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
            lexer::ast::Ident("x"),
            Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
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
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                lexer::ast::Ident("x"),
                Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
            ),
            ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
        ]))),
    );
    assert_eq!(
        parser::var_decl(&context)("(mut x: int, y,)").ok(),
        Some(("", ast::VarDecl::TupleDecl(vec![
            ast::VarDecl::SingleDecl(
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                lexer::ast::Ident("x"),
                Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
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
            lexer::ast::Keyword::Fn("fn"),
            ast::FnDecl(
                lexer::ast::Ident("f"),
                ast::VarDecl::TupleDecl(vec![
                    ast::VarDecl::SingleDecl(
                        Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                        lexer::ast::Ident("x"),
                        Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                    ),
                    ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
                ]),
                Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
            ),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                            vec![ast::Op::EvalFn(vec![])]
                        )),
                    ),
                ],
                Some(Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
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
                    Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                    lexer::ast::Ident("x"),
                    Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                ),
                ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
            ]),
            Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
        ))),
    );
}

#[test]
fn test_type_expr() {
    let context = Context::new();
    assert_eq!(
        parser::type_expr(&context)("T::U::V").ok(),
        Some(("", ast::TypeExpr(
            Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("T"))),
            vec![
                ast::TypeOp::Access(
                    lexer::ast::OpCode::DoubleColon("::"),
                    Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("U"))),
                ),
                ast::TypeOp::Access(
                    lexer::ast::OpCode::DoubleColon("::"),
                    Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("V"))),
                ),
            ],
        ))),
    );
}

#[test]
fn test_type_op() {
    let context = Context::new();
    assert_eq!(
        parser::type_op(&context)("::T").ok(),
        Some(("", ast::TypeOp::Access(
            lexer::ast::OpCode::DoubleColon("::"),
            Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("T"))),
        ))),
    );
}

#[test]
fn test_type_term() {
    let context = Context::new();
    assert_eq!(
        parser::type_term(&context)("string").ok(),
        Some(("", ast::TypeTerm::EvalType(lexer::ast::Ident("string")))),
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
                    Box::new(ast::Expr(
                        Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                        vec![ast::Op::EvalFn(vec![])]
                    )),
                ),
            ],
            Some(Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
        ))),
    );
    assert_eq!(
        parser::stats_block(&context)("{ f(); x; }").ok(),
        Some(("", ast::StatsBlock(
            vec![
                ast::Stat::Expr(
                    Box::new(ast::Expr(
                        Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                        vec![ast::Op::EvalFn(vec![])]
                    )),
                ),
                ast::Stat::Expr(Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])))
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
        Some(("", ast::Stat::Return(lexer::ast::Keyword::Return("return"), None))),
    );
    assert_eq!(
        parser::stat(&context)("return x;").ok(),
        Some(("", ast::Stat::Return(
            lexer::ast::Keyword::Return("return"),
            Some(Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
        ))),
    );
}

#[test]
fn test_continue_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("continue;").ok(),
        Some(("", ast::Stat::Continue(lexer::ast::Keyword::Continue("continue")))),
    );
}

#[test]
fn test_break_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("break;").ok(),
        Some(("", ast::Stat::Break(lexer::ast::Keyword::Break("break")))),
    );
}

#[test]
fn test_var_bind_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("let x = 123;").ok(),
        Some(("", ast::Stat::VarBind(
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::SingleDecl(
                    None,
                    lexer::ast::Ident("x"),
                    None,
                ),
                Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
            ),
        ))),
    );
    assert_eq!(
        parser::stat(&context)("let mut x: int = 123;").ok(),
        Some(("", ast::Stat::VarBind(
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                    lexer::ast::Ident("x"),
                    Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                ),
                Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
            ),
        ))),
    );
    assert_eq!(
        parser::stat(&context)("let (mut x: int, y) = (123, 456);").ok(),
        Some(("", ast::Stat::VarBind(
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::TupleDecl(
                    vec![
                        ast::VarDecl::SingleDecl(
                            Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                            lexer::ast::Ident("x"),
                            Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                        ),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
                    ],
                ),
                Box::new(ast::Expr(
                    Box::new(ast::Term::Tuple(
                        vec![
                            ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![]),
                            ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("456"))), vec![]),
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
                lexer::ast::Keyword::Fn("fn"),
                ast::FnDecl(
                    lexer::ast::Ident("f"),
                    ast::VarDecl::TupleDecl(vec![
                        ast::VarDecl::SingleDecl(
                            None,
                            lexer::ast::Ident("x"),
                            Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                        ),
                    ]),
                    Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                ),
                ast::StatsBlock(
                    vec![],
                    Some(Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
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
            Box::new(ast::Expr(
                Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
                vec![
                    ast::Op::Assign(
                        Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))),
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
        Some(("", ast::Expr(
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
            vec![
                ast::Op::Assign(
                    Box::new(ast::Term::EvalVar(lexer::ast::Ident("T"))),
                ),
                ast::Op::TypeAccess(
                    lexer::ast::OpCode::DoubleColon("::"),
                    Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                ),
                ast::Op::EvalFn(vec![
                    ast::ArgExpr(
                        None,
                        Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![])),
                    ),
                    ast::ArgExpr(
                        None,
                        Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
                    )
                ]),
                ast::Op::Access(
                    lexer::ast::OpCode::Dot("."),
                    Box::new(ast::Term::EvalVar(lexer::ast::Ident("t"))),
                ),
                ast::Op::InfixOp(
                    lexer::ast::OpCode::Plus("+"),
                    Box::new(ast::Term::EvalVar(lexer::ast::Ident("a"))),
                ),
                ast::Op::Access(
                    lexer::ast::OpCode::Dot("."),
                    Box::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                ),
                ast::Op::EvalSpreadFn(
                    Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("b"))), vec![])),
                ),
                ast::Op::EvalKey(
                    Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![])),
                ),
            ]
        ))),
    );
}

#[test]
fn test_type_access_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("::x").ok(),
        Some(("", ast::Op::TypeAccess(
            lexer::ast::OpCode::DoubleColon("::"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
}

#[test]
fn test_access_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)(".x").ok(),
        Some(("", ast::Op::Access(
            lexer::ast::OpCode::Dot("."),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("?.x").ok(),
        Some(("", ast::Op::Access(
            lexer::ast::OpCode::CoalescingAccess("?."),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
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
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))
            ),
            ast::ArgExpr(
                None,
                Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![]))
            ),
            ast::ArgExpr(
                None,
                Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("z"))), vec![]))
            ),
        ]))),
    );
    assert_eq!(
        parser::op(&context)("(mut x, y, z,)").ok(),
        Some(("", ast::Op::EvalFn(vec![
            ast::ArgExpr(
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))
            ),
            ast::ArgExpr(
                None,
                Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![]))
            ),
            ast::ArgExpr(
                None,
                Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("z"))), vec![]))
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
            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
}

#[test]
fn test_eval_key_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("[x]").ok(),
        Some(("", ast::Op::EvalKey(
            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
}

#[test]
fn test_cast_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("as T").ok(),
        Some(("", ast::Op::CastOp(
            lexer::ast::Keyword::As("as"),
            Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("T"))), vec![])),
        ))),
    );
}

#[test]
fn test_infix_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("* x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Star("*"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("/ x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Div("/"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("% x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Percent("%"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("+ x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Plus("+"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("- x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Minus("-"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("<< x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::LeftShift("<<"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)(">> x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::RightShift(">>"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("< x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Lt("<"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("> x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Gt(">"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("<= x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Le("<="),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)(">= x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Ge(">="),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("== x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Eq("=="),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("!= x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Ne("!="),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("& x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Amp("&"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("^ x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Caret("^"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("| x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Pipe("|"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("&& x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::And("&&"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("|| x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Or("||"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("?? x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Coalescing("??"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("|> x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::RightPipeline("|>"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::op(&context)("<| x").ok(),
        Some(("", ast::Op::InfixOp(
            lexer::ast::OpCode::LeftPipeline("<|"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
}

#[test]
fn test_assign_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("= x").ok(),
        Some(("", ast::Op::Assign(
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
}

#[test]
fn test_prefix_op_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("+x").ok(),
        Some(("", ast::Term::PrefixOp(
            lexer::ast::OpCode::Plus("+"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::term(&context)("-123").ok(),
        Some(("", ast::Term::PrefixOp(
            lexer::ast::OpCode::Minus("-"),
            Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))),
        ))),
    );
    assert_eq!(
        parser::term(&context)("!false").ok(),
        Some(("", ast::Term::PrefixOp(
            lexer::ast::OpCode::Bang("!"),
            Box::new(ast::Term::Literal(lexer::ast::Literal::Bool(lexer::ast::Keyword::False("false")))),
        ))),
    );
    assert_eq!(
        parser::term(&context)("~0xFFFF").ok(),
        Some(("", ast::Term::PrefixOp(
            lexer::ast::OpCode::Tilde("~"),
            Box::new(ast::Term::Literal(lexer::ast::Literal::HexInteger("0xFFFF"))),
        ))),
    );
}

#[test]
fn test_block_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("{ f(); g(); x }").ok(),
        Some(("", ast::Term::Block(
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                Some(Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]))),
            ),
        ))),
    );
    assert_eq!(
        parser::term(&context)("{ f(); g(); }").ok(),
        Some(("", ast::Term::Block(
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
        ))),
    );
}

#[test]
fn test_paren_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("(x)").ok(),
        Some(("", ast::Term::Paren(
            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
}

#[test]
fn test_tuple_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("(1, 2, 3)").ok(),
        Some(("", ast::Term::Tuple(
            vec![
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![]),
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![]),
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("3"))), vec![]),
            ],
        ))),
    );
    assert_eq!(
        parser::term(&context)("(1, 2, 3,)").ok(),
        Some(("", ast::Term::Tuple(
            vec![
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![]),
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![]),
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("3"))), vec![]),
            ],
        ))),
    );
    assert_eq!(
        parser::term(&context)("(1,)").ok(),
        Some(("", ast::Term::Tuple(
            vec![
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![]),
            ],
        ))),
    );
}

#[test]
fn test_array_ctor_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("[]").ok(),
        Some(("", ast::Term::ArrayCtor(None))),
    );
    assert_eq!(
        parser::term(&context)("[0..10]").ok(),
        Some(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::Range(
                Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
                Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
            )),
        ))),
    );
    assert_eq!(
        parser::term(&context)("[0..10..2]").ok(),
        Some(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::SteppedRange(
                Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
                Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
                Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
            )),
        ))),
    );
    assert_eq!(
        parser::term(&context)("[...x]").ok(),
        Some(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::Spread(
                Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
            )),
        ))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3]").ok(),
        Some(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::Elements(vec![
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![]),
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![]),
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("3"))), vec![]),
            ])),
        ))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3,]").ok(),
        Some(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::Elements(vec![
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))), vec![]),
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![]),
                ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("3"))), vec![]),
            ])),
        ))),
    );
}

#[test]
fn test_literal_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("()").ok(),
        Some(("", ast::Term::Literal(lexer::ast::Literal::Unit(lexer::ast::OpCode::OpenParen("("), lexer::ast::OpCode::CloseParen(")"))))),
    );
    assert_eq!(parser::term(&context)("null").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::Null(lexer::ast::Keyword::Null("null"))))));
    assert_eq!(parser::term(&context)("true").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::Bool(lexer::ast::Keyword::True("true"))))));
    assert_eq!(parser::term(&context)("false").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::Bool(lexer::ast::Keyword::False("false"))))));
    assert_eq!(parser::term(&context)("123.45").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::RealNumber("123.45")))));
    assert_eq!(parser::term(&context)("0x1AF").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::HexInteger("0x1AF")))));
    assert_eq!(parser::term(&context)("0b101").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::BinInteger("0b101")))));
    assert_eq!(parser::term(&context)("123").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::PureInteger("123")))));
    assert_eq!(parser::term(&context)("'a'").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::Character("a")))));
    assert_eq!(parser::term(&context)("\"abc\"").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::RegularString("abc")))));
    assert_eq!(parser::term(&context)("@\"\\abc\"").ok(), Some(("", ast::Term::Literal(lexer::ast::Literal::VerbatiumString("\\abc")))));
}

#[test]
fn test_this_literal_term() {
    let context = Context::new();
    assert_eq!(parser::term(&context)("this").ok(), Some(("", ast::Term::ThisLiteral(lexer::ast::Literal::This(lexer::ast::Keyword::This("this"))))));
}

#[test]
fn test_interpolated_string_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("$\"abc{123}{x}def\"").ok(),
        Some(("", ast::Term::InterpolatedString(
            lexer::ast::InterpolatedString(
                vec![
                    "abc",
                    "",
                    "def",
                ],
                vec![
                    ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![]),
                    ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]),
                ],
            )
        ))),
    );
}

#[test]
fn test_eval_var_term() {
    let context = Context::new();
    assert_eq!(parser::term(&context)("someVar").ok(), Some(("", ast::Term::EvalVar(lexer::ast::Ident("someVar")))));
    assert_eq!(parser::term(&context)("some_var").ok(), Some(("", ast::Term::EvalVar(lexer::ast::Ident("some_var")))));
}

#[test]
fn test_let_in_bind_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("let mut i: int = 123 in i + 1").ok(),
        Some(("", ast::Term::LetInBind(
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                    lexer::ast::Ident("i"),
                    Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                ),
                Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
            ),
            lexer::ast::Keyword::In("in"),
            Box::new(ast::Expr(
                Box::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Plus("+"),
                        Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("1"))),
                    ),
                ],
            )),
        ))),
    );
}

#[test]
fn test_if_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); }").ok(),
        Some(("", ast::Term::If(
            lexer::ast::Keyword::If("if"),
            Box::new(ast::Expr(
                Box::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Eq("=="),
                        Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                    ),
                ],
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
            None,
        ))),
    );
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); } else { g(); }").ok(),
        Some(("", ast::Term::If(
            lexer::ast::Keyword::If("if"),
            Box::new(ast::Expr(
                Box::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Eq("=="),
                        Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                    ),
                ],
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
            Some((
                lexer::ast::Keyword::Else("else"),
                ast::StatsBlock(
                    vec![
                        ast::Stat::Expr(
                            Box::new(ast::Expr(
                                Box::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
                                vec![ast::Op::EvalFn(vec![])],
                            )),
                        ),
                    ],
                    None,
                ),
            )),
        ))),
    );
    assert_eq!(
        parser::term(&context)("if i == 0 { f(); } else if j == 0 { g(); }").ok(),
        Some(("", ast::Term::If(
            lexer::ast::Keyword::If("if"),
            Box::new(ast::Expr(
                Box::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Eq("=="),
                        Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                    ),
                ]
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
            Some((
                lexer::ast::Keyword::Else("else"),
                ast::StatsBlock(
                    vec![],
                    Some(Box::new(ast::Expr(
                        Box::new(ast::Term::If(
                            lexer::ast::Keyword::If("if"),
                            Box::new(ast::Expr(
                                Box::new(ast::Term::EvalVar(lexer::ast::Ident("j"))),
                                vec![
                                    ast::Op::InfixOp(
                                        lexer::ast::OpCode::Eq("=="),
                                        Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                                    ),
                                ],
                            )),
                            ast::StatsBlock(
                                vec![
                                    ast::Stat::Expr(
                                        Box::new(ast::Expr(
                                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("g"))),
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
        ))),
    );
}

#[test]
fn test_while_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("while i == 0 { f(); }").ok(),
        Some(("", ast::Term::While(
            lexer::ast::Keyword::While("while"),
            Box::new(ast::Expr(
                Box::new(ast::Term::EvalVar(lexer::ast::Ident("i"))),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Eq("=="),
                        Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))),
                    ),
                ],
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
        ))),
    );
}

#[test]
fn test_loop_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("loop { f(); }").ok(),
        Some(("", ast::Term::Loop(
            lexer::ast::Keyword::Loop("loop"),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
        ))),
    );
}

#[test]
fn test_for_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("for let i <- 0..10 for let j <- 0..10..2 for k <- arr { f(); }").ok(),
        Some(("", ast::Term::For(
            vec![
                (
                    lexer::ast::Keyword::For("for"),
                    ast::ForBind::Let(
                        lexer::ast::Keyword::Let("let"),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("i"), None),
                        ast::ForIterExpr::Range(
                            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
                            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
                        ),
                    ),
                ),
                (
                    lexer::ast::Keyword::For("for"),
                    ast::ForBind::Let(
                        lexer::ast::Keyword::Let("let"),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("j"), None),
                        ast::ForIterExpr::SteppedRange(
                            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
                            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
                            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
                        ),
                    ),
                ),
                (
                    lexer::ast::Keyword::For("for"),
                    ast::ForBind::Assign(
                        Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("k"))), vec![])),
                        ast::ForIterExpr::Spread(
                            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
                        ),
                    ),
                ),
            ],
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("f"))),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                None,
            ),
        ))),
    );
}

#[test]
fn test_closure_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("|| 123").ok(),
        Some(("", ast::Term::Closure(
            ast::VarDecl::TupleDecl(vec![]),
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))), vec![])),
        ))),
    );
    assert_eq!(
        parser::term(&context)("|x| x").ok(),
        Some(("", ast::Term::Closure(
            ast::VarDecl::TupleDecl(vec![
                ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
            ]),
            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
    assert_eq!(
        parser::term(&context)("|x: int, y: int| { x + y }").ok(),
        Some(("", ast::Term::Closure(
            ast::VarDecl::TupleDecl(vec![
                ast::VarDecl::SingleDecl(
                    None,
                    lexer::ast::Ident("x"),
                    Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                ),
                ast::VarDecl::SingleDecl(
                    None,
                    lexer::ast::Ident("y"),
                    Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
                ),
            ]),
            Box::new(ast::Expr(
                Box::new(ast::Term::Block(
                    ast::StatsBlock(
                        vec![],
                        Some(Box::new(ast::Expr(
                            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
                            vec![
                                ast::Op::InfixOp(
                                    lexer::ast::OpCode::Plus("+"),
                                    Box::new(ast::Term::EvalVar(lexer::ast::Ident("y"))),
                                ),
                            ],
                        )))
                    )
                )),
                vec![],
            )),
        ))),
    );
}

#[test]
fn test_range_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("0..10").ok(),
        Some(("", ast::IterExpr::Range(
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
        ))),
    );
}

#[test]
fn test_stepped_range_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("0..10..2").ok(),
        Some(("", ast::IterExpr::SteppedRange(
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
        ))),
    );
}

#[test]
fn test_spread_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("...arr").ok(),
        Some(("", ast::IterExpr::Spread(
            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
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
                ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]),
                ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![]),
            ]
        ))),
    );
    assert_eq!(
        parser::iter_expr(&context)("x, y,").ok(),
        Some(("", ast::IterExpr::Elements(
            vec![
                ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![]),
                ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("y"))), vec![]),
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
            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
    assert_eq!(
        parser::arg_expr(&context)("mut x").ok(),
        Some(("", ast::ArgExpr(
            Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))), vec![])),
        ))),
    );
}

#[test]
fn test_let_for_bind() {
    let context = Context::new();
    assert_eq!(
        parser::for_bind(&context)("let i: int <- arr").ok(),
        Some(("", ast::ForBind::Let(
            lexer::ast::Keyword::Let("let"),
            ast::VarDecl::SingleDecl(
                None,
                lexer::ast::Ident("i"),
                Some(Box::new(ast::TypeExpr(Box::new(ast::TypeTerm::EvalType(lexer::ast::Ident("int"))), vec![]))),
            ),
            ast::ForIterExpr::Spread(
                Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
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
            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("i"))), vec![])),
            ast::ForIterExpr::Spread(
                Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
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
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
        ))),
    );
}

#[test]
fn test_stepped_range_for_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::for_iter_expr(&context)("0..10..2").ok(),
        Some(("", ast::ForIterExpr::SteppedRange(
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("0"))), vec![])),
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("10"))), vec![])),
            Box::new(ast::Expr(Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("2"))), vec![])),
        ))),
    );
}

#[test]
fn test_spread_for_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::for_iter_expr(&context)("arr").ok(),
        Some(("", ast::ForIterExpr::Spread(
            Box::new(ast::Expr(Box::new(ast::Term::EvalVar(lexer::ast::Ident("arr"))), vec![])),
        ))),
    );
}
