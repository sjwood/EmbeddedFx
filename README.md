# EmbeddedFx

[![AppVeyor][1]][2]

EmbeddedFx is a simple library that locates and loads .NET assemblies
that have been stored as embedded resources in the manifest of another
assembly. It's a bit like a User-defined classloader in Java, but for
.NET.

EmbeddedFx aims to overcome some of the [issues][3] with [ILMerge][4],
specifically the preservation of an embedded assembly's identity. It
is based on the technique originally shown by [Jeffrey Richter][5] in
his book [CLR via C#][6].

_Please Note: This repository is no longer maintained. An active project
with more complete functionality to EmbeddedFx is the rather excellent
[Costura][7] plugin for [Fody][8]._

## Getting started

### The easy way (using [Git][9])

Get the repo and build it from the command line:

	"C:\Program Files\Git\bin\git.exe" clone https://github.com/sjwood/EmbeddedFx.git
	.\EmbeddedFx\build\build.cmd

### The slightly-more-work way

Download the repo as a [zip][10] and unzip it.
[Remove the NTFS Alternate Data Streams][11] from the unzipped contents
(PowerShell will not execute arbitrary scripts downloaded from the
interwebs). Run the `build.cmd` file in the `build` folder.

## License

EmbeddedFx is released under the [Apache 2.0 license][12]

  [1]: https://ci.appveyor.com/api/projects/status/github/sjwood/EmbeddedFx?branch=master&svg=true
  [2]: https://ci.appveyor.com/project/StephenWood/embeddedfx
  [3]: http://stackoverflow.com/search?q=ilmerge
  [4]: http://research.microsoft.com/en-us/people/mbarnett/ilmerge.aspx
  [5]: https://github.com/jeffrichter
  [6]: http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx
  [7]: https://github.com/Fody/Costura
  [8]: https://github.com/Fody/Fody
  [9]: http://git-scm.com/
  [10]: https://github.com/sjwood/EmbeddedFx/zipball/master
  [11]: http://www.hanselman.com/blog/RemovingSecurityFromDownloadedPowerShellScriptsWithAlternativeDataStreams.aspx
  [12]: http://opensource.org/licenses/Apache-2.0
