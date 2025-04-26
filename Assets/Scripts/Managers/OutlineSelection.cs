using UnityEngine;

public class OutlineSelection : GameBehaviour
{

    //Followed the tutorial for the texture asset, but had chatgpt help me edit it and smooth it into what I needed.


    private Outline currentHighlight;
    private RaycastHit raycastHit;

    private void Start()
    {
        // Find all selectables and immediately set outline properties
        GameObject[] selectables = GameObject.FindGameObjectsWithTag("Selectable");

        foreach (GameObject selectable in selectables)
        {
            Outline outline = selectable.GetComponent<Outline>();

            if (outline == null)
            {
                outline = selectable.AddComponent<Outline>();
            }
            outline.enabled = false; // Make sure it's off at start
        }
    }

    private void Update()
    {
        HandleHighlight();
    }

    private void HandleHighlight()
    {
        // Turn off previous highlight if it exists
        if (currentHighlight != null)
        {
            currentHighlight.enabled = false;
            currentHighlight = null;
        }

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out raycastHit))
        {
            Transform hitTransform = raycastHit.transform;

            if (hitTransform.CompareTag("Selectable"))
            {
                Fusebox fb = hitTransform.GetComponent<Fusebox>();

                if (fb != null && fb.isBroken)
                {
                    Outline outline = hitTransform.GetComponent<Outline>();

                    if (outline != null)
                    {
                        outline.enabled = true;
                        currentHighlight = outline;
                    }
                }
            }
        }
    }
}
