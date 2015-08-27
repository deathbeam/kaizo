using System.IO;
using NLua;

namespace Kaizo.Tasks
{
	public class Clean : Task
	{
		public Clean(Lua lua) : base(lua) { }

		public void Run(string project) {
      var output = lua [project + ".csharp.output"] as string;
			if (output == null) output = "out";

			var resources = lua [project + ".csharp.resources"] as string;
			if (resources == null) resources = "res";

			Directory.Delete(output, true);
			Directory.Delete(resources, true);
    }
	}
}
