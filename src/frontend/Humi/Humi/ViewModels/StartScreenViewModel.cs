using System;
using System.Timers;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExCSS;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;

namespace Humi.ViewModels;

public partial class StartScreenViewModel : ViewModelBase
{
    public RelayCommand StartAnalysisCommand { get; }
    
    private readonly Timer _timer;
    private TimeSpan _elapsedTime;
    public StartScreenViewModel()
    {
        _timer = new Timer(1000);
        _timer.Elapsed += TimerElapsed;
        _elapsedTime = TimeSpan.Zero;

        StartAnalysisCommand = new RelayCommand(StartAnalysis);
    }

    [ObservableProperty]
    private bool _isMetupAnalysisActive = false;

    [ObservableProperty]
    private int _numberOfPeopleInMeetup = 0;

    [ObservableProperty]
    private string _meetupDuration;

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
                Labels = ["Neutralny", "Szczęśliwy","Przerażony", "Zły","Zaskoczony", "Smutn"],
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

        IsMetupAnalysisActive = !IsMetupAnalysisActive;
        _timer.Start();
        _elapsedTime = TimeSpan.Zero;
        MeetupDuration = "00:00";
        assistantWindow.Show();
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