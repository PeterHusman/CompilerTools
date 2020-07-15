using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Symbols
{
    public class TerminalSymbol<T> : Symbol<T>, IEquatable<TerminalSymbol<T>> where T : Enum
    {
        public T TokenType { get; }

        public TerminalSymbol(T tokenType)
        {
            TokenType = tokenType;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is TerminalSymbol<T> term))
            {
                return false;
            }

            return term.TokenType.Equals(TokenType);
        }

        public bool Equals(TerminalSymbol<T> other)
        {
            return other != null &&
                   EqualityComparer<T>.Default.Equals(TokenType, other.TokenType);
        }

        public override int GetHashCode()
        {
            return -1502943246 + EqualityComparer<T>.Default.GetHashCode(TokenType);
        }
    }
}
