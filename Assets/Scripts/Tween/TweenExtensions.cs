using System;
using UnityEngine;
using UnityEngine.UI;

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
        Action<float> setter = volume => { if(audioSource != null) audioSource.volume = volume; }; 
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

    /// <summary>
    /// Tweens the local rotation of a Transform using Quaternions for smooth interpolation.
    /// </summary>
    public static Coroutine TweenLocalRotation(this Transform transform, Quaternion targetRotation, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null. Cannot tween local rotation.");
            return null;
        }
        Quaternion startRotation = transform.localRotation;
        Action<Quaternion> setter = rot => { if(transform != null) transform.localRotation = rot; };
        return Tween.Quaternion(transform, "LocalRotation", setter, startRotation, targetRotation, duration, onComplete, easeFunction);
    }

    /// <summary>
    /// Tweens the local rotation of a Transform using Euler angles.
    /// Note: Tweening Euler angles directly can sometimes lead to gimbal lock or unexpected paths for large rotations.
    /// Prefer TweenLocalRotation (Quaternion) for complex or large rotations.
    /// </summary>
    public static Coroutine TweenLocalEulerAngles(this Transform transform, Vector3 targetEulerAngles, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null. Cannot tween local Euler angles.");
            return null;
        }
        Vector3 startEulerAngles = transform.localEulerAngles;
        Action<Vector3> setter = euler => { if(transform != null) transform.localEulerAngles = euler; };
        // Internally, this will Lerp the Vector3 Euler angles.
        return Tween.Vector3(transform, "LocalEulerAngles", setter, startEulerAngles, targetEulerAngles, duration, onComplete, easeFunction);
    }

    // --- RectTransform Specific Extensions ---

    /// <summary>
    /// Tweens the anchoredPosition of a RectTransform.
    /// </summary>
    public static Coroutine TweenAnchoredPosition(this RectTransform rectTransform, Vector2 targetPosition, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform is null. Cannot tween anchored position.");
            return null;
        }
        Vector2 startPosition = rectTransform.anchoredPosition;
        Action<Vector2> setter = pos => { if(rectTransform != null) rectTransform.anchoredPosition = pos; };
        return Tween.Vector2(rectTransform, "AnchoredPosition", setter, startPosition, targetPosition, duration, onComplete, easeFunction);
    }

    /// <summary>
    /// Tweens the sizeDelta of a RectTransform.
    /// </summary>
    public static Coroutine TweenSizeDelta(this RectTransform rectTransform, Vector2 targetSize, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform is null. Cannot tween size delta.");
            return null;
        }
        Vector2 startSize = rectTransform.sizeDelta;
        Action<Vector2> setter = size => { if(rectTransform != null) rectTransform.sizeDelta = size; };
        return Tween.Vector2(rectTransform, "SizeDelta", setter, startSize, targetSize, duration, onComplete, easeFunction);
    }
    
    public static Coroutine TweenMaterialFloat(this Material material, string propertyName, float targetValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (material == null)
        {
            Debug.LogError("Material is null. Cannot tween material float.");
            return null;
        }
        if (string.IsNullOrEmpty(propertyName))
        {
            Debug.LogError("Property name for material tween cannot be null or empty.");
            return null;
        }
        if (!material.HasProperty(propertyName)) {
             Debug.LogError($"Material '{material.name}' does not have a float property named '{propertyName}'.");
            return null;
        }

        float startValue = material.GetFloat(propertyName);
        Action<float> setter = value => { if(material != null) material.SetFloat(propertyName, value); };
        return Tween.Float(material, $"MaterialFloat_{propertyName}", setter, startValue, targetValue, duration, onComplete, easeFunction);
    }

    public static Coroutine TweenImageColor(this Image image, Color targetColor, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (image == null)
        {
            Debug.LogError("Image is null. Cannot tween Color.");
            return null;
        }
        Color startColor = image.color;
        Action<Color> setter = color => { if(image != null) image.color = color; };
        return Tween.Color(image, "Color", setter, startColor, targetColor, duration, onComplete, easeFunction);
    }
}
