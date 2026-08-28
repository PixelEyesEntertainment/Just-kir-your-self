using UnityEngine;

public class RandomAnimationTrigger : MonoBehaviour
{
	
	[SerializeField] private Animator animator;
	[SerializeField] private string[] triggerNames;
	
	public void PlayRandomTrigger()
	{
		
		if (triggerNames.Length == 0 || animator == null) return;
		int randomIndex = Random.Range(0, triggerNames.Length);
		animator.SetTrigger(triggerNames[randomIndex]);
		
	}

}
