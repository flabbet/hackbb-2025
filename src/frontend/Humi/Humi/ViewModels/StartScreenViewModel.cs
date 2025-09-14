using System;
using System.Timers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private readonly GraphDataLoaderUtility _graphLoader =  new GraphDataLoaderUtility();
    [ObservableProperty] public GraphData data;
    [ObservableProperty] public string choosenDate;
    [ObservableProperty] public ObservableCollection<string> availableDates = [];
    [ObservableProperty] public ISeries[] series;
    
    public RelayCommand StartAnalysisCommand { get; }
    

    private readonly Timer _timer;
    private TimeSpan _elapsedTime;

    public StartScreenViewModel()
    {
        Data = _graphLoader.LoadFiles("../../../../../../../data/");
        foreach (var key in Data.Keys)
        {
            AvailableDates.Add(key);
        }
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
                Labels = ["Neutralny", "Szczęśliwy", "Przerażony", "Zły", "Zaskoczony", "Smutnt"],
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
        var screenSelectorWindow = new Views.ScreenSelector();
        screenSelectorWindow.DataContext = new ScreenSelectorViewModel(screenSelectorWindow,
            OperatingSystem.IsMacOS() ? new MacOsScreenshotUtility() :
            OperatingSystem.IsLinux() ? new LinuxScreenshotUtility() : null);

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
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
        MeetupDuration = _elapsedTime.Hours == 0
            ? _elapsedTime.ToString(@"mm\:ss")
            : _elapsedTime.ToString(@"hh\:mm\:ss");
    }
}