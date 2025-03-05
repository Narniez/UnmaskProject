using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
    public TileType tileType = TileType.Default;
    private TileType originalType;

    public BiomeType currentBiome = BiomeType.None;

    public SpriteRenderer spriteRenderer;

    [SerializeField] private Vector2Int position;
    public HexGrid grid;

    [SerializeField] private TileSpriteManager spriteManager;

    [SerializeField] private List<HexTile> neighbors = new List<HexTile>();

    private CircleCollider2D neighborDetector;

    private ConnectionStrength strength = ConnectionStrength.Default;

    private void Start()
    {
        neighborDetector = gameObject.AddComponent<CircleCollider2D>();
        neighborDetector.radius = grid.tileSize * 0.93f;
        neighborDetector.isTrigger = true;
        originalType = tileType;
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
        UpdateVisual();
    }

    public void SetOriginalBiome()
    {
        tileType = originalType;  
        UpdateVisual();
    }

  

    public void UpdateVisual()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        //switch (tileType)
        //{
        //    case TileType.Desert:
        //        spriteRenderer.sprite = desertSprite;
        //        break;
        //    case TileType.Water:
        //        spriteRenderer.sprite = waterSprite;
        //        break;
        //    case TileType.Volcano:
        //        spriteRenderer.sprite = volcanoSprite;
        //        break;
        //    case TileType.Mountain:
        //        spriteRenderer.sprite = mountainSprite;
        //        break;
        //    case TileType.Default:
        //        spriteRenderer.sprite = defaultSprite;
        //        break;
        //}

            spriteRenderer.sprite = spriteManager.GetSprite(tileType, strength);
    }

    private void CheckBiomeRules()
    {
        switch (tileType)
        {
            case TileType.Desert:
                if (currentBiome == BiomeType.Cave)
                {
                    // Check if there are any water tiles nearby
                    bool hasWaterNeighbor = HasNeighbor(TileType.Water);
                    if (hasWaterNeighbor)
                    {
                        // Weak connection if water is nearby
                        currentBiome = BiomeType.Cave;
                        strength = ConnectionStrength.Weak;
                        //tileType = TileType.Cave;
                        UpdateVisual();
                    }
                    else
                    {
                        // Strong connection if no water is nearby
                        currentBiome = BiomeType.Cave;
                        //tileType = TileType.Cave;
                        UpdateVisual();
                    }
                }
                break;

            case TileType.Water:
                if (currentBiome == BiomeType.Glacier)
                {
                    // Check if the glacier is at least 1 hexagon away from a desert
                    bool isNearDesert = HasNeighbor(TileType.Desert);
                    if (isNearDesert)
                    {
                        strength = ConnectionStrength.Weak;
                        currentBiome = BiomeType.Glacier;
                        //tileType = TileType.Glacier;
                        UpdateVisual();
                    }
                    else
                    {
                        // Strong connection if not near desert
                        currentBiome = BiomeType.Glacier;
                        //tileType = TileType.Glacier;
                        UpdateVisual();
                    }
                }
                break;

                // Add more cases for other tile types and biome types
        }
    }

    private ConnectionStrength GetConnectionStrength()
    { 

        if ((tileType == TileType.Desert || tileType == TileType.Mountain) && currentBiome == BiomeType.Cave && HasNeighbor(TileType.Water))
        {
            return ConnectionStrength.Weak;
        }
        return ConnectionStrength.Default;
    }

    private bool HasNeighbor(TileType type)
    {
        foreach (HexTile neighbor in neighbors)
        {
            if (neighbor.tileType == type) return true;
        }
        return false;
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

    public TileType GetTileType()
    {
        return originalType;
    }

    public void HighlightNeighbors(Color color)
    {
        foreach (HexTile neighbor in neighbors)
        {
            neighbor.SetColor(color);
        }
    }
}
