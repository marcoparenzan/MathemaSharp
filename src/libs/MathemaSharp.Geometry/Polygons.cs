// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
namespace MathemaSharp.Geometry;

public enum PolygonOrientation { Degenerate, Clockwise, CounterClockwise }
public enum PointLocation { Outside, Boundary, Inside }
public readonly record struct BoundingBox2<T>(Vector2<T> Min, Vector2<T> Max) where T:IFloatingPointIeee754<T>;

/// <summary>Finite 2-D polygon operations in Cartesian coordinates. Polygons are implicitly closed;
/// a repeated closing vertex is allowed. Arithmetic uses T, exact comparisons and no geometric epsilon.
/// Self-intersections use algebraic signed area/centroid and nonzero winding for containment, not union area.
/// Overflow and ill-conditioned geometry are not compensated.</summary>
public static class Polygons
{
    private static void Validate<T>(ReadOnlySpan<Vector2<T>> points) where T:IFloatingPointIeee754<T>
    {
        foreach(var p in points)if(!T.IsFinite(p.X)||!T.IsFinite(p.Y))throw new ArgumentException("Coordinates must be finite.",nameof(points));
    }
    private static T Turn<T>(Vector2<T> a,Vector2<T> b,Vector2<T> c) where T:IFloatingPointIeee754<T> => Vector2<T>.Cross(b-a,c-a);
    /// <summary>Returns zero for fewer than three points.</summary>
    public static T SignedArea<T>(ReadOnlySpan<Vector2<T>> points) where T:IFloatingPointIeee754<T>
    {
        Validate(points);if(points.Length<3)return T.Zero;
        var sum=T.Zero;for(var i=0;i<points.Length;i++)sum+=Vector2<T>.Cross(points[i],points[(i+1)%points.Length]);
        return sum/T.CreateChecked(2);
    }
    public static PolygonOrientation Orientation<T>(ReadOnlySpan<Vector2<T>> points) where T:IFloatingPointIeee754<T>
    {
        var area=SignedArea(points);return area>T.Zero?PolygonOrientation.CounterClockwise:area<T.Zero?PolygonOrientation.Clockwise:PolygonOrientation.Degenerate;
    }
    /// <summary>Algebraic area centroid; rejects fewer than three points, zero area and unrepresentable results.</summary>
    public static Vector2<T> Centroid<T>(ReadOnlySpan<Vector2<T>> points) where T:IFloatingPointIeee754<T>
    {
        Validate(points);if(points.Length<3)throw new ArgumentException("Centroid requires at least three vertices.",nameof(points));
        var crossSum=T.Zero;var x=T.Zero;var y=T.Zero;
        for(var i=0;i<points.Length;i++) {
            var a=points[i];var b=points[(i+1)%points.Length];var cross=Vector2<T>.Cross(a,b);
            crossSum+=cross;x+=(a.X+b.X)*cross;y+=(a.Y+b.Y)*cross;
        }
        if(crossSum==T.Zero||!T.IsFinite(crossSum))throw new InvalidOperationException("Centroid requires nonzero representable signed area.");
        var divisor=T.CreateChecked(3)*crossSum;
        if(!T.IsFinite(divisor))throw new InvalidOperationException("Centroid denominator is not representable.");
        var result=new Vector2<T>(x/divisor,y/divisor);
        if(!T.IsFinite(result.X)||!T.IsFinite(result.Y))throw new InvalidOperationException("Centroid is not representable.");
        return result;
    }
    /// <summary>Closed perimeter: zero for zero/one vertex, twice the segment length for two vertices.</summary>
    public static T Perimeter<T>(ReadOnlySpan<Vector2<T>> points) where T:IFloatingPointIeee754<T>
    {
        Validate(points);if(points.Length<2)return T.Zero;
        var sum=T.Zero;for(var i=0;i<points.Length;i++)sum+=Vector2<T>.Distance(points[i],points[(i+1)%points.Length]);return sum;
    }
    /// <summary>Nonzero winding rule; boundary returned separately. Empty input is outside; one/two vertices have boundary only.</summary>
    public static PointLocation LocatePoint<T>(ReadOnlySpan<Vector2<T>> points,Vector2<T> point) where T:IFloatingPointIeee754<T>
    {
        Validate(points);if(!T.IsFinite(point.X)||!T.IsFinite(point.Y))throw new ArgumentException("Point must be finite.",nameof(point));
        var winding=0;
        for(var i=0;i<points.Length;i++) {
            var a=points[i];var b=points[(i+1)%points.Length];var cross=Turn(a,b,point);
            if(cross==T.Zero&&point.X>=T.Min(a.X,b.X)&&point.X<=T.Max(a.X,b.X)&&point.Y>=T.Min(a.Y,b.Y)&&point.Y<=T.Max(a.Y,b.Y))return PointLocation.Boundary;
            if(a.Y<=point.Y) {if(b.Y>point.Y&&cross>T.Zero)winding++;}
            else if(b.Y<=point.Y&&cross<T.Zero)winding--;
        }
        return winding==0?PointLocation.Outside:PointLocation.Inside;
    }
    /// <summary>Rejects empty input; a singleton has equal minimum and maximum.</summary>
    public static BoundingBox2<T> BoundingBox<T>(ReadOnlySpan<Vector2<T>> points) where T:IFloatingPointIeee754<T>
    {
        Validate(points);if(points.IsEmpty)throw new ArgumentException("Bounding box requires a point.",nameof(points));
        var min=points[0];var max=min;
        foreach(var p in points) {min=new(T.Min(min.X,p.X),T.Min(min.Y,p.Y));max=new(T.Max(max.X,p.X),T.Max(max.Y,p.Y));}
        return new(min,max);
    }
    /// <summary>Andrew monotone chain. Unique vertices in CCW order, lexicographically smallest first, no closing duplicate.
    /// Collinear intermediate vertices are removed; a collinear set returns its endpoints.</summary>
    public static Vector2<T>[] ConvexHull<T>(ReadOnlySpan<Vector2<T>> points) where T:IFloatingPointIeee754<T>
    {
        Validate(points);var sorted=points.ToArray();
        Array.Sort(sorted,(a,b)=> {var comparison=a.X.CompareTo(b.X);return comparison!=0?comparison:a.Y.CompareTo(b.Y);});
        var unique=new List<Vector2<T>>();foreach(var p in sorted)if(unique.Count==0||unique[^1]!=p)unique.Add(p);
        if(unique.Count<=2)return unique.ToArray();
        var hull=new List<Vector2<T>>();
        foreach(var p in unique) {while(hull.Count>=2&&Turn(hull[^2],hull[^1],p)<=T.Zero)hull.RemoveAt(hull.Count-1);hull.Add(p);}
        var lower=hull.Count;
        for(var i=unique.Count-2;i>=0;i--) {var p=unique[i];while(hull.Count>lower&&Turn(hull[^2],hull[^1],p)<=T.Zero)hull.RemoveAt(hull.Count-1);hull.Add(p);}
        hull.RemoveAt(hull.Count-1);return hull.ToArray();
    }
    private static T SegmentDistance<T>(Vector2<T> p,Vector2<T> a,Vector2<T> b) where T:IFloatingPointIeee754<T>
    {
        var delta=b-a;var length=delta.Length;if(length==T.Zero)return Vector2<T>.Distance(p,a);
        var direction=delta.Normalize();var distance=T.Clamp(Vector2<T>.Dot(p-a,direction),T.Zero,length);
        return Vector2<T>.Distance(p,a+direction*distance);
    }
    /// <summary>Iterative Douglas-Peucker for an open polyline; preserves endpoints and input order.
    /// Equality to tolerance removes a point. Zero tolerance removes exactly collinear intermediates.
    /// A repeated first/last endpoint is treated as an open polyline, not a topology-preserving polygon simplification.</summary>
    public static Vector2<T>[] Simplify<T>(ReadOnlySpan<Vector2<T>> points,T tolerance) where T:IFloatingPointIeee754<T>
    {
        Validate(points);if(!T.IsFinite(tolerance)||tolerance<T.Zero)throw new ArgumentOutOfRangeException(nameof(tolerance));
        if(points.Length<=2)return points.ToArray();
        var keep=new bool[points.Length];keep[0]=keep[^1]=true;var pending=new Stack<(int Start,int End)>();pending.Push((0,points.Length-1));
        while(pending.TryPop(out var segment)) {
            var furthest=-1;var maximum=tolerance;
            for(var i=segment.Start+1;i<segment.End;i++) {
                var distance=SegmentDistance(points[i],points[segment.Start],points[segment.End]);
                if(distance>maximum){maximum=distance;furthest=i;}
            }
            if(furthest>=0){keep[furthest]=true;pending.Push((segment.Start,furthest));pending.Push((furthest,segment.End));}
        }
        var result=new List<Vector2<T>>();for(var i=0;i<points.Length;i++)if(keep[i])result.Add(points[i]);return result.ToArray();
    }
}
