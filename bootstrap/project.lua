project('self')

name = 'kaizo'
version = '0.0.1'
source = '../kaizo/src'

configuration = {
  outputType = 'exe',
  outputPath = 'bin',
  rootNamespace = 'Kaizo',
  platformTarget = 'anycpu',
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

function compile()
  task('self.clean')
  task('self.build')
end
