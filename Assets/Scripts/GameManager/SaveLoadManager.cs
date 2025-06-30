using UnityEngine;
using System;
using System.IO;
using Cysharp.Threading.Tasks; // Import UniTask

public sealed class SaveLoadManager : MonoBehaviour
{
    private static SaveLoadManager instance;
    public static SaveLoadManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<SaveLoadManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("SaveLoadManager_Singleton");
                    instance = obj.AddComponent<SaveLoadManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    public static event Func<UniTask> OnSaveStarted;

    private string _savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            _savePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Asynchronously loads player data from the persistent data path.
    /// </summary>
    /// <returns>A UniTask containing the loaded or new PlayerData.</returns>
    public async UniTask<PlayerData> LoadDataAsync()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                // Asynchronously read the file content
                string json = await File.ReadAllTextAsync(_savePath);
                PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);
                if (loadedData != null)
                {
                    Debug.Log("Player data loaded successfully.");
                    return loadedData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load player data: {e.Message}. A new game will be started.");
            }
        }
        
        Debug.Log("No save file found. Creating new player data.");
        return new PlayerData();
    }

    /// <summary>
    /// Asynchronously saves the provided PlayerData object to a JSON file.
    /// </summary>
    /// <param name="playerData">The player data to save.</param>
    public async UniTask SaveDataAsync(PlayerData playerData)
    {
        if (playerData == null)
        {
            Debug.LogError("Cannot save null player data.");
            return;
        }

        // Invoke the event for any listeners and let it run independently.
        OnSaveStarted?.Invoke().Forget();

        try
        {
            string json = JsonUtility.ToJson(playerData, true);
            await File.WriteAllTextAsync(_savePath, json);
            Debug.Log($"Player data saved to {_savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save player data: {e.Message}");
        }
    }
}