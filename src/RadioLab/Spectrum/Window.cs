using System.Numerics;

namespace RadioLab.Spectrum;

/// <summary>Window functions: tapers that fade a block of samples in and out before a transform.</summary>
public static class Window
{
    /// <summary>The Hann window: one cycle of a raised cosine, zero at both ends, one in the middle.</summary>
    public static double[] Hann(int n)
    {
        var w = new double[n];
        for (int i = 0; i < n; i++) w[i] = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / n);
        return w;
    }

    /// <summary>The average value of a window: how much it scales a steady tone (0.5 for Hann).</summary>
    public static double Gain(IReadOnlyList<double> window) => window.Average();

    /// <summary>Multiplies a block of samples by a window, sample by sample.</summary>
    public static Complex[] Apply(IReadOnlyList<Complex> x, IReadOnlyList<double> window)
    {
        var y = new Complex[x.Count];
        for (int i = 0; i < y.Length; i++) y[i] = x[i] * window[i];
        return y;
    }
}
