using System.Numerics;

namespace RadioLab.Demodulation;

/// <summary>
/// Frequency modulation carries the message in how fast the arrow turns. The rate of turning is the
/// change in angle from one sample to the next, which is the angle of x[n]·conj(x[n−1]).
/// </summary>
public sealed class FmDemodulator
{
    private readonly double _sampleRate;
    private Complex _previous = Complex.One;

    public FmDemodulator(double sampleRate) => _sampleRate = sampleRate;

    /// <summary>
    /// The instantaneous frequency of each sample, in Hz. Multiplying by the conjugate of the previous
    /// sample subtracts its angle (angles add, and conjugating negates), and the result's angle is the
    /// turn made in one sample: always between −π and +π, so no unwrapping is needed.
    /// </summary>
    public double[] Process(IReadOnlyList<Complex> baseband)
    {
        var frequency = new double[baseband.Count];
        for (int n = 0; n < frequency.Length; n++)
        {
            double turn = (baseband[n] * Complex.Conjugate(_previous)).Phase;   // radians in one sample
            frequency[n] = turn * _sampleRate / (2 * Math.PI);
            _previous = baseband[n];
        }
        return frequency;
    }

    /// <summary>
    /// Undoes the treble boost FM broadcasters apply before transmitting (pre-emphasis), with a simple
    /// low-pass filter of time constant τ: 50 µs in Europe and most of the world, 75 µs in the Americas.
    /// </summary>
    public static double[] DeEmphasis(IReadOnlyList<double> audio, double sampleRate, double tauSeconds = 50e-6)
    {
        double alpha = 1 - Math.Exp(-1 / (sampleRate * tauSeconds));
        var output = new double[audio.Count];
        double y = 0;
        for (int n = 0; n < output.Length; n++)
        {
            y += alpha * (audio[n] - y);   // move a fraction of the way towards each new sample
            output[n] = y;
        }
        return output;
    }
}
