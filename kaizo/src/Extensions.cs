using System;
using NLua;

namespace Kaizo
{
	public static class Extensions
	{
		public static object GetOptional(this Lua lua, string key, object defaultValue)
		{
			var result = lua [key];
			if (result == null) return defaultValue;
			return result;
		}

		public static LuaTable CreateTable(this Lua lua)
		{
			return (LuaTable)lua.DoString("return {}")[0];
		}

		public static string ToFirstUpper(this string str)
		{
			if (str.Length > 1) {
				return char.ToUpper(str[0]) + str.Substring(1);
			}

			return str.ToUpper();
		}

		public static string ToReadableString(this TimeSpan span)
		{
			string formatted = string.Format("{0}{1}{2}{3}",
				span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
				span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
				span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
				span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

			if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

			if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

			return formatted;
		}
	}
}
