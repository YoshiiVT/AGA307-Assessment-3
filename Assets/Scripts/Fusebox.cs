using System.Collections;
using UnityEngine;

public enum FixQuality
{
    High,
    Med,
    Low
}

public class Fusebox : GameBehaviour
{
    [Header("Animation References")]
    [SerializeField] private Animator animator;
    //References to the roomlights the fusebox is attatched too.
    [Header("Fusebox Room Match")]
    [SerializeField] private GameObject lights;

    //These floats are randomly chosen when a fusebox is fixed and sets the time before they can fail again.
    [Header("Fusebox Working Time")]
    [SerializeField] private FixQuality quality;

    //Simple Bool to let me know what the status of the fusebox is.
    [Header("FuseBox Status")]
    public bool isBroken = false;

    public void Start()
    {
        SetFailTimer();
    }

    //This Detects if the fusebox has been clicked on and only does something if it is broken.
    public void Interact()
    {
        if (!isBroken)
        {
            return;
        }
        else
        {
            FuseBoxFixed();
        }
    }

    //This runs as soon as the Fusebox is fixed. Basically a grace period.
    private void SetFailTimer()
    {
        //This chooses randomly between the enum values (ChatGPT)
        int enumCount = System.Enum.GetValues(typeof(FixQuality)).Length;
        FixQuality[] values = (FixQuality[])System.Enum.GetValues(typeof(FixQuality));
        quality = values[Random.Range(0, values.Length)];

        float fixTimer;

        switch (quality)
        { 
            case FixQuality.High:
            {
                fixTimer = 20f;
                StartCoroutine(TimerStart(fixTimer));
                break;
            }
            case FixQuality.Med:
            {
                fixTimer = 15f;
                StartCoroutine(TimerStart(fixTimer));
                break;
            }
            case FixQuality.Low:
            {
                fixTimer = 10f;
                StartCoroutine(TimerStart(fixTimer));
                break;
            }
        }
    }


    private IEnumerator TimerStart(float fixTime)
    {
        yield return new WaitForSeconds(fixTime);
        FuseboxBreak();
    }

    //Once the grace period is over, its a 50/50 chance of the fusebox breaking.
    private void FuseboxBreak()
    {
        float rndFail = Random.Range(0f, 1f);
        
        if (rndFail <= 0.5f)
        {
            isBroken = true;
            _FM.LightsOff(lights);
            animator.SetTrigger("OffTrigger");
        }
        else
        {
            StartCoroutine(DelayedReset());
        }
    }
    
    private IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(1);
        FuseboxBreak();
    }

    //This function runs when the player resets the fusebox.
    private void FuseBoxFixed()
    {
        animator.SetTrigger("OnTrigger");
        isBroken = false;
        _FM.LightsOn(lights);
        SetFailTimer();
    }

}

