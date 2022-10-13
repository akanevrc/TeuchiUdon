use nom::{
    Err,
    Needed,
};
use crate::parser::error_emitter::map_err;

#[test]
fn test() {
    assert_eq!(map_err(&Err(Err::Incomplete(Needed::new(1)))).unwrap_err().len(), 1);
}
