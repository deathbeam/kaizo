using System;
using NLua;
using System.Reflection;

namespace Kaizo.Tasks
{
	public abstract class Task
	{
		protected Lua lua;

		public Task(Lua lua) {
			this.lua = lua;
			string name = this.GetType ().Name.ToLower ();
			MethodInfo method = this.GetType ().GetMethod ("Execute");
			lua.RegisterFunction(name, this, method);
		}

		public abstract void Execute(LuaTable args);
	}
}