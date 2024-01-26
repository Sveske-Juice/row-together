using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    float startDelay = 0;
    public BoatNetworkInput input;

    void Update()
    {
        if (input.input.nw == "1" && input.input.ne == "1" && input.input.sw == "1" && input.input.se == "1" || Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.O))
        {
            startDelay += Time.deltaTime;
            if (startDelay >= 1.5)
            {
                SceneManager.LoadScene("Game");
            }
        }
        else
        {
            startDelay = 0;
        }
        
    }
}
