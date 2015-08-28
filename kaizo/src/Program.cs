using System;
using NLua;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Kaizo.Tasks;
using System.Diagnostics;

namespace Kaizo
{
	class MainClass
	{
		private static Lua lua;
		private static Stopwatch time = new Stopwatch();

		public static void Main (string[] args)
		{
			time.Start ();
			Logger.Log("Build started", ConsoleColor.Magenta, "> ");

			lua = new Lua ();
			lua.LoadCLRPackage ();
			lua.DoString (@"
				package.path = package.path..';./?/project.lua'

				function project(name)
					return module(name, package.seeall)
				end
			");

			var tasks = Assembly.GetExecutingAssembly().GetTypes().Where(
				t => String.Equals(t.Namespace, "Kaizo.Tasks", StringComparison.Ordinal) &&
				!String.Equals(t.Name, "Task", StringComparison.Ordinal)).ToArray();

			foreach (var task in tasks) {
				Activator.CreateInstance(task, lua);
			}

			lua.RegisterFunction ("task", typeof(MainClass).GetMethod("CallTask"));

			try {
				lua.DoFile ("project.lua");
			} catch (Exception e) {
				Fail (e);
			}

			if (args.Length > 0) {
				var cmdtasks = new List<string> (args);
				var cmdargs = new List<string> (args);

				if (args.Contains ("-arg")) {
					int index = cmdargs.IndexOf ("-arg");
					cmdargs.RemoveRange (0, index + 1);
					cmdtasks.RemoveRange (index, args.Length - 1);
				} else {
					cmdargs.Clear ();
				}

				var luaargs = lua.CreateTable ();

				foreach (var cmdarg in cmdargs) {
					var key = cmdarg.Replace ("\"", "").Replace ("'", "").Split ('=');
					luaargs [key [0]] = key [1];
				}

				lua ["arg"] = luaargs;

				foreach (var cmdtask in cmdtasks) {
					Call (cmdtask);
				}
			}

			lua.Dispose ();
			time.Stop ();
			Logger.Log("Build succesfull", ConsoleColor.Green, "> ");
			Logger.Log("Finished in " + time.Elapsed.ToReadableString(), ConsoleColor.Green, "> ");
		}

		public static void Fail(Exception e) {
			lua.Dispose ();
			time.Stop ();
			Logger.Log("Build failed with error:", ConsoleColor.Red, "> ");
			Console.WriteLine (e);
			Logger.Log("Finished in " + time.Elapsed.ToReadableString(), ConsoleColor.Red, "> ");
			Environment.Exit (-1);
		}

		public static object CallTask(string name) {
			return Call (name);
		}

		public static object Call(string name, LuaTable args = null)
		{
			Logger.Log(name, ConsoleColor.Magenta);
			var task = lua.GetFunction (name);
			object result = null;

			var project = name.Split ('.')[0];

			try {
				if (args != null) {
					result = task.Call (project, args);
				} else {
					result = task.Call (project);
				}
			} catch (Exception e) {
				Fail (e);
			}

			return result;
		}
	}
}
