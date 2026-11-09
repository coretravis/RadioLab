using System.Numerics;
using RadioLab.Filters;

namespace RadioLab.Rates;

/// <summary>Changing the sample rate without aliasing.</summary>
public static class Resample
{
    /// <summary>
    /// Lowers the sample rate by a whole factor: first a low-pass filter removes everything that would
    /// not fit below the new Nyquist frequency, then only every <paramref name="factor"/>th sample is kept.
    /// The filter's cutoff sits a little below the new Nyquist, to leave room for its slope.
    /// </summary>
    public static Complex[] Decimate(IReadOnlyList<Complex> input, int factor, double sampleRate, int taps = 101)
    {
        double newRate = sampleRate / factor;
        var filter = new FirFilter(FilterDesign.LowPass(0.4 * newRate, sampleRate, taps));
        var output = new Complex[input.Count / factor];
        for (int n = 0, kept = 0; n < input.Count; n++)
        {
            Complex y = filter.Next(input[n]);
            if (n % factor == factor - 1 && kept < output.Length) output[kept++] = y;
        }
        return output;
    }

    /// <summary>Decimates a real signal.</summary>
    public static double[] Decimate(IReadOnlyList<double> input, int factor, double sampleRate, int taps = 101)
    {
        var asComplex = input.Select(v => new Complex(v, 0)).ToArray();
        return Decimate(asComplex, factor, sampleRate, taps).Select(c => c.Real).ToArray();
    }

    /// <summary>
    /// Raises the sample rate by a whole factor: insert zeros between the samples, then low-pass filter
    /// to smooth away the copies the zeros create. The filter's output is multiplied by the factor to
    /// make up for the energy the zeros diluted.
    /// </summary>
    public static Complex[] Interpolate(IReadOnlyList<Complex> input, int factor, double sampleRate, int taps = 101)
    {
        double newRate = sampleRate * factor;
        var filter = new FirFilter(FilterDesign.LowPass(0.4 * sampleRate, newRate, taps));
        var output = new Complex[input.Count * factor];
        for (int n = 0; n < input.Count; n++)
            for (int k = 0; k < factor; k++)
                output[n * factor + k] = factor * filter.Next(k == 0 ? input[n] : Complex.Zero);
        return output;
    }
}
