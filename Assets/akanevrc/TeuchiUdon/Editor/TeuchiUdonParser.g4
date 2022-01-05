parser grammar TeuchiUdonParser;

options
{
    language=CSharp;
    tokenVocab=TeuchiUdonLexer;
}

@parser::header
{
    #pragma warning disable 3021
}

target
    : open body close end* EOF
    | body EOF
    ;

body
    : (topBind)+
    ;

topBind
    returns [TopBindResult result]
    : varBind
    ;

varBind
    returns [VarBindResult result]
    : varDecl '=' expr end+
    ;

varDecl
    returns [VarDeclResult result]
    : '(' (varDecl (',' varDecl)* ','?)? ')'                              #TupleVarDecl
    | ('(' identifier (':' qualified)? ')' | identifier (':' qualified)?) #SingleVarDecl
    ;

qualified
    returns [QualifiedResult result]
    : identifier ('.' identifier)*
    ;

identifier
    returns [IdentifierResult result]
    : IDENTIFIER
    ;

expr
    returns [ExprResult result]
    : '(' expr ')'                             #ParensExpr
    | literal                                  #LiteralExpr
    | identifier                               #EvalVarExpr
    | identifier '(' ')'                       #EvalUnitFuncExpr
    | identifier '(' expr ')'                  #EvalSingleFuncExpr
    | identifier '(' expr (',' expr)* ','? ')' #EvalTupleFuncExpr
    | expr '.' expr                            #AccessExpr
    | varDecl '->' expr                        #FuncExpr
    ;

literal
    returns [LiteralResult result]
    : INTEGER_LITERAL     #IntegerLiteral
    | HEX_INTEGER_LITERAL #HexIntegerLiteral
    | BIN_INTEGER_LITERAL #BinIntegerLiteral
    | REAL_LITERAL        #RealLiteral
    | CHARACTER_LITERAL   #CharacterLiteral
    | REGULAR_STRING      #RegularString
    | VERBATIUM_STRING    #VervatiumString
    ;

open    : OPEN_BRACE  | V_OPEN ;
close   : CLOSE_BRACE | V_CLOSE;
end     : SEMICOLON   | V_END  ;
newline : NEWLINE;

// target
//     : module_decl semicolon? EOF
//     | open (module_decl semicolon?)? close semicolon? EOF
//     ;

// module_decl
//     : module_body
//     | module_spec
//     | module_spec (semicolon | newline) module_body
//     ;

// module_spec
//     : 'module' module_id export_spec?
//     ;

// export_spec
//     : '(' (export_id (',' export_id)* ','?)? ')'
//     ;

// export_id
//     : var_id
//     | type_id ('(' '..' ')' | '(' (ctor_id (',' ctor_id)* ','?)? ')')?
//     | 'module' module_id
//     ;

// module_body
//     : body
//     | open (body semicolon?)? close
//     ;

// body
//     : import_decls (semicolon | newline) top_decls
//     | import_decls
//     | top_decls
//     ;

// import_decls
//     : import_decl ((semicolon | newline) import_decl)*
//     ;

// import_decl
//     : 'import' module_id ('as' module_id)? import_spec?
//     ;

// import_spec
//     : '!'? '(' (import_id (',' import_id)* ','?)? ')'
//     ;

// import_id
//     : var_id
//     | type_id ('(' '..' ')' | '(' (ctor_id (',' ctor_id)* ','?)? ')')?
//     ;

// top_decls
//     : top_decl ((semicolon | newline) top_decl)*
//     ;

// top_decl
//     : type_decl
//     | newtype_decl
//     | data_decl
//     | rec_decl
//     | val_decl
//     ;

// type_decl
//     : 'type' type_id '=' type_expr
//     | 'type' type_id type_func
//     ;

// type_arg_specs
//     : (type_arg_spec | type_tuple_spec) ('->' (type_arg_spec | type_tuple_spec))*
//     ;

// type_tuple_spec
//     : '(' (type_arg_spec (',' type_arg_spec)* ','?)? ')'
//     ;

// type_arg_spec
//     : type_expr
//     | val_bind_spec
//     ;

// type_expr
//     : type_block
//     | type_factor
//     | type_func
//     | type_eval
//     | type_if
//     | type_case
//     | type_for
//     | type_while
//     | type_list
//     | type_dic
//     | type_tuple
//     ;

// type_block
//     : open (type_expr ((semicolon | newline) type_expr)* semicolon?)? close
//     ;

// type_factor
//     : (module_id '.')* type_id
//     | type_var_id
//     ;

// type_func
//     : type_arg_specs '->' type_expr
//     ;

// newtype_decl
//     : 'newtype' type_id '=' ctor_id type_id
//     | 'newtype' type_id '=' open val_bind_spec ','? close
//     ;

// data_decl
//     : 'data' type_id '=' '|'? ctor_spec ('|' ctor_spec)*
//     ;

// ctor_spec
//     : ctor_id ':' type_expr
//     ;

// rec_decl
//     : 'record' type_id '=' open (val_bind_spec (',' val_bind_spec)* ','?)? close
//     ;

// val_decl
//     : var_id '=' val_expr
//     | var_id val_func
//     ;

// val_bind_spec
//     : val_var_spec
//     | val_tuple_spec
//     ;

// val_tuple_spec
//     : '(' (val_var_spec (',' val_var_spec)* ','?)? ')'
//     ;

// val_var_spec
//     : var_id ':' type_expr
//     | '(' var_id ':' type_expr ')'
//     ;

// val_expr
//     : val_block
//     | val_factor
//     | val_func
//     | val_eval
//     | val_if
//     | val_case
//     | val_for
//     | val_while
//     | val_list
//     | val_dic
//     | val_tuple
//     | val_bind 'in' val_expr
//     ;

// val_block
//     : open ((val_bind (semicolon | newline)) val_expr semicolon?)? close
//     ;

// val_factor
//     : (module_id '.')* ctor_id
//     | (module_id '.')* var_id
//     ;

// val_func
//     : val_bind_spec '->' val_expr
//     ;

// val_eval
//     : val_factor '(' val_expr ')'
//     | val_eval '(' val_expr ')'
//     | '(' val_expr ')' '(' val_expr ')'
//     ;

// val_if
//     :
//     ;

// val_case
//     :
//     ;

// val_for
//     :
//     ;

// val_while
//     :
//     ;

// val_list
//     :
//     ;

// val_dic
//     :
//     ;

// val_tuple
//     :
//     ;

// val_bind
//     :
//     ;
