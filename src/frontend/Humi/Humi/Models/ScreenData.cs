using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace Humi.Models;

public class ScreenData
{
    public int ScreenId { get; set; }
    public Bitmap Preview { get; set; }
    public string Name { get; set; }
    public RelayCommand SelectScreenCommand { get; set; }
}