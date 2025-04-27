using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUi : GameBehaviour
{
    private void Play()
    {
        SceneManager.LoadScene("House");
    }

    private void Quit()
    {
        Application.Quit();
    }
}
