mod vm;

use std::{
    fs,
    path::Path,
};
use rstest::rstest;
use teuchiudon_compiler::compile;
use crate::json;
use self::vm::VM;

#[derive(Debug)]
struct TestCase {
    path: String,
    src: String,
    expected: Expected,
    stubs: Vec<(String, String)>
}

#[derive(Debug)]
enum Expected {
    Err,
    None,
    Some(String),
}

#[rstest]
#[case::function("./src/tests/teuchi/function")]
#[case::general("./src/tests/teuchi/general")]
#[case::infix_op("./src/tests/teuchi/infix_op")]
#[case::let_bind("./src/tests/teuchi/let_bind")]
#[case::literal("./src/tests/teuchi/literal")]
#[case::prefix_op("./src/tests/teuchi/prefix_op")]
#[case::scope("./src/tests/teuchi/scope")]
#[case::top_stat("./src/tests/teuchi/top_stat")]
fn test_teuchi(#[case] path: &str) {
    let v = Vec::new();
    let test_cases = find_teuchi(v, Path::new(path));

    let symbols = fs::read_to_string("./src/tests/teuchi/udon-symbols.json").unwrap();

    for test_case in test_cases {
        let json = compile(&test_case.src, &symbols);
        let compiled = json::from_json(&json);

        if compiled.errors.len() != 0 && matches!(test_case.expected, Expected::Err) {
            continue;
        }
        else if compiled.errors.len() == 0 && !matches!(test_case.expected, Expected::Err) {
            let mut vm = VM::new(compiled.output.clone(), compiled.default_values, test_case.stubs);
            vm.run("_start");

            if vm.logs.len() == 0 && matches!(test_case.expected, Expected::None) {
                continue;
            }
            else if vm.logs.len() == 0 {
                panic!("In \"{}\": actual no logs, expected `{:?}`\n{}", test_case.path, test_case.expected, compiled.output);
            }
            else if vm.logs.len() == 1 {
                let Expected::Some(expected) = test_case.expected
                    else {
                        panic!("In \"{}\": actual `{}`, expected `{:?}`\n{}", test_case.path, vm.logs[0], test_case.expected, compiled.output);
                    };

                if vm.logs[0] == expected {
                    continue;
                }
                else {
                    panic!("In \"{}\": actual `{}`, expected `{:?}`\n{}", test_case.path, vm.logs[0], expected, compiled.output);
                }
            }
            else {
                panic!("In \"{}\": multiple logs exist\n{}", test_case.path, compiled.output);
            }
        }
        else if compiled.errors.len() == 0 && matches!(test_case.expected, Expected::Err) {
            panic!("In \"{}\": actual compiled, expected compile error\n{}", test_case.path, compiled.output);
        }
        else {
            let errors = compiled.errors.join("\n");
            panic!("In \"{}\": actual compile error, expected `{:?}`\n{}\n", test_case.path, test_case.expected, errors);
        }
    }
}

fn find_teuchi(mut test_cases: Vec<TestCase>, path: &Path) -> Vec<TestCase> {
    let entries = fs::read_dir(path).unwrap();
    for entry in entries {
        let entry = entry.unwrap();
        let file_type = entry.file_type().unwrap();
        if file_type.is_file() {
            let path = entry.path();
            let src = fs::read_to_string(&path).unwrap();
            let (expected, stubs) = get_meta(&src);
            test_cases.push(TestCase {
                path: path.to_str().unwrap().to_owned(),
                src,
                expected,
                stubs,
            })
        }
    }
    test_cases
}

fn get_meta(src: &str) -> (Expected, Vec<(String, String)>) {
    let mut lines = src.lines();
    let line = lines.next();
    let line = line.unwrap();
    assert!(line.starts_with("//"));
    let meta = line.trim().chars().skip(2).collect::<String>().trim().to_owned();
    let expected = match meta.as_str() {
        "!" => Expected::Err,
        "?" => Expected::None,
        _ => Expected::Some(meta),
    };
    
    let stubs =
        lines
        .take_while(|x| x.starts_with("//"))
        .map(|x| {
            let meta = x.trim().chars().skip(2).collect::<String>().trim().to_owned();
            let (k, v) = meta.split_once(",").unwrap();
            (k.trim().to_owned(), v.trim().to_owned())
        })
        .collect();
    (expected, stubs)
}
