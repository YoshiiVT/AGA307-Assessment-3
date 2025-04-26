using UnityEngine;

public class FuseboxManager : Singleton<FuseboxManager>
{
    public void LightsOff(GameObject room)
    {
        room.SetActive(false);
    }

    public void LightsOn(GameObject room)
    {
        room.SetActive(true);
    }
}