using System.Numerics;

namespace RadioLab.Filters;

/// <summary>Ways to choose a filter's taps.</summary>
public static class FilterDesign
{
    /// <summary>The moving average: N equal taps of 1/N. Simple, and a poor low-pass filter.</summary>
    public static double[] MovingAverage(int length) => Enumerable.Repeat(1.0 / length, length).ToArray();

    /// <summary>
    /// A low-pass filter by the windowed-sinc method. The ideal low-pass filter's taps are a sinc
    /// function, which goes on for ever; cutting it to <paramref name="length"/> taps and tapering the
    /// ends with a Hann-shaped window gives a practical filter. The taps are scaled to add up to 1, so
    /// 0 Hz passes unchanged. An odd length keeps the filter symmetric around its centre tap.
    /// </summary>
    public static double[] LowPass(double cutoffHz, double sampleRate, int length)
    {
        if (length % 2 == 0) throw new ArgumentException("Use an odd number of taps.");
        double fc = cutoffHz / sampleRate;   // cutoff as a fraction of the sample rate
        int middle = length / 2;
        var taps = new double[length];
        for (int i = 0; i < length; i++)
        {
            int m = i - middle;
            double sinc = m == 0 ? 2 * fc : Math.Sin(2 * Math.PI * fc * m) / (Math.PI * m);
            double window = 0.5 - 0.5 * Math.Cos(2 * Math.PI * (i + 1) / (length + 1));
            taps[i] = sinc * window;
        }
        double sum = taps.Sum();
        for (int i = 0; i < length; i++) taps[i] /= sum;
        return taps;
    }

    /// <summary>
    /// The filter's gain at one frequency, in dB: feed it a steady phasor and see what comes out,
    /// computed directly as the sum of taps × e^(−j2πfk/fs).
    /// </summary>
    public static double GainDb(IReadOnlyList<double> taps, double frequencyHz, double sampleRate)
    {
        Complex response = Complex.Zero;
        for (int k = 0; k < taps.Count; k++)
            response += taps[k] * Complex.FromPolarCoordinates(1.0, -2 * Math.PI * frequencyHz * k / sampleRate);
        return 20 * Math.Log10(Math.Max(response.Magnitude, 1e-12));
    }
}
