namespace DonutPackage.Timer
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Cysharp.Threading.Tasks.Triggers;
    using UnityEngine;
    using DonutPackage.Utils;

    public static class Timer
    {
        private static readonly Dictionary<TimerKey, TimerContext> _activeTimers = new();

        #region Public API (uint hash)
        public static TimerHandle Start(float duration, Action onComplete, object owner, uint tagHash, bool ignoreTimeScale = false)
        {
            var key = new TimerKey(owner, tagHash);
            Stop(key);

            if (duration <= 0f)
            {
                onComplete?.Invoke();
                return new TimerHandle(null);
            }

            var cts = new CancellationTokenSource();
            IDisposable registration = null;

            if (owner is Component ownerAsComponent)
            {
                registration = ownerAsComponent.GetAsyncDestroyTrigger().CancellationToken.Register(cts.Cancel);
            }
            else if (owner is GameObject ownerAsGameObject)
            {
                registration = ownerAsGameObject.GetAsyncDestroyTrigger().CancellationToken.Register(cts.Cancel);
            }

            var context = new TimerContext(cts, registration);
            _activeTimers[key] = context;

            _ = TimerCoroutine(key, duration, onComplete, ignoreTimeScale, cts);
            
            return new TimerHandle(cts);
        }
        #endregion

        #region Public API (string tag)
        public static TimerHandle Start(float duration, Action onComplete, object owner = null, string tag = null, bool ignoreTimeScale = false)
        {
            return Start(duration, onComplete, owner, tag.Hash(), ignoreTimeScale);
        }
        #endregion

        #region Public API (TimerTags enum)
        /// <summary>
        /// Starts a new timer using a TimerTags enum member.
        /// </summary>
        /// <inheritdoc cref="Start(float, Action, object, uint, bool)"/>
        public static TimerHandle Start(float duration, Action onComplete, object owner, TimerTags tag, bool ignoreTimeScale = false)
        {
            return Start(duration, onComplete, owner, (uint)tag, ignoreTimeScale);
        }

        /// <summary>
        /// Stops a specific timer identified by its owner and TimerTags enum member.
        /// </summary>
        public static void Stop(object owner, TimerTags tag)
        {
            Stop(owner, (uint)tag);
        }
        #endregion

        #region Stop Methods
        public static void Stop(object owner, uint tagHash)
        {
            var key = new TimerKey(owner, tagHash);
            Stop(key);
        }

        public static void Stop(object owner, string tag = null)
        {
            Stop(owner, tag.Hash());
        }
        
        public static void Stop(TimerHandle handle)
        {
            handle.Cancel();
        }
        #endregion

        private static async UniTask TimerCoroutine(TimerKey key, float duration, Action onComplete, bool ignoreTimeScale, CancellationTokenSource cts)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale, PlayerLoopTiming.Update, cts.Token);
                onComplete?.Invoke();
            }
            catch (OperationCanceledException){}
            finally
            {
                if (_activeTimers.TryGetValue(key, out var context) && context.Cts == cts)
                {
                    _activeTimers.Remove(key);
                    context.Dispose();
                }
            }
        }

        private static void Stop(TimerKey key)
        {
            if (_activeTimers.TryGetValue(key, out var context))
            {
                context.Cts.Cancel();
            }
        }

        private readonly struct TimerKey : IEquatable<TimerKey>
        {
            public readonly object Owner;
            public readonly uint TagHash;

            public TimerKey(object owner, uint tagHash)
            {
                Owner = owner ?? typeof(Timer);
                TagHash = tagHash;
            }

            public bool Equals(TimerKey other) => Owner == other.Owner && TagHash == other.TagHash;
            public override bool Equals(object obj) => obj is TimerKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(Owner, TagHash);
        }

        private sealed class TimerContext : IDisposable
        {
            public readonly CancellationTokenSource Cts;
            private readonly IDisposable _registration;

            public TimerContext(CancellationTokenSource cts, IDisposable registration)
            {
                Cts = cts;
                _registration = registration;
            }

            public void Dispose()
            {
                _registration?.Dispose();
                Cts.Dispose();
            }
        }

        public readonly struct TimerHandle
        {
            private readonly CancellationTokenSource _cts;

            public TimerHandle(CancellationTokenSource cts)
            {
                _cts = cts;
            }

            public void Cancel()
            {
                _cts?.Cancel();
            }

            public bool IsValid => _cts != null && !_cts.IsCancellationRequested;
        }
    }

#if !STRING_HASH_ENUMS_GENERATED
    public enum TimerTags : uint {}
#endif
}