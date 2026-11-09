using RadioLab.Sampling;
using RadioLab.Signals;

namespace RadioLab.Lab;

/// <summary>Chapter 3, Sampling and aliasing.</summary>
public static class Chapter03
{
    public static void Run(LabOutput lab)
    {
        // A 7 kHz wave and a 1 kHz wave, drawn finely over 1 ms, and the samples an 8 kHz converter takes.
        const double fine = 400_000, fs = 8000;
        double[] tFine = Generate.Time(fine, 400).Select(t => t * 1000).ToArray();
        double[] tSamples = Generate.Time(fs, 9).Select(t => t * 1000).ToArray();
        lab.SeriesCsv("ch03-alias.csv", "milliseconds", "value",
        [
            ("7 kHz wave", tFine, Generate.Cosine(7000, fine, 400)),
            ("1 kHz wave", tFine, Generate.Cosine(1000, fine, 400)),
            ("samples at 8 kHz", tSamples, Generate.Cosine(7000, fs, 9)),
        ]);

        // Where every input frequency from 0 to 24 kHz ends up after sampling at 8 kHz.
        double[] input = Enumerable.Range(0, 241).Select(i => i * 100.0).ToArray();
        lab.Csv("ch03-apparent.csv", ["input_khz", "apparent_khz"],
            input.Select(f => f / 1000).ToArray(),
            input.Select(f => Aliasing.ApparentFrequency(f, fs) / 1000).ToArray());

        // Hear it: a glide from 0 to 4 kHz (all below Nyquist), then a glide from 0 to 8 kHz.
        // Both are computed at 8000 samples per second, so the second one folds back down at 4 kHz.
        const int rate = 8000;
        double[] safe = Generate.Chirp(0, 4000, rate, 4 * rate, amplitude: 0.4);
        double[] gap = new double[rate / 2];
        double[] folding = Generate.Chirp(0, 8000, rate, 4 * rate, amplitude: 0.4);
        lab.Wav("aliasing-glide.wav", [.. safe, .. gap, .. folding], rate);
    }
}
