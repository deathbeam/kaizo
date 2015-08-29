using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using NLua;
using NuGet;
using System.Net;

namespace Kaizo.Tasks
{
	public class Build : Task
	{
		public Build(Lua lua) : base(lua) { }

		public ProjectRootElement Run(string project) {
			var source = lua [project + ".source"] as string;
			if (source == null) source = "src";

			var resources = lua [project + ".resources"] as string;
			if (resources == null) resources = "res";

			var root = ProjectRootElement.Create ();
			root.AddImport ("$(MSBuildBinPath)\\Microsoft.CSharp.targets");

			var properties = root.AddPropertyGroup ();
			properties.AddProperty ("AssemblyName", lua.GetOptional(project + ".name", "project") as string);
			properties.AddProperty ("ProductVersion", lua.GetOptional(project + ".version", "1.0.0") as string);
			properties.AddProperty ("ProjectGuid", "{" + System.Guid.NewGuid ().ToString () + "}");
			properties.AddProperty ("SchemaVersion", "2.0");

			var luaproperties = lua[project + ".properties"] as LuaTable;

			if (luaproperties.Keys.Count > 0) {
				foreach (string key in luaproperties.Keys) {
					var val = luaproperties[key];
					if (val is string) properties.AddProperty(key.ToFirstUpper(), val as string);
				}
			}

			Logger.Log (project + ".dependencies", ConsoleColor.Magenta);

      PackageManager packages;

      try {
        packages = new PackageManager(PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2"), "packages");
      } catch (WebException e) {
        packages = new PackageManager(new LocalPackageRepository("packages", true), "packages");
      }

			packages.PackageInstalled += delegate(object sender, PackageOperationEventArgs arg) {
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine (" [DONE]");
				Console.ResetColor();
			};

			packages.PackageInstalling += delegate(object sender, PackageOperationEventArgs arg) {
				Console.Write ("Installing " + arg.Package.GetFullName());
			};

			var references = root.AddItemGroup ();
			var dependencies = lua [project + ".dependencies.system"] as LuaTable;

			if (dependencies.Values.Count > 0) {
				foreach (string dep in dependencies.Values) {
					references.AddItem("Reference", dep);
				}
			}

			dependencies = lua [project + ".dependencies.nuget"] as LuaTable;

			if (dependencies.Values.Count > 0) {
				foreach (string dep in dependencies.Values) {
					IPackage dependency = null;

					if (dep.IndexOf (':') > -1) {
						var splitdep = dep.Split (':');
            try { packages.InstallPackage (splitdep [0], SemanticVersion.Parse (splitdep [1])); } catch (WebException e) { }
						dependency = packages.LocalRepository.FindPackage (splitdep [0], SemanticVersion.Parse (splitdep [1]));
					} else {
            try { packages.InstallPackage (dep); } catch (WebException e) { }
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

			dependencies = lua [project + ".dependencies.project"] as LuaTable;

			if (dependencies != null) {
				var projects = root.AddItemGroup();

				foreach (string dep in (dependencies as LuaTable).Values) {
					Task.Call(dep + ".build");
					projects.AddItem("ProjectReference", dep).AddMetadata("name", dep);
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
			ConsoleLogger logger = new ConsoleLogger(LoggerVerbosity.Normal);
			BuildManager manager = BuildManager.DefaultBuildManager;

			manager.Build(
				new BuildParameters() { DetailedSummary = true, Loggers = new[] { logger } },
				new BuildRequestData(projectInstance, new string[] { "Build" }));

			return root;
		}
	}
}
