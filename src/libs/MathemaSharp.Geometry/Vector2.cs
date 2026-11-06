// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
namespace MathemaSharp.Geometry;

/// <summary>A two-dimensional vector. Angles are radians. Zero vectors cannot be normalized or used as angle/projection references.</summary>
public readonly record struct Vector2<T>(T X, T Y) where T : IFloatingPointIeee754<T>
{
    public static Vector2<T> Zero => new(T.Zero, T.Zero);
    public static Vector2<T> operator +(Vector2<T> a, Vector2<T> b) => new(a.X+b.X,a.Y+b.Y);
    public static Vector2<T> operator -(Vector2<T> a, Vector2<T> b) => new(a.X-b.X,a.Y-b.Y);
    public static Vector2<T> operator -(Vector2<T> a) => new(-a.X,-a.Y);
    public static Vector2<T> operator *(Vector2<T> a, T s) => new(a.X*s,a.Y*s);
    public static Vector2<T> operator *(T s, Vector2<T> a) => a*s;
    public static Vector2<T> operator /(Vector2<T> a, T s) => new(a.X/s,a.Y/s);
    public static T Dot(Vector2<T> a, Vector2<T> b) => a.X*b.X+a.Y*b.Y;
    public static T Cross(Vector2<T> a, Vector2<T> b) => a.X*b.Y-a.Y*b.X;
    public T LengthSquared => Dot(this,this);
    public T Length => T.Hypot(X,Y);
    public Vector2<T> Normalize()
    {
        var scale=T.Max(T.Abs(X),T.Abs(Y));
        if(scale==T.Zero || !T.IsFinite(scale)) throw new ArgumentException("Normalization requires a finite nonzero vector.");
        var v=this/scale; return v/v.Length;
    }
    public static T Distance(Vector2<T> a, Vector2<T> b) => (a-b).Length;
    public static T Angle(Vector2<T> a, Vector2<T> b) { var x=a.Normalize(); var y=b.Normalize(); return T.Atan2(T.Abs(Cross(x,y)),Dot(x,y)); }
    public static Vector2<T> Lerp(Vector2<T> a, Vector2<T> b, T t) => a+(b-a)*t;
    public Vector2<T> ProjectOnto(Vector2<T> target) { var n=target.Normalize(); return n*Dot(this,n); }
}
