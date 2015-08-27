using System;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.IO.Compression;
using Internals;
using System.Reflection;

namespace Kaizow
{
	class MainClass
	{
		private static readonly string HOME = Path.Combine(Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), ".kaizo");
		private static readonly string URL = "https://github.com/nondev/kaizo/archive/master.zip";
		private static readonly string ZIP = Path.Combine(HOME, "kaizo.zip");
		private static readonly string BOOT = Path.Combine(HOME, "kaizo-master", "bootstrap");

		public static void Main (string[] args)
		{
			if (!Directory.Exists (BOOT) || (args.Length > 0 && args[0] == "update"))
			{
				Directory.CreateDirectory (HOME);

				using (var client = new WebClient ()) {
					client.DownloadFile (URL, ZIP);
				}

				using (var unzip = new Unzip (ZIP)) {
					unzip.ExtractToDirectory (HOME);
				}

				File.Delete (ZIP);
			}

			Assembly.Load(File.ReadAllBytes(Path.Combine(BOOT, "KopiLua.dll")));
			Assembly.Load(File.ReadAllBytes(Path.Combine(BOOT, "Microsoft.Web.XmlTransform.dll")));
			Assembly.Load(File.ReadAllBytes(Path.Combine(BOOT, "NLua.dll")));
			Assembly.Load(File.ReadAllBytes(Path.Combine(BOOT, "NuGet.Core.dll")));
			Assembly.Load (File.ReadAllBytes (Path.Combine (BOOT, "kaizo.exe"))).EntryPoint.Invoke(null, new[] { args });
		}
	}
}