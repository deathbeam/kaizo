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
			args = new[] { "compile", "-arg", "message=hello" };
			string file = @"
				name = 'kaizo'
				version = '0.0.1'
				namespace = 'Kaizo'

				csharp = {
					type = 'exe',
					source = '../../src',
					output = 'out',
					configuration = 'Release',
					namespace = 'Kaizo',
					platform = 'x86',
					framework = 'v4.0'
				}

				dependencies = {
					'System',
					'System.Core',
					'Microsoft.Build',
					'Microsoft.Build.Framework',
					'NLua_Safe:*',
					'Mono.NuGet.Core:*',
					'Microsoft.Web.Xdt:*'
				}

				function copydll()
					copy{
						from = 'NLua.dll',
						to = 'packages/NLua.dll'
					}
				end

				function compile()
					task('copydll')
					task('build')
					print(arg.message)
				end
			";

			time.Start ();
			Print("> ", "Build started", ConsoleColor.Magenta);

			lua = new Lua ();
			lua.LoadCLRPackage ();

			var tasks = Assembly.GetExecutingAssembly().GetTypes().Where(
				t => String.Equals(t.Namespace, "Kaizo.Tasks", StringComparison.Ordinal) &&
				!String.Equals(t.Name, "Task", StringComparison.Ordinal)).ToArray();

			foreach (var task in tasks) {
				Activator.CreateInstance(task, lua);
			}

			lua.RegisterFunction ("task", typeof(MainClass).GetMethod("CallTask"));

			try {
				lua.DoString (file); //lua.DoFile (Path.Combine(Directory.GetCurrentDirectory(), "project.lua"));
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
			} else {
				Call ("build");
			}

			lua.Dispose ();
			time.Stop ();
			Print("> ", "Build succesfull", ConsoleColor.Green);
			Print("> ", "Finished in " + time.Elapsed.ToReadableString(), ConsoleColor.Green);
		}

		public static void Print(string separator, string message, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.Write (separator);
			Console.ResetColor ();
			Console.WriteLine (message);
		}

		public static void Fail(Exception e) {
			lua.Dispose ();
			time.Stop ();
			Print("> ", "Build failed with error:", ConsoleColor.Red);
			Console.WriteLine (e);
			Print("> ", "Finished in " + time.Elapsed.ToReadableString(), ConsoleColor.Red);
			Environment.Exit (-1);
		}

		public static object CallTask(string name) {
			return Call (name);
		}

		public static object Call(string name, LuaTable args = null)
		{
			Print (":", name, ConsoleColor.Magenta);
			var task = lua.GetFunction (name);
			object result = null;

			try {
				if (args != null) {
					result = task.Call (args);
				} else {
					result = task.Call ();
				}
			} catch (Exception e) {
				Fail (e);
			}

			return result;
		}
	}
}
