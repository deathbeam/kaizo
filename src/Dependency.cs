using System;
using NuGet;
using System.IO;
using System.Collections.Generic;

namespace Kaizo
{
	public class Dependency
	{
		private readonly static PackageManager packages;

		static Dependency()
		{
			packages = new PackageManager(
				PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2"),
				Path.Combine(Directory.GetCurrentDirectory(), "packages"));

			packages.PackageInstalled += delegate(object sender, PackageOperationEventArgs args) {
				Console.WriteLine (" [DONE]");
			};
				
			packages.PackageInstalling += delegate(object sender, PackageOperationEventArgs args) {
				Console.Write ("Installing " + args.Package.GetFullName() + "...");
			};

			packages.PackageUninstalled += delegate(object sender, PackageOperationEventArgs args) {

			};

			packages.PackageUninstalling += delegate(object sender, PackageOperationEventArgs args) {

			};
		}

		public static IEnumerable<IPackage> All
		{
			get { return packages.LocalRepository.GetPackages (); }
		}

		public static void Install(string key)
		{
			if (key.IndexOf (':') > -1) {
				var dependency = key.Split (':');
				packages.InstallPackage (dependency [0], SemanticVersion.Parse (dependency [1]));
			} else {
				packages.InstallPackage (key);
			}
		}
	}
}