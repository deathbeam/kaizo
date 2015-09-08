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
    public static string HOME;
    public static string CURRENT;
    public static ConsoleColor COLOR = ConsoleColor.Magenta;
    public static Version VERSION = Assembly.GetExecutingAssembly().GetName().Version;

    private static Lua lua;
    private static Stopwatch time = new Stopwatch();

    public static void Main (string[] args) {
      if (args.Contains("-h")) {
        var arglist = new List<string>(args);
        int index = arglist.IndexOf("-h");
        HOME = arglist [index + 1];
        arglist.RemoveAt(index);
        arglist.RemoveAt(index);
        args = arglist.ToArray();
      }

      if (args.Contains("-d")) {
        var arglist = new List<string>(args);
        int index = arglist.IndexOf("-d");
        CURRENT = arglist [index + 1];
        arglist.RemoveAt(index);
        arglist.RemoveAt(index);
        args = arglist.ToArray();
      }

      if (HOME == null) HOME = Directory.GetCurrentDirectory();
      if (CURRENT == null)  CURRENT = HOME;
      Directory.SetCurrentDirectory(CURRENT);

      if (args.Length == 0) args = new[] { "self.build" };

      if (args [0] == "help") {
        Logger.Default
          .Log("> ", false, COLOR).Log ("Available commands:")
          .Log("./kaizow ", false, COLOR).Log ("help                       - display this help message")
          .Log("./kaizow ", false, COLOR).Log ("update (<directory>)       - update kaizo from git repository or from <directory>")
          .Log("./kaizow ", false, COLOR).Log ("<tasks> (-arg <arguments>) - run <tasks> with optional <arguments>");
        return;
      } else if (args [0] == "version") {
        Logger.Default.Log ("Kaizo ", false).Log(VERSION, true, ConsoleColor.Magenta);
        return;
      }

      time.Start ();
      Logger.Default.Log("> ", false, COLOR).Log("Build started");
      Console.Write("Loading build script");

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
      LuaFunction project = null;

      try {
        project = lua.LoadFile (Path.Combine(CURRENT, "project.lua"));
      } catch (LuaScriptException e) {
        Fail (e);
      }

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

      Console.ForegroundColor = ConsoleColor.Magenta;
      Console.WriteLine(" [DONE]");
      Console.ResetColor();

      project.Call();

      foreach (var cmdtask in cmdtasks) {
        try {
          Task.Call (cmdtask);
        } catch (LuaScriptException e) {
          Fail (e);
        } catch (NotImplementedException e) {
          Fail (e.Message);
        }
      }

      Logger.Default.Log ("> ", false, ConsoleColor.Green).Log ("Build succesfull");
      Finish ();
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
      Logger.Default.Log("> ", false, ConsoleColor.Red).Log("Build failed with error:");
      Console.WriteLine (message);
      Finish ();
      Environment.Exit (-1);
    }

    public static void Finish() {
      lua.Dispose ();
      time.Stop ();
      Logger.Default.Log("> ", false, COLOR).Log("Finished in " + time.Elapsed.ToReadableString());
    }
  }
}
