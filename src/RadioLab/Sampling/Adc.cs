namespace RadioLab.Sampling;

/// <summary>
/// A simulated analogue-to-digital converter: it rounds each voltage to one of 2^bits levels between
/// −fullScale and +fullScale, and clips anything outside.
/// </summary>
public sealed class Adc
{
    public int Bits { get; }
    public double FullScale { get; }

    /// <summary>The voltage between neighbouring levels: the converter's resolution.</summary>
    public double Step { get; }

    public int MinCode => -(1 << (Bits - 1));
    public int MaxCode => (1 << (Bits - 1)) - 1;

    public Adc(int bits, double fullScale = 1.0)
    {
        if (bits is < 1 or > 24) throw new ArgumentOutOfRangeException(nameof(bits));
        Bits = bits;
        FullScale = fullScale;
        Step = 2 * fullScale / (1 << bits);
    }

    /// <summary>The integer code the converter outputs for one voltage.</summary>
    public int ToCode(double voltage) => (int)Math.Clamp(Math.Round(voltage / Step), MinCode, MaxCode);

    /// <summary>The voltage a code stands for.</summary>
    public double ToVoltage(int code) => code * Step;

    /// <summary>Converts a whole signal and back: what the software sees of the real voltage.</summary>
    public double[] Digitise(IReadOnlyList<double> signal)
    {
        var x = new double[signal.Count];
        for (int n = 0; n < x.Length; n++) x[n] = ToVoltage(ToCode(signal[n]));
        return x;
    }

    /// <summary>
    /// The best signal-to-noise ratio a full-scale sine wave can have after this converter, in dB:
    /// 6.02·bits + 1.76. Each extra bit halves the step, and halving the error buys about 6 dB.
    /// </summary>
    public double IdealSnrDb => 6.02 * Bits + 1.76;
}
