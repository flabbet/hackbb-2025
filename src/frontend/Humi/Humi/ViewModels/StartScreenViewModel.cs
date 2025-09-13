using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

namespace Humi.ViewModels;

public class StartScreenViewModel : ViewModelBase
{
    public RelayCommand StartAnalysisCommand { get; }
    
    public StartScreenViewModel()
    {
        StartAnalysisCommand = new RelayCommand(StartAnalysis);
    }

        public ISeries[] Series { get; set; }
            = new ISeries[] { new LineSeries<int> {
                Fill = null,
                Stroke = new SolidColorPaint(new SKColor(101, 143, 100, 255)) { StrokeThickness = 4 },
                Values = new[] { 55, 69, 71, 83, 4, 90, 10 },
                GeometryFill = new SolidColorPaint(SKColors.White),
                GeometryStroke = new SolidColorPaint(new SKColor(101, 143, 100, 255)) { StrokeThickness = 4 }
                    } ,
                };

        public Axis[] XAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    Labels = ["Neutralny", "Szczęśliwy","Przerażony", "Zły","Zaskoczony", "Smutnt"],
                    LabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255, 178)),
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 2,
                        PathEffect = new DashEffect(new float[] { 2, 2 })
                    }
                }
            };

        public Axis[] YAxes { get; set; }
            = new Axis[]
            {
                new Axis
                {
                    MinStep=20,
                    LabelsPaint = new SolidColorPaint(new SKColor(255, 255, 255, 178)),
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray) 
                    {
                        StrokeThickness = 2, 
                        PathEffect = new DashEffect(new float[] { 2, 2 })
                    }
                }
            };

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