using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Humi.ViewModels;

namespace Humi.Views;

public partial class AssistantWindow : Window
{
    public AssistantWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        AlignToBottomRight();
        if (this.DataContext is MainWindowViewModel viewModel)
        {
            // Do something with viewModel
            viewModel.Initialize(this); // Example method
        }
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

    public void ScreenSelected(object? sender, SelectionChangedEventArgs e) {
        if (sender is ListBox listBox)
        {
            // Get the ViewModel from the DataContext of the ListBox
            if (listBox.DataContext is MainWindowViewModel vm)
            {
                // You now have access to the ViewModel
                Console.WriteLine(vm.SelectedScreen);
                vm.StartBackend();
            }
        }
    }
}