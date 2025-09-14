using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
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

using GraphData = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>;

public partial class StartScreenViewModel : ViewModelBase
{
    private bool analysisStarted;
    private string summary = "Podsumowanie pojawi się tutaj po zakończeniu analizy.";
    private AssistantViewModel assistantViewModel;
    
    public EmotionAnalyzer Analyzer { get; } = new EmotionAnalyzer();
    public BackendWorker BackendWorker { get; } = new BackendWorker();
    public RelayCommand StartAnalysisCommand { get; }
    public ObservableCollection<string> PostAnalysisTips { get; } = new ObservableCollection<string>();
    
    public string Summary
    {
        get => summary;
        set => SetProperty(ref summary, value);
    }
    
    private readonly GraphDataLoaderUtility _graphLoader =  new GraphDataLoaderUtility();
    [ObservableProperty] public GraphData data;
    [ObservableProperty] public string choosenDate;
    [ObservableProperty] public ObservableCollection<string> availableDates = [];
    [ObservableProperty] public ISeries[] series;
    

    private readonly Timer _timer;
    private TimeSpan _elapsedTime;

    public StartScreenViewModel()
    {
        BackendWorker.DataReceived += Analyzer.ProcessEventRaw;
        Analyzer.OnPersonCountChanged += count => NumberOfPeopleInMeetup = count;
        Data = _graphLoader.LoadFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Humi", "data"));
        foreach (var key in Data.Keys)
        {
            AvailableDates.Add(key);
        }

        if (AvailableDates.Count > 0)
        {
            ChoosenDate = AvailableDates.FirstOrDefault();
            Series = new ISeries[]
            {
                new LineSeries<int>
                {
                    Values = Data[ChoosenDate],
                    Fill = null,
                    Stroke = new SolidColorPaint(new SKColor(101, 143, 100, 255)) { StrokeThickness = 4 },
                    GeometryFill = new SolidColorPaint(SKColors.White),
                    GeometryStroke = new SolidColorPaint(new SKColor(101, 143, 100, 255)) { StrokeThickness = 4 }
                }
            };
        }

        _timer = new Timer(1000);
        _timer.Elapsed += TimerElapsed;
        _elapsedTime = TimeSpan.Zero;

        StartAnalysisCommand = new RelayCommand(StartAnalysis);
    }

    [ObservableProperty] private bool _isMetupAnalysisActive = false;

    [ObservableProperty] private int _numberOfPeopleInMeetup = 0;

    [ObservableProperty] private string _meetupDuration;

    partial void OnChoosenDateChanged(string oldValue, string newValue)
    {
        Series = new ISeries[]
        {
            new LineSeries<int>
            {
                Values = Data[ChoosenDate],
                Fill = null,
                Stroke = new SolidColorPaint(new SKColor(101, 143, 100, 255)) { StrokeThickness = 4 },
                GeometryFill = new SolidColorPaint(SKColors.White),
                GeometryStroke = new SolidColorPaint(new SKColor(101, 143, 100, 255)) { StrokeThickness = 4 }
            }
        };
    } 
    
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
                MinStep = 10,
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
        BackendWorker.SummaryReceived += (summary) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Summary = summary;
            });
        };
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
        ShowSummaryScreen();
    }
    
    private void ShowSummaryScreen()
    {
        var summaryScreen = new Views.SummaryScreen();
        summaryScreen.DataContext = new ViewModels.SummaryScreenViewModel();

        summaryScreen.Topmost = true;

        if (App.Current.ApplicationLifetime is
            Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow.WindowState = WindowState.Minimized;
        }
        
        summaryScreen.Show();
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
        MeetupDuration = _elapsedTime.Hours == 0
            ? _elapsedTime.ToString(@"mm\:ss")
            : _elapsedTime.ToString(@"hh\:mm\:ss");
    }
}