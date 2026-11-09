using System.Numerics;

namespace RadioLab.Demodulation;

/// <summary>
/// Amplitude modulation carries the message in the length of the arrow. With complex samples the
/// envelope is simply √(I² + Q²), however the carrier's phase drifts: no diode, no tuning precision.
/// </summary>
public static class AmDemodulator
{
    /// <summary>
    /// The envelope of a station already shifted to 0 Hz, with its average (the steady carrier) removed,
    /// leaving only the sound.
    /// </summary>
    public static double[] Demodulate(IReadOnlyList<Complex> baseband)
    {
        var envelope = new double[baseband.Count];
        for (int n = 0; n < envelope.Length; n++) envelope[n] = baseband[n].Magnitude;
        double carrier = envelope.Average();
        for (int n = 0; n < envelope.Length; n++) envelope[n] -= carrier;
        return envelope;
    }
}
