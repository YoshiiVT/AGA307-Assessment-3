using UnityEngine;

public class MouseLook : GameBehaviour
{
    public float mouseSensitivity = 100f;

    public Transform playerBody;

    private float xRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);


        // This part was made using chatgpt's help but i did watch a few videos on raycasting.
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f)) // Adjust distance if needed
            {
                // Check if we hit the fusebox
                Fusebox fusebox = hit.collider.GetComponent<Fusebox>();
                if (fusebox != null)
                {
                    fusebox.Interact();
                }
            }
        }
        
    }
}
