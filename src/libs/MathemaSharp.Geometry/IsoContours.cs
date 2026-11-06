// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
namespace MathemaSharp.Geometry;

/// <summary>Unconstrained coordinate record preserves existing INumber contour consumers, including decimal.</summary>
public readonly record struct ContourSegment<T>(T X1,T Y1,T X2,T Y2);
public static class IsoContours
{
    /// <summary>Sample-index contours with fixed edge-order saddle pairing and optional exact-vertex zero-length segments.
    /// Unmasked values/levels must be finite within ±1e12; 1–100000 samples. Masked cells are skipped.</summary>
    public static IReadOnlyList<Segment2<T>> MarchingSquares<T>(T[,] field,T level,bool[,]? mask=null,bool preserveDegenerateSegments=false) where T:IFloatingPointIeee754<T>
    {
        ArgumentNullException.ThrowIfNull(field);
        if(field.GetLength(0)==0||field.GetLength(1)==0||field.LongLength>100000)throw new ArgumentException("Fields require 1 to 100000 samples.",nameof(field));
        if(mask is not null&&(mask.GetLength(0)!=field.GetLength(0)||mask.GetLength(1)!=field.GetLength(1)))throw new ArgumentException("Mask dimensions must match samples.",nameof(mask));
        // Compare in double only for this legacy magnitude guard, not the interpolation arithmetic.
        for(var r=0;r<field.GetLength(0);r++)for(var c=0;c<field.GetLength(1);c++)if(mask?[r,c]!=true&&(!T.IsFinite(field[r,c])||double.CreateSaturating(T.Abs(field[r,c]))>1e12))throw new ArgumentException("Unmasked samples must be finite and within ±1e12.",nameof(field));
        if(!T.IsFinite(level)||double.CreateSaturating(T.Abs(level))>1e12)throw new ArgumentOutOfRangeException(nameof(level));
        var segments=Trace(field.GetLength(0),field.GetLength(1),(r,c)=>field[r,c],c=>T.CreateChecked(c),r=>T.CreateChecked(r),level,mask,false,preserveDegenerateSegments,false);
        var result=new Segment2<T>[segments.Count];for(var i=0;i<result.Length;i++){var s=segments[i];result[i]=new(new(s.X1,s.Y1),new(s.X2,s.Y2));}return result;
    }
    /// <summary>Shared generic traversal. Callers own validation; edge convention and summed-corner nonfinite policy are explicit.
    /// INumber is intentionally retained for binary compatibility of legacy PlotSharp rectilinear APIs.</summary>
    public static IReadOnlyList<ContourSegment<T>> Trace<T>(int rows,int columns,Func<int,int,T> sample,Func<int,T> x,Func<int,T> y,T level,bool[,]? mask,bool forwardOppositeEdges,bool preserveDegenerate,bool skipNonFiniteCornerSum) where T:INumber<T>
    {
        var segments=new List<ContourSegment<T>>();
        for(int row=0;row+1<rows;row++)for(int col=0;col+1<columns;col++) {
            if(mask?[row,col]==true||mask?[row,col+1]==true||mask?[row+1,col]==true||mask?[row+1,col+1]==true)continue;
            T a=sample(row,col),b=sample(row,col+1),c=sample(row+1,col+1),d=sample(row+1,col);
            if(skipNonFiniteCornerSum&&!T.IsFinite(a+b+c+d))continue;
            T x0=x(col),x1=x(col+1),y0=y(row),y1=y(row+1);
            var points=new List<(T X,T Y)>(4);
            void Edge(T v1,T v2,T px1,T py1,T px2,T py2) {
                if((v1<level)==(v2<level))return;
                T t=(level-v1)/(v2-v1);
                points.Add((px1+t*(px2-px1),py1+t*(py2-py1)));
            }
            Edge(a,b,x0,y0,x1,y0);Edge(b,c,x1,y0,x1,y1);
            if(forwardOppositeEdges){Edge(d,c,x0,y1,x1,y1);Edge(a,d,x0,y0,x0,y1);}
            else {Edge(c,d,x1,y1,x0,y1);Edge(d,a,x0,y1,x0,y0);}
            for(int i=0;i+1<points.Count;i+=2) {
                var p=points[i];var q=points[i+1];if(preserveDegenerate||p!=q)segments.Add(new(p.X,p.Y,q.X,q.Y));
            }
        }
        return segments;
    }
}
