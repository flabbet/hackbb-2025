using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Humi.Models;

namespace Humi.ViewModels;

public class StartScreenViewModel : ViewModelBase
{
    public RelayCommand StartAnalysisCommand { get; }
    
    public StartScreenViewModel()
    {
        StartAnalysisCommand = new RelayCommand(ShowScreenPicker);
    }
    
    private void ShowScreenPicker()
    {
        var screenSelectorWindow = new Views.ScreenSelector();
        screenSelectorWindow.DataContext = new ScreenSelectorViewModel(screenSelectorWindow, OperatingSystem.IsMacOS() ? new MacOsScreenshotUtility() : null);
        
        screenSelectorWindow.Topmost = true;
        
        if (App.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.WindowState = WindowState.Minimized;
        }
        
        screenSelectorWindow.Show();
    }
}