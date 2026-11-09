using RadioLab.Output;
using RadioLab.Signals;

namespace RadioLab.Tests;

public class SignalTests
{
    [Fact]
    public void A_sine_starts_at_zero_and_peaks_a_quarter_cycle_later()
    {
        // 1 kHz sampled at 8 kHz: 8 samples per cycle, so the peak is sample 2.
        double[] x = Generate.Sine(1000, 8000, 8);
        Assert.Equal(0, x[0], 9);
        Assert.Equal(1, x[2], 9);
        Assert.Equal(-1, x[6], 9);
    }

    [Fact]
    public void A_cosine_is_the_sine_a_quarter_turn_ahead()
    {
        double[] c = Generate.Cosine(1000, 8000, 8);
        Assert.Equal(1, c[0], 9);
        Assert.Equal(0, c[2], 9);
    }

    [Fact]
    public void A_phasor_is_a_cosine_on_the_real_axis_and_a_sine_on_the_imaginary_axis()
    {
        var p = Generate.Phasor(1000, 8000, 8);
        double[] c = Generate.Cosine(1000, 8000, 8), s = Generate.Sine(1000, 8000, 8);
        for (int n = 0; n < 8; n++)
        {
            Assert.Equal(c[n], p[n].Real, 9);
            Assert.Equal(s[n], p[n].Imaginary, 9);
            Assert.Equal(1, p[n].Magnitude, 9);
        }
    }

    [Fact]
    public void The_RMS_of_a_sine_is_its_peak_divided_by_root_two()
    {
        double[] x = Generate.Sine(50, 10_000, 10_000, amplitude: 325);
        Assert.Equal(325 / Math.Sqrt(2), Measure.Rms(x), 1);
    }

    [Fact]
    public void Gaussian_noise_has_the_RMS_asked_for()
    {
        double[] noise = Generate.Noise(200_000, 0.25);
        Assert.Equal(0.25, Measure.Rms(noise), 2);
        Assert.Equal(0, Measure.Mean(noise), 2);
    }

    [Fact]
    public void A_full_scale_sine_reads_zero_dBFS_and_half_scale_reads_minus_six()
    {
        Assert.Equal(0, Measure.DbFs(Generate.Sine(100, 8000, 8000)), 2);
        Assert.Equal(-6.02, Measure.DbFs(Generate.Sine(100, 8000, 8000, amplitude: 0.5)), 2);
    }

    [Fact]
    public void Multiplying_two_tones_gives_the_sum_and_the_difference()
    {
        // cos A · cos B = ½cos(A−B) + ½cos(A+B): 1000 Hz × 1100 Hz → 100 Hz and 2100 Hz.
        const int rate = 48_000, count = 4800;
        double[] product = Generate.Multiply(Generate.Cosine(1000, rate, count), Generate.Cosine(1100, rate, count));
        double[] expected = Generate.Add(Generate.Cosine(100, rate, count, 0.5), Generate.Cosine(2100, rate, count, 0.5));
        for (int n = 0; n < count; n++) Assert.Equal(expected[n], product[n], 9);
    }

    [Fact]
    public void A_wav_file_has_a_44_byte_header_and_two_bytes_per_sample()
    {
        string path = Path.GetTempFileName();
        Wav.WriteMono16(path, Generate.Sine(440, 8000, 800), 8000);
        byte[] bytes = File.ReadAllBytes(path);
        File.Delete(path);
        Assert.Equal(44 + 800 * 2, bytes.Length);
        Assert.Equal("RIFF"u8.ToArray(), bytes[..4]);
        Assert.Equal(8000, BitConverter.ToInt32(bytes, 24));
    }

    [Fact]
    public void A_csv_file_has_a_header_and_uses_decimal_points()
    {
        string path = Path.GetTempFileName();
        Csv.Write(path, ["t", "x"], [0.0, 0.5], [1.25, -2.5]);
        string[] lines = File.ReadAllLines(path);
        File.Delete(path);
        Assert.Equal(["t,x", "0,1.25", "0.5,-2.5"], lines);
    }
}
