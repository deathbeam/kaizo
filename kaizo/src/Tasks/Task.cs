using System;
using NLua;
using System.Reflection;
using System.Collections.Generic;

namespace Kaizo.Tasks
{
	public abstract class Task
	{
    public static Dictionary<string, Task> All { get; }
		protected Lua lua;
		protected string name;

    static Task() {
      All = new Dictionary<string, Task> ();
    }

    public Task(Lua lua) {
      this.lua = lua;
			name = this.GetType ().Name.ToLower ();
			MethodInfo method = this.GetType ().GetMethod ("Run");
			All[name] = this;
			lua.RegisterFunction(name, this, method);
		}

    public static object Call(string name, LuaTable args = null)
    {
      Logger.Log(name, ConsoleColor.Magenta);
      var task = MainClass.GetLua().GetFunction (name);

      if (task == null) {
        throw new NotImplementedException ("Unknown task '" + name + "'");
      }

      object result = null;

      var project = name.Split ('.')[0];

      try {
        if (args != null) {
          result = task.Call (project, args);
        } else {
          result = task.Call (project);
        }
      } catch (Exception e) {
        MainClass.Fail (e);
      }

      return result;
    }
	}
}
