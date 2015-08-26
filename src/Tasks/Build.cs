using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;
using NLua;
using NuGet;
using Microsoft.Build.Framework;

namespace Kaizo.Tasks
{
	public class Build : Task
	{
		public Build(Lua lua) : base(lua) { }

		public override object Execute(LuaTable args = null) {
			string name = (args != null) ? args ["name"] as string : lua ["name"] as string;
			if (name == null) name = lua ["name"] as string;

			string namspace = (args != null) ? args ["natablemespace"] as string : lua ["namespace"] as string;
			if (namspace == null) namspace = lua ["namespace"] as string;

			string version = (args != null) ? args ["version"] as string : lua ["version"] as string;
			if (version == null) version = lua ["version"] as string;

			var root = ProjectRootElement.Create ();
			root.AddImport ("$(MSBuildBinPath)\\Microsoft.CSharp.targets");

			var group = root.AddPropertyGroup ();
			group.AddProperty ("Configuration", "Release");
			group.AddProperty ("Platform", "x86");
			group.AddProperty ("PlatformTarget", "x86");
			group.AddProperty ("RootNamespace", namspace);
			group.AddProperty ("AssemblyName", name);
			group.AddProperty ("ProductVersion", version);
			group.AddProperty ("OutputPath", "bin");
			group.AddProperty ("OutputType", "exe");
			group.AddProperty ("ProjectGuid", "{" + System.Guid.NewGuid ().ToString () + "}");
			group.AddProperty ("TargetFrameworkVersion", "v4.0");
			group.AddProperty ("SchemaVersion", "2.0");

			var packages = new PackageManager(PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2"), "packages");

			packages.PackageInstalled += delegate(object sender, PackageOperationEventArgs arg) {
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine (" [DONE]");
				Console.ResetColor();
			};

			packages.PackageInstalling += delegate(object sender, PackageOperationEventArgs arg) {
				Console.Write ("Installing " + arg.Package.GetFullName() + "...");
			};

			packages.PackageUninstalled += delegate(object sender, PackageOperationEventArgs arg) {

			};

			packages.PackageUninstalling += delegate(object sender, PackageOperationEventArgs arg) {

			};

			var references = root.AddItemGroup ();
			var dependencies = (lua ["dependencies"] as LuaTable).Values;

			foreach (string key in dependencies) {
				if (key.IndexOf (':') > -1) {
					var splitkey = key.Split (':');
					packages.InstallPackage (splitkey [0], SemanticVersion.Parse (splitkey [1]));
					var dependency = packages.LocalRepository.FindPackage (splitkey [0], SemanticVersion.Parse (splitkey [1]));

					foreach (var reference in dependency.AssemblyReferences) {
						foreach (var frmwrk in reference.SupportedFrameworks) {
							if (frmwrk.Version == new Version (4, 0)) {
								references.AddItem ("Reference", Path.GetFileNameWithoutExtension(reference.Name),
									new KeyValuePair<string, string>[] {
										new KeyValuePair<string, string> (
											"HintPath", 
											Path.Combine ("packages", dependency.Id + "." + dependency.Version.ToString(), reference.Path))
									}
								);
							}
						}
					}
				} else {
					references.AddItem("Reference", key);
				}
			}

			if (Directory.Exists (Path.Combine (Directory.GetCurrentDirectory (), "src"))) {
				var compile = root.AddItemGroup ();

				foreach (var file in Directory.GetFiles (Path.Combine (Directory.GetCurrentDirectory (), "src"), "*.cs", SearchOption.AllDirectories)) {
					compile.AddItem ("Compile", file);
				}
			}

			ProjectInstance project = new ProjectInstance (root);
			ConsoleLogger logger = new ConsoleLogger(LoggerVerbosity.Minimal);
			BuildManager manager = BuildManager.DefaultBuildManager;

			manager.Build(
				new BuildParameters() { DetailedSummary = true, Loggers = new[] { logger } }, 
				new BuildRequestData(project, new string[] { "Build" }));

			return project.FullPath;
		}
	}
}