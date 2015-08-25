using System;
using NLua;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Kaizo.Tasks;

namespace Kaizo
{
	public static class LuaExtensions
	{
		public static LuaTable CreateTable(this Lua lua)
		{
			return (LuaTable)lua.DoString("return {}")[0];
		}
	} 

	class MainClass
	{
		public static void Main (string[] args)
		{
			using (Lua lua = new Lua ()) {
				lua.LoadCLRPackage ();

				var tasks = Assembly.GetExecutingAssembly().GetTypes().Where(
					t => String.Equals(t.Namespace, "Kaizo.Tasks", StringComparison.Ordinal) &&
					!String.Equals(t.Name, "Task", StringComparison.Ordinal)).ToArray();

				foreach (var task in tasks) {
					Activator.CreateInstance(task, lua);
				}

				lua.DoFile (Path.Combine(Directory.GetCurrentDirectory(), "project.lua"));

				foreach (string key in (lua ["dependencies"] as LuaTable).Values) {
					Dependency.Install (key);
				}

				if (args.Length > 0) {
					var task = lua.GetFunction(args[0]);

					if (args.Length > 1) {
						var list = new List<string> (args);
						list.RemoveAt (0);
						args = list.ToArray ();

						var table = lua.CreateTable ();

						foreach (var arg in args) {
							var key = arg.Split (':');
							table [key [0]] = key [1];
						}

						task.Call (table);
					} else {
						task.Call ();
					}
				}
			}
		}
	}
}