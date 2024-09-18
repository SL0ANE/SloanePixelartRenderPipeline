float multiStep(float value, float level, float minValue, float offset)
{
    if(level <= 1.0) return 1.0;
    
    float curLevel = value * level;
    curLevel = floor(curLevel + offset);
    
    float curOffset = curLevel / (level - 1.0);
    curLevel += lerp(minValue, 1.0, curOffset);
    curLevel = curLevel / level;
    
    return saturate(curLevel);
}