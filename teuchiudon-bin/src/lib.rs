use std::ffi::{
    CStr,
    CString,
};
use std::os::raw::c_char;

#[cfg(windows)]
const NEWLINE: &'static str = "\r\n";
#[cfg(not(windows))]
const NEWLINE: &'static str = "\n";

#[no_mangle]
pub extern "C" fn compile(input: *const c_char) -> *const c_char {
    let s = unsafe { CStr::from_ptr(input) }.to_str().unwrap();
    let output =
        teuchiudon_compiler::compile(s)
        .map_or_else(|e| format!("!{}", e.join(NEWLINE)), |x| x);
    CString::new(output).unwrap().into_raw()
}

#[no_mangle]
pub extern "C" fn free_str(ptr: *mut c_char) {
    std::mem::drop(unsafe { CString::from_raw(ptr) });
}
