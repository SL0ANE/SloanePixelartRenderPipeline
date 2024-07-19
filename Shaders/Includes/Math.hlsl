float multiStep(float value, float level, float minValue, float offset)
{
    if(level <= 1.0) return 1.0;
    
    float curLevel = value * level;
    float curOffset = floor(curLevel) / (level - 1.0);
    curLevel = curLevel + lerp(offset, 0.0, curOffset);
    float curLevelCache = curLevel;
    float downLevel = round(curLevel - 1.0);
    float upLevel = downLevel + 1.0;
    
    float curLevelFirst = curLevel - downLevel;
    float aaf = fwidth(curLevelFirst);
    
    curLevelFirst = downLevel >= 0.0 ? lerp(downLevel, upLevel, smoothstep(1.0 - aaf, 1.0, curLevelFirst)) : upLevel;
    
    downLevel = floor(curLevel);
    upLevel = downLevel + 1.0;
    
    float curLevelSecond = curLevel - downLevel;
    aaf = fwidth(curLevelSecond);
    
    curLevelSecond = upLevel < level ? lerp(downLevel, upLevel, smoothstep(1.0 - aaf, 1.0, curLevelSecond)) : downLevel;
    
    curLevel = min(curLevelFirst, curLevelSecond);
    
    curOffset = curLevel / (level - 1.0);
    curLevel += lerp(minValue, 1.0, curOffset);
    curLevel = curLevel / level;
    
    return curLevel;
}