using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Humi.Analyzer;
using Humi.Models;
using Humi.Views;

namespace Humi.ViewModels;

public class ScreenSelectorViewModel : ViewModelBase
{
    public ObservableCollection<ScreenData> Screens { get; } = new ObservableCollection<ScreenData>();
    
    public EmotionAnalyzer Analyzer { get; }
    private BackendWorker worker;


    private Window owningWindow;
    public ScreenSelectorViewModel(Window window, IScreenshotUtility screenshotUtility, EmotionAnalyzer analyzer, BackendWorker worker)
    {
        owningWindow = window;
        Analyzer = analyzer;
        this.worker = worker;
        for (int i = 0; i < window.Screens.ScreenCount; i++)
        {
            int handle = (int?)window.Screens.All[i].TryGetPlatformHandle()?.Handle ?? 0;

            var iCopy = System.OperatingSystem.IsLinux() ? i + 1 : i;
            var screenshotHandle = System.OperatingSystem.IsMacOS() ? handle : i;
            var screenData = new ScreenData
            {
                ScreenId = i,
                Preview = screenshotUtility.CaptureScreen(screenshotHandle),
                Name = window.Screens.All[i].DisplayName,
                SelectScreenCommand = new RelayCommand(() => SelectScreen(iCopy))
            };
            
            Screens.Add(screenData);
        }
    }
    
    private void SelectScreen(int id)
    {
        AssistantWindow assistantWindow = new AssistantWindow();
        assistantWindow.DataContext = new AssistantViewModel(assistantWindow, id, Analyzer, worker);
        
        owningWindow.Close();
        
        assistantWindow.Show();
    }
}