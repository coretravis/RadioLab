using System.Numerics;

namespace RadioLab.Signals;

/// <summary>Measurements on signals: the averages, RMS and decibels of Book 1, computed from samples.</summary>
public static class Measure
{
    public static double Mean(IReadOnlyList<double> x) => x.Average();

    /// <summary>Root mean square: the square root of the average of the squares.</summary>
    public static double Rms(IReadOnlyList<double> x) => Math.Sqrt(x.Average(v => v * v));

    /// <summary>RMS of complex samples: the square root of the average squared length.</summary>
    public static double Rms(IReadOnlyList<Complex> x) => Math.Sqrt(x.Average(v => v.Magnitude * v.Magnitude));

    /// <summary>A power ratio in decibels: 10·log₁₀(ratio).</summary>
    public static double PowerDb(double ratio) => 10 * Math.Log10(ratio);

    /// <summary>An amplitude (voltage) ratio in decibels: 20·log₁₀(ratio).</summary>
    public static double AmplitudeDb(double ratio) => 20 * Math.Log10(ratio);

    /// <summary>
    /// Level relative to a converter's full scale, in dBFS. A full-scale sine wave has an RMS of
    /// fullScale/√2 and reads 0 dBFS; every quieter signal reads negative.
    /// </summary>
    public static double DbFs(IReadOnlyList<double> x, double fullScale = 1.0) =>
        AmplitudeDb(Rms(x) / (fullScale / Math.Sqrt(2)));

    /// <summary>Signal-to-noise ratio in dB, from a clean signal and the noise that was added to it.</summary>
    public static double SnrDb(IReadOnlyList<double> signal, IReadOnlyList<double> noise) =>
        AmplitudeDb(Rms(signal) / Rms(noise));
}
