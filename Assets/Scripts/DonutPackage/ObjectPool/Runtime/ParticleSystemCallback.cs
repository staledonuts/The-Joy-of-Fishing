using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DonutPackage.ObjectPooling
{
    /// <summary>
    /// A PoolableObject that plays a particle effect and automatically returns to the pool when finished.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSystemCallback : PoolableObject
    {
        private ParticleSystem _ps;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
        }

        /// <summary>
        /// This is now the entry point for the effect. It's called by SpawnPooledObject.
        /// </summary>
        public override async void Play()
        {
            _cts = new CancellationTokenSource();
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _ps.Play();

            try
            {
                float lifetime = _ps.main.duration + _ps.main.startLifetime.constantMax;
                await UniTask.Delay(TimeSpan.FromSeconds(lifetime), cancellationToken: _cts.Token);
                
                // If the task wasn't cancelled, return this object to the pool.
                ReturnToPool();
            }
            catch (OperationCanceledException)
            {
                // Expected if the object is returned to the pool early.
            }
        }

        /// <summary>
        /// This hook is called just before the object is disabled by the pool.
        /// It's the perfect place to cancel any running async operations.
        /// </summary>
        public override void OnReturnToPool()
        {
            _ps.Clear();
            _ps.Stop();
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}