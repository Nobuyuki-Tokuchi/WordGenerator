using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordGenerator.Generator
{
    /// <summary>
    /// 遷移単語作成
    /// </summary>
    public class TransitionGenerator : IGenerator
    {
        private readonly TransitionGeneratorOption option;
        private readonly Random random;

        private readonly IDictionary<string, int> transitionCount;
        private int wordLengthCount;
        private int prohibitionCount;

        private const string OPTIONS_KEY = "TransitionOptions";
        private const string WORD_LENGTHS_KEY = "WordLengths";
        private const string PROHIBITIONS_KEY = "Prohibitions";

        public TransitionGenerator()
        {
            this.option = new TransitionGeneratorOption();
            this.random = new Random();

            this.transitionCount = new Dictionary<string, int>();
            this.wordLengthCount = 0;
            this.prohibitionCount = 0;
        }

        public IDictionary<string, object> Settings {
            get
            {
                return new Dictionary<string, object>
                {
                    [OPTIONS_KEY] = this.option.TransitionOptions,
                    [WORD_LENGTHS_KEY] = this.option.WordLengths,
                    [PROHIBITIONS_KEY] = this.option.Prohibitions,
                };
            }
            set
            {
                this.option.TransitionOptions = (value.ContainsKey(OPTIONS_KEY) && value[OPTIONS_KEY] is JObject)
                    ? (value[OPTIONS_KEY] as JObject).ToObject<Dictionary<string, IList<string>>>() : new Dictionary<string, IList<string>>();
                this.option.WordLengths = (value.ContainsKey(WORD_LENGTHS_KEY) && value[WORD_LENGTHS_KEY] is JArray)
                    ? (value[WORD_LENGTHS_KEY] as JArray).ToObject<List<int>>() : new List<int>();
                this.option.Prohibitions = (value.ContainsKey(PROHIBITIONS_KEY) && value[PROHIBITIONS_KEY] is JArray)
                    ? (value[PROHIBITIONS_KEY] as JArray).ToObject<List<string>>() : new List<string>();

                foreach (var item in option.TransitionOptions.Select(x => new KeyValuePair<string, int>(x.Key, x.Value.Count)))
                {
                    this.transitionCount.Add(item);
                }
                this.wordLengthCount = option.WordLengths.Count;
                this.prohibitionCount = option.Prohibitions.Count;
            }
        }

        public string Create()
        {
            int length = this.option.WordLengths[this.random.Next(0, this.wordLengthCount)];
            StringBuilder builder = new StringBuilder();
            string result = "";
            bool isInvalid;

            do
            {
                builder.Clear();
                isInvalid = false;

                string str = option.TransitionOptions.Keys.Skip(this.random.Next(0, option.TransitionOptions.Keys.Count - 1)).First();
                for (int i = 0; i < length; i++)
                {
                    str = option.TransitionOptions[str][this.random.Next(0, this.transitionCount[str])];
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

    class TransitionGeneratorOption
    {
        public IDictionary<string, IList<string>> TransitionOptions { get; set; }

        public IList<int> WordLengths { get; set; }

        public IList<string> Prohibitions { get; set; }
    }
}
