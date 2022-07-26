using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSharp
{
    class Program
    {
        public const string VERSION = "1.1";
        public static string SCRIPT_PATH;
        public static List<Line> LastLines = new List<Line>();

        static void Main(string[] args)
        {
            InitilazeConsole();

            Console.Write("Script Directory : ");
            SCRIPT_PATH = Console.ReadLine();

            if(!File.Exists(SCRIPT_PATH))
            {
                Debug.Error($"{SCRIPT_PATH} doesn't exitsts!");
                return;
            }

            LastLines = Parser.ParseFile(SCRIPT_PATH);

            //Console.Clear();

            foreach(Line line in LastLines)
            {
                Engine.ConvertAllGlobals(line.Tokens);
                Engine.ConvertAllVariables(line.Tokens);
                Engine.ConvertAllMath(line.Tokens);

                /*
                Console.WriteLine($"Line {line.LineIndex}");
                foreach(Token token in line.Tokens)
                {
                    Console.WriteLine($"[{token.INDEX}] | {token.TYPE} | {token.Root.LineIndex} | {token.VALUE} | {token.STATIC_VALUE} | {token.LAYER}");
                }
                Console.WriteLine($"---------------------------");
                */

                Engine.RunTokens(line.Tokens);

            }

            Debug.Info("Program has been ended...");
            Console.ReadLine();
        }

        private static void InitilazeConsole()
        {
            Console.Title = $"KSharp | Kaan Temizkan | {VERSION}";
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
