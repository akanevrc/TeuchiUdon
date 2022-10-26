use std::rc::Rc;
use crate::lexer;

#[derive(Clone, Debug, PartialEq)]
pub struct Target<'input>(pub Option<Body<'input>>);

#[derive(Clone, Debug, PartialEq)]
pub struct Body<'input>(pub Vec<TopStat<'input>>);

#[derive(Clone, Debug, PartialEq)]
pub enum TopStat<'input> {
    VarBind(Option<AccessAttr<'input>>, Option<SyncAttr<'input>>, VarBind<'input>),
    FnBind(Option<AccessAttr<'input>>, FnBind<'input>),
    Stat(Stat<'input>),
}

#[derive(Clone, Debug, PartialEq)]
pub struct AccessAttr<'input>(pub lexer::ast::Keyword<'input>);

#[derive(Clone, Debug, PartialEq)]
pub struct SyncAttr<'input>(pub lexer::ast::Keyword<'input>);

#[derive(Clone, Debug, PartialEq)]
pub struct VarBind<'input>(pub lexer::ast::Keyword<'input>, pub VarDecl<'input>, pub Rc<Expr<'input>>);

#[derive(Clone, Debug, PartialEq)]
pub enum VarDecl<'input> {
    SingleDecl(Option<MutAttr<'input>>, lexer::ast::Ident<'input>, Option<Rc<TypeExpr<'input>>>),
    TupleDecl(Vec<VarDecl<'input>>),
}

#[derive(Clone, Debug, PartialEq)]
pub struct MutAttr<'input>(pub lexer::ast::Keyword<'input>);

#[derive(Clone, Debug, PartialEq)]
pub struct FnBind<'input>(pub lexer::ast::Keyword<'input>, pub FnDecl<'input>, pub StatsBlock<'input>);

#[derive(Clone, Debug, PartialEq)]
pub struct FnDecl<'input>(pub lexer::ast::Ident<'input>, pub VarDecl<'input>, pub Option<Rc<TypeExpr<'input>>>);

#[derive(Clone, Debug, PartialEq)]
pub struct TypeExpr<'input>(pub Rc<TypeTerm<'input>>, pub Vec<TypeOp<'input>>);

#[derive(Clone, Debug, PartialEq)]
pub enum TypeOp<'input> {
    Access(lexer::ast::OpCode<'input>, Rc<TypeTerm<'input>>),
}

#[derive(Clone, Debug, PartialEq)]
pub enum TypeTerm<'input> {
    EvalType(lexer::ast::Ident<'input>),
}

#[derive(Clone, Debug, PartialEq)]
pub struct StatsBlock<'input>(pub Vec<Stat<'input>>, pub Option<Rc<Expr<'input>>>);

#[derive(Clone, Debug, PartialEq)]
pub enum Stat<'input> {
    Return(lexer::ast::Keyword<'input>, Option<Rc<Expr<'input>>>),
    Continue(lexer::ast::Keyword<'input>),
    Break(lexer::ast::Keyword<'input>),
    VarBind(VarBind<'input>),
    FnBind(FnBind<'input>),
    Expr(Rc<Expr<'input>>),
}

#[derive(Clone, Debug, PartialEq)]
pub struct Expr<'input>(pub Rc<Term<'input>>, pub Vec<Op<'input>>);

#[derive(Clone, Debug, PartialEq)]
pub enum Op<'input> {
    TypeAccess(lexer::ast::OpCode<'input>, Rc<Term<'input>>),
    Access(lexer::ast::OpCode<'input>, Rc<Term<'input>>),
    EvalFn(Vec<ArgExpr<'input>>),
    EvalSpreadFn(Rc<Expr<'input>>),
    EvalKey(Rc<Expr<'input>>),
    CastOp(lexer::ast::Keyword<'input>, Rc<TypeExpr<'input>>),
    InfixOp(lexer::ast::OpCode<'input>, Rc<Term<'input>>),
    Assign(Rc<Term<'input>>),
}

#[derive(Clone, Debug, PartialEq)]
pub enum Term<'input> {
    PrefixOp(lexer::ast::OpCode<'input>, Rc<Term<'input>>),
    Block(StatsBlock<'input>),
    Paren(Rc<Expr<'input>>),
    Tuple(Vec<Rc<Expr<'input>>>),
    ArrayCtor(Option<IterExpr<'input>>),
    Literal(lexer::ast::Literal<'input>),
    ThisLiteral(lexer::ast::Literal<'input>),
    InterpolatedString(lexer::ast::InterpolatedString<'input>),
    EvalVar(lexer::ast::Ident<'input>),
    LetInBind(VarBind<'input>, lexer::ast::Keyword<'input>, Rc<Expr<'input>>),
    If(lexer::ast::Keyword<'input>, Rc<Expr<'input>>, StatsBlock<'input>, Option<(lexer::ast::Keyword<'input>, StatsBlock<'input>)>),
    While(lexer::ast::Keyword<'input>, Rc<Expr<'input>>, StatsBlock<'input>),
    Loop(lexer::ast::Keyword<'input>, StatsBlock<'input>),
    For(Vec<(lexer::ast::Keyword<'input>, ForBind<'input>)>, StatsBlock<'input>),
    Closure(VarDecl<'input>, Rc<Expr<'input>>),
}

#[derive(Clone, Debug, PartialEq)]
pub enum IterExpr<'input> {
    Range(Rc<Expr<'input>>, Rc<Expr<'input>>),
    SteppedRange(Rc<Expr<'input>>, Rc<Expr<'input>>, Rc<Expr<'input>>),
    Spread(Rc<Expr<'input>>),
    Elements(Vec<Rc<Expr<'input>>>),
}

#[derive(Clone, Debug, PartialEq)]
pub struct ArgExpr<'input>(pub Option<MutAttr<'input>>, pub Rc<Expr<'input>>);

#[derive(Clone, Debug, PartialEq)]
pub enum ForBind<'input> {
    Let(lexer::ast::Keyword<'input>, VarDecl<'input>, ForIterExpr<'input>),
    Assign(Rc<Expr<'input>>, ForIterExpr<'input>),
}

#[derive(Clone, Debug, PartialEq)]
pub enum ForIterExpr<'input> {
    Range(Rc<Expr<'input>>, Rc<Expr<'input>>),
    SteppedRange(Rc<Expr<'input>>, Rc<Expr<'input>>, Rc<Expr<'input>>),
    Spread(Rc<Expr<'input>>),
}
