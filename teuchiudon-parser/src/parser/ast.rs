use crate::lexer as lexer;

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Target {
    Empty,
    Body(Body),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Body {
    Stats(Vec<TopStat>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum TopStat {
    VarBind(AccessAttr, SyncAttr, VarBind),
    FnBind(AccessAttr, FnBind),
    Stat(Stat),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum AccessAttr {
    None,
    Attr(lexer::ast::Keyword),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum SyncAttr {
    None,
    Attr(lexer::ast::Keyword),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum VarBind {
    Bind(lexer::ast::Keyword, MutAttr, VarDecl, Expr),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum MutAttr {
    None,
    Attr(lexer::ast::Keyword),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum VarDecl {
    SingleDecl(VarDeclPart),
    TupleDecl(Vec<VarDecl>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum VarDeclPart {
    Part(Ident, TypeExpr),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum FnBind {
    Bind(lexer::ast::Keyword, FnDecl, StatsBlock),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum FnDecl {
    Decl(Ident, VarDecl, TypeExpr),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Ident {
    Ident(lexer::ast::Ident),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum Stat {
    ImplicitReturn(Expr),
    Return(lexer::ast::Keyword, Expr),
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
    Block(StatsBlock),
    Paren(Box<Expr>),
    Tuple(Vec<Expr>),
    ArrayCtor(IterExpr),
    Literal(lexer::ast::Literal),
    ThisLiteral(lexer::ast::Literal),
    InterpolatedString(lexer::ast::InterpolatedString),
    TypeAccess(TypeExpr, lexer::ast::OpCode, Box<Expr>),
    Access(Box<Expr>, lexer::ast::OpCode, Box<Expr>),
    Cast(Box<Expr>, lexer::ast::Keyword, TypeExpr),
    EvalFunc(Box<Expr>, Vec<ArgExpr>),
    EvalSpreadFunc(Box<Expr>, Box<Expr>),
    EvalKey(Box<Expr>, Box<Expr>),
    PrefixOp(lexer::ast::OpCode, Box<Expr>),
    InfixOp(Box<Expr>, lexer::ast::OpCode, Box<Expr>),
    Assign(Box<Expr>, Box<Expr>),
    EvalVar(Ident),
    LetInBind(Box<VarBind>, lexer::ast::Keyword, Box<Expr>),
    If(lexer::ast::Keyword, Box<Expr>, StatsBlock, Option<(lexer::ast::Keyword, StatsBlock)>),
    While(lexer::ast::Keyword, Box<Expr>, StatsBlock),
    Loop(lexer::ast::Keyword, StatsBlock),
    For(Vec<lexer::ast::Keyword>, Vec<ForBind>, StatsBlock),
    Closure(VarDecl, Box<Expr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum TypeExpr {
    None,
    EvalType(Ident),
    TypeAccess(Box<TypeExpr>, lexer::ast::OpCode, Box<TypeExpr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum IterExpr {
    None,
    Elements(Vec<Expr>),
    Range(Box<Expr>, Box<Expr>),
    SteppedRange(Box<Expr>, Box<Expr>, Box<Expr>),
    Spread(Box<Expr>),
}

#[derive(Clone, Debug, Eq, PartialEq)]
pub enum ArgExpr {
    Expr(MutAttr, Expr),
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
