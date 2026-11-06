// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
namespace MathemaSharp.Geometry;

public readonly record struct Segment2<T>(Vector2<T> Start,Vector2<T> End) where T:IFloatingPointIeee754<T>;
public enum IntersectionKind { None, Point, TwoPoints, Overlap, Coincident }
/// <summary>First/Second are meaningful for Point, TwoPoints or Overlap only.
/// Overlap endpoints follow the first segment; Coincident denotes infinitely many circle intersections.</summary>
public readonly record struct Intersection2<T>(IntersectionKind Kind,Vector2<T> First,Vector2<T> Second) where T:IFloatingPointIeee754<T>;

/// <summary>Finite Cartesian geometry using exact comparisons in T, including boundary points.
/// No epsilon-based snapping or adaptive exact predicates; overflow and ill-conditioned cases are not guaranteed.</summary>
public static class Intersections
{
    private static void Finite<T>(Vector2<T> p) where T:IFloatingPointIeee754<T>
    {
        if(!T.IsFinite(p.X)||!T.IsFinite(p.Y))throw new ArgumentException("Coordinates must be finite.");
    }
    private static void Radius<T>(T radius) where T:IFloatingPointIeee754<T>
    {
        if(!T.IsFinite(radius)||radius<T.Zero)throw new ArgumentOutOfRangeException(nameof(radius),"Radius must be finite and nonnegative.");
    }
    private static Intersection2<T> None<T>() where T:IFloatingPointIeee754<T> =>new(IntersectionKind.None,Vector2<T>.Zero,Vector2<T>.Zero);
    private static Intersection2<T> Point<T>(Vector2<T> p) where T:IFloatingPointIeee754<T> =>new(IntersectionKind.Point,p,p);
    private static bool OnSegment<T>(Vector2<T> p,Segment2<T> s) where T:IFloatingPointIeee754<T> => Vector2<T>.Cross(s.End-s.Start,p-s.Start)==T.Zero&&p.X>=T.Min(s.Start.X,s.End.X)&&p.X<=T.Max(s.Start.X,s.End.X)&&p.Y>=T.Min(s.Start.Y,s.End.Y)&&p.Y<=T.Max(s.Start.Y,s.End.Y);
    /// <summary>Closed segments. Parallel disjoint segments return None; collinear overlap returns its endpoints.</summary>
    public static Intersection2<T> SegmentSegment<T>(Segment2<T> a,Segment2<T> b) where T:IFloatingPointIeee754<T>
    {
        Finite(a.Start);Finite(a.End);Finite(b.Start);Finite(b.End);
        var r=a.End-a.Start;var s=b.End-b.Start;var q=b.Start-a.Start;
        if(a.Start==a.End)return OnSegment(a.Start,b)?Point(a.Start):None<T>();
        if(b.Start==b.End)return OnSegment(b.Start,a)?Point(b.Start):None<T>();
        var cross=Vector2<T>.Cross(r,s);
        if(cross!=T.Zero) {
            var t=Vector2<T>.Cross(q,s)/cross;var u=Vector2<T>.Cross(q,r)/cross;
            return t>=T.Zero&&t<=T.One&&u>=T.Zero&&u<=T.One?Point(a.Start+r*t):None<T>();
        }
        if(Vector2<T>.Cross(q,r)!=T.Zero)return None<T>();
        var useX=T.Abs(r.X)>=T.Abs(r.Y);
        var first=useX?(b.Start.X-a.Start.X)/r.X:(b.Start.Y-a.Start.Y)/r.Y;
        var second=useX?(b.End.X-a.Start.X)/r.X:(b.End.Y-a.Start.Y)/r.Y;
        var start=T.Max(T.Zero,T.Min(first,second));var end=T.Min(T.One,T.Max(first,second));
        if(start>end)return None<T>();
        var p=a.Start+r*start;return start==end?Point(p):new(IntersectionKind.Overlap,p,a.Start+r*end);
    }
    /// <summary>Infinite line origin + t*direction; results ordered by increasing t. Zero direction is rejected.</summary>
    public static Intersection2<T> LineCircle<T>(Vector2<T> origin,Vector2<T> direction,Vector2<T> center,T radius) where T:IFloatingPointIeee754<T>
    {
        Finite(origin);Finite(direction);Finite(center);Radius(radius);var unit=direction.Normalize();
        var along=Vector2<T>.Dot(center-origin,unit);var closest=origin+unit*along;
        var distance=Vector2<T>.Distance(closest,center);
        if(distance>radius)return None<T>();
        if(distance==radius)return Point(closest);
        var offset=T.Sqrt((radius-distance)*(radius+distance));
        return new(IntersectionKind.TwoPoints,closest-unit*offset,closest+unit*offset);
    }
    /// <summary>Closed circle circumferences. Two-point results use the positive then negative perpendicular to center2-center1.</summary>
    public static Intersection2<T> CircleCircle<T>(Vector2<T> center1,T radius1,Vector2<T> center2,T radius2) where T:IFloatingPointIeee754<T>
    {
        Finite(center1);Finite(center2);Radius(radius1);Radius(radius2);
        var delta=center2-center1;var distance=delta.Length;
        if(distance==T.Zero) {
            if(radius1!=radius2)return None<T>();
            return radius1==T.Zero?Point(center1):new(IntersectionKind.Coincident,Vector2<T>.Zero,Vector2<T>.Zero);
        }
        if(distance>radius1+radius2||distance<T.Abs(radius1-radius2))return None<T>();
        var unit=delta.Normalize();var two=T.CreateChecked(2);
        var along=(distance+(radius1-radius2)*(radius1+radius2)/distance)/two;
        var closest=center1+unit*along;
        if(distance==radius1+radius2||distance==T.Abs(radius1-radius2))return Point(closest);
        var squared=(radius1-along)*(radius1+along);
        if(squared<T.Zero)throw new InvalidOperationException("Circle intersection is numerically unresolved.");
        var offset=new Vector2<T>(-unit.Y,unit.X)*T.Sqrt(squared);
        return new(IntersectionKind.TwoPoints,closest+offset,closest-offset);
    }
    /// <summary>Unsigned distance to a closed segment; a zero-length segment is a point.</summary>
    public static T PointSegmentDistance<T>(Vector2<T> point,Segment2<T> segment) where T:IFloatingPointIeee754<T>
    {
        Finite(point);Finite(segment.Start);Finite(segment.End);
        var delta=segment.End-segment.Start;var length=delta.Length;
        if(length==T.Zero)return Vector2<T>.Distance(point,segment.Start);
        var unit=delta.Normalize();var along=T.Clamp(Vector2<T>.Dot(point-segment.Start,unit),T.Zero,length);
        return Vector2<T>.Distance(point,segment.Start+unit*along);
    }
    /// <summary>Distance to the filled polygon under nonzero winding: zero inside or on boundary.
    /// Empty input is rejected; one/two points reduce to point/segment distance.</summary>
    public static T PointPolygonDistance<T>(Vector2<T> point,ReadOnlySpan<Vector2<T>> polygon) where T:IFloatingPointIeee754<T>
    {
        if(polygon.IsEmpty)throw new ArgumentException("Polygon requires a vertex.",nameof(polygon));
        if(Polygons.LocatePoint(polygon,point)!=PointLocation.Outside)return T.Zero;
        var minimum=T.PositiveInfinity;
        for(var i=0;i<polygon.Length;i++)minimum=T.Min(minimum,PointSegmentDistance(point,new Segment2<T>(polygon[i],polygon[(i+1)%polygon.Length])));
        return minimum;
    }
    /// <summary>Closed disk membership, including the boundary; radius zero is a point.</summary>
    public static bool InsideCircle<T>(Vector2<T> point,Vector2<T> center,T radius) where T:IFloatingPointIeee754<T>
    {
        Finite(point);Finite(center);Radius(radius);return Vector2<T>.Distance(point,center)<=radius;
    }
    /// <summary>Closed axis-aligned ellipse membership; both semiaxes must be strictly positive.</summary>
    public static bool InsideEllipse<T>(Vector2<T> point,Vector2<T> center,T semiAxisX,T semiAxisY) where T:IFloatingPointIeee754<T>
    {
        Finite(point);Finite(center);Radius(semiAxisX);Radius(semiAxisY);
        if(semiAxisX==T.Zero||semiAxisY==T.Zero)throw new ArgumentOutOfRangeException(nameof(semiAxisX),"Ellipse semiaxes must be positive.");
        return T.Hypot((point.X-center.X)/semiAxisX,(point.Y-center.Y)/semiAxisY)<=T.One;
    }
}
