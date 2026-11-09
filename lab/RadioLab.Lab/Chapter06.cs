using System.Diagnostics;
using System.Numerics;
using RadioLab.Signals;
using RadioLab.Spectrum;

namespace RadioLab.Lab;

/// <summary>Chapter 6, The discrete Fourier transform.</summary>
public static class Chapter06
{
    public static void Run(LabOutput lab)
    {
        const double rate = 8000;
        const int n = 1024;

        // Three tones at very different strengths, plus a little noise: a spectrum shows all three at once.
        var x = new Complex[n];
        var noise = Generate.ComplexNoise(n, 0.001);
        var a = Generate.Phasor(-2000, rate, n, amplitude: 0.5);     // −6 dB
        var b = Generate.Phasor(1000, rate, n, amplitude: 0.05);     // −26 dB
        var c = Generate.Phasor(2500, rate, n, amplitude: 0.005);    // −46 dB
        for (int i = 0; i < n; i++) x[i] = a[i] + b[i] + c[i] + noise[i];

        double[] hann = Window.Hann(n);
        double[] spectrum = Fourier.Centre(Fourier.PowerDb(Fourier.Fft(Window.Apply(x, hann)), Window.Gain(hann)));
        double[] f = Fourier.CentredFrequencies(n, rate);
        lab.Csv("ch06-three-tones.csv", ["hz", "db"], f, spectrum);

        // Leakage: a tone halfway between two bins, without a window and with a Hann window.
        var between = Generate.Phasor(1000 + 0.5 * rate / n, rate, n);
        double[] plain = Fourier.Centre(Fourier.PowerDb(Fourier.Fft(between)));
        double[] windowed = Fourier.Centre(Fourier.PowerDb(Fourier.Fft(Window.Apply(between, hann)), Window.Gain(hann)));
        lab.SeriesCsv("ch06-leakage.csv", "hz", "db",
        [
            ("no window", f, plain),
            ("Hann window", f, windowed),
        ]);

        // How long the DFT and the FFT take on this computer.
        foreach (int size in new[] { 256, 1024, 4096 })
        {
            var block = Generate.Phasor(1000, rate, size);
            var watch = Stopwatch.StartNew();
            Fourier.Dft(block);
            double dft = watch.Elapsed.TotalMilliseconds;
            watch.Restart();
            for (int r = 0; r < 100; r++) Fourier.Fft(block);
            double fft = watch.Elapsed.TotalMilliseconds / 100;
            Console.WriteLine($"  N = {size,5}: DFT {dft,8:0.00} ms, FFT {fft,6:0.000} ms");
        }
    }
}
