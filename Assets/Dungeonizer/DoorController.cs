using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorController : MonoBehaviour
{
    public float doorOrientation = 1f;

    [SerializeField]
    public bool doorIsOpen = false;
    private float rotationSpeed = 90f; // Degrees per second
    private float targetRotationY = 0;

    private void Update()
    {
        // Lerp the door's rotation towards the target
        SetTargetRotationBasedOnState();

        Vector3 currentRotation = transform.eulerAngles;
        float newY = Mathf.MoveTowardsAngle(currentRotation.y, targetRotationY, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(currentRotation.x, newY, currentRotation.z);
    }

    private void OnMouseDown()
    {
        // ToggleDoor();
    }

    public void ToggleDoor()
    {
        doorIsOpen = !doorIsOpen;
        SetTargetRotationBasedOnState();
    }

    private void SetTargetRotationBasedOnState()
    {
        switch (doorOrientation)
        {
            case 1:
                targetRotationY = doorIsOpen ? 90f : 0f;
                break;
            case 2:
                targetRotationY = doorIsOpen ? -180f : -90f;
                break;
            case 3:
                targetRotationY = doorIsOpen ? 270f : 180f;
                break;
            case 4:
                targetRotationY = doorIsOpen ? 0f : 90f;
                break;
        }
    }
    public void Reset()
    {
        doorIsOpen = false;
    }

}
