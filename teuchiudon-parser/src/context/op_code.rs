use std::rc::Rc;
use crate::lexer::ast::{
    OpCode,
    OpCodeKind,
};

pub struct OpCodeContext {
    op_codes: Vec<(&'static str, OpCodeKind)>,
}

impl OpCodeContext {
    pub fn new() -> Self {
        Self {
            op_codes: vec![
                ("{", OpCodeKind::OpenBrace),
                ("}", OpCodeKind::CloseBrace),
                ("(", OpCodeKind::OpenParen),
                (")", OpCodeKind::CloseParen),
                ("[", OpCodeKind::OpenBracket),
                ("]", OpCodeKind::CloseBracket),
                (",", OpCodeKind::Comma),
                (":", OpCodeKind::Colon),
                ("=", OpCodeKind::Bind),
                (";", OpCodeKind::Semicolon),
                (".", OpCodeKind::Dot),
                ("+", OpCodeKind::Plus),
                ("-", OpCodeKind::Minus),
                ("*", OpCodeKind::Star),
                ("/", OpCodeKind::Div),
                ("%", OpCodeKind::Percent),
                ("&", OpCodeKind::Amp),
                ("|", OpCodeKind::Pipe),
                ("^", OpCodeKind::Caret),
                ("!", OpCodeKind::Bang),
                ("~", OpCodeKind::Tilde),
                ("<", OpCodeKind::Lt),
                (">", OpCodeKind::Gt),
                ("_", OpCodeKind::Wildcard),
                ("<-", OpCodeKind::Iter),
                ("->", OpCodeKind::Arrow),
                ("::", OpCodeKind::DoubleColon),
                ("??", OpCodeKind::Coalescing),
                ("?.", OpCodeKind::CoalescingAccess),
                ("&&", OpCodeKind::And),
                ("||", OpCodeKind::Or),
                ("==", OpCodeKind::Eq),
                ("!=", OpCodeKind::Ne),
                ("<=", OpCodeKind::Le),
                (">=", OpCodeKind::Ge),
                ("<<", OpCodeKind::LeftShift),
                (">>", OpCodeKind::RightShift),
                ("<|", OpCodeKind::LeftPipeline),
                ("|>", OpCodeKind::RightPipeline),
                ("..", OpCodeKind::Range),
                ("...", OpCodeKind::Spread),
            ]
        }
    }

    pub fn from_str<'input>(&self, name: &str, slice: &'input str) -> Option<Rc<OpCode<'input>>> {
        self.op_codes.iter().find(|x| x.0 == name).map(|x| Rc::new(OpCode { slice, kind: x.1 }))
    }

    pub fn iter_op_code_str(&self) -> impl Iterator<Item = &str> {
        self.op_codes.iter().map(|x| x.0)
    }
}
