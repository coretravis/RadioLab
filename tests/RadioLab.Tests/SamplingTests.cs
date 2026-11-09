using RadioLab.Sampling;
using RadioLab.Signals;

namespace RadioLab.Tests;

public class SamplingTests
{
    [Theory]
    [InlineData(1000, 8000, 1000)]   // below Nyquist: unchanged
    [InlineData(5000, 8000, 3000)]   // above Nyquist: folds back to fs − f
    [InlineData(7000, 8000, 1000)]
    [InlineData(9000, 8000, 1000)]   // above fs: shifts down by fs
    [InlineData(4000, 8000, 4000)]   // exactly Nyquist
    public void A_tone_appears_at_its_folded_frequency(double f, double fs, double apparent) =>
        Assert.Equal(apparent, Aliasing.ApparentFrequency(f, fs), 9);

    [Fact]
    public void Samples_of_a_tone_and_its_alias_are_identical()
    {
        // 7 kHz sampled at 8 kHz gives exactly the samples of 1 kHz (with the phase flipped).
        double[] high = Generate.Cosine(7000, 8000, 64);
        double[] low = Generate.Cosine(1000, 8000, 64);
        for (int n = 0; n < 64; n++) Assert.Equal(low[n], high[n], 9);
    }

    [Fact]
    public void An_eight_bit_converter_has_256_levels_and_clips()
    {
        var adc = new Adc(8);
        Assert.Equal(-128, adc.MinCode);
        Assert.Equal(127, adc.MaxCode);
        Assert.Equal(127, adc.ToCode(5.0));
        Assert.Equal(-128, adc.ToCode(-5.0));
        Assert.Equal(2.0 / 256, adc.Step, 12);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(12)]
    public void Quantization_noise_matches_six_dB_per_bit(int bits)
    {
        var adc = new Adc(bits);
        // A full-scale tone that does not repeat in step with the samples, so the errors look random.
        double[] signal = Generate.Sine(1234.567, 100_000, 100_000, amplitude: 0.999);
        double[] digital = adc.Digitise(signal);
        double[] error = new double[signal.Length];
        for (int n = 0; n < error.Length; n++) error[n] = digital[n] - signal[n];
        Assert.InRange(Measure.SnrDb(signal, error), adc.IdealSnrDb - 1, adc.IdealSnrDb + 1);
    }
}
