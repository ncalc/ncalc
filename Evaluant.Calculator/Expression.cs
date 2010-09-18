using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Antlr.Runtime.Tree;
using NCalc.Domain;
using Antlr.Runtime;
using System.Diagnostics;
using System.Threading;

namespace NCalc
{
    public class Expression
    {
        public EvaluateOptions Options { get; set; }

        /// <summary>
        /// Textual representation of the expression to evaluate.
        /// </summary>
        protected string expression;

        public Expression(string expression) : this(expression, EvaluateOptions.None)
        {
        }

        public Expression(string expression, EvaluateOptions options)
        {
            if (String.IsNullOrEmpty(expression))
                throw new 
                    ArgumentException("Expression can't be empty", "expression");

            this.expression = expression;
            Options = options;
        }

        public Expression(LogicalExpression expression) : this(expression, EvaluateOptions.None)
        {
        }

        public Expression(LogicalExpression expression, EvaluateOptions options)
        {
            if (expression == null)
                throw new
                    ArgumentException("Expression can't be null", "expression");

            ParsedExpression = expression;
            Options = options;
        }

        #region Cache management
        private static bool cacheEnabled = true;
        private static object synLock = new object();
        private static Dictionary<string, WeakReference> compiledExpressions = new Dictionary<string, WeakReference>();
        private static ReaderWriterLock rwl = new ReaderWriterLock();

        public static bool CacheEnabled
        {
            get { return cacheEnabled; }
            set 
            { 
                cacheEnabled = value;

                if (!CacheEnabled)
                {
                    // Clears cache
                    compiledExpressions = new Dictionary<string, WeakReference>();
                }
            }
        }

        /// <summary>
        /// Removed unused entries from cached compiled expression
        /// </summary>
        private static void CleanCache()
        {
            List<string> keysToRemove = new List<string>();

            try
            {
                rwl.AcquireWriterLock(Timeout.Infinite);
                foreach (var de in compiledExpressions)
                {
                    if (!de.Value.IsAlive)
                    {
                        keysToRemove.Add(de.Key);
                    }
                }


                foreach (string key in keysToRemove)
                {
                    compiledExpressions.Remove(key);
                    Trace.TraceInformation("Cache entry released: " + key);
                }
            }
            finally
            {
                rwl.ReleaseReaderLock();
            }
        }

        #endregion

        public static LogicalExpression Compile(string expression, bool nocache)
        {
            LogicalExpression logicalExpression = null;

            if (cacheEnabled && !nocache)
            {
                try
                {
                    rwl.AcquireReaderLock(Timeout.Infinite);

                    if (compiledExpressions.ContainsKey(expression))
                    {
                        Trace.TraceInformation("Expression retrieved from cache: " + expression);
                        var wr = compiledExpressions[expression];
                        logicalExpression = wr.Target as LogicalExpression;
                    
                        if (wr.IsAlive && logicalExpression != null)
                        {
                            return logicalExpression;
                        }
                    }
                }
                finally
                {
                    rwl.ReleaseReaderLock();
                }
            }

            if (logicalExpression == null)
            {
                NCalcLexer lexer = new NCalcLexer(new ANTLRStringStream(expression));
                NCalcParser parser = new NCalcParser(new CommonTokenStream(lexer));

                logicalExpression = parser.ncalcExpression().value;

                if (parser.Errors != null && parser.Errors.Count > 0)
                {
                    throw new EvaluationException(String.Join(Environment.NewLine, parser.Errors.ToArray()));
                }

                if (cacheEnabled && !nocache)
                {
                    try
                    {
                        rwl.AcquireWriterLock(Timeout.Infinite);
                        compiledExpressions[expression] = new WeakReference(logicalExpression);
                    }
                    finally
                    {
                        rwl.ReleaseWriterLock();
                    }

                    CleanCache();

                    Trace.TraceInformation("Expression added to cache: " + expression);
                }
            }

            return logicalExpression;
        }

        /// <summary>
        /// Pre-compiles the expression in order to check syntax errors.
        /// If errors are detected, the Error property contains the message.
        /// </summary>
        /// <returns>True if the expression syntax is correct, otherwiser False</returns>
        public bool HasErrors()
        {
            try
            {
                if (ParsedExpression == null)
                {
                    ParsedExpression = Expression.Compile(expression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
                }

                // In case HasErrors() is called multiple times for the same expression
                return ParsedExpression != null && Error != null;
            }
            catch(Exception e)
            {
                Error = e.Message;
                return true;
            }
        }

        public string Error { get; private set; }

        public LogicalExpression ParsedExpression { get; private set; }

        protected Dictionary<string, IEnumerator> parameterEnumerators;
        protected Dictionary<string, object> parametersBackup;

        public object Evaluate()
        {
            if (HasErrors())
            {
                throw new EvaluationException(Error);
            }

            if (ParsedExpression == null)
            {
                ParsedExpression = Compile(expression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);
            }


            EvaluationVisitor visitor = new EvaluationVisitor(Options);
            visitor.EvaluateFunction += EvaluateFunction;
            visitor.EvaluateParameter += EvaluateParameter;
            visitor.Parameters = Parameters;

            // if array evaluation, execute the same expression multiple times
            if ((Options & EvaluateOptions.IterateParameters) == EvaluateOptions.IterateParameters)
            {
                int size = -1;
                parametersBackup = new Dictionary<string, object>();
                foreach (string key in Parameters.Keys)
                {
                    parametersBackup.Add(key, Parameters[key]);
                }

                parameterEnumerators = new Dictionary<string, IEnumerator>();

                foreach (object parameter in Parameters.Values)
                {
                    if (parameter is IEnumerable)
                    {
                        int localsize = 0;
                        foreach (object o in (IEnumerable)parameter)
                        {
                            localsize++;
                        }

                        if (size == -1)
                        {
                            size = localsize;
                        }
                        else if (localsize != size)
                        {
                            throw new EvaluationException("When IterateParameters option is used, IEnumerable parameters must have the same number of items");
                        }
                    }
                }

                foreach (string key in Parameters.Keys)
                {
                    IEnumerable parameter = Parameters[key] as IEnumerable;
                    if (parameter != null)
                    {
                        parameterEnumerators.Add(key, parameter.GetEnumerator());
                    }
                }

                List<object> results = new List<object>();
                for (int i = 0; i < size; i++)
                {
                    foreach (string key in parameterEnumerators.Keys)
                    {
                        IEnumerator enumerator = parameterEnumerators[key];
                        enumerator.MoveNext();
                        Parameters[key] = enumerator.Current;
                    }

                    ParsedExpression.Accept(visitor);
                    results.Add(visitor.Result);
                }

                return results;
            }
            else
            {
                ParsedExpression.Accept(visitor);
                return visitor.Result;
            }

        }

        public event EvaluateFunctionHandler EvaluateFunction;
        public event EvaluateParameterHandler EvaluateParameter;

        private Dictionary<string, object> parameters;

        public Dictionary<string, object> Parameters
        {
            get 
            { 
                if(parameters == null)
                {
                    parameters = new Dictionary<string, object>();
                }

                return parameters; 
            }
            set { parameters = value; }
        }

    }
}
