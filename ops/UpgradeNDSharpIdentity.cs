#:property TargetFramework=net11.0
var root=@"D:\Dev\Science\NDSharp";
var path=Path.Combine(root,"src","NDSharp","Reduce.cs");
var text=File.ReadAllText(path);
var old="=> identity is null ? T.Zero : T.CreateTruncating(Convert.ToDouble(identity is bool b ? (b ? 1 : 0) : identity));";
var replacement="""
=> identity switch
        {
            null => T.Zero,
            T value => value,
            bool value => value ? T.One : T.Zero,
            sbyte value => T.CreateTruncating(value),
            byte value => T.CreateTruncating(value),
            short value => T.CreateTruncating(value),
            ushort value => T.CreateTruncating(value),
            int value => T.CreateTruncating(value),
            uint value => T.CreateTruncating(value),
            long value => T.CreateTruncating(value),
            ulong value => T.CreateTruncating(value),
            Half value => T.CreateTruncating(value),
            float value => T.CreateTruncating(value),
            double value => T.CreateTruncating(value),
            decimal value => T.CreateTruncating(value),
            _ => throw new NDNotSupportedException($"Unsupported reduction identity type {identity.GetType()}")
        };
""";
if(!text.Contains(old))throw new Exception("Expected identity implementation not found.");
File.WriteAllText(path,text.Replace(old,replacement));
File.WriteAllText(Path.Combine(root,"src","NDSharp.Tests","GenericIdentityTests.cs"),"""
using System.Reflection;
using NDSharp;
using Xunit;
public class GenericIdentityTests
{
    private static T Identity<T>(object value) => (T)typeof(NDArray).Assembly
        .GetType("NDSharp.Red")!.GetMethod("IdentityOf", BindingFlags.NonPublic | BindingFlags.Static)!
        .MakeGenericMethod(typeof(T)).Invoke(null, [value])!;
    [Fact] public void Int64IdentityRetainsLowBits() => Assert.Equal(9007199254740993L, Identity<long>(9007199254740993L));
    [Fact] public void UInt64IdentityRetainsMaximum() => Assert.Equal(ulong.MaxValue, Identity<ulong>(ulong.MaxValue));
    [Fact] public void DecimalIdentityDoesNotUseDouble() => Assert.Equal(1.0000000000000000000000001m, Identity<decimal>(1.0000000000000000000000001m));
    [Fact] public void BooleanIdentityUsesGenericConstants() { Assert.Equal(1, Identity<int>(true)); Assert.Equal(0, Identity<int>(false)); }
}
""");
Console.WriteLine("Reduction identity converted; four regression tests added.");
