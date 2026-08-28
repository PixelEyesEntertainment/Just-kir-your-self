using EvolveGames;
using UnityEngine;

public class Transformer : MonoBehaviour
{
    [Header("Leave Below Empty if you want to move Player")]
    public Transform objectToTeleport;
    private PlayerController cachedPlayer;

    private PlayerController GetPlayer()
    {
        if (cachedPlayer == null)
            cachedPlayer = FindFirstObjectByType<PlayerController>();
        return cachedPlayer;
    }

    public void PlayerPositionTo(GameObject toThis)
    {
        PlayerController player = GetPlayer();

        float posX = toThis.transform.position.x;
        float posY = toThis.transform.position.y;
        float posZ = toThis.transform.position.z;

        player.Pause();
        player.transform.position = new Vector3(posX, posY, posZ);
        player.UnPause();
    }
    public void PlayerRotationTo(GameObject toThis)
    {
        PlayerController player = GetPlayer();

        float rotX = toThis.transform.eulerAngles.x;
        float rotY = toThis.transform.eulerAngles.y;
        float rotZ = toThis.transform.eulerAngles.z;

        if (float.IsNaN(rotX) || float.IsNaN(rotY) || float.IsNaN(rotZ))
            return;

        player.Pause();
        player.transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
        player.UnPause();
    }
    public void ObjectRotationTo(GameObject toThis)
    {
        float rotX = toThis.transform.position.x;
        float rotY = toThis.transform.position.y;
        float rotZ = toThis.transform.position.z;

        objectToTeleport.transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
    }
    public void ObjectPositionTo(GameObject toThis)
    {
        float posX = toThis.transform.position.x;
        float posY = toThis.transform.position.y;
        float posZ = toThis.transform.position.z;

        objectToTeleport.transform.position = new Vector3(posX, posY, posZ);
    }
}
