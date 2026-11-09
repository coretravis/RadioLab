using System.Numerics;
using RadioLab.Demodulation;
using RadioLab.Recordings;
using RadioLab.Signals;

namespace RadioLab.Tests;

public class DemodulationTests
{
    [Fact]
    public void AM_recovers_the_message_whatever_the_carrier_phase()
    {
        const double rate = 32_000;
        double[] message = Generate.Sine(400, rate, 3200, amplitude: 0.5);
        // Envelope (1 + message), with the carrier's phase drifting slowly: a real receiver is never exactly in tune.
        var drift = Generate.Phasor(3, rate, 3200);
        var signal = message.Select((m, n) => (1 + m) * drift[n]).ToArray();
        double[] audio = AmDemodulator.Demodulate(signal);
        for (int n = 0; n < audio.Length; n++) Assert.Equal(message[n], audio[n], 2);
    }

    [Fact]
    public void FM_turns_the_speed_of_rotation_back_into_the_message()
    {
        const double rate = 256_000, deviation = 75_000;
        double[] message = Generate.Sine(1000, rate, 25_600);
        // Frequency modulation: the phase grows by 2π·(deviation·message)/rate each sample.
        var signal = new Complex[message.Length];
        double phase = 0;
        for (int n = 0; n < message.Length; n++)
        {
            phase += 2 * Math.PI * deviation * message[n] / rate;
            signal[n] = Complex.FromPolarCoordinates(1, phase);
        }
        double[] frequency = new FmDemodulator(rate).Process(signal);
        for (int n = 1; n < message.Length; n++) Assert.Equal(deviation * message[n], frequency[n], tolerance: 0.001);
    }

    [Fact]
    public void A_recording_survives_a_round_trip_within_8_bit_precision()
    {
        string basePath = Path.Combine(Path.GetTempPath(), "radiolab-test");
        var samples = Generate.Phasor(1000, 48_000, 480, amplitude: 0.9);
        new IqRecording(samples, 48_000, 100e6, "test").Write(basePath);
        var back = IqRecording.Read(basePath);
        File.Delete(basePath + ".sigmf-data");
        File.Delete(basePath + ".sigmf-meta");

        Assert.Equal(48_000, back.SampleRate);
        Assert.Equal(100e6, back.CentreFrequency);
        for (int n = 0; n < samples.Length; n++) Assert.True((back.Samples[n] - samples[n]).Magnitude < 1.0 / 127.5);
    }
}
