using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RadioLab.Recordings;

/// <summary>
/// A recording of complex samples, stored the way inexpensive USB SDR receivers store them: unsigned
/// 8-bit I and Q, interleaved (I₀ Q₀ I₁ Q₁ …), with 127.5 standing for zero. Beside the data file, a
/// small SigMF metadata file (JSON) records the sample rate and the frequency the receiver was tuned to.
/// </summary>
public sealed record IqRecording(Complex[] Samples, double SampleRate, double CentreFrequency, string Description)
{
    /// <summary>Reads <c>name.sigmf-data</c> and <c>name.sigmf-meta</c>.</summary>
    public static IqRecording Read(string basePath)
    {
        var meta = JsonNode.Parse(File.ReadAllText(basePath + ".sigmf-meta"))!;
        var global = meta["global"]!;
        if ((string?)global["core:datatype"] != "cu8")
            throw new InvalidDataException("Only unsigned 8-bit complex samples (cu8) are supported.");
        double rate = (double)global["core:sample_rate"]!;
        double centre = (double)meta["captures"]![0]!["core:frequency"]!;
        string description = (string?)global["core:description"] ?? "";

        byte[] bytes = File.ReadAllBytes(basePath + ".sigmf-data");
        var samples = new Complex[bytes.Length / 2];
        for (int n = 0; n < samples.Length; n++)
            samples[n] = new Complex(FromByte(bytes[2 * n]), FromByte(bytes[2 * n + 1]));
        return new IqRecording(samples, rate, centre, description);
    }

    /// <summary>Writes the data file and its SigMF metadata. Samples are scaled so ±1 fills the 8 bits.</summary>
    public void Write(string basePath)
    {
        var bytes = new byte[Samples.Length * 2];
        for (int n = 0; n < Samples.Length; n++)
        {
            bytes[2 * n] = ToByte(Samples[n].Real);
            bytes[2 * n + 1] = ToByte(Samples[n].Imaginary);
        }
        File.WriteAllBytes(basePath + ".sigmf-data", bytes);

        var meta = new JsonObject
        {
            ["global"] = new JsonObject
            {
                ["core:datatype"] = "cu8",
                ["core:sample_rate"] = SampleRate,
                ["core:version"] = "1.0.0",
                ["core:description"] = Description,
            },
            ["captures"] = new JsonArray(new JsonObject
            {
                ["core:sample_start"] = 0,
                ["core:frequency"] = CentreFrequency,
            }),
            ["annotations"] = new JsonArray(),
        };
        File.WriteAllText(basePath + ".sigmf-meta", meta.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static double FromByte(byte b) => (b - 127.5) / 127.5;

    private static byte ToByte(double v) => (byte)Math.Clamp(Math.Round(v * 127.5 + 127.5), 0, 255);
}
