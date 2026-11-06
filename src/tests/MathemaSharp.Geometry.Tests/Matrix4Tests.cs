// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using Xunit;
namespace MathemaSharp.Geometry.Tests;
public class Matrix4Tests
{
    [Fact] public void Double()=>Check<double>();
    [Fact] public void Float()=>Check<float>();
    [Fact] public void Half()=>Check<Half>();
    [Fact] public void Decimal()=>Check<Decimal64>();
    private static void Check<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);
        void Near(T x,T y)=>Assert.True(T.Abs(x-y)<=N(64)*(T.BitIncrement(T.One)-T.One)*T.Max(T.One,T.Abs(x)),$"Expected {x}, actual {y}");
        void Mat(Matrix4<T> a,Matrix4<T> b){for(var i=0;i<4;i++)for(var j=0;j<4;j++)Near(a[i,j],b[i,j]);}
        void Vec(Vector3<T> a,Vector3<T> b){Near(a.X,b.X);Near(a.Y,b.Y);Near(a.Z,b.Z);}
        var id=Matrix4<T>.Identity;var p=new Vector3<T>(N(2),N(3),N(4));var angle=T.Pi/N(2);
        var x=new Vector3<T>(T.One,T.Zero,T.Zero);var y=new Vector3<T>(T.Zero,T.One,T.Zero);var z=new Vector3<T>(T.Zero,T.Zero,T.One);
        Assert.Equal(p,id.TransformPoint(p));Assert.Equal(id,id.Inverse());
        Vec(y,Matrix4<T>.RotationZ(angle).TransformDirection(x));Vec(z,Matrix4<T>.RotationX(angle).TransformDirection(y));Vec(x,Matrix4<T>.RotationY(angle).TransformDirection(z));
        Mat(Matrix4<T>.RotationX(angle),Matrix4<T>.RotationAxis(x,angle));
        var r=Matrix4<T>.RotationAxis(new(N(2),N(3),N(4)),N(1)/N(3));
        Mat(id,r*r.Transpose());Mat(r.Transpose(),r.Inverse());Near(T.One,r.Determinant);Near(p.Length,r.TransformDirection(p).Length);
        var t=Matrix4<T>.Translation(N(4),N(5),N(6));var s=Matrix4<T>.Scale(N(2),N(3),N(4));
        Assert.Equal(new Vector3<T>(N(8),N(14),N(22)),(t*s).TransformPoint(p));
        Assert.Equal(p,t.TransformDirection(p));Near(N(24),s.Determinant);
        var m=t*r*s;Mat(id,m*m.Inverse());Mat(id,m.Inverse()*m);Vec(p,m.Inverse().TransformPoint(m.TransformPoint(p)));Assert.Equal(m,m.Transpose().Transpose());
        // Permutation matrix forces a pivot row exchange; determinant is negative.
        var swap=new Matrix4<T>(T.Zero,T.One,T.Zero,T.Zero,T.One,T.Zero,T.Zero,T.Zero,T.Zero,T.Zero,T.One,T.Zero,T.Zero,T.Zero,T.Zero,T.One);
        Near(-T.One,swap.Determinant);Assert.Equal(swap,swap.Inverse());
        Assert.Throws<InvalidOperationException>(()=>Matrix4<T>.Scale(T.One,T.Zero,T.One).Inverse());
        Assert.Throws<InvalidOperationException>(()=>new Matrix4<T>().Inverse());
        Assert.Throws<InvalidOperationException>(()=>Matrix4<T>.Translation(T.NaN,T.Zero,T.Zero).Inverse());
        Assert.Throws<ArgumentException>(()=>Matrix4<T>.RotationAxis(Vector3<T>.Zero,T.One));
        Assert.Throws<ArgumentOutOfRangeException>(()=>Matrix4<T>.RotationAxis(x,T.PositiveInfinity));
        var projective=new Matrix4<T>(T.One,T.Zero,T.Zero,T.Zero,T.Zero,T.One,T.Zero,T.Zero,T.Zero,T.Zero,T.One,T.Zero,T.One,T.Zero,T.Zero,T.One);
        Vec(new(T.One/N(2),T.One,T.One),projective.TransformPoint(new(T.One,N(2),N(2))));
        Assert.Throws<InvalidOperationException>(()=>projective.TransformPoint(new(-T.One,T.Zero,T.Zero)));
        Assert.Throws<InvalidOperationException>(()=>projective.TransformDirection(x));
        Assert.Throws<ArgumentOutOfRangeException>(()=>id[-1,0]);
    }
    [Fact] public void NumericsReference()
    {
        var axis=new System.Numerics.Vector3(2,3,4);var angle=.37;
        var expected=Matrix4x4.CreateScale(2,3,4)*Matrix4x4.CreateFromAxisAngle(System.Numerics.Vector3.Normalize(axis),(float)angle)*Matrix4x4.CreateTranslation(4,5,6);
        var m=Matrix4<double>.Translation(4,5,6)*Matrix4<double>.RotationAxis(new(2,3,4),angle)*Matrix4<double>.Scale(2,3,4);
        var p=System.Numerics.Vector3.Transform(new(2,3,4),expected);var actual=m.TransformPoint(new(2,3,4));
        Assert.InRange(Math.Abs(p.X-actual.X),0,5e-6);Assert.InRange(Math.Abs(p.Y-actual.Y),0,5e-6);Assert.InRange(Math.Abs(p.Z-actual.Z),0,5e-6);
    }
}
