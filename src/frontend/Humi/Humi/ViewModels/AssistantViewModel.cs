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
using Humi.Models;

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


    private Window owningWindow;
    

    public AssistantViewModel(Window window, int screenId, EmotionAnalyzer analyzer, BackendWorker worker)
    {
        owningWindow = window;
        analyzer.OnOutstandingEvent += (e) => { Dispatcher.UIThread.Invoke(() => ShowNotification(e)); };

        CloseCommand = new RelayCommand(CloseNotification);

        analyzer.Start();

        Dispatcher.UIThread.Post(() => ShowNotification(new OutstandingEvent()
        {
            EventText =
                "Witaj! Jestem Twoim asystentem do spraw nastroju w zespole. Będę Cię informować o nastrojach panujących w zespole oraz sugerować działania, które mogą poprawić atmosferę."
        }));

        worker.StartBackend(screenId);
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