namespace Humi.Analyzer;

public class EmotionAnalyzer
{
    public System.Timers.Timer OutstandingEventTimer { get; private set; }
    public Dictionary<int, List<PersonEmotion>> LatestData { get; private set; } = new Dictionary<int, List<PersonEmotion>>();
    
    public event Action<OutstandingEvent> OnOutstandingEvent;

    private DateTime firstEntryTime;

    public TimeSpan TimeSinceStart => LatestData == null || LatestData.Count == 0 ? TimeSpan.Zero : DateTime.Now - firstEntryTime;
    

    public EmotionAnalyzer()
    {
        OutstandingEventTimer = new(TimeSpan.FromSeconds(5));
        OutstandingEventTimer.Elapsed += (sender, args) => ProcessOutstandingEvents();
    }

    public void Start()
    {
        OutstandingEventTimer.Start();
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
    }

    private void ProcessOutstandingEvents()
    {
        bool handled = false;
        if (TimeSinceStart < TimeSpan.FromSeconds(50))
        {
            handled = ProcessInitialEmotions(LatestData);
            if (handled) return;
        }
    }
    
    /*private List<PersonEmotion> GatherDataFromInterval(DateTime startTime, TimeSpan duration)
    {
        DateTime endTime = startTime + duration;
        return LatestData.Where(e => e.Timestamp >= startTime && e.Timestamp <= endTime).ToList();
    }
    
    private bool ProcessEmotionTransitions(List<PersonEmotion> lastIntervalData, List<PersonEmotion> latestIntervalData)
    {
        Emotion dominantEmotionLast = GetDominantEmotion(lastIntervalData);
        Emotion dominantEmotionLatest = GetDominantEmotion(latestIntervalData);
        
        if(SomeoneGotAngered(dominantEmotionLast, dominantEmotionLatest))
        {
            OnOutstandingEvent?.Invoke(new OutstandingEvent
            {
                EventText = "Someone is angry. Run away!",
                NotificationEmotion = Emotion.Angry
            });
            return true;
        }
        
        return false;
    }*/
    
    private bool ProcessInitialEmotions(Dictionary<int, List<PersonEmotion>> data)
    {
        Emotion dominantEmotion = GetDominantEmotionOfMajority(data);
        if (dominantEmotion == Emotion.Sad)
        {
            OnOutstandingEvent?.Invoke(new OutstandingEvent
            {
                EventText = "Wygląda na to, że zespół jest w ponurych humorach, spróbuj poprowadzić to spotkanie w luźniejszej formie. Możesz również pochwalić za ostatnie sukcesy",
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
}

public struct OutstandingEvent
{
    public string EventText { get; set; }
    public Emotion NotificationEmotion { get; set; }
}