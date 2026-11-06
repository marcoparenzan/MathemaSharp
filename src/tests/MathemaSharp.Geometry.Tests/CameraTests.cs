// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using Xunit;
namespace MathemaSharp.Geometry.Tests;
public class CameraTests
{
    [Fact] public void Double()=>Check<double>();
    [Fact] public void Float()=>Check<float>();
    [Fact] public void Half()=>Check<Half>();
    [Fact] public void Decimal()=>Check<Decimal64>();
    private static void Check<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);
        void Near(T a,T b)=>Assert.True(T.Abs(a-b)<=N(64)*(T.BitIncrement(T.One)-T.One)*T.Max(T.One,T.Abs(a)),$"Expected {a}, actual {b}");
        var p=Camera<T>.Project((-T.One,T.One),(-T.One,T.One),(-T.One,T.One),T.Zero,T.Zero,N(200),N(160),N(100),N(80),T.Zero,T.Zero,T.Zero);
        Near(N(100),p.X);Near(N(80),p.Y);Near(-T.One,p.Depth);
        var reverse=Camera<T>.Project((T.One,-T.One),(T.One,-T.One),(T.One,-T.One),T.Zero,T.Zero,N(200),N(160),N(100),N(80),T.Zero,T.Zero,T.Zero);
        Near(p.X,reverse.X);Near(p.Y,reverse.Y);Near(p.Depth,reverse.Depth);
        var corner=Camera<T>.Project((-T.One,T.One),(-T.One,T.One),(-T.One,T.One),T.Zero,T.Zero,N(200),N(160),N(100),N(80),T.One,T.One,T.One);
        Assert.True(T.IsFinite(corner.X)&&T.IsFinite(corner.Y)&&T.IsFinite(corner.Depth));
        var singular=Camera<T>.Project((T.Zero,T.Zero),(-T.One,T.One),(-T.One,T.One),T.Zero,T.Zero,N(200),N(160),N(100),N(80),T.Zero,T.Zero,T.Zero);
        Assert.True(!T.IsFinite(singular.X)||!T.IsFinite(singular.Y));
        var order=DepthOrdering.BackToFront(new[]{(Id:0,Depth:T.One),(Id:1,Depth:N(2)),(Id:2,Depth:T.One)},v=>v.Depth).Select(v=>v.Id).ToArray();Assert.Equal(new[]{1,0,2},order);
        Assert.Equal(N(2),DepthOrdering.Average(new[]{T.One,N(2),N(3)}));Assert.Throws<InvalidOperationException>(()=>DepthOrdering.Average(Array.Empty<T>()));
    }
    [Fact] public void PainterOrderMatchesLinqForNonfiniteValues()
    {
        double[] depths=[0,double.NaN,1,1,double.PositiveInfinity,double.NegativeInfinity,-0d];
        Assert.Equal(Enumerable.Range(0,depths.Length).OrderByDescending(i=>depths[i]),DepthOrdering.BackToFront(Enumerable.Range(0,depths.Length),i=>depths[i]));
    }
}
