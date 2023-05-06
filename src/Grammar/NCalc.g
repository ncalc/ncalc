grammar NCalc;

options
{
	language=CSharp3;
}

@header {
using System.Globalization;
using NCalc.Domain;
}

@members {
private const char BS = '\\';
private static NumberFormatInfo numberFormatInfo = new NumberFormatInfo();

private string extractString(string text) {
    
    StringBuilder sb = new StringBuilder(text);
    int startIndex = 1; // Skip initial quote
    int slashIndex = -1;

    while ((slashIndex = sb.ToString().IndexOf(BS, startIndex)) != -1)
    {
        char escapeType = sb[slashIndex + 1];
        switch (escapeType)
        {
            case 'u':
              string hcode = String.Concat(sb[slashIndex+4], sb[slashIndex+5]);
              string lcode = String.Concat(sb[slashIndex+2], sb[slashIndex+3]);
              char unicodeChar = Encoding.Unicode.GetChars(new byte[] { System.Convert.ToByte(hcode, 16), System.Convert.ToByte(lcode, 16)} )[0];
              sb.Remove(slashIndex, 6).Insert(slashIndex, unicodeChar); 
              break;
            case 'n': sb.Remove(slashIndex, 2).Insert(slashIndex, '\n'); break;
            case 'r': sb.Remove(slashIndex, 2).Insert(slashIndex, '\r'); break;
            case 't': sb.Remove(slashIndex, 2).Insert(slashIndex, '\t'); break;
            case '\'': sb.Remove(slashIndex, 2).Insert(slashIndex, '\''); break;
            case '\\': sb.Remove(slashIndex, 2).Insert(slashIndex, '\\'); break;
            default: throw new RecognitionException(null, CharStreams.fromString("Invalid escape sequence: \\" + escapeType));
        }

        startIndex = slashIndex + 1;

    }

    sb.Remove(0, 1);
    sb.Remove(sb.Length - 1, 1);

    return sb.ToString();
}

}

@init {
    numberFormatInfo.NumberDecimalSeparator = ".";
}

@lexer::namespace { NCalc }
@parser::namespace { NCalc }

ncalcExpression returns [LogicalExpression retValue]
	: logicalExpression EOF { $retValue = $logicalExpression.retValue; }
	;

logicalExpression returns [LogicalExpression retValue]
	:	left=conditionalExpression { $retValue = $left.retValue; } ( '?' middle=conditionalExpression ':' right=conditionalExpression { $retValue = new TernaryExpression($left.retValue, $middle.retValue, $right.retValue); })? 
	;

conditionalExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=booleanAndExpression { $retValue = $left.retValue; } (
			('||' | OR) { type = BinaryExpressionType.Or; } 
			right=conditionalExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;
		
booleanAndExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=bitwiseOrExpression { $retValue = $left.retValue; } (
			('&&' | AND) { type = BinaryExpressionType.And; } 
			right=bitwiseOrExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;

bitwiseOrExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=bitwiseXOrExpression { $retValue = $left.retValue; } (
			'|' { type = BinaryExpressionType.BitwiseOr; } 
			right=bitwiseOrExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;
		
bitwiseXOrExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=bitwiseAndExpression { $retValue = $left.retValue; } (
			'^' { type = BinaryExpressionType.BitwiseXOr; } 
			right=bitwiseAndExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;

bitwiseAndExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=equalityExpression { $retValue = $left.retValue; } (
			'&' { type = BinaryExpressionType.BitwiseAnd; } 
			right=equalityExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;
		
equalityExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=relationalExpression { $retValue = $left.retValue; } (
			( ('==' | '=' ) { type = BinaryExpressionType.Equal; } 
			| ('!=' | '<>' ) { type = BinaryExpressionType.NotEqual; } ) 
			right=relationalExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;
	
relationalExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=shiftExpression { $retValue = $left.retValue; } (
			( '<' { type = BinaryExpressionType.Lesser; } 
			| '<=' { type = BinaryExpressionType.LesserOrEqual; }  
			| '>' { type = BinaryExpressionType.Greater; } 
			| '>=' { type = BinaryExpressionType.GreaterOrEqual; } ) 
			right=shiftExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;

shiftExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	: left=additiveExpression { $retValue = $left.retValue; } (
			( '<<' { type = BinaryExpressionType.LeftShift; } 
			| '>>' { type = BinaryExpressionType.RightShift; }  )
			right=additiveExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;

additiveExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=multiplicativeExpression { $retValue = $left.retValue; } (
			( '+' { type = BinaryExpressionType.Plus; } 
			| '-' { type = BinaryExpressionType.Minus; } ) 
			right=multiplicativeExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;

multiplicativeExpression returns [LogicalExpression retValue]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=unaryExpression { $retValue = $left.retValue; } (
			( '*' { type = BinaryExpressionType.Times; } 
			| '/' { type = BinaryExpressionType.Div; } 
			| '%' { type = BinaryExpressionType.Modulo; } ) 
			right=unaryExpression { $retValue = new BinaryExpression(type, $retValue, $right.retValue); } 
			)* 
	;
	
unaryExpression returns [LogicalExpression retValue]
	:	exponentialExpression { $retValue = $exponentialExpression.retValue; }
    |	('!' | NOT) exponentialExpression { $retValue = new UnaryExpression(UnaryExpressionType.Not, $exponentialExpression.retValue); }
    |	('~') exponentialExpression { $retValue = new UnaryExpression(UnaryExpressionType.BitwiseNot, $exponentialExpression.retValue); }
    |	'-' exponentialExpression { $retValue = new UnaryExpression(UnaryExpressionType.Negate, $exponentialExpression.retValue); }
    |	'+' exponentialExpression { $retValue = new UnaryExpression(UnaryExpressionType.Positive, $exponentialExpression.retValue); }
   	;
		
exponentialExpression returns [LogicalExpression retValue]
	: 	left=primaryExpression { $retValue = $left.retValue; } (
			'**' right=unaryExpression { $retValue = new BinaryExpression(BinaryExpressionType.Exponentiation, $retValue, $right.retValue); }
			)*
	;

primaryExpression returns [LogicalExpression retValue]
	:	'(' logicalExpression ')' 	{ $retValue = $logicalExpression.retValue; }
	|	expr=value		{ $retValue = $expr.retValue; }
	|	identifier {$retValue = (LogicalExpression) $identifier.retValue; } (arguments {$retValue = new Function($identifier.retValue, ($arguments.retValue).ToArray()); })?
	;

value returns [ValueExpression retValue]
	: 	INTEGER		{ try { $retValue = new ValueExpression(int.Parse($INTEGER.text)); } catch(System.OverflowException) { $retValue = new ValueExpression(long.Parse($INTEGER.text)); } }
	|	FLOAT		{ $retValue = new ValueExpression(double.Parse($FLOAT.text, NumberStyles.Float, numberFormatInfo)); }
	|	STRING		{ $retValue = new ValueExpression(extractString($STRING.text)); }
	| 	DATETIME	{ $retValue = new ValueExpression(DateTime.Parse($DATETIME.text.Substring(1, $DATETIME.text.Length-2))); }
	|	TRUE		{ $retValue = new ValueExpression(true); }
	|	FALSE		{ $retValue = new ValueExpression(false); }
	;

identifier returns[Identifier retValue]
	: 	ID { $retValue = new Identifier($ID.text); }
	| 	NAME { $retValue = new Identifier($NAME.text.Substring(1, $NAME.text.Length-2)); }
	;

expressionList returns [List<LogicalExpression> retValue]
@init {
List<LogicalExpression> expressions = new List<LogicalExpression>();
}
	:	first=logicalExpression {expressions.Add($first.retValue);}  ( ',' follow=logicalExpression {expressions.Add($follow.retValue);})* 
	{ $retValue = expressions; }
	;
	
arguments returns [List<LogicalExpression> retValue]
@init {
$retValue = new List<LogicalExpression>();
}
	:	'(' ( expressionList {$retValue = $expressionList.retValue;} )? ')' 
	;			

TRUE:	T R U E ;
FALSE:	F A L S E ;
AND:	A N D ;
OR:		O R ;
NOT:	N O T ;

ID 
	: 	LETTER (LETTER | DIGIT)*
	;

INTEGER
	:	DIGIT+
	;

FLOAT 
	:	DIGIT* '.' DIGIT+ EXPONENT?
	|	DIGIT+ '.' DIGIT* EXPONENT?
	|	DIGIT+ EXPONENT
	;

STRING
    	:  	'\'' ( EscapeSequence | ( ~('\u0000'..'\u001f' | '\\' | '\'' ) ).*? )* '\''
    	;

DATETIME 
 	:	'#' ( ~('#')*) '#'
        ;

NAME	:	'[' ( ~(']')*) ']'
	;
	
EXPONENT
	:	('E'|'e') ('+'|'-')? DIGIT+ 
	;	
	
fragment LETTER
	:	'a'..'z'
	|	'A'..'Z'
	|	'_'
	;

fragment DIGIT
	:	'0'..'9'
	;
	
fragment EscapeSequence 
	:	'\\'
  	(	
  		'n' 
	|	'r' 
	|	't'
	|	'\'' 
	|	'\\'
	|	UnicodeEscape
	)
  ;

fragment HexDigit 
	: 	('0'..'9'|'a'..'f'|'A'..'F') ;


fragment UnicodeEscape
    	:    	'u' HexDigit HexDigit HexDigit HexDigit 
    	;

/* Ignore white spaces */	
WS : (' '|'\r'|'\t'|'\u000C'|'\n') -> skip;

/* Allow case-insensitive operators by constructing them out of fragments.
 * Solution adapted from https://stackoverflow.com/a/22160240
 */
fragment A: 'a' | 'A';
fragment B: 'b' | 'B';
fragment C: 'c' | 'C';
fragment D: 'd' | 'D';
fragment E: 'e' | 'E';
fragment F: 'f' | 'F';
fragment G: 'g' | 'G';
fragment H: 'h' | 'H';
fragment I: 'i' | 'I';
fragment J: 'j' | 'J';
fragment K: 'k' | 'K';
fragment L: 'l' | 'L';
fragment M: 'm' | 'M';
fragment N: 'n' | 'N';
fragment O: 'o' | 'O';
fragment P: 'p' | 'P';
fragment Q: 'q' | 'Q';
fragment R: 'r' | 'R';
fragment S: 's' | 'S';
fragment T: 't' | 'T';
fragment U: 'u' | 'U';
fragment V: 'v' | 'V';
fragment W: 'w' | 'W';
fragment X: 'x' | 'X';
fragment Y: 'y' | 'Y';
fragment Z: 'z' | 'Z';