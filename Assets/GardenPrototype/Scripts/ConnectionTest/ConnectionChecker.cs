using UnityEngine;

public class ConnectionChecker : MonoBehaviour
{
    private void Start()
    {
        GameManager.AddPuzzleCondition(IsEveryPlantConnected);
    }

    public bool IsEveryPlantConnected()
    {
        int oneConnected = 0;
        int moreThanOneConnection = 0;

        ImprovedPlantSpot[] spots = FindObjectsByType<ImprovedPlantSpot>(FindObjectsSortMode.None);
        //Debug.Log($"Found PlantSpots: {spots.Length}");

        foreach (ImprovedPlantSpot spot in spots)
        {
            if (!spot.IsOccupied)
                continue;

            int currentConnected = 0;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(spot.transform.position, 225);
            //Debug.Log($"Interacted Colliders: {colliders.Length}");

            foreach (Collider2D collider in colliders)
            {
                ImprovedPlantSpot otherSpot = collider.GetComponentInParent<ImprovedPlantSpot>();

                if (otherSpot == null || otherSpot == spot) continue;

                if (otherSpot.IsOccupied)
                {
                    //Debug.Log("[ConnectionChecker]: Found Connection.");
                    currentConnected++;
                }
            }

            switch (currentConnected)
            {
                case 0:
                    //Debug.Log($"[ConnectionChecker]: Puzzle FAILED because {spot.CurrentPlant.name} has no neighbors.");
                    return false;
                case 1:
                    //Debug.Log("[ConnectionChecker]: Found plant with one neighbor.");
                    oneConnected++;
                    if (oneConnected > 2)
                    {
                        //Debug.Log("[ConnectionChecker]: Puzzle FAILED because <more> than 2 plants have only one neighbor.");
                        return false;
                    }
                    break;

                default:
                    moreThanOneConnection++;
                    break;
            }
        }

        if (oneConnected < 2)
        {
            Debug.Log("[ConnectionChecker]: Puzzle FAILED because <less> than 2 plants have only one neighbor.");
            return false;
        }

        Debug.Log($"[ConnectionChecker]: Check success with stats: <OneConnection>:{oneConnected}, <MoreThanOneConnection>:{moreThanOneConnection}, <ZeroConnection>:0");
        return true;
    }
}