using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Humi.Analyzer;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

using Humi.Models;
namespace Humi.ViewModels;

public class StartScreenViewModel : ViewModelBase
{
    public RelayCommand StartAnalysisCommand { get; }
    
    private bool analysisStarted;
    private AssistantViewModel assistantViewModel;
    
    public EmotionAnalyzer Analyzer { get; } = new EmotionAnalyzer();
    public BackendWorker BackendWorker { get; } = new BackendWorker();

    public StartScreenViewModel()
    {
        StartAnalysisCommand = new RelayCommand(ShowScreenPicker);
        BackendWorker.DataReceived += Analyzer.ProcessEventRaw;
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
                Labels = ["Neutralny", "Szczęśliwy","Przerażony", "Zły","Zaskoczony", "Smutny"],
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

    private void ShowScreenPicker()
    {
        if (analysisStarted)
        {
            BackendWorker.StopBackend();
            Analyzer.Stop();
            return;
        };
        
        analysisStarted = true;
        Analyzer.Start();
        var screenSelectorWindow = new Views.ScreenSelector();
        screenSelectorWindow.DataContext = new ScreenSelectorViewModel(screenSelectorWindow,
            OperatingSystem.IsMacOS() ? new MacOsScreenshotUtility() :
            OperatingSystem.IsLinux() ? new LinuxScreenshotUtility() : null, Analyzer, BackendWorker);

        screenSelectorWindow.Topmost = true;

        if (App.Current.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.WindowState = WindowState.Minimized;
        }

        screenSelectorWindow.Show();
    }
}