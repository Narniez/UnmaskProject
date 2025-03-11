using System.Collections.Generic;
using UnityEngine;

public class HexTilePathfinder : MonoBehaviour
{
    public HexTile startTile;
    public HexTile endTile;
    public List<HexTile> FindPath()
    {
        List<HexTile> path = new List<HexTile>();
        if (startTile == null || endTile == null || startTile.currentBiome == BiomeType.None || endTile.currentBiome == BiomeType.None) return path;

        Queue<HexTile> queue = new Queue<HexTile>();
        HashSet<HexTile> visited = new HashSet<HexTile>();
        Dictionary<HexTile, HexTile> cameFrom = new Dictionary<HexTile, HexTile>();

        queue.Enqueue(startTile);
        visited.Add(startTile);
        cameFrom[startTile] = null;

        while (queue.Count > 0)
        {
            HexTile currentTile = queue.Dequeue();

            if (currentTile == endTile)
            {
                path = ReconstructPath(cameFrom, endTile);
                return path;
            }

            foreach (HexTile neighbor in currentTile.GetNeighbors())
            {
                if (!visited.Contains(neighbor) && IsValidBiomeTile(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = currentTile;
                }
            }
        }

        return path; 
    }

    private List<HexTile> ReconstructPath(Dictionary<HexTile, HexTile> cameFrom, HexTile endTile)
    {
        List<HexTile> path = new List<HexTile>();
        HexTile currentTile = endTile;

        while (currentTile != null)
        {
            path.Add(currentTile);
            currentTile = cameFrom[currentTile];
        }

        path.Reverse();
        return path;
    }

    private bool IsValidBiomeTile(HexTile tile)
    {
        return tile.currentBiome != BiomeType.None; 
    }
}
