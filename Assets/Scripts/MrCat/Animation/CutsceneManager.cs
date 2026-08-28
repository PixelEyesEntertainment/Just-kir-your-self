using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using UnityEngine.Events;
using EvolveGames;
using DG.Tweening;

public class CutsceneManager : MonoBehaviour
{
	
	public bool MovePlayerToCutscene = true;
	
	public float duration = 1f; 
	public Ease easeType = Ease.InOutSine;

	public Animator CutsceneCanvas;
	
	public GameObject Player;
	public GameObject CameraHolder;
	public Transform PlayerTarget;
	public Transform CameraTarget;
	public PlayableDirector timeline;
	
	public UnityEvent WhenCutsceneStarts;
	public UnityEvent AfterCutsceneEnds;
	
	static public bool IsCutscene = false;
	
	void Awake(){
		
		IsCutscene = false;
		
		Player = FindFirstObjectByType<PlayerController>().gameObject;
		CameraHolder = Player.transform.Find("CameraHolder").gameObject;
		CutsceneCanvas = GameObject.Find("Cutscene Canvas").GetComponent<Animator>();
		
	}
	
	public void RunCutscene()
	{
		
		Player.GetComponent<PlayerController>().Pause();
		
		IsCutscene = true;

		if(MovePlayerToCutscene){
			
			Player.GetComponentInChildren<Camera>().gameObject.transform.DOLocalRotate(Vector3.zero, duration).SetEase(easeType);
			Player.GetComponentInChildren<Camera>().gameObject.transform.DOLocalMove(Vector3.zero, duration).SetEase(easeType);
			Player.transform.DORotateQuaternion(PlayerTarget.rotation, duration).SetEase(easeType);
			CameraHolder.transform.DORotateQuaternion(CameraTarget.rotation ,duration).SetEase(easeType);
			Player.transform.DOMove(PlayerTarget.position, duration).SetEase(easeType).OnComplete(() => {
			
				StartCoroutine(MovePlayer());
				WhenCutsceneStarts.Invoke();
			
			});
		
		}else{
			
			StartCoroutine(MovePlayer());
			WhenCutsceneStarts.Invoke();
			
		}
		
		CutsceneCanvas.SetBool("isCutscene",true);
		
	}
	
	public void StopCutscene(){
		
		CutsceneCanvas.SetBool("isCutscene", false);
		IsCutscene = false;
		Player.GetComponent<PlayerController>().UnPause();
		
	}
	
	private IEnumerator MovePlayer()
	{
		
		
		timeline.Play();
		yield return new WaitForSeconds((float)timeline.duration);
		CutsceneCanvas.SetBool("isCutscene", false);
		IsCutscene = false;
		if(!IsCutscene){
			
			Player.GetComponent<PlayerController>().UnPause();
			
		}
		AfterCutsceneEnds.Invoke();
		
	}
}
