using System;
using NLua;
using System.Reflection;
using System.Collections.Generic;

namespace Kaizo.Tasks
{
	public abstract class Task
	{
		public static Dictionary<string, Task> Get = new Dictionary<string, Task> ();
		protected Lua lua;
		protected string name;

		public Task(Lua lua) {
			this.lua = lua;
			name = this.GetType ().Name.ToLower ();
			MethodInfo method = this.GetType ().GetMethod ("Execute");
			Get[name] = this;
			lua.RegisterFunction(name, this, method);
		}

		public abstract object Execute(LuaTable args = null);
	}
}
