using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class NonterminalNode<T> : Node, IEquatable<NonterminalNode<T>> where T : Enum
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

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as NonterminalNode<T>);
        }

        public bool Equals(NonterminalNode<T> other)
        {
            return other != null &&
                   //EqualityComparer<Node[]>.Default.Equals(Children, other.Children) &&
                   Children.SequenceEqual(other.Children, EqualityComparer<Node>.Default) &&
                   Name == other.Name;
        }

        public override int GetHashCode()
        {
            var hashCode = 317540552;
            //hashCode = hashCode * -1521134295 + EqualityComparer<Node[]>.Default.GetHashCode(Children);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }

        public override string FullToString
        {
            get
            {
                if(Children.Length == 0)
                {
                    return "";
                }
                return Children.Select(a => a.FullToString).Aggregate((a, b) => $"{a} {b}");
            }
        }
    }
}
