using System;

namespace Kaizo
{
	public class Logger
	{
		public static void Log(string message, ConsoleColor color = ConsoleColor.Magenta, string separator = ":")
		{
			Console.ForegroundColor = color;
			Console.Write (separator);
			Console.ResetColor ();
			Console.WriteLine (message);
		}
	}
}