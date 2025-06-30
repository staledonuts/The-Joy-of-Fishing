using UnityEngine;
using DonutEngine;
using Cysharp.Threading.Tasks;

public class SaveNotifierController : MonoBehaviour
{
    [SerializeField] private SaveNotifier _saveNotifier;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Subscribe to the correct event from your SaveLoadManager.
        SaveLoadManager.OnSaveStarted += HandleSaveStarted;
    }

    private void OnDisable()
    {
        // Always unsubscribe from the same event.
        SaveLoadManager.OnSaveStarted -= HandleSaveStarted;
    }

    private async UniTask HandleSaveStarted()
    {
        if (_saveNotifier != null)
        {
            await _saveNotifier.ShowNotifier();
        }
        else
        {
            Debug.LogError("SaveNotifier reference is not set in the SaveNotifierController inspector!");
        }
    }
}