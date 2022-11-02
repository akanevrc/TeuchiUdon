use nom_supreme::error::{
    BaseErrorKind,
    Expectation,
    GenericErrorTree,
    StackContext,
};
use crate::error::{
    ErrorTree,
    line_infoes,
};

#[test]
fn test() {
    let input = "1:abcde\n2:abcde\n3:abcde\n4:abcde\n5:abcde";
    assert_eq!(
        line_infoes(
            input,
            &ErrorTree::Stack {
                base: Box::new(
                    GenericErrorTree::Base {
                        location: &input[0..1],
                        kind: BaseErrorKind::Expected(Expectation::Something),
                    },
                ),
                contexts: vec![
                    (&input[0..1], StackContext::Context("1".to_owned())),
                ],
            },
        ),
        vec![(1, 1, &input[0..7], &input[0..1], &"1".to_owned())],
    );
    assert_eq!(
        line_infoes(
            input,
            &ErrorTree::Stack {
                base: Box::new(
                    GenericErrorTree::Base {
                        location: &input[0..1],
                        kind: BaseErrorKind::Expected(Expectation::Something),
                    },
                ),
                contexts: vec![
                    (&input[2..4], StackContext::Context("a".to_owned())),
                ],
            },
        ),
        vec![(1, 3, &input[0..7], &input[2..4], &"a".to_owned())],
    );
    assert_eq!(
        line_infoes(
            input,
            &ErrorTree::Stack {
                base: Box::new(
                    GenericErrorTree::Base {
                        location: &input[0..1],
                        kind: BaseErrorKind::Expected(Expectation::Something),
                    },
                ),
                contexts: vec![
                    (&input[8..10], StackContext::Context("2".to_owned())),
                ],
            },
        ),
        vec![(2, 1, &input[8..15], &input[8..10], &"2".to_owned())],
    );
    assert_eq!(
        line_infoes(
            input,
            &ErrorTree::Stack {
                base: Box::new(
                    GenericErrorTree::Base {
                        location: &input[0..1],
                        kind: BaseErrorKind::Expected(Expectation::Something),
                    },
                ),
                contexts: vec![
                    (&input[18..32], StackContext::Context("a".to_owned())),
                ],
            },
        ),
        vec![(3, 3, &input[16..23], &input[18..32], &"a".to_owned())],
    );
    assert_eq!(
        line_infoes(
            input,
            &ErrorTree::Stack {
                base: Box::new(
                    GenericErrorTree::Base {
                        location: &input[0..1],
                        kind: BaseErrorKind::Expected(Expectation::Something),
                    },
                ),
                contexts: vec![
                    (&input[18..20], StackContext::Context("a".to_owned())),
                    (&input[20..22], StackContext::Context("c".to_owned())),
                ],
            },
        ),
        vec![(3, 3, &input[16..23], &input[18..20], &"a".to_owned()), (3, 5, &input[16..23], &input[20..22], &"c".to_owned())],
    );
    assert_eq!(
        line_infoes(
            input,
            &ErrorTree::Stack {
                base: Box::new(
                    GenericErrorTree::Base {
                        location: &input[0..1],
                        kind: BaseErrorKind::Expected(Expectation::Something),
                    },
                ),
                contexts: vec![
                    (&input[18..20], StackContext::Context("a".to_owned())),
                    (&input[26..28], StackContext::Context("a".to_owned())),
                ],
            },
        ),
        vec![(3, 3, &input[16..23], &input[18..20], &"a".to_owned()), (4, 3, &input[24..31], &input[26..28], &"a".to_owned())],
    );
    assert_eq!(
        line_infoes(
            input,
            &ErrorTree::Stack {
                base: Box::new(
                    GenericErrorTree::Base {
                        location: &input[0..1],
                        kind: BaseErrorKind::Expected(Expectation::Something),
                    },
                ),
                contexts: vec![
                    (&input[18..20], StackContext::Context("a".to_owned())),
                    (&input[27..29], StackContext::Context("b".to_owned())),
                ],
            },
        ),
        vec![(3, 3, &input[16..23], &input[18..20], &"a".to_owned()), (4, 4, &input[24..31], &input[27..29], &"b".to_owned())],
    );
    assert_eq!(
        line_infoes(
            input,
            &ErrorTree::Stack {
                base: Box::new(
                    GenericErrorTree::Base {
                        location: &input[0..1],
                        kind: BaseErrorKind::Expected(Expectation::Something),
                    },
                ),
                contexts: vec![
                    (&input[27..29], StackContext::Context("b".to_owned())),
                    (&input[18..20], StackContext::Context("a".to_owned())),
                ],
            },
        ),
        vec![(4, 4, &input[24..31], &input[27..29], &"b".to_owned()), (3, 3, &input[16..23], &input[18..20], &"a".to_owned())],
    );
}
