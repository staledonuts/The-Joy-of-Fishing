using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tween
{
    private static TweenRunner _runner;
    private static readonly Dictionary<Tuple<object, string>, Coroutine> _activeTweens = new Dictionary<Tuple<object, string>, Coroutine>();

    private static TweenRunner Runner
    {
        get
        {
            if (_runner == null)
            {
                _runner = UnityEngine.Object.FindFirstObjectByType<TweenRunner>();
                if (_runner == null)
                {
                    GameObject runnerObject = new GameObject("TweenRunner_Singleton");
                    _runner = runnerObject.AddComponent<TweenRunner>();
                    UnityEngine.Object.DontDestroyOnLoad(runnerObject);
                }
            }
            return _runner;
        }
    }

    private static Tuple<object, string> CreateKey(object owner, string tweenIdentifierTag)
    {
        return Tuple.Create(owner, tweenIdentifierTag ?? string.Empty);
    }

    public static Coroutine Float(object owner, string tweenIdentifierTag, Action<float> setter, float startValue, float endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        var key = CreateKey(owner, tweenIdentifierTag);
        StopTweenForKey(key);

        if (duration <= 0)
        {
            setter(endValue);
            onComplete?.Invoke();
            return null;
        }

        Coroutine newCoroutine = Runner.StartCoroutine(TweenValueCoroutine(key, startValue, endValue, duration, setter, Mathf.LerpUnclamped, onComplete, easeFunction));
        if (newCoroutine != null)
        {
            _activeTweens[key] = newCoroutine;
        }
        return newCoroutine;
    }

    public static Coroutine Vector2(object owner, string tweenIdentifierTag, Action<Vector2> setter, Vector2 startValue, Vector2 endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        var key = CreateKey(owner, tweenIdentifierTag);
        StopTweenForKey(key);

        if (duration <= 0)
        {
            setter(endValue);
            onComplete?.Invoke();
            return null;
        }

        Coroutine newCoroutine = Runner.StartCoroutine(TweenValueCoroutine(key, startValue, endValue, duration, setter, UnityEngine.Vector2.LerpUnclamped, onComplete, easeFunction));
        if (newCoroutine != null)
        {
            _activeTweens[key] = newCoroutine;
        }
        return newCoroutine;
    }

    public static Coroutine Vector3(object owner, string tweenIdentifierTag, Action<Vector3> setter, Vector3 startValue, Vector3 endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        var key = CreateKey(owner, tweenIdentifierTag);
        StopTweenForKey(key);

        if (duration <= 0)
        {
            setter(endValue);
            onComplete?.Invoke();
            return null;
        }

        Coroutine newCoroutine = Runner.StartCoroutine(TweenValueCoroutine(key, startValue, endValue, duration, setter, UnityEngine.Vector3.LerpUnclamped, onComplete, easeFunction));
        if (newCoroutine != null)
        {
            _activeTweens[key] = newCoroutine;
        }
        return newCoroutine;
    }
    
    public static Coroutine Quaternion(object owner, string tweenIdentifierTag, Action<Quaternion> setter, Quaternion startValue, Quaternion endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        var key = CreateKey(owner, tweenIdentifierTag);
        StopTweenForKey(key);

        if (duration <= 0)
        {
            setter(endValue);
            onComplete?.Invoke();
            return null;
        }
        
        Coroutine newCoroutine = Runner.StartCoroutine(TweenValueCoroutine(key, startValue, endValue, duration, setter, UnityEngine.Quaternion.SlerpUnclamped, onComplete, easeFunction));
        if (newCoroutine != null)
        {
            _activeTweens[key] = newCoroutine;
        }
        return newCoroutine;
    }

    public static Coroutine Color(object owner, string tweenIdentifierTag, Action<Color> setter, Color startValue, Color endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null)
    {
        var key = CreateKey(owner, tweenIdentifierTag);
        StopTweenForKey(key);

        if (duration <= 0)
        {
            setter(endValue);
            onComplete?.Invoke();
            return null;
        }
        Coroutine newCoroutine = Runner.StartCoroutine(TweenValueCoroutine(key, startValue, endValue, duration, setter, UnityEngine.Color.LerpUnclamped, onComplete, easeFunction));
        if (newCoroutine != null)
        {
           _activeTweens[key] = newCoroutine;
        }
        return newCoroutine;
    }
    
    private static void StopTweenForKey(Tuple<object, string> key)
    {
        if (_activeTweens.TryGetValue(key, out Coroutine existingCoroutine))
        {
            if (existingCoroutine != null && Runner != null && Runner.gameObject.activeInHierarchy) // Check Runner and its active state
            {
                Runner.StopCoroutine(existingCoroutine);
            }
            _activeTweens.Remove(key); 
        }
    }

    public static void StopTween(object owner, string tweenIdentifierTag)
    {
        var key = CreateKey(owner, tweenIdentifierTag);
        StopTweenForKey(key);
    }

    private static IEnumerator TweenValueCoroutine<T>(Tuple<object, string> key, T from, T to, float duration, Action<T> setter, Func<T, T, float, T> interpolator, Action onComplete, Func<float, float> easeFunction)
    {
        float elapsedTime = 0f;
        if (easeFunction == null) easeFunction = progress => progress; // Linear

        try
        {
            while (elapsedTime < duration)
            {
                if (Runner == null || !Runner.gameObject.activeInHierarchy) yield break;

                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration);
                float easedProgress = easeFunction(progress);
                
                // Safety check for setter target (owner of the tween)
                var owner = key.Item1 as UnityEngine.Object;
                if (owner == null && key.Item1 != null) // Owner is not a UnityEngine.Object, cannot check if destroyed easily. Assume it's alive.
                {
                     setter(interpolator(from, to, easedProgress));
                }
                else if (owner != null) // It is a UnityEngine.Object, check if it was destroyed
                {
                    setter(interpolator(from, to, easedProgress));
                }
                else // Owner was null from the start, or became null (if not UnityEngine.Object and GC'd)
                {
                     yield break; // Stop if owner is gone
                }
                yield return null;
            }

            var finalOwner = key.Item1 as UnityEngine.Object;
            if (finalOwner != null || (key.Item1 != null && !(key.Item1 is UnityEngine.Object))) // Check if owner still valid for final set
            {
                setter(to); 
            }
            onComplete?.Invoke();
        }
        finally
        {
            if (_activeTweens.TryGetValue(key, out Coroutine currentCoroutine) && ReferenceEquals(currentCoroutine, _activeTweens[key]))
            {
                 _activeTweens.Remove(key);
            }
        }
    }
    
    public static void StopAndClearAllManagedTweens()
    {
        if (_runner != null && _runner.gameObject.activeInHierarchy) 
        {
            List<Tuple<object, string>> keys = new List<Tuple<object, string>>(_activeTweens.Keys);
            foreach (var key in keys)
            {
                if (_activeTweens.TryGetValue(key, out Coroutine coroutineToStop))
                {
                    if (coroutineToStop != null)
                    {
                        _runner.StopCoroutine(coroutineToStop);
                    }
                }
            }
        }
        _activeTweens.Clear();
    }

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
