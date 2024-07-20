float multiStep(float value, float level, float minValue, float offset)
{
    if(level <= 1.0) return 1.0;
    
    float curLevel = value * level;
    float curOffset = floor(curLevel) / (level - 1.0);
    curLevel = floor(curLevel + lerp(offset, 0.0, curOffset));
    
    curOffset = curLevel / (level - 1.0);
    curLevel += lerp(minValue, 1.0, curOffset);
    curLevel = curLevel / level;
    
    return min(curLevel, 1.0);
}