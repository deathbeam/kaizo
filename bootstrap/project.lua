project('self')

name = 'kaizo'
version = '0.0.1'

configuration = {
	type = 'exe',
	source = '../kaizo/src',
	output = 'bin',
	resources = 'res',
	deploy = 'Release',
	namespace = 'Kaizo',
	platform = 'x86',
	framework = 'v4.0'
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

function compile()
	task('self.clean')
	task('self.build')
end
