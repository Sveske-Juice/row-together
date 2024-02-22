using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighScoreDisplayer : MonoBehaviour
{
    public TextMeshProUGUI hs;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hs.text = PlayerPrefs.GetInt("HighScore", 0) + "M";
    }
}
