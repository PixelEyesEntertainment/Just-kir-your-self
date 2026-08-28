using UnityEngine;

public class RandomSound : MonoBehaviour
{
	// Array to hold your audio clips
	public AudioClip[] audioClips;

	// Reference to the AudioSource component
	private AudioSource audioSource;

	void Start()
	{
		// Get the AudioSource component attached to this GameObject
		audioSource = GetComponent<AudioSource>();
	}

	// Public method to play a random sound
	public void PlayRandomSound()
	{
		if (audioClips.Length == 0)
		{
			Debug.LogWarning("No audio clips assigned!");
			return;
		}

		// Select a random audio clip from the array
		int randomIndex = Random.Range(0, audioClips.Length);
		AudioClip randomClip = audioClips[randomIndex];

		// Play the selected audio clip
		audioSource.PlayOneShot(randomClip);
	}
}
