using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Doors : MonoBehaviour
{
    public bool isLocked;
    public bool isOpen;
    public AudioClip doorIsLocked, lockingDoor, unlockingDoor;
    public AudioClip doorOpening, doorClosing;

    private AudioSource audioSource;
    private Animator anim;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    public void OpenDoorFront()
    {
        if(isLocked)
        {
            audioSource.clip = doorIsLocked;
            audioSource.Play();

            anim.Play("Door is Locked");
            return;
        }

        audioSource.clip = doorOpening;
        audioSource.Play();

        isOpen = true;
        anim.SetBool("isOpenFront", isOpen);
    }

    public void OpenDoorBehind()
    {
        if (isLocked)
        {
            audioSource.clip = doorIsLocked;
            audioSource.Play();

            anim.Play("Door is Locked");
            return;
        }

        audioSource.clip = doorOpening;
        audioSource.Play();

        isOpen = true;
        anim.SetBool("isOpenBehind", isOpen);
    }

    public void CloseDoor()
    {
        if (isLocked)
            return;

        audioSource.clip = doorClosing;
        audioSource.Play();

        isOpen = false;
        anim.SetBool("isOpenBehind", isOpen);
        anim.SetBool("isOpenFront", isOpen);
    }

    public void LockDoor()
    {
        if (isLocked)
            return;

        audioSource.clip = lockingDoor;
        audioSource.Play();

        isLocked = true;
        CloseDoor();
    }

    public void UnlockDoor()
    {
        if (!isLocked)
            return;

        audioSource.clip = unlockingDoor;
        audioSource.Play();

        isLocked = false;
    }
}
