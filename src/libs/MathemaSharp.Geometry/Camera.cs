// Copyright (c) 2026 Marco Parenzan
// Licensed under the MIT License. See the LICENSE file in the project root.
using System.Numerics;
namespace MathemaSharp.Geometry;

/// <summary>Legacy mplot3d perspective camera: box aspect 4:4:3, distance 10, focal length 1.
/// Arithmetic order deliberately preserves NDSharp double results. Angles are degrees.
/// No clipping, orthographic mode or input validation is added: singular views/ranges retain IEEE results.</summary>
public static class Camera<T> where T:IFloatingPointIeee754<T>
{
    private static T N(double value)=>T.CreateChecked(value);
    private static T[,] Matrix((T Lo,T Hi)[] limits,T elevation,T azimuth)
    {
        T[] box=[N(4),N(4),N(3)];var sum=T.Zero;foreach(var b in box)sum+=b*b;
        var n=T.Sqrt(sum);var asp=box.Select(b=>b/n*N(1.8294640721620434)).ToArray();var r=asp.Select(v=>v*N(.5)).ToArray();
        var el=elevation*T.Pi/N(180);var az=azimuth*T.Pi/N(180);
        T[] ps=[T.Cos(el)*T.Cos(az),T.Cos(el)*T.Sin(az),T.Sin(el)];
        T[] eye=[r[0]+N(10)*ps[0],r[1]+N(10)*ps[1],r[2]+N(10)*ps[2]];
        var w=Norm([eye[0]-r[0],eye[1]-r[1],eye[2]-r[2]]);var u=Norm(Cross([T.Zero,T.Zero,T.One],w));var v=Cross(w,u);
        T[,] view={{u[0],u[1],u[2],-Dot(u,eye)},{v[0],v[1],v[2],-Dot(v,eye)},{w[0],w[1],w[2],-Dot(w,eye)},{T.Zero,T.Zero,T.Zero,T.One}};
        T[,] projection={{T.One,T.Zero,T.Zero,T.Zero},{T.Zero,T.One,T.Zero,T.Zero},{T.Zero,T.Zero,T.Zero,-N(10)},{T.Zero,T.Zero,-T.One,T.Zero}};
        var world=new T[4,4];for(var i=0;i<4;i++)for(var j=0;j<4;j++)world[i,j]=T.Zero;
        for(var i=0;i<3;i++){var (lo,hi)=limits[i];world[i,i]=asp[i]/(hi-lo);world[i,3]=-lo*asp[i]/(hi-lo);}world[3,3]=T.One;
        return Mul(Mul(projection,view),world);
    }
    private static T[,] Mul(T[,] a,T[,] b)
    {
        var result=new T[4,4];for(var i=0;i<4;i++)for(var j=0;j<4;j++){result[i,j]=T.Zero;for(var k=0;k<4;k++)result[i,j]+=a[i,k]*b[k,j];}return result;
    }
    private static T[] Cross(T[] a,T[] b)=>[a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]];
    private static T Dot(T[] a,T[] b)=>a[0]*b[0]+a[1]*b[1]+a[2]*b[2];
    private static T[] Norm(T[] a){var length=T.Sqrt(Dot(a,a));return [a[0]/length,a[1]/length,a[2]/length];}
    private static (T X,T Y,T Depth) Apply(T[,] m,T x,T y,T z)
    {
        T[] input=[x,y,z,T.One];var output=new T[4];for(var i=0;i<4;i++)output[i]=m[i,0]*input[0]+m[i,1]*input[1]+m[i,2]*input[2]+m[i,3];
        return(output[0]/output[3],output[1]/output[3],output[2]/output[3]);
    }
    private static (T X,T Y,T Scale) Fit((T Lo,T Hi)[] limits,T elevation,T azimuth,T width,T height)
    {
        var m=Matrix(limits,elevation,azimuth);
        var minx=T.CreateSaturating(double.MaxValue);var maxx=T.CreateSaturating(double.MinValue);var miny=minx;var maxy=maxx;
        for(var k=0;k<8;k++){
            var p=new T[3];for(var i=0;i<3;i++){var(lo,hi)=limits[i];p[i]=(k>>i&1)==0?lo:hi;}
            var(x,y,_)=Apply(m,p[0],p[1],p[2]);minx=T.Min(minx,x);maxx=T.Max(maxx,x);miny=T.Min(miny,y);maxy=T.Max(maxy,y);
        }
        var scale=N(.86)*T.Min(width/(maxx-minx),height/(maxy-miny));
        return((minx+maxx)/N(2),(miny+maxy)/N(2),scale);
    }
    /// <summary>Projects data to screen coordinates (screen Y increases downward). Viewport mids are caller-supplied to preserve native pixel arithmetic.</summary>
    public static (T X,T Y,T Depth) Project((T Lo,T Hi) xlim,(T Lo,T Hi) ylim,(T Lo,T Hi) zlim,T elevation,T azimuth,T width,T height,T midX,T midY,T x,T y,T z)
    {
        (T Lo,T Hi)[] limits=[xlim,ylim,zlim];var m=Matrix(limits,elevation,azimuth);var(px,py,d)=Apply(m,x,y,z);var(cx,cy,s)=Fit(limits,elevation,azimuth,width,height);
        return(midX+(px-cx)*s,midY-(py-cy)*s,d);
    }
}

/// <summary>Painter-order operations; equal depths preserve input order.</summary>
public static class DepthOrdering
{
    public static IOrderedEnumerable<TItem> BackToFront<TItem,T>(IEnumerable<TItem> items,Func<TItem,T> depth) where T:INumber<T>
        =>items.OrderByDescending(depth);
    public static T Average<T>(IEnumerable<T> depths) where T:INumber<T>
    {
        var sum=T.Zero;var count=0;foreach(var value in depths){sum+=value;count=checked(count+1);}
        if(count==0)throw new InvalidOperationException("Sequence contains no elements");return sum/T.CreateChecked(count);
    }
}
