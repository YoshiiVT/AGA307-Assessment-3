using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpscareManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TempJumpscareScript());
    }

    IEnumerator TempJumpscareScript()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("Death");
    }

}
