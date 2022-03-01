parser grammar TeuchiUdonParser;

options
{
    language=CSharp;
    superClass=TeuchiUdonBaseParser;
    tokenVocab=TeuchiUdonLexer;
}

@parser::header
{
    #pragma warning disable 3021
}

target
    : body EOF
    | EOF
    ;

body
    returns [BodyResult result]
    : topStatement+
    ;

topStatement
    returns [TopStatementResult result]
    : varAttr* varBind ';' #VarBindTopStatement
    | exprAttr* expr ';'   #ExprTopStatement
    ;

varAttr
    returns [VarAttrResult result]
    : '@init' '(' identifier ')' #InitVarAttr
    | '@export'                  #ExportVarAttr
    | '@sync'                    #SyncVarAttr
    | '@linear'                  #LinearVarAttr
    | '@smooth'                  #SmoothVarAttr
    ;

exprAttr
    returns [ExprAttrResult result]
    : '@init' '(' identifier ')' #InitExprAttr
    ;

varBind
    returns [VarBindResult result, int tableIndex]
    : 'mut'? varDecl[true] '=' expr
    ;

varDecl[bool isActual]
    returns [VarDeclResult result]
    : '(' ')'                                                      #UnitVarDecl
    | identifier (':' expr)?                                       #SingleVarDecl
    | '(' identifier (':' expr)? (',' identifier (':' expr)?)* ')' #TupleVarDecl
    ;

identifier
    returns [IdentifierResult result]
    : IDENTIFIER
    ;

statement
    returns [StatementResult result, int tableIndex]
    : 'return'        #ReturnUnitStatement
    | 'return'   expr #ReturnValueStatement
    | 'continue'      #ContinueUnitStatement
    | 'continue' expr #ContinueValueStatement
    | 'break'         #BreakUnitStatement
    | 'break'    expr #BreakValueStatement
    | 'let' varBind   #LetBindStatement
    | expr            #ExprStatement
    ;

expr
    returns [ExprResult result, int tableIndex]
    : '{' (statement ';')* '}'                      #UnitBlockExpr
    | '{' (statement ';')* expr '}'                 #ValueBlockExpr
    | '(' expr ')'                                  #ParenExpr
    | literal                                       #LiteralExpr
    | thisLiteral                                   #ThisLiteralExpr
    | identifier                                    #EvalVarExpr
    | expr op=('.' | '?.') expr                     #AccessExpr
    | expr '(' ')'                                  #EvalUnitFuncExpr
    | expr '(' expr ')'                             #EvalSingleFuncExpr
    | expr '(' expr (',' expr)+ ')'                 #EvalTupleFuncExpr
    | 'nameof' '(' identifier ')'                   #NameOfExpr
    | expr op=('++' | '--')                         #PostfixExpr
    | op=('+' | '-' | '!' | '~' | '++' | '--') expr #PrefixExpr
    | expr op='..' expr                             #RangeExpr
    | expr op=('*' | '/' | '%') expr                #MultiplicationExpr
    | expr op=('+' | '-') expr                      #AdditionExpr
    | expr op=('<<' | '>>') expr                    #ShiftExpr
    | expr op=('<' | '>' | '<=' | '>=') expr        #RelationExpr
    | expr op=('==' | '!=') expr                    #EqualityExpr
    | expr op='&' expr                              #LogicalAndExpr
    | expr op='^' expr                              #LogicalXorExpr
    | expr op='|' expr                              #LogicalOrExpr
    | expr op='&&' expr                             #ConditionalAndExpr
    | expr op='||' expr                             #ConditionalOrExpr
    | expr op='??' expr                             #CoalescingExpr
    |<assoc=right> expr '?' expr ':' expr           #ConditionalExpr
    |<assoc=right> expr op=('=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '&&=' | '||=' | '<<=' | '>>=' | '??=') expr #AssignExpr
    | 'let' varBind 'in' expr                       #LetInBindExpr
    | varDecl[false] '->' expr                      #FuncExpr
    ;

literal
    returns [LiteralResult result]
    : '(' ')'             #UnitLiteral
    | NULL_LITERAL        #NullLiteral
    | BOOL_LITERAL        #BoolLiteral
    | INTEGER_LITERAL     #IntegerLiteral
    | HEX_INTEGER_LITERAL #HexIntegerLiteral
    | BIN_INTEGER_LITERAL #BinIntegerLiteral
    | REAL_LITERAL        #RealLiteral
    | CHARACTER_LITERAL   #CharacterLiteral
    | REGULAR_STRING      #RegularString
    | VERBATIUM_STRING    #VervatiumString
    ;

thisLiteral
    returns [ThisResult result]
    : THIS_LITERAL
    ;

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
