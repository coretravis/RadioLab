using System.Numerics;
using RadioLab.Rates;
using RadioLab.Signals;
using RadioLab.Spectrum;

namespace RadioLab.Lab;

/// <summary>Chapter 8, Changing the sample rate.</summary>
public static class Chapter08
{
    public static void Run(LabOutput lab)
    {
        const double rate = 48_000;
        const int factor = 4, n = 4096;
        double newRate = rate / factor;

        // A wanted tone at 1 kHz and an unwanted one at 10 kHz, which will not fit below the new
        // Nyquist frequency of 6 kHz.
        var wanted = Generate.Phasor(1000, rate, 48_000, amplitude: 0.5);
        var unwanted = Generate.Phasor(10_000, rate, 48_000, amplitude: 0.5);
        var input = wanted.Zip(unwanted, (a, b) => a + b).ToArray();

        // The wrong way: keep every 4th sample. The 10 kHz tone lands at 10 − 12 = −2 kHz.
        Complex[] naive = input.Where((_, i) => i % factor == 0).ToArray();
        // The right way: low-pass filter first.
        Complex[] proper = Resample.Decimate(input, factor, rate);

        double[] hann = Window.Hann(n);
        double[] Spectrum(Complex[] x) =>
            Fourier.Centre(Fourier.PowerDb(Fourier.Fft(Window.Apply(x.Skip(2000).Take(n).ToArray(), hann)), Window.Gain(hann)));
        double[] f = Fourier.CentredFrequencies(n, newRate);
        lab.SeriesCsv("ch08-decimation.csv", "hz", "db",
        [
            ("every 4th sample with no filter", f, Spectrum(naive)),
            ("filtered first then every 4th sample", f, Spectrum(proper)),
        ]);
    }
}
