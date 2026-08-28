using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using EvolveGames;
public class FlashLight : MonoBehaviour
{
    public Light spotLight;
    public Animator Hand;
    public bool isOpen;
	public bool canUse;
	bool CanPlayJump = true;
    bool lockTimer=true;
    PlayerController player;


    void Start()
    {
        player = GetComponent<PlayerController>();
        
    }
    public void CanUse(bool canUse_)
    {
        canUse = canUse_;
    }
	public void CanPlayingJump(bool CanPlayJump_)
	{
		CanPlayJump = CanPlayJump_;
	}
    // Update is called once per frame
    void Update()
    {

        Hand.SetBool("isWalking", player.isMoving);
	    Hand.SetBool("isSprinting", player.isRunning);
	    if(CanPlayJump){
        Hand.SetBool("isJumping", player.isJumping);
	    }else{
		    Hand.SetBool("isJumping", false);    	
	    }

        if (player.enabled == false || player.isPause)
        {
            canUse = false;
            isOpen = false;
            Hand.SetBool("isOpen", isOpen);
        }


        if (canUse == false)
            return;

            if (Input.GetKeyDown(KeyCode.F) && lockTimer == true) 
            {
                lockTimer = false;
                StartCoroutine(locktimer());

                isOpen = !isOpen;
                Hand.SetBool("isOpen", isOpen);
            }

    }
    IEnumerator locktimer()
    {
        yield return new WaitForSeconds(1);
	    lockTimer = true;
    }
}
