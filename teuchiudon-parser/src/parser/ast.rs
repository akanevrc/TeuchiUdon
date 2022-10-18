use crate::lexer as lexer;

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Target {
    Body(Option<Body>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Body {
    Stats(Vec<TopStat>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum TopStat {
    VarBind(Option<AccessAttr>, Option<SyncAttr>, VarBind),
    FnBind(Option<AccessAttr>, FnBind),
    Stat(Stat),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum AccessAttr {
    Attr(lexer::ast::Keyword),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum SyncAttr {
    Attr(lexer::ast::Keyword),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum VarBind {
    Bind(lexer::ast::Keyword, Option<MutAttr>, VarDecl, Expr),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum MutAttr {
    Attr(lexer::ast::Keyword),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum VarDecl {
    SingleDecl(VarDeclPart),
    TupleDecl(Vec<VarDecl>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum VarDeclPart {
    Part(Ident, Option<TypeExpr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum FnBind {
    Bind(lexer::ast::Keyword, FnDecl, StatsBlock),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum FnDecl {
    Decl(Ident, VarDecl, Option<TypeExpr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Ident {
    Ident(lexer::ast::Ident),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum TypeExpr {
    Expr(TypeTerm, Option<TypeOp>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum TypeOp {
    Access(lexer::ast::OpCode, Box<TypeExpr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum TypeTerm {
    EvalType(Ident),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Stat {
    ImplicitReturn(Expr),
    Return(lexer::ast::Keyword, Option<Expr>),
    Continue(lexer::ast::Keyword),
    Break(lexer::ast::Keyword),
    VarBind(VarBind),
    Expr(Expr),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum StatsBlock {
    Block(Vec<Stat>, Option<Box<Expr>>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Expr {
    Expr(Term, Option<Op>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Op {
    TypeAccess(lexer::ast::OpCode, Box<Expr>),
    Access(lexer::ast::OpCode, Box<Expr>),
    EvalFn(Vec<ArgExpr>),
    EvalSpreadFn(Box<Expr>),
    EvalKey(Box<Expr>),
    CastOp(lexer::ast::Keyword, TypeExpr),
    InfixOp(lexer::ast::OpCode, Box<Expr>),
    Assign(Box<Expr>),
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
    EvalVar(Ident),
    LetInBind(Box<VarBind>, lexer::ast::Keyword, Box<Expr>),
    If(lexer::ast::Keyword, Box<Expr>, StatsBlock, Option<(lexer::ast::Keyword, StatsBlock)>),
    While(lexer::ast::Keyword, Box<Expr>, StatsBlock),
    Loop(lexer::ast::Keyword, StatsBlock),
    For(Vec<(lexer::ast::Keyword, ForBind)>, StatsBlock),
    Closure(VarDecl, Box<Expr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum IterExpr {
    Elements(Vec<Expr>),
    Range(Box<Expr>, Box<Expr>),
    SteppedRange(Box<Expr>, Box<Expr>, Box<Expr>),
    Spread(Box<Expr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum ArgExpr {
    Expr(Option<MutAttr>, Expr),
}

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
