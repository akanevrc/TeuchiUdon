use crate::lexer;

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Target(pub Option<Body>);

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Body(pub Vec<TopStat>);

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum TopStat {
    VarBind(Option<AccessAttr>, Option<SyncAttr>, VarBind),
    FnBind(Option<AccessAttr>, FnBind),
    Stat(Stat),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct AccessAttr(pub lexer::ast::Keyword);

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct SyncAttr(pub lexer::ast::Keyword);

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct VarBind(pub lexer::ast::Keyword, pub Option<MutAttr>, pub VarDecl, pub Box<Expr>);

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct MutAttr(pub lexer::ast::Keyword);

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum VarDecl {
    SingleDecl(VarDeclPart),
    TupleDecl(Vec<VarDecl>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct VarDeclPart(pub lexer::ast::Ident, pub Option<Box<TypeExpr>>);

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct FnBind(pub lexer::ast::Keyword, pub FnDecl, pub StatsBlock);

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct FnDecl(pub lexer::ast::Ident, pub VarDecl, pub Option<Box<TypeExpr>>);

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct TypeExpr(pub TypeTerm, pub Vec<TypeOp>);

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum TypeOp {
    Access(lexer::ast::OpCode, TypeTerm),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum TypeTerm {
    EvalType(lexer::ast::Ident),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Stat {
    ImplicitReturn(Box<Expr>),
    Return(lexer::ast::Keyword, Option<Box<Expr>>),
    Continue(lexer::ast::Keyword),
    Break(lexer::ast::Keyword),
    VarBind(VarBind),
    Expr(Box<Expr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct StatsBlock(pub Vec<Stat>, pub Option<Box<Expr>>);

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct Expr(pub Term, pub Vec<Op>);

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Op {
    TypeAccess(lexer::ast::OpCode, Term),
    Access(lexer::ast::OpCode, Term),
    EvalFn(Vec<ArgExpr>),
    EvalSpreadFn(Box<Expr>),
    EvalKey(Box<Expr>),
    CastOp(lexer::ast::Keyword, Box<TypeExpr>),
    InfixOp(lexer::ast::OpCode, Term),
    Assign(Term),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Term {
    PrefixOp(lexer::ast::OpCode, Box<Term>),
    Block(StatsBlock),
    Paren(Box<Expr>),
    Tuple(Vec<Expr>),
    ArrayCtor(Option<IterExpr>),
    Literal(lexer::ast::Literal),
    ThisLiteral(lexer::ast::Literal),
    InterpolatedString(lexer::ast::InterpolatedString),
    EvalVar(lexer::ast::Ident),
    LetInBind(VarBind, lexer::ast::Keyword, Box<Expr>),
    If(lexer::ast::Keyword, Box<Expr>, StatsBlock, Option<(lexer::ast::Keyword, StatsBlock)>),
    While(lexer::ast::Keyword, Box<Expr>, StatsBlock),
    Loop(lexer::ast::Keyword, StatsBlock),
    For(Vec<(lexer::ast::Keyword, ForBind)>, StatsBlock),
    Closure(VarDecl, Box<Expr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum IterExpr {
    Range(Box<Expr>, Box<Expr>),
    SteppedRange(Box<Expr>, Box<Expr>, Box<Expr>),
    Spread(Box<Expr>),
    Elements(Vec<Expr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub struct ArgExpr(pub Option<MutAttr>, pub Box<Expr>);

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum ForBind {
    Let(lexer::ast::Keyword, VarDecl, ForIterExpr),
    Assign(Box<Expr>, ForIterExpr),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum ForIterExpr {
    Range(Box<Expr>, Box<Expr>),
    SteppedRange(Box<Expr>, Box<Expr>, Box<Expr>),
    Spread(Box<Expr>),
}
