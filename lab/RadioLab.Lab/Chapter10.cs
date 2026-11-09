using System.Numerics;
using RadioLab.Baseband;
using RadioLab.Demodulation;
using RadioLab.Filters;
using RadioLab.Rates;
using RadioLab.Recordings;
using RadioLab.Signals;

namespace RadioLab.Lab;

/// <summary>Chapter 10, First demodulators: an AM receiver and an FM receiver, built from every earlier chapter.</summary>
public static class Chapter10
{
    public static void Run(LabOutput lab)
    {
        var fm = IqRecording.Read(Path.Combine(lab.Recordings, "fm-band-100mhz"));
        double[] music = ReceiveFm(fm, stationHz: 100.05e6, out double[] frequency);
        lab.Wav("fm-station.wav", music, 32_000);
        double[] weak = ReceiveFm(fm, stationHz: 100.30e6, out _);
        lab.Wav("fm-weak-station.wav", weak, 32_000);

        // The turning speed over 4 ms, in kHz: the music, as frequency.
        int start = 256_000 / 2;
        double[] ms = Enumerable.Range(0, 1024).Select(i => i * 1000.0 / 256_000).ToArray();
        lab.Csv("ch10-fm-frequency.csv", ["milliseconds", "khz"], ms, frequency.Skip(start).Take(1024).Select(f => f / 1000).ToArray());

        var am = IqRecording.Read(Path.Combine(lab.Recordings, "am-band-1mhz"));
        double[] speech = ReceiveAm(am, stationHz: 1.020e6, out double[] envelope);
        lab.Wav("am-station.wav", speech, 32_000);
        double[] amMs = Enumerable.Range(0, 640).Select(i => i * 1000.0 / 32_000).ToArray();
        lab.Csv("ch10-am-envelope.csv", ["milliseconds", "length"], amMs, envelope.Skip(16_000).Take(640).ToArray());
    }

    /// <summary>
    /// An FM receiver: shift the station to 0 Hz, filter and decimate to 256 kS/s, measure the turning
    /// speed, filter and decimate to 32 kS/s, undo the pre-emphasis, remove the steady offset, scale.
    /// </summary>
    public static double[] ReceiveFm(IqRecording recording, double stationHz, out double[] frequency)
    {
        double rate = recording.SampleRate;                                     // 768 kS/s
        Complex[] tuned = Mixer.Shift(recording.Samples, -(stationHz - recording.CentreFrequency), rate);
        Complex[] narrow = Resample.Decimate(tuned, 3, rate);                   // 256 kS/s: keeps ±100 kHz
        frequency = new FmDemodulator(rate / 3).Process(narrow);                // Hz, sample by sample
        double[] audio = Resample.Decimate(frequency, 8, rate / 3);             // 32 kS/s
        audio = FmDemodulator.DeEmphasis(audio, 32_000);
        double offset = Measure.Mean(audio);                                    // a mistuned receiver's offset
        return audio.Select(a => (a - offset) / 75_000).ToArray();              // ±75 kHz becomes ±1
    }

    /// <summary>
    /// An AM receiver: shift the station to 0 Hz, filter and decimate to 32 kS/s, keep ±5 kHz, take the
    /// length of each sample, remove the carrier.
    /// </summary>
    public static double[] ReceiveAm(IqRecording recording, double stationHz, out double[] envelope)
    {
        double rate = recording.SampleRate;                                     // 256 kS/s
        Complex[] tuned = Mixer.Shift(recording.Samples, -(stationHz - recording.CentreFrequency), rate);
        Complex[] narrow = Resample.Decimate(tuned, 8, rate);                   // 32 kS/s
        narrow = new FirFilter(FilterDesign.LowPass(5_000, 32_000, 101)).Process(narrow);
        envelope = narrow.Select(s => s.Magnitude).ToArray();
        double[] audio = AmDemodulator.Demodulate(narrow);
        double peak = audio.Max(Math.Abs);
        return audio.Select(a => 0.8 * a / peak).ToArray();
    }
}
