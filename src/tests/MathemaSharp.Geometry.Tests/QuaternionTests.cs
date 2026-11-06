// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
using Xunit;
namespace MathemaSharp.Geometry.Tests;
public class QuaternionTests
{
    [Fact] public void Double()=>Check<double>();
    [Fact] public void Float()=>Check<float>();
    [Fact] public void Half()=>Check<Half>();
    [Fact] public void Decimal()=>Check<Decimal64>();
    private static void Check<T>() where T:IFloatingPointIeee754<T>
    {
        T N(int n)=>T.CreateChecked(n);
        void Near(T a,T b)=>Assert.True(T.Abs(a-b)<=N(64)*(T.BitIncrement(T.One)-T.One)*T.Max(T.One,T.Abs(a)),$"Expected {a}, actual {b}");
        void Vec(Vector3<T> a,Vector3<T> b){Near(a.X,b.X);Near(a.Y,b.Y);Near(a.Z,b.Z);}
        void Mat(Matrix4<T> a,Matrix4<T> b){for(var i=0;i<4;i++)for(var j=0;j<4;j++)Near(a[i,j],b[i,j]);}
        var x=new Vector3<T>(T.One,T.Zero,T.Zero);var y=new Vector3<T>(T.Zero,T.One,T.Zero);var z=new Vector3<T>(T.Zero,T.Zero,T.One);
        var q=Quaternion<T>.FromAxisAngle(z,T.Pi/N(2));Vec(y,q.Rotate(x));Near(T.One,q.Length);
        var raw=new Quaternion<T>(N(1),N(2),N(3),N(4));var norm=raw*raw.Conjugate();Near(raw.LengthSquared,norm.W);Near(T.Zero,norm.X);Near(T.Zero,norm.Y);Near(T.Zero,norm.Z);
        Near(T.One,raw.Normalize().Length);
        var p=new Vector3<T>(N(2),N(3),N(4));Near(p.Length,q.Rotate(p).Length);
        Mat(q.ToRotationMatrix(),(-q).ToRotationMatrix());
        var a=Quaternion<T>.FromAxisAngle(x,N(1)/N(3));var b=Quaternion<T>.FromAxisAngle(y,N(1)/N(4));
        Vec(a.Rotate(b.Rotate(p)),(a*b).Rotate(p));Mat(a.ToRotationMatrix()*b.ToRotationMatrix(),(a*b).ToRotationMatrix());
        foreach(var axis in new[]{x,y,z,new Vector3<T>(N(2),N(3),N(4))})foreach(var angle in new[]{T.Zero,N(1)/N(3),T.Pi}) {
            var m=Matrix4<T>.RotationAxis(axis,angle);var round=Quaternion<T>.FromRotationMatrix(m).ToRotationMatrix();Mat(m,round);Near(T.One,round.Determinant);Mat(Matrix4<T>.Identity,round*round.Transpose());
        }
        var midpoint=Quaternion<T>.Slerp(Quaternion<T>.Identity,q,T.One/N(2));Vec(new(T.Sqrt(T.One/N(2)),T.Sqrt(T.One/N(2)),T.Zero),midpoint.Rotate(x));
        Mat(Quaternion<T>.Identity.ToRotationMatrix(),Quaternion<T>.Slerp(Quaternion<T>.Identity,q,T.Zero).ToRotationMatrix());
        Mat(q.ToRotationMatrix(),Quaternion<T>.Slerp(Quaternion<T>.Identity,q,T.One).ToRotationMatrix());
        Mat(q.ToRotationMatrix(),Quaternion<T>.Slerp(q,-q,T.One/N(2)).ToRotationMatrix());
        Mat(q.ToRotationMatrix(),Quaternion<T>.Slerp(q,q,T.One/N(2)).ToRotationMatrix());
        var tiny=Quaternion<T>.FromAxisAngle(x,(T.BitIncrement(T.One)-T.One));Near(T.One,Quaternion<T>.Slerp(Quaternion<T>.Identity,tiny,T.One/N(2)).Length);
        Assert.Throws<ArgumentException>(()=>new Quaternion<T>().Normalize());
        Assert.Throws<ArgumentException>(()=>new Quaternion<T>(T.NaN,T.Zero,T.Zero,T.One).Normalize());
        Assert.Throws<ArgumentException>(()=>Quaternion<T>.FromAxisAngle(Vector3<T>.Zero,T.One));
        Assert.Throws<ArgumentOutOfRangeException>(()=>Quaternion<T>.FromAxisAngle(x,T.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(()=>Quaternion<T>.Slerp(q,q,-T.One));
        Assert.Throws<ArgumentOutOfRangeException>(()=>Quaternion<T>.Slerp(q,q,T.NaN));
        Assert.Throws<ArgumentException>(()=>Quaternion<T>.FromRotationMatrix(Matrix4<T>.Scale(-T.One,T.One,T.One)));
        Assert.Throws<ArgumentException>(()=>Quaternion<T>.FromRotationMatrix(Matrix4<T>.Scale(N(2),T.One,T.One)));
        Assert.Throws<ArgumentException>(()=>Quaternion<T>.FromRotationMatrix(Matrix4<T>.Translation(T.One,T.Zero,T.Zero)));
        Assert.Throws<ArgumentException>(()=>Quaternion<T>.FromRotationMatrix(Matrix4<T>.Translation(T.NaN,T.Zero,T.Zero)));
    }
    [Fact] public void NumericsReference()
    {
        var axis=System.Numerics.Vector3.Normalize(new(2,3,4));var reference=System.Numerics.Quaternion.CreateFromAxisAngle(axis,.37f);
        var q=Quaternion<double>.FromAxisAngle(new(2,3,4),.37);var p=new Vector3<double>(2,3,4);
        void Compare(Quaternion<double> actual,System.Numerics.Quaternion expected) {
            var v=actual.Rotate(p);var r=System.Numerics.Vector3.Transform(new(2,3,4),expected);
            Assert.InRange(Math.Abs(v.X-r.X),0,3e-6);Assert.InRange(Math.Abs(v.Y-r.Y),0,3e-6);Assert.InRange(Math.Abs(v.Z-r.Z),0,3e-6);
        }
        Compare(q,reference);Compare(Quaternion<double>.Slerp(Quaternion<double>.Identity,q,.3),System.Numerics.Quaternion.Slerp(System.Numerics.Quaternion.Identity,reference,.3f));
    }
}
