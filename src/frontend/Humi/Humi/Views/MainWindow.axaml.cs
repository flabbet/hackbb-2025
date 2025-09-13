using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Humi.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        AlignToBottomRight();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        AlignToBottomRight();
    }

    public void AlignToBottomRight()
    {
        PixelSize screenSize = Screens.Primary.Bounds.Size;
        PixelSize windowSize = PixelSize.FromSize(ClientSize, Screens.Primary.Scaling);

        Position = new PixelPoint(
            screenSize.Width - windowSize.Width,
            screenSize.Height - windowSize.Height);
    }
}