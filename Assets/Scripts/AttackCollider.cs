using UnityEngine;

public class AttackCollider : GameBehaviour
{
    private Collider col;

    // Taken from Classwork
    void Start()
    {
        if (GetComponent<Collider>() != null)
        {
            col = GetComponent<Collider>();
            col.enabled = false;
        }
        else
        {
            Debug.LogError("Weapon Collider not found.");
            return;
        }
    }

    public void SetCollider(bool _enabled) => col.enabled = _enabled;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("entered the trigger");
        if (other.GetComponent<CharacterManager>() != null)
        {
            Debug.Log("entered the trigger2");
            other.GetComponent<CharacterManager>().CaughtPlayer();
        }
        
    }
}
