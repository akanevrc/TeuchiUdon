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
    | expr ';'             #ExprTopStatement
    ;

varAttr
    returns [VarAttrResult result]
    : '@public' #PublicVarAttr
    | '@sync'   #SyncVarAttr
    | '@linear' #LinearVarAttr
    | '@smooth' #SmoothVarAttr
    ;

varBind
    returns [VarBindResult result, int tableIndex]
    : 'mut'? varDecl[true] '=' expr
    ;

varDecl[bool isActual]
    returns [VarDeclResult result]
    : '(' ')'                                  #UnitVarDecl
    | qualifiedVar                             #SingleVarDecl
    | '(' qualifiedVar (',' qualifiedVar)* ')' #TupleVarDecl
    ;

qualifiedVar
    returns [QualifiedVarResult result]
    : identifier (':' expr)?
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
    : '{' (statement ';')* '}'                                                  #UnitBlockExpr
    | '{' (statement ';')* expr '}'                                             #ValueBlockExpr
    | '(' expr ')'                                                              #ParenExpr
    | '(' expr (',' expr)+ ')'                                                  #TupleExpr
    | '[|' '|]'                                                                 #EmptyArrayCtorExpr
    | '[|' iterExpr '|]'                                                        #ArrayCtorExpr
    | '[' ']'                                                                   #EmptyListCtorExpr
    | '[' iterExpr (',' iterExpr)* ']'                                          #ListCtorExpr
    | literal                                                                   #LiteralExpr
    | thisLiteral                                                               #ThisLiteralExpr
    | identifier                                                                #EvalVarExpr
    | expr op=('.' | '?.') expr                                                 #AccessExpr
    | expr '.' 'cast' '(' expr ')'                                              #CastExpr
    | expr '(' ')'                                                              #EvalUnitFuncExpr
    | expr '(' argExpr ')'                                                      #EvalSingleFuncExpr
    | expr '(' argExpr (',' argExpr)+ ')'                                       #EvalTupleFuncExpr
    | expr '(' '...' expr ')'                                                   #EvalSpreadFuncExpr
    | expr '[' expr ']'                                                         #EvalSingleKeyExpr
    | expr '[' expr (',' expr)+ ']'                                             #EvalTupleKeyExpr
    | op=('+' | '-' | '!' | '~') expr                                           #PrefixExpr
    | expr op=('*' | '/' | '%') expr                                            #MultiplicationExpr
    | expr op=('+' | '-') expr                                                  #AdditionExpr
    | expr op=('<<' | '>>') expr                                                #ShiftExpr
    | expr op=('<' | '>' | '<=' | '>=') expr                                    #RelationExpr
    | expr op=('==' | '!=') expr                                                #EqualityExpr
    | expr op='&' expr                                                          #LogicalAndExpr
    | expr op='^' expr                                                          #LogicalXorExpr
    | expr op='|' expr                                                          #LogicalOrExpr
    | expr op='&&' expr                                                         #ConditionalAndExpr
    | expr op='||' expr                                                         #ConditionalOrExpr
    | expr op='??' expr                                                         #CoalescingExpr
    | expr '|>' expr                                                            #RightPipelineExpr
    |<assoc=right> expr '<|' expr                                               #LeftPipelineExpr
    |<assoc=right> expr '?' expr ':' expr                                       #ConditionalExpr
    |<assoc=right> expr op='<-' expr                                            #AssignExpr
    | 'let' varBind 'in' expr                                                   #LetInBindExpr
    |<assoc=right> 'if' expr 'then' expr                                        #IfExpr
    |<assoc=right> 'if' expr 'then' expr ('elif' expr 'then' expr)+             #IfElifExpr
    |<assoc=right> 'if' expr 'then' expr 'else' expr                            #IfElseExpr
    |<assoc=right> 'if' expr 'then' expr ('elif' expr 'then' expr)+ 'else' expr #IfElifElseExpr
    |<assoc=right> 'while' expr 'do' expr                                       #WhileExpr
    |'for' forBind (',' forBind)* 'do' expr                                     #ForExpr
    |'loop' expr                                                                #LoopExpr
    | varDecl[false] '->' expr                                                  #FuncExpr
    ;

iterExpr
    returns [IterExprResult result]
    : elementExpr (',' elementExpr)* #ElementsIterExpr
    | expr '..' expr                 #RangeIterExpr
    | expr '..' expr '..' expr       #SteppedRangeIterExpr
    | '...' expr                     #SpreadIterExpr
    ;

elementExpr
    returns [ElementExprResult result]
    : expr
    ;

argExpr
    returns [ArgExprResult result]
    : 'ref'? expr
    ;

forBind
    returns [ForBindResult result]
    : 'let' expr '<-' forIterExpr #LetForBind
    | expr '<-' forIterExpr       #AssignForBind
    ;

forIterExpr
    returns [ForIterExprResult result]
    : expr '..' expr           #RangeForIterExpr
    | expr '..' expr '..' expr #SteppedRangeForIterExpr
    | expr                     #SpreadForIterExpr
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
