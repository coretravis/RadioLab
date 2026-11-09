using System.Numerics;
using RadioLab.Baseband;
using RadioLab.Signals;
using RadioLab.Spectrum;

namespace RadioLab.Tests;

public class SpectrumTests
{
    [Fact]
    public void The_FFT_gives_the_same_bins_as_the_DFT()
    {
        var random = new Random(3);
        var x = Enumerable.Range(0, 64).Select(_ => new Complex(random.NextDouble() - 0.5, random.NextDouble() - 0.5)).ToArray();
        Complex[] slow = Fourier.Dft(x), fast = Fourier.Fft(x);
        for (int k = 0; k < 64; k++) Assert.True((slow[k] - fast[k]).Magnitude < 1e-9);
    }

    [Fact]
    public void A_tone_on_a_bin_lands_in_exactly_that_bin()
    {
        // 64 samples at 6400 Hz: bins are 100 Hz apart, so 1000 Hz is bin 10.
        var x = Generate.Phasor(1000, 6400, 64);
        Complex[] bins = Fourier.Fft(x);
        Assert.Equal(64, bins[10].Magnitude, 6);
        for (int k = 0; k < 64; k++)
            if (k != 10) Assert.True(bins[k].Magnitude < 1e-9);
    }

    [Fact]
    public void A_negative_frequency_lands_left_of_centre()
    {
        var x = Generate.Phasor(-1000, 6400, 64);
        Complex[] centred = Fourier.Centre(Fourier.Fft(x));
        double[] f = Fourier.CentredFrequencies(64, 6400);
        int peak = Array.IndexOf(centred, centred.MaxBy(b => b.Magnitude));
        Assert.Equal(-1000, f[peak], 9);
    }

    [Fact]
    public void A_full_scale_tone_reads_zero_dB_in_its_bin()
    {
        var x = Generate.Phasor(1000, 6400, 64);
        Assert.Equal(0, Fourier.PowerDb(Fourier.Fft(x))[10], 6);
    }

    [Fact]
    public void Shifting_moves_a_tone_by_exactly_the_shift()
    {
        var x = Generate.Phasor(1500, 6400, 64);
        var shifted = Mixer.Shift(x, -500, 6400);
        Complex[] bins = Fourier.Fft(shifted);
        Assert.Equal(64, bins[10].Magnitude, 6);   // 1500 − 500 = 1000 Hz = bin 10
    }

    [Fact]
    public void A_real_tone_has_two_mirror_image_frequencies()
    {
        double[] real = Generate.Cosine(1000, 6400, 64);
        Complex[] bins = Fourier.Fft(real.Select(v => new Complex(v, 0)).ToArray());
        Assert.Equal(32, bins[10].Magnitude, 6);        // +1000 Hz, half the amplitude
        Assert.Equal(32, bins[64 - 10].Magnitude, 6);   // −1000 Hz, the other half
    }
}
