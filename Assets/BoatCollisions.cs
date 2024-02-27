using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class BoatCollisions : MonoBehaviour
{
    Rigidbody rb;
    public float minDmgSpeed;
    int hp = 3;
    public GameObject[] hpIcons;
    public ParticleSystem dmgParticles;

    bool cooldown = false;
    bool isDead = false;
    float timer = 0;
    float cooldownTime = 5;

    public GameObject livesUI, scoreUI, goScreen, newBestText;
    public TextMeshProUGUI returnTimer, goScoreText;
    public GameObject scoreManagerObject;
    public ScoreManager scoreManager;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(hp <= 0 && !isDead)
        {
            
            GetComponent<Boat>().enabled = false;
            GetComponent<PlayerInput>().enabled = false;
            isDead = true;
            goScreen.SetActive(true);
            newBestText.SetActive(false);
            scoreUI.SetActive(false);
            livesUI.SetActive(false);
            goScoreText.text = scoreManager.score + "M";
            if (scoreManager.score == PlayerPrefs.GetInt("HighScore", 0))
            {
                newBestText.SetActive(true);
            }
            scoreManagerObject.SetActive(false);
            StartCoroutine(ReturnToMenu());
        }

        for (int i = 0; i < hpIcons.Length; i++)
        {
            hpIcons[i].SetActive(false);
        }
        for (int i = 0; i < hp; i++)
        {
            hpIcons[i].SetActive(true);
        }

        if(cooldown && timer < cooldownTime)
        {
            timer += Time.deltaTime;
        }
        if(timer >= cooldownTime)
        {
            cooldown = false;
            timer = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Obstacle") && collision.impulse.magnitude > minDmgSpeed && !cooldown)
        {
            hp--;
            dmgParticles.Play();
            cooldown = true;
        }
    }

    IEnumerator ReturnToMenu()
    {
        int time = 10;
        while (time > 0)
        {
            returnTimer.text = time-- +"";
            yield return new WaitForSecondsRealtime(1f);
        }
        SceneManager.LoadScene(0);
    }
}
