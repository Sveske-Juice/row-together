using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatCollisions : MonoBehaviour
{
    Rigidbody rb;
    public float minDmgSpeed;
    int hp = 3;
    public GameObject[] hpIcons;
    public ParticleSystem dmgParticles;

    bool cooldown = false;
    float timer = 0;
    float cooldownTime = 10;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(hp <= 0)
        {
            //End Game
            Time.timeScale = 0;
            print("dead");
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
        if(collision.gameObject.CompareTag("Obstacle") && rb.velocity.magnitude > minDmgSpeed && !cooldown)
        {
            hp--;
            dmgParticles.Play();
            cooldown = true;
        }
    }
}
