using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EvolveGames;

public class PauseMenu : MonoBehaviour
{

	public bool CanPause = true;

	public bool IsPause = false;
	public CursorSetting CursorSettingSet;
	public PlayerController Player;
	
    void Update()
    {
	    if(CanPause){
	    if(Input.GetKeyDown(KeyCode.Escape) && !CutsceneManager.IsCutscene){
	    	
	    	if(!IsPause){
	    		
	    		gameObject.GetComponent<Canvas>().enabled =  true;
	    		Time.timeScale = 0;
	    		CursorSettingSet.SetCursor(false);
	    		
	    		if(Player != null){
	    		Player.Pause();
	    		}
	    		
	    		IsPause = true;
	    		
	    	}else if(IsPause){
	    		
		    	UnPauseGame();
		    	
	    	}
	    	
	    }
	   }
        
    }
    
	public void UnPauseGame(){
		
		gameObject.GetComponent<Canvas>().enabled =  false;
		Time.timeScale = 1;
		CursorSettingSet.SetCursor(true);

		if(Player != null){
		Player.UnPause();
		}
		
		IsPause = false;
		
	}
}
