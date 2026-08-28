using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSpeedSet : MonoBehaviour
{

    void Start()
	{
    	
		float RandomSpeed = Random.Range(0.75f,1.5f);
    	
		gameObject.GetComponent<Animator>().speed = RandomSpeed;
		
    }

 
}
