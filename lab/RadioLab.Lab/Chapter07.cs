using RadioLab.Filters;
using RadioLab.Signals;

namespace RadioLab.Lab;

/// <summary>Chapter 7, Digital filters.</summary>
public static class Chapter07
{
    public static void Run(LabOutput lab)
    {
        const double rate = 48_000;
        double[] f = Enumerable.Range(0, 481).Select(i => i * 50.0).ToArray();   // 0 to 24 kHz

        double[] average = FilterDesign.MovingAverage(24);
        double[] short31 = FilterDesign.LowPass(2000, rate, 31);
        double[] long101 = FilterDesign.LowPass(2000, rate, 101);
        lab.SeriesCsv("ch07-response.csv", "hz", "gain_db",
        [
            ("moving average, 24 taps", f, f.Select(x => FilterDesign.GainDb(average, x, rate)).ToArray()),
            ("windowed sinc, 31 taps", f, f.Select(x => FilterDesign.GainDb(short31, x, rate)).ToArray()),
            ("windowed sinc, 101 taps", f, f.Select(x => FilterDesign.GainDb(long101, x, rate)).ToArray()),
        ]);

        // Hear it: a tune buried in hiss, then the same after a 2.5 kHz low-pass filter.
        const int audioRate = 22_050;
        double[] tune = Tunes.FrereJacquesTune(audioRate, 4.0);
        double[] hiss = Generate.Noise(tune.Length, 0.2, seed: 11);
        double[] noisy = Generate.Add(tune, hiss);
        double[] filtered = new FirFilter(FilterDesign.LowPass(2500, audioRate, 101)).Process(noisy);
        lab.Wav("filtering-hiss.wav", [.. noisy, .. new double[audioRate / 2], .. filtered], audioRate);

        Console.WriteLine($"  hiss RMS before {Measure.Rms(hiss):0.000}, after the filter " +
                          $"{Measure.Rms(new FirFilter(FilterDesign.LowPass(2500, audioRate, 101)).Process(hiss)):0.000}");
    }
}
