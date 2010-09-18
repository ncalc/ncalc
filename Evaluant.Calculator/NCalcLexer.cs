// $ANTLR 3.2 Sep 23, 2009 12:02:23 C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g 2009-11-11 17:56:42


using System;
using Antlr.Runtime;
using IList 		= System.Collections.IList;
using ArrayList 	= System.Collections.ArrayList;
using Stack 		= Antlr.Runtime.Collections.StackList;


public class NCalcLexer : Lexer {
    public const int T__29 = 29;
    public const int T__28 = 28;
    public const int T__27 = 27;
    public const int T__26 = 26;
    public const int T__25 = 25;
    public const int T__24 = 24;
    public const int LETTER = 12;
    public const int T__23 = 23;
    public const int T__22 = 22;
    public const int T__21 = 21;
    public const int T__20 = 20;
    public const int FLOAT = 5;
    public const int ID = 10;
    public const int EOF = -1;
    public const int HexDigit = 17;
    public const int T__19 = 19;
    public const int NAME = 11;
    public const int DIGIT = 13;
    public const int T__42 = 42;
    public const int INTEGER = 4;
    public const int E = 14;
    public const int T__43 = 43;
    public const int T__40 = 40;
    public const int T__41 = 41;
    public const int T__46 = 46;
    public const int T__47 = 47;
    public const int T__44 = 44;
    public const int T__45 = 45;
    public const int T__48 = 48;
    public const int DATETIME = 7;
    public const int TRUE = 8;
    public const int T__30 = 30;
    public const int T__31 = 31;
    public const int T__32 = 32;
    public const int WS = 18;
    public const int T__33 = 33;
    public const int T__34 = 34;
    public const int T__35 = 35;
    public const int T__36 = 36;
    public const int T__37 = 37;
    public const int T__38 = 38;
    public const int T__39 = 39;
    public const int UnicodeEscape = 16;
    public const int FALSE = 9;
    public const int EscapeSequence = 15;
    public const int STRING = 6;

    // delegates
    // delegators

    public NCalcLexer() 
    {
		InitializeCyclicDFAs();
    }
    public NCalcLexer(ICharStream input)
		: this(input, null) {
    }
    public NCalcLexer(ICharStream input, RecognizerSharedState state)
		: base(input, state) {
		InitializeCyclicDFAs(); 

    }
    
    override public string GrammarFileName
    {
    	get { return "C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g";} 
    }

    // $ANTLR start "T__19"
    public void mT__19() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__19;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:7:7: ( '?' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:7:9: '?'
            {
            	Match('?'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__19"

    // $ANTLR start "T__20"
    public void mT__20() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__20;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:8:7: ( ':' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:8:9: ':'
            {
            	Match(':'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__20"

    // $ANTLR start "T__21"
    public void mT__21() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__21;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:9:7: ( '||' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:9:9: '||'
            {
            	Match("||"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__21"

    // $ANTLR start "T__22"
    public void mT__22() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__22;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:10:7: ( 'or' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:10:9: 'or'
            {
            	Match("or"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__22"

    // $ANTLR start "T__23"
    public void mT__23() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__23;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:11:7: ( '&&' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:11:9: '&&'
            {
            	Match("&&"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__23"

    // $ANTLR start "T__24"
    public void mT__24() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__24;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:12:7: ( 'and' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:12:9: 'and'
            {
            	Match("and"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__24"

    // $ANTLR start "T__25"
    public void mT__25() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__25;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:13:7: ( '|' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:13:9: '|'
            {
            	Match('|'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__25"

    // $ANTLR start "T__26"
    public void mT__26() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__26;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:14:7: ( '^' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:14:9: '^'
            {
            	Match('^'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__26"

    // $ANTLR start "T__27"
    public void mT__27() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__27;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:15:7: ( '&' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:15:9: '&'
            {
            	Match('&'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__27"

    // $ANTLR start "T__28"
    public void mT__28() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__28;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:16:7: ( '==' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:16:9: '=='
            {
            	Match("=="); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__28"

    // $ANTLR start "T__29"
    public void mT__29() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__29;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:17:7: ( '=' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:17:9: '='
            {
            	Match('='); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__29"

    // $ANTLR start "T__30"
    public void mT__30() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__30;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:18:7: ( '!=' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:18:9: '!='
            {
            	Match("!="); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__30"

    // $ANTLR start "T__31"
    public void mT__31() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__31;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:19:7: ( '<>' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:19:9: '<>'
            {
            	Match("<>"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__31"

    // $ANTLR start "T__32"
    public void mT__32() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__32;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:20:7: ( '<' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:20:9: '<'
            {
            	Match('<'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__32"

    // $ANTLR start "T__33"
    public void mT__33() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__33;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:21:7: ( '<=' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:21:9: '<='
            {
            	Match("<="); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__33"

    // $ANTLR start "T__34"
    public void mT__34() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__34;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:22:7: ( '>' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:22:9: '>'
            {
            	Match('>'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__34"

    // $ANTLR start "T__35"
    public void mT__35() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__35;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:23:7: ( '>=' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:23:9: '>='
            {
            	Match(">="); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__35"

    // $ANTLR start "T__36"
    public void mT__36() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__36;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:24:7: ( '<<' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:24:9: '<<'
            {
            	Match("<<"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__36"

    // $ANTLR start "T__37"
    public void mT__37() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__37;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:25:7: ( '>>' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:25:9: '>>'
            {
            	Match(">>"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__37"

    // $ANTLR start "T__38"
    public void mT__38() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__38;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:26:7: ( '+' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:26:9: '+'
            {
            	Match('+'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__38"

    // $ANTLR start "T__39"
    public void mT__39() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__39;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:27:7: ( '-' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:27:9: '-'
            {
            	Match('-'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__39"

    // $ANTLR start "T__40"
    public void mT__40() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__40;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:28:7: ( '*' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:28:9: '*'
            {
            	Match('*'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__40"

    // $ANTLR start "T__41"
    public void mT__41() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__41;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:29:7: ( '/' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:29:9: '/'
            {
            	Match('/'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__41"

    // $ANTLR start "T__42"
    public void mT__42() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__42;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:30:7: ( '%' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:30:9: '%'
            {
            	Match('%'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__42"

    // $ANTLR start "T__43"
    public void mT__43() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__43;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:31:7: ( '!' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:31:9: '!'
            {
            	Match('!'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__43"

    // $ANTLR start "T__44"
    public void mT__44() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__44;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:32:7: ( 'not' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:32:9: 'not'
            {
            	Match("not"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__44"

    // $ANTLR start "T__45"
    public void mT__45() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__45;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:33:7: ( '~' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:33:9: '~'
            {
            	Match('~'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__45"

    // $ANTLR start "T__46"
    public void mT__46() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__46;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:34:7: ( '(' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:34:9: '('
            {
            	Match('('); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__46"

    // $ANTLR start "T__47"
    public void mT__47() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__47;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:35:7: ( ')' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:35:9: ')'
            {
            	Match(')'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__47"

    // $ANTLR start "T__48"
    public void mT__48() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = T__48;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:36:7: ( ',' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:36:9: ','
            {
            	Match(','); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "T__48"

    // $ANTLR start "TRUE"
    public void mTRUE() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = TRUE;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:237:2: ( 'true' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:237:4: 'true'
            {
            	Match("true"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "TRUE"

    // $ANTLR start "FALSE"
    public void mFALSE() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = FALSE;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:241:2: ( 'false' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:241:4: 'false'
            {
            	Match("false"); 


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "FALSE"

    // $ANTLR start "ID"
    public void mID() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = ID;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:245:2: ( LETTER ( LETTER | DIGIT )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:245:5: LETTER ( LETTER | DIGIT )*
            {
            	mLETTER(); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:245:12: ( LETTER | DIGIT )*
            	do 
            	{
            	    int alt1 = 2;
            	    int LA1_0 = input.LA(1);

            	    if ( ((LA1_0 >= '0' && LA1_0 <= '9') || (LA1_0 >= 'A' && LA1_0 <= 'Z') || LA1_0 == '_' || (LA1_0 >= 'a' && LA1_0 <= 'z')) )
            	    {
            	        alt1 = 1;
            	    }


            	    switch (alt1) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:
            			    {
            			    	if ( (input.LA(1) >= '0' && input.LA(1) <= '9') || (input.LA(1) >= 'A' && input.LA(1) <= 'Z') || input.LA(1) == '_' || (input.LA(1) >= 'a' && input.LA(1) <= 'z') ) 
            			    	{
            			    	    input.Consume();

            			    	}
            			    	else 
            			    	{
            			    	    MismatchedSetException mse = new MismatchedSetException(null,input);
            			    	    Recover(mse);
            			    	    throw mse;}


            			    }
            			    break;

            			default:
            			    goto loop1;
            	    }
            	} while (true);

            	loop1:
            		;	// Stops C# compiler whining that label 'loop1' has no statements


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "ID"

    // $ANTLR start "INTEGER"
    public void mINTEGER() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = INTEGER;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:249:2: ( ( DIGIT )+ )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:249:4: ( DIGIT )+
            {
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:249:4: ( DIGIT )+
            	int cnt2 = 0;
            	do 
            	{
            	    int alt2 = 2;
            	    int LA2_0 = input.LA(1);

            	    if ( ((LA2_0 >= '0' && LA2_0 <= '9')) )
            	    {
            	        alt2 = 1;
            	    }


            	    switch (alt2) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:249:4: DIGIT
            			    {
            			    	mDIGIT(); 

            			    }
            			    break;

            			default:
            			    if ( cnt2 >= 1 ) goto loop2;
            		            EarlyExitException eee2 =
            		                new EarlyExitException(2, input);
            		            throw eee2;
            	    }
            	    cnt2++;
            	} while (true);

            	loop2:
            		;	// Stops C# compiler whining that label 'loop2' has no statements


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "INTEGER"

    // $ANTLR start "FLOAT"
    public void mFLOAT() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = FLOAT;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:253:2: ( ( DIGIT )* '.' ( DIGIT )+ ( E )? | ( DIGIT )+ E )
            int alt7 = 2;
            alt7 = dfa7.Predict(input);
            switch (alt7) 
            {
                case 1 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:253:4: ( DIGIT )* '.' ( DIGIT )+ ( E )?
                    {
                    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:253:4: ( DIGIT )*
                    	do 
                    	{
                    	    int alt3 = 2;
                    	    int LA3_0 = input.LA(1);

                    	    if ( ((LA3_0 >= '0' && LA3_0 <= '9')) )
                    	    {
                    	        alt3 = 1;
                    	    }


                    	    switch (alt3) 
                    		{
                    			case 1 :
                    			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:253:4: DIGIT
                    			    {
                    			    	mDIGIT(); 

                    			    }
                    			    break;

                    			default:
                    			    goto loop3;
                    	    }
                    	} while (true);

                    	loop3:
                    		;	// Stops C# compiler whining that label 'loop3' has no statements

                    	Match('.'); 
                    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:253:15: ( DIGIT )+
                    	int cnt4 = 0;
                    	do 
                    	{
                    	    int alt4 = 2;
                    	    int LA4_0 = input.LA(1);

                    	    if ( ((LA4_0 >= '0' && LA4_0 <= '9')) )
                    	    {
                    	        alt4 = 1;
                    	    }


                    	    switch (alt4) 
                    		{
                    			case 1 :
                    			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:253:15: DIGIT
                    			    {
                    			    	mDIGIT(); 

                    			    }
                    			    break;

                    			default:
                    			    if ( cnt4 >= 1 ) goto loop4;
                    		            EarlyExitException eee4 =
                    		                new EarlyExitException(4, input);
                    		            throw eee4;
                    	    }
                    	    cnt4++;
                    	} while (true);

                    	loop4:
                    		;	// Stops C# compiler whining that label 'loop4' has no statements

                    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:253:22: ( E )?
                    	int alt5 = 2;
                    	int LA5_0 = input.LA(1);

                    	if ( (LA5_0 == 'E' || LA5_0 == 'e') )
                    	{
                    	    alt5 = 1;
                    	}
                    	switch (alt5) 
                    	{
                    	    case 1 :
                    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:253:22: E
                    	        {
                    	        	mE(); 

                    	        }
                    	        break;

                    	}


                    }
                    break;
                case 2 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:254:4: ( DIGIT )+ E
                    {
                    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:254:4: ( DIGIT )+
                    	int cnt6 = 0;
                    	do 
                    	{
                    	    int alt6 = 2;
                    	    int LA6_0 = input.LA(1);

                    	    if ( ((LA6_0 >= '0' && LA6_0 <= '9')) )
                    	    {
                    	        alt6 = 1;
                    	    }


                    	    switch (alt6) 
                    		{
                    			case 1 :
                    			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:254:4: DIGIT
                    			    {
                    			    	mDIGIT(); 

                    			    }
                    			    break;

                    			default:
                    			    if ( cnt6 >= 1 ) goto loop6;
                    		            EarlyExitException eee6 =
                    		                new EarlyExitException(6, input);
                    		            throw eee6;
                    	    }
                    	    cnt6++;
                    	} while (true);

                    	loop6:
                    		;	// Stops C# compiler whining that label 'loop6' has no statements

                    	mE(); 

                    }
                    break;

            }
            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "FLOAT"

    // $ANTLR start "STRING"
    public void mSTRING() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = STRING;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:258:6: ( '\\'' ( EscapeSequence | ( options {greedy=false; } : ~ ( '\\u0000' .. '\\u001f' | '\\\\' | '\\'' ) ) )* '\\'' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:258:10: '\\'' ( EscapeSequence | ( options {greedy=false; } : ~ ( '\\u0000' .. '\\u001f' | '\\\\' | '\\'' ) ) )* '\\''
            {
            	Match('\''); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:258:15: ( EscapeSequence | ( options {greedy=false; } : ~ ( '\\u0000' .. '\\u001f' | '\\\\' | '\\'' ) ) )*
            	do 
            	{
            	    int alt8 = 3;
            	    int LA8_0 = input.LA(1);

            	    if ( (LA8_0 == '\\') )
            	    {
            	        alt8 = 1;
            	    }
            	    else if ( ((LA8_0 >= ' ' && LA8_0 <= '&') || (LA8_0 >= '(' && LA8_0 <= '[') || (LA8_0 >= ']' && LA8_0 <= '\uFFFF')) )
            	    {
            	        alt8 = 2;
            	    }


            	    switch (alt8) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:258:17: EscapeSequence
            			    {
            			    	mEscapeSequence(); 

            			    }
            			    break;
            			case 2 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:258:34: ( options {greedy=false; } : ~ ( '\\u0000' .. '\\u001f' | '\\\\' | '\\'' ) )
            			    {
            			    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:258:34: ( options {greedy=false; } : ~ ( '\\u0000' .. '\\u001f' | '\\\\' | '\\'' ) )
            			    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:258:61: ~ ( '\\u0000' .. '\\u001f' | '\\\\' | '\\'' )
            			    	{
            			    		if ( (input.LA(1) >= ' ' && input.LA(1) <= '&') || (input.LA(1) >= '(' && input.LA(1) <= '[') || (input.LA(1) >= ']' && input.LA(1) <= '\uFFFF') ) 
            			    		{
            			    		    input.Consume();

            			    		}
            			    		else 
            			    		{
            			    		    MismatchedSetException mse = new MismatchedSetException(null,input);
            			    		    Recover(mse);
            			    		    throw mse;}


            			    	}


            			    }
            			    break;

            			default:
            			    goto loop8;
            	    }
            	} while (true);

            	loop8:
            		;	// Stops C# compiler whining that label 'loop8' has no statements

            	Match('\''); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "STRING"

    // $ANTLR start "DATETIME"
    public void mDATETIME() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = DATETIME;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:262:3: ( '#' ( options {greedy=false; } : (~ ( '#' ) )* ) '#' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:262:5: '#' ( options {greedy=false; } : (~ ( '#' ) )* ) '#'
            {
            	Match('#'); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:262:9: ( options {greedy=false; } : (~ ( '#' ) )* )
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:262:36: (~ ( '#' ) )*
            	{
            		// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:262:36: (~ ( '#' ) )*
            		do 
            		{
            		    int alt9 = 2;
            		    int LA9_0 = input.LA(1);

            		    if ( ((LA9_0 >= '\u0000' && LA9_0 <= '\"') || (LA9_0 >= '$' && LA9_0 <= '\uFFFF')) )
            		    {
            		        alt9 = 1;
            		    }


            		    switch (alt9) 
            			{
            				case 1 :
            				    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:262:36: ~ ( '#' )
            				    {
            				    	if ( (input.LA(1) >= '\u0000' && input.LA(1) <= '\"') || (input.LA(1) >= '$' && input.LA(1) <= '\uFFFF') ) 
            				    	{
            				    	    input.Consume();

            				    	}
            				    	else 
            				    	{
            				    	    MismatchedSetException mse = new MismatchedSetException(null,input);
            				    	    Recover(mse);
            				    	    throw mse;}


            				    }
            				    break;

            				default:
            				    goto loop9;
            		    }
            		} while (true);

            		loop9:
            			;	// Stops C# compiler whining that label 'loop9' has no statements


            	}

            	Match('#'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "DATETIME"

    // $ANTLR start "NAME"
    public void mNAME() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = NAME;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:265:6: ( '[' ( options {greedy=false; } : (~ ( ']' ) )* ) ']' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:265:8: '[' ( options {greedy=false; } : (~ ( ']' ) )* ) ']'
            {
            	Match('['); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:265:12: ( options {greedy=false; } : (~ ( ']' ) )* )
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:265:39: (~ ( ']' ) )*
            	{
            		// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:265:39: (~ ( ']' ) )*
            		do 
            		{
            		    int alt10 = 2;
            		    int LA10_0 = input.LA(1);

            		    if ( ((LA10_0 >= '\u0000' && LA10_0 <= '\\') || (LA10_0 >= '^' && LA10_0 <= '\uFFFF')) )
            		    {
            		        alt10 = 1;
            		    }


            		    switch (alt10) 
            			{
            				case 1 :
            				    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:265:39: ~ ( ']' )
            				    {
            				    	if ( (input.LA(1) >= '\u0000' && input.LA(1) <= '\\') || (input.LA(1) >= '^' && input.LA(1) <= '\uFFFF') ) 
            				    	{
            				    	    input.Consume();

            				    	}
            				    	else 
            				    	{
            				    	    MismatchedSetException mse = new MismatchedSetException(null,input);
            				    	    Recover(mse);
            				    	    throw mse;}


            				    }
            				    break;

            				default:
            				    goto loop10;
            		    }
            		} while (true);

            		loop10:
            			;	// Stops C# compiler whining that label 'loop10' has no statements


            	}

            	Match(']'); 

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "NAME"

    // $ANTLR start "E"
    public void mE() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = E;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:268:3: ( ( 'E' | 'e' ) ( '+' | '-' )? ( DIGIT )+ )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:268:5: ( 'E' | 'e' ) ( '+' | '-' )? ( DIGIT )+
            {
            	if ( input.LA(1) == 'E' || input.LA(1) == 'e' ) 
            	{
            	    input.Consume();

            	}
            	else 
            	{
            	    MismatchedSetException mse = new MismatchedSetException(null,input);
            	    Recover(mse);
            	    throw mse;}

            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:268:15: ( '+' | '-' )?
            	int alt11 = 2;
            	int LA11_0 = input.LA(1);

            	if ( (LA11_0 == '+' || LA11_0 == '-') )
            	{
            	    alt11 = 1;
            	}
            	switch (alt11) 
            	{
            	    case 1 :
            	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:
            	        {
            	        	if ( input.LA(1) == '+' || input.LA(1) == '-' ) 
            	        	{
            	        	    input.Consume();

            	        	}
            	        	else 
            	        	{
            	        	    MismatchedSetException mse = new MismatchedSetException(null,input);
            	        	    Recover(mse);
            	        	    throw mse;}


            	        }
            	        break;

            	}

            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:268:26: ( DIGIT )+
            	int cnt12 = 0;
            	do 
            	{
            	    int alt12 = 2;
            	    int LA12_0 = input.LA(1);

            	    if ( ((LA12_0 >= '0' && LA12_0 <= '9')) )
            	    {
            	        alt12 = 1;
            	    }


            	    switch (alt12) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:268:26: DIGIT
            			    {
            			    	mDIGIT(); 

            			    }
            			    break;

            			default:
            			    if ( cnt12 >= 1 ) goto loop12;
            		            EarlyExitException eee12 =
            		                new EarlyExitException(12, input);
            		            throw eee12;
            	    }
            	    cnt12++;
            	} while (true);

            	loop12:
            		;	// Stops C# compiler whining that label 'loop12' has no statements


            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "E"

    // $ANTLR start "LETTER"
    public void mLETTER() // throws RecognitionException [2]
    {
    		try
    		{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:272:2: ( 'a' .. 'z' | 'A' .. 'Z' | '_' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:
            {
            	if ( (input.LA(1) >= 'A' && input.LA(1) <= 'Z') || input.LA(1) == '_' || (input.LA(1) >= 'a' && input.LA(1) <= 'z') ) 
            	{
            	    input.Consume();

            	}
            	else 
            	{
            	    MismatchedSetException mse = new MismatchedSetException(null,input);
            	    Recover(mse);
            	    throw mse;}


            }

        }
        finally 
    	{
        }
    }
    // $ANTLR end "LETTER"

    // $ANTLR start "DIGIT"
    public void mDIGIT() // throws RecognitionException [2]
    {
    		try
    		{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:278:2: ( '0' .. '9' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:278:4: '0' .. '9'
            {
            	MatchRange('0','9'); 

            }

        }
        finally 
    	{
        }
    }
    // $ANTLR end "DIGIT"

    // $ANTLR start "EscapeSequence"
    public void mEscapeSequence() // throws RecognitionException [2]
    {
    		try
    		{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:282:2: ( '\\\\' ( 'n' | 'r' | 't' | '\\'' | '\\\\' | UnicodeEscape ) )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:282:4: '\\\\' ( 'n' | 'r' | 't' | '\\'' | '\\\\' | UnicodeEscape )
            {
            	Match('\\'); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:283:4: ( 'n' | 'r' | 't' | '\\'' | '\\\\' | UnicodeEscape )
            	int alt13 = 6;
            	switch ( input.LA(1) ) 
            	{
            	case 'n':
            		{
            	    alt13 = 1;
            	    }
            	    break;
            	case 'r':
            		{
            	    alt13 = 2;
            	    }
            	    break;
            	case 't':
            		{
            	    alt13 = 3;
            	    }
            	    break;
            	case '\'':
            		{
            	    alt13 = 4;
            	    }
            	    break;
            	case '\\':
            		{
            	    alt13 = 5;
            	    }
            	    break;
            	case 'u':
            		{
            	    alt13 = 6;
            	    }
            	    break;
            		default:
            		    NoViableAltException nvae_d13s0 =
            		        new NoViableAltException("", 13, 0, input);

            		    throw nvae_d13s0;
            	}

            	switch (alt13) 
            	{
            	    case 1 :
            	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:284:5: 'n'
            	        {
            	        	Match('n'); 

            	        }
            	        break;
            	    case 2 :
            	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:285:4: 'r'
            	        {
            	        	Match('r'); 

            	        }
            	        break;
            	    case 3 :
            	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:286:4: 't'
            	        {
            	        	Match('t'); 

            	        }
            	        break;
            	    case 4 :
            	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:287:4: '\\''
            	        {
            	        	Match('\''); 

            	        }
            	        break;
            	    case 5 :
            	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:288:4: '\\\\'
            	        {
            	        	Match('\\'); 

            	        }
            	        break;
            	    case 6 :
            	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:289:4: UnicodeEscape
            	        {
            	        	mUnicodeEscape(); 

            	        }
            	        break;

            	}


            }

        }
        finally 
    	{
        }
    }
    // $ANTLR end "EscapeSequence"

    // $ANTLR start "HexDigit"
    public void mHexDigit() // throws RecognitionException [2]
    {
    		try
    		{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:294:2: ( ( '0' .. '9' | 'a' .. 'f' | 'A' .. 'F' ) )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:294:5: ( '0' .. '9' | 'a' .. 'f' | 'A' .. 'F' )
            {
            	if ( (input.LA(1) >= '0' && input.LA(1) <= '9') || (input.LA(1) >= 'A' && input.LA(1) <= 'F') || (input.LA(1) >= 'a' && input.LA(1) <= 'f') ) 
            	{
            	    input.Consume();

            	}
            	else 
            	{
            	    MismatchedSetException mse = new MismatchedSetException(null,input);
            	    Recover(mse);
            	    throw mse;}


            }

        }
        finally 
    	{
        }
    }
    // $ANTLR end "HexDigit"

    // $ANTLR start "UnicodeEscape"
    public void mUnicodeEscape() // throws RecognitionException [2]
    {
    		try
    		{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:298:6: ( 'u' HexDigit HexDigit HexDigit HexDigit )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:298:12: 'u' HexDigit HexDigit HexDigit HexDigit
            {
            	Match('u'); 
            	mHexDigit(); 
            	mHexDigit(); 
            	mHexDigit(); 
            	mHexDigit(); 

            }

        }
        finally 
    	{
        }
    }
    // $ANTLR end "UnicodeEscape"

    // $ANTLR start "WS"
    public void mWS() // throws RecognitionException [2]
    {
    		try
    		{
            int _type = WS;
    	int _channel = DEFAULT_TOKEN_CHANNEL;
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:302:4: ( ( ' ' | '\\r' | '\\t' | '\\u000C' | '\\n' ) )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:302:7: ( ' ' | '\\r' | '\\t' | '\\u000C' | '\\n' )
            {
            	if ( (input.LA(1) >= '\t' && input.LA(1) <= '\n') || (input.LA(1) >= '\f' && input.LA(1) <= '\r') || input.LA(1) == ' ' ) 
            	{
            	    input.Consume();

            	}
            	else 
            	{
            	    MismatchedSetException mse = new MismatchedSetException(null,input);
            	    Recover(mse);
            	    throw mse;}

            	_channel=HIDDEN;

            }

            state.type = _type;
            state.channel = _channel;
        }
        finally 
    	{
        }
    }
    // $ANTLR end "WS"

    override public void mTokens() // throws RecognitionException 
    {
        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:8: ( T__19 | T__20 | T__21 | T__22 | T__23 | T__24 | T__25 | T__26 | T__27 | T__28 | T__29 | T__30 | T__31 | T__32 | T__33 | T__34 | T__35 | T__36 | T__37 | T__38 | T__39 | T__40 | T__41 | T__42 | T__43 | T__44 | T__45 | T__46 | T__47 | T__48 | TRUE | FALSE | ID | INTEGER | FLOAT | STRING | DATETIME | NAME | E | WS )
        int alt14 = 40;
        alt14 = dfa14.Predict(input);
        switch (alt14) 
        {
            case 1 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:10: T__19
                {
                	mT__19(); 

                }
                break;
            case 2 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:16: T__20
                {
                	mT__20(); 

                }
                break;
            case 3 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:22: T__21
                {
                	mT__21(); 

                }
                break;
            case 4 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:28: T__22
                {
                	mT__22(); 

                }
                break;
            case 5 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:34: T__23
                {
                	mT__23(); 

                }
                break;
            case 6 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:40: T__24
                {
                	mT__24(); 

                }
                break;
            case 7 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:46: T__25
                {
                	mT__25(); 

                }
                break;
            case 8 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:52: T__26
                {
                	mT__26(); 

                }
                break;
            case 9 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:58: T__27
                {
                	mT__27(); 

                }
                break;
            case 10 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:64: T__28
                {
                	mT__28(); 

                }
                break;
            case 11 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:70: T__29
                {
                	mT__29(); 

                }
                break;
            case 12 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:76: T__30
                {
                	mT__30(); 

                }
                break;
            case 13 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:82: T__31
                {
                	mT__31(); 

                }
                break;
            case 14 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:88: T__32
                {
                	mT__32(); 

                }
                break;
            case 15 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:94: T__33
                {
                	mT__33(); 

                }
                break;
            case 16 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:100: T__34
                {
                	mT__34(); 

                }
                break;
            case 17 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:106: T__35
                {
                	mT__35(); 

                }
                break;
            case 18 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:112: T__36
                {
                	mT__36(); 

                }
                break;
            case 19 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:118: T__37
                {
                	mT__37(); 

                }
                break;
            case 20 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:124: T__38
                {
                	mT__38(); 

                }
                break;
            case 21 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:130: T__39
                {
                	mT__39(); 

                }
                break;
            case 22 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:136: T__40
                {
                	mT__40(); 

                }
                break;
            case 23 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:142: T__41
                {
                	mT__41(); 

                }
                break;
            case 24 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:148: T__42
                {
                	mT__42(); 

                }
                break;
            case 25 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:154: T__43
                {
                	mT__43(); 

                }
                break;
            case 26 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:160: T__44
                {
                	mT__44(); 

                }
                break;
            case 27 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:166: T__45
                {
                	mT__45(); 

                }
                break;
            case 28 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:172: T__46
                {
                	mT__46(); 

                }
                break;
            case 29 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:178: T__47
                {
                	mT__47(); 

                }
                break;
            case 30 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:184: T__48
                {
                	mT__48(); 

                }
                break;
            case 31 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:190: TRUE
                {
                	mTRUE(); 

                }
                break;
            case 32 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:195: FALSE
                {
                	mFALSE(); 

                }
                break;
            case 33 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:201: ID
                {
                	mID(); 

                }
                break;
            case 34 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:204: INTEGER
                {
                	mINTEGER(); 

                }
                break;
            case 35 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:212: FLOAT
                {
                	mFLOAT(); 

                }
                break;
            case 36 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:218: STRING
                {
                	mSTRING(); 

                }
                break;
            case 37 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:225: DATETIME
                {
                	mDATETIME(); 

                }
                break;
            case 38 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:234: NAME
                {
                	mNAME(); 

                }
                break;
            case 39 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:239: E
                {
                	mE(); 

                }
                break;
            case 40 :
                // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:1:241: WS
                {
                	mWS(); 

                }
                break;

        }

    }


    protected DFA7 dfa7;
    protected DFA14 dfa14;
	private void InitializeCyclicDFAs()
	{
	    this.dfa7 = new DFA7(this);
	    this.dfa14 = new DFA14(this);


	}

    const string DFA7_eotS =
        "\x04\uffff";
    const string DFA7_eofS =
        "\x04\uffff";
    const string DFA7_minS =
        "\x02\x2e\x02\uffff";
    const string DFA7_maxS =
        "\x01\x39\x01\x65\x02\uffff";
    const string DFA7_acceptS =
        "\x02\uffff\x01\x01\x01\x02";
    const string DFA7_specialS =
        "\x04\uffff}>";
    static readonly string[] DFA7_transitionS = {
            "\x01\x02\x01\uffff\x0a\x01",
            "\x01\x02\x01\uffff\x0a\x01\x0b\uffff\x01\x03\x1f\uffff\x01"+
            "\x03",
            "",
            ""
    };

    static readonly short[] DFA7_eot = DFA.UnpackEncodedString(DFA7_eotS);
    static readonly short[] DFA7_eof = DFA.UnpackEncodedString(DFA7_eofS);
    static readonly char[] DFA7_min = DFA.UnpackEncodedStringToUnsignedChars(DFA7_minS);
    static readonly char[] DFA7_max = DFA.UnpackEncodedStringToUnsignedChars(DFA7_maxS);
    static readonly short[] DFA7_accept = DFA.UnpackEncodedString(DFA7_acceptS);
    static readonly short[] DFA7_special = DFA.UnpackEncodedString(DFA7_specialS);
    static readonly short[][] DFA7_transition = DFA.UnpackEncodedStringArray(DFA7_transitionS);

    protected class DFA7 : DFA
    {
        public DFA7(BaseRecognizer recognizer)
        {
            this.recognizer = recognizer;
            this.decisionNumber = 7;
            this.eot = DFA7_eot;
            this.eof = DFA7_eof;
            this.min = DFA7_min;
            this.max = DFA7_max;
            this.accept = DFA7_accept;
            this.special = DFA7_special;
            this.transition = DFA7_transition;

        }

        override public string Description
        {
            get { return "252:1: FLOAT : ( ( DIGIT )* '.' ( DIGIT )+ ( E )? | ( DIGIT )+ E );"; }
        }

    }

    const string DFA14_eotS =
        "\x03\uffff\x01\x21\x01\x1e\x01\x24\x01\x1e\x01\uffff\x01\x27\x01"+
        "\x29\x01\x2d\x01\x30\x05\uffff\x01\x1e\x04\uffff\x03\x1e\x01\x36"+
        "\x08\uffff\x01\x37\x02\uffff\x01\x1e\x0b\uffff\x03\x1e\x01\uffff"+
        "\x01\x1e\x02\uffff\x01\x3c\x01\x3d\x02\x1e\x02\uffff\x01\x40\x01"+
        "\x1e\x01\uffff\x01\x42\x01\uffff";
    const string DFA14_eofS =
        "\x43\uffff";
    const string DFA14_minS =
        "\x01\x09\x02\uffff\x01\x7c\x01\x72\x01\x26\x01\x6e\x01\uffff\x02"+
        "\x3d\x01\x3c\x01\x3d\x05\uffff\x01\x6f\x04\uffff\x01\x72\x01\x61"+
        "\x01\x2b\x01\x2e\x08\uffff\x01\x30\x02\uffff\x01\x64\x0b\uffff\x01"+
        "\x74\x01\x75\x01\x6c\x01\uffff\x01\x30\x02\uffff\x02\x30\x01\x65"+
        "\x01\x73\x02\uffff\x01\x30\x01\x65\x01\uffff\x01\x30\x01\uffff";
    const string DFA14_maxS =
        "\x01\x7e\x02\uffff\x01\x7c\x01\x72\x01\x26\x01\x6e\x01\uffff\x02"+
        "\x3d\x02\x3e\x05\uffff\x01\x6f\x04\uffff\x01\x72\x01\x61\x01\x39"+
        "\x01\x65\x08\uffff\x01\x7a\x02\uffff\x01\x64\x0b\uffff\x01\x74\x01"+
        "\x75\x01\x6c\x01\uffff\x01\x39\x02\uffff\x02\x7a\x01\x65\x01\x73"+
        "\x02\uffff\x01\x7a\x01\x65\x01\uffff\x01\x7a\x01\uffff";
    const string DFA14_acceptS =
        "\x01\uffff\x01\x01\x01\x02\x04\uffff\x01\x08\x04\uffff\x01\x14"+
        "\x01\x15\x01\x16\x01\x17\x01\x18\x01\uffff\x01\x1b\x01\x1c\x01\x1d"+
        "\x01\x1e\x04\uffff\x01\x23\x01\x24\x01\x25\x01\x26\x01\x21\x01\x28"+
        "\x01\x03\x01\x07\x01\uffff\x01\x05\x01\x09\x01\uffff\x01\x0a\x01"+
        "\x0b\x01\x0c\x01\x19\x01\x0d\x01\x0f\x01\x12\x01\x0e\x01\x11\x01"+
        "\x13\x01\x10\x03\uffff\x01\x27\x01\uffff\x01\x22\x01\x04\x04\uffff"+
        "\x01\x06\x01\x1a\x02\uffff\x01\x1f\x01\uffff\x01\x20";
    const string DFA14_specialS =
        "\x43\uffff}>";
    static readonly string[] DFA14_transitionS = {
            "\x02\x1f\x01\uffff\x02\x1f\x12\uffff\x01\x1f\x01\x09\x01\uffff"+
            "\x01\x1c\x01\uffff\x01\x10\x01\x05\x01\x1b\x01\x13\x01\x14\x01"+
            "\x0e\x01\x0c\x01\x15\x01\x0d\x01\x1a\x01\x0f\x0a\x19\x01\x02"+
            "\x01\uffff\x01\x0a\x01\x08\x01\x0b\x01\x01\x01\uffff\x04\x1e"+
            "\x01\x18\x15\x1e\x01\x1d\x02\uffff\x01\x07\x01\x1e\x01\uffff"+
            "\x01\x06\x03\x1e\x01\x18\x01\x17\x07\x1e\x01\x11\x01\x04\x04"+
            "\x1e\x01\x16\x06\x1e\x01\uffff\x01\x03\x01\uffff\x01\x12",
            "",
            "",
            "\x01\x20",
            "\x01\x22",
            "\x01\x23",
            "\x01\x25",
            "",
            "\x01\x26",
            "\x01\x28",
            "\x01\x2c\x01\x2b\x01\x2a",
            "\x01\x2e\x01\x2f",
            "",
            "",
            "",
            "",
            "",
            "\x01\x31",
            "",
            "",
            "",
            "",
            "\x01\x32",
            "\x01\x33",
            "\x01\x34\x01\uffff\x01\x34\x02\uffff\x0a\x35",
            "\x01\x1a\x01\uffff\x0a\x19\x0b\uffff\x01\x1a\x1f\uffff\x01"+
            "\x1a",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "\x0a\x1e\x07\uffff\x1a\x1e\x04\uffff\x01\x1e\x01\uffff\x1a"+
            "\x1e",
            "",
            "",
            "\x01\x38",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "\x01\x39",
            "\x01\x3a",
            "\x01\x3b",
            "",
            "\x0a\x35",
            "",
            "",
            "\x0a\x1e\x07\uffff\x1a\x1e\x04\uffff\x01\x1e\x01\uffff\x1a"+
            "\x1e",
            "\x0a\x1e\x07\uffff\x1a\x1e\x04\uffff\x01\x1e\x01\uffff\x1a"+
            "\x1e",
            "\x01\x3e",
            "\x01\x3f",
            "",
            "",
            "\x0a\x1e\x07\uffff\x1a\x1e\x04\uffff\x01\x1e\x01\uffff\x1a"+
            "\x1e",
            "\x01\x41",
            "",
            "\x0a\x1e\x07\uffff\x1a\x1e\x04\uffff\x01\x1e\x01\uffff\x1a"+
            "\x1e",
            ""
    };

    static readonly short[] DFA14_eot = DFA.UnpackEncodedString(DFA14_eotS);
    static readonly short[] DFA14_eof = DFA.UnpackEncodedString(DFA14_eofS);
    static readonly char[] DFA14_min = DFA.UnpackEncodedStringToUnsignedChars(DFA14_minS);
    static readonly char[] DFA14_max = DFA.UnpackEncodedStringToUnsignedChars(DFA14_maxS);
    static readonly short[] DFA14_accept = DFA.UnpackEncodedString(DFA14_acceptS);
    static readonly short[] DFA14_special = DFA.UnpackEncodedString(DFA14_specialS);
    static readonly short[][] DFA14_transition = DFA.UnpackEncodedStringArray(DFA14_transitionS);

    protected class DFA14 : DFA
    {
        public DFA14(BaseRecognizer recognizer)
        {
            this.recognizer = recognizer;
            this.decisionNumber = 14;
            this.eot = DFA14_eot;
            this.eof = DFA14_eof;
            this.min = DFA14_min;
            this.max = DFA14_max;
            this.accept = DFA14_accept;
            this.special = DFA14_special;
            this.transition = DFA14_transition;

        }

        override public string Description
        {
            get { return "1:1: Tokens : ( T__19 | T__20 | T__21 | T__22 | T__23 | T__24 | T__25 | T__26 | T__27 | T__28 | T__29 | T__30 | T__31 | T__32 | T__33 | T__34 | T__35 | T__36 | T__37 | T__38 | T__39 | T__40 | T__41 | T__42 | T__43 | T__44 | T__45 | T__46 | T__47 | T__48 | TRUE | FALSE | ID | INTEGER | FLOAT | STRING | DATETIME | NAME | E | WS );"; }
        }

    }

 
    
}
