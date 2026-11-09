# RadioLab

The reference project for **Radio from the Ground Up: Signals in Software**, the second book of the *Radio from the Ground Up* series. In the book you build RadioLab yourself, one chapter at a time; this repository is the finished result, to compare with yours or to start from.

RadioLab is plain C# on .NET 10, using only the libraries built into .NET. It generates signals, samples and quantises them, shifts them in frequency, computes spectra, filters and resamples them, reads recordings, and demodulates AM and FM.

## Running it

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```sh
git clone https://github.com/coretravis/RadioLab.git
cd RadioLab
dotnet test                                     # the unit tests, one file per part of the library
dotnet run --project lab/RadioLab.Lab -- 3      # chapter 3's experiment, written to ./output
dotnet run --project lab/RadioLab.Lab -- all    # every chapter's experiment
```

Each experiment writes CSV files you can open in a spreadsheet (`output/data`) and WAV files you can play (`output/audio`). Every chart and sound in the book was produced by these experiments.

## What is where

| Folder | What it holds |
| --- | --- |
| `src/RadioLab` | The library. `Output` (CSV, WAV), `Signals` (generating and measuring), `Sampling` (aliasing, the ADC), `Baseband` (frequency shifting), `Spectrum` (DFT, FFT, windows), `Filters`, `Rates` (decimation and interpolation), `Recordings` (I/Q files with SigMF metadata), `Demodulation` (AM and FM) |
| `tests/RadioLab.Tests` | Unit tests (xUnit) |
| `lab/RadioLab.Lab` | One experiment per chapter of the book |
| `recordings` | The two recordings the book studies |

## The recordings are simulated

`recordings/fm-band-100mhz` and `recordings/am-band-1mhz` are **simulations**, not captures from real hardware: three FM stations around 100 MHz and three AM stations around 1 MHz, built the way transmitters build their signals, with noise added, stored as 8-bit I/Q samples with [SigMF](https://sigmf.org/) metadata, the format real software-defined radio receivers produce. Their metadata says they are simulated. Chapter 9's experiment regenerates them exactly.

The stations play two old folk tunes, *Frère Jacques* and *Ode to Joy*, and Morse code.

## Licence

The code and the simulated recordings are released under the MIT licence; see `LICENSE`. The book itself is published separately under CC BY 4.0.
