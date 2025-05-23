using UnityEngine;

/// <summary>
/// Internal helper MonoBehaviour to run tweening coroutines.
/// This is automatically created and managed by the Tween class.
/// </summary>
[AddComponentMenu("")] // Hides it from the Add Component menu
internal sealed class TweenRunner : MonoBehaviour
{
    private void OnDestroy()
    {
        // If the runner is destroyed, attempt to clean up active tweens
        // to prevent issues if tweens were ongoing.
        Tween.StopAndClearAllManagedTweens();
    }
}
