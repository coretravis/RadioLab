using System.Numerics;

namespace RadioLab.Filters;

/// <summary>
/// A finite impulse response filter: each output is a weighted sum of the latest input samples.
/// The weights are the taps. This is convolution, done one sample at a time.
/// </summary>
public sealed class FirFilter
{
    private readonly double[] _taps;
    private readonly Complex[] _history;   // the latest inputs, newest at _position
    private int _position;

    public IReadOnlyList<double> Taps => _taps;

    public FirFilter(IReadOnlyList<double> taps)
    {
        if (taps.Count == 0) throw new ArgumentException("A filter needs at least one tap.");
        _taps = taps.ToArray();
        _history = new Complex[_taps.Length];
    }

    /// <summary>Feeds one sample in and returns one sample out.</summary>
    public Complex Next(Complex input)
    {
        _history[_position] = input;
        Complex sum = Complex.Zero;
        int index = _position;
        for (int k = 0; k < _taps.Length; k++)
        {
            sum += _taps[k] * _history[index];      // tap k weighs the input from k samples ago
            index = index == 0 ? _history.Length - 1 : index - 1;
        }
        _position = (_position + 1) % _history.Length;
        return sum;
    }

    /// <summary>Filters a whole block. State carries over, so blocks can be fed one after another.</summary>
    public Complex[] Process(IReadOnlyList<Complex> input)
    {
        var output = new Complex[input.Count];
        for (int n = 0; n < output.Length; n++) output[n] = Next(input[n]);
        return output;
    }

    /// <summary>Filters a real signal.</summary>
    public double[] Process(IReadOnlyList<double> input)
    {
        var output = new double[input.Count];
        for (int n = 0; n < output.Length; n++) output[n] = Next(new Complex(input[n], 0)).Real;
        return output;
    }
}
