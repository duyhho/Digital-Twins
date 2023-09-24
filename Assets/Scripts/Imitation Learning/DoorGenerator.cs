using UnityEngine;

public class DoorGenerator : MonoBehaviour
{

    public GameObject door;
    public GameObject doorSwitch;
    public GameObject leftWall;
    public GameObject rightWall;
    float roomWidth = 20f;
    float doorWidth = 5.3f; // You can adjust this value if it's different
    float maxDoorPositionOffset = 7.2f; // This ensures the door doesn't go too close to the room's edges
    float maxDoorSwitchPositionOffset = 0.5f; // This ensures the door doesn't go too close to the room's edges

    openandclosedoor DoorComponent;
    Button3D ButtonComponent;
    // You can call this function, for example, in Start()
    private void Start()
    {

        DoorComponent = door.transform.GetChild(0).GetComponent<openandclosedoor>();
        ButtonComponent = doorSwitch.GetComponent<Button3D>();
        // GenerateDoor();

    }

    public void GenerateDoor()
    {
        // DoorComponent.CloseDoor();
        // ButtonComponent.isPressed = false;
        // Randomize the DoorGenerator's Z position
        float randomZPosition = Random.Range(2f, 6.2f);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, randomZPosition);

        float doorSwitchXPosition = Random.Range(-maxDoorSwitchPositionOffset, maxDoorSwitchPositionOffset);
        doorSwitch.transform.localPosition = new Vector3(doorSwitchXPosition, doorSwitch.transform.localPosition.y, doorSwitch.transform.localPosition.z);
        // Debug.Log("doorSwitchXPosition" + doorSwitchXPosition);

        // Randomize the door's X position
        float doorXPosition = Random.Range(-maxDoorPositionOffset, maxDoorPositionOffset);

        // Set door's position
        door.transform.localPosition = new Vector3(doorXPosition, door.transform.localPosition.y, door.transform.localPosition.z);

        // Calculate the walls' widths and positions based on door's position
        float leftWallWidth = roomWidth / 2f + doorXPosition - doorWidth / 2f;
        float rightWallWidth = roomWidth / 2f - doorXPosition - doorWidth / 2f;

        // Calculate the walls' positions
        float leftWallPosition = doorXPosition - doorWidth / 2f - leftWallWidth / 2f;
        float rightWallPosition = doorXPosition + doorWidth / 2f + rightWallWidth / 2f;

        // Set walls' positions and scales
        leftWall.transform.localPosition = new Vector3(leftWallPosition, leftWall.transform.localPosition.y, leftWall.transform.localPosition.z);
        rightWall.transform.localPosition = new Vector3(rightWallPosition, rightWall.transform.localPosition.y, rightWall.transform.localPosition.z);

        leftWall.transform.localScale = new Vector3(leftWallWidth, leftWall.transform.localScale.y, leftWall.transform.localScale.z);
        rightWall.transform.localScale = new Vector3(rightWallWidth, rightWall.transform.localScale.y, rightWall.transform.localScale.z);
    }


}