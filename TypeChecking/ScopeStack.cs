using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeChecking
{
    public class ScopeStack<T1,T2>
    {
        public Stack<Scope<T1, T2>> Scopes { get; set; } = new Stack<Scope<T1, T2>>();

        public Scope<T1, T2> Current => Scopes.Peek();

        public T2 Search(T1 name)
        {
            foreach(Scope<T1, T2> scope in Scopes)
            {
                if(scope.Types.ContainsKey(name))
                {
                    return scope.Types[name];
                }
            }

            return default(T2);
        }

        public bool TrySearch(T1 name, out T2 type)
        {
            foreach (Scope<T1, T2> scope in Scopes)
            {
                if (scope.Types.ContainsKey(name))
                {
                    type = scope.Types[name];
                    return true;
                }
            }

            type = default(T2);
            return false;
        }

        public Scope<T1, T2> Pop()
        {
            return Scopes.Pop();
        }

        public void Push(Scope<T1, T2> toPush)
        {
            Scopes.Push(toPush);
        }

        public void PushNew()
        {
            Scopes.Push(new Scope<T1, T2>());
        }
    }
}
