using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Playing,
    Jumpscare,
    Loss,
    Victory
}

//Josh gave me the script of SingletonDontDestroy, he said it should stop GameManager from being destroyed on another scene loading.
public class GameManager : SingletonDontDestroy<GameManager>
{
    [Header("GameState")]
    [SerializeField] private GameState gameState;

    // Public read-only property (ChatGPT)
    public GameState _gameState => gameState;

    [Header("Timer Variables")]
    [SerializeField] private float timeLeft = 180f;



    private void Update()
    { 
        switch (gameState)
        {
            case GameState.Menu :
                break;
            case GameState.Playing :
                timeLeft -= Time.deltaTime;
                if (timeLeft < 0)
                {
                    GameVictory();
                }
                break;
            case GameState.Jumpscare :
                break;
            case GameState.Loss :
                break;
            case GameState.Victory :
                break;
        }
        
        
    }

    private void GameVictory()
    {
        Cursor.lockState = CursorLockMode.None;
        gameState = GameState.Victory;
        timeLeft = 180f;
        SceneManager.LoadScene("VictoryTitleCard");
    }

    public void Jumpscare()
    {
        Cursor.lockState = CursorLockMode.None;
        gameState = GameState.Jumpscare;
        SceneManager.LoadScene("Jumpscare");
    }    

    public void Playing()
    {
        gameState = GameState.Playing;
    }

}
