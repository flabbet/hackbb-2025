using System;
using System.Collections.ObjectModel;
using System.Timers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humi.Analyzer;
using ExCSS;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using Humi.Models;

namespace Humi.ViewModels;

public partial class StartScreenViewModel : ViewModelBase
{
    
    private bool analysisStarted;
    private AssistantViewModel assistantViewModel;
    
    public EmotionAnalyzer Analyzer { get; } = new EmotionAnalyzer();
    public BackendWorker BackendWorker { get; } = new BackendWorker();
    public RelayCommand StartAnalysisCommand { get; }
    public ObservableCollection<string> PostAnalysisTips { get; } = new ObservableCollection<string>();

    private readonly Timer _timer;
    private TimeSpan _elapsedTime;

    public StartScreenViewModel()
    {
        BackendWorker.DataReceived += Analyzer.ProcessEventRaw;
        Analyzer.OnPersonCountChanged += count => NumberOfPeopleInMeetup = count;
        _timer = new Timer(1000);
        _timer.Elapsed += TimerElapsed;
        _elapsedTime = TimeSpan.Zero;

        StartAnalysisCommand = new RelayCommand(StartAnalysis);
    }

    [ObservableProperty] private bool _isMetupAnalysisActive = false;

    [ObservableProperty] private int _numberOfPeopleInMeetup = 0;

    [ObservableProperty] private string _meetupDuration;

    public ISeries[] Series { get; set; }
        = new ISeries[]
        {
            new LineSeries<int>
            {
                Fill = null,
                Stroke = new SolidColorPaint(new SKColor(101, 143, 100, 255)) { StrokeThickness = 4 },
                Values = new[] { 55, 69, 71, 83, 4, 90, 10 },
                GeometryFill = new SolidColorPaint(SKColors.White),
                GeometryStroke = new SolidColorPaint(new SKColor(101, 143, 100, 255)) { StrokeThickness = 4 }
            },
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
                MinStep = 20,
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
        IsMetupAnalysisActive = !IsMetupAnalysisActive;
        _timer.Start();
        _elapsedTime = TimeSpan.Zero;
        MeetupDuration = "00:00";
        ShowScreenPicker();
    } 
    
    private void ShowScreenPicker()
    {
        if (analysisStarted)
        {
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

        _timer.Start();
        _elapsedTime = TimeSpan.Zero;
        MeetupDuration = "00:00";
        screenSelectorWindow.Show();
    }

    [RelayCommand]
    private void StopAnalysis()
    {
        _timer.Stop();
        IsMetupAnalysisActive = !IsMetupAnalysisActive;
        BackendWorker.StopBackend();
        Analyzer.Stop();
        
        PostAnalysisTips.Clear();
        foreach (var tip in Analyzer.PostAnalysisEvents)
        {
            PostAnalysisTips.Add(tip.EventText);
        }
        
        analysisStarted = false;
        NumberOfPeopleInMeetup = 0;
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
        MeetupDuration = _elapsedTime.Hours == 0
            ? _elapsedTime.ToString(@"mm\:ss")
            : _elapsedTime.ToString(@"hh\:mm\:ss");
    }
}