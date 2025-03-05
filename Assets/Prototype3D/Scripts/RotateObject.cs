using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float rotationSpeed = 10f; // Speed of rotation
    public float movementSpeed = 2f; // Speed of movement (slower than rotation)
    public bool invertRotation = false; // Invert rotation direction
    public GameObject obj1; // First object to rotate
    public GameObject obj2; // Second object to rotate
    public bool CHANGE = false; // Boolean to switch between objects
    public bool canMove = false; // Boolean to enable moving obj2

    private Vector3 lastMousePosition;

    void Update()
    {
        // Determine which object to interact with based on the CHANGE variable
        GameObject targetObject = CHANGE ? obj2 : obj1;

        if (Input.GetMouseButtonDown(0))
        {
            // Store the initial mouse position when the mouse button is pressed
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) && targetObject != null)
        {
            // Calculate the difference in mouse position
            Vector3 delta = Input.mousePosition - lastMousePosition;

            if (CHANGE && canMove)
            {
                // Move obj2 based on mouse movement (using movementSpeed)
                MoveTargetObject(targetObject, delta);
            }
            else
            {
                // Rotate the target object based on mouse movement (using rotationSpeed)
                RotateTargetObject(targetObject, delta);
            }

            // Update the last mouse position
            lastMousePosition = Input.mousePosition;
        }
    }

    void RotateTargetObject(GameObject target, Vector3 delta)
    {
        // Determine the rotation direction based on the invertRotation flag
        float direction = invertRotation ? -1f : 1f;

        // Rotate the target object around the Y and X axes based on mouse movement
        target.transform.Rotate(Vector3.up, delta.x * rotationSpeed * direction * Time.deltaTime, Space.World);
        target.transform.Rotate(Vector3.right, -delta.y * rotationSpeed * direction * Time.deltaTime, Space.World);
    }

    void MoveTargetObject(GameObject target, Vector3 delta)
    {
        // Convert screen space delta to world space movement (using movementSpeed)
        Vector3 movement = new Vector3(delta.x, delta.y, 0) * movementSpeed * Time.deltaTime;

        // Move the target object
        target.transform.Translate(movement, Space.World);
    }

    public void WORM()
    {
        CHANGE = true;
    }

    public void POTATO()
    {
        CHANGE = false;
    }
}