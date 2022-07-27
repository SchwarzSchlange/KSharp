using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSharp
{
    public class Debug
    {
        public static bool IsDebug = true;
        public static bool IsDeveloperMode = false;


        public static void Warning(string content)
        {
            if(IsDebug)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"[{TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToLongTimeString()}] [Warning] => {content}");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }

        public static void Success(string content)
        {
            if(IsDebug)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[{TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToLongTimeString()}] {content}");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }

        public static void Error(string content)
        { 
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToLongTimeString()}] {content}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.ReadKey();
            Environment.Exit(0);
            Console.WriteLine("Press any key to exit...");

        }

        public static void Info(string content)
        {
            if(IsDebug)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[{TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now).ToLongTimeString()}] {content}");
                Console.ForegroundColor = ConsoleColor.White;
            }

        }



    }
}
