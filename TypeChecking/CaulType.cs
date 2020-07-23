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

        public static CaulType Void { get; } = new CaulType() { Name = "void" };

        public static CaulType Int { get; } = new CaulType() { Name = "int" };

        public static CaulType String { get; } = new CaulType() { Name = "string" };

        public static CaulType Bool { get; } = new CaulType() { Name = "bool" };

        public static CaulType FromNonterminal(NonterminalNode<ThingType> node)
        {
            return new CaulType() { Name = ((Terminal<ThingType>)((NonterminalNode<ThingType>)node.Children[0]).Children.Last()).TokenValue };
            //throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
