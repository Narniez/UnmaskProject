[System.Flags]
public enum PlantProperties
{
    Normal = 0,
    Shade = 1 << 0,
    Sun = 1 << 1,
    Wet = 1 << 2,
    Dry = 1 << 3,
    Barrier = 1 << 4
}