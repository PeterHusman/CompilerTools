using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class Terminal<T> : Node where T : Enum
    {
        public T TokenType { get; set; }
        public string TokenValue { get; set; }

        public override string FullToString => TokenType.ToString();

        public override string ToString()
        {
            return TokenType.ToString();
        }


    }
}
