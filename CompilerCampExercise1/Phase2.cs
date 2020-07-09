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
                        DeclarationAssignment declAssign = new DeclarationAssignment { Name = name, Type = type, Value = ParseExpr() };
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
