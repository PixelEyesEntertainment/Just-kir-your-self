using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using EvolveGames;

public class GameLoader : MonoBehaviour
{
	
	public PauseMenu Pause;
	public GameObject FadePrefab;
	public GameObject LoadingPrefab;
	public float MinLoadingTime = 1f;

	private bool isLoading = false;
	
	public void Start(){
		
		FadeOut();
		
	}
	
	public void FadeIn(){
		
		GameObject Fade	= Instantiate(FadePrefab);
		Fade.GetComponent<Animator>().Play("FadeIn");
		
	}
	
	public void FadeOut(){
		
		GameObject Fade	= Instantiate(FadePrefab);
		Fade.GetComponent<Animator>().Play("FadeOut");
		Destroy(Fade,2);
		
	}
	
	 public void LoadGame(string LevelName)
	{
		
		if(isLoading) return;
		
		Pause.UnPauseGame();
		GameObject.FindFirstObjectByType<PlayerController>().GetComponent<PlayerController>().Pause();
		FadeIn();
		Instantiate(LoadingPrefab);
		isLoading = true;
		
		 StartCoroutine(LoadSceneAsync(LevelName));

		 }

	 public void LoadSave()
	{
		
		if(isLoading) return;
		
		Pause.UnPauseGame();
		GameObject.FindFirstObjectByType<PlayerController>().GetComponent<PlayerController>().Pause();
		FadeIn();
		Instantiate(LoadingPrefab);
		isLoading = true;
		
		 StartCoroutine(LoadSceneAsync(PlayerPrefs.GetString("Save", "Level1")));
		
	}

	  private IEnumerator LoadSceneAsync(string SceneName)
	{
		
		if(Pause != null){
			Pause.CanPause = false;
		}

		 AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
		 asyncLoad.allowSceneActivation = false;

		 float timer = 0f;

		 while (asyncLoad.progress < 0.9f)
			 {
				 timer += Time.deltaTime;
				 yield return null;
				 }

		 while (timer < MinLoadingTime)
			 {
				 timer += Time.deltaTime;
				 yield return null;
				 }


		 yield return new WaitForSeconds(0.5f);

		 asyncLoad.allowSceneActivation = true;
		 yield return new WaitUntil(() => asyncLoad.isDone);

		 }
}
