using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FlashlightManager : GameBehaviour
{
    public int batteries;    
    

    public GameObject flashLight;
    private bool flashLightOn = false;
    public float maxBatteryPercentage;
    public float batteryPercentage;
    private Coroutine batteryDrainCoroutine;
    public Image batteryBar;

    void Update()
    {
        if (batteryPercentage <= 0f)
        {
            flashLightOn = false;
            FlashlightToggle();
        }
        else
        {
            if (Input.GetButtonDown("FlashlightToggle"))
            {
                flashLightOn = !flashLightOn;
                FlashlightToggle();
            }
        }

        if (Input.GetButtonDown("Reload") && batteryPercentage >= 0 && batteries >= 1)
        {
            batteryPercentage = 100;
            batteries--;
        }


        batteryBar.fillAmount = batteryPercentage / maxBatteryPercentage;
    }

    void FlashlightToggle()
    {
        if (flashLightOn)
        {
            flashLight.SetActive(true);

            // Start draining battery if the flashlight is on and the coroutine isn't already running
            if (batteryDrainCoroutine == null)
            {
                batteryDrainCoroutine = StartCoroutine(DrainBattery());
            }
        }
        else
        {
            flashLight.SetActive(false);

            // Stop the battery drain coroutine if the flashlight is off
            if (batteryDrainCoroutine != null)
            {
                StopCoroutine(batteryDrainCoroutine);
                batteryDrainCoroutine = null;
            }
        }
    }

    private IEnumerator DrainBattery()
    {
        while (flashLightOn && batteryPercentage > 0f)
        {
            batteryPercentage -= 1f;
            yield return new WaitForSeconds(0.1f); // Drain battery over time
        }
    }
}
