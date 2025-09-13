namespace Humi.Analyzer;

public struct PersonEmotion
{
    public int PersonId { get; set; }
    public Emotion DominantEmotion { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum Emotion
{
    Angry = 1,
    Disgust = 2,
    Fear = 3,
    Happy = 4,
    Neutral = 5,
    Sad = 6,
    Surprise = 7
}