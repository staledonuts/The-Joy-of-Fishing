using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using DonutPackage.BTween;
using Cysharp.Threading.Tasks;

namespace DonutPackage.BTween.Editor
{
    [InitializeOnLoad]
    public static class BTweenEditorManager
    {
        private static readonly List<TweenBase> _activeTweens = new List<TweenBase>();
        private static readonly object _listLock = new object();
        private static double _lastUpdateTime;

        static BTweenEditorManager()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            _lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        private static void Update()
        {
            if (Application.isPlaying || _activeTweens.Count == 0) return;

            float deltaTime = (float)(EditorApplication.timeSinceStartup - _lastUpdateTime);
            _lastUpdateTime = EditorApplication.timeSinceStartup;

            bool needsRepaint = false;

            lock (_listLock)
            {
                for (int i = _activeTweens.Count - 1; i >= 0; i--)
                {
                    var tween = _activeTweens[i];

                    if (tween.Owner is UnityEngine.Object unityObject && unityObject == null)
                    {
                        tween.Cts.Cancel();
                    }

                    if (tween.Cts.IsCancellationRequested)
                    {
                        _activeTweens.RemoveAt(i);
                        tween.TaskCompletionSource?.TrySetCanceled();
                        continue;
                    }

                    float previousElapsedTime = tween.ElapsedTime;
                    tween.ElapsedTime += deltaTime;

                    // If tween is running
                    if (previousElapsedTime < tween.Duration)
                    {
                        if (tween.ElapsedTime >= tween.Duration)
                        {
                            // Just finished
                            tween.ApplyFinalValue();
                            needsRepaint = true;
                        }
                        else
                        {
                            // Still running
                            float progress = Mathf.Clamp01(tween.ElapsedTime / tween.Duration);
                            tween.Update(progress);
                            needsRepaint = true;
                        }
                    }

                    // Check for completion (after delay)
                    if (tween.ElapsedTime >= tween.Duration + tween.OnCompleteDelay)
                    {
                        _activeTweens.RemoveAt(i);
                        tween.OnComplete?.Invoke();
                        tween.TaskCompletionSource?.TrySetResult();
                        needsRepaint = true;
                    }
                }
            }

            if (needsRepaint)
            {
                SceneView.RepaintAll();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        public static void RegisterTween(TweenBase tween)
        {
            lock (_listLock)
            {
                _activeTweens.Add(tween);
            }
        }

        public static void StopTween(object owner, uint tweenIdentifierHash)
        {
            lock (_listLock)
            {
                var tweenToStop = _activeTweens.FirstOrDefault(t => t.Owner.Equals(owner) && t.TweenIdentifierHash == tweenIdentifierHash);
                if (tweenToStop != null)
                {
                    tweenToStop.Cts.Cancel();
                }
            }
        }

        public static void StopAllTweens()
        {
            lock (_listLock)
            {
                foreach (var tween in _activeTweens)
                {
                    tween.Cts.Cancel();
                }
                _activeTweens.Clear();
            }
        }
        
        public static void StopAllTweensForOwner(object owner)
        {
            lock (_listLock)
            {
                var tweensToStop = _activeTweens.Where(t => t.Owner.Equals(owner)).ToList();
                foreach (var tween in tweensToStop)
                {
                    tween.Cts.Cancel();
                }
            }
        }

        public static bool IsTweening(object owner, uint tweenIdentifierHash)
        {
            lock (_listLock)
            {
                return _activeTweens.Any(t => t.Owner.Equals(owner) && t.TweenIdentifierHash == tweenIdentifierHash && !t.Cts.IsCancellationRequested);
            }
        }

        public static Tween<T> GetTween<T>()
        {
            return new Tween<T>();
        }
    }
}
