using System.Linq;
using UnityEngine;

[CreateAssetMenu]
public class Plant : ScriptableObject
{
    public PlantProperties Property;
    public PestType[] EatenBy;
    public ProtectionData Protection;
    public Sprite Sprite;

    public bool CanBeEatedBy(PestType pestType) => EatenBy.Contains(pestType);
}
