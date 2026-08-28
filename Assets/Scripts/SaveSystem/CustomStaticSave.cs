using UnityEngine;
using UnityEngine.Events;

public class CustomStaticSave : MonoBehaviour
{
    public string saveName;
    public UnityEvent onSaveLoad;

    public void Save()
    {
        PlayerPrefs.SetInt(saveName, 1);
    }

    public void Load()
    {
        if (PlayerPrefs.GetInt(saveName) == 1)
        {
            onSaveLoad.Invoke();
        }
    }
}