using Assets.GardenPrototype.Scripts.ConnectionTest;
using System.Collections.Generic;
using UnityEngine;

public class PropertySpriteLookupTable : InstantiateOncePersistentBase<PropertySpriteLookupTable>
{
    [SerializeField] private PropertySpritePair[] _propertySpritePairs;
    //[SerializeField] private Sprite _invalidPropertySprite;

    private Dictionary<PlantProperties, Sprite> _lookupTable;

    private void Awake()
    {
        _lookupTable = new();
        foreach (PropertySpritePair pair in _propertySpritePairs)
        {
            _lookupTable.Add(pair.Property, pair.Sprite);
        }
    }

    public Sprite FromProperty(PlantProperties property)
    {
        if (_lookupTable.TryGetValue(property, out Sprite sprite))
        {
            return sprite;
        }

        return null;
    }
}
