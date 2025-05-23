using System;
using UnityEngine;

public static class TweenExtensions
{
    public static Coroutine TweenVolume(this AudioSource audioSource, float targetVolume, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null. Cannot tween volume.");
            return null;
        }
        float startVolume = audioSource.volume;
        Action<float> setter = volume => { if(audioSource != null) audioSource.volume = volume; }; // Add null check for safety
        return Tween.Float(audioSource, "Volume", setter, startVolume, targetVolume, duration, onComplete, easeFunction);
    }

    public static Coroutine TweenAlpha(this CanvasGroup canvasGroup, float targetAlpha, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null. Cannot tween alpha.");
            return null;
        }
        float startAlpha = canvasGroup.alpha;
        Action<float> setter = alpha => { if(canvasGroup != null) canvasGroup.alpha = alpha; };
        return Tween.Float(canvasGroup, "Alpha", setter, startAlpha, targetAlpha, duration, onComplete, easeFunction);
    }

    public static Coroutine TweenLocalPosition(this Transform transform, Vector3 targetPosition, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null. Cannot tween local position.");
            return null;
        }
        Vector3 startPosition = transform.localPosition;
        Action<Vector3> setter = pos => { if(transform != null) transform.localPosition = pos; };
        return Tween.Vector3(transform, "LocalPosition", setter, startPosition, targetPosition, duration, onComplete, easeFunction);
    }

    public static Coroutine TweenLocalScale(this Transform transform, Vector3 targetScale, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null. Cannot tween local scale.");
            return null;
        }
        Vector3 startScale = transform.localScale;
        Action<Vector3> setter = scale => { if(transform != null) transform.localScale = scale; };
        return Tween.Vector3(transform, "LocalScale", setter, startScale, targetScale, duration, onComplete, easeFunction);
    }

    public static Coroutine TweenMaterialFloat(this Material material, string propertyName, float targetValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (material == null)
        {
            Debug.LogError("Material is null. Cannot tween material float.");
            return null;
        }
        // Ensure propertyName is not null or empty for a valid key
        if (string.IsNullOrEmpty(propertyName))
        {
            Debug.LogError("Property name for material tween cannot be null or empty.");
            return null;
        }
        // Check if material has the float property before trying to get it
        if (!material.HasProperty(propertyName)) {
            // Some shaders might not expose float that way, or it might be a color component etc.
            // Unity often uses string IDs that are prefixed with an underscore.
            // Check if an int ID is needed. For simplicity, assuming string name works.
             Debug.LogError($"Material '{material.name}' does not have a float property named '{propertyName}'.");
            return null;
        }

        float startValue = material.GetFloat(propertyName);
        Action<float> setter = value => { if(material != null) material.SetFloat(propertyName, value); };
        // Use the material instance and the propertyName to create a unique key
        return Tween.Float(material, $"MaterialFloat_{propertyName}", setter, startValue, targetValue, duration, onComplete, easeFunction);
    }

    // You could add TweenMaterialColor similarly:
    // public static Coroutine TweenMaterialColor(this Material material, string propertyName, Color targetValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    // {
    //     // ... null checks, propertyName check, material.HasProperty(propertyName) ...
    //     Color startValue = material.GetColor(propertyName);
    //     Action<Color> setter = value => { if(material != null) material.SetColor(propertyName, value); };
    //     return Tween.Color(material, $"MaterialColor_{propertyName}", setter, startValue, targetValue, duration, onComplete, easeFunction);
    // }
}
