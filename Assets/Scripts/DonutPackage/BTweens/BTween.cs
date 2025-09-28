using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using DonutPackage.BTween;
using DonutPackage.Utils;
#if UNITY_EDITOR
using DonutPackage.BTween.Editor;
#endif

namespace DonutPackage.BTween
{
    public static class BTween
    {
        #region Public API Overloads (uint hash)
        /// <summary>
        /// Starts a tween for a float value using a uint hash identifier.
        /// </summary>
        /// <param name="owner">The object that owns this tween. Used for identification and automatic cancellation on destruction.</param>
        /// <param name="tweenIdentifierHash">A uint hash to uniquely identify this tween on the owner object.</param>
        /// <param name="setter">The action that applies the tweened value each frame.</param>
        /// <param name="startValue">The starting value of the tween.</param>
        /// <param name="endValue">The target value of the tween.</param>
        /// <param name="duration">The duration of the tween in seconds.</param>
        /// <param name="onComplete">An optional action to invoke when the tween completes.</param>
        /// <param name="easeFunction">The easing function to use for the tween's progression.</param>
        /// <param name="onCompleteDelay">An optional delay in seconds before invoking the onComplete action.</param>
        /// <param name="ignoreTimeScale">If true, the tween will ignore Time.timeScale and use unscaled time.</param>
        /// <returns>A UniTask that completes when the tween is finished or cancelled.</returns>
        public static UniTask Float(object owner, uint tweenIdentifierHash, Action<float> setter, float startValue, float endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, Mathf.LerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }

        /// <summary>
        /// Starts a tween for a Vector2 value using a uint hash identifier.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Vector2(object owner, uint tweenIdentifierHash, Action<Vector2> setter, Vector2 startValue, Vector2 endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, UnityEngine.Vector2.LerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }

        /// <summary>
        /// Starts a tween for a Vector3 value using a uint hash identifier.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Vector3(object owner, uint tweenIdentifierHash, Action<Vector3> setter, Vector3 startValue, Vector3 endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, UnityEngine.Vector3.LerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }

        /// <summary>
        /// Starts a tween for a Quaternion value using a uint hash identifier. Uses Slerp for correct rotational interpolation.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Quaternion(object owner, uint tweenIdentifierHash, Action<Quaternion> setter, Quaternion startValue, Quaternion endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, UnityEngine.Quaternion.SlerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }

        /// <summary>
        /// Starts a tween for a Color value using a uint hash identifier.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Color(object owner, uint tweenIdentifierHash, Action<Color> setter, Color startValue, Color endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, UnityEngine.Color.LerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }
        #endregion

        #region Public API Overloads (string tag - for convenience)
        /// <summary>
        /// Starts a tween for a float value. Hashes the string tag to a uint for internal use.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Float(object owner, string tweenIdentifierTag, Action<float> setter, float startValue, float endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Float(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        
        /// <summary>
        /// Starts a tween for a Vector2 value. Hashes the string tag to a uint for internal use.
        /// </summary>
        /// <inheritdoc cref="Float(object, string, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Vector2(object owner, string tweenIdentifierTag, Action<Vector2> setter, Vector2 startValue, Vector2 endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Vector2(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);

        /// <summary>
        /// Starts a tween for a Vector3 value. Hashes the string tag to a uint for internal use.
        /// </summary>
        /// <inheritdoc cref="Float(object, string, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Vector3(object owner, string tweenIdentifierTag, Action<Vector3> setter, Vector3 startValue, Vector3 endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Vector3(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);

        /// <summary>
        /// Starts a tween for a Quaternion value. Hashes the string tag to a uint for internal use. Uses Slerp for correct rotational interpolation.
        /// </summary>
        /// <inheritdoc cref="Float(object, string, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Quaternion(object owner, string tweenIdentifierTag, Action<Quaternion> setter, Quaternion startValue, Quaternion endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Quaternion(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);

        /// <summary>
        /// Starts a tween for a Color value. Hashes the string tag to a uint for internal use.
        /// </summary>
        /// <inheritdoc cref="Float(object, string, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Color(object owner, string tweenIdentifierTag, Action<Color> setter, Color startValue, Color endValue, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Color(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        #endregion

        #region Public API Overloads (AnimationCurve)
        /// <summary>
        /// Starts a tween for a float value using a uint hash identifier and an AnimationCurve.
        /// </summary>
        /// <param name="easeCurve">The AnimationCurve to use for the tween's progression.</param>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, Func{float, float}, float, bool)"/>
        public static UniTask Float(object owner, uint tweenIdentifierHash, Action<float> setter, float startValue, float endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            Func<float, float> easeFunction = (easeCurve == null) ? Ease.Linear : easeCurve.Evaluate;
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, Mathf.LerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }

        /// <summary>
        /// Starts a tween for a Vector2 value using a uint hash identifier and an AnimationCurve.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, AnimationCurve, float, bool)"/>
        public static UniTask Vector2(object owner, uint tweenIdentifierHash, Action<Vector2> setter, Vector2 startValue, Vector2 endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            Func<float, float> easeFunction = (easeCurve == null) ? Ease.Linear : easeCurve.Evaluate;
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, UnityEngine.Vector2.LerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }

        /// <summary>
        /// Starts a tween for a Vector3 value using a uint hash identifier and an AnimationCurve.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, AnimationCurve, float, bool)"/>
        public static UniTask Vector3(object owner, uint tweenIdentifierHash, Action<Vector3> setter, Vector3 startValue, Vector3 endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            Func<float, float> easeFunction = (easeCurve == null) ? Ease.Linear : easeCurve.Evaluate;
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, UnityEngine.Vector3.LerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }

        /// <summary>
        /// Starts a tween for a Quaternion value using a uint hash identifier and an AnimationCurve.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, AnimationCurve, float, bool)"/>
        public static UniTask Quaternion(object owner, uint tweenIdentifierHash, Action<Quaternion> setter, Quaternion startValue, Quaternion endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            Func<float, float> easeFunction = (easeCurve == null) ? Ease.Linear : easeCurve.Evaluate;
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, UnityEngine.Quaternion.SlerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }

        /// <summary>
        /// Starts a tween for a Color value using a uint hash identifier and an AnimationCurve.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, AnimationCurve, float, bool)"/>
        public static UniTask Color(object owner, uint tweenIdentifierHash, Action<Color> setter, Color startValue, Color endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
        {
            Func<float, float> easeFunction = (easeCurve == null) ? Ease.Linear : easeCurve.Evaluate;
            return StartTween(owner, tweenIdentifierHash, startValue, endValue, duration, setter, UnityEngine.Color.LerpUnclamped, onComplete, easeFunction, onCompleteDelay, ignoreTimeScale);
        }

        /// <summary>
        /// Starts a tween for a float value using a string tag and an AnimationCurve.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, AnimationCurve, float, bool)"/>
        public static UniTask Float(object owner, string tweenIdentifierTag, Action<float> setter, float startValue, float endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Float(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeCurve, onCompleteDelay, ignoreTimeScale);

        /// <summary>
        /// Starts a tween for a Vector2 value using a string tag and an AnimationCurve.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, AnimationCurve, float, bool)"/>
        public static UniTask Vector2(object owner, string tweenIdentifierTag, Action<Vector2> setter, Vector2 startValue, Vector2 endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Vector2(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeCurve, onCompleteDelay, ignoreTimeScale);

        /// <summary>
        /// Starts a tween for a Vector3 value using a string tag and an AnimationCurve.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, AnimationCurve, float, bool)"/>
        public static UniTask Vector3(object owner, string tweenIdentifierTag, Action<Vector3> setter, Vector3 startValue, Vector3 endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Vector3(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeCurve, onCompleteDelay, ignoreTimeScale);

        /// <summary>
        /// Starts a tween for a Quaternion value using a string tag and an AnimationCurve.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, AnimationCurve, float, bool)"/>
        public static UniTask Quaternion(object owner, string tweenIdentifierTag, Action<Quaternion> setter, Quaternion startValue, Quaternion endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Quaternion(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeCurve, onCompleteDelay, ignoreTimeScale);

        /// <summary>
        /// Starts a tween for a Color value using a string tag and an AnimationCurve.
        /// </summary>
        /// <inheritdoc cref="Float(object, uint, Action{float}, float, float, float, Action, AnimationCurve, float, bool)"/>
        public static UniTask Color(object owner, string tweenIdentifierTag, Action<Color> setter, Color startValue, Color endValue, float duration, Action onComplete, AnimationCurve easeCurve, float onCompleteDelay = 0f, bool ignoreTimeScale = false)
            => Color(owner, tweenIdentifierTag.Hash(), setter, startValue, endValue, duration, onComplete, easeCurve, onCompleteDelay, ignoreTimeScale);
        #endregion

        private static UniTask StartTween<T>(object owner, uint tweenIdentifierHash, T startValue, T endValue, float duration, Action<T> setter, Func<T, T, float, T> interpolator, Action onComplete, Func<float, float> easeFunction, float onCompleteDelay, bool ignoreTimeScale)
        {
            if (Application.isPlaying)
            {
                if (BTweenManager.Instance == null)
                {
                    Debug.LogError("BTweenManager could not be created.");
                    return UniTask.CompletedTask;
                }

                BTweenManager.Instance.StopTween(owner, tweenIdentifierHash);

                if (duration <= 0)
                {
                    setter(endValue);
                    onComplete?.Invoke();
                    return UniTask.CompletedTask;
                }

                var cts = new CancellationTokenSource();
                IDisposable registration = null;

                CancellationToken ownerDestroyToken = default;
                if (owner is Component ownerAsComponent)
                {
                    ownerDestroyToken = ownerAsComponent.GetAsyncDestroyTrigger().CancellationToken;
                }
                else if (owner is GameObject ownerAsGameObject)
                {
                    ownerDestroyToken = ownerAsGameObject.GetAsyncDestroyTrigger().CancellationToken;
                }

                if (ownerDestroyToken.CanBeCanceled)
                {
                    registration = ownerDestroyToken.Register(cts.Cancel);
                }

                var tween = BTweenManager.Instance.GetTween<T>();

                tween.Owner = owner;
                tween.TweenIdentifierHash = tweenIdentifierHash;
                tween.StartValue = startValue;
                tween.EndValue = endValue;
                tween.Duration = duration;
                tween.Setter = setter;
                tween.Interpolator = interpolator;
                tween.OnComplete = onComplete;
                tween.EaseFunction = easeFunction ?? Ease.Linear;
                tween.OnCompleteDelay = onCompleteDelay;
                tween.IgnoreTimeScale = ignoreTimeScale;
                tween.ElapsedTime = 0f;
                tween.Cts = cts;
                tween.Registration = registration;
                tween.TaskCompletionSource = new UniTaskCompletionSource();

                BTweenManager.Instance.RegisterTween(tween);
                
                return tween.TaskCompletionSource.Task;
            }
    #if UNITY_EDITOR
            else
            {
                BTweenEditorManager.StopTween(owner, tweenIdentifierHash);

                if (duration <= 0)
                {
                    setter(endValue);
                    onComplete?.Invoke();
                    return UniTask.CompletedTask;
                }

                var cts = new CancellationTokenSource();
                var tween = BTweenEditorManager.GetTween<T>();

                tween.Owner = owner;
                tween.TweenIdentifierHash = tweenIdentifierHash;
                tween.StartValue = startValue;
                tween.EndValue = endValue;
                tween.Duration = duration;
                tween.Setter = setter;
                tween.Interpolator = interpolator;
                tween.OnComplete = onComplete;
                tween.EaseFunction = easeFunction ?? Ease.Linear;
                tween.OnCompleteDelay = onCompleteDelay;
                tween.IgnoreTimeScale = true; // Always ignore time scale in editor
                tween.ElapsedTime = 0f;
                tween.Cts = cts;
                tween.Registration = null;
                tween.TaskCompletionSource = new UniTaskCompletionSource();

                BTweenEditorManager.RegisterTween(tween);
                
                return tween.TaskCompletionSource.Task;
            }
    #else
            return UniTask.CompletedTask;
    #endif
        }

        public static void StopTween(object owner, uint tweenIdentifierHash)
        {
            if (Application.isPlaying)
            {
                if (BTweenManager.Instance != null)
                    BTweenManager.Instance.StopTween(owner, tweenIdentifierHash);
            }
    #if UNITY_EDITOR
            else
            {
                BTweenEditorManager.StopTween(owner, tweenIdentifierHash);
            }
    #endif
        }
        
        public static void StopTween(object owner, string tweenIdentifierTag)
        {
            StopTween(owner, tweenIdentifierTag.Hash());
        }

        public static void StopAllTweensForOwner(object owner)
        {
            if (Application.isPlaying)
            {
                if (BTweenManager.Instance != null)
                    BTweenManager.Instance.StopAllTweensForOwner(owner);
            }
    #if UNITY_EDITOR
            else
            {
                BTweenEditorManager.StopAllTweensForOwner(owner);
            }
    #endif
        }

        public static void StopAndClearAllManagedTweens()
        {
            if (Application.isPlaying)
            {
                if (BTweenManager.Instance != null)
                    BTweenManager.Instance.StopAllTweens();
            }
    #if UNITY_EDITOR
            else
            {
                BTweenEditorManager.StopAllTweens();
            }
    #endif
        }

        public static bool IsTweening(object owner, string tweenIdentifierTag)
        {
            return IsTweening(owner, tweenIdentifierTag.Hash());
        }

        public static bool IsTweening(object owner, uint tweenIdentifierHash)
        {
            if (Application.isPlaying)
            {
                if (BTweenManager.Instance == null) return false;
                return BTweenManager.Instance.IsTweening(owner, tweenIdentifierHash);
            }
    #if UNITY_EDITOR
            else
            {
                return BTweenEditorManager.IsTweening(owner, tweenIdentifierHash);
            }
    #else
            return false;
    #endif
        }

        public static Func<float, float> GetEaseFunc(BTweenums ease)
        {
            switch (ease)
            {
                case BTweenums.Linear: return Ease.Linear;
                case BTweenums.InSine: return Ease.InSine;
                case BTweenums.OutSine: return Ease.OutSine;
                case BTweenums.InOutSine: return Ease.InOutSine;
                case BTweenums.InQuad: return Ease.InQuad;
                case BTweenums.OutQuad: return Ease.OutQuad;
                case BTweenums.InOutQuad: return Ease.InOutQuad;
                case BTweenums.InCubic: return Ease.InCubic;
                case BTweenums.OutCubic: return Ease.OutCubic;
                case BTweenums.InOutCubic: return Ease.InOutCubic;
                case BTweenums.InQuart: return Ease.InQuart;
                case BTweenums.OutQuart: return Ease.OutQuart;
                case BTweenums.InOutQuart: return Ease.InOutQuart;
                case BTweenums.InQuint: return Ease.InQuint;
                case BTweenums.OutQuint: return Ease.OutQuint;
                case BTweenums.InOutQuint: return Ease.InOutQuint;
                case BTweenums.InExpo: return Ease.InExpo;
                case BTweenums.OutExpo: return Ease.OutExpo;
                case BTweenums.InOutExpo: return Ease.InOutExpo;
                case BTweenums.InCirc: return Ease.InCirc;
                case BTweenums.OutCirc: return Ease.OutCirc;
                case BTweenums.InOutCirc: return Ease.InOutCirc;
                case BTweenums.InElastic: return Ease.InElastic;
                case BTweenums.OutElastic: return Ease.OutElastic;
                case BTweenums.InOutElastic: return Ease.InOutElastic;
                case BTweenums.InBack: return Ease.InBack;
                case BTweenums.OutBack: return Ease.OutBack;
                case BTweenums.InOutBack: return Ease.InOutBack;
                case BTweenums.InBounce: return Ease.InBounce;
                case BTweenums.OutBounce: return Ease.OutBounce;
                case BTweenums.InOutBounce: return Ease.InOutBounce;
                default:
                    Debug.LogWarning($"Unsupported ease type {ease}, defaulting to Linear.");
                    return Ease.Linear;
            }
        }

        public static class Ease
        {
            public static float Linear(float p) => p;
            public static float InQuad(float p) => p * p;
            public static float OutQuad(float p) => p * (2 - p);
            public static float InOutQuad(float p) => p < 0.5f ? 2 * p * p : -1 + (4 - 2 * p) * p;
            public static float InCubic(float p) => p * p * p;
            public static float OutCubic(float p) => (--p) * p * p + 1;
            public static float InOutCubic(float p) => p < 0.5f ? 4 * p * p * p : (p - 1) * (2 * p - 2) * (2 * p - 2) + 1;
            public static float InSine(float t) => 1 - Mathf.Cos(t * Mathf.PI / 2);
            public static float OutSine(float t) => Mathf.Sin(t * Mathf.PI / 2);
            public static float InOutSine(float t) => (Mathf.Cos(t * Mathf.PI) - 1) / -2;
            public static float InQuint(float t) => t * t * t * t * t;
            public static float OutQuint(float t) => 1 - InQuint(1 - t);
            public static float InOutQuint(float t)
            {
                if (t < 0.5) return InQuint(t * 2) / 2;
                return 1 - InQuint((1 - t) * 2) / 2;
            }
            public static float InQuart(float t) => t * t * t * t;
            public static float OutQuart(float t) => 1 - InQuart(1 - t);
            public static float InOutQuart(float t)
            {
                if (t < 0.5) return InQuart(t * 2) / 2;
                return 1 - InQuart((1 - t) * 2) / 2;
            }
            public static float InExpo(float t) => Mathf.Pow(2, 10 * (t - 1));
            public static float OutExpo(float t) => 1 - InExpo(1 - t);
            public static float InOutExpo(float t)
            {
                if (t < 0.5) return InExpo(t * 2) / 2;
                return 1 - InExpo((1 - t) * 2) / 2;
            }
            public static float InCirc(float t) => -(Mathf.Sqrt(1 - t * t) - 1);
            public static float OutCirc(float t) => 1 - InCirc(1 - t);
            public static float InOutCirc(float t)
            {
                if (t < 0.5) return InCirc(t * 2) / 2;
                return 1 - InCirc((1 - t) * 2) / 2;
            }
            public static float InElastic(float t) => 1 - OutElastic(1 - t);
            public static float OutElastic(float t)
            {
                float p = 0.3f;
                return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
            }
            public static float InOutElastic(float t)
            {
                if (t < 0.5) return InElastic(t * 2) / 2;
                return 1 - InElastic((1 - t) * 2) / 2;
            }
            public static float InBack(float t)
            {
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                return c3 * t * t * t - c1 * t * t;
            }

            public static float OutBack(float t)
            {
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
            }

            public static float InOutBack(float t)
            {
                const float c1 = 1.70158f;
                const float c2 = c1 * 1.525f;
                return t < 0.5f
                  ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
                  : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2;
            }

            public static float OutBounce(float t)
            {
                const float n1 = 7.5625f;
                const float d1 = 2.75f;

                if (t < 1 / d1)
                {
                    return n1 * t * t;
                }
                else if (t < 2 / d1)
                {
                    return n1 * (t -= 1.5f / d1) * t + 0.75f;
                }
                else if (t < 2.5 / d1)
                {
                    return n1 * (t -= 2.25f / d1) * t + 0.9375f;
                }
                else
                {
                    return n1 * (t -= 2.625f / d1) * t + 0.984375f;
                }
            }

            public static float InBounce(float t)
            {
                return 1 - OutBounce(1 - t);
            }

            public static float InOutBounce(float t)
            {
                return t < 0.5f
                  ? (1 - OutBounce(1 - 2 * t)) / 2
                  : (1 + OutBounce(2 * t - 1)) / 2;
            }
        }
    }
}