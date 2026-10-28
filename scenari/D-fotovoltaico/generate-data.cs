#!/usr/bin/env dotnet
#:property TargetFramework=net11.0
#:property Nullable=enable
#:property ImplicitUsings=enable

// Generatore (deterministico, con seme) dei dati sintetici dello scenario D: un parco fotovoltaico da 1 MWp (inverter da 600 kW AC (limita la potenza nelle ore di sole pieno)) a 45°N.
//   dotnet run scenari/D-fotovoltaico/generate-data.cs
// Scrive in data/:
//   impianto_orario.csv  56 giorni orari: irraggiamento misurato, irraggiamento PREVISTO il giorno prima (con errore), temperature,
//                        potenza AC, stato dell'inverter
//   verita.json          ciò che il generatore SA: giorno della pulizia, sporcamento, guasto dell'inverter
// Nessun dato è una misura di un impianto reale.
using System.Globalization;
using System.Text;

var inv = CultureInfo.InvariantCulture;
var here = args.Length > 0 ? args[0] : AppContext.GetData("EntryPointFileDirectoryPath") as string ?? Directory.GetCurrentDirectory();
var dataDir = Path.Combine(here, "data");
Directory.CreateDirectory(dataDir);

const int days = 56;
const int hours = days * 24;
const double nominalKwp = 1000, inverterMaxKw = 600, soilingPerDay = 0.0009;
const int cleaningDay = 30, faultDay = 38, faultFromHour = 10, faultToHour = 14;      // guasto: 10:00–14:59
var start = new DateTime(2026, 7, 27, 0, 0, 0);                                      // lunedì
var rng = new Random(20260727);
double Normal() { var u1 = 1 - rng.NextDouble(); var u2 = rng.NextDouble(); return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2); }

// Tipo di giornata: chiara (45 %), variabile (35 %), coperta (20 %). Il servizio meteo conosce il tipo con un errore di giornata (σ = 0.12).
var dayClearness = new double[days];
var dayForecastClearness = new double[days];
for (var d = 0; d < days; d++)
{
    var u = rng.NextDouble();
    dayClearness[d] = u < 0.45 ? 0.97 : u < 0.80 ? 0.75 : 0.35;
    dayForecastClearness[d] = Math.Clamp(dayClearness[d] + 0.12 * Normal(), 0.1, 1.0);
}

double ar = 0, arForecast = 0;
var csv = new StringBuilder("timestamp,irraggiamento_w_m2,irraggiamento_previsto_w_m2,temp_aria_c,temp_modulo_c,potenza_ac_kw,stato_inverter\n");
var faultHours = 0;
for (var h = 0; h < hours; h++)
{
    int day = h / 24, hour = h % 24;
    var dayOfYear = start.AddDays(day).DayOfYear;
    var declination = 23.44 * Math.PI / 180 * Math.Sin(2 * Math.PI * (dayOfYear - 81) / 365.0);
    var latitude = 45 * Math.PI / 180;
    var hourAngle = 15 * Math.PI / 180 * (hour + 0.5 - 13.5);                         // mezzogiorno solare alle 13:30 (ora legale)
    var sinElevation = Math.Sin(latitude) * Math.Sin(declination) + Math.Cos(latitude) * Math.Cos(declination) * Math.Cos(hourAngle);
    var clearSky = sinElevation > 0 ? 1000 * Math.Pow(sinElevation, 1.15) : 0;

    ar = 0.7 * ar + 0.8 * Normal();
    arForecast = 0.7 * arForecast + 0.8 * Normal();
    var irradiance = clearSky * Math.Clamp(dayClearness[day] + 0.10 * ar, 0.08, 1.0);
    var irradianceForecast = clearSky * Math.Clamp(dayForecastClearness[day] + 0.07 * arForecast, 0.08, 1.0);

    var airTemperature = 22 + 7 * Math.Sin(2 * Math.PI * (hour - 9) / 24.0) - 0.04 * day + 0.8 * Normal();
    var moduleTemperature = airTemperature + 0.03 * irradiance;
    var soiling = soilingPerDay * (day < cleaningDay ? day : day - cleaningDay);       // la pioggia del giorno 30 pulisce i moduli

    var fault = day == faultDay && hour >= faultFromHour && hour <= faultToHour;
    if (fault) faultHours++;
    var dc = nominalKwp * (irradiance / 1000) * 0.88 * (1 - 0.004 * (moduleTemperature - 25)) * (1 - soiling) + (irradiance > 50 ? 4 * Normal() : 0);
    var ac = fault || irradiance <= 0 ? 0 : Math.Clamp(0.97 * dc, 0, inverterMaxKw);

    csv.Append(start.AddHours(h).ToString("yyyy-MM-ddTHH:mm:ss", inv)).Append(',')
       .Append(irradiance.ToString("0.0", inv)).Append(',').Append(irradianceForecast.ToString("0.0", inv)).Append(',')
       .Append(airTemperature.ToString("0.00", inv)).Append(',').Append(moduleTemperature.ToString("0.00", inv)).Append(',')
       .Append(ac.ToString("0.0", inv)).Append(',').Append(fault ? 0 : 1).Append('\n');
}
File.WriteAllText(Path.Combine(dataDir, "impianto_orario.csv"), csv.ToString());

var truth =
    "{\n" +
    $"  \"potenza_nominale_kwp\": {nominalKwp.ToString(inv)},\n" +
    $"  \"potenza_ac_max_kw\": {inverterMaxKw.ToString(inv)},\n" +
    $"  \"giorno_pulizia\": {cleaningDay},\n" +
    $"  \"data_pulizia\": \"{start.AddDays(cleaningDay).ToString("yyyy-MM-dd", inv)}\",\n" +
    $"  \"sporcamento_per_giorno\": {soilingPerDay.ToString(inv)},\n" +
    $"  \"giorno_guasto\": {faultDay},\n" +
    $"  \"data_guasto\": \"{start.AddDays(faultDay).ToString("yyyy-MM-dd", inv)}\",\n" +
    $"  \"ore_guasto\": {faultHours}\n" +
    "}\n";
File.WriteAllText(Path.Combine(dataDir, "verita.json"), truth);
Console.WriteLine($"impianto_orario.csv: {hours} righe; pulizia il giorno {cleaningDay}, guasto il giorno {faultDay} ({faultHours} ore)");
