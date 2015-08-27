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
		private static readonly string EXE = Path.Combine(HOME, "kaizo-master", "bootstrap", "kaizo.exe");

		public static void Main (string[] args)
		{
			Directory.CreateDirectory (HOME);

			using (var client = new WebClient ())
			{
				client.DownloadFile (URL, ZIP);
			}

			using (var unzip = new Unzip(ZIP))
			{
				unzip.ExtractToDirectory(HOME);
			}

			File.Delete (ZIP);

			Assembly assembly = Assembly.Load(File.ReadAllBytes(EXE));
			assembly.EntryPoint.Invoke(null, args);
		}
	}
}