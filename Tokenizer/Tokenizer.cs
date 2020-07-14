using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tokenizer
{
    public class Tokenizer<T> where T : Enum
    {
        public List<KeyValuePair<T, Regex>> Regexes { get; set; } = new List<KeyValuePair<T, Regex>>();

        public HashSet<T> Useless { get; set; }

        public Tokenizer(Dictionary<T, string> notRealRegexes, HashSet<T> useless, Func<T, int> precedence)
        {
            foreach (T type in notRealRegexes.Keys.OrderBy(a => precedence(a)))
            {
                Regexes.Add(new KeyValuePair<T, Regex>(type, new Regex($"\\G({notRealRegexes[type]})", RegexOptions.Compiled)));
            }

            Useless = useless;
        }

        public List<KeyValuePair<string, T>> Tokenize(string input)
        {
            List<KeyValuePair<string, T>> thingies = new List<KeyValuePair<string, T>>();

            int startingPos = 0;

            //string remaining = input;

            //Again, do not use
            while (startingPos < input.Length)
            {
                KeyValuePair<T, Regex>? type = Regexes.FirstOrDefault(a => a.Value.IsMatch(input, startingPos));

                if (type == null || type.Value.Value == null)
                {
                    throw new Exception($"lol invalid, get urself a real token and come back ({input.Substring(startingPos, 10)})");
                }
                T thingType = type.Value.Key;
                Regex regex = type.Value.Value;
                string match = regex.Match(input, startingPos).Value;
                if (!Useless.Contains(thingType))
                {
                    thingies.Add(new KeyValuePair<string, T>(match, thingType));
                }

                //remaining = remaining.Remove(0, match.Length);
                startingPos += match.Length;
            }

            return thingies;
        }
    }
}
