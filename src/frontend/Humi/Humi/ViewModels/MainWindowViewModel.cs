using System;
using System.Diagnostics;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Humi.Analyzer;

namespace Humi.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string notificationText = "No notifications";

    public string NotificationText
    {
        get => notificationText;
        set => SetProperty(ref notificationText, value);
    }

    public EmotionAnalyzer Analyzer { get; } = new EmotionAnalyzer();

    public MainWindowViewModel()
    {
        Analyzer.OnOutstandingEvent += (e) =>
        {
            Dispatcher.UIThread.Invoke(() => ShowNotification(e));
        };

        Analyzer.Start();

        StartBackend();
    }
    
    private void StartBackend()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = "run_backend.sh",
            WorkingDirectory = "/Users/flabbet/Git/Emotion-detection/src",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true
        };
        
        Process process = new Process
        {
            StartInfo = startInfo
        };
        
        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                Analyzer.ProcessEventRaw(args.Data);
            }
        };
        
        process.Start();
        process.BeginOutputReadLine();
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