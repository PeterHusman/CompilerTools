using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CauliflowerSpecifics;
using Parser;

namespace TypeChecking
{
    public class MethodInformation
    {
        public TypeTypes ReturnType { get; set; }
        public string Name { get; set; }
        public List<ParameterInformation> Parameters { get; set; }

        internal static MethodInformation FromNontermin\u0061l(NonterminalNode<ThingType> member, TypeTypes retType = null)
        {
            MethodInformation methodInfo = new MethodInformation();

            methodInfo.Name = member.Children.Select(a => a as Terminal<ThingType>).Where(a => a != null && a.TokenType == ThingType.Identifier).Select(a => a.TokenValue).First();
            if(!member.Children.Any(a => a is NonterminalNode<ThingType> nonterm && nonterm.Name == "TypeName"))
            {
                methodInfo.Name = ".ctor";
            }

            if (retType != null)
            {
                methodInfo.ReturnType = retType;
            }
            else
            {
                methodInfo.ReturnType = TypeTypes.FromTypeNameNonterminal(TypeTypes.GetChild(member, "TypeName"));
            }

            var paramsInParens = TypeTypes.GetChild(member, "ParamsInParens");

            methodInfo.Parameters = new List<ParameterInformation>();

            if (paramsInParens.Children.Length < 3)
            {
                return methodInfo;
            }

            var paramsList = paramsInParens.Children[1] as NonterminalNode<ThingType>;

            for(int i = 0; i < paramsList.Children.Length; i++)
            {
                if(paramsList.Children[i] is Terminal<ThingType>)
                {
                    continue;
                }

                var parameter = paramsList.Children[i] as NonterminalNode<ThingType>;
                ParameterInformation param = ParameterInformation.FromNonterminal(parameter);
                if(methodInfo.Parameters.Any(a => a.Name == param.Name))
                {
                    throw new Exception("Duplicate parameter name");
                }
                methodInfo.Parameters.Add(param);
            }

            return methodInfo;
        }
    }
}
