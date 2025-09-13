using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Humi.Analyzer;
using Humi.Services;

namespace Humi.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string notificationText = "No notifications";

    [ObservableProperty] private ObservableCollection<int> _emotions = [];

    [ObservableProperty] private int? _selectedScreen;

    public string NotificationText
    {
        get => notificationText;
        set => SetProperty(ref notificationText, value);
    }

    public EmotionAnalyzer Analyzer { get; } = new EmotionAnalyzer();

    readonly private SimpleScreensProvider _screensProvider = new();

    public MainWindowViewModel()
    {
        Analyzer.OnOutstandingEvent += (e) => { Dispatcher.UIThread.Invoke(() => ShowNotification(e)); };

        Analyzer.Start();
    }

    public void Initialize(Window window)
    {
        var screensId = _screensProvider.GetScreens(window);
        foreach (var screenId in screensId)
        {
            Emotions.Add(screenId);
        }
    }

    public void StartBackend()
    {
        Console.Write(SelectedScreen + 1);
        var arguments = $"backend/run_backend.sh analyze {SelectedScreen + 1}";
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = arguments,
            WorkingDirectory = "../../../../../..",
            UseShellExecute = true,
            CreateNoWindow = true,
        };

        Process process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();

        RunPipeReader();
    }

    private void RunPipeReader()
    {
        string pipePath = "/tmp/emotions_feed";


        // Read asynchronously in background
        _ = Task.Run(async () =>
        {
            using var fs = new FileStream(pipePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096,
                useAsync: true);
            using var reader = new StreamReader(fs, Encoding.UTF8);
            Console.WriteLine("Listening to pipe");
            while (true)
            {
                if (reader.EndOfStream)
                {
                    await Task.Delay(50); // Avoid busy waiting
                    continue;
                }

                string? line = await reader.ReadLineAsync();
            }
        });
    }

    private void ShowNotification(OutstandingEvent e)
    {
        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.Width = 550;
            NotificationText = e.EventText;
            DispatcherTimer.RunOnce(() =>
            {
                NotificationText = "";
                desktop.MainWindow.Width = 200;
            }, TimeSpan.FromSeconds(5));
        }
    }
}