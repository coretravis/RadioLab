namespace RadioLab.Lab;

/// <summary>Test sounds for the experiments: two old folk tunes and Morse code, made of sine waves.</summary>
public static class Tunes
{
    // Notes as (MIDI number, length in beats). MIDI 60 is middle C; each step is a semitone.
    private static readonly (int Note, double Beats)[] FrereJacques =
    [
        (60, 1), (62, 1), (64, 1), (60, 1), (60, 1), (62, 1), (64, 1), (60, 1),
        (64, 1), (65, 1), (67, 2), (64, 1), (65, 1), (67, 2),
    ];

    private static readonly (int Note, double Beats)[] OdeToJoy =
    [
        (64, 1), (64, 1), (65, 1), (67, 1), (67, 1), (65, 1), (64, 1), (62, 1),
        (60, 1), (60, 1), (62, 1), (64, 1), (64, 1.5), (62, 0.5), (62, 2),
    ];

    public static double[] FrereJacquesTune(double sampleRate, double seconds, double beat = 0.25, int transpose = 0) =>
        Play(FrereJacques, sampleRate, seconds, beat, transpose);

    public static double[] OdeToJoyTune(double sampleRate, double seconds, double beat = 0.25, int transpose = 0) =>
        Play(OdeToJoy, sampleRate, seconds, beat, transpose);

    /// <summary>A tune played on a simple "instrument": a tone with two harmonics and a soft attack and decay.</summary>
    private static double[] Play((int Note, double Beats)[] tune, double sampleRate, double seconds, double beat, int transpose)
    {
        var x = new double[(int)(sampleRate * seconds)];
        int n = 0;
        while (n < x.Length)
        {
            foreach (var (note, beats) in tune)
            {
                double f = 440 * Math.Pow(2, (note + transpose - 69) / 12.0);
                int length = (int)(beats * beat * sampleRate);
                for (int i = 0; i < length && n < x.Length; i++, n++)
                {
                    double t = i / sampleRate;
                    double envelope = Math.Min(1, t / 0.01) * Math.Exp(-3 * t / (beats * beat));
                    double tone = Math.Sin(2 * Math.PI * f * t) + 0.4 * Math.Sin(4 * Math.PI * f * t) + 0.15 * Math.Sin(6 * Math.PI * f * t);
                    x[n] = 0.6 * envelope * tone;
                }
                if (n >= x.Length) break;
            }
        }
        return x;
    }

    private static readonly Dictionary<char, string> MorseCode = new()
    {
        ['A'] = ".-", ['B'] = "-...", ['C'] = "-.-.", ['D'] = "-..", ['E'] = ".", ['F'] = "..-.", ['G'] = "--.",
        ['H'] = "....", ['I'] = "..", ['J'] = ".---", ['K'] = "-.-", ['L'] = ".-..", ['M'] = "--", ['N'] = "-.",
        ['O'] = "---", ['P'] = ".--.", ['Q'] = "--.-", ['R'] = ".-.", ['S'] = "...", ['T'] = "-", ['U'] = "..-",
        ['V'] = "...-", ['W'] = ".--", ['X'] = "-..-", ['Y'] = "-.--", ['Z'] = "--..",
    };

    /// <summary>
    /// A message in Morse code as an on/off tone. A dot is one unit, a dash three, the gap inside a letter
    /// one, between letters three, between words seven.
    /// </summary>
    public static double[] Morse(string message, double sampleRate, double seconds, double toneHz = 700, double unitSeconds = 0.05)
    {
        var x = new double[(int)(sampleRate * seconds)];
        int unit = (int)(unitSeconds * sampleRate), n = 0;
        void Tone(int units) { for (int i = 0; i < units * unit && n < x.Length; i++, n++) x[n] = 0.6 * Math.Sin(2 * Math.PI * toneHz * n / sampleRate); }
        void Gap(int units) => n = Math.Min(x.Length, n + units * unit);

        foreach (string word in message.ToUpperInvariant().Split(' '))
        {
            foreach (char letter in word)
            {
                foreach (char symbol in MorseCode[letter]) { Tone(symbol == '.' ? 1 : 3); Gap(1); }
                Gap(2);
            }
            Gap(4);
        }
        return x;
    }
}
