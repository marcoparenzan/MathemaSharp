// Common settings of the C# kernel (≥ 0.3.0, which already captures Console.Out and resolves relative paths).
// Each cell may run on a different thread: the invariant culture must also be set as the threads' default,
// otherwise with an Italian culture numbers would come out as "0,2".
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
