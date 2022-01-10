# Kaizo
> A powerful build system for the CLR

**Kaizo** is a build automation tool for [CLR](https://en.wikipedia.org/wiki/Common_Language_Runtime). Kaizo is using [Lua](http://lua.org) as build script language. Right now, Kaizo is in very early development, but it will work on Windows, Linux and Mac. Kaizo is heavily inspired by [Gradle](https://gradle.org/).

### Installation

You must have [Git](https://git-scm.com/) and [Mono](http://www.mono-project.com/) or [.NET](http://www.microsoft.com/en-us/download/details.aspx?id=17851) installed.

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
cd kaizo && install && xcopy /t bootstrap "%YOUR_PROJECT_DIRECTORY%"
```

### Example

Here is example [project.lua](bootstrap/project.lua) what can build **Kaizo** itself. To build it, simply run this from console:

```bash
./kaizo self.compile
```
