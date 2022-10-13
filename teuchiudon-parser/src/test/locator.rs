use logos::Logos;
use crate::locator::{
    locations::Locations,
    token::Token,
};

#[test]
fn test_normal() {
    let lexer = Token::lexer("ABCDE\rabcde\n123\r\n45");
    let locations = Locations::new(lexer);
    assert_eq!(locations.lines.len(), 4);
    assert_eq!(locations.to_line_column(0), Some((1, 1)));
    assert_eq!(locations.to_line_column(2), Some((1, 3)));
    assert_eq!(locations.to_line_column(4), Some((1, 5)));
    assert_eq!(locations.to_line_column(6), Some((2, 1)));
    assert_eq!(locations.to_line_column(10), Some((2, 5)));
    assert_eq!(locations.to_line_column(12), Some((3, 1)));
    assert_eq!(locations.to_line_column(14), Some((3, 3)));
    assert_eq!(locations.to_line_column(17), Some((4, 1)));
    assert_eq!(locations.to_line_column(18), Some((4, 2)));
    assert_eq!(locations.to_line_column(19), Some((4, 3)));
    assert_eq!(locations.to_line_column(20), None);
}

#[test]
fn test_newline() {
    let lexer = Token::lexer("ABCDE\rabcde\n123\r\n");
    let locations = Locations::new(lexer);
    assert_eq!(locations.lines.len(), 3);
    assert_eq!(locations.to_line_column(0), Some((1, 1)));
    assert_eq!(locations.to_line_column(2), Some((1, 3)));
    assert_eq!(locations.to_line_column(4), Some((1, 5)));
    assert_eq!(locations.to_line_column(6), Some((2, 1)));
    assert_eq!(locations.to_line_column(10), Some((2, 5)));
    assert_eq!(locations.to_line_column(12), Some((3, 1)));
    assert_eq!(locations.to_line_column(14), Some((3, 3)));
    assert_eq!(locations.to_line_column(17), Some((4, 1)));
    assert_eq!(locations.to_line_column(18), None);
}
