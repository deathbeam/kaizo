using System;
using NLua;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Kaizo.Tasks;
using System.Diagnostics;
using NLua.Exceptions;

namespace Kaizo
{
	class MainClass
	{
    private static Lua lua;
		private static Stopwatch time = new Stopwatch();

		public static void Main (string[] args)
		{
      if (args.Length > 0) {
        if (args [0] == "help") {
          Logger.Log ("Available commands:", ConsoleColor.Magenta, "> ");
          Logger.Log ("help \t                    - display this help message", ConsoleColor.Magenta, "./kaizow ");
          Logger.Log ("update (<directory>)       - update kaizo from git repository or from <directory>", ConsoleColor.Magenta, "./kaizow ");
          Logger.Log ("<tasks> (-arg <arguments>) - run <tasks> with optional <arguments>", ConsoleColor.Magenta, "./kaizow ");
          return;
        } else if (args [0] == "version") {
          Logger.Log ("v" + Environment.Version.ToString(), ConsoleColor.Magenta, "Kaizo ");
          return;
        }
      }

			time.Start ();
			Logger.Log("Build started", ConsoleColor.Magenta, "> ");

			lua = new Lua ();
			lua.LoadCLRPackage ();
			lua.DoString (@"
				package.path = package.path..';./?/project.lua'

				properties = {}
				dependencies = {
					project = {},
					system = {},
					nuget = {}
				}

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

      lua.RegisterFunction ("task", typeof(Task).GetMethod("Call"));

			try {
				lua.DoFile ("project.lua");
      } catch (LuaScriptException e) {
				Fail (e);
			}

      if (args.Length == 0) args = new[] { "self.build" };

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
        try {
          Task.Call (cmdtask);
        } catch (LuaScriptException e) {
          Fail (e);
        } catch (NotImplementedException e) {
          Fail (e.Message);
        }
			}

			lua.Dispose ();
			time.Stop ();
			Logger.Log("Build succesfull", ConsoleColor.Green, "> ");
      Logger.Log("Finished in " + time.Elapsed.ToReadableString(), ConsoleColor.Magenta, "> ");
		}

    public static Lua GetLua() {
      return lua;
    }

    public static void Fail(Exception e) {
      if (e is LuaScriptException) {
        var le = e as LuaScriptException;

        if (le.IsNetException) {
          Fail(le.InnerException.ToString());
        } else {
          Fail (e.ToString ());
        }
      } else {
        Fail (e.ToString ());
      }
		}

    public static void Fail(string message) {
      lua.Dispose ();
      time.Stop ();
      Logger.Log("Build failed with error:", ConsoleColor.Red, "> ");
      Console.WriteLine (message);
      Logger.Log("Finished in " + time.Elapsed.ToReadableString(), ConsoleColor.Magenta, "> ");
      Environment.Exit (-1);
    }
	}
}
