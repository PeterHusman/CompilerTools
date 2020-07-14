using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser.Symbols
{
    public class TerminalSymbol<T> : Symbol<T> where T : Enum
    {
        public T TokenType { get; }

        public TerminalSymbol(T tokenType)
        {
            TokenType = tokenType;
        }
    }
}
