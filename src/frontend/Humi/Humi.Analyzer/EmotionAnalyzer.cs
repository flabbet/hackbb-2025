namespace Humi.Analyzer;

public class EmotionAnalyzer
{
    public System.Timers.Timer OutstandingEventTimer { get; private set; }
    public List<PersonEmotion> LatestData { get; private set; } = new List<PersonEmotion>();
    
    public event Action<OutstandingEvent> OnOutstandingEvent;

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

        LatestData.Add(newData);
    }

    private void ProcessOutstandingEvents()
    {
        List<PersonEmotion> dataFromLastInterval = GatherDataFromInterval(DateTime.Now - TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
        List<PersonEmotion> dataFromLatestInterval = GatherDataFromInterval(DateTime.Now - TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        if (dataFromLastInterval.Count == 0 && dataFromLatestInterval.Count == 0) return;
        
        bool handled = false;
        handled = ProcessEmotionTransitions(dataFromLastInterval, dataFromLatestInterval);
        if (handled) return;
    }
    
    private List<PersonEmotion> GatherDataFromInterval(DateTime startTime, TimeSpan duration)
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
    }
    
    private Emotion GetDominantEmotion(List<PersonEmotion> data)
    {
        if (data.Count == 0) return Emotion.Neutral;

        var emotionCounts = new Dictionary<Emotion, int>();
        foreach (var entry in data)
        {
            if (!emotionCounts.TryAdd(entry.DominantEmotion, 1))
            {
                emotionCounts[entry.DominantEmotion]++;
            }
        }

        return emotionCounts.OrderByDescending(e => e.Value).First().Key;
    }

    private bool SomeoneGotAngered(Emotion last, Emotion latest)
    {
        return last != Emotion.Angry && latest == Emotion.Angry;
    }
}

public struct OutstandingEvent
{
    public string EventText { get; set; }
    public Emotion NotificationEmotion { get; set; }
}