using CauliflowerSpecifics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompilerCampExercise1
{
    public class Phase2
    {
        List<KeyValuePair<string, ThingType>> tokens;
        int pos = 0;
        public Phase2(List<KeyValuePair<string, ThingType>> tokenStream)
        {
            tokens = tokenStream;
        }


        private string ValidateToken(ThingType next)
        {
            if (tokens[pos].Value != next)
            {
                throw new Exception();
            }
            pos++;
            return tokens[pos - 1].Key;
        }

        private string GetOptionalToken(ThingType next)
        {
            if (tokens[pos].Value != next)
            {
                return null;
            }

            pos++;
            return tokens[pos - 1].Key;
        }

        private bool TryNextToken(ThingType next, out string value)
        {
            if (tokens[pos].Value != next)
            {
                value = null;
                return false;
            }

            pos++;
            value = tokens[pos - 1].Key;
            return true;
        }

        private bool CheckNextToken(ThingType next)
        {
            if (tokens[pos].Value != next)
            {
                return false;
            }

            pos++;
            return true;
        }

        private KeyValuePair<string, ThingType> GetTokenUnconditionally()
        {
            pos++;
            return tokens[pos - 1];
        }



        List<Parameter> ParseParams()
        {
            List<Parameter> @params = new List<Parameter>();

            bool first = true;
            while (!CheckNextToken(ThingType.CloseParenthesis))
            {
                if (!first)
                {
                    ValidateToken(ThingType.Comma);
                }
                first = false;
                if (TryNextToken(ThingType.Identifier, out string pType))
                {
                    @params.Add(new Parameter { Type = ParseNamespacedThingStartingWith(pType), Name = ValidateToken(ThingType.Identifier) });
                    continue;
                }
                else
                {
                    throw new Exception();
                }
            }

            return @params;
        }

        public CompilationUnit Parse()
        {
            CompilationUnit cUnit = new CompilationUnit { Namespace = "global" };
            int braces = 0;
            if (CheckNextToken(ThingType.NamespaceKeyword))
            {
                cUnit.Namespace = ValidateToken(ThingType.Identifier);
                ValidateToken(ThingType.OpenCurlyBrace);
                braces++;
            }

            while (pos < tokens.Count)
            {
                if(CheckNextToken(ThingType.CloseCurlyBrace))
                {
                    braces--;
                    continue;
                }
                Class @class = new Class { AccessLevel = AccessLevel.Public };
                cUnit.Classes.Add(@class);

                if (TryNextToken(ThingType.AccessModifier, out string accessMod))
                {
                    if (accessMod == "private")
                    {
                        @class.AccessLevel = AccessLevel.Private;
                    }
                }

                if(CheckNextToken(ThingType.StaticKeyword))
                {
                    @class.Static = true;
                }

                ValidateToken(ThingType.ClassKeyword);

                @class.Name = ValidateToken(ThingType.Identifier);

                if(CheckNextToken(ThingType.OpenParenthesis))
                {
                    if(@class.Static)
                    {
                        throw new Exception("pls no, if its a record then it dont be static");
                    }
                    List<Parameter> fields = ParseParams();
                    List<Declaration> decls = fields.Select(a => new Declaration { Name = a.Name, Type = a.Type }).ToList();
                    @class.InstanceFields = decls;
                    Function func = new Function { AccessLevel = @class.AccessLevel, Name = ".ctor", ReturnType = null, Parameters = fields };
                    @class.InstanceMethods.Add(func);
                    @class.Ctor = func;
                    foreach(Declaration decl in decls)
                    {
                        Assignment assignment = new Assignment();
                        assignment.LHS = new NamespacedThing { Parent = new NamespacedThing { Name = "this", Parent = null }, Name = decl.Name };
                        assignment.RHS = new GetVariableOrField { Value = new NamespacedThing { Parent = null, Name = decl.Name } };
                        func.Statements.Add(assignment);
                    }

                    ValidateToken(ThingType.Semicolon);
                    continue;
                }

                ValidateToken(ThingType.OpenCurlyBrace);
                braces++;

                while (true)
                {
                    if(CheckNextToken(ThingType.CloseCurlyBrace))
                    {
                        braces--;
                        break;
                    }
                    AccessLevel level = AccessLevel.Public;
                    if (TryNextToken(ThingType.AccessModifier, out string acc))
                    {
                        if (acc == "private")
                        {
                            level = AccessLevel.Private;
                        }
                    }

                    bool @static = TryNextToken(ThingType.StaticKeyword, out _);

                    bool isEntryPoint = false;

                    if(@class.Static && !@static)
                    {
                        throw new Exception("if ur class is static ur members gotta be static, too");
                    }

                    if(@static && CheckNextToken(ThingType.EntrypointKeyword))
                    {
                        if(cUnit.EntryPoint != null)
                        {
                            throw new Exception("pls onlee hav won ntree point");
                        }

                        isEntryPoint = true;
                    }

                    NamespacedThing type = ParseNamespacedThing();
                    string name;
                    bool isCtor = false;
                    if(type.Name == @class.Name && type.Parent == null)
                    {
                        if(@class.Ctor != null)
                        {
                            throw new Exception("pls onlee hav won seetorr");
                        }
                        isCtor = true;
                        ValidateToken(ThingType.OpenParenthesis);
                        name = ".ctor";
                    }
                    else
                    {
                        name = ValidateToken(ThingType.Identifier);
                    }

                    if (CheckNextToken(ThingType.Semicolon))
                    {
                        Declaration decl = new Declaration { Name = name, Type = type };
                        if (@static)
                        {
                            @class.StaticFields.Add(decl);
                        }
                        else
                        {
                            @class.InstanceFields.Add(decl);
                        }

                        if(isEntryPoint)
                        {
                            throw new Exception("what is that even supposed to mean");
                        }
                    }

                    else if (CheckNextToken(ThingType.EqualsOperator))
                    {
                        DeclarationAssignment declAssign = new DeclarationAssignment { Name = name, Type = type, Value = ParseExpr(false, false, true, out _) };
                        if (@static)
                        {
                            @class.StaticFields.Add(declAssign);
                        }
                        else
                        {
                            @class.InstanceFields.Add(declAssign);
                        }

                        if (isEntryPoint)
                        {
                            throw new Exception("what is that even supposed to mean");
                        }
                    }

                    else if (isCtor || TryNextToken(ThingType.OpenParenthesis, out _))
                    {
                        Function func = new Function { AccessLevel = level, Name = name, ReturnType = type };
                        if(@static)
                        {
                            @class.StaticMethods.Add(func);
                        }
                        else
                        {
                            @class.InstanceMethods.Add(func);
                        }

                        if(isEntryPoint)
                        {
                            cUnit.EntryPoint = func;
                        }

                        if(isCtor)
                        {
                            @class.Ctor = func;
                        }

                        func.Parameters = ParseParams();

                        ValidateToken(ThingType.OpenCurlyBrace);

                        UnparsedExprCuzLazy expr = new UnparsedExprCuzLazy();
                        int tinyBraces = 1;
                        while(tinyBraces != 0)
                        {
                            var t = GetTokenUnconditionally();
                            if(t.Value == ThingType.OpenCurlyBrace)
                            {
                                tinyBraces++;
                            }
                            else if(t.Value == ThingType.CloseCurlyBrace)
                            {
                                tinyBraces--;
                            }
                            else if(t.Value == ThingType.Semicolon)
                            {
                                func.Statements.Add(new LazyStatement { LazyExpr = expr });
                                expr = new UnparsedExprCuzLazy();
                            }
                            else
                            {
                                expr.Tokens.Add(t);
                            }
                        }

                    }

                    else
                    {
                        throw new Exception();
                    }
                }
            }

            if (braces != 0)
            {
                throw new Exception();
            }

            return cUnit;
        }

        NamespacedThing ParseNamespacedThing()
        {
            return ParseNamespacedThingStartingWith(ValidateToken(ThingType.Identifier));
        }

        NamespacedThing ParseNamespacedThingStartingWith(string start)
        {
            NamespacedThing current = new NamespacedThing();
            string name = start;
            current.Name = name;
            while (CheckNextToken(ThingType.DotOperator))
            {
                NamespacedThing thing = new NamespacedThing();
                thing.Parent = current;
                current = thing;

                name = ValidateToken(ThingType.Identifier);
                current.Name = name;
            }

            return current;
        }


        private List<Statement> ParseStatementsUntilCloseCurlyBrace()
        {
            List<Statement> statements = new List<Statement>();
            while(true)
            {
                if(TryNextToken(ThingType.Identifier, out string ident))
                {
                    NamespacedThing namespacedIdent = ParseNamespacedThingStartingWith(ident);

                    //Declaration
                    if (TryNextToken(ThingType.Identifier, out string name))
                    {
                        if (CheckNextToken(ThingType.EqualsOperator))
                        {
                            Expression value = ParseExpr(false, false, true, out _);

                            statements.Add(new DeclarationAssignment() { Name = name, Type = namespacedIdent, Value = value });
                        }
                        else if (CheckNextToken(ThingType.Semicolon))
                        {
                            statements.Add(new Declaration() { Name = name, Type = namespacedIdent });
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }

                    //Function call
                    else if (CheckNextToken(ThingType.OpenParenthesis))
                    {
                        List<Expression> parameters = ParseParameterExpressionList();

                        statements.Add(new FunctionCall() { FunctionName = namespacedIdent, Parameters = parameters });
                    }

                    //Assignment
                    else if (CheckNextToken(ThingType.EqualsOperator))
                    {
                        Expression value = ParseExpr(false, false, true, out _);

                        statements.Add(new Assignment() { LHS = namespacedIdent, RHS = value });
                    }

                    //Increment
                    else if(CheckNextToken(ThingType.Increment))
                    {
                        statements.Add(new IncrementExpression() { Value = namespacedIdent });
                    }

                    //Decrement
                }
            }
        }


        private static Dictionary<ThingType, int> precedences = new Dictionary<ThingType, int> {
            [ThingType.PlusOperator] = AdditionExpression.Precedence,
            [ThingType.MinusOperator] = SubtractionExpression.Precedence,
            [ThingType.MultiplyOperator] = MultiplicationExpression.Precedence,
            [ThingType.DivideOperator] = DivisionExpression.Precedence
        };

        private static Dictionary<ThingType, Associativity> associativities = new Dictionary<ThingType, Associativity>
        {
            [ThingType.PlusOperator] = AdditionExpression.Associative,
            [ThingType.MinusOperator] = SubtractionExpression.Associative,
            [ThingType.MultiplyOperator] = MultiplicationExpression.Associative,
            [ThingType.DivideOperator] = DivisionExpression.Associative
        };

        private static HashSet<ThingType> operators = new HashSet<ThingType> { ThingType.PlusOperator, ThingType.MinusOperator, ThingType.MultiplyOperator, ThingType.DivideOperator };
        private Expression ParseExpr(bool finishOnCloseParen, bool finishOnComma, bool finishOnSemicolon, out ThingType finishedToken)
        {
            //UnparsedExprCuzLazy unparsedExprCuzLazy = new UnparsedExprCuzLazy();
            //while (!TryNextToken(ThingType.Semicolon, out string semic))
            //{
            //    unparsedExprCuzLazy.Tokens.Add(tokens[pos++]);
            //}
            //finishedToken = ThingType.Semicolon;
            //return unparsedExprCuzLazy;

            Stack<ThingType> opStack = new Stack<ThingType>();
            LinkedList<Expression> output = new LinkedList<Expression>();

            void pushOperatorFromStack()
            {
                ThingType stackTop = opStack.Pop();
                switch (stackTop)
                {
                    case ThingType.PlusOperator:
                        var lastNode = output.Last;
                        output.Remove(lastNode);
                        var last2 = output.Last;
                        output.Remove(last2);
                        output.AddLast(new AdditionExpression { LHS = last2.Value, RHS = lastNode.Value });
                        break;
                    case ThingType.MinusOperator:
                        lastNode = output.Last;
                        output.Remove(lastNode);
                        last2 = output.Last;
                        output.Remove(last2);
                        output.AddLast(new SubtractionExpression { LHS = last2.Value, RHS = lastNode.Value });
                        break;
                    case ThingType.MultiplyOperator:
                        lastNode = output.Last;
                        output.Remove(lastNode);
                        last2 = output.Last;
                        output.Remove(last2);
                        output.AddLast(new MultiplicationExpression { LHS = last2.Value, RHS = lastNode.Value });
                        break;
                    case ThingType.DivideOperator:
                        lastNode = output.Last;
                        output.Remove(lastNode);
                        last2 = output.Last;
                        output.Remove(last2);
                        output.AddLast(new DivisionExpression { LHS = last2.Value, RHS = lastNode.Value });
                        break;
                }
            }
            
            while(true)
            {
                var token = GetTokenUnconditionally();

                if(token.Value == ThingType.IntLiteral)
                {
                    int num = int.Parse(token.Key);
                    output.AddLast(new ConstantIntExpression { Value = num });
                    continue;
                }

                if (token.Value == ThingType.StringLiteral)
                {
                    string val = token.Key;
                    output.AddLast(new ConstantStringExpression { Value = FromStringToken(val) });
                    continue;
                }

                if (token.Value == ThingType.BoolLiteral)
                {
                    bool val = bool.Parse(token.Key);
                    output.AddLast(new ConstantBooleanExpression { Value = val });
                    continue;
                }

                if (token.Value == ThingType.Identifier)
                {
                    NamespacedThing thing = ParseNamespacedThingStartingWith(token.Key);
                    if(CheckNextToken(ThingType.OpenParenthesis))
                    {
                        List<Expression> parameters = ParseParameterExpressionList();

                        output.AddLast(new FunctionCall() { FunctionName = thing, Parameters = parameters });
                    }
                    else
                    {
                        output.AddLast(new GetVariableOrField { Value = thing });
                    }

                    continue;
                }


                if(operators.Contains(token.Value))
                {
                    while(opStack.Count > 0 && ((operators.Contains(opStack.Peek())) && (precedences[opStack.Peek()] > precedences[token.Value]) || (precedences[opStack.Peek()] == precedences[token.Value] && associativities[token.Value] == Associativity.Left)))
                    {
                        pushOperatorFromStack();
                    }

                    opStack.Push(token.Value);
                }

                else if(token.Value == ThingType.OpenParenthesis)
                {
                    opStack.Push(ThingType.OpenParenthesis);
                }

                else if(token.Value == ThingType.CloseParenthesis)
                {
                    while(opStack.Count > 0 && opStack.Peek() != ThingType.OpenParenthesis)
                    {
                        pushOperatorFromStack();
                    }

                    if(opStack.Count == 0)
                    {
                        if(finishOnCloseParen && output.Count == 1)
                        {
                            finishedToken = ThingType.CloseParenthesis;
                            if(output.Count != 1)
                            {
                                throw new Exception();
                            }
                            return output.First.Value;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }

                    opStack.Pop();
                }
                else if(token.Value == ThingType.Semicolon)
                {
                    if(finishOnSemicolon)
                    {
                        finishedToken = ThingType.Semicolon;
                        break;
                    }

                    throw new Exception();
                }
                else if(token.Value == ThingType.Comma)
                {
                    if(finishOnComma)
                    {
                        finishedToken = ThingType.Comma;
                        break;
                    }

                    throw new Exception();
                }
                else
                {
                    throw new Exception();
                }
            }

            while(opStack.Count > 0)
            {
                pushOperatorFromStack();
            }
            if(output.Count != 1)
            {
                throw new Exception();
            }
            return output.First.Value;

        }

        private List<Expression> ParseParameterExpressionList()
        {
            List<Expression> parameters = new List<Expression>();

            if (!CheckNextToken(ThingType.CloseParenthesis))
            {
                ThingType tType;

                do
                {
                    Expression expr = ParseExpr(true, true, false, out tType);
                    parameters.Add(expr);
                } while (tType != ThingType.CloseParenthesis);
            }

            return parameters;
        }

        public static string FromStringToken(string value)
        {
            List<char> chars = new List<char>(value.Length);
            int state = 0;
            value = value.Substring(1, value.Length - 2);
            for(int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch(state)
                {
                    case 0:
                        if(c == '\\')
                        {
                            state = 1;
                            break;
                        }

                        chars.Add(c);
                        break;
                    case 1:
                        switch(c)
                        {
                            case '"':
                            case '\\':
                            case '\'':
                                chars.Add(c);
                                break;
                            case 'a':
                                chars.Add('\a');
                                break;
                            case 'b':
                                chars.Add('\b');
                                break;
                            case 'f':
                                chars.Add('\f');
                                break;
                            case 'n':
                                chars.Add('\n');
                                break;
                            case 'r':
                                chars.Add('\r');
                                break;
                            case 't':
                                chars.Add('\t');
                                break;
                            case 'v':
                                chars.Add('\v');
                                break;
                            default:
                                throw new Exception();
                        }
                        break;
                }
            }

            return new string(chars.ToArray());
        }
    }
}
