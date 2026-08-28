using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class Mover : MonoBehaviour
{
	public UnityEvent AfterMoved;
	public GameObject ReferenceObject;
	public float duration = 1f; 
	public Ease easeType = Ease.InOutSine;


	public float forceStrength = 10f;
	
	public void AddForceTo(GameObject TheObject){
		
		Vector3 direction = (ReferenceObject.GetComponent<Rigidbody>().position - TheObject.transform.position).normalized;
		ReferenceObject.GetComponent<Rigidbody>().AddForce(direction * forceStrength, ForceMode.Impulse);
		
	}
	
	public	void MoveTo(GameObject TheObject)
    {
	   
	   	ReferenceObject.transform.DORotateQuaternion(TheObject.transform.rotation, duration).SetEase(easeType);
	    ReferenceObject.transform.DORotateQuaternion(TheObject.transform.rotation ,duration).SetEase(easeType);
	    ReferenceObject.transform.DOMove(TheObject.transform.position, duration).SetEase(easeType).OnComplete(() => {
			
		    AfterMoved.Invoke();
			
	    });
	   
    }
    
	public void TeleportTo(GameObject TheObject){
			
		ReferenceObject.transform.position = TheObject.transform.position;
		AfterMoved.Invoke();
		
	}
	
	public void SetScale(float Scale){
		
		ReferenceObject.transform.localScale = new Vector3(Scale,Scale,Scale);
		AfterMoved.Invoke();
		
	}
	
}
