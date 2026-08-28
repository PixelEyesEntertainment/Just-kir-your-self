using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DoTweenFixer : MonoBehaviour
{

    void Start()
	{
    	
		DOTween.KillAll();
	    Destroy(GameObject.Find("[DOTween]"));  
	    
    }

}
