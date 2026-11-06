// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using Xunit;
namespace MathemaSharp.Geometry.Tests;
public class IntersectionTests
{
    [Fact] public void Double()=>Check<double>();
    [Fact] public void Float()=>Check<float>();
    [Fact] public void Half()=>Check<Half>();
    [Fact] public void Decimal()=>Check<Decimal64>();
    private static void Check<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);Vector2<T> P(int x,int y)=>new(N(x),N(y));Segment2<T> S(int x,int y,int u,int v)=>new(P(x,y),P(u,v));
        void Near(T a,T b)=>Assert.True(T.Abs(a-b)<=N(32)*(T.BitIncrement(T.One)-T.One)*T.Max(T.One,T.Abs(a)),$"Expected {a}, actual {b}");
        var cross=Intersections.SegmentSegment(S(0,0,2,2),S(0,2,2,0));Assert.Equal(IntersectionKind.Point,cross.Kind);Assert.Equal(P(1,1),cross.First);
        Assert.Equal(cross,Intersections.SegmentSegment(S(0,2,2,0),S(0,0,2,2)));
        Assert.Equal(IntersectionKind.None,Intersections.SegmentSegment(S(0,0,2,0),S(0,1,2,1)).Kind);
        Assert.Equal(IntersectionKind.None,Intersections.SegmentSegment(S(0,0,1,0),S(2,-1,2,1)).Kind);
        Assert.Equal(P(2,0),Intersections.SegmentSegment(S(0,0,2,0),S(2,0,2,2)).First);
        var overlap=Intersections.SegmentSegment(S(0,0,4,0),S(3,0,1,0));Assert.Equal(IntersectionKind.Overlap,overlap.Kind);Assert.Equal(P(1,0),overlap.First);Assert.Equal(P(3,0),overlap.Second);
        var vertical=Intersections.SegmentSegment(S(0,4,0,0),S(0,1,0,3));Assert.Equal(P(0,3),vertical.First);Assert.Equal(P(0,1),vertical.Second);
        Assert.Equal(IntersectionKind.None,Intersections.SegmentSegment(S(0,0,1,0),S(2,0,3,0)).Kind);
        Assert.Equal(IntersectionKind.Point,Intersections.SegmentSegment(S(1,0,1,0),S(0,0,2,0)).Kind);
        Assert.Equal(IntersectionKind.None,Intersections.SegmentSegment(S(1,1,1,1),S(0,0,2,0)).Kind);
        Assert.Equal(IntersectionKind.Point,Intersections.SegmentSegment(S(1,1,1,1),S(1,1,1,1)).Kind);
        Assert.Equal(IntersectionKind.None,Intersections.SegmentSegment(S(1,1,1,1),S(2,2,2,2)).Kind);
        var line=Intersections.LineCircle(P(0,0),P(2,0),P(0,0),N(2));Assert.Equal(IntersectionKind.TwoPoints,line.Kind);Assert.Equal(P(-2,0),line.First);Assert.Equal(P(2,0),line.Second);
        var reverse=Intersections.LineCircle(P(0,0),P(-1,0),P(0,0),N(2));Assert.Equal(line.Second,reverse.First);
        var tangent=Intersections.LineCircle(P(0,2),P(1,0),P(0,0),N(2));Assert.Equal(IntersectionKind.Point,tangent.Kind);Assert.Equal(P(0,2),tangent.First);
        Assert.Equal(IntersectionKind.None,Intersections.LineCircle(P(0,3),P(1,0),P(0,0),N(2)).Kind);
        Assert.Equal(IntersectionKind.Point,Intersections.LineCircle(P(0,0),P(1,0),P(0,0),T.Zero).Kind);
        var circles=Intersections.CircleCircle(P(0,0),N(5),P(6,0),N(5));Assert.Equal(IntersectionKind.TwoPoints,circles.Kind);Assert.Equal(P(3,4),circles.First);Assert.Equal(P(3,-4),circles.Second);
        Near(N(5),Vector2<T>.Distance(circles.First,P(0,0)));Near(N(5),Vector2<T>.Distance(circles.First,P(6,0)));
        var external=Intersections.CircleCircle(P(0,0),N(2),P(4,0),N(2));Assert.Equal(IntersectionKind.Point,external.Kind);Assert.Equal(P(2,0),external.First);
        var internalTouch=Intersections.CircleCircle(P(0,0),N(3),P(2,0),T.One);Assert.Equal(IntersectionKind.Point,internalTouch.Kind);Assert.Equal(P(3,0),internalTouch.First);
        Assert.Equal(IntersectionKind.None,Intersections.CircleCircle(P(0,0),N(3),P(1,0),T.One).Kind);
        Assert.Equal(IntersectionKind.None,Intersections.CircleCircle(P(0,0),T.One,P(3,0),T.One).Kind);
        Assert.Equal(IntersectionKind.None,Intersections.CircleCircle(P(0,0),T.One,P(0,0),N(2)).Kind);
        Assert.Equal(IntersectionKind.Coincident,Intersections.CircleCircle(P(0,0),T.One,P(0,0),T.One).Kind);
        Assert.Equal(IntersectionKind.Point,Intersections.CircleCircle(P(0,0),T.Zero,P(0,0),T.Zero).Kind);
        Assert.Equal(IntersectionKind.Point,Intersections.CircleCircle(P(0,0),T.Zero,P(1,0),T.One).Kind);
        Near(N(2),Intersections.PointSegmentDistance(P(1,2),S(0,0,3,0)));Near(N(5),Intersections.PointSegmentDistance(P(-3,4),S(0,0,3,0)));Near(N(5),Intersections.PointSegmentDistance(P(6,4),S(0,0,3,0)));Near(N(5),Intersections.PointSegmentDistance(P(3,4),S(0,0,0,0)));
        Vector2<T>[] square=[P(0,0),P(2,0),P(2,2),P(0,2)];Assert.Equal(T.Zero,Intersections.PointPolygonDistance<T>(P(1,1),square));Assert.Equal(T.Zero,Intersections.PointPolygonDistance<T>(P(1,0),square));Near(T.One,Intersections.PointPolygonDistance<T>(P(3,1),square));Near(T.Sqrt(N(2)),Intersections.PointPolygonDistance<T>(P(3,3),square));
        Near(N(5),Intersections.PointPolygonDistance<T>(P(3,4),new[]{P(0,0)}));Near(N(2),Intersections.PointPolygonDistance<T>(P(1,2),new[]{P(0,0),P(2,0)}));
        Assert.True(Intersections.InsideCircle(P(3,4),P(0,0),N(5)));Assert.False(Intersections.InsideCircle(P(4,4),P(0,0),N(5)));Assert.True(Intersections.InsideCircle(P(1,1),P(1,1),T.Zero));
        Assert.True(Intersections.InsideEllipse(P(3,0),P(0,0),N(3),N(2)));Assert.True(Intersections.InsideEllipse(P(0,2),P(0,0),N(3),N(2)));Assert.True(Intersections.InsideEllipse(P(1,1),P(0,0),N(3),N(2)));Assert.False(Intersections.InsideEllipse(P(3,2),P(0,0),N(3),N(2)));
        Assert.Throws<ArgumentException>(()=>Intersections.LineCircle(P(0,0),P(0,0),P(0,0),T.One));
        Assert.Throws<ArgumentOutOfRangeException>(()=>Intersections.InsideCircle(P(0,0),P(0,0),-T.One));Assert.Throws<ArgumentOutOfRangeException>(()=>Intersections.CircleCircle(P(0,0),T.NaN,P(0,0),T.One));Assert.Throws<ArgumentOutOfRangeException>(()=>Intersections.InsideEllipse(P(0,0),P(0,0),T.Zero,T.One));
        Assert.Throws<ArgumentException>(()=>Intersections.PointPolygonDistance<T>(P(0,0),Array.Empty<Vector2<T>>()));
        Assert.Throws<ArgumentException>(()=>Intersections.PointSegmentDistance(new(T.PositiveInfinity,T.Zero),S(0,0,1,1)));
    }
}
