use std::{
    ffi::{
        CStr,
        CString,
    },
    panic,
};
use std::os::raw::c_char;

#[no_mangle]
pub extern "C" fn compile(input: *const c_char, json: *const c_char) -> *const c_char {
    let result = panic::catch_unwind(|| {
        let input = unsafe { CStr::from_ptr(input) }.to_str().unwrap();
        let json = unsafe { CStr::from_ptr(json) }.to_str().unwrap();
        teuchiudon_compiler::compile(input, json)
    });
    let output = match result {
        Ok(x) => x,
        Err(_) => "!panic".to_owned(),
    };
    CString::new(output).unwrap().into_raw()
}

#[no_mangle]
pub extern "C" fn free_str(ptr: *mut c_char) {
    std::mem::drop(unsafe { CString::from_raw(ptr) });
}
