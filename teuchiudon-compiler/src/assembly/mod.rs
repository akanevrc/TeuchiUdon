pub mod label;
pub mod container;

use std::{
    fmt,
    rc::Rc,
};
use teuchiudon_parser::semantics::elements::label::{
    CodeLabel,
    DataLabel,
    TyLabel,
};
use self::label::HasLabel;

#[derive(Clone, Debug, PartialEq)]
pub enum Instruction {
    NoCode,
    NewLine,
    Indent(i32),
    Comment(String),
    Nop,
    Push(DataAddr),
    Pop,
    JumpIfFalse(CodeAddr),
    Jump(CodeAddr),
    Extern,
    Annotation,
    JumpIndirect(DataAddr),
    Copy,
    DataStart,
    DataEnd,
    CodeStart,
    CodeEnd,
    ExportData(Rc<DataLabel>),
    SyncData(Rc<DataLabel>, SyncMode),
    DeclData(Rc<DataLabel>, Rc<TyLabel>, AsmLiteral),
    ExportCode(Rc<CodeLabel>),
    Label(Rc<CodeLabel>),
}

#[derive(Clone, Debug, PartialEq)]
pub enum SyncMode {
    Sync,
    Linear,
    Smooth,
}

#[derive(Clone, Debug, PartialEq)]
pub enum AsmLiteral {
    Null,
    This,
    Address(u32),
    Raw(String),
}

#[derive(Clone, Debug, PartialEq)]
pub enum DataAddr {
    Label(Rc<DataLabel>),
    Indirect,
}

#[derive(Clone, Debug, PartialEq)]
pub enum CodeAddr {
    Label(Rc<CodeLabel>),
    Number(u32),
}

impl fmt::Display for Instruction {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Self::Comment(text) =>
                write!(f, "# {}", text),
            Self::Nop =>
                write!(f, "NOP"),
            Self::Push(addr) =>
                write!(f, "PUSH, {}", addr.to_string()),
            Self::Pop =>
                write!(f, "POP"),
            Self::JumpIfFalse(addr) =>
                write!(f, "JUMP_IF_FALSE, {}", addr.to_string()),
            Self::Jump(addr) =>
                write!(f, "JUMP, {}", addr.to_string()),
            Self::Extern =>
                write!(f, "EXTERN,"),
            Self::Annotation =>
                write!(f, "ANNOTATION"),
            Self::JumpIndirect(addr) =>
                write!(f, "JUMP_INDIRECT, {}", addr.to_string()),
            Self::Copy =>
                write!(f, "COPY"),
            Self::DataStart =>
                write!(f, ".data_start"),
            Self::DataEnd =>
                write!(f, ".data_end"),
            Self::CodeStart =>
                write!(f, ".code_start"),
            Self::CodeEnd =>
                write!(f, ".code_end"),
            Self::ExportData(data) =>
                write!(f, ".export {}", data.full_name()),
            Self::SyncData(data, mode) =>
                write!(f, ".sync {}, {}", data.full_name(), mode.to_string()),
            Self::DeclData(data, ty, literal) =>
                write!(f, "{}: %{}, {}", data.full_name(), ty.full_name(), literal.to_string()),
            Self::ExportCode(code) =>
                write!(f, ".export {}", code.full_name()),
            Self::Label(code) =>
                write!(f, "{}", code.full_name()),
            _ =>
                fmt::Result::Err(fmt::Error),
        }
    }
}

impl fmt::Display for SyncMode {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Self::Sync =>
                write!(f, "none"),
            Self::Linear =>
                write!(f, "linear"),
            Self::Smooth =>
                write!(f, "smooth"),
        }
    }
}

impl fmt::Display for AsmLiteral {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Self::Null =>
                write!(f, "null"),
            Self::This =>
                write!(f, "this"),
            Self::Address(addr) =>
                write!(f, "0x{:#010X}", addr),
            Self::Raw(raw) =>
                write!(f, "{}", raw),
        }
    }
}

impl fmt::Display for DataAddr {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Self::Label(data) =>
                write!(f, "{}", data.full_name()),
            Self::Indirect =>
                write!(f, ""),
        }
    }
}

impl fmt::Display for CodeAddr {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            Self::Label(code) =>
                write!(f, "{}", code.full_name()),
            Self::Number(addr) =>
                write!(f, "0x{:#010X}", addr),
        }
    }
}

impl Instruction {
    pub fn byte_size(&self) -> u32 {
        match self {
            Self::Nop =>
                4,
            Self::Push(_) =>
                8,
            Self::Pop =>
                4,
            Self::JumpIfFalse(_) =>
                8,
            Self::Jump(_) =>
                8,
            Self::Extern =>
                8,
            Self::Annotation =>
                4,
            Self::JumpIndirect(_) =>
                8,
            Self::Copy =>
                4,
            Self::DeclData(_, _, _) =>
                1,
            _ =>
                0,
        }
    }
}
