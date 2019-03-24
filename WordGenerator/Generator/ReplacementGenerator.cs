using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace WordGenerator.Generator
{
    /// <summary>
    /// 置換単語作成
    /// </summary>
    public class ReplacementGenerator : IGenerator
    {
        private readonly ReplacementGeneratorOption option;
        private readonly Random random;

        private readonly IDictionary<string, int> replacementCount;
        private int createPatternCount;
        private int prohibitionCount;

        private const string OPTIONS_KEY = "ReplacementOptions";
        private const string PATTERNS_KEY = "CreatePatterns";
        private const string PROHIBITIONS_KEY = "Prohibitions";

        public ReplacementGenerator()
        {
            this.option = new ReplacementGeneratorOption();
            this.random = new Random();

            this.replacementCount = new Dictionary<string, int>();
            this.createPatternCount = 0;
            this.prohibitionCount = 0;
        }

        public IDictionary<string, object> Settings {
            get
            {
                return new Dictionary<string, object>
                {
                    [OPTIONS_KEY] = this.option.ReplacementOptions,
                    [PATTERNS_KEY] = this.option.CreatePatterns,
                    [PROHIBITIONS_KEY] = this.option.Prohibitions,
                };
            }
            set
            {
                this.option.ReplacementOptions = (value.ContainsKey(OPTIONS_KEY) && value[OPTIONS_KEY] is JObject)
                    ? (value[OPTIONS_KEY] as JObject).ToObject<Dictionary<string, IList<string>>>() : new Dictionary<string, IList<string>>();
                this.option.CreatePatterns = (value.ContainsKey(PATTERNS_KEY) && value[PATTERNS_KEY] is JArray)
                    ? (value[PATTERNS_KEY] as JArray).ToObject<List<string>>() : new List<string>();
                this.option.Prohibitions = (value.ContainsKey(PROHIBITIONS_KEY) && value[PROHIBITIONS_KEY] is JArray)
                    ? (value[PROHIBITIONS_KEY] as JArray).ToObject<List<string>>() : new List<string>();

                foreach (var item in option.ReplacementOptions.Select(x => new KeyValuePair<string, int>(x.Key, x.Value.Count)))
                {
                    this.replacementCount.Add(item);
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
                builder.Clear();
                isInvalid = false;

                foreach (var c in pattern)
                {
                    string str = c.ToString();
                    IList<string> caseLetters = this.option.ReplacementOptions[str];
                    builder.Append(caseLetters[this.random.Next(0, this.replacementCount[str])]);
                }
                
                result = builder.ToString();
                foreach (var prohibition in option.Prohibitions)
                {
                    if(prohibition.StartsWith("^"))
                    {
                        isInvalid |= result.StartsWith(prohibition.Substring(1));
                    }
                    else if(prohibition.EndsWith("$"))
                    {
                        isInvalid |= result.EndsWith(prohibition.Substring(0, prohibition.Length - 1));
                    }
                    else
                    {
                        isInvalid |= result.Contains(prohibition);
                    }

                    if(isInvalid) { break; }
                }
            } while (isInvalid);

            return result;
        }
    }

    class ReplacementGeneratorOption
    {
        public IDictionary<string, IList<string>> ReplacementOptions { get; set; }

        public IList<string> CreatePatterns { get; set; }

        public IList<string> Prohibitions { get; set; }
    }
}
