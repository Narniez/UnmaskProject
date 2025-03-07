using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileSpriteManager", menuName = "Scriptable Objects/TileSpriteManager")]
public class TileSpriteManager : ScriptableObject
{
    [System.Serializable]
    public class BiomeSprites
    {
        public BiomeType biomeType; // Biome type (e.g., Cave, Oasis)
        public Sprite strongConnectionSprite;
        public Sprite weakConnectionSprite;
        public Sprite badConnectionSprite;
        public Sprite defaultSprite;
    }

    [System.Serializable]
    public class TileSprites
    {
        public TileType tileType; // Tile type (e.g., Desert, Mountain)
        public List<BiomeSprites> biomeSprites; // List of biome-specific sprites
    }

    public List<TileSprites> tileSprites = new List<TileSprites>();

    private Dictionary<TileType, Dictionary<BiomeType, BiomeSprites>> spriteDictionary;

    private void OnEnable()
    {
        // Convert list to nested dictionary for quick lookup
        spriteDictionary = new Dictionary<TileType, Dictionary<BiomeType, BiomeSprites>>();
        foreach (var tile in tileSprites)
        {
            var biomeDict = new Dictionary<BiomeType, BiomeSprites>();
            foreach (var biome in tile.biomeSprites)
            {
                biomeDict[biome.biomeType] = biome;
            }
            spriteDictionary[tile.tileType] = biomeDict;
        }
    }

    public Sprite GetSprite(TileType tileType, BiomeType biomeType, ConnectionStrength strength)
    {
        if (spriteDictionary.TryGetValue(tileType, out var biomeDict))
        {
            if (biomeDict.TryGetValue(biomeType, out var biomeSprites))
            {
                return strength switch
                {
                    ConnectionStrength.Strong => biomeSprites.strongConnectionSprite,
                    ConnectionStrength.Weak => biomeSprites.weakConnectionSprite,
                    ConnectionStrength.Bad => biomeSprites.badConnectionSprite,
                    ConnectionStrength.Default => biomeSprites.defaultSprite,
                    _ => null
                };
            }
        }
        return null;
    }
}
