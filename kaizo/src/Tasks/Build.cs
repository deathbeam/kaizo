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

		public void Run(string project) {
			var name = lua [project + ".name"] as string;
			if (name == null) name = "project";

			var version = lua [project + ".version"] as string;
			if (version == null) version = "1.0.0";

			var nspace = lua [project + ".configuration.namespace"] as string;
			if (nspace == null) nspace = name;

			var deploy = lua [project + ".configuration.deploy"] as string;
			if (deploy == null) deploy = "Release";

			var platform = lua [project + ".configuration.platform"] as string;
			if (platform == null) platform = "anycpu";

			var type = lua [project + ".configuration.type"] as string;
			if (type == null) type = "exe";

			var source = lua [project + ".configuration.source"] as string;
			if (source == null) source = "src";

			var output = lua [project + ".configuration.output"] as string;
			if (output == null) output = "out";

			var resources = lua [project + ".configuration.resources"] as string;
			if (resources == null) resources = "res";

			var framework = lua [project + ".configuration.framework"] as string;
			if (framework == null) framework = "v4.0";

			var root = ProjectRootElement.Create ();
			root.AddImport ("$(MSBuildBinPath)\\Microsoft.configuration.targets");

			var group = root.AddPropertyGroup ();
			group.AddProperty ("Configuration", deploy);
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

			Logger.Log (project + ".dependencies", ConsoleColor.Magenta);

			var packages = new PackageManager(PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2"), "packages");

			packages.PackageInstalled += delegate(object sender, PackageOperationEventArgs arg) {
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine (" [DONE]");
				Console.ResetColor();
			};

			packages.PackageInstalling += delegate(object sender, PackageOperationEventArgs arg) {
				Console.Write ("Installing " + arg.Package.GetFullName());
			};

			var references = root.AddItemGroup ();
			var dependencies = lua [project + ".dependencies.system"];

			if (dependencies != null) {
				foreach (string dep in (dependencies as LuaTable).Values) {
					references.AddItem("Reference", dep);
				}
			}

			dependencies = lua [project + ".dependencies.nuget"];

			if (dependencies != null) {
				foreach (string dep in (dependencies as LuaTable).Values) {
					IPackage dependency = null;

					if (dep.IndexOf (':') > -1) {
						var splitdep = dep.Split (':');
						packages.InstallPackage (splitdep [0], SemanticVersion.Parse (splitdep [1]));
						dependency = packages.LocalRepository.FindPackage (splitdep [0], SemanticVersion.Parse (splitdep [1]));

					} else {
						packages.InstallPackage (dep);
						dependency = packages.LocalRepository.FindPackage (dep);
					}

					foreach (var reference in dependency.AssemblyReferences) {
						foreach (var frmwrk in reference.SupportedFrameworks) {
							if (frmwrk.Version == new Version (4, 0)) {
								references
									.AddItem ("Reference", Path.GetFileNameWithoutExtension (reference.Name))
									.AddMetadata ("HintPath", Path.Combine ("packages", dependency.Id + "." + dependency.Version.ToString (), reference.Path));
							}
						}
					}
				}
			}

			dependencies = lua [project + ".dependencies.project"];

			if (dependencies != null) {
				var projects = root.AddItemGroup();

				foreach (string dep in (dependencies as LuaTable).Values) {
					MainClass.Call(dep + ".build");
					projects.AddItem("ProjectReference", dep + ".csproj").AddMetadata("name", dep);
				}
			}

			if (Directory.Exists (source)) {
				var compile = root.AddItemGroup ();

				foreach (var file in Directory.GetFiles (source, "*.cs", SearchOption.AllDirectories)) {
					compile.AddItem ("Compile", file);
				}
			}

			if (Directory.Exists (resources)) {
				var none = root.AddItemGroup ();

				foreach (var file in Directory.GetFiles (source, "*", SearchOption.AllDirectories)) {
					none.AddItem ("None", file).AddMetadata ("CopyToOutputDirectory", "PreserveNewest");
				}
			}

			ProjectInstance projectInstance = new ProjectInstance (root);
			root.Save(Path.Combine(output, name + ".csproj"));
			ConsoleLogger logger = new ConsoleLogger(LoggerVerbosity.Quiet);
			BuildManager manager = BuildManager.DefaultBuildManager;

			manager.Build(
				new BuildParameters() { DetailedSummary = true, Loggers = new[] { logger } },
				new BuildRequestData(projectInstance, new string[] { "Build" }));
		}
	}
}
