using Avalonia.Media.Imaging;

namespace Humi.Models;

public interface IScreenshotUtility
{
    public Bitmap CaptureScreen(int screenId);
}