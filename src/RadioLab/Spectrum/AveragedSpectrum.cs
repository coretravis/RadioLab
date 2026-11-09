using System.Numerics;

namespace RadioLab.Spectrum;

/// <summary>
/// A spectrum averaged over many blocks (Welch's method). One FFT of noise is ragged; averaging the
/// power of many blocks smooths the noise into a steady floor, so weak signals stand out.
/// </summary>
public static class AveragedSpectrum
{
    /// <summary>
    /// Splits the samples into blocks of <paramref name="size"/>, windows each with Hann, and averages the
    /// power of their FFTs. Returns centred bins in dB, scaled so a full-scale tone reads 0 dB.
    /// </summary>
    public static double[] PowerDb(IReadOnlyList<Complex> samples, int size, int maxBlocks = int.MaxValue)
    {
        double[] window = Window.Hann(size);
        double gain = Window.Gain(window);
        var power = new double[size];
        int blocks = Math.Min(samples.Count / size, maxBlocks);
        if (blocks == 0) throw new ArgumentException("Not enough samples for one block.");

        for (int b = 0; b < blocks; b++)
        {
            var block = new Complex[size];
            for (int i = 0; i < size; i++) block[i] = samples[b * size + i] * window[i];
            Complex[] bins = Fourier.Fft(block);
            for (int k = 0; k < size; k++)
            {
                double magnitude = bins[k].Magnitude / (size * gain);
                power[k] += magnitude * magnitude / blocks;   // average power, not average amplitude
            }
        }
        return Fourier.Centre(power.Select(p => 10 * Math.Log10(Math.Max(p, 1e-24))).ToArray());
    }
}
