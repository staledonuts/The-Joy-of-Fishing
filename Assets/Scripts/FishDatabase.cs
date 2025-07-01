using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FishDatabase", menuName = "Fishing/Fish Database", order = 1)]
public class FishDatabase : ScriptableObject
{
    [SerializeField]
    private List<FishStats> _fishPrefabs = new List<FishStats>();

    private Dictionary<string, Sprite> _spriteDatabase;

    // This method is called when the ScriptableObject is loaded.
    private void OnEnable()
    {
        _spriteDatabase = new Dictionary<string, Sprite>();
        foreach (var fishPrefab in _fishPrefabs)
        {
            if (fishPrefab != null && !_spriteDatabase.ContainsKey(fishPrefab.FishName))
            {
                _spriteDatabase.Add(fishPrefab.FishName, fishPrefab.FishSprite);
            }
        }
    }

    /// <summary>
    /// Gets a fish sprite from the database by its name.
    /// </summary>
    public Sprite GetSprite(string fishName)
    {
        _spriteDatabase.TryGetValue(fishName, out Sprite fishSprite);
        return fishSprite; // Returns the sprite, or null if not found
    }
}
