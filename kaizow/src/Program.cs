using System;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.IO.Compression;
using Internals;
using System.Reflection;
using System.Collections.Generic;

namespace Kaizow
{
	class MainClass
	{
    private static string HOME_DIR = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile));
    private static string HOME_ORIG = Path.Combine (HOME_DIR, "kaizo-master");
    private static string HOME = Path.Combine (HOME_DIR, ".kaizo");
		private static string URL = "http://github.com/nondev/kaizo/archive/master.zip";
    private static string ZIP = Path.Combine(HOME_DIR, "kaizo.zip");
		private static string BOOT = Path.Combine(HOME, "bootstrap");

		public static void Main (string[] args)
		{
			bool doUpdate = args.Length > 0 && args[0] == "update";

			if (!Directory.Exists (HOME) || (doUpdate))
			{
        if (Directory.Exists (HOME)) {
          Console.Write("Deleting " + HOME);
          DirectoryDelete (HOME);
          Console.ForegroundColor = ConsoleColor.Magenta;
          Console.WriteLine(" [DONE]");
          Console.ResetColor();
        }

        if (args.Length > 1) {
          Console.Write("Copying " + args[1]);
          DirectoryCopy (args [1], HOME);
          Console.ForegroundColor = ConsoleColor.Magenta;
          Console.WriteLine(" [DONE]");
          Console.ResetColor();
          Console.WriteLine ("Installed to " + HOME);
          return;
        }

				Console.Write("Downloading " + URL);

				using (var client = new WebClient ()) {
					client.DownloadFile (URL, ZIP);
				}

				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine(" [DONE]");
				Console.ResetColor();

				Console.Write("Extracting " + ZIP);
				using (var unzip = new Unzip (ZIP)) {
					unzip.ExtractToDirectory (HOME_DIR);
				}

        Directory.Move (HOME_ORIG, HOME);
				File.Delete (ZIP);

				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine(" [DONE]");
				Console.ResetColor();
        Console.WriteLine ("Installed to " + HOME);

				if (doUpdate) return;
			}

      Assembly.LoadFile(Path.Combine(BOOT, "KopiLua.dll"));
      Assembly.LoadFile(Path.Combine(BOOT, "Microsoft.Web.XmlTransform.dll"));
      Assembly.LoadFile(Path.Combine(BOOT, "NLua.dll"));
      Assembly.LoadFile(Path.Combine(BOOT, "NuGet.Core.dll"));

			var kaizo = Assembly.LoadFile (Path.Combine (BOOT, "kaizo.exe"));

			var finalargs = new List<string>(args);
      finalargs.Add ("-d");
      finalargs.Add (Directory.GetCurrentDirectory ());

      kaizo.GetType("Kaizo.MainClass").GetMethod("Main", BindingFlags.Public | BindingFlags.Static).Invoke (null, new[] { (string[])finalargs.ToArray() });
    }

    private static void DirectoryDelete(string src)
    {
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

    private static void DirectoryCopy(string src, string dest)
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
