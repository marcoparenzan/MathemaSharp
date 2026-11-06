// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
namespace MathemaSharp.Geometry;

/// <summary>Quaternion in (x,y,z,w) order, using right-handed rotations and Hamilton multiplication.
/// A*B applies rotation B first. Equality compares components, not equivalent rotations q and -q.</summary>
public readonly record struct Quaternion<T>(T X,T Y,T Z,T W) where T:IFloatingPointIeee754<T>
{
    public static Quaternion<T> Identity=>new(T.Zero,T.Zero,T.Zero,T.One);
    public T LengthSquared=>X*X+Y*Y+Z*Z+W*W;
    public T Length=>T.Hypot(T.Hypot(X,Y),T.Hypot(Z,W));
    public Quaternion<T> Conjugate()=>new(-X,-Y,-Z,W);
    public static Quaternion<T> operator -(Quaternion<T> q)=>new(-q.X,-q.Y,-q.Z,-q.W);
    public static Quaternion<T> operator *(Quaternion<T> a,Quaternion<T> b)=>new(
        a.W*b.X+a.X*b.W+a.Y*b.Z-a.Z*b.Y,
        a.W*b.Y-a.X*b.Z+a.Y*b.W+a.Z*b.X,
        a.W*b.Z+a.X*b.Y-a.Y*b.X+a.Z*b.W,
        a.W*b.W-a.X*b.X-a.Y*b.Y-a.Z*b.Z);
    public Quaternion<T> Normalize()
    {
        if(!T.IsFinite(X)||!T.IsFinite(Y)||!T.IsFinite(Z)||!T.IsFinite(W))throw new ArgumentException("Normalization requires a finite nonzero quaternion.");
        var s=T.Max(T.Max(T.Abs(X),T.Abs(Y)),T.Max(T.Abs(Z),T.Abs(W)));
        if(s==T.Zero)throw new ArgumentException("Normalization requires a finite nonzero quaternion.");
        var q=new Quaternion<T>(X/s,Y/s,Z/s,W/s);var n=q.Length;
        return new(q.X/n,q.Y/n,q.Z/n,q.W/n);
    }
    public static Quaternion<T> FromAxisAngle(Vector3<T> axis,T radians)
    {
        if(!T.IsFinite(radians))throw new ArgumentOutOfRangeException(nameof(radians));
        var n=axis.Normalize();var half=radians/T.CreateChecked(2);var s=T.Sin(half);
        return new(n.X*s,n.Y*s,n.Z*s,T.Cos(half));
    }
    /// <summary>Normalizes this quaternion before converting it to a homogeneous rotation matrix.</summary>
    public Matrix4<T> ToRotationMatrix()
    {
        var q=Normalize();var x=q.X;var y=q.Y;var z=q.Z;var w=q.W;var two=T.CreateChecked(2);
        return new(T.One-two*(y*y+z*z),two*(x*y-z*w),two*(x*z+y*w),T.Zero,
            two*(x*y+z*w),T.One-two*(x*x+z*z),two*(y*z-x*w),T.Zero,
            two*(x*z-y*w),two*(y*z+x*w),T.One-two*(x*x+y*y),T.Zero,
            T.Zero,T.Zero,T.Zero,T.One);
    }
    /// <summary>Accepts only a finite homogeneous pure rotation, tested with a tolerance of 64 unit roundoff spacings.
    /// Rejects translation, reflection, scaling and shear outside that tolerance.</summary>
    public static Quaternion<T> FromRotationMatrix(Matrix4<T> m)
    {
        var tolerance=T.CreateChecked(64)*(T.BitIncrement(T.One)-T.One);
        bool Near(T a,T b)=>T.Abs(a-b)<=tolerance;
        for(var i=0;i<4;i++)for(var j=0;j<4;j++)if(!T.IsFinite(m[i,j]))throw new ArgumentException("Rotation matrix must be finite.",nameof(m));
        if(m.M14!=T.Zero||m.M24!=T.Zero||m.M34!=T.Zero||m.M41!=T.Zero||m.M42!=T.Zero||m.M43!=T.Zero||m.M44!=T.One)
            throw new ArgumentException("Expected a homogeneous pure rotation.",nameof(m));
        var product=m*m.Transpose();
        for(var i=0;i<3;i++)for(var j=0;j<3;j++)if(!Near(product[i,j],i==j?T.One:T.Zero))throw new ArgumentException("Rotation must be orthogonal.",nameof(m));
        if(!Near(m.Determinant,T.One))throw new ArgumentException("Rotation must have determinant one.",nameof(m));
        var two=T.CreateChecked(2);var four=T.CreateChecked(4);var trace=m.M11+m.M22+m.M33;
        Quaternion<T> q;
        if(trace>T.Zero) {
            var s=two*T.Sqrt(T.One+trace);
            q=new((m.M32-m.M23)/s,(m.M13-m.M31)/s,(m.M21-m.M12)/s,s/four);
        } else if(m.M11>m.M22 && m.M11>m.M33) {
            var s=two*T.Sqrt(T.One+m.M11-m.M22-m.M33);
            q=new(s/four,(m.M12+m.M21)/s,(m.M13+m.M31)/s,(m.M32-m.M23)/s);
        } else if(m.M22>m.M33) {
            var s=two*T.Sqrt(T.One+m.M22-m.M11-m.M33);
            q=new((m.M12+m.M21)/s,s/four,(m.M23+m.M32)/s,(m.M13-m.M31)/s);
        } else {
            var s=two*T.Sqrt(T.One+m.M33-m.M11-m.M22);
            q=new((m.M13+m.M31)/s,(m.M23+m.M32)/s,s/four,(m.M21-m.M12)/s);
        }
        return q.Normalize();
    }
    public Vector3<T> Rotate(Vector3<T> vector)=>ToRotationMatrix().TransformDirection(vector);
    /// <summary>Shortest-path interpolation of normalized endpoints for finite t in [0,1].
    /// Antipodal endpoints represent the same rotation. Nearly coincident endpoints use normalized linear interpolation.</summary>
    public static Quaternion<T> Slerp(Quaternion<T> from,Quaternion<T> to,T t)
    {
        if(!T.IsFinite(t)||t<T.Zero||t>T.One)throw new ArgumentOutOfRangeException(nameof(t));
        var a=from.Normalize();var b=to.Normalize();var dot=a.X*b.X+a.Y*b.Y+a.Z*b.Z+a.W*b.W;
        if(dot<T.Zero){b=-b;dot=-dot;}
        dot=T.Min(T.One,dot);
        var threshold=T.CreateChecked(8)*(T.BitIncrement(T.One)-T.One);
        T left,right;
        if(T.One-dot<=threshold){left=T.One-t;right=t;}
        else {var angle=T.Acos(dot);var denominator=T.Sin(angle);left=T.Sin((T.One-t)*angle)/denominator;right=T.Sin(t*angle)/denominator;}
        return new Quaternion<T>(left*a.X+right*b.X,left*a.Y+right*b.Y,left*a.Z+right*b.Z,left*a.W+right*b.W).Normalize();
    }
}
