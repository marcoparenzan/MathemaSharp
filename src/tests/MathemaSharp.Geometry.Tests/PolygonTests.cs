// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using Xunit;
namespace MathemaSharp.Geometry.Tests;
public class PolygonTests
{
    [Fact] public void Double()=>Check<double>();
    [Fact] public void Float()=>Check<float>();
    [Fact] public void Half()=>Check<Half>();
    [Fact] public void Decimal()=>Check<Decimal64>();
    private static void Check<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);Vector2<T> P(int x,int y)=>new(N(x),N(y));
        void Near(T a,T b)=>Assert.True(T.Abs(a-b)<=N(32)*(T.BitIncrement(T.One)-T.One)*T.Max(T.One,T.Abs(a)),$"Expected {a}, actual {b}");
        Vector2<T>[] square=[P(0,0),P(2,0),P(2,2),P(0,2)];
        Assert.Equal(N(4),Polygons.SignedArea<T>(square));Assert.Equal(N(8),Polygons.Perimeter<T>(square));Assert.Equal(P(1,1),Polygons.Centroid<T>(square));
        Assert.Equal(PolygonOrientation.CounterClockwise,Polygons.Orientation<T>(square));
        var reversed=square.Reverse().ToArray();Assert.Equal(-N(4),Polygons.SignedArea<T>(reversed));Assert.Equal(PolygonOrientation.Clockwise,Polygons.Orientation<T>(reversed));Assert.Equal(P(1,1),Polygons.Centroid<T>(reversed));
        Assert.Equal(new BoundingBox2<T>(P(0,0),P(2,2)),Polygons.BoundingBox<T>(square));
        Assert.Equal(PointLocation.Inside,Polygons.LocatePoint<T>(square,P(1,1)));Assert.Equal(PointLocation.Inside,Polygons.LocatePoint<T>(reversed,P(1,1)));
        foreach(var p in new[]{P(0,0),P(2,2),P(1,0),P(2,1),P(1,2),P(0,1)})Assert.Equal(PointLocation.Boundary,Polygons.LocatePoint<T>(square,p));
        Assert.Equal(PointLocation.Outside,Polygons.LocatePoint<T>(square,P(3,1)));
        Vector2<T>[] triangle=[P(0,0),P(3,0),P(0,4)];Assert.Equal(N(6),Polygons.SignedArea<T>(triangle));Assert.Equal(N(12),Polygons.Perimeter<T>(triangle));var centroid=Polygons.Centroid<T>(triangle);Near(T.One,centroid.X);Near(N(4)/N(3),centroid.Y);
        Vector2<T>[] l=[P(0,0),P(3,0),P(3,1),P(1,1),P(1,3),P(0,3)];Assert.Equal(N(5),Polygons.SignedArea<T>(l));Assert.Equal(N(12),Polygons.Perimeter<T>(l));var lc=Polygons.Centroid<T>(l);Near(N(11)/N(10),lc.X);Near(N(11)/N(10),lc.Y);
        Assert.Equal(PointLocation.Outside,Polygons.LocatePoint<T>(l,P(2,2)));Assert.Equal(PointLocation.Inside,Polygons.LocatePoint<T>(l,new(T.One/N(2),N(2))));
        Vector2<T>[] empty=[];Assert.Equal(T.Zero,Polygons.SignedArea<T>(empty));Assert.Equal(T.Zero,Polygons.Perimeter<T>(empty));Assert.Equal(PointLocation.Outside,Polygons.LocatePoint<T>(empty,P(0,0)));Assert.Empty(Polygons.ConvexHull<T>(empty));Assert.Empty(Polygons.Simplify<T>(empty,T.Zero));
        Assert.Throws<ArgumentException>(()=>Polygons.BoundingBox<T>(empty));Assert.Throws<ArgumentException>(()=>Polygons.Centroid<T>(empty));
        Vector2<T>[] line=[P(0,0),P(1,0),P(2,0),P(1,0)];Assert.Equal(PolygonOrientation.Degenerate,Polygons.Orientation<T>(line));Assert.Throws<InvalidOperationException>(()=>Polygons.Centroid<T>(line));
        Assert.Equal(new[]{P(0,0),P(2,0)},Polygons.ConvexHull<T>(line));Assert.Equal(PointLocation.Boundary,Polygons.LocatePoint<T>(line,P(1,0)));Assert.Equal(PointLocation.Outside,Polygons.LocatePoint<T>(line,P(1,1)));
        Vector2<T>[] single=[P(1,2)];Assert.Equal(T.Zero,Polygons.Perimeter<T>(single));Assert.Equal(new BoundingBox2<T>(P(1,2),P(1,2)),Polygons.BoundingBox<T>(single));Assert.Equal(single,Polygons.ConvexHull<T>(single));Assert.Equal(PointLocation.Boundary,Polygons.LocatePoint<T>(single,P(1,2)));
        Vector2<T>[] pair=[P(0,0),P(3,4)];Assert.Equal(N(10),Polygons.Perimeter<T>(pair));Assert.Equal(pair,Polygons.Simplify<T>(pair,T.Zero));
        var closed=square.Concat(new[]{square[0],square[0]}).ToArray();Assert.Equal(N(4),Polygons.SignedArea<T>(closed));Assert.Equal(N(8),Polygons.Perimeter<T>(closed));Assert.Equal(P(1,1),Polygons.Centroid<T>(closed));
        var cloud=square.Concat(new[]{P(1,1),P(1,0),P(0,0)}).Reverse().ToArray();Assert.Equal(square,Polygons.ConvexHull<T>(cloud));Assert.Equal(square,Polygons.ConvexHull<T>(Polygons.ConvexHull<T>(cloud)));
        Vector2<T>[] zig=[P(0,0),P(1,1),P(2,0)];Assert.Equal(zig,Polygons.Simplify<T>(zig,T.Zero));Assert.Equal(new[]{zig[0],zig[2]},Polygons.Simplify<T>(zig,T.One));
        Vector2<T>[] straight=[P(0,0),P(1,0),P(1,0),P(2,0)];Assert.Equal(new[]{P(0,0),P(2,0)},Polygons.Simplify<T>(straight,T.Zero));
        Assert.Equal(new[]{P(0,0),P(0,0)},Polygons.Simplify<T>(new[]{P(0,0),P(0,0),P(0,0)},T.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(()=>Polygons.Simplify<T>(square,-T.One));Assert.Throws<ArgumentOutOfRangeException>(()=>Polygons.Simplify<T>(square,T.NaN));
        Vector2<T>[] invalid=[new(T.NaN,T.Zero)];Assert.Throws<ArgumentException>(()=>Polygons.SignedArea<T>(invalid));Assert.Throws<ArgumentException>(()=>Polygons.ConvexHull<T>(invalid));Assert.Throws<ArgumentException>(()=>Polygons.LocatePoint<T>(square,new(T.PositiveInfinity,T.Zero)));
        var translated=square.Select(p=>p+P(3,4)).ToArray();Assert.Equal(N(4),Polygons.SignedArea<T>(translated));Assert.Equal(P(4,5),Polygons.Centroid<T>(translated));
    }
    [Fact] public void WindingAndSelfIntersectionContracts()
    {
        Vector2<double>[] twice=[new(0,0),new(2,0),new(2,2),new(0,2),new(0,0),new(2,0),new(2,2),new(0,2)];
        Assert.Equal(8,Polygons.SignedArea<double>(twice));Assert.Equal(PointLocation.Inside,Polygons.LocatePoint<double>(twice,new(1,1)));
        Vector2<double>[] bow=[new(0,0),new(2,2),new(0,2),new(2,0)];Assert.Equal(0,Polygons.SignedArea<double>(bow));Assert.Throws<InvalidOperationException>(()=>Polygons.Centroid<double>(bow));Assert.Equal(PointLocation.Boundary,Polygons.LocatePoint<double>(bow,new(1,1)));
    }
}
