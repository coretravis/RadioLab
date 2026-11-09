using System.Numerics;

namespace RadioLab.Spectrum;

/// <summary>The discrete Fourier transform: from samples in time to amounts of each frequency.</summary>
public static class Fourier
{
    /// <summary>
    /// The DFT, straight from its definition: for each frequency bin k, turn every sample backwards by
    /// k turns per N samples and add them all up. Anything turning at exactly that speed stops still and
    /// adds up; everything else spins and cancels. N² operations: clear, and slow.
    /// </summary>
    public static Complex[] Dft(IReadOnlyList<Complex> x)
    {
        int n = x.Count;
        var bins = new Complex[n];
        for (int k = 0; k < n; k++)
        {
            Complex sum = Complex.Zero;
            for (int t = 0; t < n; t++)
                sum += x[t] * Complex.FromPolarCoordinates(1.0, -2 * Math.PI * k * t / n);
            bins[k] = sum;
        }
        return bins;
    }

    /// <summary>
    /// The same result as <see cref="Dft"/>, computed by the radix-2 fast Fourier transform in N·log₂N
    /// operations. The length must be a power of two.
    /// </summary>
    public static Complex[] Fft(IReadOnlyList<Complex> x)
    {
        int n = x.Count;
        if (n == 0 || (n & (n - 1)) != 0) throw new ArgumentException("The FFT length must be a power of two.");
        var a = x.ToArray();

        // Reorder by bit-reversed index, so each pass can combine neighbours in place.
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) (a[i], a[j]) = (a[j], a[i]);
        }

        // Combine pairs of half-length transforms into full ones: the butterfly.
        for (int length = 2; length <= n; length <<= 1)
        {
            Complex step = Complex.FromPolarCoordinates(1.0, -2 * Math.PI / length);
            for (int start = 0; start < n; start += length)
            {
                Complex twiddle = Complex.One;
                for (int k = 0; k < length / 2; k++)
                {
                    Complex even = a[start + k];
                    Complex odd = a[start + k + length / 2] * twiddle;
                    a[start + k] = even + odd;
                    a[start + k + length / 2] = even - odd;
                    twiddle *= step;
                }
            }
        }
        return a;
    }

    /// <summary>
    /// Reorders bins so frequencies run from −fs/2 up to +fs/2, with 0 Hz in the middle, the way a
    /// spectrum is drawn.
    /// </summary>
    public static T[] Centre<T>(IReadOnlyList<T> bins)
    {
        int n = bins.Count, half = n / 2;
        var centred = new T[n];
        for (int k = 0; k < n; k++) centred[k] = bins[(k + half) % n];
        return centred;
    }

    /// <summary>The frequency of each centred bin, in Hz: from −fs/2 in steps of fs/N.</summary>
    public static double[] CentredFrequencies(int n, double sampleRate)
    {
        var f = new double[n];
        for (int k = 0; k < n; k++) f[k] = (k - n / 2) * sampleRate / n;
        return f;
    }

    /// <summary>
    /// A spectrum in dB relative to full scale: each bin's power, normalised so that a full-scale
    /// complex tone (amplitude 1) reads 0 dB in its bin.
    /// </summary>
    public static double[] PowerDb(IReadOnlyList<Complex> bins, double windowGain = 1.0)
    {
        int n = bins.Count;
        var db = new double[n];
        for (int k = 0; k < n; k++)
        {
            double magnitude = bins[k].Magnitude / (n * windowGain);
            db[k] = 20 * Math.Log10(Math.Max(magnitude, 1e-12));   // floor avoids log(0)
        }
        return db;
    }
}
