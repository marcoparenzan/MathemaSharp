#:property TargetFramework=net11.0
var p=@"D:\Dev\Science\PySharp\src\PySharp.Tests\Oracle\OracleTests.cs";
var t=File.ReadAllText(p);
t=t.Replace("public class OracleTests\n{", "[Collection(\"oracle-process-state\")]\npublic class OracleTests\n{");
t=t.Replace("        var expected = File.ReadAllText", "        // This snippet shares a historical fixed temp name with other tests. Isolate each run.\n        if (lib == \"pandas\" && name == \"P3_io\")\n            source = source.Replace(\"pysharp_pandas_io_test.csv\", \"pysharp_pandas_oracle_\" + Guid.NewGuid().ToString(\"N\") + \".csv\");\n        var expected = File.ReadAllText");
File.WriteAllText(p,t);
File.WriteAllText(@"D:\Dev\Science\PySharp\src\PySharp.Tests\Oracle\OracleCollection.cs","""
namespace PySharp.Tests.Oracle;
// The golden runner changes process-wide current directory and some graphics libraries use shared state.
[CollectionDefinition("oracle-process-state", DisableParallelization = true)]
public class OracleCollection { }
""");
