grammar NCalc;

options
{
	output=AST;
	ASTLabelType=CommonTree;
	language=CSharp3;
}

@header {
using System;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
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
            default: throw new RecognitionException("Unvalid escape sequence: \\" + escapeType);
        }

        startIndex = slashIndex + 1;

    }

    sb.Remove(0, 1);
    sb.Remove(sb.Length - 1, 1);

    return sb.ToString();
}

public List<string> Errors { get; private set; }

public override void DisplayRecognitionError(String[] tokenNames, RecognitionException e) {
    
    base.DisplayRecognitionError(tokenNames, e);
    
    if(Errors == null)
    {
    	Errors = new List<string>();
    }
    
    String hdr = GetErrorHeader(e);
    String msg = GetErrorMessage(e, tokenNames);
    Errors.Add(msg + " at " + hdr);
}
}

@init {
    numberFormatInfo.NumberDecimalSeparator = ".";
}

ncalcExpression returns [LogicalExpression value]
	: logicalExpression EOF! {$value = $logicalExpression.value; }
	;

logicalExpression returns [LogicalExpression value]
	:	left=conditionalExpression { $value = $left.value; } ( '?' middle=conditionalExpression ':' right=conditionalExpression { $value = new TernaryExpression($left.value, $middle.value, $right.value); })? 
	;

conditionalExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=booleanAndExpression { $value = $left.value; } (
			('||' | 'or') { type = BinaryExpressionType.Or; } 
			right=conditionalExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;
		
booleanAndExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=bitwiseOrExpression { $value = $left.value; } (
			('&&' | 'and') { type = BinaryExpressionType.And; } 
			right=bitwiseOrExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;

bitwiseOrExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=bitwiseXOrExpression { $value = $left.value; } (
			'|' { type = BinaryExpressionType.BitwiseOr; } 
			right=bitwiseOrExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;
		
bitwiseXOrExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=bitwiseAndExpression { $value = $left.value; } (
			'^' { type = BinaryExpressionType.BitwiseXOr; } 
			right=bitwiseAndExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;

bitwiseAndExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=equalityExpression { $value = $left.value; } (
			'&' { type = BinaryExpressionType.BitwiseAnd; } 
			right=equalityExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;
		
equalityExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=relationalExpression { $value = $left.value; } (
			( ('==' | '=' ) { type = BinaryExpressionType.Equal; } 
			| ('!=' | '<>' ) { type = BinaryExpressionType.NotEqual; } ) 
			right=relationalExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;
	
relationalExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=shiftExpression { $value = $left.value; } (
			( '<' { type = BinaryExpressionType.Lesser; } 
			| '<=' { type = BinaryExpressionType.LesserOrEqual; }  
			| '>' { type = BinaryExpressionType.Greater; } 
			| '>=' { type = BinaryExpressionType.GreaterOrEqual; } ) 
			right=shiftExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;

shiftExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	: left=additiveExpression { $value = $left.value; } (
			( '<<' { type = BinaryExpressionType.LeftShift; } 
			| '>>' { type = BinaryExpressionType.RightShift; }  )
			right=additiveExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;

additiveExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=multiplicativeExpression { $value = $left.value; } (
			( '+' { type = BinaryExpressionType.Plus; } 
			| '-' { type = BinaryExpressionType.Minus; } ) 
			right=multiplicativeExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;

multiplicativeExpression returns [LogicalExpression value]
@init {
BinaryExpressionType type = BinaryExpressionType.Unknown;
}
	:	left=unaryExpression { $value = $left.value); } (
			( '*' { type = BinaryExpressionType.Times; } 
			| '/' { type = BinaryExpressionType.Div; } 
			| '%' { type = BinaryExpressionType.Modulo; } ) 
			right=unaryExpression { $value = new BinaryExpression(type, $value, $right.value); } 
			)* 
	;

	
unaryExpression returns [LogicalExpression value]
	:	primaryExpression { $value = $primaryExpression.value; }
    	|	('!' | 'not') primaryExpression { $value = new UnaryExpression(UnaryExpressionType.Not, $primaryExpression.value); }
    	|	('~') primaryExpression { $value = new UnaryExpression(UnaryExpressionType.BitwiseNot, $primaryExpression.value); }
    	|	'-' primaryExpression { $value = new UnaryExpression(UnaryExpressionType.Negate, $primaryExpression.value); }
   	;
		
primaryExpression returns [LogicalExpression value]
	:	'(' logicalExpression ')' 	{ $value = $logicalExpression.value; }
	|	expr=value		{ $value = $expr.value; }
	|	identifier {$value = (LogicalExpression) $identifier.value; } (arguments {$value = new Function($identifier.value, ($arguments.value).ToArray()); })?
	;

value returns [ValueExpression value]
	: 	INTEGER		{ try { $value = new ValueExpression(int.Parse($INTEGER.text)); } catch(System.OverflowException) { $value = new ValueExpression(long.Parse($INTEGER.text)); } }
	|	FLOAT		{ $value = new ValueExpression(double.Parse($FLOAT.text, NumberStyles.Float, numberFormatInfo)); }
	|	STRING		{ $value = new ValueExpression(extractString($STRING.text)); }
	| 	DATETIME	{ $value = new ValueExpression(DateTime.Parse($DATETIME.text.Substring(1, $DATETIME.text.Length-2))); }
	|	TRUE		{ $value = new ValueExpression(true); }
	|	FALSE		{ $value = new ValueExpression(false); }
	;

identifier returns[Identifier value]
	: 	ID { $value = new Identifier($ID.text); }
	| 	NAME { $value = new Identifier($NAME.text.Substring(1, $NAME.text.Length-2)); }
	;

expressionList returns [List<LogicalExpression> value]
@init {
List<LogicalExpression> expressions = new List<LogicalExpression>();
}
	:	first=logicalExpression {expressions.Add($first.value);}  ( ',' follow=logicalExpression {expressions.Add($follow.value);})* 
	{ $value = expressions; }
	;
	
arguments returns [List<LogicalExpression> value]
@init {
$value = new List<LogicalExpression>();
}
	:	'(' ( expressionList {$value = $expressionList.value;} )? ')' 
	;			

TRUE
	:	'true'
	;

FALSE
	:	'false'
	;
			
ID 
	: 	LETTER (LETTER | DIGIT)*
	;

INTEGER
	:	DIGIT+
	;

FLOAT 
	:	DIGIT* '.' DIGIT+ E?
	|	DIGIT+ E
	;

STRING
    	:  	'\'' ( EscapeSequence | (options {greedy=false;} : ~('\u0000'..'\u001f' | '\\' | '\'' ) ) )* '\''
    	;

DATETIME 
 	:	'#' (options {greedy=false;} : ~('#')*) '#'
        ;

NAME	:	'[' (options {greedy=false;} : ~(']')*) ']'
	;
	
E	:	('E'|'e') ('+'|'-')? DIGIT+ 
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
WS	:  (' '|'\r'|'\t'|'\u000C'|'\n') {$channel=Hidden;}
	;
