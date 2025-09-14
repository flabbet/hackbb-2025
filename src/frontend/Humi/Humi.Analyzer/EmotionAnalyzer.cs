namespace Humi.Analyzer;

public class EmotionAnalyzer
{
    public System.Timers.Timer OutstandingEventTimer { get; private set; }

    public Dictionary<int, List<PersonEmotion>> LatestData { get; private set; } =
        new Dictionary<int, List<PersonEmotion>>();
    
    public List<OutstandingEvent> PostAnalysisEvents { get; private set; } = new List<OutstandingEvent>();

    public event Action<OutstandingEvent> OnOutstandingEvent;
    public event Action<int> OnPersonCountChanged;
    public event Action<Emotion> EmotionCountChanged;

    private DateTime firstEntryTime;
    private bool initialEmotionProcessed = false;
    private int lastPersonCount = 0;

    public TimeSpan TimeSinceStart =>
        LatestData == null || LatestData.Count == 0 ? TimeSpan.Zero : DateTime.Now - firstEntryTime;


    public EmotionAnalyzer()
    {
        OutstandingEventTimer = new(TimeSpan.FromSeconds(5));
        OutstandingEventTimer.Elapsed += (sender, args) => ProcessOutstandingEvents();
    }

    public void Start()
    {
        OutstandingEventTimer.Start();
    }
    
    public void Stop()
    {
        OutstandingEventTimer.Stop();
        AnalyzeWholeData();
    }

    // personId:emotionString
    public void ProcessEventRaw(string eventInput)
    {
        if (string.IsNullOrEmpty(eventInput)) return;

        string[] split = eventInput.Split(':');
        if (split.Length != 2) return;
        if (!int.TryParse(split[0], out var id)) return;

        var emotion = split[1].Trim();

        if (string.IsNullOrEmpty(emotion)) return;

        if (!Enum.TryParse(emotion, true, out Emotion dominantEmotion)) return;

        PersonEmotion newData = new PersonEmotion
        {
            PersonId = id,
            DominantEmotion = dominantEmotion,
            Timestamp = DateTime.Now
        };

        if (LatestData.Count == 0) firstEntryTime = DateTime.Now;
        LatestData.TryAdd(id, new List<PersonEmotion>());
        LatestData[id].Add(newData);

        EmotionCountChanged?.Invoke(dominantEmotion);
    }

    private void ProcessOutstandingEvents()
    {
        bool handled = false;
        var dataFromLastInterval = GatherDataFromInterval(DateTime.Now - TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(3));
        if (dataFromLastInterval.Count != lastPersonCount)
        {
            OnPersonCountChanged?.Invoke(dataFromLastInterval.Count);
            lastPersonCount = dataFromLastInterval.Count;
        }
        
        if (TimeSinceStart < TimeSpan.FromSeconds(50) && !initialEmotionProcessed)
        {
            handled = ProcessInitialEmotions(LatestData);
            if (handled)
            {
                initialEmotionProcessed = true;
                return;
            }
        }

        handled = ProcessSuddenMoodChanges();
        if (handled) return;
    }
    
    private void AnalyzeWholeData()
    {
        if (LatestData.Count == 0) return;

        foreach (var (id, data) in LatestData)
        {
            var mostFrequentEmotion = GetDominantEmotion(data);
            if (mostFrequentEmotion is Emotion.Sad or Emotion.Fear)
            {
                PostAnalysisEvents.Add(new OutstandingEvent
                {
                    EventText =
                        $"Wygląda na to, że osoba {id} przez większość czasu była smutna i wystraszona, porozmawiaj z nim/nią, być może potrzebuje trochę czasu wolnego aby rozwiązać swoje sprawy. Na pewno pozytywnie to wpłynie na późniejszą produktywność i relacje.",
                    NotificationEmotion = mostFrequentEmotion
                });
            }
        }
        
        var dominantEmotion = GetDominantEmotionOfMajority(LatestData);
        if (dominantEmotion == Emotion.Happy)
        {
            PostAnalysisEvents.Add(new OutstandingEvent
            {
                EventText =
                    "Większość osób wydaje się być zadowolona, świetna robota! Utrzymuj pozytywną atmosferę i kontynuuj dobrą pracę.",
                NotificationEmotion = dominantEmotion
            });
        }
    }

    private Dictionary<int, PersonEmotion> GatherDataFromInterval(DateTime startTime, TimeSpan duration)
    {
        DateTime endTime = startTime + duration;
        Dictionary<int, PersonEmotion> result = new();
        
        foreach (var kvp in LatestData)
        {
            var recentEmotions = kvp.Value.Where(e => e.Timestamp >= startTime && e.Timestamp < endTime).ToList();
            if (recentEmotions.Count > 0)
            {
                // Get the most recent emotion in the interval
                var latestEmotion = recentEmotions.OrderByDescending(e => e.Timestamp).First();
                result[kvp.Key] = latestEmotion;
            }
        }
        
        return result;
    }


    private bool ProcessSuddenMoodChanges()
    {
        if (LatestData.Count == 0) return false;

        DateTime now = DateTime.Now;
        DateTime tenSecondsAgo = now - TimeSpan.FromSeconds(5);
        DateTime twentySecondsAgo = now - TimeSpan.FromSeconds(10);

        Dictionary<int, List<PersonEmotion>> lastTenSecondsData = new();
        Dictionary<int, List<PersonEmotion>> previousTenSecondsData = new();

        foreach (var kvp in LatestData)
        {
            var recentEmotions = kvp.Value.Where(e => e.Timestamp >= tenSecondsAgo).ToList();
            if (recentEmotions.Count > 0)
            {
                lastTenSecondsData[kvp.Key] = recentEmotions;
            }

            var earlierEmotions = kvp.Value.Where(e => e.Timestamp >= twentySecondsAgo && e.Timestamp < tenSecondsAgo)
                .ToList();
            if (earlierEmotions.Count > 0)
            {
                previousTenSecondsData[kvp.Key] = earlierEmotions;
            }
        }

        Emotion[] dominantLastTen = GetDominantEmotionsOfMajority(lastTenSecondsData).Take(2).ToArray();
        Emotion[] dominantPreviousTen = GetDominantEmotionsOfMajority(previousTenSecondsData).Take(2).ToArray();
        
        if (dominantLastTen.Length == 0 || dominantPreviousTen.Length == 0) return false;
        
        bool isSurprisedAndAngry = dominantLastTen.Contains(Emotion.Surprised) && dominantLastTen.Contains(Emotion.Angry);
        bool wasSurprisedAndAngry = dominantPreviousTen.Contains(Emotion.Surprised) && dominantPreviousTen.Contains(Emotion.Angry);

        if (isSurprisedAndAngry && !wasSurprisedAndAngry)
        {
            OnOutstandingEvent?.Invoke(new OutstandingEvent
            {
                EventText =
                    "Większość osób jest zdziwiona i niezadowolona, spróbuj doprecyzować nieścisłości i rozszerz kontekst jeżeli to możliwe.",
                NotificationEmotion = Emotion.Surprised
            });
            return true;
        }

        return false;
    }

    private bool ProcessInitialEmotions(Dictionary<int, List<PersonEmotion>> data)
    {
        Emotion dominantEmotion = GetDominantEmotionOfMajority(data);
        if (dominantEmotion == Emotion.Sad)
        {
            OnOutstandingEvent?.Invoke(new OutstandingEvent
            {
                EventText =
                    "Wygląda na to, że zespół jest w ponurych humorach, spróbuj poprowadzić to spotkanie w luźniejszej formie. Możesz również pochwalić za ostatnie sukcesy",
                NotificationEmotion = Emotion.Sad
            });
            
            return true;
        }
        
        if (dominantEmotion == Emotion.Happy)
        {
            OnOutstandingEvent?.Invoke(new OutstandingEvent
            {
                EventText =
                    "Większość osób wydaje się być zadowolona, świetna robota! Utrzymuj pozytywną atmosferę i kontynuuj dobrą pracę.",
                NotificationEmotion = Emotion.Sad
            });
            
            return true;
        }

        return false;
    }

    private Emotion GetDominantEmotionOfMajority(Dictionary<int, List<PersonEmotion>> data)
    {
        Dictionary<Emotion, int> emotionCounts = new();
        foreach (var personData in data.Values)
        {
            if (personData.Count == 0) continue;
            Emotion getDominant = GetDominantEmotion(personData);
            emotionCounts.TryAdd(getDominant, 0);

            emotionCounts[getDominant]++;
        }

        if (emotionCounts.Count == 0) return Emotion.Neutral;
        return emotionCounts.OrderByDescending(e => e.Value).First().Key;
    }

    private Emotion[] GetDominantEmotionsOfMajority(Dictionary<int, List<PersonEmotion>> data)
    {
        Dictionary<Emotion, int> emotionCounts = new();
        foreach (var personData in data.Values)
        {
            if (personData.Count == 0) continue;
            Emotion[] emotions = GetDominantEmotions(personData);

            foreach (var emotion in emotions)
            {
                emotionCounts.TryAdd(emotion, 0);
                emotionCounts[emotion]++;
            }
        }

        if (emotionCounts.Count == 0) return [Emotion.Neutral];
        return emotionCounts.OrderByDescending(e => e.Value).Select(e => e.Key).ToArray();
    }

    private Emotion GetDominantEmotion(List<PersonEmotion> data)
    {
        if (data == null || data.Count == 0) return Emotion.Neutral;

        Dictionary<Emotion, int> emotionCounts = new();
        foreach (var entry in data)
        {
            emotionCounts.TryAdd(entry.DominantEmotion, 0);
            emotionCounts[entry.DominantEmotion]++;
        }

        return emotionCounts.OrderByDescending(e => e.Value).First().Key;
    }

    private Emotion[] GetDominantEmotions(List<PersonEmotion> data)
    {
        if (data == null || data.Count == 0) return [Emotion.Neutral];

        Dictionary<Emotion, int> emotionCounts = new();
        foreach (var entry in data)
        {
            emotionCounts.TryAdd(entry.DominantEmotion, 0);
            emotionCounts[entry.DominantEmotion]++;
        }

        return emotionCounts.OrderByDescending(e => e.Value).Select(e => e.Key).ToArray();
    }
}

public struct OutstandingEvent
{
    public string EventText { get; set; }
    public Emotion NotificationEmotion { get; set; }
}