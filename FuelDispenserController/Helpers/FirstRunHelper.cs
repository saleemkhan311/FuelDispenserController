using System;
using System.IO;

namespace FuelDispenserController.Core.Helpers;

public static class FirstRunHelper
{
    private static readonly string AppFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FuelDispenserController");

    private static readonly string FirstRunFlagPath = Path.Combine(AppFolder, "first_run.flag");

    public static bool IsFirstRun()
    {
        return !File.Exists(FirstRunFlagPath);
    }

    public static void MarkFirstRunComplete()
    {
        Directory.CreateDirectory(AppFolder); // ensure folder exists
        File.WriteAllText(FirstRunFlagPath, "Initialized");
    }
}
