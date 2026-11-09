using RadioLab.Sampling;
using RadioLab.Signals;

namespace RadioLab.Lab;

/// <summary>Chapter 4, Quantization and the ADC.</summary>
public static class Chapter04
{
    public static void Run(LabOutput lab)
    {
        // One cycle of a sine wave through a 3-bit converter: eight levels, a staircase.
        const double fine = 1000;
        double[] wave = Generate.Sine(1, fine, 1000, amplitude: 0.95);
        double[] t = Generate.Time(fine, 1000);
        lab.SeriesCsv("ch04-staircase.csv", "time", "value",
        [
            ("original", t, wave),
            ("3-bit converter", t, new Adc(3).Digitise(wave)),
        ]);

        // Measured signal-to-noise ratio of a full-scale tone, against the 6.02·bits + 1.76 rule.
        int[] bits = Enumerable.Range(2, 15).ToArray();
        double[] tone = Generate.Sine(1234.567, 100_000, 100_000, amplitude: 0.999);
        double[] measured = bits.Select(b =>
        {
            double[] digital = new Adc(b).Digitise(tone);
            double[] error = digital.Select((d, n) => d - tone[n]).ToArray();
            return Measure.SnrDb(tone, error);
        }).ToArray();
        lab.SeriesCsv("ch04-snr-bits.csv", "bits", "snr_db",
        [
            ("measured", bits.Select(b => (double)b).ToArray(), measured),
            ("6.02·bits + 1.76", bits.Select(b => (double)b).ToArray(), bits.Select(b => new Adc(b).IdealSnrDb).ToArray()),
        ]);
        foreach (var (b, m) in bits.Zip(measured)) Console.WriteLine($"  {b,2} bits: {m:0.0} dB");

        // Hear it: the same tune through 16-, 8-, 4- and 2-bit converters.
        const int rate = 22_050;
        double[] tune = Tunes.OdeToJoyTune(rate, 3.0);
        var clip = new List<double>();
        foreach (int b in new[] { 16, 8, 4, 2 })
        {
            clip.AddRange(new Adc(b).Digitise(tune));
            clip.AddRange(new double[rate / 2]);
        }
        lab.Wav("quantization-steps.wav", clip, rate);
    }
}
