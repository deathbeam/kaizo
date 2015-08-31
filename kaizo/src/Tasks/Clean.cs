using System.IO;
using NLua;

namespace Kaizo.Tasks
{
  public class Clean : Task
  {
    public Clean(Lua lua) : base(lua) { }

    public void Run(string project) {
      var output = lua [project + ".configuration.outputPath"];

      if (output != null) {
        var outputPath = Path.Combine(MainClass.CURRENT, output as string);

        if (Directory.Exists(outputPath)) DirectoryDelete(outputPath);
      }

      var objPath = Path.Combine (MainClass.CURRENT, "obj");
      if (Directory.Exists(objPath)) {
        DirectoryDelete(objPath);
      }
    }

    private static void DirectoryDelete(string src) {
      DirectoryInfo dir = new DirectoryInfo(src);
      DirectoryInfo[] dirs = dir.GetDirectories();

      if (!dir.Exists) {
        return;
      }

      FileInfo[] files = dir.GetFiles();

      foreach (FileInfo file in files) {
        file.Delete ();
      }

      foreach (DirectoryInfo subdir in dirs) {
        DirectoryDelete(subdir.FullName);
      }
    }
  }
}
