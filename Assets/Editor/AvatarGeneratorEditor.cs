using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.IO;
using System.Threading;

public class AvatarGeneratorEditor : EditorWindow
{
    private string height = "";
    private string weight = "";
    private string gender = "male";
    private Texture2D avatarTexture;

    private GUIStyle errorStyle;
    private Process fastAPIProcess;  // Référence du processus FastAPI

    private bool hasCheckedAPI = false;  // Empêche le check en boucle
    private bool apiIsRunning;

    [MenuItem("Tools/Avatar Generator")]
    public static void ShowWindow()
    {
        // Ouvre ou focus la fenêtre
        GetWindow<AvatarGeneratorEditor>("Avatar Generator");
    }

    private void OnGUI()
    {
        // Initialiser le style d'erreur
        errorStyle = new GUIStyle(GUI.skin.label);
        errorStyle.normal.textColor = Color.red;
        errorStyle.fontSize = 14;
        errorStyle.fontStyle = FontStyle.Bold;

        GUILayout.Space(10); // Espacement

        // Titre avec un style
        GUILayout.Label("Avatar Generator", EditorStyles.boldLabel);
        GUILayout.Space(20);

        // Saisie de la taille (placeholder inclus)
        height = EditorGUILayout.TextField(new GUIContent("Height (cm)", "Enter the height in centimeters"), height);
        if (string.IsNullOrEmpty(height)) // Afficher un message d'erreur si vide
        {
            GUILayout.Label("Height is required", errorStyle);
        }

        GUILayout.Space(10);

        // Saisie du poids (placeholder inclus)
        weight = EditorGUILayout.TextField(new GUIContent("Weight (kg)", "Enter the weight in kilograms"), weight);
        if (string.IsNullOrEmpty(weight)) // Afficher un message d'erreur si vide
        {
            GUILayout.Label("Weight is required", errorStyle);
        }

        GUILayout.Space(10);

        // Sélection du sexe
        gender = EditorGUILayout.Popup("Gender", gender == "male" ? 0 : 1, new string[] { "Male", "Female" }) == 0 ? "male" : "female";

        GUILayout.Space(20);

        // Affichage de la texture 2D avec Drag and Drop
        avatarTexture = (Texture2D)EditorGUILayout.ObjectField("Avatar Image", avatarTexture, typeof(Texture2D), false);

        if (avatarTexture == null)
        {
            GUILayout.Label("Drag and Drop a 2D Texture here", errorStyle);
        }

        GUILayout.Space(20);

        // Bouton pour générer l'avatar
        if (GUILayout.Button("Generate Avatar"))
        {
            GenerateAvatar();
        }

        GUILayout.Space(10);

        if (!hasCheckedAPI)
        {
            hasCheckedAPI = true;
            apiIsRunning = IsFastAPIRunning();
            UnityEngine.Debug.Log("checked " + apiIsRunning);
        }

        if (apiIsRunning)
        {
            if (GUILayout.Button("Stop FastAPI"))
            {
                StopFastAPI();
                apiIsRunning = false;  // Met à jour immédiatement l'état
                Repaint();  // Force la fenêtre à se rafraîchir
            }
        }
        else
        {
            if (GUILayout.Button("Start FastAPI"))
            {
                StartFastAPI();
                apiIsRunning = true; // Met à jour l'état immédiatement
            }
        }
    }

    private void GenerateAvatar()
    {
        if (!IsFastAPIRunning())
        {
            // Afficher un message d'erreur dans l'éditeur si l'API n'est pas lancée
            EditorUtility.DisplayDialog("Error", "FastAPI is not running. Please start FastAPI first.", "OK");
            return;
        }

        // Appelle ta méthode pour générer l'avatar avec les paramètres actuels
        string apiUrl = "http://127.0.0.1:8000/generate-avatar/";

        // Convertir la texture en un tableau de bytes
        byte[] imageBytes = TextureToByteArray(avatarTexture);

        // Prépare ta requête (tu peux utiliser UnityWebRequest ici ou System.Net.Http)
        // L'envoi de la requête se fait sans passer par le mode Play !

        WWWForm form = new WWWForm();
        form.AddField("height", height);
        form.AddField("weight", weight);
        form.AddField("gender", gender);

        form.AddBinaryData("file", imageBytes, "avatar.jpg", "image/jpeg");

        UnityEngine.Debug.Log("Sending request to API: " + apiUrl);


        // Exemple de code d'appel à l'API FastAPI
        UnityWebRequest request = UnityWebRequest.Post(apiUrl, form);
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Content-Type", "multipart/form-data");

        request.SendWebRequest().completed += (asyncOp) =>
        {
            UnityEngine.Debug.Log("Response Code: " + request.responseCode);
            UnityEngine.Debug.Log("Response Text: " + request.downloadHandler.text);

            if (request.result == UnityWebRequest.Result.Success)
            {
                // L'avatar a été généré, récupère l'image et affiche-la
                avatarTexture = LoadTextureFromResponse(request.downloadHandler.data);
            }
            else
            {
                UnityEngine.Debug.LogError("Error generating avatar: " + request.error);
            }
        };
    }

    private Texture2D LoadTextureFromResponse(byte[] data)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(data);
        return texture;
    }

    private byte[] TextureToByteArray(Texture2D texture)
    {
        if (texture != null)
        {
            // Si la texture est non lisible, utiliser la méthode GetPixels() pour créer une version lisible
            if (!texture.isReadable)
            {
                // Créer une nouvelle texture qui sera lisible
                Texture2D readableTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount > 1);
                readableTexture.SetPixels(texture.GetPixels());
                readableTexture.Apply();

                // Encoder la texture lisible en PNG
                return readableTexture.EncodeToPNG();
            }

            // Si la texture est déjà lisible, on peut directement l'encoder en PNG
            return texture.EncodeToPNG();
        }

        return null;
    }

    private void StartFastAPI()
    {
        string pythonPath = GetPythonPathViaCommand();
        string apiPath = GetFastAPIPath();

        // ✅ Vérifier que les chemins existent avant de créer le batch
        if (!File.Exists(pythonPath))
        {
            UnityEngine.Debug.LogError($"❌ Python path invalid: {pythonPath}");
            return;
        }

        if (!File.Exists(apiPath))
        {
            UnityEngine.Debug.LogError($"❌ API script not found: {apiPath}");
            return;
        }

        UnityEngine.Debug.Log($"📌 Python Path: {pythonPath}");
        UnityEngine.Debug.Log($"📌 API Path: {apiPath}");
        UnityEngine.Debug.Log($"📌 Working Directory: {Path.GetDirectoryName(apiPath)}");


        // ✅ Créer un script batch temporaire
        string batchScriptPath = Path.Combine(Path.GetTempPath(), "start_fastapi.bat");

        using (StreamWriter writer = new StreamWriter(batchScriptPath))
        {
            writer.WriteLine($"@echo off");
            writer.WriteLine($"chcp 65001 > nul"); // ✅ Active l'encodage UTF-8 pour éviter les bugs d'accent
            writer.WriteLine($"cd /d \"{Path.GetDirectoryName(apiPath)}\" || exit"); // ✅ Quitte si le chemin est invalide
            writer.WriteLine($"\"{pythonPath}\" \"{apiPath}\" || exit"); // ✅ Quitte si le fichier Python est introuvable

        }

        // ✅ Lancer le script batch
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = batchScriptPath,
            WorkingDirectory = Path.GetDirectoryName(apiPath),
            CreateNoWindow = false,
            UseShellExecute = true
        };

        try
        {
            fastAPIProcess = Process.Start(startInfo);
            UnityEngine.Debug.Log($"✅ FastAPI started successfully. Process ID: {fastAPIProcess?.Id}");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Failed to start FastAPI: {ex.Message}");
        }

    }


    private void StopFastAPI()
    {
        // 🔍 Vérifie si on a déjà le bon PID enregistré
        if (File.Exists("fastapi_pid.txt"))
        {
            try
            {
                int pid = int.Parse(File.ReadAllText("fastapi_pid.txt"));
                Process process = Process.GetProcessById(pid);
                process.Kill();
                process.WaitForExit();
                File.Delete("fastapi_pid.txt");
                UnityEngine.Debug.Log("✅ FastAPI process stopped successfully.");
                apiIsRunning = false;
                return;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"⚠️ Could not stop FastAPI process: {ex.Message}");
            }
        }

        // 🛠 Si aucun PID enregistré, on cherche le process FastAPI via le port 8000
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c netstat -ano | findstr :8000",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = Process.Start(psi);
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // 🔥 Extraire le vrai PID du processus écoutant sur le port 8000
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 4 && int.TryParse(parts[^1], out int foundPid))
                {
                    Process foundProcess = Process.GetProcessById(foundPid);
                    foundProcess.Kill();
                    foundProcess.WaitForExit();
                    UnityEngine.Debug.Log($"✅ FastAPI (PID {foundPid}) stopped via port scanning.");
                    apiIsRunning = false;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Failed to stop FastAPI: {ex.Message}");
        }
    }




    private bool IsFastAPIRunning()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c netstat -ano | findstr :8000",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = Process.Start(psi);
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        Process foundProcess;

        // 🔥 Extraire le vrai PID du processus écoutant sur le port 8000
        string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string line in lines)
        {
            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 4 && int.TryParse(parts[^1], out int foundPid))
            {
                foundProcess = Process.GetProcessById(foundPid);
                return !foundProcess.HasExited;
            }
        }

        return false;
    }


    #region Get Paths

    private string GetPythonPathViaCommand()
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c where python",
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process process = Process.Start(startInfo);
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            // 🔥 Séparer les chemins si `where python` en retourne plusieurs
            string[] paths = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    UnityEngine.Debug.Log($"✅ Python trouvé : {path}");
                    return path;
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"❌ Erreur en cherchant Python via `where python`: {ex.Message}");
        }

        UnityEngine.Debug.LogError("❌ Python not found.");
        return null;
    }



    private string GetFastAPIPath()
    {
        // Récupérer le chemin du répertoire contenant le fichier Unity
        string unityProjectPath = Application.dataPath;  // Chemin vers le dossier "Assets"
        string backendPath = Path.GetFullPath(Path.Combine(unityProjectPath, "..", "backend"));
        string apiPath = Path.Combine(backendPath, "api.py");  // Construire le chemin complet du script FastAPI

        return apiPath;
    }


    #endregion
}
