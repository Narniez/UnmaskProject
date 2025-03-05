using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileSpriteManager", menuName = "Scriptable Objects/TileSpriteManager")]
public class TileSpriteManager : ScriptableObject
{
    [System.Serializable]
    public class TileSprites
    {
        public TileType tileType;
        public Sprite strongConnectionSprite;
        public Sprite weakConnectionSprite;
        public Sprite badConnectionSprite;
        public Sprite defaultSprite;
    }

    public List<TileSprites> tileSprites = new List<TileSprites>();

    private Dictionary<TileType, TileSprites> spriteDictionary;

    private void OnEnable()
    {
        // Convert list to dictionary for quick lookup
        spriteDictionary = new Dictionary<TileType, TileSprites>();
        foreach (var tile in tileSprites)
        {
            spriteDictionary[tile.tileType] = tile;
        }
    }

    public Sprite GetSprite(TileType type, ConnectionStrength strength)
    {

        if (spriteDictionary.TryGetValue(type, out TileSprites sprites))
        {
            return strength switch
            {
                ConnectionStrength.Strong => sprites.strongConnectionSprite,
                ConnectionStrength.Weak => sprites.weakConnectionSprite,
                ConnectionStrength.Bad => sprites.badConnectionSprite,
                ConnectionStrength.Default => sprites.defaultSprite,
                _ => null
            };
        }
        return null;
    }
}
