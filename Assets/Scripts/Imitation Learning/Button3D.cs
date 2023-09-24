using UnityEngine;

public class Button3D : MonoBehaviour
{
    float pressDistance = 0.05f;
    private Vector3 initialPosition;
    public bool isPressed = false;

    public Material defaultMaterial; // The default material of the button (set this in the inspector)
    public Material pressedMaterial; // The material to change to when the button is pressed (set this in the inspector)
    private Renderer buttonRenderer; // Reference to the Renderer component

    public openandclosedoor door;

    private void Start()
    {
        initialPosition = transform.localPosition;
        buttonRenderer = GetComponent<Renderer>();

        // Just to ensure that we start with the default material
        buttonRenderer.material = defaultMaterial;
        Debug.Log("initialPosition: " + initialPosition);
    }

    private void Update()
    {
        if (isPressed)
        {
            PressButton();
        }
        else
        {
            ReleaseButton();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("agent"))
        {
            isPressed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("agent"))
        {
            // isPressed = false;
        }
    }

    public void PressButton()
    {
        if (transform.localPosition.y > initialPosition.y - pressDistance)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, initialPosition.y - pressDistance, transform.localPosition.z);
            Debug.Log("Pressed Position: " + transform.localPosition);

            // Change the button's material to the pressed material
            buttonRenderer.material = pressedMaterial;
            door.OpenDoor();
        }
    }

    public void ReleaseButton()
    {
        if (transform.localPosition.y < initialPosition.y)
        {
            Debug.Log("initialPosition: " + initialPosition);

            transform.localPosition = new Vector3(transform.localPosition.x, initialPosition.y, transform.localPosition.z);

            Debug.Log("Released Position: " + transform.localPosition);

            // Change the button's material back to the default material
            buttonRenderer.material = defaultMaterial;
        }
    }
}
