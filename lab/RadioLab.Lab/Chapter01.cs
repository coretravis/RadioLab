using RadioLab.Signals;

namespace RadioLab.Lab;

/// <summary>Chapter 1, Setting up RadioLab: a first signal, seen as numbers and heard as sound.</summary>
public static class Chapter01
{
    public static void Run(LabOutput lab)
    {
        const int sampleRate = 8000;

        // Two seconds of a 440 Hz tone, the A above middle C, at half of full scale.
        double[] tone = Generate.Sine(440, sampleRate, 2 * sampleRate, amplitude: 0.5);
        lab.Wav("first-tone.wav", tone, sampleRate);

        // The first 40 samples, with their times in milliseconds, to look at in a chart.
        double[] milliseconds = Generate.Time(sampleRate, 40).Select(t => t * 1000).ToArray();
        lab.Csv("ch01-first-samples.csv", ["milliseconds", "value"], milliseconds, tone[..40]);

        Console.WriteLine($"  RMS {Measure.Rms(tone):0.0000}, level {Measure.DbFs(tone):0.0} dBFS");
    }
}
