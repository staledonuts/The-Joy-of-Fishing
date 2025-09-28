using UnityEngine;
using DonutPackage.Utils;

namespace DonutPackage.ObjectPooling
{
    /// <summary>
    /// A ScriptableObject that acts as a data container for an object pool's configuration.
    /// This includes the prefab to be pooled, the initial size, and whether it can grow.
    /// </summary>
    [CreateAssetMenu(fileName = "PoolObject", menuName = "DonutPackage/ObjectPool/PoolObject", order = 100)]
    public class PoolObject : ScriptableObject
    {
        /// <summary>
        /// The GameObject prefab that this pool will manage instances of.
        /// </summary>
        public GameObject prefab = null;

        /// <summary>
        /// The initial number of instances to create when the pool is initialized.
        /// </summary>
        public int instanceCount = 10;

        /// <summary>
        /// If true, the pool can create new instances if an object is requested when none are available.
        /// </summary>
        public bool allowIncrease = false;

        /// <summary>
        /// A computed hash based on the ScriptableObject's asset name, used for efficient lookups.
        /// </summary>
        public PoolObjectHash Hash => (PoolObjectHash)name.Hash();
    }
}