using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ElectronicJournal.Services;
using ElectronicJournal.ViewModels;
using ElectronicJournal.Views;

namespace ElectronicJournal;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var dbService = new DatabaseService();
            dbService.Initialize();

            var mainWindow = new MainWindow();

            mainWindow.DataContext = new MainWindowViewModel(dbService);

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}