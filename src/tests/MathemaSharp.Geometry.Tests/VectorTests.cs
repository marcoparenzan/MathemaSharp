// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using MathemaSharp.Geometry;
using Xunit;
namespace MathemaSharp.Geometry.Tests;
public class VectorTests
{
    [Fact] public void Double() => Check<double>();
    [Fact] public void Float() => Check<float>();
    [Fact] public void Half() => Check<Half>();
    [Fact] public void Decimal() => Check<Decimal64>();
    private static void Check<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);
        void Near(T expected,T actual) => Assert.True(T.Abs(expected-actual)<=N(32)*(T.BitIncrement(T.One)-T.One)*T.Max(T.One,T.Abs(expected)),$"Expected {expected}, actual {actual}");
        var a=new Vector2<T>(N(3),N(4)); var x=new Vector2<T>(T.One,T.Zero); var y=new Vector2<T>(T.Zero,T.One);
        Near(N(5),a.Length); Near(N(25),a.LengthSquared); Near(T.One,a.Normalize().Length);
        Assert.Equal(T.One,Vector2<T>.Cross(x,y)); Assert.Equal(T.Zero,Vector2<T>.Dot(x,y));
        Near(T.Pi/N(2),Vector2<T>.Angle(x,y)); Assert.Equal(new Vector2<T>(N(3),T.Zero),a.ProjectOnto(x));
        Assert.Equal(a,Vector2<T>.Lerp(Vector2<T>.Zero,a,T.One));
        Assert.Equal(a, (a+x)-x); Assert.Equal(a, a*N(2)/N(2)); Assert.Equal(-a,-T.One*a);
        Near(N(5),Vector2<T>.Distance(a,Vector2<T>.Zero));
        var u=new Vector3<T>(T.One,T.Zero,T.Zero); var v=new Vector3<T>(T.Zero,T.One,T.Zero); var w=new Vector3<T>(T.Zero,T.Zero,T.One);
        Assert.Equal(w,Vector3<T>.Cross(u,v)); Assert.Equal(T.Zero,Vector3<T>.Dot(u,v));
        var b=new Vector3<T>(N(2),N(3),N(6)); Near(N(7),b.Length); Near(N(49),b.LengthSquared); Near(T.One,b.Normalize().Length);
        Near(T.Pi/N(2),Vector3<T>.Angle(u,v)); Assert.Equal(u*N(2),b.ProjectOnto(u));
        Assert.Equal(b,Vector3<T>.Lerp(Vector3<T>.Zero,b,T.One)); Near(N(7),Vector3<T>.Distance(b,Vector3<T>.Zero));
        Assert.Equal(b,(b+u)-u); Assert.Equal(b,b*N(2)/N(2));
        Assert.Throws<ArgumentException>(()=>Vector2<T>.Zero.Normalize()); Assert.Throws<ArgumentException>(()=>Vector3<T>.Zero.Normalize());
        Assert.Throws<ArgumentException>(()=>a.ProjectOnto(Vector2<T>.Zero));
        Assert.Throws<ArgumentException>(()=>new Vector3<T>(T.PositiveInfinity,T.Zero,T.Zero).Normalize());
        Assert.Throws<ArgumentException>(()=>new Vector2<T>(T.NaN,T.Zero).Normalize());
        var large=new Vector2<T>(T.CreateChecked(100),T.CreateChecked(100)); Near(T.One,large.Normalize().Length);
    }
    [Fact] public void NumericsReference()
    {
        var a=new Vector3<double>(2,3,6); var b=new System.Numerics.Vector3(2,3,6);
        Assert.Equal((double)b.Length(),a.Length,5);
        var n=a.Normalize(); var expected=System.Numerics.Vector3.Normalize(b);
        Assert.InRange(Math.Abs(n.X-expected.X),0,1e-6); Assert.InRange(Math.Abs(n.Y-expected.Y),0,1e-6); Assert.InRange(Math.Abs(n.Z-expected.Z),0,1e-6);
    }
}
