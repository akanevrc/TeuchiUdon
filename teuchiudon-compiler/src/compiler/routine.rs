use std::{
    cell::RefCell,
    rc::Rc,
};
use teuchiudon_parser::semantics::elements::label::{
    CodeLabel,
    DataLabel,
    ExternLabel,
};
use crate::assembly::{
    AsmLiteral,
    CodeAddr,
    DataAddr,
    Instruction,
    SyncMode,
    label::EvalLabel,
};

pub fn comment(text: String) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
            Instruction::Comment(text),
    ].into_iter())
}

pub fn export_data(data: Rc<DataLabel>) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new(
        data.to_name().into_iter()
        .map(|data| Instruction::ExportData(data))
    )
}

pub fn sync_data(data: Rc<DataLabel>, mode: SyncMode) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new(
        data.to_name().into_iter()
        .map(move |data| Instruction::SyncData(data, mode.clone()))
    )
}

pub fn decl_data(data: Rc<DataLabel>, literal: AsmLiteral) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new(
        data.to_name().into_iter().zip(data.ty.to_name().into_iter())
        .map(move |(data, ty)| Instruction::DeclData(data, ty, literal.clone()))
    )
}

pub fn pop() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        Instruction::Pop,
    ].into_iter())
}

pub fn get(data: Rc<DataLabel>) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new(
        data.to_name().into_iter()
        .map(|data| Instruction::Push(DataAddr::Label(data)))
    )
}

pub fn set(data: Rc<DataLabel>) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new(
        data.to_name().into_iter().rev()
        .flat_map(|data|
            [
                Instruction::Push(DataAddr::Label(data)),
                Instruction::Copy,
            ]
            .into_iter()
        )
    )
}

pub fn indirect(code: Rc<CodeLabel>) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        Instruction::Push(DataAddr::Indirect(code.to_name(), RefCell::new(None))),
    ].into_iter())
}

pub fn jump_indirect(data: Rc<DataLabel>) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        Instruction::JumpIndirect(DataAddr::Label(data.to_name()[0].clone()))
    ].into_iter())
}

pub fn jump(code: Rc<CodeLabel>) -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        Instruction::Jump(CodeAddr::Label(code.to_name()))
    ].into_iter())
}

pub fn decl_fn() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn decl_start_ev<'a>(
    code: Rc<CodeLabel>,
    top_stats: Box<dyn Iterator<Item = Instruction> + 'a>,
    stats: Box<dyn Iterator<Item = Instruction> + 'a>
) -> Box<dyn Iterator<Item = Instruction> + 'a> {
    Box::new(
        [
            Instruction::ExportCode(code.to_name()),
            Instruction::Label(code.to_name()),
            Instruction::Indent(1),
        ].into_iter()
        .chain(top_stats.into_iter())
        .chain(stats.into_iter())
        .chain([
            Instruction::Jump(CodeAddr::Number(0xFFFFFFFC)),
            Instruction::Indent(-1),
        ].into_iter())
    )
}

pub fn decl_ev<'a>(code: Rc<CodeLabel>, stats: Box<dyn Iterator<Item = Instruction> + 'a>) -> Box<dyn Iterator<Item = Instruction> + 'a> {
    Box::new(
        [
            Instruction::ExportCode(code.to_name()),
            Instruction::Label(code.to_name()),
            Instruction::Indent(1),
        ].into_iter()
        .chain(stats.into_iter())
        .chain([
            Instruction::Jump(CodeAddr::Number(0xFFFFFFFC)),
            Instruction::Indent(-1),
        ].into_iter())
    )
}

pub fn eval_fn() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_method() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn call_method<'a>(args: Box<dyn Iterator<Item = Instruction> + 'a>, ext: Rc<ExternLabel>) -> Box<dyn Iterator<Item = Instruction> + 'a> {
    Box::new(
        args
        .chain([
            Instruction::Extern(ext.to_name())
        ]
        .into_iter())
    )
}

pub fn eval_assign() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_setter_assign() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_array_setter_assign() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_if() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_while() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_loop() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_for() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_range_iter() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_stepped_range_iter() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_spread_iter() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_empty_array_ctor() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_range_array_ctor() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_stepped_range_array_ctor() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_spread_array_ctor() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}

pub fn eval_elements_array_ctor() -> Box<dyn Iterator<Item = Instruction>> {
    Box::new([
        // TODO
    ].into_iter())
}
