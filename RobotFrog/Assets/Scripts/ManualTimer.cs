
public class ManualTimer {
    

    public float TimeRemaining = 1.0f;
    public float Duration = 1.0f;
    
    public void SetTime(float InDuration)
    {
        TimeRemaining = InDuration;
        Duration = InDuration;
    }

    public bool Tick(float DeltaTime)
    {
        TimeRemaining -= DeltaTime;
        if(TimeRemaining < 0.0f)
        {
            TimeRemaining += Duration;
            return true;
        }
        return false;
    }

}
