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

                    if(@class.Static && !@static)
                    {
                        throw new Exception("if ur class is static ur members gotta be static, too");
                    }

                    string type = ValidateToken(ThingType.Identifier);
                    string name = ValidateToken(ThingType.Identifier);

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
                    }

                    else if (CheckNextToken(ThingType.EqualsOperator))
                    {
                        DeclarationAssignment declAssign = new DeclarationAssignment { Name = name, Type = type, Value = ParseExpr() };
                        if (@static)
                        {
                            @class.StaticFields.Add(declAssign);
                        }
                        else
                        {
                            @class.InstanceFields.Add(declAssign);
                        }
                    }

                    else if (TryNextToken(ThingType.OpenParenthesis, out _))
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

                        bool first = true;

                        while (!CheckNextToken(ThingType.CloseParenthesis))
                        {
                            if(!first)
                            {
                                ValidateToken(ThingType.Comma);
                            }
                            first = false;
                            if (TryNextToken(ThingType.Identifier, out string pType))
                            {
                                func.Parameters.Add(new Parameter { Type = pType, Name = ValidateToken(ThingType.Identifier) });
                                continue;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }

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

        private Expression ParseExpr()
        {
            UnparsedExprCuzLazy unparsedExprCuzLazy = new UnparsedExprCuzLazy();
            while (!TryNextToken(ThingType.Semicolon, out string semic))
            {
                unparsedExprCuzLazy.Tokens.Add(tokens[pos++]);
            }
            return unparsedExprCuzLazy;
        }
    }
}
