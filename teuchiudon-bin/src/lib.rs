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
    let input = unsafe { CStr::from_ptr(input) }.to_str().unwrap();
    let json = unsafe { CStr::from_ptr(json) }.to_str().unwrap();
    let result = panic::catch_unwind(|| {
        teuchiudon_compiler::compile(input, json)
    });
    match result {
        Ok(output) => CString::new(output).unwrap().into_raw(),
        Err(_) => CString::new("!panic").unwrap().into_raw(),
    }
}

#[no_mangle]
pub extern "C" fn free_str(ptr: *mut c_char) {
    std::mem::drop(unsafe { CString::from_raw(ptr) });
}
