using System.Numerics;
using RadioLab.Baseband;
using RadioLab.Signals;

namespace RadioLab.Lab;

/// <summary>Chapter 5, Complex baseband in code.</summary>
public static class Chapter05
{
    public static void Run(LabOutput lab)
    {
        // A signal 3 Hz above the centre, sampled at 100 Hz: shift it down by 3 Hz and it stops turning.
        const double rate = 100;
        var signal = Generate.Phasor(3, rate, 100, amplitude: 0.8, phase: 0.6);
        var stopped = Mixer.Shift(signal, -3, rate);
        double[] t = Generate.Time(rate, 100);
        lab.SeriesCsv("ch05-shift.csv", "seconds", "value",
        [
            ("I before", t, signal.Select(s => s.Real).ToArray()),
            ("Q before", t, signal.Select(s => s.Imaginary).ToArray()),
            ("I after shifting by −3 Hz", t, stopped.Select(s => s.Real).ToArray()),
            ("Q after shifting by −3 Hz", t, stopped.Select(s => s.Imaginary).ToArray()),
        ]);

        // A real signal made of two tones, 9 Hz and 11 Hz, as an AM station's two sidebands around 10 Hz.
        // Downconverting by 10 Hz gives −1 Hz and +1 Hz: still two tones, now distinguishable by direction.
        double[] real = Generate.Add(Generate.Cosine(9, rate, 200), Generate.Cosine(11, rate, 200));
        Complex[] baseband = Mixer.Downconvert(real, 10, rate);
        Console.WriteLine($"  first baseband samples: {string.Join(", ", baseband.Take(3).Select(z => $"{z.Real:0.00}{z.Imaginary:+0.00;-0.00}j"))}");
    }
}
