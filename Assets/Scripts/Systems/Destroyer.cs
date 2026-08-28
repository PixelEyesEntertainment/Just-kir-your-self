using UnityEngine;

public class Destroyer : MonoBehaviour
{
    public float delay;
    public void DestroyThis(GameObject obj)
    {
        Destroy(obj, delay);
    }
}
