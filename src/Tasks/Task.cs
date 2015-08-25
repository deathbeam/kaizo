using System;
using NLua;

namespace Kaizo.Tasks
{
	public abstract class Task
	{
		protected Lua lua;

		public Task(Lua lua) {
			this.lua = lua;
			lua.RegisterFunction(this.GetType().Name.ToLower(), this, this.GetType().GetMethod("Execute"));
		}

		public abstract void Execute(LuaTable args);
	}
}