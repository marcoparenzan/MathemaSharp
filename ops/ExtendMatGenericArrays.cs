#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\MatSharp\src\MatSharpLib";
File.WriteAllText(Path.Combine(root,"Runtime","GenericScalar.cs"),"""
using System.Globalization;
using System.Numerics;
namespace MatSharpLib.Runtime;

public interface IGenericScalar
{
    MValue Binary(string op,MValue other,bool reversed=false);
    MValue Unary(string op);
    MValue Transpose();
    MValue Cat(int axis,IList<MValue> values);
}

/// <summary>Typed real 2D numeric storage, including scalars. Column-major; no conversion through double.</summary>
public sealed class GenericScalar<T> : MValue, IGenericScalar where T:INumber<T>
{
    private T[] data;
    public T Value=>IsScalar?data[0]:throw new InvalidOperationException("Not a scalar.");
    public T[] ToArray()=>(T[])data.Clone();
    public GenericScalar(T value):this(new[]{value},new[]{1,1}){}
    public GenericScalar(T[] values,int[] dimensions)
    {
        if(dimensions.Length!=2 || dimensions.Any(d=>d<0) || checked(dimensions[0]*dimensions[1])!=values.Length)
            throw new MatException("MathemaSharp:typed:shape","Expected a valid 2D column-major shape.");
        Dims=(int[])dimensions.Clone();data=(T[])values.Clone();
    }
    public override string ClassName=>typeof(T).Name.ToLowerInvariant();
    public override string ToString()=>IsScalar?Value.ToString(null,CultureInfo.InvariantCulture):$"{Rows}x{Cols} {ClassName}";
    public static MValue Parse(string value)=>new GenericScalar<T>(T.Parse(value,CultureInfo.InvariantCulture));
    private static GenericScalar<T> Read(MValue value)
    {
        if(value is GenericScalar<T> typed)return typed;
        if(value is Mat {Numel:1,Im:null} m && m.Class!=MClass.Char && double.IsFinite(m.Re[0]) && Math.Truncate(m.Re[0])==m.Re[0] && Math.Abs(m.Re[0])<=9007199254740991d)
            return new GenericScalar<T>(T.CreateChecked((long)m.Re[0]));
        throw new MatException("MathemaSharp:typed:mixedType","Use the same numeric type or an exactly representable integer scalar.");
    }
    public MValue Unary(string op)
    {
        if(op=="+")return Clone();
        if(op!="-")throw new MatException("MathemaSharp:typed:operator","Unsupported unary operator.");
        return new GenericScalar<T>(data.Select(v=>-v).ToArray(),Dims);
    }
    public MValue Binary(string op,MValue other,bool reversed=false)
    {
        var operand=Read(other);var a=reversed?operand:this;var b=reversed?this:operand;
        if(op=="*" && !a.IsScalar && !b.IsScalar)
        {
            if(a.Cols!=b.Rows)throw new MatException("MATLAB:innerdim","Inner dimensions must agree.");
            var product=new T[checked(a.Rows*b.Cols)];
            for(int j=0;j<b.Cols;j++)for(int i=0;i<a.Rows;i++)
            { var sum=T.Zero;for(int k=0;k<a.Cols;k++)sum+=a.data[k*a.Rows+i]*b.data[j*b.Rows+k];product[j*a.Rows+i]=sum; }
            return new GenericScalar<T>(product,new[]{a.Rows,b.Cols});
        }
        if(op=="/" && !b.IsScalar || op=="\\" && (!a.IsScalar || !b.IsScalar))
            throw new MatException("MathemaSharp:typed:solve","Typed matrix division/solve is not implemented; use elementwise division or shared-kernel adapters.");
        var dims=Ops.BroadcastDims(a.Dims,b.Dims);var values=new T[checked(dims[0]*dims[1])];
        bool compare=op is "==" or "~=" or "<" or ">" or "<=" or ">=";
        var logical=compare?new double[values.Length]:null;
        for(int j=0;j<dims[1];j++)for(int i=0;i<dims[0];i++)
        {
            var x=a.data[(a.Cols==1?0:j)*a.Rows+(a.Rows==1?0:i)];
            var y=b.data[(b.Cols==1?0:j)*b.Rows+(b.Rows==1?0:i)];int index=j*dims[0]+i;
            if(compare)logical![index]=(op switch {"=="=>x==y,"~="=>x!=y,"<"=>x<y,">"=>x>y,"<="=>x<=y,">="=>x>=y,_=>false})?1:0;
            else values[index]=op switch {"+"=>x+y,"-"=>x-y,"*" or ".*"=>x*y,"/" or "./"=>x/y,"\\" or ".\\"=>y/x,_=>throw new MatException("MathemaSharp:typed:operator","Unsupported binary operator "+op)};
        }
        return compare?new Mat(MClass.Logical,dims,logical!):new GenericScalar<T>(values,dims);
    }
    public MValue Transpose()
    {
        var result=new T[data.Length];for(int j=0;j<Cols;j++)for(int i=0;i<Rows;i++)result[i*Cols+j]=data[j*Rows+i];
        return new GenericScalar<T>(result,new[]{Cols,Rows});
    }
    public MValue Cat(int axis,IList<MValue> values)
    {
        if(axis is not (0 or 1))throw new MatException("MathemaSharp:typed:cat","Only 2D concatenation is supported.");
        var arrays=values.Select(Read).ToArray();if(arrays.Length==0)return new GenericScalar<T>(Array.Empty<T>(),new[]{0,0});
        int rows=axis==0?arrays.Sum(v=>v.Rows):arrays[0].Rows,cols=axis==1?arrays.Sum(v=>v.Cols):arrays[0].Cols;
        if(arrays.Any(v=>axis==0?v.Cols!=cols:v.Rows!=rows))throw new MatException("MATLAB:catenate:dimensionMismatch","Concatenation dimensions differ.");
        var result=new T[checked(rows*cols)];int shift=0;
        foreach(var array in arrays)
        {
            for(int j=0;j<array.Cols;j++)for(int i=0;i<array.Rows;i++)result[(j+(axis==1?shift:0))*rows+i+(axis==0?shift:0)]=array.data[j*array.Rows+i];
            shift+=axis==0?array.Rows:array.Cols;
        }
        return new GenericScalar<T>(result,new[]{rows,cols});
    }
    public override MValue Clone()=>new GenericScalar<T>(data,Dims);
    public override MValue Gather(int[] offsets,int[] dimensions)
    {
        var result=new T[offsets.Length];for(int i=0;i<offsets.Length;i++)result[i]=data[offsets[i]];
        return new GenericScalar<T>(result,dimensions);
    }
    public override MValue Reshape(int[] dimensions)=>new GenericScalar<T>(data,dimensions);
    public override MValue Resize(int[] dimensions)
    {
        if(dimensions.Length!=2 || dimensions[0]<Rows || dimensions[1]<Cols)throw new MatException("MathemaSharp:typed:resize","Expected growing 2D dimensions.");
        var result=new T[checked(dimensions[0]*dimensions[1])];
        for(int j=0;j<Cols;j++)Array.Copy(data,j*Rows,result,j*dimensions[0],Rows);
        return new GenericScalar<T>(result,dimensions);
    }
    public override void SetFrom(int[] offsets,MValue rhs)
    {
        var other=Read(rhs);
        if(!other.IsScalar && other.Numel!=offsets.Length)throw new MatException("MATLAB:subsassigndimmismatch","Assignment dimensions differ.");
        if(offsets.Any(i=>(uint)i>=(uint)data.Length))throw new MatException("MATLAB:badsubscript","Index out of bounds.");
        var source=other.ToArray();for(int i=0;i<offsets.Length;i++)data[offsets[i]]=source[other.IsScalar?0:i];
    }
}
""");
var path=Path.Combine(root,"Runtime","Ops.cs");var text=File.ReadAllText(path).Replace("\r\n","\n");
foreach(var pair in new[]{("public static MValue Transpose(MValue v, bool conj)","if(v is IGenericScalar typed)return typed.Transpose();"),("public static MValue Cat(int dim, IList<MValue> items)","if(items.FirstOrDefault(v=>v is IGenericScalar) is IGenericScalar typed)return typed.Cat(dim,items);")})
{ var marker=pair.Item1+"\n    {";if(!text.Contains(marker))throw new Exception("Missing "+marker);text=text.Replace(marker,marker+"\n        "+pair.Item2); }
File.WriteAllText(path,text);Console.WriteLine("Typed real matrix storage added.");
