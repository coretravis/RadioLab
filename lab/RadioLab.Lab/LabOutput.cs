using System.Globalization;
using System.Text;
using RadioLab.Output;

namespace RadioLab.Lab;

/// <summary>Where an experiment writes its results: data/ for CSV, audio/ for WAV, recordings/ for I/Q.</summary>
public sealed class LabOutput
{
    public string Root { get; }
    public string Data => Path.Combine(Root, "data");
    public string Audio => Path.Combine(Root, "audio");
    public string Recordings => Path.Combine(Root, "recordings");

    public LabOutput(string root)
    {
        Root = root;
        Directory.CreateDirectory(Data);
        Directory.CreateDirectory(Audio);
        Directory.CreateDirectory(Recordings);
    }

    public void Csv(string name, IReadOnlyList<string> headers, params IReadOnlyList<double>[] columns)
    {
        Output.Csv.Write(Path.Combine(Data, name), headers, columns);
        Console.WriteLine($"  data/{name}");
    }

    /// <summary>
    /// A CSV in "long" form, one row per point with a series label: how a chart draws several lines
    /// on the same axes.
    /// </summary>
    public void SeriesCsv(string name, string x, string y, IEnumerable<(string Series, IReadOnlyList<double> Xs, IReadOnlyList<double> Ys)> series)
    {
        var text = new StringBuilder($"{x},{y},series\n");
        foreach (var (label, xs, ys) in series)
            for (int i = 0; i < xs.Count; i++)
                text.Append(Format(xs[i])).Append(',').Append(Format(ys[i])).Append(',').Append(label).Append('\n');
        File.WriteAllText(Path.Combine(Data, name), text.ToString());
        Console.WriteLine($"  data/{name}");
    }

    public void Wav(string name, IReadOnlyList<double> samples, int sampleRate)
    {
        Output.Wav.WriteMono16(Path.Combine(Audio, name), samples, sampleRate);
        Console.WriteLine($"  audio/{name} ({samples.Count / (double)sampleRate:0.0} s)");
    }

    private static string Format(double v) => Math.Round(v, 6).ToString(CultureInfo.InvariantCulture);
}
