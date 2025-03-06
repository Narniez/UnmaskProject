using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
    private ConnectionStrength strength = ConnectionStrength.Default;

    //SERIALIZABLES
    [SerializeField] private Vector2Int position;
    [SerializeField] private HexGrid grid;
    [SerializeField] private TileSpriteManager spriteManager;
    [SerializeField] private List<HexTile> neighbors = new List<HexTile>();

    //PRIVATE
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D neighborDetector;

    private HexTile supportedTile = null;

    [SerializeField] private List<HexTile> supportedOasisTiles = new List<HexTile>();
    [SerializeField] int supportedOasisCount = 0;

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
        CheckBiomeRules();
        UpdateNeighbors();
    }

    public void SetOriginalBiome()
    {
        currentBiome = BiomeType.None;
        if (tileType == TileType.Default)
        {
            foreach (HexTile neighbor in neighbors)
            {
                // Check if the neighbor is a Water tile and if its supportedTile is the same as the Default tile
                if (neighbor.tileType == TileType.Water && neighbor.GetSupportedForest() == this)
                {
                    // Call RemoveSupportedForest on the Water tile
                    neighbor.RemoveSupportedForest();
                }
            }
        }
        if (tileType == TileType.Desert)
        {
            //Debug.Log(neighbors.Count);
            foreach (HexTile neighbor in neighbors)
            {
               // Debug.Log($"{neighbor.tileType}");
                if (neighbor.tileType == TileType.Water)
                {
                    neighbor.DecreaseOasisSupport();
                }
            }
        }

        UpdateVisual();
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

    private void UpdateVisual(ConnectionStrength strength)
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = spriteManager.GetSprite(tileType, currentBiome, strength);
    }

    private void CheckBiomeRules()
    {
        strength = ConnectionStrength.Default;
        if (currentBiome == BiomeType.None)
        {
            return;
        }

        Debug.Log("CALLED FOR TILE: " + name);
        switch (tileType)
        {
            case TileType.Desert:

                if (currentBiome == BiomeType.Cave)
                {
                    strength = HasNeighbor(TileType.Water) ? ConnectionStrength.Weak : ConnectionStrength.Strong;
                }
                else if (currentBiome == BiomeType.Oasis)
                {
                    HexTile waterTile = FindAvailableWaterTileForOasis();
                    if (waterTile != null)
                    {
                        waterTile.IncreaseOasisSupport();
                        strength = ConnectionStrength.Strong;
                    }
                    else
                    {
                        strength = ConnectionStrength.Weak;
                    }
                }
                break;

            case TileType.Water:
                if (currentBiome == BiomeType.Glacier)
                {
                    strength = HasNeighbor(TileType.Desert) ? ConnectionStrength.Weak : ConnectionStrength.Strong;
                }
                break;

            case TileType.Mountain:
                if (currentBiome == BiomeType.Cave)
                {
                    strength = HasNeighbor(TileType.Water) ? ConnectionStrength.Weak : ConnectionStrength.Strong;
                }
                break;

            case TileType.Default:
                if (currentBiome == BiomeType.Forest)
                {
                    HexTile waterTile = FindAvailableWaterTileForForest();
                    if (waterTile != null)
                    {
                        waterTile.SetSupportingForest(this);
                        strength = ConnectionStrength.Strong;
                    }
                    else
                    {
                        strength = ConnectionStrength.Weak;
                    }
                }
                break;
        }

        UpdateVisual(strength);
    }

    private HexTile FindAvailableWaterTileForForest()
    {
        foreach (HexTile neighbor in neighbors)
        {
            if (neighbor.tileType == TileType.Water && neighbor.GetSupportedForest() == null && neighbor.GetSupportedOasesCount() == 0)
            {
                return neighbor;
            }
        }
        return null;
    }

    private HexTile FindAvailableWaterTileForOasis()
    {
        foreach (HexTile neighbor in neighbors)
        {
            if (neighbor.tileType == TileType.Water && neighbor.GetSupportedOasesCount() < 2 && neighbor.GetSupportedForest() == null)
            {
                return neighbor;
            }
        }
        return null;
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

    public HexTile GetSupportedForest()
    {
        return supportedTile;
    }

    public int GetSupportedOasesCount()
    {
        return supportedOasisTiles.Count;
    }

    public void SetSupportingForest(HexTile _supportedTile)
    {
        supportedTile = _supportedTile;
        //supportedTile = supportedForest;
    }

    public void RemoveSupportedForest()
    {
        supportedTile = null;
    }


    private void AddSupportedOasisTile(HexTile _supportedTile)
    {
        supportedOasisTiles.Add(_supportedTile);
    }

    private void RemoveSupportedoasisTile(HexTile _supportedTile)
    {
        supportedOasisTiles.Remove(_supportedTile);
    }

    public void IncreaseOasisSupport()
    {
        supportedOasisCount++;
        Debug.Log("INCREASE OASIS COUNT: " + supportedOasisCount);
    }

    public void DecreaseOasisSupport()
    {

        supportedOasisCount--;
        Debug.Log("DECREASE OASIS COUNT: " + supportedOasisCount);

    }

    private void RemoveForestSupportFromWaterTile()
    {
        foreach (HexTile neighbor in neighbors)
        {
            if (neighbor.tileType == TileType.Water && neighbor.GetSupportedForest() != null)
            {
                neighbor.RemoveSupportedForest();
                neighbor.CheckBiomeRules();
            }
        }
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

    public void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
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
}
