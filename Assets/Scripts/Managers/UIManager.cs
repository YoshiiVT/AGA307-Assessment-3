using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : SingletonDontDestroy<UIManager>
{
    public void Play()
    {
        _GM.Playing();
        SceneManager.LoadScene("House");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}

