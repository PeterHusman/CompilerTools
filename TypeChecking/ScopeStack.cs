using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeChecking
{
    public class ScopeStack
    {
        public Stack<Scope> Scopes { get; set; } = new Stack<Scope>();

        public Scope Current => Scopes.Peek();

        public TypeTypes Search(string name)
        {
            foreach(Scope scope in Scopes)
            {
                if(scope.Types.ContainsKey(name))
                {
                    return scope.Types[name];
                }
            }

            return null;
        }

        public bool TrySearch(string name, out TypeTypes type)
        {
            foreach (Scope scope in Scopes)
            {
                if (scope.Types.ContainsKey(name))
                {
                    type = scope.Types[name];
                    return true;
                }
            }

            type = null;
            return false;
        }

        public Scope Pop()
        {
            return Scopes.Pop();
        }

        public void Push(Scope toPush)
        {
            Scopes.Push(toPush);
        }

        public void PushNew()
        {
            Scopes.Push(new Scope());
        }
    }
}
