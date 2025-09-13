using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;

namespace Humi.ViewModels;

public class StartScreenViewModel : ViewModelBase
{
    public RelayCommand StartAnalysisCommand { get; }
    
    public StartScreenViewModel()
    {
        StartAnalysisCommand = new RelayCommand(StartAnalysis);
    }

    private void StartAnalysis()
    {
        var assistantWindow = new Views.AssistantWindow();
        assistantWindow.DataContext = new AssistantViewModel(assistantWindow);
        
        assistantWindow.Topmost = true;
        
        if (App.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.WindowState = WindowState.Minimized;
        }
        
        assistantWindow.Show();
    }
}