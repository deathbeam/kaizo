# Kaizo
> A powerful build system for the CLR

**Kaizo** is a build automation tool for [CLR](https://en.wikipedia.org/wiki/Common_Lasnguage_Runtime). Kaizo is using [Lua](http://lua.org) as build script language. Right now, Kaizo is in very early development, but it will work on Windows, Linux and Mac. Kaizo is heavily inspired by [Gradle](https://gradle.org/).

### Installation

You must have [Git](http://git.com) and [Mono](https://mono.org) or [.NET](https://dotnetframework.com) installed.

This installation method will install and compile latest Kaizo version from this repository.
You can always simply download the wrapper scripts from bootstrap directory, and they will
handle the installation.

#### Linux or Mac OS X

```bash
git clone https://github.com/nondev/kaizo.git
cd kaizo && ./install && cp -Rf bootstrap "$YOUR_PROJECT_DIRECTORY"
```

#### Windows

```bash
git clone https://github.com/nondev/kaizo.git
cd kaizo
install
xcopy /t bootstrap "%YOUR_PROJECT_DIRECTORY%"
```

### Example

Here is example [project.lua](bootstrap/project.lua) what can build **Kaizo** itself:

```lua
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
```

And to build it, simply run this from console:

```bash
./kaizo self.compile
```
