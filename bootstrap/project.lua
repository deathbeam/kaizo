project('self')

name = 'kaizo'
version = '0.0.1'
namespace = 'Kaizo'

csharp = {
	type = 'exe',
	source = '../kaizo/src',
	output = 'bin',
	configuration = 'Release',
	namespace = 'Kaizo',
	platform = 'x86',
	framework = 'v4.0'
}

dependencies = {
	'System',
	'System.Core',
	'Microsoft.Build',
	'Microsoft.Build.Framework',
	'NLua_Safe:*',
	'Mono.NuGet.Core:*',
	'Microsoft.Web.Xdt:*'
}
