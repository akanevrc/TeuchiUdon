use std::{
    cell::RefCell,
    rc::Rc,
};
use teuchiudon_parser::semantics::elements::label::{
    CodeLabel,
    DataLabel,
};
use crate::assembly::{
    AsmLiteral,
    CodeAddr,
    DataAddr,
    Instruction,
    SyncMode,
};

pub fn export_data(data: Rc<DataLabel>) -> impl Iterator<Item = Instruction> {
    [
        Instruction::ExportData(data),
    ]
    .into_iter()
}

pub fn sync_data(data: Rc<DataLabel>, mode: SyncMode) -> impl Iterator<Item = Instruction> {
    [
        Instruction::SyncData(data, mode),
    ]
    .into_iter()
}

pub fn decl_data(data: Rc<DataLabel>, literal: AsmLiteral) -> impl Iterator<Item = Instruction> {
    [
        Instruction::DeclData(data.clone(), data.ty.clone(), literal),
    ]
    .into_iter()
}

pub fn pop() -> impl Iterator<Item = Instruction> {
    [
        Instruction::Pop,
    ]
    .into_iter()
}

pub fn get(data: Rc<DataLabel>) -> impl Iterator<Item = Instruction> {
    [
        Instruction::Push(DataAddr::Label(data)),
    ]
    .into_iter()
}

pub fn set(data: Rc<DataLabel>) -> impl Iterator<Item = Instruction> {
    [
        Instruction::Push(DataAddr::Label(data)),
        Instruction::Copy,
    ]
    .into_iter()
}

pub fn indirect(code: Rc<CodeLabel>) -> impl Iterator<Item = Instruction> {
    [
        Instruction::Push(DataAddr::Indirect(code, RefCell::new(None))),
    ]
    .into_iter()
}

pub fn jump_indirect(data: Rc<DataLabel>) -> impl Iterator<Item = Instruction> {
    [
        Instruction::JumpIndirect(DataAddr::Label(data))
    ]
    .into_iter()
}

pub fn jump(code: Rc<CodeLabel>) -> impl Iterator<Item = Instruction> {
    [
        Instruction::Jump(CodeAddr::Label(code))
    ]
    .into_iter()
}

pub fn decl_fn() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn decl_ev() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_fn() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_method() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn call_method() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_assign() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_setter_assign() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_array_setter_assign() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_if() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_while() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_loop() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_for() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_range_iter() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_stepped_range_iter() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_spread_iter() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_empty_array_ctor() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_range_array_ctor() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_stepped_range_array_ctor() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_spread_array_ctor() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}

pub fn eval_elements_array_ctor() -> impl Iterator<Item = Instruction> {
    [
        // TODO
    ]
    .into_iter()
}
