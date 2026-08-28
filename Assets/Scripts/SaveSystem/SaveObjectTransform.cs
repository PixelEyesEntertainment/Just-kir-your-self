using UnityEngine;

public class SaveObjectTransform : MonoBehaviour
{
    public string saveName;

    public void SaveObjectTransformFunction()
    {
        PlayerPrefs.SetFloat(saveName + "_PositionX", transform.position.x);
        PlayerPrefs.SetFloat(saveName + "_PositionY", transform.position.y);
        PlayerPrefs.SetFloat(saveName + "_PositionZ", transform.position.z);

        Vector3 euler = transform.eulerAngles;
        PlayerPrefs.SetFloat(saveName + "_RotationX", euler.x);
        PlayerPrefs.SetFloat(saveName + "_RotationY", euler.y);
        PlayerPrefs.SetFloat(saveName + "_RotationZ", euler.z);
    }

    public void LoadObjectTransformFunction()
    {
        float posX = PlayerPrefs.GetFloat(saveName + "_PositionX", float.NaN);
        float posY = PlayerPrefs.GetFloat(saveName + "_PositionY", float.NaN);
        float posZ = PlayerPrefs.GetFloat(saveName + "_PositionZ", float.NaN);

        if (float.IsNaN(posX) || float.IsNaN(posY) || float.IsNaN(posZ))
            return;

        transform.position = new Vector3(posX, posY, posZ);

        float rotX = PlayerPrefs.GetFloat(saveName + "_RotationX", float.NaN);
        float rotY = PlayerPrefs.GetFloat(saveName + "_RotationY", float.NaN);
        float rotZ = PlayerPrefs.GetFloat(saveName + "_RotationZ", float.NaN);

        if (!float.IsNaN(rotX) && !float.IsNaN(rotY) && !float.IsNaN(rotZ))
            transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
    }


    public void ResetAllSaves()
    {
        PlayerPrefs.DeleteAll();
    }
}