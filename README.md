# Kaizo [![Build Status](https://travis-ci.org/nondev/kaizo.svg?branch=master)](https://travis-ci.org/nondev/kaizo)
> A powerful build system for the CLR

<img src="http://i.imgur.com/S1oVruZ.png" alt="Terminal demo" title="Terminal demo" align="center" width="100%"/>

**Kaizo** is build automation tool for [CLR](https://en.wikipedia.org/wiki/Common_Language_Runtime). It is using Lua as build script language. Right now, it is in very early development, but it will work on Windows, Linux and Mac. Kaizo is inspired by [Gradle](https://gradle.org/) for Java.

Here is example `project.lua` what will build **Kaizo** itself:

```lua
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
	'System',
	'System.Core',
	'Microsoft.Build',
	'Microsoft.Build.Framework',
	'NLua_Safe:*',
	'Mono.NuGet.Core:*',
	'Microsoft.Web.Xdt:*'
}

function compile()
	task('self.clean')
	task('self.build')
end
```

And to build it, simply navigate to bootstrap directory and run this from console:

```shell
./kaizow self.compile
```
