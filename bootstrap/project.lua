project('self')

name = 'kaizo'
version = '0.0.1'
source = '../kaizo/src'
resources = 'res'

properties = {
	outputType = 'exe',
	outputPath = 'bin',
	configuration = 'Release',
	rootNamespace = 'Kaizo',
	platform = 'x86',
	platformTarget = 'x86',
	targetFrameworkVersion = 'v4.0'
}

dependencies = {
	system = {
		'System',
		'System.Core',
		'Microsoft.Build',
		'Microsoft.Build.Framework'
	},
	nuget = {
		'NLua_Safe',
		'Mono.NuGet.Core',
		'Microsoft.Web.Xdt'
	}
}
