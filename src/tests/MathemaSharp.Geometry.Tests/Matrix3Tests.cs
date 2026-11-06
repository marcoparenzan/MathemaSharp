// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using Xunit;
namespace MathemaSharp.Geometry.Tests;
public class Matrix3Tests
{
    [Fact] public void Double()=>Check<double>();
    [Fact] public void Float()=>Check<float>();
    [Fact] public void Half()=>Check<Half>();
    [Fact] public void Decimal()=>Check<Decimal64>();
    private static void Check<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);
        void Near(T x,T y)=>Assert.True(T.Abs(x-y)<=N(64)*(T.BitIncrement(T.One)-T.One)*T.Max(T.One,T.Abs(x)), $"Expected {x}, actual {y}");
        void MatrixNear(Matrix3<T> a,Matrix3<T> b){for(var i=0;i<3;i++)for(var j=0;j<3;j++)Near(a[i,j],b[i,j]);}
        var id=Matrix3<T>.Identity;var p=new Vector2<T>(N(2),N(3));
        Assert.Equal(p,id.TransformPoint(p)); Assert.Equal(id,id.Transpose());
        var tr=Matrix3<T>.Translation(N(4),N(5));var sc=Matrix3<T>.Scale(N(2),N(3));
        Assert.Equal(new Vector2<T>(N(8),N(14)),(tr*sc).TransformPoint(p));
        Assert.Equal(new Vector2<T>(N(12),N(24)),(sc*tr).TransformPoint(p));
        Assert.Equal(p,tr.TransformDirection(p)); Near(N(6),sc.Determinant);
        var r=Matrix3<T>.Rotation(T.Pi/N(2));var rotated=r.TransformPoint(new(T.One,T.Zero));Near(T.Zero,rotated.X);Near(T.One,rotated.Y);
        Near(T.One,r.Determinant);MatrixNear(id,r*r.Transpose());MatrixNear(r.Transpose(),r.Inverse());
        var m=tr*r*sc;MatrixNear(id,m*m.Inverse());MatrixNear(id,m.Inverse()*m);
        var back=m.Inverse().TransformPoint(m.TransformPoint(p));Near(p.X,back.X);Near(p.Y,back.Y);
        var general=new Matrix3<T>(N(1),N(2),N(3),N(0),N(1),N(4),N(5),N(6),N(0));
        Near(T.One,general.Determinant);MatrixNear(id,general*general.Inverse());
        Assert.Equal(general,general.Transpose().Transpose());
        Assert.Throws<InvalidOperationException>(()=>Matrix3<T>.Scale(T.Zero,T.One).Inverse());
        Assert.Throws<InvalidOperationException>(()=>new Matrix3<T>().Inverse());
        Assert.Throws<InvalidOperationException>(()=>Matrix3<T>.Translation(T.NaN,T.Zero).Inverse());
        var perspective=new Matrix3<T>(T.One,T.Zero,T.Zero,T.Zero,T.One,T.Zero,T.One,T.Zero,T.One);
        Assert.Equal(new Vector2<T>(T.One/N(2),T.One),perspective.TransformPoint(new(T.One,N(2))));
        Assert.Throws<InvalidOperationException>(()=>perspective.TransformPoint(new(-T.One,T.Zero)));
        Assert.Throws<InvalidOperationException>(()=>perspective.TransformDirection(p));
        Assert.Throws<ArgumentOutOfRangeException>(()=>id[3,0]);
    }
    [Fact] public void NumericsReference()
    {
        const double angle=.37;var m=Matrix3<double>.Translation(4,5)*Matrix3<double>.Rotation(angle)*Matrix3<double>.Scale(2,3);
        var b=Matrix4x4.CreateScale(2,3,1)*Matrix4x4.CreateRotationZ((float)angle)*Matrix4x4.CreateTranslation(4,5,0);
        var expected=System.Numerics.Vector3.Transform(new(2,3,0),b);var actual=m.TransformPoint(new(2,3));
        Assert.InRange(Math.Abs(expected.X-actual.X),0,2e-6);Assert.InRange(Math.Abs(expected.Y-actual.Y),0,2e-6);
    }
}
