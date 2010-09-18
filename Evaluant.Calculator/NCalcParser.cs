// $ANTLR 3.2 Sep 23, 2009 12:02:23 C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g 2009-11-11 17:56:42


using System.Text;
using System.Globalization;
using System.Collections.Generic;
using NCalc.Domain;


using System;
using Antlr.Runtime;
using IList 		= System.Collections.IList;
using ArrayList 	= System.Collections.ArrayList;
using Stack 		= Antlr.Runtime.Collections.StackList;



using Antlr.Runtime.Tree;

public class NCalcParser : Parser 
{
    public static readonly string[] tokenNames = new string[] 
	{
        "<invalid>", 
		"<EOR>", 
		"<DOWN>", 
		"<UP>", 
		"INTEGER", 
		"FLOAT", 
		"STRING", 
		"DATETIME", 
		"TRUE", 
		"FALSE", 
		"ID", 
		"NAME", 
		"LETTER", 
		"DIGIT", 
		"E", 
		"EscapeSequence", 
		"UnicodeEscape", 
		"HexDigit", 
		"WS", 
		"'?'", 
		"':'", 
		"'||'", 
		"'or'", 
		"'&&'", 
		"'and'", 
		"'|'", 
		"'^'", 
		"'&'", 
		"'=='", 
		"'='", 
		"'!='", 
		"'<>'", 
		"'<'", 
		"'<='", 
		"'>'", 
		"'>='", 
		"'<<'", 
		"'>>'", 
		"'+'", 
		"'-'", 
		"'*'", 
		"'/'", 
		"'%'", 
		"'!'", 
		"'not'", 
		"'~'", 
		"'('", 
		"')'", 
		"','"
    };

    public const int T__29 = 29;
    public const int T__28 = 28;
    public const int T__27 = 27;
    public const int T__26 = 26;
    public const int T__25 = 25;
    public const int T__24 = 24;
    public const int T__23 = 23;
    public const int LETTER = 12;
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



        public NCalcParser(ITokenStream input)
    		: this(input, new RecognizerSharedState()) {
        }

        public NCalcParser(ITokenStream input, RecognizerSharedState state)
    		: base(input, state) {
            InitializeCyclicDFAs();

             
       }
        
    protected ITreeAdaptor adaptor = new CommonTreeAdaptor();

    public ITreeAdaptor TreeAdaptor
    {
        get { return this.adaptor; }
        set {
    	this.adaptor = value;
    	}
    }

    override public string[] TokenNames {
		get { return NCalcParser.tokenNames; }
    }

    override public string GrammarFileName {
		get { return "C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g"; }
    }


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


    public class ncalcExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "ncalcExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:77:1: ncalcExpression returns [LogicalExpression value] : logicalExpression EOF ;
    public NCalcParser.ncalcExpression_return ncalcExpression() // throws RecognitionException [1]
    {   
        NCalcParser.ncalcExpression_return retval = new NCalcParser.ncalcExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken EOF2 = null;
        NCalcParser.logicalExpression_return logicalExpression1 = null;


        CommonTree EOF2_tree=null;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:78:2: ( logicalExpression EOF )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:78:4: logicalExpression EOF
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_logicalExpression_in_ncalcExpression56);
            	logicalExpression1 = logicalExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, logicalExpression1.Tree);
            	EOF2=(IToken)Match(input,EOF,FOLLOW_EOF_in_ncalcExpression58); 
            	retval.value =  ((logicalExpression1 != null) ? logicalExpression1.value : null); 

            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "ncalcExpression"

    public class logicalExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "logicalExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:81:1: logicalExpression returns [LogicalExpression value] : left= conditionalExpression ( '?' middle= conditionalExpression ':' right= conditionalExpression )? ;
    public NCalcParser.logicalExpression_return logicalExpression() // throws RecognitionException [1]
    {   
        NCalcParser.logicalExpression_return retval = new NCalcParser.logicalExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal3 = null;
        IToken char_literal4 = null;
        NCalcParser.conditionalExpression_return left = null;

        NCalcParser.conditionalExpression_return middle = null;

        NCalcParser.conditionalExpression_return right = null;


        CommonTree char_literal3_tree=null;
        CommonTree char_literal4_tree=null;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:82:2: (left= conditionalExpression ( '?' middle= conditionalExpression ':' right= conditionalExpression )? )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:82:4: left= conditionalExpression ( '?' middle= conditionalExpression ':' right= conditionalExpression )?
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_conditionalExpression_in_logicalExpression78);
            	left = conditionalExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:82:57: ( '?' middle= conditionalExpression ':' right= conditionalExpression )?
            	int alt1 = 2;
            	int LA1_0 = input.LA(1);

            	if ( (LA1_0 == 19) )
            	{
            	    alt1 = 1;
            	}
            	switch (alt1) 
            	{
            	    case 1 :
            	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:82:59: '?' middle= conditionalExpression ':' right= conditionalExpression
            	        {
            	        	char_literal3=(IToken)Match(input,19,FOLLOW_19_in_logicalExpression84); 
            	        		char_literal3_tree = (CommonTree)adaptor.Create(char_literal3);
            	        		adaptor.AddChild(root_0, char_literal3_tree);

            	        	PushFollow(FOLLOW_conditionalExpression_in_logicalExpression88);
            	        	middle = conditionalExpression();
            	        	state.followingStackPointer--;

            	        	adaptor.AddChild(root_0, middle.Tree);
            	        	char_literal4=(IToken)Match(input,20,FOLLOW_20_in_logicalExpression90); 
            	        		char_literal4_tree = (CommonTree)adaptor.Create(char_literal4);
            	        		adaptor.AddChild(root_0, char_literal4_tree);

            	        	PushFollow(FOLLOW_conditionalExpression_in_logicalExpression94);
            	        	right = conditionalExpression();
            	        	state.followingStackPointer--;

            	        	adaptor.AddChild(root_0, right.Tree);
            	        	 retval.value =  new TernaryExpression(((left != null) ? left.value : null), ((middle != null) ? middle.value : null), ((right != null) ? right.value : null)); 

            	        }
            	        break;

            	}


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "logicalExpression"

    public class conditionalExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "conditionalExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:85:1: conditionalExpression returns [LogicalExpression value] : left= booleanAndExpression ( ( '||' | 'or' ) right= conditionalExpression )* ;
    public NCalcParser.conditionalExpression_return conditionalExpression() // throws RecognitionException [1]
    {   
        NCalcParser.conditionalExpression_return retval = new NCalcParser.conditionalExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken set5 = null;
        NCalcParser.booleanAndExpression_return left = null;

        NCalcParser.conditionalExpression_return right = null;


        CommonTree set5_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:89:2: (left= booleanAndExpression ( ( '||' | 'or' ) right= conditionalExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:89:4: left= booleanAndExpression ( ( '||' | 'or' ) right= conditionalExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_booleanAndExpression_in_conditionalExpression121);
            	left = booleanAndExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:89:56: ( ( '||' | 'or' ) right= conditionalExpression )*
            	do 
            	{
            	    int alt2 = 2;
            	    int LA2_0 = input.LA(1);

            	    if ( ((LA2_0 >= 21 && LA2_0 <= 22)) )
            	    {
            	        alt2 = 1;
            	    }


            	    switch (alt2) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:90:4: ( '||' | 'or' ) right= conditionalExpression
            			    {
            			    	set5 = (IToken)input.LT(1);
            			    	if ( (input.LA(1) >= 21 && input.LA(1) <= 22) ) 
            			    	{
            			    	    input.Consume();
            			    	    adaptor.AddChild(root_0, (CommonTree)adaptor.Create(set5));
            			    	    state.errorRecovery = false;
            			    	}
            			    	else 
            			    	{
            			    	    MismatchedSetException mse = new MismatchedSetException(null,input);
            			    	    throw mse;
            			    	}

            			    	 type = BinaryExpressionType.Or; 
            			    	PushFollow(FOLLOW_conditionalExpression_in_conditionalExpression146);
            			    	right = conditionalExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop2;
            	    }
            	} while (true);

            	loop2:
            		;	// Stops C# compiler whining that label 'loop2' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "conditionalExpression"

    public class booleanAndExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "booleanAndExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:95:1: booleanAndExpression returns [LogicalExpression value] : left= bitwiseOrExpression ( ( '&&' | 'and' ) right= bitwiseOrExpression )* ;
    public NCalcParser.booleanAndExpression_return booleanAndExpression() // throws RecognitionException [1]
    {   
        NCalcParser.booleanAndExpression_return retval = new NCalcParser.booleanAndExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken set6 = null;
        NCalcParser.bitwiseOrExpression_return left = null;

        NCalcParser.bitwiseOrExpression_return right = null;


        CommonTree set6_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:99:2: (left= bitwiseOrExpression ( ( '&&' | 'and' ) right= bitwiseOrExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:99:4: left= bitwiseOrExpression ( ( '&&' | 'and' ) right= bitwiseOrExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_bitwiseOrExpression_in_booleanAndExpression180);
            	left = bitwiseOrExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:99:55: ( ( '&&' | 'and' ) right= bitwiseOrExpression )*
            	do 
            	{
            	    int alt3 = 2;
            	    int LA3_0 = input.LA(1);

            	    if ( ((LA3_0 >= 23 && LA3_0 <= 24)) )
            	    {
            	        alt3 = 1;
            	    }


            	    switch (alt3) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:100:4: ( '&&' | 'and' ) right= bitwiseOrExpression
            			    {
            			    	set6 = (IToken)input.LT(1);
            			    	if ( (input.LA(1) >= 23 && input.LA(1) <= 24) ) 
            			    	{
            			    	    input.Consume();
            			    	    adaptor.AddChild(root_0, (CommonTree)adaptor.Create(set6));
            			    	    state.errorRecovery = false;
            			    	}
            			    	else 
            			    	{
            			    	    MismatchedSetException mse = new MismatchedSetException(null,input);
            			    	    throw mse;
            			    	}

            			    	 type = BinaryExpressionType.And; 
            			    	PushFollow(FOLLOW_bitwiseOrExpression_in_booleanAndExpression205);
            			    	right = bitwiseOrExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop3;
            	    }
            	} while (true);

            	loop3:
            		;	// Stops C# compiler whining that label 'loop3' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "booleanAndExpression"

    public class bitwiseOrExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "bitwiseOrExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:105:1: bitwiseOrExpression returns [LogicalExpression value] : left= bitwiseXOrExpression ( '|' right= bitwiseOrExpression )* ;
    public NCalcParser.bitwiseOrExpression_return bitwiseOrExpression() // throws RecognitionException [1]
    {   
        NCalcParser.bitwiseOrExpression_return retval = new NCalcParser.bitwiseOrExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal7 = null;
        NCalcParser.bitwiseXOrExpression_return left = null;

        NCalcParser.bitwiseOrExpression_return right = null;


        CommonTree char_literal7_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:109:2: (left= bitwiseXOrExpression ( '|' right= bitwiseOrExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:109:4: left= bitwiseXOrExpression ( '|' right= bitwiseOrExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_bitwiseXOrExpression_in_bitwiseOrExpression237);
            	left = bitwiseXOrExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:109:56: ( '|' right= bitwiseOrExpression )*
            	do 
            	{
            	    int alt4 = 2;
            	    int LA4_0 = input.LA(1);

            	    if ( (LA4_0 == 25) )
            	    {
            	        alt4 = 1;
            	    }


            	    switch (alt4) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:110:4: '|' right= bitwiseOrExpression
            			    {
            			    	char_literal7=(IToken)Match(input,25,FOLLOW_25_in_bitwiseOrExpression246); 
            			    		char_literal7_tree = (CommonTree)adaptor.Create(char_literal7);
            			    		adaptor.AddChild(root_0, char_literal7_tree);

            			    	 type = BinaryExpressionType.BitwiseOr; 
            			    	PushFollow(FOLLOW_bitwiseOrExpression_in_bitwiseOrExpression256);
            			    	right = bitwiseOrExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop4;
            	    }
            	} while (true);

            	loop4:
            		;	// Stops C# compiler whining that label 'loop4' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "bitwiseOrExpression"

    public class bitwiseXOrExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "bitwiseXOrExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:115:1: bitwiseXOrExpression returns [LogicalExpression value] : left= bitwiseAndExpression ( '^' right= bitwiseAndExpression )* ;
    public NCalcParser.bitwiseXOrExpression_return bitwiseXOrExpression() // throws RecognitionException [1]
    {   
        NCalcParser.bitwiseXOrExpression_return retval = new NCalcParser.bitwiseXOrExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal8 = null;
        NCalcParser.bitwiseAndExpression_return left = null;

        NCalcParser.bitwiseAndExpression_return right = null;


        CommonTree char_literal8_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:119:2: (left= bitwiseAndExpression ( '^' right= bitwiseAndExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:119:4: left= bitwiseAndExpression ( '^' right= bitwiseAndExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_bitwiseAndExpression_in_bitwiseXOrExpression290);
            	left = bitwiseAndExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:119:56: ( '^' right= bitwiseAndExpression )*
            	do 
            	{
            	    int alt5 = 2;
            	    int LA5_0 = input.LA(1);

            	    if ( (LA5_0 == 26) )
            	    {
            	        alt5 = 1;
            	    }


            	    switch (alt5) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:120:4: '^' right= bitwiseAndExpression
            			    {
            			    	char_literal8=(IToken)Match(input,26,FOLLOW_26_in_bitwiseXOrExpression299); 
            			    		char_literal8_tree = (CommonTree)adaptor.Create(char_literal8);
            			    		adaptor.AddChild(root_0, char_literal8_tree);

            			    	 type = BinaryExpressionType.BitwiseXOr; 
            			    	PushFollow(FOLLOW_bitwiseAndExpression_in_bitwiseXOrExpression309);
            			    	right = bitwiseAndExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop5;
            	    }
            	} while (true);

            	loop5:
            		;	// Stops C# compiler whining that label 'loop5' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "bitwiseXOrExpression"

    public class bitwiseAndExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "bitwiseAndExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:125:1: bitwiseAndExpression returns [LogicalExpression value] : left= equalityExpression ( '&' right= equalityExpression )* ;
    public NCalcParser.bitwiseAndExpression_return bitwiseAndExpression() // throws RecognitionException [1]
    {   
        NCalcParser.bitwiseAndExpression_return retval = new NCalcParser.bitwiseAndExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal9 = null;
        NCalcParser.equalityExpression_return left = null;

        NCalcParser.equalityExpression_return right = null;


        CommonTree char_literal9_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:129:2: (left= equalityExpression ( '&' right= equalityExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:129:4: left= equalityExpression ( '&' right= equalityExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_equalityExpression_in_bitwiseAndExpression341);
            	left = equalityExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:129:54: ( '&' right= equalityExpression )*
            	do 
            	{
            	    int alt6 = 2;
            	    int LA6_0 = input.LA(1);

            	    if ( (LA6_0 == 27) )
            	    {
            	        alt6 = 1;
            	    }


            	    switch (alt6) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:130:4: '&' right= equalityExpression
            			    {
            			    	char_literal9=(IToken)Match(input,27,FOLLOW_27_in_bitwiseAndExpression350); 
            			    		char_literal9_tree = (CommonTree)adaptor.Create(char_literal9);
            			    		adaptor.AddChild(root_0, char_literal9_tree);

            			    	 type = BinaryExpressionType.BitwiseAnd; 
            			    	PushFollow(FOLLOW_equalityExpression_in_bitwiseAndExpression360);
            			    	right = equalityExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop6;
            	    }
            	} while (true);

            	loop6:
            		;	// Stops C# compiler whining that label 'loop6' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "bitwiseAndExpression"

    public class equalityExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "equalityExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:135:1: equalityExpression returns [LogicalExpression value] : left= relationalExpression ( ( ( '==' | '=' ) | ( '!=' | '<>' ) ) right= relationalExpression )* ;
    public NCalcParser.equalityExpression_return equalityExpression() // throws RecognitionException [1]
    {   
        NCalcParser.equalityExpression_return retval = new NCalcParser.equalityExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken set10 = null;
        IToken set11 = null;
        NCalcParser.relationalExpression_return left = null;

        NCalcParser.relationalExpression_return right = null;


        CommonTree set10_tree=null;
        CommonTree set11_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:139:2: (left= relationalExpression ( ( ( '==' | '=' ) | ( '!=' | '<>' ) ) right= relationalExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:139:4: left= relationalExpression ( ( ( '==' | '=' ) | ( '!=' | '<>' ) ) right= relationalExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_relationalExpression_in_equalityExpression394);
            	left = relationalExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:139:56: ( ( ( '==' | '=' ) | ( '!=' | '<>' ) ) right= relationalExpression )*
            	do 
            	{
            	    int alt8 = 2;
            	    int LA8_0 = input.LA(1);

            	    if ( ((LA8_0 >= 28 && LA8_0 <= 31)) )
            	    {
            	        alt8 = 1;
            	    }


            	    switch (alt8) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:140:4: ( ( '==' | '=' ) | ( '!=' | '<>' ) ) right= relationalExpression
            			    {
            			    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:140:4: ( ( '==' | '=' ) | ( '!=' | '<>' ) )
            			    	int alt7 = 2;
            			    	int LA7_0 = input.LA(1);

            			    	if ( ((LA7_0 >= 28 && LA7_0 <= 29)) )
            			    	{
            			    	    alt7 = 1;
            			    	}
            			    	else if ( ((LA7_0 >= 30 && LA7_0 <= 31)) )
            			    	{
            			    	    alt7 = 2;
            			    	}
            			    	else 
            			    	{
            			    	    NoViableAltException nvae_d7s0 =
            			    	        new NoViableAltException("", 7, 0, input);

            			    	    throw nvae_d7s0;
            			    	}
            			    	switch (alt7) 
            			    	{
            			    	    case 1 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:140:6: ( '==' | '=' )
            			    	        {
            			    	        	set10 = (IToken)input.LT(1);
            			    	        	if ( (input.LA(1) >= 28 && input.LA(1) <= 29) ) 
            			    	        	{
            			    	        	    input.Consume();
            			    	        	    adaptor.AddChild(root_0, (CommonTree)adaptor.Create(set10));
            			    	        	    state.errorRecovery = false;
            			    	        	}
            			    	        	else 
            			    	        	{
            			    	        	    MismatchedSetException mse = new MismatchedSetException(null,input);
            			    	        	    throw mse;
            			    	        	}

            			    	        	 type = BinaryExpressionType.Equal; 

            			    	        }
            			    	        break;
            			    	    case 2 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:141:6: ( '!=' | '<>' )
            			    	        {
            			    	        	set11 = (IToken)input.LT(1);
            			    	        	if ( (input.LA(1) >= 30 && input.LA(1) <= 31) ) 
            			    	        	{
            			    	        	    input.Consume();
            			    	        	    adaptor.AddChild(root_0, (CommonTree)adaptor.Create(set11));
            			    	        	    state.errorRecovery = false;
            			    	        	}
            			    	        	else 
            			    	        	{
            			    	        	    MismatchedSetException mse = new MismatchedSetException(null,input);
            			    	        	    throw mse;
            			    	        	}

            			    	        	 type = BinaryExpressionType.NotEqual; 

            			    	        }
            			    	        break;

            			    	}

            			    	PushFollow(FOLLOW_relationalExpression_in_equalityExpression441);
            			    	right = relationalExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop8;
            	    }
            	} while (true);

            	loop8:
            		;	// Stops C# compiler whining that label 'loop8' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "equalityExpression"

    public class relationalExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "relationalExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:146:1: relationalExpression returns [LogicalExpression value] : left= shiftExpression ( ( '<' | '<=' | '>' | '>=' ) right= shiftExpression )* ;
    public NCalcParser.relationalExpression_return relationalExpression() // throws RecognitionException [1]
    {   
        NCalcParser.relationalExpression_return retval = new NCalcParser.relationalExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal12 = null;
        IToken string_literal13 = null;
        IToken char_literal14 = null;
        IToken string_literal15 = null;
        NCalcParser.shiftExpression_return left = null;

        NCalcParser.shiftExpression_return right = null;


        CommonTree char_literal12_tree=null;
        CommonTree string_literal13_tree=null;
        CommonTree char_literal14_tree=null;
        CommonTree string_literal15_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:150:2: (left= shiftExpression ( ( '<' | '<=' | '>' | '>=' ) right= shiftExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:150:4: left= shiftExpression ( ( '<' | '<=' | '>' | '>=' ) right= shiftExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_shiftExpression_in_relationalExpression474);
            	left = shiftExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:150:51: ( ( '<' | '<=' | '>' | '>=' ) right= shiftExpression )*
            	do 
            	{
            	    int alt10 = 2;
            	    int LA10_0 = input.LA(1);

            	    if ( ((LA10_0 >= 32 && LA10_0 <= 35)) )
            	    {
            	        alt10 = 1;
            	    }


            	    switch (alt10) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:151:4: ( '<' | '<=' | '>' | '>=' ) right= shiftExpression
            			    {
            			    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:151:4: ( '<' | '<=' | '>' | '>=' )
            			    	int alt9 = 4;
            			    	switch ( input.LA(1) ) 
            			    	{
            			    	case 32:
            			    		{
            			    	    alt9 = 1;
            			    	    }
            			    	    break;
            			    	case 33:
            			    		{
            			    	    alt9 = 2;
            			    	    }
            			    	    break;
            			    	case 34:
            			    		{
            			    	    alt9 = 3;
            			    	    }
            			    	    break;
            			    	case 35:
            			    		{
            			    	    alt9 = 4;
            			    	    }
            			    	    break;
            			    		default:
            			    		    NoViableAltException nvae_d9s0 =
            			    		        new NoViableAltException("", 9, 0, input);

            			    		    throw nvae_d9s0;
            			    	}

            			    	switch (alt9) 
            			    	{
            			    	    case 1 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:151:6: '<'
            			    	        {
            			    	        	char_literal12=(IToken)Match(input,32,FOLLOW_32_in_relationalExpression485); 
            			    	        		char_literal12_tree = (CommonTree)adaptor.Create(char_literal12);
            			    	        		adaptor.AddChild(root_0, char_literal12_tree);

            			    	        	 type = BinaryExpressionType.Lesser; 

            			    	        }
            			    	        break;
            			    	    case 2 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:152:6: '<='
            			    	        {
            			    	        	string_literal13=(IToken)Match(input,33,FOLLOW_33_in_relationalExpression495); 
            			    	        		string_literal13_tree = (CommonTree)adaptor.Create(string_literal13);
            			    	        		adaptor.AddChild(root_0, string_literal13_tree);

            			    	        	 type = BinaryExpressionType.LesserOrEqual; 

            			    	        }
            			    	        break;
            			    	    case 3 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:153:6: '>'
            			    	        {
            			    	        	char_literal14=(IToken)Match(input,34,FOLLOW_34_in_relationalExpression506); 
            			    	        		char_literal14_tree = (CommonTree)adaptor.Create(char_literal14);
            			    	        		adaptor.AddChild(root_0, char_literal14_tree);

            			    	        	 type = BinaryExpressionType.Greater; 

            			    	        }
            			    	        break;
            			    	    case 4 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:154:6: '>='
            			    	        {
            			    	        	string_literal15=(IToken)Match(input,35,FOLLOW_35_in_relationalExpression516); 
            			    	        		string_literal15_tree = (CommonTree)adaptor.Create(string_literal15);
            			    	        		adaptor.AddChild(root_0, string_literal15_tree);

            			    	        	 type = BinaryExpressionType.GreaterOrEqual; 

            			    	        }
            			    	        break;

            			    	}

            			    	PushFollow(FOLLOW_shiftExpression_in_relationalExpression528);
            			    	right = shiftExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop10;
            	    }
            	} while (true);

            	loop10:
            		;	// Stops C# compiler whining that label 'loop10' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "relationalExpression"

    public class shiftExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "shiftExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:159:1: shiftExpression returns [LogicalExpression value] : left= additiveExpression ( ( '<<' | '>>' ) right= additiveExpression )* ;
    public NCalcParser.shiftExpression_return shiftExpression() // throws RecognitionException [1]
    {   
        NCalcParser.shiftExpression_return retval = new NCalcParser.shiftExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken string_literal16 = null;
        IToken string_literal17 = null;
        NCalcParser.additiveExpression_return left = null;

        NCalcParser.additiveExpression_return right = null;


        CommonTree string_literal16_tree=null;
        CommonTree string_literal17_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:163:2: (left= additiveExpression ( ( '<<' | '>>' ) right= additiveExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:163:4: left= additiveExpression ( ( '<<' | '>>' ) right= additiveExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_additiveExpression_in_shiftExpression560);
            	left = additiveExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:163:54: ( ( '<<' | '>>' ) right= additiveExpression )*
            	do 
            	{
            	    int alt12 = 2;
            	    int LA12_0 = input.LA(1);

            	    if ( ((LA12_0 >= 36 && LA12_0 <= 37)) )
            	    {
            	        alt12 = 1;
            	    }


            	    switch (alt12) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:164:4: ( '<<' | '>>' ) right= additiveExpression
            			    {
            			    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:164:4: ( '<<' | '>>' )
            			    	int alt11 = 2;
            			    	int LA11_0 = input.LA(1);

            			    	if ( (LA11_0 == 36) )
            			    	{
            			    	    alt11 = 1;
            			    	}
            			    	else if ( (LA11_0 == 37) )
            			    	{
            			    	    alt11 = 2;
            			    	}
            			    	else 
            			    	{
            			    	    NoViableAltException nvae_d11s0 =
            			    	        new NoViableAltException("", 11, 0, input);

            			    	    throw nvae_d11s0;
            			    	}
            			    	switch (alt11) 
            			    	{
            			    	    case 1 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:164:6: '<<'
            			    	        {
            			    	        	string_literal16=(IToken)Match(input,36,FOLLOW_36_in_shiftExpression571); 
            			    	        		string_literal16_tree = (CommonTree)adaptor.Create(string_literal16);
            			    	        		adaptor.AddChild(root_0, string_literal16_tree);

            			    	        	 type = BinaryExpressionType.LeftShift; 

            			    	        }
            			    	        break;
            			    	    case 2 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:165:6: '>>'
            			    	        {
            			    	        	string_literal17=(IToken)Match(input,37,FOLLOW_37_in_shiftExpression581); 
            			    	        		string_literal17_tree = (CommonTree)adaptor.Create(string_literal17);
            			    	        		adaptor.AddChild(root_0, string_literal17_tree);

            			    	        	 type = BinaryExpressionType.RightShift; 

            			    	        }
            			    	        break;

            			    	}

            			    	PushFollow(FOLLOW_additiveExpression_in_shiftExpression593);
            			    	right = additiveExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop12;
            	    }
            	} while (true);

            	loop12:
            		;	// Stops C# compiler whining that label 'loop12' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "shiftExpression"

    public class additiveExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "additiveExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:170:1: additiveExpression returns [LogicalExpression value] : left= multiplicativeExpression ( ( '+' | '-' ) right= multiplicativeExpression )* ;
    public NCalcParser.additiveExpression_return additiveExpression() // throws RecognitionException [1]
    {   
        NCalcParser.additiveExpression_return retval = new NCalcParser.additiveExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal18 = null;
        IToken char_literal19 = null;
        NCalcParser.multiplicativeExpression_return left = null;

        NCalcParser.multiplicativeExpression_return right = null;


        CommonTree char_literal18_tree=null;
        CommonTree char_literal19_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:174:2: (left= multiplicativeExpression ( ( '+' | '-' ) right= multiplicativeExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:174:4: left= multiplicativeExpression ( ( '+' | '-' ) right= multiplicativeExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_multiplicativeExpression_in_additiveExpression625);
            	left = multiplicativeExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:174:60: ( ( '+' | '-' ) right= multiplicativeExpression )*
            	do 
            	{
            	    int alt14 = 2;
            	    int LA14_0 = input.LA(1);

            	    if ( ((LA14_0 >= 38 && LA14_0 <= 39)) )
            	    {
            	        alt14 = 1;
            	    }


            	    switch (alt14) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:175:4: ( '+' | '-' ) right= multiplicativeExpression
            			    {
            			    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:175:4: ( '+' | '-' )
            			    	int alt13 = 2;
            			    	int LA13_0 = input.LA(1);

            			    	if ( (LA13_0 == 38) )
            			    	{
            			    	    alt13 = 1;
            			    	}
            			    	else if ( (LA13_0 == 39) )
            			    	{
            			    	    alt13 = 2;
            			    	}
            			    	else 
            			    	{
            			    	    NoViableAltException nvae_d13s0 =
            			    	        new NoViableAltException("", 13, 0, input);

            			    	    throw nvae_d13s0;
            			    	}
            			    	switch (alt13) 
            			    	{
            			    	    case 1 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:175:6: '+'
            			    	        {
            			    	        	char_literal18=(IToken)Match(input,38,FOLLOW_38_in_additiveExpression636); 
            			    	        		char_literal18_tree = (CommonTree)adaptor.Create(char_literal18);
            			    	        		adaptor.AddChild(root_0, char_literal18_tree);

            			    	        	 type = BinaryExpressionType.Plus; 

            			    	        }
            			    	        break;
            			    	    case 2 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:176:6: '-'
            			    	        {
            			    	        	char_literal19=(IToken)Match(input,39,FOLLOW_39_in_additiveExpression646); 
            			    	        		char_literal19_tree = (CommonTree)adaptor.Create(char_literal19);
            			    	        		adaptor.AddChild(root_0, char_literal19_tree);

            			    	        	 type = BinaryExpressionType.Minus; 

            			    	        }
            			    	        break;

            			    	}

            			    	PushFollow(FOLLOW_multiplicativeExpression_in_additiveExpression658);
            			    	right = multiplicativeExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop14;
            	    }
            	} while (true);

            	loop14:
            		;	// Stops C# compiler whining that label 'loop14' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "additiveExpression"

    public class multiplicativeExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "multiplicativeExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:181:1: multiplicativeExpression returns [LogicalExpression value] : left= unaryExpression ( ( '*' | '/' | '%' ) right= unaryExpression )* ;
    public NCalcParser.multiplicativeExpression_return multiplicativeExpression() // throws RecognitionException [1]
    {   
        NCalcParser.multiplicativeExpression_return retval = new NCalcParser.multiplicativeExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal20 = null;
        IToken char_literal21 = null;
        IToken char_literal22 = null;
        NCalcParser.unaryExpression_return left = null;

        NCalcParser.unaryExpression_return right = null;


        CommonTree char_literal20_tree=null;
        CommonTree char_literal21_tree=null;
        CommonTree char_literal22_tree=null;


        BinaryExpressionType type = BinaryExpressionType.Unknown;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:185:2: (left= unaryExpression ( ( '*' | '/' | '%' ) right= unaryExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:185:4: left= unaryExpression ( ( '*' | '/' | '%' ) right= unaryExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_unaryExpression_in_multiplicativeExpression690);
            	left = unaryExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, left.Tree);
            	 retval.value =  ((left != null) ? left.value : null); 
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:185:52: ( ( '*' | '/' | '%' ) right= unaryExpression )*
            	do 
            	{
            	    int alt16 = 2;
            	    int LA16_0 = input.LA(1);

            	    if ( ((LA16_0 >= 40 && LA16_0 <= 42)) )
            	    {
            	        alt16 = 1;
            	    }


            	    switch (alt16) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:186:4: ( '*' | '/' | '%' ) right= unaryExpression
            			    {
            			    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:186:4: ( '*' | '/' | '%' )
            			    	int alt15 = 3;
            			    	switch ( input.LA(1) ) 
            			    	{
            			    	case 40:
            			    		{
            			    	    alt15 = 1;
            			    	    }
            			    	    break;
            			    	case 41:
            			    		{
            			    	    alt15 = 2;
            			    	    }
            			    	    break;
            			    	case 42:
            			    		{
            			    	    alt15 = 3;
            			    	    }
            			    	    break;
            			    		default:
            			    		    NoViableAltException nvae_d15s0 =
            			    		        new NoViableAltException("", 15, 0, input);

            			    		    throw nvae_d15s0;
            			    	}

            			    	switch (alt15) 
            			    	{
            			    	    case 1 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:186:6: '*'
            			    	        {
            			    	        	char_literal20=(IToken)Match(input,40,FOLLOW_40_in_multiplicativeExpression701); 
            			    	        		char_literal20_tree = (CommonTree)adaptor.Create(char_literal20);
            			    	        		adaptor.AddChild(root_0, char_literal20_tree);

            			    	        	 type = BinaryExpressionType.Times; 

            			    	        }
            			    	        break;
            			    	    case 2 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:187:6: '/'
            			    	        {
            			    	        	char_literal21=(IToken)Match(input,41,FOLLOW_41_in_multiplicativeExpression711); 
            			    	        		char_literal21_tree = (CommonTree)adaptor.Create(char_literal21);
            			    	        		adaptor.AddChild(root_0, char_literal21_tree);

            			    	        	 type = BinaryExpressionType.Div; 

            			    	        }
            			    	        break;
            			    	    case 3 :
            			    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:188:6: '%'
            			    	        {
            			    	        	char_literal22=(IToken)Match(input,42,FOLLOW_42_in_multiplicativeExpression721); 
            			    	        		char_literal22_tree = (CommonTree)adaptor.Create(char_literal22);
            			    	        		adaptor.AddChild(root_0, char_literal22_tree);

            			    	        	 type = BinaryExpressionType.Modulo; 

            			    	        }
            			    	        break;

            			    	}

            			    	PushFollow(FOLLOW_unaryExpression_in_multiplicativeExpression733);
            			    	right = unaryExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, right.Tree);
            			    	 retval.value =  new BinaryExpression(type, retval.value, ((right != null) ? right.value : null)); 

            			    }
            			    break;

            			default:
            			    goto loop16;
            	    }
            	} while (true);

            	loop16:
            		;	// Stops C# compiler whining that label 'loop16' has no statements


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "multiplicativeExpression"

    public class unaryExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "unaryExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:194:1: unaryExpression returns [LogicalExpression value] : ( primaryExpression | ( '!' | 'not' ) primaryExpression | ( '~' ) primaryExpression | '-' primaryExpression );
    public NCalcParser.unaryExpression_return unaryExpression() // throws RecognitionException [1]
    {   
        NCalcParser.unaryExpression_return retval = new NCalcParser.unaryExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken set24 = null;
        IToken char_literal26 = null;
        IToken char_literal28 = null;
        NCalcParser.primaryExpression_return primaryExpression23 = null;

        NCalcParser.primaryExpression_return primaryExpression25 = null;

        NCalcParser.primaryExpression_return primaryExpression27 = null;

        NCalcParser.primaryExpression_return primaryExpression29 = null;


        CommonTree set24_tree=null;
        CommonTree char_literal26_tree=null;
        CommonTree char_literal28_tree=null;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:195:2: ( primaryExpression | ( '!' | 'not' ) primaryExpression | ( '~' ) primaryExpression | '-' primaryExpression )
            int alt17 = 4;
            switch ( input.LA(1) ) 
            {
            case INTEGER:
            case FLOAT:
            case STRING:
            case DATETIME:
            case TRUE:
            case FALSE:
            case ID:
            case NAME:
            case 46:
            	{
                alt17 = 1;
                }
                break;
            case 43:
            case 44:
            	{
                alt17 = 2;
                }
                break;
            case 45:
            	{
                alt17 = 3;
                }
                break;
            case 39:
            	{
                alt17 = 4;
                }
                break;
            	default:
            	    NoViableAltException nvae_d17s0 =
            	        new NoViableAltException("", 17, 0, input);

            	    throw nvae_d17s0;
            }

            switch (alt17) 
            {
                case 1 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:195:4: primaryExpression
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	PushFollow(FOLLOW_primaryExpression_in_unaryExpression760);
                    	primaryExpression23 = primaryExpression();
                    	state.followingStackPointer--;

                    	adaptor.AddChild(root_0, primaryExpression23.Tree);
                    	 retval.value =  ((primaryExpression23 != null) ? primaryExpression23.value : null); 

                    }
                    break;
                case 2 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:196:8: ( '!' | 'not' ) primaryExpression
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	set24 = (IToken)input.LT(1);
                    	if ( (input.LA(1) >= 43 && input.LA(1) <= 44) ) 
                    	{
                    	    input.Consume();
                    	    adaptor.AddChild(root_0, (CommonTree)adaptor.Create(set24));
                    	    state.errorRecovery = false;
                    	}
                    	else 
                    	{
                    	    MismatchedSetException mse = new MismatchedSetException(null,input);
                    	    throw mse;
                    	}

                    	PushFollow(FOLLOW_primaryExpression_in_unaryExpression779);
                    	primaryExpression25 = primaryExpression();
                    	state.followingStackPointer--;

                    	adaptor.AddChild(root_0, primaryExpression25.Tree);
                    	 retval.value =  new UnaryExpression(UnaryExpressionType.Not, ((primaryExpression25 != null) ? primaryExpression25.value : null)); 

                    }
                    break;
                case 3 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:197:8: ( '~' ) primaryExpression
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:197:8: ( '~' )
                    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:197:9: '~'
                    	{
                    		char_literal26=(IToken)Match(input,45,FOLLOW_45_in_unaryExpression791); 
                    			char_literal26_tree = (CommonTree)adaptor.Create(char_literal26);
                    			adaptor.AddChild(root_0, char_literal26_tree);


                    	}

                    	PushFollow(FOLLOW_primaryExpression_in_unaryExpression794);
                    	primaryExpression27 = primaryExpression();
                    	state.followingStackPointer--;

                    	adaptor.AddChild(root_0, primaryExpression27.Tree);
                    	 retval.value =  new UnaryExpression(UnaryExpressionType.BitwiseNot, ((primaryExpression27 != null) ? primaryExpression27.value : null)); 

                    }
                    break;
                case 4 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:198:8: '-' primaryExpression
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	char_literal28=(IToken)Match(input,39,FOLLOW_39_in_unaryExpression805); 
                    		char_literal28_tree = (CommonTree)adaptor.Create(char_literal28);
                    		adaptor.AddChild(root_0, char_literal28_tree);

                    	PushFollow(FOLLOW_primaryExpression_in_unaryExpression807);
                    	primaryExpression29 = primaryExpression();
                    	state.followingStackPointer--;

                    	adaptor.AddChild(root_0, primaryExpression29.Tree);
                    	 retval.value =  new UnaryExpression(UnaryExpressionType.Negate, ((primaryExpression29 != null) ? primaryExpression29.value : null)); 

                    }
                    break;

            }
            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "unaryExpression"

    public class primaryExpression_return : ParserRuleReturnScope
    {
        public LogicalExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "primaryExpression"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:201:1: primaryExpression returns [LogicalExpression value] : ( '(' logicalExpression ')' | expr= value | identifier ( arguments )? );
    public NCalcParser.primaryExpression_return primaryExpression() // throws RecognitionException [1]
    {   
        NCalcParser.primaryExpression_return retval = new NCalcParser.primaryExpression_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal30 = null;
        IToken char_literal32 = null;
        NCalcParser.value_return expr = null;

        NCalcParser.logicalExpression_return logicalExpression31 = null;

        NCalcParser.identifier_return identifier33 = null;

        NCalcParser.arguments_return arguments34 = null;


        CommonTree char_literal30_tree=null;
        CommonTree char_literal32_tree=null;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:202:2: ( '(' logicalExpression ')' | expr= value | identifier ( arguments )? )
            int alt19 = 3;
            switch ( input.LA(1) ) 
            {
            case 46:
            	{
                alt19 = 1;
                }
                break;
            case INTEGER:
            case FLOAT:
            case STRING:
            case DATETIME:
            case TRUE:
            case FALSE:
            	{
                alt19 = 2;
                }
                break;
            case ID:
            case NAME:
            	{
                alt19 = 3;
                }
                break;
            	default:
            	    NoViableAltException nvae_d19s0 =
            	        new NoViableAltException("", 19, 0, input);

            	    throw nvae_d19s0;
            }

            switch (alt19) 
            {
                case 1 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:202:4: '(' logicalExpression ')'
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	char_literal30=(IToken)Match(input,46,FOLLOW_46_in_primaryExpression829); 
                    		char_literal30_tree = (CommonTree)adaptor.Create(char_literal30);
                    		adaptor.AddChild(root_0, char_literal30_tree);

                    	PushFollow(FOLLOW_logicalExpression_in_primaryExpression831);
                    	logicalExpression31 = logicalExpression();
                    	state.followingStackPointer--;

                    	adaptor.AddChild(root_0, logicalExpression31.Tree);
                    	char_literal32=(IToken)Match(input,47,FOLLOW_47_in_primaryExpression833); 
                    		char_literal32_tree = (CommonTree)adaptor.Create(char_literal32);
                    		adaptor.AddChild(root_0, char_literal32_tree);

                    	 retval.value =  ((logicalExpression31 != null) ? logicalExpression31.value : null); 

                    }
                    break;
                case 2 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:203:4: expr= value
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	PushFollow(FOLLOW_value_in_primaryExpression843);
                    	expr = value();
                    	state.followingStackPointer--;

                    	adaptor.AddChild(root_0, expr.Tree);
                    	 retval.value =  ((expr != null) ? expr.value : null); 

                    }
                    break;
                case 3 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:204:4: identifier ( arguments )?
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	PushFollow(FOLLOW_identifier_in_primaryExpression851);
                    	identifier33 = identifier();
                    	state.followingStackPointer--;

                    	adaptor.AddChild(root_0, identifier33.Tree);
                    	retval.value =  (LogicalExpression) ((identifier33 != null) ? identifier33.value : null); 
                    	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:204:66: ( arguments )?
                    	int alt18 = 2;
                    	int LA18_0 = input.LA(1);

                    	if ( (LA18_0 == 46) )
                    	{
                    	    alt18 = 1;
                    	}
                    	switch (alt18) 
                    	{
                    	    case 1 :
                    	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:204:67: arguments
                    	        {
                    	        	PushFollow(FOLLOW_arguments_in_primaryExpression856);
                    	        	arguments34 = arguments();
                    	        	state.followingStackPointer--;

                    	        	adaptor.AddChild(root_0, arguments34.Tree);
                    	        	retval.value =  new Function(((identifier33 != null) ? identifier33.value : null), (((arguments34 != null) ? arguments34.value : null)).ToArray()); 

                    	        }
                    	        break;

                    	}


                    }
                    break;

            }
            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "primaryExpression"

    public class value_return : ParserRuleReturnScope
    {
        public ValueExpression value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "value"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:207:1: value returns [ValueExpression value] : ( INTEGER | FLOAT | STRING | DATETIME | TRUE | FALSE );
    public NCalcParser.value_return value() // throws RecognitionException [1]
    {   
        NCalcParser.value_return retval = new NCalcParser.value_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken INTEGER35 = null;
        IToken FLOAT36 = null;
        IToken STRING37 = null;
        IToken DATETIME38 = null;
        IToken TRUE39 = null;
        IToken FALSE40 = null;

        CommonTree INTEGER35_tree=null;
        CommonTree FLOAT36_tree=null;
        CommonTree STRING37_tree=null;
        CommonTree DATETIME38_tree=null;
        CommonTree TRUE39_tree=null;
        CommonTree FALSE40_tree=null;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:208:2: ( INTEGER | FLOAT | STRING | DATETIME | TRUE | FALSE )
            int alt20 = 6;
            switch ( input.LA(1) ) 
            {
            case INTEGER:
            	{
                alt20 = 1;
                }
                break;
            case FLOAT:
            	{
                alt20 = 2;
                }
                break;
            case STRING:
            	{
                alt20 = 3;
                }
                break;
            case DATETIME:
            	{
                alt20 = 4;
                }
                break;
            case TRUE:
            	{
                alt20 = 5;
                }
                break;
            case FALSE:
            	{
                alt20 = 6;
                }
                break;
            	default:
            	    NoViableAltException nvae_d20s0 =
            	        new NoViableAltException("", 20, 0, input);

            	    throw nvae_d20s0;
            }

            switch (alt20) 
            {
                case 1 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:208:5: INTEGER
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	INTEGER35=(IToken)Match(input,INTEGER,FOLLOW_INTEGER_in_value876); 
                    		INTEGER35_tree = (CommonTree)adaptor.Create(INTEGER35);
                    		adaptor.AddChild(root_0, INTEGER35_tree);

                            try
                            {
                                retval.value = new ValueExpression(int.Parse(((INTEGER35 != null) ? INTEGER35.Text : null)));
                            }
                            catch (System.OverflowException)
                            {
                                retval.value = new ValueExpression(long.Parse(((INTEGER35 != null) ? INTEGER35.Text : null)));
                            }

                    }
                    break;
                case 2 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:209:4: FLOAT
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	FLOAT36=(IToken)Match(input,FLOAT,FOLLOW_FLOAT_in_value884); 
                    		FLOAT36_tree = (CommonTree)adaptor.Create(FLOAT36);
                    		adaptor.AddChild(root_0, FLOAT36_tree);

                    	 retval.value =  new ValueExpression(double.Parse(((FLOAT36 != null) ? FLOAT36.Text : null), NumberStyles.Float, numberFormatInfo)); 

                    }
                    break;
                case 3 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:210:4: STRING
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	STRING37=(IToken)Match(input,STRING,FOLLOW_STRING_in_value892); 
                    		STRING37_tree = (CommonTree)adaptor.Create(STRING37);
                    		adaptor.AddChild(root_0, STRING37_tree);

                    	 retval.value =  new ValueExpression(extractString(((STRING37 != null) ? STRING37.Text : null))); 

                    }
                    break;
                case 4 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:211:5: DATETIME
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	DATETIME38=(IToken)Match(input,DATETIME,FOLLOW_DATETIME_in_value901); 
                    		DATETIME38_tree = (CommonTree)adaptor.Create(DATETIME38);
                    		adaptor.AddChild(root_0, DATETIME38_tree);

                    	 retval.value =  new ValueExpression(DateTime.Parse(((DATETIME38 != null) ? DATETIME38.Text : null).Substring(1, ((DATETIME38 != null) ? DATETIME38.Text : null).Length-2))); 

                    }
                    break;
                case 5 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:212:4: TRUE
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	TRUE39=(IToken)Match(input,TRUE,FOLLOW_TRUE_in_value908); 
                    		TRUE39_tree = (CommonTree)adaptor.Create(TRUE39);
                    		adaptor.AddChild(root_0, TRUE39_tree);

                    	 retval.value =  new ValueExpression(true); 

                    }
                    break;
                case 6 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:213:4: FALSE
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	FALSE40=(IToken)Match(input,FALSE,FOLLOW_FALSE_in_value916); 
                    		FALSE40_tree = (CommonTree)adaptor.Create(FALSE40);
                    		adaptor.AddChild(root_0, FALSE40_tree);

                    	 retval.value =  new ValueExpression(false); 

                    }
                    break;

            }
            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "value"

    public class identifier_return : ParserRuleReturnScope
    {
        public Identifier value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "identifier"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:216:1: identifier returns [Identifier value] : ( ID | NAME );
    public NCalcParser.identifier_return identifier() // throws RecognitionException [1]
    {   
        NCalcParser.identifier_return retval = new NCalcParser.identifier_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken ID41 = null;
        IToken NAME42 = null;

        CommonTree ID41_tree=null;
        CommonTree NAME42_tree=null;

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:217:2: ( ID | NAME )
            int alt21 = 2;
            int LA21_0 = input.LA(1);

            if ( (LA21_0 == ID) )
            {
                alt21 = 1;
            }
            else if ( (LA21_0 == NAME) )
            {
                alt21 = 2;
            }
            else 
            {
                NoViableAltException nvae_d21s0 =
                    new NoViableAltException("", 21, 0, input);

                throw nvae_d21s0;
            }
            switch (alt21) 
            {
                case 1 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:217:5: ID
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	ID41=(IToken)Match(input,ID,FOLLOW_ID_in_identifier934); 
                    		ID41_tree = (CommonTree)adaptor.Create(ID41);
                    		adaptor.AddChild(root_0, ID41_tree);

                    	 retval.value =  new Identifier(((ID41 != null) ? ID41.Text : null)); 

                    }
                    break;
                case 2 :
                    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:218:5: NAME
                    {
                    	root_0 = (CommonTree)adaptor.GetNilNode();

                    	NAME42=(IToken)Match(input,NAME,FOLLOW_NAME_in_identifier942); 
                    		NAME42_tree = (CommonTree)adaptor.Create(NAME42);
                    		adaptor.AddChild(root_0, NAME42_tree);

                    	 retval.value =  new Identifier(((NAME42 != null) ? NAME42.Text : null).Substring(1, ((NAME42 != null) ? NAME42.Text : null).Length-2)); 

                    }
                    break;

            }
            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "identifier"

    public class expressionList_return : ParserRuleReturnScope
    {
        public List<LogicalExpression> value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "expressionList"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:221:1: expressionList returns [List<LogicalExpression> value] : first= logicalExpression ( ',' follow= logicalExpression )* ;
    public NCalcParser.expressionList_return expressionList() // throws RecognitionException [1]
    {   
        NCalcParser.expressionList_return retval = new NCalcParser.expressionList_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal43 = null;
        NCalcParser.logicalExpression_return first = null;

        NCalcParser.logicalExpression_return follow = null;


        CommonTree char_literal43_tree=null;


        List<LogicalExpression> expressions = new List<LogicalExpression>();

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:225:2: (first= logicalExpression ( ',' follow= logicalExpression )* )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:225:4: first= logicalExpression ( ',' follow= logicalExpression )*
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	PushFollow(FOLLOW_logicalExpression_in_expressionList966);
            	first = logicalExpression();
            	state.followingStackPointer--;

            	adaptor.AddChild(root_0, first.Tree);
            	expressions.Add(((first != null) ? first.value : null));
            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:225:62: ( ',' follow= logicalExpression )*
            	do 
            	{
            	    int alt22 = 2;
            	    int LA22_0 = input.LA(1);

            	    if ( (LA22_0 == 48) )
            	    {
            	        alt22 = 1;
            	    }


            	    switch (alt22) 
            		{
            			case 1 :
            			    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:225:64: ',' follow= logicalExpression
            			    {
            			    	char_literal43=(IToken)Match(input,48,FOLLOW_48_in_expressionList973); 
            			    		char_literal43_tree = (CommonTree)adaptor.Create(char_literal43);
            			    		adaptor.AddChild(root_0, char_literal43_tree);

            			    	PushFollow(FOLLOW_logicalExpression_in_expressionList977);
            			    	follow = logicalExpression();
            			    	state.followingStackPointer--;

            			    	adaptor.AddChild(root_0, follow.Tree);
            			    	expressions.Add(((follow != null) ? follow.value : null));

            			    }
            			    break;

            			default:
            			    goto loop22;
            	    }
            	} while (true);

            	loop22:
            		;	// Stops C# compiler whining that label 'loop22' has no statements

            	 retval.value =  expressions; 

            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "expressionList"

    public class arguments_return : ParserRuleReturnScope
    {
        public List<LogicalExpression> value;
        private CommonTree tree;
        override public object Tree
        {
        	get { return tree; }
        	set { tree = (CommonTree) value; }
        }
    };

    // $ANTLR start "arguments"
    // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:229:1: arguments returns [List<LogicalExpression> value] : '(' ( expressionList )? ')' ;
    public NCalcParser.arguments_return arguments() // throws RecognitionException [1]
    {   
        NCalcParser.arguments_return retval = new NCalcParser.arguments_return();
        retval.Start = input.LT(1);

        CommonTree root_0 = null;

        IToken char_literal44 = null;
        IToken char_literal46 = null;
        NCalcParser.expressionList_return expressionList45 = null;


        CommonTree char_literal44_tree=null;
        CommonTree char_literal46_tree=null;


        retval.value =  new List<LogicalExpression>();

        try 
    	{
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:233:2: ( '(' ( expressionList )? ')' )
            // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:233:4: '(' ( expressionList )? ')'
            {
            	root_0 = (CommonTree)adaptor.GetNilNode();

            	char_literal44=(IToken)Match(input,46,FOLLOW_46_in_arguments1006); 
            		char_literal44_tree = (CommonTree)adaptor.Create(char_literal44);
            		adaptor.AddChild(root_0, char_literal44_tree);

            	// C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:233:8: ( expressionList )?
            	int alt23 = 2;
            	int LA23_0 = input.LA(1);

            	if ( ((LA23_0 >= INTEGER && LA23_0 <= NAME) || LA23_0 == 39 || (LA23_0 >= 43 && LA23_0 <= 46)) )
            	{
            	    alt23 = 1;
            	}
            	switch (alt23) 
            	{
            	    case 1 :
            	        // C:\\Users\\s.ros\\Documents\\Développement\\NCalc\\Grammar\\NCalc.g:233:10: expressionList
            	        {
            	        	PushFollow(FOLLOW_expressionList_in_arguments1010);
            	        	expressionList45 = expressionList();
            	        	state.followingStackPointer--;

            	        	adaptor.AddChild(root_0, expressionList45.Tree);
            	        	retval.value =  ((expressionList45 != null) ? expressionList45.value : null);

            	        }
            	        break;

            	}

            	char_literal46=(IToken)Match(input,47,FOLLOW_47_in_arguments1017); 
            		char_literal46_tree = (CommonTree)adaptor.Create(char_literal46);
            		adaptor.AddChild(root_0, char_literal46_tree);


            }

            retval.Stop = input.LT(-1);

            	retval.Tree = (CommonTree)adaptor.RulePostProcessing(root_0);
            	adaptor.SetTokenBoundaries(retval.Tree, (IToken) retval.Start, (IToken) retval.Stop);
        }
        catch (RecognitionException re) 
    	{
            ReportError(re);
            Recover(input,re);
    	// Conversion of the second argument necessary, but harmless
    	retval.Tree = (CommonTree)adaptor.ErrorNode(input, (IToken) retval.Start, input.LT(-1), re);

        }
        finally 
    	{
        }
        return retval;
    }
    // $ANTLR end "arguments"

    // Delegated rules


	private void InitializeCyclicDFAs()
	{
	}

 

    public static readonly BitSet FOLLOW_logicalExpression_in_ncalcExpression56 = new BitSet(new ulong[]{0x0000000000000000UL});
    public static readonly BitSet FOLLOW_EOF_in_ncalcExpression58 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_conditionalExpression_in_logicalExpression78 = new BitSet(new ulong[]{0x0000000000080002UL});
    public static readonly BitSet FOLLOW_19_in_logicalExpression84 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_conditionalExpression_in_logicalExpression88 = new BitSet(new ulong[]{0x0000000000100000UL});
    public static readonly BitSet FOLLOW_20_in_logicalExpression90 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_conditionalExpression_in_logicalExpression94 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_booleanAndExpression_in_conditionalExpression121 = new BitSet(new ulong[]{0x0000000000600002UL});
    public static readonly BitSet FOLLOW_set_in_conditionalExpression130 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_conditionalExpression_in_conditionalExpression146 = new BitSet(new ulong[]{0x0000000000600002UL});
    public static readonly BitSet FOLLOW_bitwiseOrExpression_in_booleanAndExpression180 = new BitSet(new ulong[]{0x0000000001800002UL});
    public static readonly BitSet FOLLOW_set_in_booleanAndExpression189 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_bitwiseOrExpression_in_booleanAndExpression205 = new BitSet(new ulong[]{0x0000000001800002UL});
    public static readonly BitSet FOLLOW_bitwiseXOrExpression_in_bitwiseOrExpression237 = new BitSet(new ulong[]{0x0000000002000002UL});
    public static readonly BitSet FOLLOW_25_in_bitwiseOrExpression246 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_bitwiseOrExpression_in_bitwiseOrExpression256 = new BitSet(new ulong[]{0x0000000002000002UL});
    public static readonly BitSet FOLLOW_bitwiseAndExpression_in_bitwiseXOrExpression290 = new BitSet(new ulong[]{0x0000000004000002UL});
    public static readonly BitSet FOLLOW_26_in_bitwiseXOrExpression299 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_bitwiseAndExpression_in_bitwiseXOrExpression309 = new BitSet(new ulong[]{0x0000000004000002UL});
    public static readonly BitSet FOLLOW_equalityExpression_in_bitwiseAndExpression341 = new BitSet(new ulong[]{0x0000000008000002UL});
    public static readonly BitSet FOLLOW_27_in_bitwiseAndExpression350 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_equalityExpression_in_bitwiseAndExpression360 = new BitSet(new ulong[]{0x0000000008000002UL});
    public static readonly BitSet FOLLOW_relationalExpression_in_equalityExpression394 = new BitSet(new ulong[]{0x00000000F0000002UL});
    public static readonly BitSet FOLLOW_set_in_equalityExpression405 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_set_in_equalityExpression422 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_relationalExpression_in_equalityExpression441 = new BitSet(new ulong[]{0x00000000F0000002UL});
    public static readonly BitSet FOLLOW_shiftExpression_in_relationalExpression474 = new BitSet(new ulong[]{0x0000000F00000002UL});
    public static readonly BitSet FOLLOW_32_in_relationalExpression485 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_33_in_relationalExpression495 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_34_in_relationalExpression506 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_35_in_relationalExpression516 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_shiftExpression_in_relationalExpression528 = new BitSet(new ulong[]{0x0000000F00000002UL});
    public static readonly BitSet FOLLOW_additiveExpression_in_shiftExpression560 = new BitSet(new ulong[]{0x0000003000000002UL});
    public static readonly BitSet FOLLOW_36_in_shiftExpression571 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_37_in_shiftExpression581 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_additiveExpression_in_shiftExpression593 = new BitSet(new ulong[]{0x0000003000000002UL});
    public static readonly BitSet FOLLOW_multiplicativeExpression_in_additiveExpression625 = new BitSet(new ulong[]{0x000000C000000002UL});
    public static readonly BitSet FOLLOW_38_in_additiveExpression636 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_39_in_additiveExpression646 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_multiplicativeExpression_in_additiveExpression658 = new BitSet(new ulong[]{0x000000C000000002UL});
    public static readonly BitSet FOLLOW_unaryExpression_in_multiplicativeExpression690 = new BitSet(new ulong[]{0x0000070000000002UL});
    public static readonly BitSet FOLLOW_40_in_multiplicativeExpression701 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_41_in_multiplicativeExpression711 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_42_in_multiplicativeExpression721 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_unaryExpression_in_multiplicativeExpression733 = new BitSet(new ulong[]{0x0000070000000002UL});
    public static readonly BitSet FOLLOW_primaryExpression_in_unaryExpression760 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_set_in_unaryExpression771 = new BitSet(new ulong[]{0x0000400000000FF0UL});
    public static readonly BitSet FOLLOW_primaryExpression_in_unaryExpression779 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_45_in_unaryExpression791 = new BitSet(new ulong[]{0x0000400000000FF0UL});
    public static readonly BitSet FOLLOW_primaryExpression_in_unaryExpression794 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_39_in_unaryExpression805 = new BitSet(new ulong[]{0x0000400000000FF0UL});
    public static readonly BitSet FOLLOW_primaryExpression_in_unaryExpression807 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_46_in_primaryExpression829 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_logicalExpression_in_primaryExpression831 = new BitSet(new ulong[]{0x0000800000000000UL});
    public static readonly BitSet FOLLOW_47_in_primaryExpression833 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_value_in_primaryExpression843 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_identifier_in_primaryExpression851 = new BitSet(new ulong[]{0x0000400000000002UL});
    public static readonly BitSet FOLLOW_arguments_in_primaryExpression856 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_INTEGER_in_value876 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_FLOAT_in_value884 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_STRING_in_value892 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_DATETIME_in_value901 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_TRUE_in_value908 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_FALSE_in_value916 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_ID_in_identifier934 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_NAME_in_identifier942 = new BitSet(new ulong[]{0x0000000000000002UL});
    public static readonly BitSet FOLLOW_logicalExpression_in_expressionList966 = new BitSet(new ulong[]{0x0001000000000002UL});
    public static readonly BitSet FOLLOW_48_in_expressionList973 = new BitSet(new ulong[]{0x0000788000000FF0UL});
    public static readonly BitSet FOLLOW_logicalExpression_in_expressionList977 = new BitSet(new ulong[]{0x0001000000000002UL});
    public static readonly BitSet FOLLOW_46_in_arguments1006 = new BitSet(new ulong[]{0x0000F88000000FF0UL});
    public static readonly BitSet FOLLOW_expressionList_in_arguments1010 = new BitSet(new ulong[]{0x0000800000000000UL});
    public static readonly BitSet FOLLOW_47_in_arguments1017 = new BitSet(new ulong[]{0x0000000000000002UL});

}
