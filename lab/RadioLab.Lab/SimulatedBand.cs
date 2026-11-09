using System.Numerics;
using RadioLab.Recordings;
using RadioLab.Signals;

namespace RadioLab.Lab;

/// <summary>
/// Builds the two simulated recordings the book studies: a slice of the FM broadcast band and a slice
/// of the medium-wave AM band. They are made the way a transmitter makes its signal, then mixed with
/// noise and stored as 8-bit samples, like a real receiver's output. They are simulations, and their
/// metadata says so.
/// </summary>
public static class SimulatedBand
{
    public const double FmRate = 768_000, FmCentre = 100.0e6, FmTuningError = 2_000;
    public const double AmRate = 256_000, AmCentre = 1.0e6;
    public const double Seconds = 3.0;

    /// <summary>Three FM stations around 100 MHz: two strong, one barely above the noise.</summary>
    public static IqRecording FmBand()
    {
        int count = (int)(FmRate * Seconds);
        var band = Generate.ComplexNoise(count, 0.01, seed: 21);
        AddFmStation(band, offsetHz: 50_000, amplitude: 0.30, Tunes.FrereJacquesTune(FmRate, Seconds));
        AddFmStation(band, offsetHz: -250_000, amplitude: 0.20, Tunes.OdeToJoyTune(FmRate, Seconds, transpose: 5));
        AddFmStation(band, offsetHz: 300_000, amplitude: 0.012, Tunes.Morse("CQ CQ DE RADIOLAB", FmRate, Seconds));
        return new IqRecording(band, FmRate, FmCentre,
            "SIMULATED, not a real capture: three FM stations at 99.75, 100.05 and 100.30 MHz, with noise, " +
            "made by RadioLab.Lab chapter 9. The simulated receiver is tuned 2 kHz low, as cheap receivers often are.");
    }

    /// <summary>Three AM stations around 1 MHz, the way a medium-wave receiver would see them.</summary>
    public static IqRecording AmBand()
    {
        int count = (int)(AmRate * Seconds);
        var band = Generate.ComplexNoise(count, 0.01, seed: 22);
        AddAmStation(band, offsetHz: 20_000, carrier: 0.25, index: 0.7, Tunes.FrereJacquesTune(AmRate, Seconds, transpose: -12));
        AddAmStation(band, offsetHz: 50_000, carrier: 0.12, index: 0.7, Tunes.OdeToJoyTune(AmRate, Seconds));
        AddAmStation(band, offsetHz: -40_000, carrier: 0.08, index: 0.8, Tunes.Morse("VVV DE RADIOLAB", AmRate, Seconds, toneHz: 600));
        return new IqRecording(band, AmRate, AmCentre,
            "SIMULATED, not a real capture: three AM stations at 960, 1020 and 1050 kHz, with noise, " +
            "made by RadioLab.Lab chapter 9.");
    }

    /// <summary>
    /// Frequency modulation, as a transmitter does it: the audio sets how fast the phase turns. The
    /// broadcaster's treble boost (pre-emphasis, 50 µs) is applied first, so the receiver must undo it.
    /// </summary>
    private static void AddFmStation(Complex[] band, double offsetHz, double amplitude, double[] audio)
    {
        const double deviation = 75_000, tau = 50e-6;
        double phase = 0, previous = 0;
        double carrierStep = 2 * Math.PI * (offsetHz + FmTuningError) / FmRate;
        for (int n = 0; n < band.Length; n++)
        {
            // Pre-emphasis: the audio plus τ times its rate of change, which lifts the treble.
            double boosted = audio[n] + tau * FmRate * (audio[n] - previous);
            previous = audio[n];
            phase += carrierStep + 2 * Math.PI * deviation * boosted / FmRate;
            if (phase > Math.PI) phase -= 2 * Math.PI;
            else if (phase < -Math.PI) phase += 2 * Math.PI;
            band[n] += Complex.FromPolarCoordinates(amplitude, phase);
        }
    }

    /// <summary>Amplitude modulation: the carrier's length is carrier·(1 + index·audio).</summary>
    private static void AddAmStation(Complex[] band, double offsetHz, double carrier, double index, double[] audio)
    {
        for (int n = 0; n < band.Length; n++)
        {
            double angle = 2 * Math.PI * offsetHz * n / AmRate;
            band[n] += Complex.FromPolarCoordinates(carrier * (1 + index * audio[n]), angle);
        }
    }
}
