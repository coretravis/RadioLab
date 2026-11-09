using RadioLab.Recordings;
using RadioLab.Spectrum;

namespace RadioLab.Lab;

/// <summary>Chapter 9, Radio recordings: write the two simulated recordings, read them back, look at them.</summary>
public static class Chapter09
{
    public static void Run(LabOutput lab)
    {
        string fmPath = Path.Combine(lab.Recordings, "fm-band-100mhz");
        string amPath = Path.Combine(lab.Recordings, "am-band-1mhz");
        SimulatedBand.FmBand().Write(fmPath);
        SimulatedBand.AmBand().Write(amPath);
        Console.WriteLine("  recordings/fm-band-100mhz.sigmf-data, recordings/am-band-1mhz.sigmf-data");

        // Read them back exactly as you would read a recording from real hardware.
        Spectrum(lab, IqRecording.Read(fmPath), "ch09-fm-band.csv", megahertz: true);
        Spectrum(lab, IqRecording.Read(amPath), "ch09-am-band.csv", megahertz: false);
    }

    private static void Spectrum(LabOutput lab, IqRecording recording, string name, bool megahertz)
    {
        const int size = 1024;
        double[] db = AveragedSpectrum.PowerDb(recording.Samples, size);
        // Bin frequencies are offsets from the centre; add the centre to get the real radio frequency.
        double scale = megahertz ? 1e6 : 1e3;
        double[] f = Fourier.CentredFrequencies(size, recording.SampleRate)
            .Select(offset => (recording.CentreFrequency + offset) / scale).ToArray();
        lab.Csv(name, [megahertz ? "mhz" : "khz", "db"], f, db);
        Console.WriteLine($"  {recording.Samples.Length:N0} samples at {recording.SampleRate / 1000:0} kS/s, centre {recording.CentreFrequency / scale:0.###} {(megahertz ? "MHz" : "kHz")}");
    }
}
