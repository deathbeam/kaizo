# Kaizo
> A powerful build system for the CLR

**Kaizo** is build automation tool for [CLR](https://en.wikipedia.org/wiki/Common_Language_Runtime). It is using Lua as build script language. Right now, it is in very early development, but it will work on Windows, Linux and Mac. Kaizo is inspired by [Gradle](https://gradle.org/) for Java.

Here is example `project.lua` what will build **Kaizo** itself:

```lua
project('root')

name = 'kaizo'
version = '0.0.1'
namespace = 'Kaizo'

csharp = {
	type = 'exe',
	source = '../../src',
	output = 'out',
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
```

And to build it, you will simply run this from console:

```shell
./kaizo root.compile
```
