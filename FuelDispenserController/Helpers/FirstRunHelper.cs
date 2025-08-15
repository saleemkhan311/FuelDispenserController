using System;
using System.IO;

namespace FuelDispenserController.Core.Helpers;

public static class FirstRunHelper
{
    private static readonly string DbFolder = @"C:\Database"; // database folder
    private static readonly string FirstRunFlagPath = Path.Combine(DbFolder, "first_run.flag");

    public static bool IsFirstRun()
    {
        return !File.Exists(FirstRunFlagPath);
    }

    public static void MarkFirstRunComplete()
    {
       
        Directory.CreateDirectory(DbFolder); // make sure the folder exists
        File.WriteAllText(FirstRunFlagPath, "Initialized");
    }
}
