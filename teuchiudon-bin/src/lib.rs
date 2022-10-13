use std::ffi::{CStr, CString};
use std::os::raw::{c_char};

#[no_mangle]
pub extern "C" fn compile(input: *const c_char) -> *const c_char {
    let s = unsafe { CStr::from_ptr(input) }.to_str().unwrap();
    let output = teuchiudon_compiler::compile(s).map_or_else(|e| e, |x| x);
    CString::new(output).unwrap().into_raw()
}

#[no_mangle]
pub extern "C" fn free_str(ptr: *mut c_char) {
    std::mem::drop(unsafe { CString::from_raw(ptr) });
}
