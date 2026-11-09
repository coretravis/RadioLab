using System.Numerics;

namespace RadioLab.Baseband;

/// <summary>Moves signals up and down in frequency by complex multiplication.</summary>
public static class Mixer
{
    /// <summary>
    /// Shifts every component of <paramref name="samples"/> by <paramref name="shiftHz"/>: positive moves
    /// up, negative moves down. Each sample is multiplied by a unit phasor turning at the shift frequency,
    /// and because angles add, every frequency in the signal moves by the same amount.
    /// </summary>
    public static Complex[] Shift(IReadOnlyList<Complex> samples, double shiftHz, double sampleRate)
    {
        var output = new Complex[samples.Count];
        double radiansPerSample = 2 * Math.PI * shiftHz / sampleRate;
        double angle = 0;
        for (int n = 0; n < output.Length; n++)
        {
            output[n] = samples[n] * Complex.FromPolarCoordinates(1.0, angle);
            angle += radiansPerSample;
            if (angle > Math.PI) angle -= 2 * Math.PI;   // keep the angle small, so rounding stays tiny
            else if (angle < -Math.PI) angle += 2 * Math.PI;
        }
        return output;
    }

    /// <summary>
    /// Turns a real signal into complex baseband around <paramref name="centreHz"/>: multiply by cos for I
    /// and by −sin for Q, which is the same as multiplying by e^(−j2πf₀t). The result still contains a
    /// copy at −2f₀ that a low-pass filter must remove.
    /// </summary>
    public static Complex[] Downconvert(IReadOnlyList<double> real, double centreHz, double sampleRate)
    {
        var asComplex = new Complex[real.Count];
        for (int n = 0; n < real.Count; n++) asComplex[n] = new Complex(real[n], 0);
        return Shift(asComplex, -centreHz, sampleRate);
    }
}
