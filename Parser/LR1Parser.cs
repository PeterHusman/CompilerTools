using Parser.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{

    class LR0Item<T> where T : Enum
    {
        public Production<T> Production { get; set; }

        public int DotPosition { get; set; }
    }

    class LR1Item<T> where T : Enum
    {
        public Production<T> Production { get; set; }

        public int DotPosition { get; set; }

        public Symbol<T> LookAhead { get; set; }
    }


    public class LR1Parser<T> where T : Enum
    {
        public Grammar<T> Grammar { get; }

        public LR1Parser(Grammar<T> grammar)
        {
            Grammar = grammar;
        }

        private void SetupParseTable()
        {

        }

        //private List<LR0Item>
    }
}
