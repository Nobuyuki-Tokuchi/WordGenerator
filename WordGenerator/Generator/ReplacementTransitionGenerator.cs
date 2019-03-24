using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGenerator.Generator
{
    /// <summary>
    /// 置換遷移単語文字列作成
    /// </summary>
    public class ReplacementTransitionGenerator : IGenerator
    {
        private readonly ReplacementTransitionGeneratorOption option;
        private readonly Random random;

        private readonly IDictionary<string, int> replacementCount;
        private readonly IDictionary<string, IDictionary<string, int>> transitionCount;
        private int createPatternCount;
        private int prohibitionCount;

        private const string REPLACEMENT_OPTIONS_KEY = "ReplacementOptions";
        private const string TRANSITION_OPTIONS_KEY = "TransitionOptions";
        private const string PATTERNS_KEY = "CreatePatterns";
        private const string PROHIBITIONS_KEY = "Prohibitions";


        public ReplacementTransitionGenerator()
        {
            this.option = new ReplacementTransitionGeneratorOption();
            this.random = new Random();

            this.replacementCount = new Dictionary<string, int>();
            this.transitionCount = new Dictionary<string, IDictionary<string, int>>();
            this.createPatternCount = 0;
            this.prohibitionCount = 0;
        }
        public IDictionary<string, object> Settings
        {
            get
            {
                return new Dictionary<string, object>
                {
                    [TRANSITION_OPTIONS_KEY] = this.option.TransitionOptions,
                    [PATTERNS_KEY] = this.option.CreatePatterns,
                    [PROHIBITIONS_KEY] = this.option.Prohibitions,
                };
            }
            set
            {
                this.option.ReplacementOptions = (value.ContainsKey(REPLACEMENT_OPTIONS_KEY) && value[REPLACEMENT_OPTIONS_KEY] is JObject)
                    ? (value[REPLACEMENT_OPTIONS_KEY] as JObject).ToObject<Dictionary<string, IList<string>>>()
                    : new Dictionary<string, IList<string>>();
                this.option.TransitionOptions = (value.ContainsKey(TRANSITION_OPTIONS_KEY) && value[TRANSITION_OPTIONS_KEY] is JObject)
                    ? (value[TRANSITION_OPTIONS_KEY] as JObject).ToObject<Dictionary<string, IDictionary<string, IList<string>>>>()
                    : new Dictionary<string, IDictionary<string, IList<string>>>();
                this.option.CreatePatterns = (value.ContainsKey(PATTERNS_KEY) && value[PATTERNS_KEY] is JArray)
                    ? (value[PATTERNS_KEY] as JArray).ToObject<List<string>>() : new List<string>();
                this.option.Prohibitions = (value.ContainsKey(PROHIBITIONS_KEY) && value[PROHIBITIONS_KEY] is JArray)
                    ? (value[PROHIBITIONS_KEY] as JArray).ToObject<List<string>>() : new List<string>();

                foreach (var item in option.ReplacementOptions.Select(x => new KeyValuePair<string, int>(x.Key, x.Value.Count)))
                {
                    this.replacementCount.Add(item);
                }

                foreach (var transitions in option.TransitionOptions)
                {
                    string key = transitions.Key;
                    IDictionary<string, int> dictionary = new Dictionary<string, int>();

                    foreach (var replacement in transitions.Value.Select(x => new KeyValuePair<string, int>(x.Key, x.Value.Count)))
                    {
                        dictionary.Add(replacement.Key, replacement.Value);
                    }

                    this.transitionCount.Add(key, dictionary);
                }
                this.createPatternCount = option.CreatePatterns.Count;
                this.prohibitionCount = option.Prohibitions.Count;
            }
        }

        public string Create()
        {
            string pattern = this.option.CreatePatterns[this.random.Next(0, this.createPatternCount)];
            StringBuilder builder = new StringBuilder();
            string result = "";
            bool isInvalid;

            do
            {
                if (pattern.Length == 0) { break; }

                builder.Clear();
                isInvalid = false;

                string str = option.ReplacementOptions[pattern[0].ToString()]
                    .Skip(this.random.Next(0, this.replacementCount[pattern[0].ToString()] - 1)).First();
                builder.Append(str);

                foreach (var c in pattern.Skip(1))
                {
                    string ch = c.ToString();
                    str = this.option.TransitionOptions[str][ch][this.random.Next(0, this.transitionCount[str][ch])];
                    builder.Append(str);
                }

                result = builder.ToString();
                foreach (var prohibition in option.Prohibitions)
                {
                    if (prohibition.StartsWith("^"))
                    {
                        isInvalid |= result.StartsWith(prohibition.Substring(1));
                    }
                    else if (prohibition.EndsWith("$"))
                    {
                        isInvalid |= result.EndsWith(prohibition.Substring(0, prohibition.Length - 1));
                    }
                    else
                    {
                        isInvalid |= result.Contains(prohibition);
                    }

                    if (isInvalid) { break; }
                }
            } while (isInvalid);

            return result;
        }
    }

    class ReplacementTransitionGeneratorOption
    {
        public IDictionary<string, IList<string>> ReplacementOptions { get; set; }

        public IDictionary<string, IDictionary<string, IList<string>>> TransitionOptions { get; set; }

        public IList<string> CreatePatterns { get; set; }

        public IList<string> Prohibitions { get; set; }
    }
}
