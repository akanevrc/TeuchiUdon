#[macro_use] extern crate lalrpop_util;

lalrpop_mod!(pub grammar);

#[test]
fn test_target() {
    assert!(grammar::TargetParser::new().parse("Hello, TeuchiUdon!").is_ok());
    assert!(grammar::TargetParser::new().parse("Goodbye, TeuchiUdon!").is_err());

    assert_eq!(grammar::TargetParser::new().parse("Hello, TeuchiUdon!").unwrap(), "TeuchiUdon");
}
