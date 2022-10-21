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
        parser::target(&context)("pub let x = 123; pub fn f() {};"),
        Ok(("", ast::Target(
            Some(ast::Body(vec![
                ast::TopStat::VarBind(
                    Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
                    None,
                    ast::VarBind(
                        lexer::ast::Keyword::Let("let"),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
                        Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![])),
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
        parser::body(&context)("pub let x = 123; pub fn f() {};"),
        Ok(("", ast::Body(vec![
            ast::TopStat::VarBind(
                Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
                None,
                ast::VarBind(
                    lexer::ast::Keyword::Let("let"),
                    ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
                    Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![])),
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
        parser::top_stat(&context)("pub sync let mut x = 123;"),
        Ok(("", ast::TopStat::VarBind(
            Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
            Some(ast::SyncAttr(lexer::ast::Keyword::Sync("sync"))),
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                    lexer::ast::Ident("x"),
                    None,
                ),
                Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![])),
            ),
        ))),
    );
}

#[test]
fn test_fn_bind_top_stat() {
    let context = Context::new();
    assert_eq!(
        parser::top_stat(&context)("pub fn f(x: int) -> int { x };"),
        Ok(("", ast::TopStat::FnBind(
            Some(ast::AccessAttr(lexer::ast::Keyword::Pub("pub"))),
            ast::FnBind(
                lexer::ast::Keyword::Fn("fn"),
                ast::FnDecl(
                    lexer::ast::Ident("f"),
                    ast::VarDecl::TupleDecl(vec![
                        ast::VarDecl::SingleDecl(
                            None,
                            lexer::ast::Ident("x"),
                            Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                        ),
                    ]),
                    Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                ),
                ast::StatsBlock(
                    vec![],
                    Some(Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]))),
                ),
            ),
        ))),
    );
}

#[test]
fn test_stat_top_stat() {
    let context = Context::new();
    assert_eq!(
        parser::top_stat(&context)("f();"),
        Ok(("", ast::TopStat::Stat(
            ast::Stat::Expr(
                Box::new(ast::Expr(
                    ast::Term::EvalVar(lexer::ast::Ident("f")),
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
        parser::var_bind(&context)("let mut x: int = 123"),
        Ok(("", ast::VarBind(
            lexer::ast::Keyword::Let("let"),
            ast::VarDecl::SingleDecl(
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                lexer::ast::Ident("x"),
                Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
            ),
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![])),
        ))),
    );
}

#[test]
fn test_single_var_decl() {
    let context = Context::new();
    assert_eq!(
        parser::var_decl(&context)("x"),
        Ok(("", ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None))),
    );
    assert_eq!(
        parser::var_decl(&context)("mut x"),
        Ok(("", ast::VarDecl::SingleDecl(
            Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
            lexer::ast::Ident("x"),
            None,
        ))),
    );
    assert_eq!(
        parser::var_decl(&context)("mut x: int"),
        Ok(("", ast::VarDecl::SingleDecl(
            Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
            lexer::ast::Ident("x"),
            Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
        ))),
    );
}

#[test]
fn test_tuple_var_decl() {
    let context = Context::new();
    assert_eq!(
        parser::var_decl(&context)("()"),
        Ok(("", ast::VarDecl::TupleDecl(vec![]))),
    );
    assert_eq!(
        parser::var_decl(&context)("(x)"),
        Ok(("", ast::VarDecl::TupleDecl(vec![
            ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
        ]))),
    );
    assert_eq!(
        parser::var_decl(&context)("(mut x: int, y)"),
        Ok(("", ast::VarDecl::TupleDecl(vec![
            ast::VarDecl::SingleDecl(
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                lexer::ast::Ident("x"),
                Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
            ),
            ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
        ]))),
    );
    assert_eq!(
        parser::var_decl(&context)("(mut x: int, y,)"),
        Ok(("", ast::VarDecl::TupleDecl(vec![
            ast::VarDecl::SingleDecl(
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                lexer::ast::Ident("x"),
                Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
            ),
            ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
        ]))),
    );
}

#[test]
fn test_fn_bind() {
    let context = Context::new();
    assert_eq!(
        parser::fn_bind(&context)("fn f(mut x: int, y) -> int { g(); x }"),
        Ok(("", ast::FnBind(
            lexer::ast::Keyword::Fn("fn"),
            ast::FnDecl(
                lexer::ast::Ident("f"),
                ast::VarDecl::TupleDecl(vec![
                    ast::VarDecl::SingleDecl(
                        Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                        lexer::ast::Ident("x"),
                        Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                    ),
                    ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
                ]),
                Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
            ),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("g")),
                            vec![ast::Op::EvalFn(vec![])]
                        )),
                    ),
                ],
                Some(Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]))),
            ),
        ))),
    );
}

#[test]
fn test_fn_decl() {
    let context = Context::new();
    assert_eq!(
        parser::fn_decl(&context)("f()"),
        Ok(("", ast::FnDecl(
            lexer::ast::Ident("f"),
            ast::VarDecl::TupleDecl(vec![]),
            None,
        ))),
    );
    assert_eq!(
        parser::fn_decl(&context)("f(mut x: int, y) -> int"),
        Ok(("", ast::FnDecl(
            lexer::ast::Ident("f"),
            ast::VarDecl::TupleDecl(vec![
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                    lexer::ast::Ident("x"),
                    Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                ),
                ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
            ]),
            Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
        ))),
    );
}

#[test]
fn test_type_expr() {
    let context = Context::new();
    assert_eq!(
        parser::type_expr(&context)("T::U::V"),
        Ok(("", ast::TypeExpr(
            ast::TypeTerm::EvalType(lexer::ast::Ident("T")),
            vec![
                ast::TypeOp::Access(
                    lexer::ast::OpCode::DoubleColon("::"),
                    ast::TypeTerm::EvalType(lexer::ast::Ident("U")),
                ),
                ast::TypeOp::Access(
                    lexer::ast::OpCode::DoubleColon("::"),
                    ast::TypeTerm::EvalType(lexer::ast::Ident("V")),
                ),
            ],
        ))),
    );
}

#[test]
fn test_type_op() {
    let context = Context::new();
    assert_eq!(
        parser::type_op(&context)("::T"),
        Ok(("", ast::TypeOp::Access(
            lexer::ast::OpCode::DoubleColon("::"),
            ast::TypeTerm::EvalType(lexer::ast::Ident("T")),
        ))),
    );
}

#[test]
fn test_type_term() {
    let context = Context::new();
    assert_eq!(
        parser::type_term(&context)("string"),
        Ok(("", ast::TypeTerm::EvalType(lexer::ast::Ident("string")))),
    );
}

#[test]
fn test_stats_block() {
    let context = Context::new();
    assert_eq!(
        parser::stats_block(&context)("{}"),
        Ok(("", ast::StatsBlock(vec![], None))),
    );
    assert_eq!(
        parser::stats_block(&context)("{ f(); x }"),
        Ok(("", ast::StatsBlock(
            vec![
                ast::Stat::Expr(
                    Box::new(ast::Expr(
                        ast::Term::EvalVar(lexer::ast::Ident("f")),
                        vec![ast::Op::EvalFn(vec![])]
                    )),
                ),
            ],
            Some(Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]))),
        ))),
    );
    assert_eq!(
        parser::stats_block(&context)("{ f(); x; }"),
        Ok(("", ast::StatsBlock(
            vec![
                ast::Stat::Expr(
                    Box::new(ast::Expr(
                        ast::Term::EvalVar(lexer::ast::Ident("f")),
                        vec![ast::Op::EvalFn(vec![])]
                    )),
                ),
                ast::Stat::Expr(Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![])))
            ],
            None,
        ))),
    );
}

#[test]
fn test_return_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("return;"),
        Ok(("", ast::Stat::Return(lexer::ast::Keyword::Return("return"), None))),
    );
    assert_eq!(
        parser::stat(&context)("return x;"),
        Ok(("", ast::Stat::Return(
            lexer::ast::Keyword::Return("return"),
            Some(Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]))),
        ))),
    );
}

#[test]
fn test_continue_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("continue;"),
        Ok(("", ast::Stat::Continue(lexer::ast::Keyword::Continue("continue")))),
    );
}

#[test]
fn test_break_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("break;"),
        Ok(("", ast::Stat::Break(lexer::ast::Keyword::Break("break")))),
    );
}

#[test]
fn test_var_bind_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("let x = 123;"),
        Ok(("", ast::Stat::VarBind(
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::SingleDecl(
                    None,
                    lexer::ast::Ident("x"),
                    None,
                ),
                Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![])),
            ),
        ))),
    );
    assert_eq!(
        parser::stat(&context)("let mut x: int = 123;"),
        Ok(("", ast::Stat::VarBind(
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                    lexer::ast::Ident("x"),
                    Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                ),
                Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![])),
            ),
        ))),
    );
    assert_eq!(
        parser::stat(&context)("let (mut x: int, y) = (123, 456);"),
        Ok(("", ast::Stat::VarBind(
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::TupleDecl(
                    vec![
                        ast::VarDecl::SingleDecl(
                            Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                            lexer::ast::Ident("x"),
                            Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                        ),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("y"), None),
                    ],
                ),
                Box::new(ast::Expr(
                    ast::Term::Tuple(
                        vec![
                            ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![]),
                            ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("456")), vec![]),
                        ],
                    ),
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
        parser::stat(&context)("fn f(x: int) -> int { x };"),
        Ok(("", ast::Stat::FnBind(
            ast::FnBind(
                lexer::ast::Keyword::Fn("fn"),
                ast::FnDecl(
                    lexer::ast::Ident("f"),
                    ast::VarDecl::TupleDecl(vec![
                        ast::VarDecl::SingleDecl(
                            None,
                            lexer::ast::Ident("x"),
                            Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                        ),
                    ]),
                    Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                ),
                ast::StatsBlock(
                    vec![],
                    Some(Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]))),
                ),
            ),
        ))),
    );
}

#[test]
fn test_expr_stat() {
    let context = Context::new();
    assert_eq!(
        parser::stat(&context)("x = 123;"),
        Ok(("", ast::Stat::Expr(
            Box::new(ast::Expr(
                ast::Term::EvalVar(lexer::ast::Ident("x")),
                vec![
                    ast::Op::Assign(
                        ast::Term::Literal(lexer::ast::Literal::PureInteger("123")),
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
        parser::expr(&context)("x = T::f(1, 2).t + a.g(...b)[y]"),
        Ok(("", ast::Expr(
            ast::Term::EvalVar(lexer::ast::Ident("x")),
            vec![
                ast::Op::Assign(
                    ast::Term::EvalVar(lexer::ast::Ident("T")),
                ),
                ast::Op::TypeAccess(
                    lexer::ast::OpCode::DoubleColon("::"),
                    ast::Term::EvalVar(lexer::ast::Ident("f")),
                ),
                ast::Op::EvalFn(vec![
                    ast::ArgExpr(
                        None,
                        Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("1")), vec![])),
                    ),
                    ast::ArgExpr(
                        None,
                        Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("2")), vec![])),
                    )
                ]),
                ast::Op::Access(
                    lexer::ast::OpCode::Dot("."),
                    ast::Term::EvalVar(lexer::ast::Ident("t")),
                ),
                ast::Op::InfixOp(
                    lexer::ast::OpCode::Plus("+"),
                    ast::Term::EvalVar(lexer::ast::Ident("a")),
                ),
                ast::Op::Access(
                    lexer::ast::OpCode::Dot("."),
                    ast::Term::EvalVar(lexer::ast::Ident("g")),
                ),
                ast::Op::EvalSpreadFn(
                    Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("b")), vec![])),
                ),
                ast::Op::EvalKey(
                    Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("y")), vec![])),
                ),
            ]
        ))),
    );
}

#[test]
fn test_type_access_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("::x"),
        Ok(("", ast::Op::TypeAccess(
            lexer::ast::OpCode::DoubleColon("::"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
}

#[test]
fn test_access_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)(".x"),
        Ok(("", ast::Op::Access(
            lexer::ast::OpCode::Dot("."),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("?.x"),
        Ok(("", ast::Op::Access(
            lexer::ast::OpCode::CoalescingAccess("?."),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
}

#[test]
fn test_eval_fn_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("()"),
        Ok(("", ast::Op::EvalFn(vec![]))),
    );
    assert_eq!(
        parser::op(&context)("(mut x, y, z)"),
        Ok(("", ast::Op::EvalFn(vec![
            ast::ArgExpr(
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]))
            ),
            ast::ArgExpr(
                None,
                Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("y")), vec![]))
            ),
            ast::ArgExpr(
                None,
                Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("z")), vec![]))
            ),
        ]))),
    );
    assert_eq!(
        parser::op(&context)("(mut x, y, z,)"),
        Ok(("", ast::Op::EvalFn(vec![
            ast::ArgExpr(
                Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]))
            ),
            ast::ArgExpr(
                None,
                Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("y")), vec![]))
            ),
            ast::ArgExpr(
                None,
                Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("z")), vec![]))
            ),
        ]))),
    );
}

#[test]
fn test_eval_spread_fn_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("(...x)"),
        Ok(("", ast::Op::EvalSpreadFn(
            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![])),
        ))),
    );
}

#[test]
fn test_eval_key_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("[x]"),
        Ok(("", ast::Op::EvalKey(
            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![])),
        ))),
    );
}

#[test]
fn test_cast_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("as T"),
        Ok(("", ast::Op::CastOp(
            lexer::ast::Keyword::As("as"),
            Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("T")), vec![])),
        ))),
    );
}

#[test]
fn test_infix_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("* x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Star("*"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("/ x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Div("/"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("% x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Percent("%"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("+ x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Plus("+"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("- x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Minus("-"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("<< x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::LeftShift("<<"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)(">> x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::RightShift(">>"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("< x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Lt("<"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("> x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Gt(">"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("<= x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Le("<="),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)(">= x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Ge(">="),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("== x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Eq("=="),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("!= x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Ne("!="),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("& x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Amp("&"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("^ x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Caret("^"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("| x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Pipe("|"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("&& x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::And("&&"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("|| x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Or("||"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("?? x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::Coalescing("??"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("|> x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::RightPipeline("|>"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
    assert_eq!(
        parser::op(&context)("<| x"),
        Ok(("", ast::Op::InfixOp(
            lexer::ast::OpCode::LeftPipeline("<|"),
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
}

#[test]
fn test_assign_op() {
    let context = Context::new();
    assert_eq!(
        parser::op(&context)("= x"),
        Ok(("", ast::Op::Assign(
            ast::Term::EvalVar(lexer::ast::Ident("x")),
        ))),
    );
}

#[test]
fn test_prefix_op_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("+x"),
        Ok(("", ast::Term::PrefixOp(
            lexer::ast::OpCode::Plus("+"),
            Box::new(ast::Term::EvalVar(lexer::ast::Ident("x"))),
        ))),
    );
    assert_eq!(
        parser::term(&context)("-123"),
        Ok(("", ast::Term::PrefixOp(
            lexer::ast::OpCode::Minus("-"),
            Box::new(ast::Term::Literal(lexer::ast::Literal::PureInteger("123"))),
        ))),
    );
    assert_eq!(
        parser::term(&context)("!false"),
        Ok(("", ast::Term::PrefixOp(
            lexer::ast::OpCode::Bang("!"),
            Box::new(ast::Term::Literal(lexer::ast::Literal::Bool(lexer::ast::Keyword::False("false")))),
        ))),
    );
    assert_eq!(
        parser::term(&context)("~0xFFFF"),
        Ok(("", ast::Term::PrefixOp(
            lexer::ast::OpCode::Tilde("~"),
            Box::new(ast::Term::Literal(lexer::ast::Literal::HexInteger("0xFFFF"))),
        ))),
    );
}

#[test]
fn test_block_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("{ f(); g(); x }"),
        Ok(("", ast::Term::Block(
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("f")),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("g")),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                ],
                Some(Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]))),
            ),
        ))),
    );
    assert_eq!(
        parser::term(&context)("{ f(); g(); }"),
        Ok(("", ast::Term::Block(
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("f")),
                            vec![ast::Op::EvalFn(vec![])],
                        )),
                    ),
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("g")),
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
        parser::term(&context)("(x)"),
        Ok(("", ast::Term::Paren(
            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![])),
        ))),
    );
}

#[test]
fn test_tuple_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("(1, 2, 3)"),
        Ok(("", ast::Term::Tuple(
            vec![
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("1")), vec![]),
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("2")), vec![]),
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("3")), vec![]),
            ],
        ))),
    );
    assert_eq!(
        parser::term(&context)("(1, 2, 3,)"),
        Ok(("", ast::Term::Tuple(
            vec![
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("1")), vec![]),
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("2")), vec![]),
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("3")), vec![]),
            ],
        ))),
    );
    assert_eq!(
        parser::term(&context)("(1,)"),
        Ok(("", ast::Term::Tuple(
            vec![
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("1")), vec![]),
            ],
        ))),
    );
}

#[test]
fn test_array_ctor_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("[]"),
        Ok(("", ast::Term::ArrayCtor(None))),
    );
    assert_eq!(
        parser::term(&context)("[0..10]"),
        Ok(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::Range(
                Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("0")), vec![])),
                Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("10")), vec![])),
            )),
        ))),
    );
    assert_eq!(
        parser::term(&context)("[0..10..2]"),
        Ok(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::SteppedRange(
                Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("0")), vec![])),
                Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("10")), vec![])),
                Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("2")), vec![])),
            )),
        ))),
    );
    assert_eq!(
        parser::term(&context)("[...x]"),
        Ok(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::Spread(
                Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![])),
            )),
        ))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3]"),
        Ok(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::Elements(vec![
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("1")), vec![]),
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("2")), vec![]),
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("3")), vec![]),
            ])),
        ))),
    );
    assert_eq!(
        parser::term(&context)("[1, 2, 3,]"),
        Ok(("", ast::Term::ArrayCtor(
            Some(ast::IterExpr::Elements(vec![
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("1")), vec![]),
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("2")), vec![]),
                ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("3")), vec![]),
            ])),
        ))),
    );
}

#[test]
fn test_literal_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("()"),
        Ok(("", ast::Term::Literal(lexer::ast::Literal::Unit(lexer::ast::OpCode::OpenParen("("), lexer::ast::OpCode::CloseParen(")"))))),
    );
    assert_eq!(parser::term(&context)("null"), Ok(("", ast::Term::Literal(lexer::ast::Literal::Null(lexer::ast::Keyword::Null("null"))))));
    assert_eq!(parser::term(&context)("true"), Ok(("", ast::Term::Literal(lexer::ast::Literal::Bool(lexer::ast::Keyword::True("true"))))));
    assert_eq!(parser::term(&context)("false"), Ok(("", ast::Term::Literal(lexer::ast::Literal::Bool(lexer::ast::Keyword::False("false"))))));
    assert_eq!(parser::term(&context)("123.45"), Ok(("", ast::Term::Literal(lexer::ast::Literal::RealNumber("123.45")))));
    assert_eq!(parser::term(&context)("0x1AF"), Ok(("", ast::Term::Literal(lexer::ast::Literal::HexInteger("0x1AF")))));
    assert_eq!(parser::term(&context)("0b101"), Ok(("", ast::Term::Literal(lexer::ast::Literal::BinInteger("0b101")))));
    assert_eq!(parser::term(&context)("123"), Ok(("", ast::Term::Literal(lexer::ast::Literal::PureInteger("123")))));
    assert_eq!(parser::term(&context)("'a'"), Ok(("", ast::Term::Literal(lexer::ast::Literal::Character("a")))));
    assert_eq!(parser::term(&context)("\"abc\""), Ok(("", ast::Term::Literal(lexer::ast::Literal::RegularString("abc")))));
    assert_eq!(parser::term(&context)("@\"\\abc\""), Ok(("", ast::Term::Literal(lexer::ast::Literal::VerbatiumString("\\abc")))));
}

#[test]
fn test_this_literal_term() {
    let context = Context::new();
    assert_eq!(parser::term(&context)("this"), Ok(("", ast::Term::ThisLiteral(lexer::ast::Literal::This(lexer::ast::Keyword::This("this"))))));
}

#[test]
fn test_interpolated_string_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("$\"abc{123}{x}def\""),
        Ok(("", ast::Term::InterpolatedString(
            lexer::ast::InterpolatedString(
                vec![
                    "abc",
                    "",
                    "def",
                ],
                vec![
                    ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![]),
                    ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]),
                ],
            )
        ))),
    );
}

#[test]
fn test_eval_var_term() {
    let context = Context::new();
    assert_eq!(parser::term(&context)("someVar"), Ok(("", ast::Term::EvalVar(lexer::ast::Ident("someVar")))));
    assert_eq!(parser::term(&context)("some_var"), Ok(("", ast::Term::EvalVar(lexer::ast::Ident("some_var")))));
}

#[test]
fn test_let_in_bind_term() {
    let context = Context::new();
    assert_eq!(
        parser::term(&context)("let mut i: int = 123 in i + 1"),
        Ok(("", ast::Term::LetInBind(
            ast::VarBind(
                lexer::ast::Keyword::Let("let"),
                ast::VarDecl::SingleDecl(
                    Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
                    lexer::ast::Ident("i"),
                    Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                ),
                Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![])),
            ),
            lexer::ast::Keyword::In("in"),
            Box::new(ast::Expr(
                ast::Term::EvalVar(lexer::ast::Ident("i")),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Plus("+"),
                        ast::Term::Literal(lexer::ast::Literal::PureInteger("1")),
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
        parser::term(&context)("if i == 0 { f(); }"),
        Ok(("", ast::Term::If(
            lexer::ast::Keyword::If("if"),
            Box::new(ast::Expr(
                ast::Term::EvalVar(lexer::ast::Ident("i")),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Eq("=="),
                        ast::Term::Literal(lexer::ast::Literal::PureInteger("0")),
                    ),
                ],
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("f")),
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
        parser::term(&context)("if i == 0 { f(); } else { g(); }"),
        Ok(("", ast::Term::If(
            lexer::ast::Keyword::If("if"),
            Box::new(ast::Expr(
                ast::Term::EvalVar(lexer::ast::Ident("i")),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Eq("=="),
                        ast::Term::Literal(lexer::ast::Literal::PureInteger("0")),
                    ),
                ],
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("f")),
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
                                ast::Term::EvalVar(lexer::ast::Ident("g")),
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
        parser::term(&context)("if i == 0 { f(); } else if j == 0 { g(); }"),
        Ok(("", ast::Term::If(
            lexer::ast::Keyword::If("if"),
            Box::new(ast::Expr(
                ast::Term::EvalVar(lexer::ast::Ident("i")),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Eq("=="),
                        ast::Term::Literal(lexer::ast::Literal::PureInteger("0")),
                    ),
                ]
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("f")),
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
                        ast::Term::If(
                            lexer::ast::Keyword::If("if"),
                            Box::new(ast::Expr(
                                ast::Term::EvalVar(lexer::ast::Ident("j")),
                                vec![
                                    ast::Op::InfixOp(
                                        lexer::ast::OpCode::Eq("=="),
                                        ast::Term::Literal(lexer::ast::Literal::PureInteger("0")),
                                    ),
                                ],
                            )),
                            ast::StatsBlock(
                                vec![
                                    ast::Stat::Expr(
                                        Box::new(ast::Expr(
                                            ast::Term::EvalVar(lexer::ast::Ident("g")),
                                            vec![ast::Op::EvalFn(vec![])],
                                        )),
                                    ),
                                ],
                                None,
                            ),
                            None,
                        ),
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
        parser::term(&context)("while i == 0 { f(); }"),
        Ok(("", ast::Term::While(
            lexer::ast::Keyword::While("while"),
            Box::new(ast::Expr(
                ast::Term::EvalVar(lexer::ast::Ident("i")),
                vec![
                    ast::Op::InfixOp(
                        lexer::ast::OpCode::Eq("=="),
                        ast::Term::Literal(lexer::ast::Literal::PureInteger("0")),
                    ),
                ],
            )),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("f")),
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
        parser::term(&context)("loop { f(); }"),
        Ok(("", ast::Term::Loop(
            lexer::ast::Keyword::Loop("loop"),
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("f")),
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
        parser::term(&context)("for let i <- 0..10 for let j <- 0..10..2 for k <- arr { f(); }"),
        Ok(("", ast::Term::For(
            vec![
                (
                    lexer::ast::Keyword::For("for"),
                    ast::ForBind::Let(
                        lexer::ast::Keyword::Let("let"),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("i"), None),
                        ast::ForIterExpr::Range(
                            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("0")), vec![])),
                            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("10")), vec![])),
                        ),
                    ),
                ),
                (
                    lexer::ast::Keyword::For("for"),
                    ast::ForBind::Let(
                        lexer::ast::Keyword::Let("let"),
                        ast::VarDecl::SingleDecl(None, lexer::ast::Ident("j"), None),
                        ast::ForIterExpr::SteppedRange(
                            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("0")), vec![])),
                            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("10")), vec![])),
                            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("2")), vec![])),
                        ),
                    ),
                ),
                (
                    lexer::ast::Keyword::For("for"),
                    ast::ForBind::Assign(
                        Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("k")), vec![])),
                        ast::ForIterExpr::Spread(
                            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("arr")), vec![])),
                        ),
                    ),
                ),
            ],
            ast::StatsBlock(
                vec![
                    ast::Stat::Expr(
                        Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("f")),
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
        parser::term(&context)("|| 123"),
        Ok(("", ast::Term::Closure(
            ast::VarDecl::TupleDecl(vec![]),
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("123")), vec![])),
        ))),
    );
    assert_eq!(
        parser::term(&context)("|x| x"),
        Ok(("", ast::Term::Closure(
            ast::VarDecl::TupleDecl(vec![
                ast::VarDecl::SingleDecl(None, lexer::ast::Ident("x"), None),
            ]),
            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![])),
        ))),
    );
    assert_eq!(
        parser::term(&context)("|x: int, y: int| { x + y }"),
        Ok(("", ast::Term::Closure(
            ast::VarDecl::TupleDecl(vec![
                ast::VarDecl::SingleDecl(
                    None,
                    lexer::ast::Ident("x"),
                    Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                ),
                ast::VarDecl::SingleDecl(
                    None,
                    lexer::ast::Ident("y"),
                    Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
                ),
            ]),
            Box::new(ast::Expr(
                ast::Term::Block(
                    ast::StatsBlock(
                        vec![],
                        Some(Box::new(ast::Expr(
                            ast::Term::EvalVar(lexer::ast::Ident("x")),
                            vec![
                                ast::Op::InfixOp(
                                    lexer::ast::OpCode::Plus("+"),
                                    ast::Term::EvalVar(lexer::ast::Ident("y")),
                                ),
                            ],
                        )))
                    )
                ),
                vec![],
            )),
        ))),
    );
}

#[test]
fn test_range_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("0..10"),
        Ok(("", ast::IterExpr::Range(
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("0")), vec![])),
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("10")), vec![])),
        ))),
    );
}

#[test]
fn test_stepped_range_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("0..10..2"),
        Ok(("", ast::IterExpr::SteppedRange(
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("0")), vec![])),
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("10")), vec![])),
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("2")), vec![])),
        ))),
    );
}

#[test]
fn test_spread_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("...arr"),
        Ok(("", ast::IterExpr::Spread(
            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("arr")), vec![])),
        ))),
    );
}

#[test]
fn test_elements_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::iter_expr(&context)("x, y"),
        Ok(("", ast::IterExpr::Elements(
            vec![
                ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]),
                ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("y")), vec![]),
            ]
        ))),
    );
    assert_eq!(
        parser::iter_expr(&context)("x, y,"),
        Ok(("", ast::IterExpr::Elements(
            vec![
                ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![]),
                ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("y")), vec![]),
            ]
        ))),
    );
}

#[test]
fn test_arg_expr() {
    let context = Context::new();
    assert_eq!(
        parser::arg_expr(&context)("x"),
        Ok(("", ast::ArgExpr(
            None,
            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![])),
        ))),
    );
    assert_eq!(
        parser::arg_expr(&context)("mut x"),
        Ok(("", ast::ArgExpr(
            Some(ast::MutAttr(lexer::ast::Keyword::Mut("mut"))),
            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("x")), vec![])),
        ))),
    );
}

#[test]
fn test_let_for_bind() {
    let context = Context::new();
    assert_eq!(
        parser::for_bind(&context)("let i: int <- arr"),
        Ok(("", ast::ForBind::Let(
            lexer::ast::Keyword::Let("let"),
            ast::VarDecl::SingleDecl(
                None,
                lexer::ast::Ident("i"),
                Some(Box::new(ast::TypeExpr(ast::TypeTerm::EvalType(lexer::ast::Ident("int")), vec![]))),
            ),
            ast::ForIterExpr::Spread(
                Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("arr")), vec![])),
            ),
        ))),
    );
}

#[test]
fn test_assign_for_bind() {
    let context = Context::new();
    assert_eq!(
        parser::for_bind(&context)("i <- arr"),
        Ok(("", ast::ForBind::Assign(
            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("i")), vec![])),
            ast::ForIterExpr::Spread(
                Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("arr")), vec![])),
            ),
        ))),
    );
}

#[test]
fn test_range_for_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::for_iter_expr(&context)("0..10"),
        Ok(("", ast::ForIterExpr::Range(
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("0")), vec![])),
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("10")), vec![])),
        ))),
    );
}

#[test]
fn test_stepped_range_for_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::for_iter_expr(&context)("0..10..2"),
        Ok(("", ast::ForIterExpr::SteppedRange(
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("0")), vec![])),
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("10")), vec![])),
            Box::new(ast::Expr(ast::Term::Literal(lexer::ast::Literal::PureInteger("2")), vec![])),
        ))),
    );
}

#[test]
fn test_spread_for_iter_expr() {
    let context = Context::new();
    assert_eq!(
        parser::for_iter_expr(&context)("arr"),
        Ok(("", ast::ForIterExpr::Spread(
            Box::new(ast::Expr(ast::Term::EvalVar(lexer::ast::Ident("arr")), vec![])),
        ))),
    );
}
