using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public GameObject crosshairUI;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crosshairUI.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            crosshairUI.SetActive(false);
        }
    }
}