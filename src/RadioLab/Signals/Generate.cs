using System.Numerics;

namespace RadioLab.Signals;

/// <summary>Makes test signals: the sine waves and rotating phasors of Book 1, as arrays of samples.</summary>
public static class Generate
{
    /// <summary>The time of each sample, in seconds: n / sampleRate.</summary>
    public static double[] Time(double sampleRate, int count)
    {
        var t = new double[count];
        for (int n = 0; n < count; n++) t[n] = n / sampleRate;
        return t;
    }

    /// <summary>A·sin(2πft + φ), sampled <paramref name="count"/> times at <paramref name="sampleRate"/>.</summary>
    public static double[] Sine(double frequency, double sampleRate, int count, double amplitude = 1, double phase = 0)
    {
        var x = new double[count];
        for (int n = 0; n < count; n++)
            x[n] = amplitude * Math.Sin(2 * Math.PI * frequency * n / sampleRate + phase);
        return x;
    }

    /// <summary>A·cos(2πft + φ): the same wave as <see cref="Sine"/>, a quarter turn ahead.</summary>
    public static double[] Cosine(double frequency, double sampleRate, int count, double amplitude = 1, double phase = 0) =>
        Sine(frequency, sampleRate, count, amplitude, phase + Math.PI / 2);

    /// <summary>
    /// A·e^(j(2πft + φ)): an arrow of length A turning f times per second. Its real part is the cosine,
    /// its imaginary part the sine. A negative frequency turns clockwise.
    /// </summary>
    public static Complex[] Phasor(double frequency, double sampleRate, int count, double amplitude = 1, double phase = 0)
    {
        var x = new Complex[count];
        for (int n = 0; n < count; n++)
            x[n] = Complex.FromPolarCoordinates(amplitude, 2 * Math.PI * frequency * n / sampleRate + phase);
        return x;
    }

    /// <summary>
    /// A tone whose frequency glides from <paramref name="startHz"/> to <paramref name="endHz"/>.
    /// The phase is accumulated sample by sample, so the wave never jumps.
    /// </summary>
    public static double[] Chirp(double startHz, double endHz, double sampleRate, int count, double amplitude = 1)
    {
        var x = new double[count];
        double phase = 0;
        for (int n = 0; n < count; n++)
        {
            double f = startHz + (endHz - startHz) * n / count;
            x[n] = amplitude * Math.Sin(phase);
            phase += 2 * Math.PI * f / sampleRate;
        }
        return x;
    }

    /// <summary>Adds signals sample by sample: superposition.</summary>
    public static double[] Add(params double[][] signals)
    {
        var sum = new double[signals[0].Length];
        foreach (var s in signals)
            for (int n = 0; n < sum.Length; n++) sum[n] += s[n];
        return sum;
    }

    /// <summary>Multiplies two signals sample by sample: the mixer.</summary>
    public static double[] Multiply(double[] a, double[] b)
    {
        var product = new double[a.Length];
        for (int n = 0; n < a.Length; n++) product[n] = a[n] * b[n];
        return product;
    }

    /// <summary>Gaussian noise with the given RMS value, from a seeded generator so results repeat.</summary>
    public static double[] Noise(int count, double rms, int seed = 1)
    {
        var random = new Random(seed);
        var x = new double[count];
        for (int n = 0; n < count; n++)
        {
            // Box–Muller: two uniform numbers become one Gaussian number.
            double u1 = 1.0 - random.NextDouble(), u2 = random.NextDouble();
            x[n] = rms * Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
        }
        return x;
    }

    /// <summary>Complex Gaussian noise: independent noise on I and on Q, with the given total RMS.</summary>
    public static Complex[] ComplexNoise(int count, double rms, int seed = 1)
    {
        double perPart = rms / Math.Sqrt(2);
        var i = Noise(count, perPart, seed);
        var q = Noise(count, perPart, seed + 7919);
        var x = new Complex[count];
        for (int n = 0; n < count; n++) x[n] = new Complex(i[n], q[n]);
        return x;
    }
}
