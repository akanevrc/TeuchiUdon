use std::collections::HashMap;
use regex::{
    Regex,
    RegexSet,
};
use super::json::DefaultValue;

#[derive(Clone, Debug)]
pub struct VM {
    pub var_pubs: HashMap<String, bool>,
    pub var_syncs: HashMap<String, Option<String>>,
    pub var_tys: HashMap<String, String>,
    pub var_values: HashMap<String, String>,
    pub label_pubs: HashMap<String, bool>,
    pub label_addrs: HashMap<String, u32>,
    pub codes: HashMap<u32, (u32, String)>,
    pub stack: Vec<String>,
    pub logs: Vec<String>,
}

impl VM {
    const INSTRUCTION_LIMIT: i32 = 10000;
    const VALUE_DELIMITER: char = ',';

    pub fn new(asm: String, default_values: Vec<DefaultValue>) -> Self {
        let mut vm = Self {
            var_pubs: HashMap::new(),
            var_syncs: HashMap::new(),
            var_tys: HashMap::new(),
            var_values: HashMap::new(),
            label_pubs: HashMap::new(),
            label_addrs: HashMap::new(),
            codes: HashMap::new(),
            stack: Vec::new(),
            logs: Vec::new(),
        };
        vm.load_asm(asm, default_values);
        vm
    }

    fn load_asm(&mut self, asm: String, default_values: Vec<DefaultValue>) {
        let lines = asm.lines().map(|x| x.trim()).collect::<Vec<_>>().into_iter();
        let before = lines.clone().take_while(|x| *x != ".data_start");
        let data_part = lines.clone().skip_while(|x| *x != ".data_start").skip(1).take_while(|x| *x != ".data_end");
        let middle = lines.clone().skip_while(|x| *x != ".data_end").skip(1).take_while(|x| *x != ".code_start");
        let code_part = lines.clone().skip_while(|x| *x != ".code_start").skip(1).take_while(|x| *x != ".code_end");
        let after = lines.skip_while(|x| *x != ".code_end").skip(1);

        self.load_blank(before);
        self.load_blank(middle);
        self.load_blank(after);

        self.load_data(data_part);
        self.load_code(code_part);

        self.load_default_values(default_values);
    }

    fn load_blank<'a>(&mut self, iter: impl Iterator<Item = &'a str>) {
        for line in iter {
            let set = RegexSet::new(&[
                r"^$",
                r"^#.*$",
            ]).unwrap();
            assert!(set.is_match(line));
        }
    }

    fn load_data<'a>(&mut self, iter: impl Iterator<Item = &'a str>) {
        for line in iter {
            if Regex::new(r"^$").unwrap().is_match(line) {
                ()
            }
            else if Regex::new(r"^#.*$").unwrap().is_match(line) {
                ()
            }
            else if let Some(caps) = Regex::new(r"^\.export (.+)$").unwrap().captures(line) {
                let var = caps.get(1).unwrap().as_str().to_owned();
                self.var_pubs.insert(var, true);
            }
            else if let Some(caps) = Regex::new(r"^\.sync (.+), (.+)$").unwrap().captures(line) {
                let var = caps.get(1).unwrap().as_str().to_owned();
                let sync = caps.get(2).unwrap().as_str().to_owned();
                self.var_syncs.insert(var.to_owned(), Some(sync.to_owned()));
            }
            else if let Some(caps) = Regex::new(r"^(.+): %(.+), (.+)$").unwrap().captures(line) {
                let var = caps.get(1).unwrap().as_str().to_owned();
                let ty = caps.get(2).unwrap().as_str().to_owned();
                let value = caps.get(3).unwrap().as_str().to_owned();
                if !self.var_pubs.contains_key(&var) {
                    self.var_pubs.insert(var.clone(), false);
                }
                if !self.var_syncs.contains_key(&var) {
                    self.var_syncs.insert(var.clone(), None);
                }
                self.var_tys.insert(var.clone(), ty);
                self.var_values.insert(var, value);
            }
            else {
                panic!("Invalid instruction detected");
            }
        }
    }

    fn load_code<'a>(&mut self, iter: impl Iterator<Item = &'a str>) {
        let mut addr = 0u32;
        for line in iter {
            if Regex::new(r"^$").unwrap().is_match(line) {
                ()
            }
            else if Regex::new(r"^#.*$").unwrap().is_match(line) {
                ()
            }
            else if Regex::new(r"^NOP$").unwrap().is_match(line) {
                self.codes.insert(addr, (4, line.to_owned()));
                addr += 4;
            }
            else if Regex::new(r"^PUSH, (.+)$").unwrap().is_match(line) {
                self.codes.insert(addr, (8, line.to_owned()));
                addr += 8;
            }
            else if Regex::new(r"^POP$").unwrap().is_match(line) {
                self.codes.insert(addr, (4, line.to_owned()));
                addr += 4;
            }
            else if Regex::new(r"^JUMP_IF_FALSE, (.+)$").unwrap().is_match(line) {
                self.codes.insert(addr, (8, line.to_owned()));
                addr += 8;
            }
            else if Regex::new(r"^JUMP, (.+)$").unwrap().is_match(line) {
                self.codes.insert(addr, (8, line.to_owned()));
                addr += 8;
            }
            else if Regex::new(r#"^EXTERN, "(.+)"$"#).unwrap().is_match(line) {
                self.codes.insert(addr, (8, line.to_owned()));
                addr += 8;
            }
            else if Regex::new(r"^ANNOTATION$").unwrap().is_match(line) {
                self.codes.insert(addr, (4, line.to_owned()));
                addr += 4;
            }
            else if Regex::new(r"^JUMP_INDIRECT, (.+)$").unwrap().is_match(line) {
                self.codes.insert(addr, (8, line.to_owned()));
                addr += 8;
            }
            else if Regex::new(r"^COPY$").unwrap().is_match(line) {
                self.codes.insert(addr, (4, line.to_owned()));
                addr += 4;
            }
            else if let Some(caps) = Regex::new(r"^\.export (.+)$").unwrap().captures(line) {
                let label = caps.get(1).unwrap().as_str().to_owned();
                self.label_pubs.insert(label, true);
            }
            else if let Some(caps) = Regex::new(r"^(.+):$").unwrap().captures(line) {
                let label = caps.get(1).unwrap().as_str().to_owned();
                if !self.label_pubs.contains_key(&label) {
                    self.label_pubs.insert(label.clone(), false);
                }
                self.label_addrs.insert(label, addr);
            }
        }
    }

    fn load_default_values(&mut self, default_values: Vec<DefaultValue>) {
        for dv in default_values {
            if self.var_values.contains_key(&dv.name) {
                self.var_values.insert(dv.name, dv.value);
            }
            else {
                panic!("Variable of default value not found");
            }
        }
    }

    pub fn run(&mut self, label: &str) {
        if !self.label_addrs.contains_key(label) {
            return;
        }

        let mut addr = self.label_addrs[label];
        for _ in 0..Self::INSTRUCTION_LIMIT {
            if addr == 0xFFFFFFFC {
                return;
            }

            let (offset, instruction) = self.codes[&addr].clone();
            if Regex::new(r"^NOP$").unwrap().is_match(&instruction) {
                ()
            }
            else if let Some(caps) = Regex::new(r"^PUSH, (.+)$").unwrap().captures(&instruction) {
                let value = caps.get(1).unwrap().as_str();
                self.stack.push(value.to_owned());
            }
            else if Regex::new(r"^POP$").unwrap().is_match(&instruction) {
                self.stack.pop();
            }
            else if let Some(caps) = Regex::new(r"^JUMP_IF_FALSE, (.+)$").unwrap().captures(&instruction) {
                let var = self.stack.pop().unwrap();
                let value = &self.var_values[&var];
                if Self::end_value(value) == "true" {
                    ()
                }
                else if Self::end_value(value) == "false" {
                    let label = caps.get(1).unwrap().as_str();
                    let label_addr = self.get_addr(label);
                    addr = label_addr;
                    continue;
                }
                else {
                    panic!("Popped value is not boolean value");
                }
            }
            else if let Some(caps) = Regex::new(r"^JUMP, (.+)$").unwrap().captures(&instruction) {
                let label = caps.get(1).unwrap().as_str();
                let label_addr = self.get_addr(label);
                addr = label_addr;
                continue;
            }
            else if let Some(caps) = Regex::new(r#"^EXTERN, "(.+)"$"#).unwrap().captures(&instruction) {
                let symbol = caps.get(1).unwrap().as_str();
                self.call_method(&symbol);
            }
            else if Regex::new(r"^ANNOTATION$").unwrap().is_match(&instruction) {
                ()
            }
            else if let Some(caps) = Regex::new(r"^JUMP_INDIRECT, (.+)$").unwrap().captures(&instruction) {
                let var = caps.get(1).unwrap().as_str();
                let value = &self.var_values[var];
                let label_addr = self.get_addr(value);
                addr = label_addr;
                continue;
            }
            else if Regex::new(r"^COPY$").unwrap().is_match(&instruction) {
                let assigned = self.stack.pop().unwrap();
                let var = self.stack.pop().unwrap();
                let value = &self.var_values[&var];
                self.var_values.insert(assigned, value.to_owned());
            }
            else {
                panic!("Illegal state");
            }

            addr += offset;
        }

        panic!("Too many instructions executed");
    }

    fn call_method(&mut self, symbol: &str) {
        match symbol {
            "SystemInt32.__op_UnaryMinus__SystemInt32__SystemInt32" => {
                let out_var = self.stack.pop().unwrap();
                let in_var = self.stack.pop().unwrap();
                *self.var_values.get_mut(&out_var).unwrap() = format!("{}{}{}", self.var_values.get(&in_var).unwrap(), Self::VALUE_DELIMITER, "-")
            }
            "UnityEngineDebug.__Log__SystemObject__SystemVoid" => {
                let var = self.stack.pop().unwrap();
                let value = &self.var_values[&var];
                self.logs.push(value.to_owned());
            },
            _ => ()
        }
    }

    fn get_addr(&self, label: &str) -> u32 {
         if label.starts_with("0x") {
            u32::from_str_radix(&label[2..], 16).unwrap()
         }
         else {
            self.label_addrs[label]
         }
    }

    fn end_value(value: &str) -> String {
        value.chars()
        .rev()
        .take_while(|x| *x != Self::VALUE_DELIMITER)
        .collect::<Vec<_>>().into_iter()
        .rev()
        .collect()
    }
}
