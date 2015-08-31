using System;

namespace Kaizo
{
	public class Logger
	{
    public static Logger Default { get; }

    static Logger() {
      Default = new Logger();
    }

    public Logger Log(object message, bool endLine = true, ConsoleColor color = ConsoleColor.White) {

      if (color != ConsoleColor.White) Console.ForegroundColor = color;
      Console.Write (message.ToString());
      if (color != ConsoleColor.White) Console.ResetColor ();

      if (endLine) Console.WriteLine ();

      return this;
		}
	}
}