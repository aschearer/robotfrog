
public class ManualTimer {
    
    public bool IsLooping = true;
    public float TimeRemaining = 1.0f;
    public float Duration = 1.0f;
    
    public void SetTime(float InDuration)
    {
        TimeRemaining = InDuration;
        Duration = InDuration;
    }

    public bool Tick(float DeltaTime)
    {
        if(!IsTicking())
        {
            return false;
        }
        TimeRemaining -= DeltaTime;
        if(TimeRemaining < 0.0f)
        {
            if(IsLooping)
            {
                TimeRemaining += Duration;
            }
            return true;
        }
        return false;
    }

    public void Reset()
    {
        TimeRemaining = Duration;
    }

    public bool IsTicking()
    {
        return TimeRemaining >= 0.0f;
    }

}
