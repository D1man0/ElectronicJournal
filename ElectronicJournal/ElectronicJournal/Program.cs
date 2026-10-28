using Avalonia;
using System;

namespace ElectronicJournal.Desktop;

internal class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<ElectronicJournal.App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}