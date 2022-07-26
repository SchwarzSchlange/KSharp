﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSharp
{
    class Program
    {
        public const string VERSION = "1.7";
        public static int LINE_RUN_DELAY = 1;
        public static string SCRIPT_PATH;
        public static List<Line> LastLines = new List<Line>();

        public static int CurrentReadingLine = 0;

        static void Main(string[] args)
        {
            InitilazeConsole();

            Console.Write(">>> ");
            SCRIPT_PATH = Console.ReadLine();

            if(!File.Exists(SCRIPT_PATH))
            {
                Debug.Error($"{SCRIPT_PATH} doesn't exitsts!");
                return;
            }

            LastLines = Parser.ParseFile(SCRIPT_PATH);


            for(CurrentReadingLine = 0; CurrentReadingLine < LastLines.Count; CurrentReadingLine++)
            {
                Engine.ConvertAllGlobals(LastLines[CurrentReadingLine].Tokens);
                Engine.ConvertAllVariables(LastLines[CurrentReadingLine].Tokens);
                Engine.ConvertAllMath(LastLines[CurrentReadingLine].Tokens);

                if(Debug.IsDeveloperMode)
                {
                    Console.WriteLine($"Line {LastLines[CurrentReadingLine].LineIndex}");
                    foreach (Token token in LastLines[CurrentReadingLine].Tokens)
                    {
                        Console.WriteLine($"[{token.INDEX}] | {token.TYPE} | {token.Root.LineIndex} | {token.VALUE} | {token.STATIC_VALUE} | {token.LAYER} | {token.UNION}");
                    }
                    Console.WriteLine($"---------------------------");
                    Console.WriteLine(Environment.NewLine);
                }


                Engine.RunTokens(LastLines[CurrentReadingLine],LastLines[CurrentReadingLine].Tokens);

            }

            Debug.Info("Program has been ended...");
            Console.ReadLine();
        }

        private static void InitilazeConsole()
        {
            Console.OutputEncoding = Encoding.Default;
            Console.Title = $"K# | {VERSION} | Developer Mode : " + Debug.IsDeveloperMode;
            
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
