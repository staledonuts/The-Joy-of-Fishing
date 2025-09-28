using UnityEngine;

namespace DonutPackage.ObjectPooling
{
    /// <summary>
    /// An abstract base class for any component attached to a GameObject that can be managed by the ObjectPool.
    /// Provides a standard interface for activation and returning to the pool.
    /// </summary>
    public abstract class PoolableObject : MonoBehaviour
    {
        /// <summary>
        /// The unique hash identifier that links this object back to its pool.
        /// This is set by the ObjectPool during instantiation.
        /// </summary>
        public PoolObjectHash PoolHash { get; set; }

        /// <summary>
        /// The primary method to activate the object's behavior after it's retrieved from the pool.
        /// Subclasses must implement this to define what the object does when it's "played".
        /// </summary>
        public abstract void Play();

        /// <summary>
        /// A lifecycle hook called by the ObjectPool immediately after the object is set active
        /// but before Play() is called. Good for resetting state.
        /// </summary>
        public virtual void OnGetFromPool() { }
        
        /// <summary>
        /// A lifecycle hook called by the ObjectPool just before the object is set inactive.
        /// Good for cleanup logic.
        /// </summary>
        public virtual void OnReturnToPool() { }

        /// <summary>
        /// A convenience method for subclasses to easily return themselves to the pool.
        /// </summary>
        public void ReturnToPool()
        {
            if (ObjectPool.Instance != null)
            {
                ObjectPool.Instance.ReturnToPool(this.gameObject);
            }
        }
    }
}