using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;

namespace Kaizo
{
	public class Project
	{
		private string name, version, nmspace;

		public Project(string name, string version, string nmspace)
		{
			this.name = name;
			this.version = version;
			this.nmspace = nmspace;
		}

		public void Build()
		{
			var root = ProjectRootElement.Create ();
			var group = root.AddPropertyGroup ();
			group.AddProperty ("Configuration", "Release");
			group.AddProperty ("Platform", "x86");
			group.AddProperty ("RootNamespace", nmspace);
			group.AddProperty ("AssemblyName", name);
			group.AddProperty ("ProductVersion", version);
			group.AddProperty ("OutputPath", "bin");
			group.AddProperty ("OutputType", "exe");
			group.AddProperty ("ProjectGuid", System.Guid.NewGuid ().ToString ());
			group.AddProperty ("TargetFrameworkVersion", "v4.5");

			var references = root.AddItemGroup ();

			foreach (var dependency in Dependency.All) {
				foreach (var reference in dependency.AssemblyReferences) {
					foreach (var frmwrk in reference.SupportedFrameworks) {
						if (frmwrk.Version == new Version (4, 5)) {
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