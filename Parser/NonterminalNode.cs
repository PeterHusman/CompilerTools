using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class NonterminalNode<T> : Node where T : Enum
    {
        public Node[] Children { get; set; }

        public NonterminalNode(Node[] children, string name, Production<T> producer)
        {
            Children = children;
            Name = name;
            Producer = producer;
        }

        public string Name { get; }

        public Production<T> Producer { get; }
    }
}
