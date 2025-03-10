using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Desert,
    Water,
    Volcano,
    Mountain,
    Default,
}

public enum ConnectionStrength { Strong, Weak, Bad, Default }

public class HexTile : MonoBehaviour
{
    //ENUMS
    public TileType tileType = TileType.Default;
    public BiomeType currentBiome = BiomeType.None;
    [SerializeField] private ConnectionStrength strength = ConnectionStrength.Default;

    //SERIALIZABLES
    [SerializeField] private Vector2Int position;
    [SerializeField] private HexGrid grid;
    [SerializeField] private TileSpriteManager spriteManager;
    [SerializeField] private List<HexTile> neighbors = new List<HexTile>();

    //PRIVATE
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D neighborDetector;

    [SerializeField] private List<HexTile> supportedTiles = new List<HexTile>();

    private void Start()
    {
        neighborDetector = gameObject.AddComponent<CircleCollider2D>();
        neighborDetector.radius = grid.tileSize * 0.93f;
        neighborDetector.isTrigger = true;
        AssignNeighbors();
    }

    public void Initialize(Vector2Int position, HexGrid grid)
    {
        this.position = position;
        this.grid = grid;
    }

    public void SetBiome(BiomeType biome)
    {
        currentBiome = biome;

        //if (biome == BiomeType.Forest || biome == BiomeType.Oasis)
        //{
        //    HexTile waterTile = FindAvailableWaterTile();
        //    if (waterTile != null)
        //    {
        //        waterTile.AddSupportedTile(this); 
        //    }
        //}

        CheckBiomeRules();
        // UpdateNeighbors();
    }

    public void SetOriginalBiome()
    {
        if (currentBiome == BiomeType.Forest || currentBiome == BiomeType.Oasis)
        {
            foreach (HexTile neighbor in neighbors)
            {
                if (neighbor.tileType == TileType.Water)
                {
                    neighbor.RemoveSupportedTile(this);
                }
            }
        }
        currentBiome = BiomeType.None;
        UpdateVisual(ConnectionStrength.Strong);
        UpdateNeighbors();
    }

    public void UpdateVisual()
    {
        currentBiome = BiomeType.None;
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        switch (tileType)
        {
            case TileType.Desert:
                spriteRenderer.sprite = spriteManager.GetSprite(TileType.Desert, BiomeType.None, ConnectionStrength.Strong);
                break;
            case TileType.Water:
                spriteRenderer.sprite = spriteManager.GetSprite(TileType.Water, BiomeType.None, ConnectionStrength.Strong);
                break;
            case TileType.Volcano:
                spriteRenderer.sprite = spriteManager.GetSprite(TileType.Volcano, BiomeType.None, ConnectionStrength.Strong);
                break;
            case TileType.Mountain:
                spriteRenderer.sprite = spriteManager.GetSprite(TileType.Mountain, BiomeType.None, ConnectionStrength.Strong);
                break;
            case TileType.Default:
                spriteRenderer.sprite = spriteManager.GetSprite(TileType.Default, BiomeType.None, ConnectionStrength.Strong);
                break;
        }
    }

    public void UpdateVisual(ConnectionStrength _strength)
    {
        strength = _strength;
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = spriteManager.GetSprite(tileType, currentBiome, strength);
        //Debug.Log("Current strength for tile " + this.name + " : " + strength);
    }

    private void CheckBiomeRules()
    {
        strength = ConnectionStrength.Default;
        
        if (currentBiome == BiomeType.None)
        {
            UpdateVisual(ConnectionStrength.Strong);
           
            return;
        }

        switch (tileType)
        {

            case TileType.Desert:
                if (currentBiome == BiomeType.Cave)
                {
                    HexTile waterTile = FindAvailableWaterTile();

                    if(waterTile!= null) CheckWaterTileForCave(waterTile);
                    else
                    {
                        strength = ConnectionStrength.Strong;
                    }
                }
                else if (currentBiome == BiomeType.Oasis)
                {
                    HexTile waterTile = FindAvailableWaterTile();
                    if (waterTile != null) CheckWaterTileForOasis(waterTile);
                    //strength = waterTile != null ? ConnectionStrength.Strong : ConnectionStrength.Weak;
                }
                break;

            case TileType.Water:
                if (currentBiome == BiomeType.Glacier)
                {

                    strength = HasNeighbor(TileType.Desert) ? ConnectionStrength.Weak : ConnectionStrength.Strong;
                    if (GetNeighboursByBiome(BiomeType.Cave).Count != 0)
                    {
                        foreach(HexTile cave in GetNeighboursByBiome(BiomeType.Cave))
                        {
                            cave.UpdateVisual(ConnectionStrength.Strong);
                        }
                    }
    
                }
                break;

            case TileType.Mountain:
                if (currentBiome == BiomeType.Cave)
                {
                    HexTile waterTile = FindAvailableWaterTile();

                    if (waterTile != null) CheckWaterTileForCave(waterTile);
                    else
                    {
                        strength = ConnectionStrength.Strong;
                    }
                }
                break;

            case TileType.Default:
                if (currentBiome == BiomeType.Forest)
                {
                    Debug.Log("Forest time");
                    HexTile waterTile = FindAvailableWaterTile();
                    if (waterTile != null) CheckWaterTileForForest(waterTile);
                    else
                    {
                        strength = ConnectionStrength.Weak;
                    }
                    //strength = waterTile != null ? ConnectionStrength.Strong : ConnectionStrength.Weak;
                }
                break;
        }
        
        UpdateVisual(strength);
    }

    public List<HexTile> GetSupportedTiles()
    {
        return supportedTiles;
    }

    public void AddSupportedTile(HexTile tile)
    {
        if (!supportedTiles.Contains(tile))
        {
            supportedTiles.Add(tile);
        }
        UpdateSupportStrength();
    }

    public void RemoveSupportedTile(HexTile tile)
    {
        if (supportedTiles.Contains(tile))
        {
            supportedTiles.Remove(tile);
        }
        UpdateSupportStrength();
    }

    private void UpdateSupportStrength()
    {
        int forestCount = 0;
        int oasisCount = 0;

        // Count the number of forests and oases in supportedTiles
        foreach (HexTile tile in supportedTiles)
        {
            if (tile.currentBiome == BiomeType.Forest)
            {
                forestCount++;
            }
            else if (tile.currentBiome == BiomeType.Oasis)
            {
                oasisCount++;
            }
        }

        // Special case: Exactly 2 oases should remain strong
        if (oasisCount == 2)
        {
            foreach (HexTile tile in supportedTiles)
            {
                if (tile.currentBiome == BiomeType.Oasis)
                {
                    tile.UpdateVisual(ConnectionStrength.Strong);
                }
            }
        }
        else
        {
            foreach (HexTile tile in supportedTiles)
            {
                if (forestCount > 1 || oasisCount >= 2 || (forestCount == 1 && oasisCount >= 1))
                {
                    tile.UpdateVisual(ConnectionStrength.Weak);
                }
                else
                {
                    //tile.UpdateVisual(ConnectionStrength.Strong); 
                }
            }
        }
    }

    private HexTile FindAvailableWaterTile()
    {
        foreach (HexTile neighbor in neighbors)
        {
            if (neighbor.tileType == TileType.Water)
            {
                neighbor.AddSupportedTile(this);
                return neighbor;
            }
        }
        return null;
    }

    private void CheckWaterTileForForest(HexTile tileToCheck)
    {
        int supportedCount = 0;
        foreach (HexTile supportedTile in tileToCheck.supportedTiles)
        {
            supportedCount++;
            if (supportedTile != this && supportedTile.currentBiome == BiomeType.Forest || supportedTile.currentBiome == BiomeType.Oasis || supportedCount > 1)
            {
                strength = ConnectionStrength.Weak;
                supportedTile.UpdateVisual(ConnectionStrength.Weak);
            }

            else
            {
                strength = ConnectionStrength.Strong;
            }

        }

        Debug.Log("Stength from checking water tile: " + strength);
    }

    private void CheckWaterTileForOasis(HexTile tileToCheck)
    {
        int supportedOasisCount = 0;
        foreach (HexTile supportedTile in tileToCheck.supportedTiles)
        {
            if (supportedTile.currentBiome == BiomeType.Oasis)
            {
                supportedOasisCount++;
            }
            if (supportedTile != this && supportedTile.currentBiome == BiomeType.Forest || supportedOasisCount > 2)
            {
                strength = ConnectionStrength.Weak;
                supportedTile.UpdateVisual(ConnectionStrength.Weak);
            }

            else
            {
                strength = ConnectionStrength.Strong;
            }

        }

        Debug.Log("Stength from checking water tile: " + strength);
    }

    private void CheckWaterTileForCave(HexTile tileToCheck)
    {
        if(tileToCheck.currentBiome == BiomeType.Glacier)
        {
            strength = ConnectionStrength.Strong;
        }
        else
        {
            strength = ConnectionStrength.Weak;
        }
    }

    private bool HasNeighbor(TileType type)
    {
        foreach (HexTile neighbor in neighbors)
        {
            if (neighbor.tileType == type)
            {
                return true;
            }
        }
        return false;
    }


    private List<HexTile> GetNeighboursByBiome(BiomeType biome)
    {
        List<HexTile> caveTiles = new List<HexTile> ();
        foreach (HexTile neighbor in neighbors)
        {
            if(neighbor.currentBiome == biome)
            {
                caveTiles.Add(neighbor);
            }
        }
        return caveTiles;
    }

    public void AssignNeighbors()
    {
        // Detect all colliders within the circle
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, neighborDetector.radius);

        foreach (var hitCollider in hitColliders)
        {
            HexTile neighbor = hitCollider.GetComponent<HexTile>();
            if (neighbor != null && neighbor != this) // Exclude self
            {
                neighbors.Add(neighbor);
            }
        }
        // Disable the collider after assigning neighbors
        neighborDetector.enabled = false;
    }

    private void UpdateNeighbors()
    {
        foreach (HexTile neighbor in neighbors)
        {
            neighbor.CheckBiomeRules();
        }
    }

    public TileType GetTileType()
    {
        return tileType;
    }

    public List<HexTile> GetNeighbors()
    {
        return neighbors;
    }
    public ConnectionStrength GetStrength()
    {
        return strength;
    }

    public void UpdateStrength(ConnectionStrength _strength)
    {
        strength = _strength;
    }
}
