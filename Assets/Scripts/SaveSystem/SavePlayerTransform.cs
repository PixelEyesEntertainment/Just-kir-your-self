using EvolveGames;
using UnityEngine;

public class SavePlayerTransform : MonoBehaviour
{
    private PlayerController cachedPlayer;

    private PlayerController GetPlayer()
    {
        if (cachedPlayer == null)
            cachedPlayer = FindFirstObjectByType<PlayerController>();
        return cachedPlayer;
    }

    public void SavePlayerTransformFunction()
    {
        PlayerController player = GetPlayer();
        if (player == null) return;

        PlayerPrefs.SetFloat("PlayerPositionX", player.transform.position.x);
        PlayerPrefs.SetFloat("PlayerPositionY", player.transform.position.y);
        PlayerPrefs.SetFloat("PlayerPositionZ", player.transform.position.z);

        Vector3 euler = player.transform.eulerAngles;
        PlayerPrefs.SetFloat("PlayerRotationX", euler.x);
        PlayerPrefs.SetFloat("PlayerRotationY", euler.y);
        PlayerPrefs.SetFloat("PlayerRotationZ", euler.z);
    }

    public void LoadPlayerTransformFunction()
    {
        PlayerController player = GetPlayer();
        if (player == null) return;

        float posX = PlayerPrefs.GetFloat("PlayerPositionX", float.NaN);
        float posY = PlayerPrefs.GetFloat("PlayerPositionY", float.NaN);
        float posZ = PlayerPrefs.GetFloat("PlayerPositionZ", float.NaN);

        if (float.IsNaN(posX) || float.IsNaN(posY) || float.IsNaN(posZ))
            return;

        float rotX = PlayerPrefs.GetFloat("PlayerRotationX", float.NaN);
        float rotY = PlayerPrefs.GetFloat("PlayerRotationY", float.NaN);
        float rotZ = PlayerPrefs.GetFloat("PlayerRotationZ", float.NaN);

        if (float.IsNaN(rotX) || float.IsNaN(rotY) || float.IsNaN(rotZ))
            return;

        player.Pause();
        player.transform.position = new Vector3(posX, posY, posZ);
        player.transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
        player.UnPause();
    }
}