using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Humi.Analyzer;

namespace Humi.ViewModels;

public partial class AssistantViewModel : ViewModelBase
{
    private bool isShown;

    private string notificationText =
        "Wygląda na to, że zespół jest w ponurych humorach, spróbuj poprowadzić to spotkanie w luźniejszej formie. Możesz również pochwalić za ostatnie sukcesy";

    public string NotificationText
    {
        get => notificationText;
        set => SetProperty(ref notificationText, value);
    }

    public bool IsShown
    {
        get => isShown;
        set => SetProperty(ref isShown, value);
    }

    public RelayCommand CloseCommand { get; set; }

    public EmotionAnalyzer Analyzer { get; } = new EmotionAnalyzer();

    private Window owningWindow;

    public AssistantViewModel(Window window)
    {
        owningWindow = window;
        Analyzer.OnOutstandingEvent += (e) => { Dispatcher.UIThread.Invoke(() => ShowNotification(e)); };

        CloseCommand = new RelayCommand(CloseNotification);

        Analyzer.Start();

        Dispatcher.UIThread.Post(() => ShowNotification(new OutstandingEvent()
        {
            EventText =
                "Witaj! Jestem Twoim asystentem do spraw nastroju w zespole. Będę Cię informować o nastrojach panujących w zespole oraz sugerować działania, które mogą poprawić atmosferę."
        }));

        StartBackend();
    }

    public void StartBackend()
    {
        var SelectedScreen = 0;
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
                    await Task.Delay(10); // Avoid busy waiting
                    continue;
                }

                string? line = await reader.ReadLineAsync();
                if (line != null)
                    Analyzer.ProcessEventRaw(line);
            }
        });
    }

    private void ShowNotification(OutstandingEvent e)
    {
        owningWindow.Width = 550;
        NotificationText = e.EventText;
        IsShown = true;
        DispatcherTimer.RunOnce(CloseNotification, TimeSpan.FromSeconds(15));
    }

    private void CloseNotification()
    {
        NotificationText = "";
        owningWindow.Width = 200;
        IsShown = false;
    }
}