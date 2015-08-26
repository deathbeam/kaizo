using System;
using System.IO;
using NLua;

namespace Kaizo.Tasks
{
	public class Copy : Task
	{
		public Copy(Lua lua) : base(lua) { }

		public override object Execute(LuaTable args) {
			if (args == null) return null;

			var copyTo = args ["to"] as string;
			var copyFrom = args ["from"];

			if (copyFrom is LuaTable) {
				foreach (string file in (copyFrom as LuaTable).Values) {
					CopyIt (file, copyTo);
				}
			} else {
				CopyIt (copyFrom as string, copyTo);
			}

			return null;
		}

		private void CopyIt(string src, string dest) {
			if (Directory.Exists (src)) {
				DirectoryCopy (src, dest);
			} else if (File.Exists (src)) {
				File.Copy (src, dest, true);
			}
		}

		private void DirectoryCopy(string src, string dest)
		{
			DirectoryInfo dir = new DirectoryInfo(src);
			DirectoryInfo[] dirs = dir.GetDirectories();

			if (!dir.Exists) {
				return;
			}

			if (!Directory.Exists(dest)) {
				Directory.CreateDirectory(dest);
			}
				
			FileInfo[] files = dir.GetFiles();

			foreach (FileInfo file in files) {
				file.CopyTo(Path.Combine(dest, file.Name), false);
			}

			foreach (DirectoryInfo subdir in dirs) {
				DirectoryCopy(subdir.FullName, Path.Combine(dest, subdir.Name));
			}
		}
	}
}