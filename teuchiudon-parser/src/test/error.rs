use nom::{
    error::VerboseError,
    error::VerboseErrorKind,
};
use crate::error::line_char_numbers;

#[test]
fn test() {
    let input = "1:abcde\n2:abcde\n3:abcde\n4:abcde\n5:abcde";
    assert_eq!(
        line_char_numbers(input, VerboseError { errors: vec![(&input[0..1], VerboseErrorKind::Char('1'))] }),
        vec![(1, 1, &input[0..1], VerboseErrorKind::Char('1'))],
    );
    assert_eq!(
        line_char_numbers(input, VerboseError { errors: vec![(&input[2..4], VerboseErrorKind::Char('a'))] }),
        vec![(1, 3, &input[2..4], VerboseErrorKind::Char('a'))],
    );
    assert_eq!(
        line_char_numbers(input, VerboseError { errors: vec![(&input[8..10], VerboseErrorKind::Char('2'))] }),
        vec![(2, 1, &input[8..10], VerboseErrorKind::Char('2'))],
    );
    assert_eq!(
        line_char_numbers(input, VerboseError { errors: vec![(&input[18..32], VerboseErrorKind::Char('a'))] }),
        vec![(3, 3, &input[18..32], VerboseErrorKind::Char('a'))],
    );
    assert_eq!(
        line_char_numbers(input, VerboseError { errors: vec![(&input[18..20], VerboseErrorKind::Char('a')), (&input[20..22], VerboseErrorKind::Char('c'))] }),
        vec![(3, 3, &input[18..20], VerboseErrorKind::Char('a')), (3, 5, &input[20..22], VerboseErrorKind::Char('c'))],
    );
    assert_eq!(
        line_char_numbers(input, VerboseError { errors: vec![(&input[18..20], VerboseErrorKind::Char('a')), (&input[26..28], VerboseErrorKind::Char('a'))] }),
        vec![(3, 3, &input[18..20], VerboseErrorKind::Char('a')), (4, 3, &input[26..28], VerboseErrorKind::Char('a'))],
    );
    assert_eq!(
        line_char_numbers(input, VerboseError { errors: vec![(&input[18..20], VerboseErrorKind::Char('a')), (&input[27..29], VerboseErrorKind::Char('b'))] }),
        vec![(3, 3, &input[18..20], VerboseErrorKind::Char('a')), (4, 4, &input[27..29], VerboseErrorKind::Char('b'))],
    );
    assert_eq!(
        line_char_numbers(input, VerboseError { errors: vec![(&input[27..29], VerboseErrorKind::Char('b')), (&input[18..20], VerboseErrorKind::Char('a'))] }),
        vec![(4, 4, &input[27..29], VerboseErrorKind::Char('b')), (3, 3, &input[18..20], VerboseErrorKind::Char('a'))],
    );
}
