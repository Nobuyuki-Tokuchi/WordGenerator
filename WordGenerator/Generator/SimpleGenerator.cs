using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace WordGenerator.Generator
{
    /// <summary>
    /// 単純単語文字列作成
    /// </summary>
    public class SimpleGenerator : IGenerator
    {
        private readonly SimpleGeneratorOption option;
        private readonly Random random;

        private int letterCount;
        private int wordLengthCount;
        private int prohibitionCount;

        private const string LETTERS_KEY = "Letters";
        private const string WORD_LENGTHS_KEY = "WordLengths";
        private const string PROHIBITIONS_KEY = "Prohibitions";

        public SimpleGenerator()
        {
            this.option = new SimpleGeneratorOption();
            this.random = new Random();

            this.letterCount = 0;
            this.wordLengthCount = 0;
            this.prohibitionCount = 0;
        }

        public IDictionary<string, object> Settings {
            get
            {
                return new Dictionary<string, object>
                {
                    [LETTERS_KEY] = new ReadOnlyCollection<string>(option.Letters),
                    [WORD_LENGTHS_KEY] = new ReadOnlyCollection<int>(option.WordLengths),
                    [PROHIBITIONS_KEY] = new ReadOnlyCollection<string>(option.Prohibitions),
                };
            }
            set
            {
                option.Letters = (value.ContainsKey(LETTERS_KEY) && value[LETTERS_KEY] is JArray)
                    ? (value[LETTERS_KEY] as JArray).ToObject<List<string>>() : new List<string>();
                option.WordLengths = (value.ContainsKey(WORD_LENGTHS_KEY) && value[WORD_LENGTHS_KEY] is JArray)
                    ? (value[WORD_LENGTHS_KEY] as JArray).ToObject<List<int>>() : new List<int>();
                option.Prohibitions = (value.ContainsKey(PROHIBITIONS_KEY) && value[PROHIBITIONS_KEY] is JArray)
                    ? (value[PROHIBITIONS_KEY] as JArray).ToObject<List<string>>() : new List<string>();

                this.letterCount = option.Letters.Count;
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

                for (int i = 0; i < length; i++)
                {
                    builder.Append(option.Letters[this.random.Next(0, this.letterCount)]);
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

    class SimpleGeneratorOption
    {
        public IList<string> Letters { get; set; }

        public IList<int> WordLengths { get; set; }

        public IList<string> Prohibitions { get; set; }
    }
}
