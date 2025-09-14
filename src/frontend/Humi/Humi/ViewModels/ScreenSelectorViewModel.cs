using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Humi.Models;
using Humi.Views;

namespace Humi.ViewModels;

public class ScreenSelectorViewModel : ViewModelBase
{
    public ObservableCollection<ScreenData> Screens { get; } = new ObservableCollection<ScreenData>();


    private Window owningWindow;
    public ScreenSelectorViewModel(Window window, IScreenshotUtility screenshotUtility)
    {
        owningWindow = window;
        for (int i = 0; i < window.Screens.ScreenCount; i++)
        {
            int handle = (int?)window.Screens.All[i].TryGetPlatformHandle()?.Handle ?? 0;
            var screenData = new ScreenData
            {
                ScreenId = i,
                Preview = screenshotUtility.CaptureScreen(handle),
                Name = window.Screens.All[i].DisplayName,
                SelectScreenCommand = new RelayCommand(() => SelectScreen(i))
            };
            
            Screens.Add(screenData);
        }
    }
    
    private void SelectScreen(int id)
    {
        AssistantWindow assistantWindow = new AssistantWindow();
        assistantWindow.DataContext = new AssistantViewModel(assistantWindow, id);
        
        owningWindow.Close();
        
        assistantWindow.Show();
    }
}