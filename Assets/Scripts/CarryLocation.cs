
using System;

[Flags]
public enum CarryLocation
{
    None = 0,
    MainHand = 1,
    OffHand = 2,
    Waist = 4,
    Back = 8
}

public static class CarryLocationExtensions
{
    /*public static CarryLocation AddSlot(this CarryLocation self, CarryLocation other)
    {
        return self | other;
    }

    public static CarryLocation RemoveSlot(this CarryLocation self, CarryLocation other) 
    {
        return self & ~flag;
    }*/
        
    public static bool HasFlag(this CarryLocation self, CarryLocation flag)
    {
        return (self & flag) == flag;
    }

}