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

        public override string ToString()
        {
            return TokenType.ToString();
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

    public class TerminalEpsilon<T> : TerminalSymbol<T>, IEquatable<TerminalEpsilon<T>> where T : Enum
    {
        public TerminalEpsilon() : base(default(T))
        {
        }

        public override string ToString()
        {
            return "Epsilon";
        }

        public override bool Equals(object obj)
        {
            return obj is TerminalEpsilon<T>;
        }

        public override int GetHashCode()
        {
            return 100;
        }

        public bool Equals(TerminalEpsilon<T> other)
        {
            return true;
        }
    }
}
