using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace DonutPackage.ObjectPooling
{
    /// <summary>
    /// A singleton manager for object pools. Handles the instantiation, retrieval, and reuse of GameObjects
    /// to optimize performance by avoiding frequent creation and destruction.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        /// <summary>
        /// The static singleton instance of the ObjectPool, accessible from anywhere.
        /// </summary>
        public static ObjectPool Instance;

        /// <summary>
        /// A wrapper class holding a reference to a pooled GameObject.
        /// </summary>
        public class PooledObject { public GameObject GameObject; }

        /// <summary>
        /// The Scriptable object with a list of pool configurations, assigned in the Unity Inspector.
        /// </summary>
        public ObjectPoolCollection poolCollection;

        /// <summary>
        /// The internal dictionary used for fast, hash-based lookups of pools at runtime.
        /// </summary>
        public Dictionary<PoolObjectHash, PoolInfo> poolsDict = new Dictionary<PoolObjectHash, PoolInfo>();

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(Instance); }
            else { Destroy(gameObject); }
        }

        private void Start()
        {
            foreach (var pool in poolCollection.pools)
            {
                CreatePool(pool);
            }
        }

        /// <summary>
        /// Instantiates the initial batch of objects for a given pool configuration.
        /// </summary>
        private void CreatePool(PoolInfo info)
        {
            if (info.m_pooledObjects.Count > 0) return;
            if (info.Object.prefab.GetComponent<PoolableObject>() == null)
            {
                Debug.LogError($"[ObjectPool] Prefab '{info.Object.prefab.name}' is missing a PoolableObject component!");
                return;
            }

            info.parentTransform = transform;
            PoolObjectHash hash = info.Object.Hash;

            for (int i = 0; i < info.Object.instanceCount; ++i)
            {
                InstantiateAndPool(info, hash);
            }
            poolsDict[hash] = info;
        }

        /// <summary>
        /// Helper method to create a single object instance and add it to a pool.
        /// </summary>
        private GameObject InstantiateAndPool(PoolInfo info, PoolObjectHash hash)
        {
            GameObject obj = Instantiate(info.Object.prefab, info.parentTransform);
            
            var poolable = obj.GetComponent<PoolableObject>();
            poolable.PoolHash = hash;
            
            info.m_pooledObjects.Add(new PooledObject { GameObject = obj });
            obj.SetActive(false);
            return obj;
        }

        #region Synchronous API

        /// <summary>
        /// Synchronously gets an inactive pooled object.
        /// </summary>
        /// <returns>An available GameObject, or null if none are available and the pool cannot increase.</returns>
        public GameObject GetPooledObject(PoolObjectHash objectHash)
        {
            if (!poolsDict.TryGetValue(objectHash, out PoolInfo info)) return null;

            foreach (var obj in info.m_pooledObjects)
            {
                if (!obj.GameObject.activeSelf)
                {
                    return obj.GameObject;
                }
            }
            return info.Object.allowIncrease ? InstantiateAndPool(info, objectHash) : null;
        }

        /// <summary>
        /// Synchronously spawns a pooled object with a default rotation.
        /// </summary>
        public T SpawnPooledObject<T>(PoolObjectHash objectHash, Vector3 position) where T : PoolableObject
        {
            return SpawnPooledObject<T>(objectHash, position, Quaternion.identity);
        }

        /// <summary>
        /// Synchronously spawns a pooled object and returns its PoolableObject component, ready to be used.
        /// </summary>
        /// <typeparam name="T">The type of PoolableObject to spawn.</typeparam>
        /// <returns>The component of type T on the spawned object, or null if it could not be spawned.</returns>
        public T SpawnPooledObject<T>(PoolObjectHash objectHash, Vector3 position, Quaternion rotation) where T : PoolableObject
        {
            GameObject obj = GetPooledObject(objectHash);
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                
                T poolableComponent = obj.GetComponent<T>();
                obj.SetActive(true);
                poolableComponent.OnGetFromPool();
                poolableComponent.Play();
                return poolableComponent;
            }
            return null;
        }

        /// <summary>
        /// Spawns a pooled object and attaches it to a parent transform.
        /// </summary>
        /// <param name="parent">The transform to parent the spawned object to.</param>
        /// <returns>The PoolableObject component of the spawned object.</returns>
        public T SpawnAttached<T>(PoolObjectHash objectHash, Transform parent) where T : PoolableObject
        {
            GameObject obj = GetPooledObject(objectHash);
            if (obj != null)
            {
                T poolableComponent = obj.GetComponent<T>();
                if (poolableComponent == null)
                {
                    Debug.LogError($"[ObjectPool] Failed to spawn: Prefab '{obj.name}' does not have the requested component of type '{typeof(T).Name}'.");
                    ReturnToPool(obj);
                    return null;
                }

                obj.transform.SetParent(parent);
                obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                
                obj.SetActive(true);
                poolableComponent.OnGetFromPool();
                poolableComponent.Play();
                return poolableComponent;
            }
            return null;
        }

        #endregion

        #region Asynchronous API

        /// <summary>
        /// Asynchronously gets a pooled object. If no object is available and the pool can't grow, it waits until one is returned.
        /// </summary>
        /// <returns>A UniTask that completes with the requested GameObject, or null if the pool doesn't exist.</returns>
        public async UniTask<GameObject> GetPooledObjectAsync(PoolObjectHash objectHash, CancellationToken cancellationToken = default)
        {
            if (!poolsDict.TryGetValue(objectHash, out PoolInfo info)) return null;

            foreach (var obj in info.m_pooledObjects)
            {
                if (!obj.GameObject.activeSelf)
                {
                    return obj.GameObject;
                }
            }
        
            if (info.Object.allowIncrease)
            {
                return InstantiateAndPool(info, objectHash);
            }

            var waiter = new UniTaskCompletionSource<GameObject>();
            cancellationToken.Register(() => 
            {
                waiter.TrySetCanceled(cancellationToken);
                // Attempt to remove the waiter if it's still in the queue
                // This part can be tricky without modifying the Queue, so we accept a potential empty Dequeue.
            });
        
            info.Waiters.Enqueue(waiter);
            return await waiter.Task;
        }

        /// <summary>
        /// Asynchronously spawns a pooled object and returns its PoolableObject component.
        /// </summary>
        /// <typeparam name="T">The type of PoolableObject to spawn.</typeparam>
        /// <returns>A UniTask that completes with the component of type T, or null if it could not be spawned.</returns>
        public async UniTask<T> SpawnPooledObjectAsync<T>(PoolObjectHash objectHash, Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default) where T : PoolableObject
        {
            GameObject obj = await GetPooledObjectAsync(objectHash, cancellationToken);
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                
                T poolableComponent = obj.GetComponent<T>();
                obj.SetActive(true);
                poolableComponent.OnGetFromPool();
                poolableComponent.Play();
                return poolableComponent;
            }
            return null;
        }

        /// <summary>
        /// Asynchronously spawns a pooled object with a default rotation.
        /// </summary>
        public async UniTask<T> SpawnPooledObjectAsync<T>(PoolObjectHash objectHash, Vector3 position, CancellationToken cancellationToken = default) where T : PoolableObject
        {
            return await SpawnPooledObjectAsync<T>(objectHash, position, Quaternion.identity, cancellationToken);
        }

        /// <summary>
        /// Asynchronously spawns a pooled object and attaches it to a parent transform.
        /// </summary>
        /// <param name="parent">The transform to parent the spawned object to.</param>
        /// <returns>A UniTask that completes with the PoolableObject component of the spawned object.</returns>
        public async UniTask<T> SpawnAttachedAsync<T>(PoolObjectHash objectHash, Transform parent, CancellationToken cancellationToken = default) where T : PoolableObject
        {
            GameObject obj = await GetPooledObjectAsync(objectHash, cancellationToken);
            if (obj != null)
            {
                T poolableComponent = obj.GetComponent<T>();
                if (poolableComponent == null)
                {
                    Debug.LogError($"[ObjectPool] Failed to spawn: Prefab '{obj.name}' does not have the requested component of type '{typeof(T).Name}'.");
                    ReturnToPool(obj);
                    return null;
                }

                obj.transform.SetParent(parent);
                obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                obj.SetActive(true);
                poolableComponent.OnGetFromPool();
                poolableComponent.Play();
                return poolableComponent;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Deactivates a GameObject and returns it to its pool for reuse. If an async request is waiting, the object is passed to it directly.
        /// </summary>
        public void ReturnToPool(GameObject go)
        {
            var poolable = go.GetComponent<PoolableObject>();
            if (poolable == null)
            {
                Debug.LogWarning($"Object '{go.name}' missing PoolableObject component. Destroying it.");
                Destroy(go);
                return;
            }

            if (poolsDict.TryGetValue(poolable.PoolHash, out PoolInfo info))
            {
                // Check for and fulfill any async requests waiting for an object
                if (info.Waiters.Count > 0)
                {
                    var waiter = info.Waiters.Dequeue();
                    if (!waiter.Task.Status.IsCompleted())
                    {
                        // Pass the object directly to the waiting task, bypassing the pool.
                        waiter.TrySetResult(go); 
                        return;
                    }
                }
                
                poolable.OnReturnToPool();
                go.transform.SetParent(info.parentTransform);
                go.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"No pool found for hash {poolable.PoolHash}. Destroying object.");
                Destroy(go);
            }
        }
    }
}

#if !POOL_HASH_GENERATED
/// <summary>
/// A fallback definition for PoolObjectHash, used when the enum hasn't been generated yet.
/// This prevents compilation errors if the generator has not been run.
/// </summary>
public enum PoolObjectHash : uint
{
    None = 0
}
#endif
