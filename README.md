# EmbeddedFx

EmbeddedFx is a simple library that locates and loads .NET assemblies
that have been stored as embedded resources in the manifest of another
assembly. It's a bit like a User-defined classloader in Java, but for
.NET.

EmbeddedFx aims to overcome some of the [issues][1] with [ILMerge][2],
specifically the preservation of an embedded assembly's identity. It
is based on the technique originally shown by [Jeffrey Richter][3] in
his book [CLR via C#][4].

  [1]: http://stackoverflow.com/search?q=ilmerge
  [2]: http://research.microsoft.com/en-us/people/mbarnett/ilmerge.aspx
  [3]: https://github.com/jeffrichter
  [4]: http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx