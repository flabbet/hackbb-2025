using System.Collections.Generic;
using Avalonia.Controls;

namespace Humi.Services;
public class SimpleScreensProvider : IScreensProvider
{
    public List<int> GetScreens(Window window)
    {
        var screens = window.Screens.All;
        List<int> result = [];
        for (int i = 0; i < screens.Count; ++i)
        {
            result.Add(i);
        }
        return result;
    }
}
