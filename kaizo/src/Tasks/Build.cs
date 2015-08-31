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

			var luaproperties = lua[project + ".configuration"] as LuaTable;

			if (luaproperties.Keys.Count > 0) {
				foreach (string key in luaproperties.Keys) {
					var val = luaproperties[key];
					if (val is string) properties.AddProperty(key.ToFirstUpper(), val as string);
				}
			}

      Logger.Default.Log(":", false, MainClass.COLOR).Log (project + ".dependencies");

      PackageManager packages;
      var packagedir = Path.Combine (MainClass.HOME, "packages");

      try {
        new WebClient().OpenRead("http://packages.nuget.org/api/v2");
        packages = new PackageManager(PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2"), packagedir);
      } catch (WebException e) {
        packages = new PackageManager(PackageRepositoryFactory.Default.CreateRepository(packagedir), packagedir);
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
                  .AddMetadata ("HintPath", Path.Combine (MainClass.HOME, "packages", dependency.Id + "." + dependency.Version.ToString (), reference.Path));
							}
						}
					}
				}
			}

			dependencies = lua [project + ".dependencies.project"];

			if (dependencies != null) {
				foreach (string dep in (dependencies as LuaTable).Values) {
          var projPath = lua[dep + ".configuration.outputPath"];
					if (projPath == null) continue;

					var projName = lua[dep + ".name"];
					if (projName == null) continue;

					var projType = lua[dep + ".configuration.outputType"];
					if (projType != null && (projType as string).ToLower() == "exe") continue;

					Task.Call(dep + ".build");
					references
            .AddItem("Reference", dep)
            .AddMetadata ("HintPath", Path.Combine (MainClass.CURRENT, dep, projPath as string, (projName as string) + ".dll"));
				}
			}

			if (Directory.Exists (source)) {
				var compile = root.AddItemGroup ();

				foreach (var file in Directory.GetFiles (source, "*.cs", SearchOption.AllDirectories)) {
          compile.AddItem ("Compile", Path.Combine(MainClass.CURRENT, file));
				}
			}

			if (Directory.Exists (resources)) {
				var none = root.AddItemGroup ();

        foreach (var file in Directory.GetFiles (Path.Combine(MainClass.CURRENT, source), "*", SearchOption.AllDirectories)) {
					none.AddItem ("None", file).AddMetadata ("CopyToOutputDirectory", "PreserveNewest");
				}
			}

			ProjectInstance projectInstance = new ProjectInstance (root);
      ConsoleLogger logger = new ConsoleLogger(LoggerVerbosity.Normal);
			BuildManager manager = BuildManager.DefaultBuildManager;

			var output = manager.Build(
				new BuildParameters() { DetailedSummary = true, Loggers = new[] { logger } },
        new BuildRequestData(projectInstance, projectInstance.DefaultTargets.ToArray()));

      if (output.OverallResult.ToString ().ToLower ().Contains ("failure")) {
        MainClass.Fail ("Compilation of project '" + project + "' failed");
      }

			return root;
		}
	}
}
