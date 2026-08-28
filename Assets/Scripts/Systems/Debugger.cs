using UnityEngine;

public class Debugger : MonoBehaviour
{
    public void StickToGameObject(GameObject stickTo)
    {
        transform.position = stickTo.transform.position;
    }

    public void DebugMessage(string message)
    {
        Debug.Log(message);
    }
}
