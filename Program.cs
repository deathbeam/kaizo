using System;
using NLua;
using System.IO;
using System.Collections.Generic;

namespace Kaizo
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			using (Lua lua = new Lua ()) {
				lua.LoadCLRPackage ();
				lua.DoFile (Path.Combine(Directory.GetCurrentDirectory(), "project.lua"));

				var dependencies = lua ["dependencies"] as LuaTable;

				foreach (string key in dependencies.Values) {
					Dependency.Install (key);
				}

				new Project (
					lua ["name"] as string,
					lua ["version"] as string,
					lua ["namespace"] as string).Build();

				if (args.GetLength(0) > 0) {
					var task = lua [args[0]] as LuaFunction;
					var list = new List<string> (args);
					list.RemoveAt (0);
					args = list.ToArray ();
					task.Call (args);
				}
			}
		}
	}
}