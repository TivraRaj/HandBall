using UnityEditor;
using UnityEngine;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.IO;

#if UNITY_EDITOR
public class CheckForUpdates : EditorWindow
{
    //Links
    private const string repositoryURL = "https://github.com/tomoclub-games/TomoMultiplayerGDK";
    private const string releasesURL = "https://github.com/tomoclub-games/TomoMultiplayerGDK/releases";

    //GitHub API 
    private const string latestURLTemplate = "https://api.github.com/repos/{owner}/{repo}/releases/latest";
    private const string owner = "tomoclub-games";
    private const string repo = "TomoMultiplayerGDK";
    private const string personalAccessToken = "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";

    //Check For Update
    private string currentVersion;
    private string currentVersionPath;
    private string latestVersion;
    private bool checkForUpdateInProcess = false;
    private bool isUpdateAvailable = false;

    //Download Update
    private string downloadURL;
    private bool isDownloaded = false;
    private bool downloadInProcess = false;
    private string downloadPath;
    private float progress;
    private string progressText;


    [MenuItem("Tools/TomoMultiplayerGDK Utility")]
    private static void ShowWindow()
    {
        var window = GetWindow<CheckForUpdates>();
        window.titleContent = new GUIContent("TomoMultiplayerGDK Utility");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Tomo Multiplayer GDK Updater", EditorStyles.boldLabel);

        GUILayout.Space(10f);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Repository"))
            Application.OpenURL(repositoryURL);

        if (GUILayout.Button("Releases"))
            Application.OpenURL(releasesURL);

        GUILayout.EndHorizontal();
        GUILayout.Space(20f);
        GUILayout.BeginVertical();
        GUILayout.Label($"Current Version: {currentVersion}");
        GUILayout.Label($"Latest Available Version: {latestVersion}");
        GUILayout.EndVertical();
        GUILayout.Space(10f);
        GUILayout.BeginVertical();

        if(GUILayout.Button("Check For Update"))
        {
            if (checkForUpdateInProcess)
                return;

            CheckForUpdate();
        }

        GUILayout.Space(5f);

        if(isUpdateAvailable)
            GUILayout.Label($"New Update is available");
        else if(!isUpdateAvailable && !checkForUpdateInProcess)
            GUILayout.Label($"Package is upto date!");

        if (isUpdateAvailable)
        {
            
            if (GUILayout.Button("Download Update"))
            {
                if (downloadInProcess)
                    return;

                DownloadPackage();
            }
        }

        GUILayout.Space(10f);
        if (downloadInProcess)
        {
            GUILayout.Label("Downloading...");
        }
           


        GUILayout.EndVertical();
    }

    private void OnEnable()
    {
        //Check For Latest Version
        CheckForUpdate();

        

    }

    private void OnDisable()
    {
        
    }

    private void CheckForUpdate()
    {
        //Get current version
        currentVersion = LoadCurrentVersion();

        isUpdateAvailable = false;
        checkForUpdateInProcess = true;
        latestVersion = "Checking For Latest Version";
        Task.Run(async () => await CheckForUpdateAsync(OnCheckForUpdateFailed, OnCheckForUpdateCompleted));
    }

    private string LoadCurrentVersion()
    {
        currentVersionPath = Application.dataPath + "/Plugins/Tomo_MultiplayerGDK/version.txt";
        string versionText = File.ReadAllText(currentVersionPath);
        return versionText.Trim();
    }


    private int IsUpdateAvailable(string latest, string current)
    {
        // Implement your version comparison logic here
        // This is a simple example assuming the version format is major.minor.patch


        string[] v1 = latest.Split('.');
        string[] v2 = current.Split('.');

        if (v1.Length > 3 || v2.Length > 3)
        {
            Debug.LogError("Version format is not semantic in nature");

        }
            

        for (int i = 0; i < v1.Length; i++)
        {
            int v1Part = int.Parse(v1[i]);
            int v2Part = int.Parse(v2[i]);

            if (v1Part != v2Part)
                return v1Part - v2Part;
        }

        return 0;
    }


    private async Task CheckForUpdateAsync(Action OnCheckForUpdateFailed, Action<string,string> OnCheckForUpdateCompleted)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                string latestURL = latestURLTemplate.Replace("{owner}", owner).Replace("{repo}", repo);
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {personalAccessToken}");

                string jsonResponse = await client.GetStringAsync(latestURL);
                
                //Parse the JSON
                ReleaseData releaseData = JsonUtility.FromJson<ReleaseData>(jsonResponse);

                //Return if no download link is found
                if (!RecivedReleaseData(releaseData))
                {
                    OnCheckForUpdateFailed?.Invoke();
                    return;
                }

                OnCheckForUpdateCompleted?.Invoke(releaseData.tag_name, releaseData.assets[0].url);
                
                    
            }
            catch (Exception e)
            {
                // Handle any exceptions that occurred during the request
                Debug.LogError("Error: " + e.Message);
                OnCheckForUpdateFailed?.Invoke();

            }
        }

    }

    private bool RecivedReleaseData(ReleaseData releaseData)
    {
        if (releaseData == null)
        {
            Debug.LogError("Parsing the Json Failed");
            return false;
        }

        if (string.IsNullOrEmpty(releaseData.tag_name))
        {
            Debug.LogError("No tag name found for the release");
            return false;
        }

        if (releaseData.assets == null || releaseData.assets.Length == 0)
        {
            Debug.LogWarning($"No assets found for the release");
            return false;
        }

        if (string.IsNullOrEmpty(releaseData.assets[0].name))
            Debug.LogWarning("No name foundfor the package");

        if (string.IsNullOrEmpty(releaseData.assets[0].url))
        {
            Debug.LogError("No Download Url found for the given package");
            return false;
        }

        return true;
    }

    private void OnCheckForUpdateCompleted(string latestVersion, string downloadURL)
    {
        checkForUpdateInProcess = false;
        this.latestVersion = latestVersion;
        this.downloadURL = downloadURL;

        isUpdateAvailable = IsUpdateAvailable(latestVersion, currentVersion) > 0;
        
    }

    private void OnCheckForUpdateFailed()
    {
        checkForUpdateInProcess = false;
        latestVersion = "Check For Update Failed Retry";
    }

    private void DownloadPackage()
    {
        //Reset on button click
        downloadInProcess = true;
        isDownloaded = false;

        downloadPath = Application.dataPath + "/Tomo_MultiplayerGDK.unitypackage";
        Task.Run(async () => await DownloadPackageAsync(downloadPath,
            OnDownloadFailed));

        EditorApplication.update += UpdateProgress;

    
    }


    private async Task DownloadPackageAsync(string path, Action OnDownloadFailed)
    {    

        using (HttpClient assetClient = new HttpClient())
        {
            try
            {
                assetClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                assetClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {personalAccessToken}");
                assetClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

                HttpResponseMessage downloadResponse = await assetClient.GetAsync(downloadURL);
                downloadResponse.EnsureSuccessStatusCode();
                using (Stream contentStream = await downloadResponse.Content.ReadAsStreamAsync())
                {
                    using (FileStream fileStream = File.Create(path))
                    {
                        const int bufferSize = 8192;
                        byte[] buffer = new byte[bufferSize];
                        long totalBytesRead = 0;
                        int bytesRead;
                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, bufferSize)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                            progress = totalBytesRead / (float)downloadResponse.Content.Headers.ContentLength;
                            progressText = $"Downloaded {totalBytesRead} bytes of {downloadResponse.Content.Headers.ContentLength} bytes";

                        }
                    }

                    isDownloaded = true;


                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occurred during package download: {e.Message}");
                OnDownloadFailed?.Invoke();

            }

        }

            

    }


    private void UpdateProgress()
    {
        if (isDownloaded)
        {
            // Unsubscribe from the EditorApplication.update event
            EditorApplication.update -= UpdateProgress;

            AssetDatabase.ImportPackage(downloadPath, true);

            File.Delete(downloadPath);

            downloadInProcess = false;
            isUpdateAvailable = false;
        }
    }

    private void OnDownloadFailed()
    {

        downloadInProcess = false;
        isDownloaded = false;

        // Unsubscribe from the EditorApplication.update event
        EditorApplication.update -= UpdateProgress;

    }


}


// Classes to deserialize the JSON response
[Serializable]
public class ReleaseData
{
    public AssetData[] assets;
    public string tag_name;
}

[Serializable]
public class AssetData
{
    public string name;
    public string url;
}
#endif