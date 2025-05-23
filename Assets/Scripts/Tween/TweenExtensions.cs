using System;
using UnityEngine;

/// <summary>
/// Extension methods for conveniently starting tweens on common Unity objects.
/// </summary>
public static class TweenExtensions
{
    /// <summary>
    /// Tweens the volume of an AudioSource.
    /// </summary>
    /// <param name="audioSource">The AudioSource to affect.</param>
    /// <param name="targetVolume">The target volume (0.0 to 1.0).</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="onComplete">Optional action on completion.</param>
    /// <param name="easeFunction">Optional easing function.</param>
    /// <returns>Coroutine reference.</returns>
    public static Coroutine TweenVolume(this AudioSource audioSource, float targetVolume, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null. Cannot tween volume.");
            return null;
        }
        float startVolume = audioSource.volume;
        Action<float> setter = volume => audioSource.volume = volume;
        return Tween.Float(setter, startVolume, targetVolume, duration, onComplete, easeFunction);
    }

    /// <summary>
    /// Tweens the alpha of a CanvasGroup.
    /// </summary>
    /// <param name="canvasGroup">The CanvasGroup to affect.</param>
    /// <param name="targetAlpha">The target alpha (0.0 to 1.0).</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="onComplete">Optional action on completion.</param>
    /// <param name="easeFunction">Optional easing function.</param>
    /// <returns>Coroutine reference.</returns>
    public static Coroutine TweenAlpha(this CanvasGroup canvasGroup, float targetAlpha, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is null. Cannot tween alpha.");
            return null;
        }
        float startAlpha = canvasGroup.alpha;
        Action<float> setter = alpha => canvasGroup.alpha = alpha;
        return Tween.Float(setter, startAlpha, targetAlpha, duration, onComplete, easeFunction);
    }

    /// <summary>
    /// Tweens the position of a Transform (local space).
    /// </summary>
    /// <param name="transform">The Transform to move.</param>
    /// <param name="targetPosition">The target local position.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="onComplete">Optional action on completion.</param>
    /// <param name="easeFunction">Optional easing function.</param>
    /// <returns>Coroutine reference.</returns>
    public static Coroutine TweenLocalPosition(this Transform transform, Vector3 targetPosition, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null. Cannot tween local position.");
            return null;
        }
        Vector3 startPosition = transform.localPosition;
        Action<Vector3> setter = pos => transform.localPosition = pos;
        return Tween.Vector3(setter, startPosition, targetPosition, duration, onComplete, easeFunction);
    }

     /// <summary>
    /// Tweens the scale of a Transform.
    /// </summary>
    /// <param name="transform">The Transform to scale.</param>
    /// <param name="targetScale">The target local scale.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="onComplete">Optional action on completion.</param>
    /// <param name="easeFunction">Optional easing function.</param>
    /// <returns>Coroutine reference.</returns>
    public static Coroutine TweenLocalScale(this Transform transform, Vector3 targetScale, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (transform == null)
        {
            Debug.LogError("Transform is null. Cannot tween local scale.");
            return null;
        }
        Vector3 startScale = transform.localScale;
        Action<Vector3> setter = scale => transform.localScale = scale;
        return Tween.Vector3(setter, startScale, targetScale, duration, onComplete, easeFunction);
    }

    public static Coroutine TweenMaterialFloat(this Material material, string variable, float target, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (material == null)
        {
            Debug.LogError("Either the material is null or you are trying to tween a non instance.");
            return null;
        }
        float startValue = material.GetFloat(variable);
        Action<float> setter = value => material.SetFloat(variable, value);
        return Tween.Float(setter, startValue, target, duration,onComplete, easeFunction);

    }
}
