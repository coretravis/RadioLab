using System.Numerics;
using RadioLab.Filters;
using RadioLab.Rates;
using RadioLab.Signals;
using RadioLab.Spectrum;

namespace RadioLab.Tests;

public class FilterTests
{
    [Fact]
    public void A_moving_average_of_a_steady_value_settles_on_that_value()
    {
        var filter = new FirFilter(FilterDesign.MovingAverage(4));
        double[] y = filter.Process(Enumerable.Repeat(2.0, 10).ToArray());
        Assert.Equal(0.5, y[0], 9);   // still filling up
        Assert.Equal(2.0, y[9], 9);
    }

    [Fact]
    public void The_low_pass_filter_passes_low_frequencies_and_stops_high_ones()
    {
        double[] taps = FilterDesign.LowPass(1000, 48_000, 101);
        Assert.Equal(1.0, taps.Sum(), 9);
        Assert.InRange(FilterDesign.GainDb(taps, 0, 48_000), -0.01, 0.01);
        Assert.InRange(FilterDesign.GainDb(taps, 500, 48_000), -1, 0.1);
        Assert.True(FilterDesign.GainDb(taps, 4000, 48_000) < -40);
    }

    [Fact]
    public void Filtering_removes_a_tone_above_the_cutoff()
    {
        const double rate = 48_000;
        double[] mix = Generate.Add(Generate.Sine(300, rate, 4800), Generate.Sine(6000, rate, 4800));
        double[] filtered = new FirFilter(FilterDesign.LowPass(1000, rate, 201)).Process(mix);
        // After the filter has filled, what is left is the 300 Hz tone, delayed by 100 samples.
        double[] reference = Generate.Sine(300, rate, 4800);
        for (int n = 400; n < 4800; n++) Assert.Equal(reference[n - 100], filtered[n], tolerance: 0.01);
    }

    [Fact]
    public void Decimating_keeps_a_tone_inside_the_new_band_and_removes_one_that_would_alias()
    {
        const double rate = 48_000;
        var wanted = Generate.Phasor(1000, rate, 48_000);
        var unwanted = Generate.Phasor(10_000, rate, 48_000);   // would alias to 2 kHz at 12 kHz
        var input = wanted.Zip(unwanted, (a, b) => a + b).ToArray();
        Complex[] output = Resample.Decimate(input, 4, rate);
        Assert.Equal(12_000, output.Length);

        Complex[] block = output.Skip(4000).Take(4096).ToArray();
        double[] db = Fourier.PowerDb(Fourier.Fft(Window.Apply(block, Window.Hann(4096))), 0.5);
        double[] f = Enumerable.Range(0, 4096).Select(k => k * 12_000.0 / 4096).ToArray();
        double Level(double hz) => db[(int)Math.Round(hz / (12_000.0 / 4096))];
        Assert.InRange(Level(1000), -1, 0.5);
        Assert.True(Level(2000) < -50);
    }

    [Fact]
    public void Interpolating_keeps_the_tone_and_multiplies_the_sample_count()
    {
        const double rate = 8000;
        var input = Generate.Phasor(500, rate, 2000);
        Complex[] output = Resample.Interpolate(input, 4, rate);
        Assert.Equal(8000, output.Length);
        Assert.InRange(Measure.Rms(output.Skip(1000).ToArray()), 0.98, 1.02);
    }
}
