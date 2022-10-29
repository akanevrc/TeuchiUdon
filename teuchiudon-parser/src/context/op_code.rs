use crate::lexer::ast::OpCode;

pub struct OpCodeContext {
    op_codes: Vec<(&'static str, Box<dyn Fn(&str) -> OpCode>)>,
}

impl OpCodeContext {
    pub fn new() -> Self {
        Self {
            op_codes: vec![
                ("{", Box::new(|input: &str| OpCode::OpenBrace(input))),
                ("}", Box::new(|input: &str| OpCode::CloseBrace(input))),
                ("(", Box::new(|input: &str| OpCode::OpenParen(input))),
                (")", Box::new(|input: &str| OpCode::CloseParen(input))),
                ("[", Box::new(|input: &str| OpCode::OpenBracket(input))),
                ("]", Box::new(|input: &str| OpCode::CloseBracket(input))),
                (",", Box::new(|input: &str| OpCode::Comma(input))),
                (":", Box::new(|input: &str| OpCode::Colon(input))),
                ("=", Box::new(|input: &str| OpCode::Bind(input))),
                (";", Box::new(|input: &str| OpCode::Semicolon(input))),
                (".", Box::new(|input: &str| OpCode::Dot(input))),
                ("+", Box::new(|input: &str| OpCode::Plus(input))),
                ("-", Box::new(|input: &str| OpCode::Minus(input))),
                ("*", Box::new(|input: &str| OpCode::Star(input))),
                ("/", Box::new(|input: &str| OpCode::Div(input))),
                ("%", Box::new(|input: &str| OpCode::Percent(input))),
                ("&", Box::new(|input: &str| OpCode::Amp(input))),
                ("|", Box::new(|input: &str| OpCode::Pipe(input))),
                ("^", Box::new(|input: &str| OpCode::Caret(input))),
                ("!", Box::new(|input: &str| OpCode::Bang(input))),
                ("~", Box::new(|input: &str| OpCode::Tilde(input))),
                ("<", Box::new(|input: &str| OpCode::Lt(input))),
                (">", Box::new(|input: &str| OpCode::Gt(input))),
                ("_", Box::new(|input: &str| OpCode::Wildcard(input))),
                ("<-", Box::new(|input: &str| OpCode::Iter(input))),
                ("->", Box::new(|input: &str| OpCode::Arrow(input))),
                ("::", Box::new(|input: &str| OpCode::DoubleColon(input))),
                ("??", Box::new(|input: &str| OpCode::Coalescing(input))),
                ("?.", Box::new(|input: &str| OpCode::CoalescingAccess(input))),
                ("&&", Box::new(|input: &str| OpCode::And(input))),
                ("||", Box::new(|input: &str| OpCode::Or(input))),
                ("==", Box::new(|input: &str| OpCode::Eq(input))),
                ("!=", Box::new(|input: &str| OpCode::Ne(input))),
                ("<=", Box::new(|input: &str| OpCode::Le(input))),
                (">=", Box::new(|input: &str| OpCode::Ge(input))),
                ("<<", Box::new(|input: &str| OpCode::LeftShift(input))),
                (">>", Box::new(|input: &str| OpCode::RightShift(input))),
                ("<|", Box::new(|input: &str| OpCode::LeftPipeline(input))),
                ("|>", Box::new(|input: &str| OpCode::RightPipeline(input))),
                ("..", Box::new(|input: &str| OpCode::Range(input))),
                ("...", Box::new(|input: &str| OpCode::Spread(input))),
            ]
        }
    }

    pub fn from_str<'s>(&'s self, name: &str, slice: &'s str) -> Option<OpCode> {
        self.op_codes.iter().find(|x| x.0 == name).map(|x| x.1(slice))
    }

    pub fn iter_op_code_str(&self) -> impl Iterator<Item = &str> {
        self.op_codes.iter().map(|x| x.0)
    }
}
