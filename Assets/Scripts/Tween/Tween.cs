using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// A basic static class for creating value tweens over time.
/// </summary>
public static class Tween
{
    private static TweenRunner _runner;

    // Ensures a TweenRunner instance exists in the scene to run coroutines
    private static TweenRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                // Try to find an existing runner
                _runner = UnityEngine.Object.FindFirstObjectByType<TweenRunner>();
                if (_runner == null)
                {
                    // If no runner exists, create one
                    GameObject runnerObject = new GameObject("TweenRunner_Singleton");
                    _runner = runnerObject.AddComponent<TweenRunner>();
                    UnityEngine.Object.DontDestroyOnLoad(runnerObject); // Make it persistent
                }
            }
            return _runner;
        }
    }

    /// <summary>
    /// Tweens a float value.
    /// </summary>
    /// <param name="setter">Action to set the float value each frame (e.g., val => myFloat = val).</param>
    /// <param name="startValue">The starting value of the float.</param>
    /// <param name="endValue">The target value of the float.</param>
    /// <param name="duration">How long the tween should take in seconds.</param>
    /// <param name="onComplete">Optional action to call when the tween finishes.</param>
    /// <param name="easeFunction">Optional easing function (progress => easedProgress). Defaults to linear.</param>
    /// <returns>Coroutine reference, can be used to stop the tween manually if needed.</returns>
    public static Coroutine Float(Action<float> setter, float startValue, float endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (duration <= 0)
        {
            setter(endValue);
            onComplete?.Invoke();
            return null;
        }
        return Runner.StartCoroutine(TweenValueCoroutine(startValue, endValue, duration, setter, Mathf.LerpUnclamped, onComplete, easeFunction));
    }

    /// <summary>
    /// Tweens a Vector3 value.
    /// </summary>
    /// <param name="setter">Action to set the Vector3 value each frame.</param>
    /// <param name="startValue">The starting value.</param>
    /// <param name="endValue">The target value.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="onComplete">Optional action on completion.</param>
    /// <param name="easeFunction">Optional easing function.</param>
    /// <returns>Coroutine reference.</returns>
    public static Coroutine Vector3(Action<Vector3> setter, Vector3 startValue, Vector3 endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (duration <= 0)
        {
            setter(endValue);
            onComplete?.Invoke();
            return null;
        }
        return Runner.StartCoroutine(TweenValueCoroutine(startValue, endValue, duration, setter, UnityEngine.Vector3.LerpUnclamped, onComplete, easeFunction));
    }

    /// <summary>
    /// Tweens a Color value.
    /// </summary>
    /// <param name="setter">Action to set the Color value each frame.</param>
    /// <param name="startValue">The starting value.</param>
    /// <param name="endValue">The target value.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="onComplete">Optional action on completion.</param>
    /// <param name="easeFunction">Optional easing function.</param>
    /// <returns>Coroutine reference.</returns>
    public static Coroutine Color(Action<Color> setter, Color startValue, Color endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        if (duration <= 0)
        {
            setter(endValue);
            onComplete?.Invoke();
            return null;
        }
        return Runner.StartCoroutine(TweenValueCoroutine(startValue, endValue, duration, setter, UnityEngine.Color.LerpUnclamped, onComplete, easeFunction));
    }

    /// <summary>
    /// Generic coroutine to tween a value of type T.
    /// </summary>
    /// <typeparam name="T">The type of value to tween.</typeparam>
    /// <param name="from">The starting value.</param>
    /// <param name="to">The target value.</param>
    /// <param name="duration">How long the tween should take.</param>
    /// <param name="setter">Action to call each frame to update the value.</param>
    /// <param name="interpolator">Function to interpolate between 'from' and 'to' based on progress (0-1).</param>
    /// <param name="onComplete">Action to call when the tween is complete.</param>
    /// <param name="easeFunction">Optional easing function that takes normalized time (0-1) and returns eased time (0-1).</param>
    private static IEnumerator TweenValueCoroutine<T>(T from, T to, float duration, Action<T> setter, Func<T, T, float, T> interpolator, Action onComplete, Func<float, float> easeFunction)
    {
        float elapsedTime = 0f;

        // Default to linear easing if no ease function is provided
        if (easeFunction == null)
        {
            easeFunction = progress => progress; // Linear easing
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);
            float easedProgress = easeFunction(progress); // Apply easing

            setter(interpolator(from, to, easedProgress));
            yield return null; // Wait for the next frame
        }

        // Ensure the final value is set exactly
        setter(to);
        onComplete?.Invoke(); // Call the onComplete action if it exists
    }

    // --- Example Easing Functions (can be expanded) ---
    public static class Easing
    {
        public static float Linear(float p) => p;
        public static float EaseInQuad(float p) => p * p;
        public static float EaseOutQuad(float p) => p * (2 - p);
        public static float EaseInOutQuad(float p) => p < 0.5f ? 2 * p * p : -1 + (4 - 2 * p) * p;
        public static float EaseInCubic(float p) => p * p * p;
        public static float EaseOutCubic(float p) => (--p) * p * p + 1;
        public static float EaseInOutCubic(float p) => p < 0.5f ? 4 * p * p * p : (p - 1) * (2 * p - 2) * (2 * p - 2) + 1;
    }
}
