use nom::combinator::success;
use super::ParsedResult;

pub fn teuchiudon(input: &str) -> ParsedResult {
    success("")(input)
}
