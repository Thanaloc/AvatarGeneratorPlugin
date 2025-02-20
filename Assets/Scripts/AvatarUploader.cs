using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class AvatarUploader : MonoBehaviour
{
    public string apiUrl = "http://127.0.0.1:8000/generate-avatar/";
    public TMP_InputField heightInput, weightInput;
    public TMP_Dropdown genderDropdown;
    public GameObject avatarPreview;

    public void UploadAvatar()
    {
        StartCoroutine(SendData());
    }

    IEnumerator SendData()
    {
        WWWForm form = new WWWForm();
        form.AddField("height", heightInput.text);
        form.AddField("weight", weightInput.text);
        form.AddField("gender", genderDropdown.options[genderDropdown.value].text);

        if (!System.IO.File.Exists("D:/OtherProjects/AvatarGeneratorUnity/unity_plugin/backend/uploads/testImg.jpg"))
        {
            Debug.LogError("Erreur : Le fichier image n'existe pas !");
            yield break;
        }

        byte[] fileData = System.IO.File.ReadAllBytes("D:/OtherProjects/AvatarGeneratorUnity/unity_plugin/backend/uploads/testImg.jpg");
        form.AddBinaryData("file", fileData, "avatar.jpg", "image/jpeg");

        UnityWebRequest request = UnityWebRequest.Post(apiUrl, form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Avatar généré !");
            // Charger le modèle 3D dans la scène (à compléter)
        }
        else
        {
            Debug.LogError("Erreur API : " + request.error);
        }
    }
}
