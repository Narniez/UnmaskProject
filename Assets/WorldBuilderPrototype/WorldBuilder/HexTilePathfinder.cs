using System.Collections.Generic;
using UnityEngine;

public class HexTilePathfinder : MonoBehaviour
{
    public HexTile startTile;
    public HexTile endTile;
    public bool IsPathConnected()
    {
        if (startTile == null || endTile == null) return false;

        Queue<HexTile> queue = new Queue<HexTile>();
        HashSet<HexTile> visited = new HashSet<HexTile>();

        queue.Enqueue(startTile);
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            HexTile currentTile = queue.Dequeue();

            if (currentTile == endTile)
                return true; 

            foreach (HexTile neighbor in currentTile.GetNeighbors())
            {
                if (!visited.Contains(neighbor) && IsValidBiomeTile(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        return false; 
    }

    private bool IsValidBiomeTile(HexTile tile)
    {
        return tile.currentBiome != BiomeType.None; 
    }
}
