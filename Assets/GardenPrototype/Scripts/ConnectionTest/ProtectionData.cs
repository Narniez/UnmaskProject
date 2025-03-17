using System;

[Serializable]
public struct ProtectionData
{
    public bool DoesProtect;

    [ConditionalHide(nameof(DoesProtect), false)]
    public ProtectionType Type;

    [ConditionalHide(nameof(DoesProtect), false)]
    public float EffectiveUnits;

    [ConditionalHide(nameof(DoesProtect), false)]
    public PestType EffectiveAgainst;
}