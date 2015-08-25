using System;
using NLua;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Kaizo.Tasks;

namespace Kaizo
{
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
					Activator.CreateInstance<Task>(task, lua);
				}

				lua.DoFile (Path.Combine(Directory.GetCurrentDirectory(), "project.lua"));

				foreach (string key in (lua ["dependencies"] as LuaTable).Values) {
					Dependency.Install (key);
				}

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