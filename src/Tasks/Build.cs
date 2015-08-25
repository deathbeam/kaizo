using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using NLua;

namespace Kaizo.Tasks
{
	public class Build : Task
	{
		public Build(Lua lua) : base(lua) { }

		public override void Execute(LuaTable args) {
			string name = args ["name"] as string;
			if (name == null) name = lua ["name"] as string;

			string namspace = args ["namespace"] as string;
			if (namspace == null) namspace = lua ["namespace"] as string;

			string version = args ["version"] as string;
			if (version == null) version = lua ["version"] as string;

			var root = ProjectRootElement.Create ();
			var group = root.AddPropertyGroup ();
			group.AddProperty ("Configuration", "Release");
			group.AddProperty ("Platform", "x86");
			group.AddProperty ("RootNamespace", namspace);
			group.AddProperty ("AssemblyName", name);
			group.AddProperty ("ProductVersion", version);
			group.AddProperty ("OutputPath", "bin");
			group.AddProperty ("OutputType", "exe");
			group.AddProperty ("ProjectGuid", System.Guid.NewGuid ().ToString ());
			group.AddProperty ("TargetFrameworkVersion", "v4.0");

			var references = root.AddItemGroup ();

			foreach (var dependency in Dependency.All) {
				foreach (var reference in dependency.AssemblyReferences) {
					foreach (var frmwrk in reference.SupportedFrameworks) {
						if (frmwrk.Version == new Version (4, 0)) {
							references.AddItem ("Reference", reference.Name,
								new KeyValuePair<string, string>[] {
									new KeyValuePair<string, string> ("HintPath", reference.Path)
								});
						}
					}
				}
			}

			var compile = root.AddItemGroup ();

			foreach (var file in Directory.GetFiles (Path.Combine (Directory.GetCurrentDirectory (), "src"), "*.cs", SearchOption.AllDirectories)) {
				compile.AddItem ("Compile", file);
			}

			root.AddImport ("$(MSBuildBinPath)\\Microsoft.CSharp.targets");
			root.Save ("test.csproj");

			ProjectInstance project = new ProjectInstance (root);
			project.Build ();
		}
	}
}