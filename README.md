# Kaizo
> A powerful build system for the CLR

**Kaizo** is a build automation tool for [CLR](https://en.wikipedia.org/wiki/Common_Lasnguage_Runtime). Kaizo is using [Lua](http://lua.org) as build script language. Right now, Kaizo is in very early development, but it will work on Windows, Linux and Mac. Kaizo is heavily inspired by [Gradle](https://gradle.org/).

### Installation

You must have [Git](http://git.com) and [Mono](https://mono.org) installed.

#### Linux or Mac OS X

```bash
git clone "https://github.com/nondev/kaizo.git"
cd kaizo && ./install && cd bootstrap
cp kaizow $YOUR_PROJECT_DIRECTORY
cp kaizow.bat $YOUR_PROJECT_DIRECTORY
cp kaizow.exe $YOUR_PROJECT_DIRECTORY
```

#### Windows

```bash
git clone "https://github.com/nondev/kaizo.git"
cd kaizo
install
cd bootstrap
xcopy kaizow %YOUR_PROJECT_DIRECTORY%
xcopy kaizow.bat %YOUR_PROJECT_DIRECTORY%
xcopy kaizow.exe %YOUR_PROJECT_DIRECTORY%
```

### Example

Here is example `project.lua` what can build **Kaizo** itself:

```lua
project('self')

name = 'kaizo'
version = '0.0.1'
source = '../kaizo/src'

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
```

And to build it, simply navigate to `bootstrap` directory and run this from console:

```
./kaizow
```
