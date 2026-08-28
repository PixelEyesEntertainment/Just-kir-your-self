using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Events;

public class SaveDoorLock : MonoBehaviour
{
    public string saveName;
    public Doors door;

    public UnityEvent onLocked, onUnlocked;
    public void Save()
    {
        int locked = 0;
        if (door.isLocked)
            locked = 1;
        else
            locked = 0;

        PlayerPrefs.SetInt(saveName, locked);
    }

    public void Load()
    {
        if (PlayerPrefs.GetInt(saveName) == 1)
        {
            onLocked.Invoke();
        }
        if (PlayerPrefs.GetInt(saveName) == 0)
        {
            onUnlocked.Invoke();
        }
    }
}
