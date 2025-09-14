using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humi.Models;

namespace Humi.ViewModels;

using GraphData = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>;

public partial class SummaryScreenViewModel : ViewModelBase
{
    private readonly GraphDataLoaderUtility _graphLoader = new GraphDataLoaderUtility();
    [ObservableProperty] public GraphData data;
    [ObservableProperty] public string choosenDate;
    [ObservableProperty] public ObservableCollection<string> availableDates = [];
    [ObservableProperty] public int neutralPercent = 0;
    [ObservableProperty] public int happyPercent = 0;
    [ObservableProperty] public int frightenedPercent = 0;
    [ObservableProperty] public int sadPercent = 0;
    [ObservableProperty] public int suprisedPercent = 0;
    [ObservableProperty] public int angryPercent = 0;
    [ObservableProperty] public string topValueName = "";
    [ObservableProperty] public string secondTopValueName = "";
    [ObservableProperty] public string topText = "";

    private readonly List<string> emotionNamesAlternatives = new List<string>
    {
        "neutralność",
        "radość",
        "przerażenie",
        "smutek",
        "zdziwienie",
        "złość"
    };


    public SummaryScreenViewModel()
    {
        Data = _graphLoader.LoadFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Humi", "data"));
        ChoosenDate = Data.Keys.First();

        int sum = Data[ChoosenDate].Sum();
        neutralPercent = (int)Math.Round((double)Data[ChoosenDate][0] / sum * 100);
        happyPercent = (int)Math.Round((double)Data[ChoosenDate][1] / sum * 100);
        frightenedPercent = (int)Math.Round((double)Data[ChoosenDate][2] / sum * 100);
        sadPercent = (int)Math.Round((double)Data[ChoosenDate][3] / sum * 100);
        suprisedPercent = (int)Math.Round((double)Data[ChoosenDate][4] / sum * 100);
        angryPercent = (int)Math.Round((double)Data[ChoosenDate][5] / sum * 100);

        var top2Indexes = Data[ChoosenDate]
            .Select((value, index) => new { value, index })
            .OrderByDescending(x => x.value)
            .Take(2)
            .Select(x => x.index)
            .ToArray();

        topValueName = emotionNamesAlternatives[top2Indexes[0]];
        secondTopValueName = emotionNamesAlternatives[top2Indexes[1]];

        if (top2Indexes.Contains(0) || top2Indexes.Contains(1))
        {
            topText = "Twoje spotkanie zakończyło się spokojnie.";
        }
        else
        {
            topText = "Twoje spotkanie zakończyło się nie spokojnie.";
        }
    }
}