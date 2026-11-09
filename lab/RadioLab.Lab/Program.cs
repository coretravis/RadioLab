// RadioLab.Lab: one experiment per chapter of "Radio from the Ground Up: Signals in Software".
//
//   dotnet run --project lab/RadioLab.Lab -- 3          runs chapter 3's experiment
//   dotnet run --project lab/RadioLab.Lab -- all        runs them all
//   dotnet run --project lab/RadioLab.Lab -- 3 myfolder  writes into myfolder instead of ./output
//
// Every chart, sound and recording in the book was produced by this program.

using RadioLab.Lab;

string which = args.Length > 0 ? args[0] : "all";
string output = args.Length > 1 ? args[1] : "output";
var lab = new LabOutput(output);

var chapters = new Dictionary<string, Action<LabOutput>>
{
    ["1"] = Chapter01.Run,
    ["2"] = Chapter02.Run,
    ["3"] = Chapter03.Run,
    ["4"] = Chapter04.Run,
    ["5"] = Chapter05.Run,
    ["6"] = Chapter06.Run,
    ["7"] = Chapter07.Run,
    ["8"] = Chapter08.Run,
    ["9"] = Chapter09.Run,
    ["10"] = Chapter10.Run,
};

foreach (var (number, run) in chapters)
{
    if (which != "all" && which != number) continue;
    Console.WriteLine($"Chapter {number}");
    run(lab);
}
