// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
namespace MathemaSharp.Geometry;

/// <summary>Row-major matrix acting on column vectors. A*B applies B first.
/// Inversion uses partial pivoting, rejects zero pivots and nonfinite results, and makes no conditioning guarantee.</summary>
public readonly record struct Matrix4<T>(T M11,T M12,T M13,T M14,T M21,T M22,T M23,T M24,T M31,T M32,T M33,T M34,T M41,T M42,T M43,T M44)
    where T:IFloatingPointIeee754<T>
{
    public static Matrix4<T> Identity => new(T.One,T.Zero,T.Zero,T.Zero,T.Zero,T.One,T.Zero,T.Zero,T.Zero,T.Zero,T.One,T.Zero,T.Zero,T.Zero,T.Zero,T.One);
    public static Matrix4<T> Translation(T x,T y,T z) => new(T.One,T.Zero,T.Zero,x,T.Zero,T.One,T.Zero,y,T.Zero,T.Zero,T.One,z,T.Zero,T.Zero,T.Zero,T.One);
    public static Matrix4<T> Scale(T x,T y,T z) => new(x,T.Zero,T.Zero,T.Zero,T.Zero,y,T.Zero,T.Zero,T.Zero,T.Zero,z,T.Zero,T.Zero,T.Zero,T.Zero,T.One);
    public static Matrix4<T> RotationX(T angle) {var c=T.Cos(angle);var s=T.Sin(angle);return new(T.One,T.Zero,T.Zero,T.Zero,T.Zero,c,-s,T.Zero,T.Zero,s,c,T.Zero,T.Zero,T.Zero,T.Zero,T.One);}
    public static Matrix4<T> RotationY(T angle) {var c=T.Cos(angle);var s=T.Sin(angle);return new(c,T.Zero,s,T.Zero,T.Zero,T.One,T.Zero,T.Zero,-s,T.Zero,c,T.Zero,T.Zero,T.Zero,T.Zero,T.One);}
    public static Matrix4<T> RotationZ(T angle) {var c=T.Cos(angle);var s=T.Sin(angle);return new(c,-s,T.Zero,T.Zero,s,c,T.Zero,T.Zero,T.Zero,T.Zero,T.One,T.Zero,T.Zero,T.Zero,T.Zero,T.One);}
    /// <summary>Right-handed Rodrigues rotation; rejects a zero/nonfinite axis or nonfinite angle.</summary>
    public static Matrix4<T> RotationAxis(Vector3<T> axis,T angle)
    {
        if(!T.IsFinite(angle))throw new ArgumentOutOfRangeException(nameof(angle));
        var n=axis.Normalize();var x=n.X;var y=n.Y;var z=n.Z;var c=T.Cos(angle);var s=T.Sin(angle);var d=T.One-c;
        return new(c+x*x*d,x*y*d-z*s,x*z*d+y*s,T.Zero,y*x*d+z*s,c+y*y*d,y*z*d-x*s,T.Zero,z*x*d-y*s,z*y*d+x*s,c+z*z*d,T.Zero,T.Zero,T.Zero,T.Zero,T.One);
    }
    public T this[int row,int column] => (row,column) switch {
        (0,0)=>M11,(0,1)=>M12,(0,2)=>M13,(0,3)=>M14,(1,0)=>M21,(1,1)=>M22,(1,2)=>M23,(1,3)=>M24,
        (2,0)=>M31,(2,1)=>M32,(2,2)=>M33,(2,3)=>M34,(3,0)=>M41,(3,1)=>M42,(3,2)=>M43,(3,3)=>M44,
        _=>throw new ArgumentOutOfRangeException(nameof(row),"Indices must be 0 through 3.")};
    private static Matrix4<T> From(T[,] a)=>new(a[0,0],a[0,1],a[0,2],a[0,3],a[1,0],a[1,1],a[1,2],a[1,3],a[2,0],a[2,1],a[2,2],a[2,3],a[3,0],a[3,1],a[3,2],a[3,3]);
    public static Matrix4<T> operator *(Matrix4<T> a,Matrix4<T> b)
    {
        T At(int i,int j)=>a[i,0]*b[0,j]+a[i,1]*b[1,j]+a[i,2]*b[2,j]+a[i,3]*b[3,j];
        return new(At(0,0),At(0,1),At(0,2),At(0,3),At(1,0),At(1,1),At(1,2),At(1,3),At(2,0),At(2,1),At(2,2),At(2,3),At(3,0),At(3,1),At(3,2),At(3,3));
    }
    public Matrix4<T> Transpose()=>new(M11,M21,M31,M41,M12,M22,M32,M42,M13,M23,M33,M43,M14,M24,M34,M44);
    public T Determinant
    {
        get {
            var self=this;
            T Minor(int column) {
                var v=new T[9];var k=0;for(var i=1;i<4;i++)for(var j=0;j<4;j++)if(j!=column)v[k++]=self[i,j];
                return v[0]*(v[4]*v[8]-v[5]*v[7])-v[1]*(v[3]*v[8]-v[5]*v[6])+v[2]*(v[3]*v[7]-v[4]*v[6]);
            }
            return M11*Minor(0)-M12*Minor(1)+M13*Minor(2)-M14*Minor(3);
        }
    }
    public Matrix4<T> Inverse()
    {
        var a=new T[4,8];
        for(var i=0;i<4;i++)for(var j=0;j<4;j++) {
            if(!T.IsFinite(this[i,j]))throw new InvalidOperationException("Inverse requires finite entries.");
            a[i,j]=this[i,j];a[i,j+4]=i==j?T.One:T.Zero;
        }
        for(var k=0;k<4;k++) {
            var pivot=k;for(var i=k+1;i<4;i++)if(T.Abs(a[i,k])>T.Abs(a[pivot,k]))pivot=i;
            if(a[pivot,k]==T.Zero)throw new InvalidOperationException("Matrix is singular.");
            if(pivot!=k)for(var j=0;j<8;j++)(a[k,j],a[pivot,j])=(a[pivot,j],a[k,j]);
            var divisor=a[k,k];for(var j=0;j<8;j++)a[k,j]/=divisor;
            for(var i=0;i<4;i++)if(i!=k) {var factor=a[i,k];for(var j=0;j<8;j++)a[i,j]-=factor*a[k,j];}
            for(var i=0;i<4;i++)for(var j=0;j<8;j++)if(!T.IsFinite(a[i,j]))throw new InvalidOperationException("Inverse is not representable.");
        }
        var result=new T[4,4];for(var i=0;i<4;i++)for(var j=0;j<4;j++)result[i,j]=a[i,j+4];return From(result);
    }
    public Vector3<T> TransformPoint(Vector3<T> p)
    {
        var w=M41*p.X+M42*p.Y+M43*p.Z+M44;
        if(w==T.Zero || !T.IsFinite(w))throw new InvalidOperationException("Point has an invalid homogeneous coordinate.");
        return new((M11*p.X+M12*p.Y+M13*p.Z+M14)/w,(M21*p.X+M22*p.Y+M23*p.Z+M24)/w,(M31*p.X+M32*p.Y+M33*p.Z+M34)/w);
    }
    /// <summary>Transforms w=0 directions; requires an affine bottom row (0,0,0,1).</summary>
    public Vector3<T> TransformDirection(Vector3<T> p)
    {
        if(M41!=T.Zero || M42!=T.Zero || M43!=T.Zero || M44!=T.One)throw new InvalidOperationException("Direction transformation requires an affine matrix.");
        return new(M11*p.X+M12*p.Y+M13*p.Z,M21*p.X+M22*p.Y+M23*p.Z,M31*p.X+M32*p.Y+M33*p.Z);
    }
}
