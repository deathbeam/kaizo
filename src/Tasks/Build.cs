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
			var name = lua ["name"] as string;
			if (name == null) name = "project";

			var version = lua ["version"] as string;
			if (version == null) version = "1.0.0";

			var nspace = lua ["csharp.namespace"] as string;
			if (nspace == null) nspace = name;

			var configuration = lua ["csharp.configuration"] as string;
			if (configuration == null) configuration = "Release";

			var platform = lua ["csharp.platform"] as string;
			if (platform == null) platform = "anycpu";

			var type = lua ["csharp.type"] as string;
			if (type == null) type = "exe";

			var source = lua ["csharp.source"] as string;
			if (source == null) source = "src";

			var output = lua ["csharp.output"] as string;
			if (output == null) output = "out";

			var framework = lua ["csharp.framework"] as string;
			if (framework == null) framework = "v4.0";

			var root = ProjectRootElement.Create ();
			root.AddImport ("$(MSBuildBinPath)\\Microsoft.CSharp.targets");


			var group = root.AddPropertyGroup ();
			group.AddProperty ("Configuration", configuration);
			group.AddProperty ("Platform", platform);
			group.AddProperty ("PlatformTarget", platform);
			group.AddProperty ("RootNamespace", nspace);
			group.AddProperty ("AssemblyName", name);
			group.AddProperty ("ProductVersion", version);
			group.AddProperty ("OutputPath", output);
			group.AddProperty ("OutputType", type);
			group.AddProperty ("ProjectGuid", "{" + System.Guid.NewGuid ().ToString () + "}");
			group.AddProperty ("TargetFrameworkVersion", framework);
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

					IPackage dependency = null;

					if (splitkey[1] == "*") {
						packages.InstallPackage (splitkey [0]);
						dependency = packages.LocalRepository.FindPackage (splitkey [0]);
					} else {
						packages.InstallPackage (splitkey [0], SemanticVersion.Parse (splitkey [1]));
						dependency = packages.LocalRepository.FindPackage (splitkey [0], SemanticVersion.Parse (splitkey [1]));
					}

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

			if (Directory.Exists (source)) {
				var compile = root.AddItemGroup ();

				foreach (var file in Directory.GetFiles (source, "*.cs", SearchOption.AllDirectories)) {
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
