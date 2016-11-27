/*
 * ================================================
 * Name: Ryan Dougherty
 * UID: u0534947
 * 
 * CS 3500
 * Assignment PS3
 * Time Spent: ~6 Hours
 * ================================================
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax; variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private List<string> Tokens { get; set; }
        //public HashSet<Formula> Dependents { get; private set; }   //not needed for this assignment
        //public HashSet<Formula> Dependees { get; private set; }    //not needed for this assignment


        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            if (formula == "")
                throw new FormulaFormatException("formula is an invalid size");
            Tokens = new List<string>();
            IEnumerable<string> tokenEnum = GetTokens(formula);
            int parenCount = 0;
            double tokenAsDouble;
            string prevToken = null, currentToken = null;
            foreach (string token in tokenEnum)
            {
                currentToken = token;           //set the current token to enumerators current value
                string normalizedToken = normalize(currentToken);
                if (prevToken == null || IsOperator(prevToken))      //check if previous token is an operator  
                {
                    if (double.TryParse(currentToken, out tokenAsDouble))
                        Tokens.Add(tokenAsDouble.ToString());
                    else if (IsVariable(normalizedToken) && isValid(normalizedToken))
                        Tokens.Add(normalizedToken);
                    else if (currentToken == ")")
                    {
                        Tokens.Add(currentToken);
                        parenCount--;
                    }
                    else if (currentToken == "(")
                    {
                        Tokens.Add(currentToken);
                        parenCount++;
                    }
                    else
                        throw new FormulaFormatException("unexpected token \'" + currentToken + "\' after \'" + prevToken + "\'");
                    prevToken = currentToken;                       //set previous token to the current token's value for the next iteration
                }
                else if (double.TryParse(prevToken, out tokenAsDouble) || IsVariable(prevToken) && isValid(prevToken))       //check to see if the previous variable was a number or a variable that represents a number
                {
                    if (IsOperator(currentToken))
                        Tokens.Add(currentToken);
                    else if (currentToken == ")")
                    {
                        Tokens.Add(currentToken);
                        parenCount--;
                    }
                    else if (currentToken == "(")
                    {
                        Tokens.Add(currentToken);
                        parenCount++;
                    }
                    else
                        throw new FormulaFormatException("unexpected token \'" + currentToken + "\' after \'" + prevToken + "\'");
                    prevToken = currentToken;                       //set previous token to the current token's value for the next iteration
                }
                else if (prevToken == "(")
                {
                    
                    if (double.TryParse(currentToken, out tokenAsDouble))
                        Tokens.Add(tokenAsDouble.ToString());
                    else if (IsVariable(normalizedToken) && isValid(normalizedToken))
                        Tokens.Add(normalizedToken);
                    else if (currentToken == ")")
                    {
                        Tokens.Add(currentToken);
                        parenCount--;
                    }
                    else if (currentToken == "(")
                    {
                        Tokens.Add(currentToken);
                        parenCount++;
                    }
                    else
                        throw new FormulaFormatException("unexpected token \'" + currentToken + "\' after \'" + prevToken + "\'");
                    prevToken = currentToken;
                }
                else if (prevToken == ")")
                {
                    if (IsOperator(currentToken))
                        Tokens.Add(currentToken);
                    else if (currentToken == ")")
                    {
                        Tokens.Add(currentToken);
                        parenCount--;
                    }
                    else if (currentToken == "(")
                    {
                        Tokens.Add(currentToken);
                        parenCount++;
                    }
                    else
                        throw new FormulaFormatException("unexpected token \'" + currentToken + "\' after \'" + prevToken + "\'");
                    prevToken = currentToken;
                }
                else
                    throw new FormulaFormatException("unexpected token \'" + currentToken + "\' after \'" + prevToken + "\'");  //if all else fails, the token isn't valid
            }
            if(IsOperator(Tokens.Last()) || parenCount != 0)
                throw new FormulaFormatException("");  //if all else fails, the token isn't valid

        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {


            Stack<double> operands = new Stack<double>();
            Stack<string> operators = new Stack<string>();

            double num;
            string errorMessage;
            int currentOperatorRank = 0, previousOperatorRank = -1;
            for (int i = 0; i < Tokens.Count; i++)
            {
                string currentToken = Tokens[i];

                //check if token is a variable
                if (IsVariable(currentToken))
                {
                    try
                    {
                        operands.Push(lookup(currentToken));    //tries to lookup variable and push it onto the stack. If it does not exist, then lookup will throw an error
                    }
                    catch (ArgumentException ex)
                    {
                        return new FormulaError(ex.Message);    //if lookup throws an error, it will be caught and Evaluate will return a corresponding formula error. Its a bit messy/inefficient but it gets the job done I guess...
                    }


                }
                //check if token is a plain integer
                else if (double.TryParse(currentToken, out num))
                {
                    operands.Push(num);

                }
                //check if token is a left parenthesis
                else if (currentToken == "(")
                {
                    operators.Push(currentToken);
                    previousOperatorRank = -1; //sets rank to -1 so that anything outside the parentheses wont be computed until all equations inside the parenthesis have been computed
                }
                //check if token is a right parenthesis
                else if (currentToken == ")")
                {
                    while (operators.Count > 0 && operators.Peek() != "(")//pop and solve everything inside parenthesis until a left parenthesis is reached
                    {
                        if (!TryPopAndSolveExpression(operands, operators, out errorMessage))   //tries to solve expression, if it cant it will return false and output a formula error message
                            return new FormulaError(errorMessage);
                        if (operators.Count <= 0)
                            return new FormulaError("No left parenthesis found");               //if operator stack count is 0, then there exists no left parenthesis to match the found right parenthesis
                    }
                    operators.Pop(); //gets rid of the left parenthesis
                    previousOperatorRank = operators.Count > 0 ? DetermineOperatorRank(operators.Peek()) : -1; //sets rank to -1 if there are no other operators in the stack
                }
                //check if token is an operator
                else if (currentToken == "+" || currentToken == "-" || currentToken == "/" || currentToken == "*")
                {
                    currentOperatorRank = DetermineOperatorRank(currentToken);
                    while(operators.Count > 0 && operands.Count > 1 && currentOperatorRank <= previousOperatorRank)
                    {
                        previousOperatorRank = currentOperatorRank;
                        if (!TryPopAndSolveExpression(operands, operators, out errorMessage))
                            return new FormulaError(errorMessage);
                        if(operators.Count > 1)
                            currentOperatorRank = DetermineOperatorRank(operators.Peek());

                    }

                    previousOperatorRank = currentOperatorRank;
                    operators.Push(currentToken);
                }
            }
            //solve all remaining equations in the stack.
            while (operators.Count > 0)
            {
                if (!TryPopAndSolveExpression(operands, operators, out errorMessage))
                    return new FormulaError(errorMessage);
            }

            return operands.Pop();
        }


        /// <summary>
        /// Pops two operands and one operator out of their respective stacks and solves the equation. 
        /// Result is pushed onto the operand stack. Returns true if successful and returns false if 
        /// expression cannot be solved. If false, an error message will be outputted
        /// </summary>
        private bool TryPopAndSolveExpression(Stack<double> operands, Stack<string> operators, out string message)
        {
            message = "";
            if (operands.Count <= 1)
                return false;
            double result, right = operands.Pop(), left = operands.Pop();
            string op = operators.Pop();

            switch (op)
            {
                case "+":
                    result = left + right;
                    operands.Push(result);
                    return true;
                case "-":
                    result = left - right;
                    operands.Push(result);
                    return true;
                case "*":
                    result = left * right;
                    operands.Push(result);
                    return true;
                case "/":
                    if (right == 0)
                    {
                        message = "Divide by Zero";
                        return false;       //divide by 0 error
                    }
                    result = left / right;
                    operands.Push(result);
                    return true;
                default:
                    return false;

            }
        }
        /// <summary>
        /// Determines the integer ranking of an operator based on PEMDAS.
        /// parenthesis are excluded because they are not an operator.
        /// </summary>
        /// <param name="op">Operator that needs to be ranked</param>
        /// <returns></returns>
        private int DetermineOperatorRank(string op)
        {
            switch (op)
            {
                case "+":
                    return 0;
                case "-":
                    return 1;
                case "*":
                    return 2;
                case "/":
                    return 3;
                default:
                    return -1;
            }
        }


        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            HashSet<string> variables = new HashSet<string>();  //uses a hashset to ensure that there will be no duplicate variables in the returned IEnumerable
            foreach (string token in Tokens)
            {
                if (IsVariable(token))
                    variables.Add(token);
            }
            return variables;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            string returnedString = "";
            foreach (string token in Tokens)
            {
                returnedString += token;    //concatenates all normalized variables, operators, and operands from the underlying list
            }                               //and returns the string
            return returnedString;
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens, which are compared as doubles, and variable tokens,
        /// whose normalized forms are compared as strings.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            double thisNum, otherNum;
            if (obj == null)
                return false;
            if (!Object.ReferenceEquals(this.GetType(), obj.GetType()))
                return false;                                   //if they aren't both Formula objects, then return false

            Formula other = (Formula)obj;                       //casting object as a formula


            string[] thisTokens = this.Tokens.ToArray();
            string[] otherTokens = other.Tokens.ToArray();
            if (thisTokens.Length != otherTokens.Length)        //lengths of arrays must be equal, if not then they are false.
                return false;

            for (int i = 0; i < thisTokens.Length; i++)
            {
                if (double.TryParse(thisTokens[i], out thisNum) && double.TryParse(otherTokens[i], out otherNum) && thisNum == otherNum)    //checking if numbers and comparing doubles
                    continue;
                else if (thisTokens[i] == otherTokens[i])                                                                                   //checking if strings are equal for variables and operators. Variables are already normalized
                    continue;
                else
                    return false;
            }
            return true;    //if it exits the for loop without returning false, then the two formulae must be equal

        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (Object.ReferenceEquals(f1, null) && Object.ReferenceEquals(f2, null))   //return true if both are equal to null
                return true;
            if (!Object.ReferenceEquals(f1, null) && Object.ReferenceEquals(f2, null) || Object.ReferenceEquals(f1, null) && !Object.ReferenceEquals(f2, null)) //return false if only one object is null
                return false;
            return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if (Object.ReferenceEquals(f1, null) && Object.ReferenceEquals(f2, null))   //return false if both objects are null
                return false;
            if (!Object.ReferenceEquals(f1, null) && Object.ReferenceEquals(f2, null) || Object.ReferenceEquals(f1, null) && !Object.ReferenceEquals(f2, null)) //return true if only one object is null
                return true;
            return !f1.Equals(f2);
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode() * Tokens.Count / 1337;   //string hashcodes are more unique than standard object hashcodes
                                                                          //Does additional math to hashcode to ensure that a formula hashcode wont match with a stand alone string hashcode
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }
        private bool IsVariable(string var)
        {
            return Regex.IsMatch(var, @"[a-zA-Z_](?: [a-zA-Z_]|\d)*");
        }
        private bool IsOperator(string token)
        {
            return Regex.IsMatch(token, @"^([\+\-*/])$");
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}