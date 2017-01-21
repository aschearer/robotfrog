﻿
public class ManualTimer {
    

    public float TimeRemaining = 1.0f;
    public float Duration = 1.0f;

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