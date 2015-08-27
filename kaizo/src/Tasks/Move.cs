using System;
using System.IO;
using NLua;

namespace Kaizo.Tasks
{
	public class Move : Task
	{
		public Move(Lua lua) : base(lua) { }

		public void Run(LuaTable args) {
			if (args == null) return;

			var moveTo = args ["to"] as string;
			var moveFrom = args ["from"];

			if (moveFrom is LuaTable) {
				foreach (string file in (moveFrom as LuaTable).Values) {
					MoveIt (file, moveTo);
				}
			} else {
				MoveIt (moveFrom as string, moveTo);
			}
		}

		private void MoveIt(string src, string dest) {
			if (Directory.Exists (src)) {
				Directory.Move (src, dest);
			} else if (File.Exists (src)) {
				File.Move (src, dest);
			}
		}
	}
}