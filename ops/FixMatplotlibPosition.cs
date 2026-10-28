#:property TargetFramework=net11.0
var p=@"D:\Dev\Science\PySharp\src\PySharpLib.Matplotlib\AxesApi.cs";
var text=File.ReadAllText(p);
var old="var b = ax.BoxAspect is not null ? ax.Slot : ax.Box; double W_ = ax.Figure.WidthPx, H_ = ax.Figure.HeightPx;";
var replacement="// get_position returns the active (aspect-adjusted) box by default.\n            var b = ax.Box; double W_ = ax.Figure.WidthPx, H_ = ax.Figure.HeightPx;";
if(!text.Contains(old)) throw new InvalidOperationException("Expected unique code not found");
File.WriteAllText(p,text.Replace(old,replacement));
