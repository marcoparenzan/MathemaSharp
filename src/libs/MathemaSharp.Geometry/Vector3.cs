// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
namespace MathemaSharp.Geometry;

/// <summary>A three-dimensional vector. Angles are radians; normalization rejects nonfinite and zero vectors.</summary>
public readonly record struct Vector3<T>(T X, T Y, T Z) where T : IFloatingPointIeee754<T>
{
    public static Vector3<T> Zero => new(T.Zero,T.Zero,T.Zero);
    public static Vector3<T> operator +(Vector3<T> a, Vector3<T> b) => new(a.X+b.X,a.Y+b.Y,a.Z+b.Z);
    public static Vector3<T> operator -(Vector3<T> a, Vector3<T> b) => new(a.X-b.X,a.Y-b.Y,a.Z-b.Z);
    public static Vector3<T> operator -(Vector3<T> a) => new(-a.X,-a.Y,-a.Z);
    public static Vector3<T> operator *(Vector3<T> a, T s) => new(a.X*s,a.Y*s,a.Z*s);
    public static Vector3<T> operator *(T s, Vector3<T> a) => a*s;
    public static Vector3<T> operator /(Vector3<T> a, T s) => new(a.X/s,a.Y/s,a.Z/s);
    public static T Dot(Vector3<T> a, Vector3<T> b) => a.X*b.X+a.Y*b.Y+a.Z*b.Z;
    public static Vector3<T> Cross(Vector3<T> a, Vector3<T> b) => new(a.Y*b.Z-a.Z*b.Y,a.Z*b.X-a.X*b.Z,a.X*b.Y-a.Y*b.X);
    public T LengthSquared => Dot(this,this);
    public T Length => T.Hypot(T.Hypot(X,Y),Z);
    public Vector3<T> Normalize()
    {
        var scale=T.Max(T.Max(T.Abs(X),T.Abs(Y)),T.Abs(Z));
        if(scale==T.Zero || !T.IsFinite(scale)) throw new ArgumentException("Normalization requires a finite nonzero vector.");
        var v=this/scale; return v/v.Length;
    }
    public static T Distance(Vector3<T> a, Vector3<T> b) => (a-b).Length;
    public static T Angle(Vector3<T> a, Vector3<T> b) { var x=a.Normalize(); var y=b.Normalize(); return T.Atan2(Cross(x,y).Length,Dot(x,y)); }
    public static Vector3<T> Lerp(Vector3<T> a, Vector3<T> b, T t) => a+(b-a)*t;
    public Vector3<T> ProjectOnto(Vector3<T> target) { var n=target.Normalize(); return n*Dot(this,n); }
}
