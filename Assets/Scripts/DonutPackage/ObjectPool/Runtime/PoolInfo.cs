using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace DonutPackage.ObjectPooling
{
    /// <summary>
    /// A runtime data class that holds the state for a single pool, including its configuration
    /// and the live list of GameObjects.
    /// </summary>
    [Serializable]
    public class PoolInfo
    {
        /// <summary>
        /// A reference to the ScriptableObject containing the pool's configuration.
        /// </summary>
        [SerializeField] public PoolObject Object;

        /// <summary>
        /// The runtime list of all GameObjects (active and inactive) belonging to this pool.
        /// </summary>
        [HideInInspector] public List<ObjectPool.PooledObject> m_pooledObjects = new List<ObjectPool.PooledObject>();

        /// <summary>
        /// The parent transform under which inactive objects are stored in the hierarchy.
        /// </summary>
        [HideInInspector] public Transform parentTransform;

        /// <summary>
        /// A queue of asynchronous requests that are waiting for an object to become available from this pool.
        /// </summary>
        [HideInInspector] public Queue<UniTaskCompletionSource<GameObject>> Waiters = new Queue<UniTaskCompletionSource<GameObject>>();
    }
}