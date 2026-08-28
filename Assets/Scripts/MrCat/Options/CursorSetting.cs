using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorSetting : MonoBehaviour
{

	public bool IsMenu = true;

	void Start(){
		
		if(IsMenu){
			
			SetCursor(false);
		
		}else{
			
			SetCursor(true);
			
		}
	}

	public void SetCursor(bool IsEnabled)
    {
        
	    if(IsEnabled){
	    	
		    Cursor.lockState = CursorLockMode.Locked;
		    Cursor.visible = false;
	    	
	    }else{
	    	
		    Cursor.lockState = CursorLockMode.None;
		    Cursor.visible = true;
	    	
	    }
        
    }
    
}
