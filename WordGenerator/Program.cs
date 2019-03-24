using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WordGenerator.Generator;

namespace WordGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 1;
            string fileName = "default.json";

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--count":
                    case "-c":
                        i++;
                        if (args.Length == i)
                        {
                            Error("オプションの値がありません．");
                        }

                        if (!int.TryParse(args[i], out count))
                        {
                            Error("回数が数値ではありません．");
                        }
                        break;
                    default:
                        fileName = args[i];
                        break;
                }
            }

            WordGeneratorOption option = null;
            try
            {
                using (var stream = File.OpenText(fileName))
                {
                    option = JsonConvert.DeserializeObject<WordGeneratorOption>(stream.ReadToEnd());
                }
            }
            catch(Exception ex)
            {
                Error("設定ファイルの読み込みに失敗しました．", ex);
            }

            IGenerator generator = null;
            switch (option.Type)
            {
                case "Simple":
                    generator = new SimpleGenerator();
                    break;
                case "Replacement":
                    generator = new ReplacementGenerator();
                    break;
                case "Transition":
                    generator = new TransitionGenerator();
                    break;
                case "ReplacementTransition":
                    generator = new ReplacementTransitionGenerator();
                    break;
                default:
                    generator = new SimpleGenerator();
                    break;
            }

            try
            {
                generator.Settings = option.GeneratorOptions;

                List<string> list = new List<string>();
                int i = 0;
                while (i < count)
                {
                    string result = generator.Create();
                    
                    if (!list.Contains(result))
                    {
                        list.Add(result);
                        i++;
                    }
                    if (result == "" && list.Contains("")) { break; }
                }

                foreach (var item in list)
                {
                    Console.WriteLine(item);
                }
            }
            catch(Exception ex)
            {
                Error("単語文字列の作成に失敗しました．", ex);
            }
        }

        private static void Error(string message, Exception exception = null)
        {
            Console.WriteLine("Error: {0}", message);
            if(exception != null) {
                Console.WriteLine(exception);
            }
            Environment.Exit(1);
        }
    }

    class WordGeneratorOption
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IDictionary<string, object> GeneratorOptions { get; set; }
    }
}
