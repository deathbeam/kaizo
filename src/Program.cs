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
			args = new[] { "echo", "message=hello" };
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

				-- Usage: echo message:<your_message>
				function echo(args)
				  build()
				  print(args.message)
				end
			";

			Stopwatch time = new Stopwatch();
			time.Start ();
			Console.WriteLine ("Build started");

			using (Lua lua = new Lua ()) {
				lua.LoadCLRPackage ();

				var tasks = Assembly.GetExecutingAssembly().GetTypes().Where(
					t => String.Equals(t.Namespace, "Kaizo.Tasks", StringComparison.Ordinal) &&
					!String.Equals(t.Name, "Task", StringComparison.Ordinal)).ToArray();

				foreach (var task in tasks) {
					Activator.CreateInstance(task, lua);
				}

				lua.DoString (file); //lua.DoFile (Path.Combine(Directory.GetCurrentDirectory(), "project.lua"));

				if (args.Length > 0) {
					var task = lua.GetFunction(args[0]);

					if (args.Length > 1) {
						var list = new List<string> (args);
						list.RemoveAt (0);
						args = list.ToArray ();

						var table = lua.CreateTable ();

						foreach (var arg in args) {
							var key = arg.Replace("\"", "").Replace("'", "").Split ('=');
							table [key [0]] = key [1];
						}

						task.Call (table);
					} else {
						task.Call ();
					}
				}
			}

			time.Stop ();
			Console.WriteLine ("Build finished in " + time.Elapsed);
		}
	}
}