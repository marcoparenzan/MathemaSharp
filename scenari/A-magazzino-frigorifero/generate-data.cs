#!/usr/bin/env dotnet
#:property TargetFramework=net11.0
#:property Nullable=enable
#:property ImplicitUsings=enable

// Generator (deterministic, seeded) of the synthetic data of scenario A: a cold store at +2 °C.
//   dotnet run scenari/A-magazzino-frigorifero/generate-data.cs
// Scrive in data/:
//   cella_1min.csv     72 h per minute: chamber temperature (with communication gaps), setpoint, door, defrost,
//                      compressor current, suction pressure, vibration
//   energia_oraria.csv 6 settimane orarie: kWh e temperatura esterna
//   verita.json        what the generator KNOWS and a real historian does not say: true minutes above +4 °C, when the vibration changes, the gaps
// No datum is a measurement of a real plant.
using System.Globalization;
using System.Text;

var inv = CultureInfo.InvariantCulture;
// Script folder: exposed by the runtime to file-based apps; alternatively it is passed as the first argument (dotnet run generate-data.cs -- <folder>).
var here = args.Length > 0 ? args[0] : AppContext.GetData("EntryPointFileDirectoryPath") as string ?? Directory.GetCurrentDirectory();
var dataDir = Path.Combine(here, "data");
Directory.CreateDirectory(dataDir);

// ------------------------------------------------------------------------------------------------------------------- cella al minuto
const int minutes = 3 * 24 * 60;
var start = new DateTime(2026, 9, 14, 0, 0, 0);               // Monday
var rng = new Random(20260914);
double Normal() { var u1 = 1 - rng.NextDouble(); var u2 = rng.NextDouble(); return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2); }

// Door open: ordinary events (3–6 min, +0.06 °C/min) during the loading hours 8–18, plus ONE long event: Tuesday 14:00, 22 minutes, +0.22 °C/min (a door left open).
var doorRng = new Random(1);
var doorExtra = new double[minutes];
var doorOpen = new bool[minutes];
void AddDoor(int from, int duration, double extra)
{
    for (var m = from; m < Math.Min(minutes, from + duration); m++) { doorExtra[m] += extra; doorOpen[m] = true; }
}
for (var day = 0; day < 3; day++)
    for (var minuteOfDay = 8 * 60; minuteOfDay < 18 * 60; minuteOfDay += 35)
        if (doorRng.NextDouble() < 0.6) AddDoor(day * 1440 + minuteOfDay + doorRng.Next(0, 30), doorRng.Next(3, 7), 0.06);
const int longDoorStart = 1440 + 14 * 60, longDoorMinutes = 22;
AddDoor(longDoorStart, longDoorMinutes, 0.22);

// Defrost: 03:00, 11:00, 19:00 for 25 minutes: compressor off, heater (+0.02 °C/min).
bool Defrost(int m) { var d = m % 1440; return (d >= 180 && d < 205) || (d >= 660 && d < 685) || (d >= 1140 && d < 1165); }

// Gateway communication gaps (start minute, duration). One falls right on the rise of the long excursion.
(int From, int Duration)[] gaps = [(5 * 60 + 10, 8), (16 * 60 + 40, 15), (1440 + 14 * 60 + 8, 12), (1440 + 22 * 60, 6), (2 * 1440 + 9 * 60 + 30, 20), (2 * 1440 + 18 * 60 + 5, 9)];
var missing = new bool[minutes];
foreach (var (from, duration) in gaps) for (var m = from; m < from + duration; m++) missing[m] = true;

// Vibration: level per state (on 2.6 / off 0.4 mm/s) + GARCH(1,1) shock. In the last 24 h ω triples: a bearing failing.
const int vibrationChange = 2 * 1440;
(double Omega, double Alpha, double Beta) Baseline = (0.002, 0.10, 0.80), Worn = (0.006, 0.10, 0.80);

double trueT = 2.0, sigma2 = Baseline.Omega / (1 - Baseline.Alpha - Baseline.Beta), previousShock = 0;
var on = false;
var trueTemperature = new double[minutes];
var csv = new StringBuilder("timestamp,temp_cella_c,setpoint_c,porta_aperta,sbrinamento,corrente_compressore_a,pressione_aspirazione_bar,vibrazione_mm_s\n");
for (var m = 0; m < minutes; m++)
{
    var defrost = Defrost(m);
    if (defrost) on = false; else if (trueT > 2.5) on = true; else if (trueT < 1.5) on = false;
    trueT += 0.03 + (defrost ? 0.02 : 0) + doorExtra[m] - (on ? 0.12 : 0) + 0.01 * Normal();
    trueTemperature[m] = trueT;

    var parameters = m < vibrationChange ? Baseline : Worn;
    sigma2 = parameters.Omega + parameters.Alpha * previousShock * previousShock + parameters.Beta * sigma2;
    previousShock = Math.Sqrt(sigma2) * Normal();
    var vibration = Math.Max(0.05, (on ? 2.6 : 0.4) + previousShock);

    var measured = Math.Round(trueT + 0.08 * Normal(), 2);
    var current = on ? 17.5 + 0.4 * Normal() : 1.8 + 0.15 * Normal();
    var pressure = on ? 1.6 + 0.05 * Normal() : 2.4 + 0.05 * Normal();
    csv.Append(start.AddMinutes(m).ToString("yyyy-MM-ddTHH:mm:ss", inv)).Append(',')
       .Append(missing[m] ? "" : measured.ToString("0.00", inv)).Append(",2.0,")
       .Append(doorOpen[m] ? 1 : 0).Append(',').Append(defrost ? 1 : 0).Append(',')
       .Append(current.ToString("0.00", inv)).Append(',').Append(pressure.ToString("0.000", inv)).Append(',').Append(vibration.ToString("0.000", inv)).Append('\n');
}
File.WriteAllText(Path.Combine(dataDir, "cella_1min.csv"), csv.ToString());

// ------------------------------------------------------------------------------------------------------------------- energia oraria
const int hours = 6 * 7 * 24;
var energyStart = new DateTime(2026, 8, 3, 0, 0, 0);          // Monday: ends on Sunday 13 September, one day before the chamber
var erng = new Random(20260803);
double ENormal() { var u1 = 1 - erng.NextDouble(); var u2 = erng.NextDouble(); return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2); }
var weather = 0.0;
var energy = new StringBuilder("timestamp,kwh,temp_esterna_c\n");
for (var h = 0; h < hours; h++)
{
    var time = energyStart.AddHours(h);
    var hourOfDay = time.Hour;
    weather = 0.9 * weather + 1.2 * ENormal();                // meteo lento (AR(1))
    var outside = 22 - 0.05 * h / 24.0 + 6 * Math.Sin(2 * Math.PI * (hourOfDay - 9) / 24.0) + weather;
    var operations = time.DayOfWeek switch
    {
        DayOfWeek.Sunday => 0.0,
        DayOfWeek.Saturday => hourOfDay is >= 8 and < 13 ? 6.0 : 0.0,
        _ => hourOfDay is >= 8 and < 18 ? 12.0 : 0.0,
    };
    var defrostHeat = hourOfDay is 3 or 11 or 19 ? 6.0 : 0.0;
    var kwh = 38 + 0.0006 * h + 0.9 * (outside - 20) + operations + defrostHeat + 1.5 * ENormal();
    energy.Append(time.ToString("yyyy-MM-ddTHH:mm:ss", inv)).Append(',').Append(kwh.ToString("0.00", inv)).Append(',').Append(outside.ToString("0.00", inv)).Append('\n');
}
File.WriteAllText(Path.Combine(dataDir, "energia_oraria.csv"), energy.ToString());

// ------------------------------------------------------------------------------------------------------------------- truth
var above = Enumerable.Range(0, minutes).Where(m => trueTemperature[m] > 4.0).ToArray();
var excursions = above.Where((m, i) => i == 0 || m != above[i - 1] + 1).ToArray();
var truth =
    "{\n" +
    $"  \"soglia_haccp_c\": 4.0,\n" +
    $"  \"minuti_veri_sopra_soglia\": {above.Length},\n" +
    $"  \"escursioni_vere\": {excursions.Length},\n" +
    $"  \"inizio_prima_escursione\": \"{(above.Length > 0 ? start.AddMinutes(above[0]).ToString("yyyy-MM-ddTHH:mm:ss", inv) : "")}\",\n" +
    $"  \"temperatura_massima_vera_c\": {trueTemperature.Max().ToString("0.00", inv)},\n" +
    $"  \"minuti_mancanti\": {missing.Count(x => x)},\n" +
    $"  \"cambio_vibrazione\": \"{start.AddMinutes(vibrationChange).ToString("yyyy-MM-ddTHH:mm:ss", inv)}\",\n" +
    $"  \"vibrazione_omega_base\": {Baseline.Omega.ToString(inv)},\n" +
    $"  \"vibrazione_omega_usura\": {Worn.Omega.ToString(inv)}\n" +
    "}\n";
File.WriteAllText(Path.Combine(dataDir, "verita.json"), truth);

Console.WriteLine($"cella_1min.csv: {minutes} righe; energia_oraria.csv: {hours} righe");
Console.WriteLine($"truth: {above.Length} true minutes above +4 °C in {excursions.Length} excursion(s), maximum {trueTemperature.Max():F2} °C, {missing.Count(x => x)} minutes missing");
