using EvolveGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepManager : MonoBehaviour
{
    public AudioClip[] Steps;
    AudioSource audioSource;
    public float stepSpeed;
    float stepTimer;
    PlayerController playerController;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        playerController = FindFirstObjectByType<PlayerController>();    
    }

    // Update is called once per frame
    void Update()
    {
        if(playerController.isMoving && playerController.characterController.isGrounded && playerController.enabled)
        {

            if (playerController.isRunning)
            {
                stepSpeed = 0.35f;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                stepSpeed = 0.9f;
            }
            else
            {
                stepSpeed = 0.7f;
            }

            stepTimer += Time.deltaTime;
            if (stepTimer >= stepSpeed)
            {
                audioSource.clip = Steps[Random.Range(0, Steps.Length)];
                stepTimer = 0;
                audioSource.Play();
            }
        }


      

        

    }
}
