using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CompilerCampExercise1
{
    public class Tokenizer
    {
        public List<KeyValuePair<ThingType, Regex>> Regexes { get; set; } = new List<KeyValuePair<ThingType, Regex>>();

        public HashSet<ThingType> Useless { get; set; }

        public Tokenizer(Dictionary<ThingType, string> notRealRegexes, HashSet<ThingType> useless)
        {
            foreach (ThingType type in notRealRegexes.Keys.OrderBy(a => (int)a))
            {
                Regexes.Add(new KeyValuePair<ThingType, Regex>(type, new Regex($"\\G({notRealRegexes[type]})", RegexOptions.Compiled)));
            }

            Useless = useless;
        }

        public List<KeyValuePair<string, ThingType>> Tokenize(string input)
        {
            List<KeyValuePair<string, ThingType>> thingies = new List<KeyValuePair<string, ThingType>>();

            int startingPos = 0;

            //string remaining = input;

            //Again, do not use
            while (startingPos < input.Length)
            {
                KeyValuePair<ThingType, Regex>? type = Regexes.FirstOrDefault(a => a.Value.IsMatch(input, startingPos));

                if (type == null || type.Value.Value == null)
                {
                    throw new Exception($"lol invalid, get urself a real token and come back ({input.Substring(startingPos, 10)})");
                }
                ThingType thingType = type.Value.Key;
                Regex regex = type.Value.Value;
                string match = regex.Match(input, startingPos).Value;
                if (!Useless.Contains(thingType))
                {
                    thingies.Add(new KeyValuePair<string, ThingType>(match, thingType));
                }

                //remaining = remaining.Remove(0, match.Length);
                startingPos += match.Length;
            }

            return thingies;
        }
    }
}
