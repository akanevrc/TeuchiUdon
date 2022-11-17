use std::{
    collections::{
        HashMap,
        HashSet,
    },
    fmt,
    rc::Rc,
};
use teuchiudon_parser::semantics::elements::label::{
    CodeLabel,
    DataLabel,
    DataLabelKind,
};
use super::{
    AsmLiteral,
    DataAddr,
    Instruction,
};

#[cfg(windows)]
const NEWLINE: &'static str = "\r\n";
#[cfg(not(windows))]
const NEWLINE: &'static str = "\n";

pub struct AsmContainer {
    pub data_part: Vec<Instruction>,
    pub code_part: Vec<Instruction>,
    pub data_addr: HashMap<Rc<DataLabel>, u32>,
    pub code_addr: HashMap<Rc<CodeLabel>, u32>,
}

impl AsmContainer {
    pub fn new() -> Self {
        Self {
            data_part: Vec::new(),
            code_part: Vec::new(),
            data_addr: HashMap::new(),
            code_addr: HashMap::new(),
        }
    }

    pub fn push_data_part(&mut self, insss: impl Iterator<Item = impl Iterator<Item = Instruction>>) {
        for inss in insss {
            for ins in inss {
                self.data_part.push(ins);
            }
        }
    }

    pub fn push_code_part(&mut self, insss: impl Iterator<Item = impl Iterator<Item = Instruction>>) {
        let mut code_exists = false;
        for inss in insss {
            if code_exists {
                self.code_part.push(Instruction::NewLine);
            }
            code_exists = false;
            for ins in inss {
                self.code_part.push(ins);
                code_exists = true;
            }
        }
    }

    pub fn prepare(&mut self) {
        for i in 0..self.code_part.len() {
            if matches!(self.code_part[i], Instruction::Label(_)) && matches!(self.code_part[i + 1], Instruction::Label(_)) {
                self.code_part.insert(i + 1, Instruction::Nop);
            }
        }

        let mut code_byte = 0;
        for ins in &self.code_part {
            match ins {
                Instruction::Label(label) if !self.code_addr.contains_key(label) => {
                    self.code_addr.insert(label.clone(), code_byte);
                },
                _ => (),
            }
            code_byte += ins.byte_size();
        }

        for ins in &self.code_part {
            let data_addr = match ins {
                Instruction::Push(data_addr) => Some(data_addr),
                Instruction::JumpIndirect(data_addr) => Some(data_addr),
                _ => None,
            };
            if let Some(DataAddr::Indirect(code, addr)) = data_addr {
                addr.replace(Some(self.code_addr[code]));
            }
        }

        let mut used_data = HashSet::new();
        for ins in &self.code_part {
            let data_addr = match ins {
                Instruction::Push(data_addr) => Some(data_addr),
                Instruction::JumpIndirect(data_addr) => Some(data_addr),
                _ => None,
            };
            match data_addr {
                Some(DataAddr::Label(data)) => {
                    used_data.insert(data.clone());
                },
                Some(DataAddr::Indirect(code, addr)) => {
                    let addr = addr.borrow().unwrap();
                    let data = Rc::new(DataLabel::new(DataLabelKind::Indirect(code.clone(), addr)));
                    self.data_part.push(Instruction::DeclData(data.clone(), data.ty.clone(), AsmLiteral::Address(addr)));
                    used_data.insert(data.clone());
                },
                _ => (),
            }
        }

        let mut data_byte = 0;
        for ins in &self.data_part {
            match ins {
                Instruction::DeclData(data, _, _) if used_data.contains(data) => {
                    self.data_addr.insert(data.clone(), data_byte);
                },
                _ => (),
            }
            data_byte += ins.byte_size();
        }
    }
}

impl fmt::Display for AsmContainer {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let mut indent = 0;

        Self::write_one(f, &Instruction::DataStart, &mut indent)?;
        Self::write_one(f, &Instruction::Indent(1), &mut indent)?;
        for ins in &self.data_part {
            Self::write_one(f, ins, &mut indent)?;
        }
        Self::write_one(f, &Instruction::Indent(-1), &mut indent)?;
        Self::write_one(f, &Instruction::DataEnd, &mut indent)?;
        Self::write_one(f, &Instruction::NewLine, &mut indent)?;

        Self::write_one(f, &Instruction::CodeStart, &mut indent)?;
        Self::write_one(f, &Instruction::Indent(1), &mut indent)?;
        for ins in &self.code_part {
            Self::write_one(f, ins, &mut indent)?;
        }
        Self::write_one(f, &Instruction::Indent(-1), &mut indent)?;
        Self::write_one(f, &Instruction::CodeEnd, &mut indent)?;
        Ok(())
    }
}

impl AsmContainer {
    fn write_one(f: &mut fmt::Formatter<'_>, ins: &Instruction, indent: &mut i32) -> fmt::Result {
        // TODO

        match ins {
            Instruction::NoCode => (),
            Instruction::NewLine =>
                write!(f, "{}", NEWLINE)?,
            Instruction::Indent(level) => {
                *indent += level;
                if *indent < 0 {
                    *indent = 0;
                }
            },
            _ => {
                Self::write_indent(f, &indent)?;
                write!(f, "{}{}", ins, NEWLINE)?;
            },
        }
        Ok(())
    }

    fn write_indent(f: &mut fmt::Formatter<'_>, indent: &i32) -> fmt::Result {
        write!(f, "{}", " ".repeat((*indent * 4).try_into().unwrap()))
    }
}
