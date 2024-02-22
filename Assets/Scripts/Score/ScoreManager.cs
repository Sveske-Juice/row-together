using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public int score;
    [SerializeField] GameObject boat;
    Transform startPos;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    // Start is called before the first frame update
    void Start()
    {
        startPos = boat.transform;
    }

    

    // Update is called once per frame
    void Update()
    {
        score = Mathf.RoundToInt((boat.transform.position.z + 8)/2);
        scoreText.text = score + " M";
        //print(boat.transform.position.z);

        if (score > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", score);
        }

        highScoreText.text = "Best: " + PlayerPrefs.GetInt("HighScore", 0) + " M";
    }
}
