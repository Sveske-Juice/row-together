using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthManager : MonoBehaviour
{
    int health = 3;
    bool gameOver = false;
    float gameOverDelay = 0;

    [SerializeField]
    GameObject gameOverScreen;

    private void Update()
    {
        if (gameOver)
        {
            gameOverDelay += Time.deltaTime;
            if (gameOverDelay >= 5)   
            {
                SceneManager.LoadScene(0);
            }
        }
    }


    public void LoseHealth()
    {
        health--;
        if (health == 0)
        {
            loseGame();
        }
    }

    private void loseGame()
    {
        //Todo: stop spillet fra at opdatere efter gameover
        gameOver = true;
        Instantiate(gameOverScreen);
    }
}
