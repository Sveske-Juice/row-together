using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
    float startDelay = 0;
    public BoatNetworkInput input;

    void Update()
    {
        if (input.input.nw == "1" && input.input.ne == "1" && input.input.sw == "1" && input.input.se == "1")
        {
            startDelay += Time.deltaTime;
            if (startDelay >= 1.5)
            {
                SceneManager.LoadScene(1);
            }
        }

        else
        {
            startDelay = 0;
        }
        
    }
}
