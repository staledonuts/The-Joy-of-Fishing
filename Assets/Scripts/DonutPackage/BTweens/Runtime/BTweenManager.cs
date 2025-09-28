using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace DonutPackage.BTween
{
    public abstract class TweenBase
    {
        public object Owner;
        public uint TweenIdentifierHash;
        public float Duration;
        public float ElapsedTime;
        public Func<float, float> EaseFunction;
        public Action OnComplete;
        public float OnCompleteDelay;
        public bool IgnoreTimeScale;
        public CancellationTokenSource Cts;
        public IDisposable Registration;
        public UniTaskCompletionSource TaskCompletionSource;

        public abstract void ApplyFinalValue();
        public abstract void Update(float progress);
        public abstract void Reset();
    }

    public class Tween<T> : TweenBase
    {
        public T StartValue;
        public T EndValue;
        public Action<T> Setter;
        public Func<T, T, float, T> Interpolator;

        public override void ApplyFinalValue()
        {
            if (Setter != null) Setter(EndValue);
        }

        public override void Update(float progress)
        {
            if (Setter != null) Setter(Interpolator(StartValue, EndValue, EaseFunction(progress)));
        }

        public override void Reset()
        {
            Owner = null;
            TweenIdentifierHash = 0;
            Duration = 0;
            ElapsedTime = 0;
            EaseFunction = null;
            OnComplete = null;
            OnCompleteDelay = 0;
            IgnoreTimeScale = false;
            
            Registration?.Dispose();
            Registration = null;
            
            Cts = null;
            TaskCompletionSource = null;

            StartValue = default;
            EndValue = default;
            Setter = null;
            Interpolator = null;
        }
    }

    public class BTweenManager : MonoBehaviour
    {
        private static BTweenManager _instance;
        public static BTweenManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<BTweenManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("BTweenManager");
                        _instance = go.AddComponent<BTweenManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private readonly List<TweenBase> _activeTweens = new List<TweenBase>();
        private readonly object _listLock = new object();
        private readonly Dictionary<Type, Stack<TweenBase>> _tweenPool = new Dictionary<Type, Stack<TweenBase>>();

        public Tween<T> GetTween<T>()
        {
            lock (_tweenPool)
            {
                if (_tweenPool.TryGetValue(typeof(T), out var stack) && stack.Count > 0)
                {
                    return (Tween<T>)stack.Pop();
                }
            }
            return new Tween<T>();
        }

        private void ReturnTweenToPool(TweenBase tween)
        {
            tween.Reset();
            Type tweenType = tween.GetType().GetGenericArguments()[0];
            lock (_tweenPool)
            {
                if (!_tweenPool.TryGetValue(tweenType, out var stack))
                {
                    stack = new Stack<TweenBase>();
                    _tweenPool[tweenType] = stack;
                }
                stack.Push(tween);
            }
        }

        public void RegisterTween(TweenBase tween)
        {
            lock (_listLock)
            {
                _activeTweens.Add(tween);
            }
        }

        public void StopTween(object owner, uint tweenIdentifierHash)
        {
            lock (_listLock)
            {
                var tweenToStop = _activeTweens.FirstOrDefault(t => t.Owner.Equals(owner) && t.TweenIdentifierHash == tweenIdentifierHash);
                tweenToStop?.Cts.Cancel();
            }
        }

        public bool IsTweening(object owner, uint tweenIdentifierHash)
        {
            lock (_listLock)
            {
                return _activeTweens.Any(t => t.Owner.Equals(owner) && t.TweenIdentifierHash == tweenIdentifierHash && !t.Cts.IsCancellationRequested);
            }
        }

        public void StopAllTweens()
        {
            lock (_listLock)
            {
                foreach (var tween in _activeTweens)
                {
                    tween.Cts.Cancel();
                }
            }
        }

        public void StopAllTweensForOwner(object owner)
        {
            lock (_listLock)
            {
                // We need to iterate backwards when removing items from a list.
                for (int i = _activeTweens.Count - 1; i >= 0; i--)
                {
                    if (_activeTweens[i].Owner.Equals(owner))
                    {
                        _activeTweens[i].Cts.Cancel();
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (_activeTweens.Count == 0) return;

            float deltaTime = Time.deltaTime;
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            lock (_listLock)
            {
                Parallel.ForEach(_activeTweens, tween =>
                {
                    if (tween.Cts.IsCancellationRequested) return;
                    tween.ElapsedTime += tween.IgnoreTimeScale ? unscaledDeltaTime : deltaTime;
                });

                for (int i = _activeTweens.Count - 1; i >= 0; i--)
                {
                    var tween = _activeTweens[i];

                    if (tween.Cts.IsCancellationRequested)
                    {
                        _activeTweens.RemoveAt(i);
                        tween.TaskCompletionSource?.TrySetCanceled();
                        ReturnTweenToPool(tween);
                        continue;
                    }

                    if (tween.ElapsedTime < tween.Duration)
                    {
                        float progress = Mathf.Clamp01(tween.ElapsedTime / tween.Duration);
                        tween.Update(progress);
                    }
                    else
                    {
                        _activeTweens.RemoveAt(i);
                        tween.ApplyFinalValue();
                        FinishTween(tween);
                    }
                }
            }
        }

        private async void FinishTween(TweenBase tween)
        {
            await HandleCompletion(tween);
            ReturnTweenToPool(tween);
        }

        private async UniTask HandleCompletion(TweenBase tween)
        {
            if (tween.OnComplete != null)
            {
                if (tween.OnCompleteDelay > 0f)
                {
                    try
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(tween.OnCompleteDelay), ignoreTimeScale: tween.IgnoreTimeScale, delayTiming: PlayerLoopTiming.Update, cancellationToken: tween.Cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        tween.TaskCompletionSource?.TrySetCanceled();
                        return;
                    }
                }
                tween.OnComplete.Invoke();
            }
            tween.TaskCompletionSource?.TrySetResult();
        }
    }
}