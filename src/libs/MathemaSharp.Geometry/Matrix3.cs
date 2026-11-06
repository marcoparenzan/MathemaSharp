// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
namespace MathemaSharp.Geometry;

/// <summary>Row-major homogeneous matrix acting on column vectors. A*B applies B first.
/// Inverse rejects nonfinite and exactly singular matrices; no conditioning guarantee is provided.</summary>
public readonly record struct Matrix3<T>(T M11,T M12,T M13,T M21,T M22,T M23,T M31,T M32,T M33)
    where T:IFloatingPointIeee754<T>
{
    public static Matrix3<T> Identity => new(T.One,T.Zero,T.Zero,T.Zero,T.One,T.Zero,T.Zero,T.Zero,T.One);
    public static Matrix3<T> Translation(T x,T y) => new(T.One,T.Zero,x,T.Zero,T.One,y,T.Zero,T.Zero,T.One);
    public static Matrix3<T> Scale(T x,T y) => new(x,T.Zero,T.Zero,T.Zero,y,T.Zero,T.Zero,T.Zero,T.One);
    public static Matrix3<T> Rotation(T radians)
    {
        var c=T.Cos(radians); var s=T.Sin(radians);
        return new(c,-s,T.Zero,s,c,T.Zero,T.Zero,T.Zero,T.One);
    }
    public T this[int row,int column] => (row,column) switch {
        (0,0)=>M11,(0,1)=>M12,(0,2)=>M13,(1,0)=>M21,(1,1)=>M22,(1,2)=>M23,(2,0)=>M31,(2,1)=>M32,(2,2)=>M33,
        _=>throw new ArgumentOutOfRangeException(nameof(row),"Indices must be 0 through 2.") };
    public static Matrix3<T> operator *(Matrix3<T> a,Matrix3<T> b)
    {
        T At(int i,int j)=>a[i,0]*b[0,j]+a[i,1]*b[1,j]+a[i,2]*b[2,j];
        return new(At(0,0),At(0,1),At(0,2),At(1,0),At(1,1),At(1,2),At(2,0),At(2,1),At(2,2));
    }
    public Matrix3<T> Transpose() => new(M11,M21,M31,M12,M22,M32,M13,M23,M33);
    public T Determinant => M11*(M22*M33-M23*M32)-M12*(M21*M33-M23*M31)+M13*(M21*M32-M22*M31);
    public Matrix3<T> Inverse()
    {
        var scale=T.Zero;
        for(var i=0;i<3;i++)for(var j=0;j<3;j++) {
            if(!T.IsFinite(this[i,j])) throw new InvalidOperationException("Inverse requires finite entries.");
            scale=T.Max(scale,T.Abs(this[i,j]));
        }
        if(scale==T.Zero) throw new InvalidOperationException("Matrix is singular.");
        // Avoid introducing rounding into small, exactly representable cofactors.
        // Scaling is reserved for entries whose unscaled cofactor arithmetic overflows.
        var unscaledFinite=T.IsFinite(Determinant);
        for(var i=0;i<3;i++)for(var j=0;j<3;j++) {
            var r1=(i+1)%3;var r2=(i+2)%3;var c1=(j+1)%3;var c2=(j+2)%3;
            unscaledFinite &= T.IsFinite(this[r1,c1]*this[r2,c2]-this[r1,c2]*this[r2,c1]);
        }
        if(unscaledFinite && Determinant!=T.Zero) scale=T.One;
        var a=M11/scale;var b=M12/scale;var c=M13/scale;var d=M21/scale;var e=M22/scale;var f=M23/scale;var g=M31/scale;var h=M32/scale;var k=M33/scale;
        var aa=e*k-f*h;var bb=c*h-b*k;var cc=b*f-c*e;
        var dd=f*g-d*k;var ee=a*k-c*g;var ff=c*d-a*f;
        var gg=d*h-e*g;var hh=b*g-a*h;var kk=a*e-b*d;
        var det=a*aa+b*dd+c*gg;
        if(det==T.Zero || !T.IsFinite(det)) throw new InvalidOperationException("Matrix is singular or inverse is not representable.");
        T V(T v) { var result=(v/det)/scale; if(!T.IsFinite(result))throw new InvalidOperationException("Inverse is not representable."); return result; }
        return new(V(aa),V(bb),V(cc),V(dd),V(ee),V(ff),V(gg),V(hh),V(kk));
    }
    /// <summary>Transforms a point and divides by its homogeneous coordinate. A zero or nonfinite coordinate is rejected.</summary>
    public Vector2<T> TransformPoint(Vector2<T> point)
    {
        var w=M31*point.X+M32*point.Y+M33;
        if(w==T.Zero || !T.IsFinite(w))throw new InvalidOperationException("Point has an invalid homogeneous coordinate.");
        return new((M11*point.X+M12*point.Y+M13)/w,(M21*point.X+M22*point.Y+M23)/w);
    }
    /// <summary>Transforms a direction with w=0. Requires an affine matrix with bottom row (0,0,1).</summary>
    public Vector2<T> TransformDirection(Vector2<T> direction)
    {
        if(M31!=T.Zero || M32!=T.Zero || M33!=T.One)throw new InvalidOperationException("Direction transformation requires an affine matrix.");
        return new(M11*direction.X+M12*direction.Y,M21*direction.X+M22*direction.Y);
    }
}
