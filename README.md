# EmbeddedFx

EmbeddedFx is a simple library that locates and loads .NET assemblies
that have been stored as embedded resources in the manifest of another
assembly. It's a bit like a User-defined classloader in Java, but for
.NET.

EmbeddedFx aims to overcome some of the [issues][1] with [ILMerge][2],
specifically the preservation of an embedded assembly's identity. It
is based on the technique originally shown by [Jeffrey Richter][3] in
his book [CLR via C#][4].

## Getting started

### The easy way (using [Git][5])

Get the repo and build it from the command line:

	"C:\Program Files\Git\bin\git.exe" clone https://github.com/sjwood/EmbeddedFx.git
	.\EmbeddedFx\build\psake.cmd

### The slightly-more-work way

Download the repo as a [zip][6] and unzip it.
[Remove the NTFS Alternate Data Streams][7] from the unzipped contents
(PowerShell will not execute the build scripts downloaded from the
interwebs). Run the `psake.cmd` file in the `build` folder.

## License

EmbeddedFx is released under the [Apache 2.0 license][8]

  [1]: http://stackoverflow.com/search?q=ilmerge
  [2]: http://research.microsoft.com/en-us/people/mbarnett/ilmerge.aspx
  [3]: https://github.com/jeffrichter
  [4]: http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx
  [5]: http://git-scm.com/
  [6]: https://github.com/sjwood/EmbeddedFx/zipball/master
  [7]: http://www.hanselman.com/blog/RemovingSecurityFromDownloadedPowerShellScriptsWithAlternativeDataStreams.aspx
  [8]: http://opensource.org/licenses/Apache-2.0

