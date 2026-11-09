using System.Numerics;
using RadioLab.Baseband;
using RadioLab.Signals;

namespace RadioLab.Tests;

public class BasebandTests
{
    [Fact]
    public void Shifting_a_tone_down_by_its_own_frequency_stops_it_turning()
    {
        var tone = Generate.Phasor(1500, 48_000, 4800, amplitude: 0.8, phase: 0.6);
        Complex[] stopped = Mixer.Shift(tone, -1500, 48_000);
        foreach (Complex s in stopped)
        {
            Assert.Equal(0.8, s.Magnitude, 9);   // the length never changes
            Assert.Equal(0.6, s.Phase, 6);       // and the angle no longer moves
        }
    }

    [Fact]
    public void Shifting_keeps_working_over_millions_of_samples()
    {
        // The angle is kept between −π and +π, so precision does not drain away in a long signal.
        var tone = Generate.Phasor(123_456, 1_000_000, 2_000_000);
        Complex[] stopped = Mixer.Shift(tone, -123_456, 1_000_000);
        Assert.Equal(0, stopped[^1].Phase, 4);
    }

    [Fact]
    public void Downconverting_a_real_signal_gives_both_sidebands_as_separate_frequencies()
    {
        // 9 Hz and 11 Hz around a 10 Hz "carrier" become −1 Hz and +1 Hz: the sum is 2·cos(2π·1·t)
        // plus copies near −20 Hz that a filter would remove. At n = 0 every part is at angle 0.
        double[] real = Generate.Add(Generate.Cosine(9, 100, 200), Generate.Cosine(11, 100, 200));
        Complex[] baseband = Mixer.Downconvert(real, 10, 100);
        Assert.Equal(2, baseband[0].Real, 9);
        Assert.Equal(0, baseband[0].Imaginary, 9);
    }
}
