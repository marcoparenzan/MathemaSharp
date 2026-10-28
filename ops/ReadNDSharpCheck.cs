#:property TargetFramework=net11.0
Console.WriteLine(File.ReadAllText(@"D:\Dev\Science\NDSharp\src\NDSharp.Notebooks.Tests\NotebookTests.cs"));
var log=File.ReadAllText(@"D:\Dev\BuildArtifacts\science-check\NDSharp-test.log");
var index=log.IndexOf("Failed NotebookTests");
if(index>=0)Console.WriteLine(log.Substring(index,Math.Min(4500,log.Length-index)));
