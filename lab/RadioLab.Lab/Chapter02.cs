using RadioLab.Signals;

namespace RadioLab.Lab;

/// <summary>Chapter 2, Generating signals: phasors, and what multiplying two tones does.</summary>
public static class Chapter02
{
    public static void Run(LabOutput lab)
    {
        // A phasor turning twice per second, sampled 100 times per second for one second:
        // its real part (I) is a cosine, its imaginary part (Q) a sine.
        const double rate = 100;
        var phasor = Generate.Phasor(2, rate, 100);
        double[] t = Generate.Time(rate, 100);
        lab.SeriesCsv("ch02-phasor.csv", "seconds", "value",
        [
            ("I (real part)", t, phasor.Select(p => p.Real).ToArray()),
            ("Q (imaginary part)", t, phasor.Select(p => p.Imaginary).ToArray()),
        ]);

        // The same phasor turning the other way: I is unchanged, Q is flipped.
        var backwards = Generate.Phasor(-2, rate, 100);
        lab.SeriesCsv("ch02-negative.csv", "seconds", "value",
        [
            ("Q at +2 Hz", t, phasor.Select(p => p.Imaginary).ToArray()),
            ("Q at −2 Hz", t, backwards.Select(p => p.Imaginary).ToArray()),
        ]);

        // Mixing: 10 Hz times 12 Hz, drawn slowly enough to see the 2 Hz and 22 Hz parts.
        const double mixRate = 1000;
        double[] tm = Generate.Time(mixRate, 1000);
        double[] product = Generate.Multiply(Generate.Cosine(10, mixRate, 1000), Generate.Cosine(12, mixRate, 1000));
        double[] difference = Generate.Cosine(2, mixRate, 1000, amplitude: 0.5);
        lab.SeriesCsv("ch02-mixing.csv", "seconds", "value",
        [
            ("10 Hz × 12 Hz", tm, product),
            ("½·cos(2π·2t), the difference part", tm, difference),
        ]);
    }
}
