#:property TargetFramework=net11.0
var lines=File.ReadAllLines(Path.Combine(@"D:\Dev\Science",args[0]));
int start=args.Length>1?int.Parse(args[1]):1; int end=args.Length>2?int.Parse(args[2]):lines.Length;
for(int i=start-1;i<Math.Min(end,lines.Length);i++) Console.WriteLine($"{i+1}: {lines[i]}");
