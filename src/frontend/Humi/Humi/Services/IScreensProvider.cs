using System.Collections.Generic;
using Avalonia.Controls;

namespace Humi.Services;

public interface IScreensProvider
{
    public List<int> GetScreens(Window window);
}
