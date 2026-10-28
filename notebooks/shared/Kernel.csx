// Impostazioni comuni del kernel C# di JupyterNet (≥ 0.3.0, che cattura già Console.Out e risolve i percorsi relativi).
// Ogni cella può girare su un thread diverso: la cultura invariante va impostata anche come default dei thread,
// altrimenti con cultura italiana i numeri uscirebbero come "0,2".
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
