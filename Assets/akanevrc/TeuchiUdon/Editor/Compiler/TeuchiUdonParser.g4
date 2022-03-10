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
    : '{' (statement ';')* '}'               #UnitBlockExpr
    | '{' (statement ';')* expr '}'          #ValueBlockExpr
    | '(' expr ')'                           #ParenExpr
    | literal                                #LiteralExpr
    | thisLiteral                            #ThisLiteralExpr
    | identifier                             #EvalVarExpr
    | expr op=('.' | '?.') expr              #AccessExpr
    | expr '.' 'cast' '(' expr ')'           #CastExpr
    | expr '(' ')'                           #EvalUnitFuncExpr
    | expr '(' argExpr ')'                   #EvalSingleFuncExpr
    | expr '(' argExpr (',' argExpr)+ ')'    #EvalTupleFuncExpr
    | expr '[' expr ']'                      #EvalSingleKeyExpr
    | expr '[' expr (',' expr)+ ']'          #EvalTupleKeyExpr
    | 'nameof' '(' identifier ')'            #NameOfExpr
    | op=('+' | '-' | '!' | '~') expr        #PrefixExpr
    | expr op='..' expr                      #RangeExpr
    | expr op=('*' | '/' | '%') expr         #MultiplicationExpr
    | expr op=('+' | '-') expr               #AdditionExpr
    | expr op=('<<' | '>>') expr             #ShiftExpr
    | expr op=('<' | '>' | '<=' | '>=') expr #RelationExpr
    | expr op=('==' | '!=') expr             #EqualityExpr
    | expr op='&' expr                       #LogicalAndExpr
    | expr op='^' expr                       #LogicalXorExpr
    | expr op='|' expr                       #LogicalOrExpr
    | expr op='&&' expr                      #ConditionalAndExpr
    | expr op='||' expr                      #ConditionalOrExpr
    | expr op='??' expr                      #CoalescingExpr
    |<assoc=right> expr '?' expr ':' expr    #ConditionalExpr
    |<assoc=right> expr op='<-' expr         #AssignExpr
    | 'let' varBind 'in' expr                #LetInBindExpr
    | varDecl[false] '->' expr               #FuncExpr
    ;

argExpr
    returns [ArgExprResult result]
    : 'ref'? expr
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
