using CauliflowerSpecifics;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeChecking
{
    public class CaulType
    {
        public string Name { get; set; }

        public static CaulType FromNonterminal(NonterminalNode<ThingType> node)
        {
            return new CaulType() { Name = ((Terminal<ThingType>)((NonterminalNode<ThingType>)node.Children[0]).Children.Last()).TokenValue };
            //throw new NotImplementedException();
        }
    }
}
