using UnityEngine;

public class Pests : MonoBehaviour
{
    public bool isRat, isSnail; // Define pest type
    public float moveSpeed = 2f; // Speed of movement
    private Transform targetPlant;
    private bool isMoving = false;
    public bool PuzzleFinished = false; // Determines when pests should move

    private void Update()
    {
        if (PuzzleFinished && !isMoving)
        {
            FindTargetPlant();
        }

        if (isMoving && targetPlant != null)
        {
            MoveTowardsPlant();
        }
    }

    private void FindTargetPlant()
    {
        GameObject[] plants = GameObject.FindGameObjectsWithTag("Plant");
        foreach (GameObject plant in plants)
        {
            DragAndDrop plantVars = plant.GetComponent<DragAndDrop>();
            if (plantVars != null && CanEatPlant(plantVars) && !IsPlantProtected(plantVars) && IsInRange(plant.transform))
            {
                targetPlant = plant.transform;
                isMoving = true;
                return;
            }
        }
    }

    private bool IsInRange(Transform plantTransform)
    {
        float distance = Vector2.Distance(transform.position, plantTransform.position);
        return distance < 5f; // Change this value as needed for detection range
    }

    private void MoveTowardsPlant()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPlant.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPlant.position) < 0.1f)
        {
            EatPlant();
        }
    }

    private bool CanEatPlant(DragAndDrop plantVars)
    {
        return (isRat && plantVars.RatEats) || (isSnail && plantVars.SnailEats);
    }

    private bool IsPlantProtected(DragAndDrop plantVars)
    {
        return (isRat && plantVars.protectedRat) || (isSnail && plantVars.protectedSnail);
    }

    private void EatPlant()
    {
        targetPlant.gameObject.SetActive(false);
        targetPlant = null;
        isMoving = false;
    }
}