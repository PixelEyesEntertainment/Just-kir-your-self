using UnityEngine;

public class Instantiator : MonoBehaviour
{
    public GameObject gameObjectToInstantiate;
    public GameObject parent;


    public void InstantiateTo(GameObject sendTo)
    {
        GameObject temp;

        if (parent != null)
            temp = Instantiate(gameObjectToInstantiate, parent.transform);
        else
            temp = Instantiate(gameObjectToInstantiate);


        temp.transform.position = sendTo.transform.position;
    }
}
