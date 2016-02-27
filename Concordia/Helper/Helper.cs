using System;


namespace Concordia
{
    public static class Helper
    {
        public static void WriteError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
        }

        public static void WriteWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Warning: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
        }

        public static void WriteCommand(string text)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[Command]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(text + "\n");
        }

    }
}
