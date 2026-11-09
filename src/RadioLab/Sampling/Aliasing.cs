namespace RadioLab.Sampling;

/// <summary>What frequency a tone appears to have once it has been sampled.</summary>
public static class Aliasing
{
    /// <summary>The Nyquist frequency: half the sample rate, the highest frequency the samples can represent.</summary>
    public static double Nyquist(double sampleRate) => sampleRate / 2;

    /// <summary>
    /// The frequency, between 0 and Nyquist, that a real tone of <paramref name="frequency"/> appears to have
    /// after sampling. Sampling cannot tell f apart from f ± k·fs, and a real signal cannot tell f from −f,
    /// so the frequency folds back and forth between 0 and fs/2.
    /// </summary>
    public static double ApparentFrequency(double frequency, double sampleRate)
    {
        double f = Math.Abs(frequency) % sampleRate;     // shift by whole multiples of fs
        return f > sampleRate / 2 ? sampleRate - f : f;  // fold the upper half back down
    }
}
