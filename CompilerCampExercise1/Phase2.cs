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
            if(tokens[pos].Value != next)
            {
                throw new Exception();
            }
            pos++;
            return tokens[pos - 1].Key;
        }

        private string GetOptionalToken(ThingType next)
        {
            if(tokens[pos].Value != next)
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

        public void Parse()
        {
            CompilationUnit cUnit = new CompilationUnit { Namespace = "global" };
            int braces = 0;
            if(CheckNextToken(ThingType.NamespaceKeyword))
            {
                cUnit.Namespace = ValidateToken(ThingType.Identifier);
                ValidateToken(ThingType.OpenCurlyBrace);
                braces++;
            }

            while(pos < tokens.Count)
            {
                Class @class = new Class { AccessLevel = AccessLevel.Public };
                cUnit.Classes.Add(@class);
                
                if(TryNextToken(ThingType.AccessModifier, out string accessMod))
                {
                    if(accessMod == "private")
                    {
                        @class.AccessLevel = AccessLevel.Private;
                    }
                }

                ValidateToken(ThingType.ClassKeyword);

                @class.Name = ValidateToken(ThingType.Identifier);

                ValidateToken(ThingType.OpenCurlyBrace);
                braces++;

                while(true)
                {
                    AccessLevel level = AccessLevel.Public;
                    if(TryNextToken(ThingType.AccessModifier, out string acc))
                    {
                        if(acc == "private")
                        {
                            level = AccessLevel.Private;
                        }
                    }

                    bool @static = TryNextToken(ThingType.StaticKeyword, out _);

                    string type = ValidateToken(ThingType.Identifier);
                    string name = ValidateToken(ThingType.Identifier);

                    if (CheckNextToken(ThingType.Semicolon))
                    {
                        @class.InstanceFields.Add(new Declaration { Name = name, Type = type });
                    }

                    else if (CheckNextToken(ThingType.EqualsOperator))
                    {
                        @class.InstanceFields.Add(new DeclarationAssignment { Name = name, Type = type, Value = ParseExpr() });
                    }

                    else if (TryNextToken(ThingType.OpenParenthesis, out _))
                    {
                        Function func = new Function { AccessLevel = level, Name = name, ReturnType = type };

                        while(true)
                        {
                            if(TryNextToken(ThingType.Identifier, out string pType))
                            {
                                func.Parameters.Add(new Parameter { Type = pType, Name = ValidateToken(ThingType.Identifier) });
                                continue;
                            }
                        }
                    }

                    else
                    {
                        throw new Exception();
                    }
                }
            }

            if(braces != 0)
            {
                throw new Exception();
            }
        }

        private Expression ParseExpr()
        {
            UnparsedExprCuzLazy unparsedExprCuzLazy = new UnparsedExprCuzLazy();
            while(!TryNextToken(ThingType.Semicolon, out string semic))
            {
                unparsedExprCuzLazy.Tokens.Add(tokens[pos++]);
            }
            return unparsedExprCuzLazy;
        }
    }
}
