// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using Xunit;
namespace MathemaSharp.Geometry.Tests;
public class IsoContourTests
{
    [Fact] public void Double()=>Check<double>();
    [Fact] public void Float()=>Check<float>();
    [Fact] public void Half()=>Check<Half>();
    [Fact] public void Decimal()=>Check<Decimal64>();
    private static void Check<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);
        T[,] exact={{T.Zero,-T.One},{-T.One,-T.One}};
        Assert.Empty(IsoContours.MarchingSquares(exact,T.Zero));
        var vertex=Assert.Single(IsoContours.MarchingSquares(exact,T.Zero,null,true));Assert.Equal(Vector2<T>.Zero,vertex.Start);Assert.Equal(vertex.Start,vertex.End);
        T[,] ramp={{T.Zero,N(2),N(4)},{T.Zero,N(2),N(4)}};
        var line=Assert.Single(IsoContours.MarchingSquares(ramp,T.One));Assert.Equal(new Vector2<T>(T.One/N(2),T.Zero),line.Start);Assert.Equal(new Vector2<T>(T.One/N(2),T.One),line.End);
        var before=ramp.Cast<T>().ToArray();_=IsoContours.MarchingSquares(ramp,T.One);Assert.Equal(before,ramp.Cast<T>().ToArray());
        T[,] saddle={{T.One,-T.One},{-T.One,T.One}};Assert.Equal(2,IsoContours.MarchingSquares(saddle,T.Zero).Count);
        Assert.Empty(IsoContours.MarchingSquares(saddle,T.Zero,new bool[,]{{true,false},{false,false}}));
        Assert.Throws<ArgumentException>(()=>IsoContours.MarchingSquares(new T[0,0],T.Zero));Assert.Throws<ArgumentException>(()=>IsoContours.MarchingSquares(ramp,T.Zero,new bool[1,1]));
        Assert.Throws<ArgumentOutOfRangeException>(()=>IsoContours.MarchingSquares(ramp,T.NaN));
        T[,] invalid={{T.NaN,T.One},{T.Zero,-T.One}};Assert.Throws<ArgumentException>(()=>IsoContours.MarchingSquares(invalid,T.Zero));Assert.Empty(IsoContours.MarchingSquares(invalid,T.Zero,new bool[,]{{true,false},{false,false}}));
    }
    [Fact] public void LegacyDecimalConstraintPreserved()
    {
        var result=IsoContours.Trace(2,2,(r,c)=>(decimal)c,c=>(decimal)c,r=>(decimal)r,.5m,null,true,true,true);
        var line=Assert.Single(result);Assert.Equal(.5m,line.X1);Assert.Equal(.5m,line.X2);
    }
}
