using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Profiling;
using DonutPackage.ObjectPooling;

namespace DonutPackage.ObjectPooling.Editor
{
    /// <summary>
    /// Custom editor for the ObjectPool class to provide detailed runtime debugging information.
    /// </summary>
    [CustomEditor(typeof(ObjectPool))]
    public class ObjectPoolEditor : UnityEditor.Editor
    {
        // Stores the open/closed state of each pool's foldout in the inspector.
        private readonly Dictionary<PoolObjectHash, bool> _foldoutStates = new Dictionary<PoolObjectHash, bool>();

        public override void OnInspectorGUI()
        {
            // Draw the default inspector fields (like the Pool Collection reference).
            base.OnInspectorGUI();

            // The debug view is only available when the application is running.
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Debug info is only available in Play Mode.", MessageType.Info);
                return;
            }

            ObjectPool poolManager = (ObjectPool)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Live Pool Debug", EditorStyles.boldLabel);

            if (poolManager.poolsDict == null || poolManager.poolsDict.Count == 0)
            {
                EditorGUILayout.LabelField("No pools have been created yet.");
                return;
            }

            // --- Summary Stats ---
            long totalMemory = GetDetailedMemoryUsage(poolManager.poolsDict.Values.SelectMany(p => p.m_pooledObjects).Select(po => po.GameObject));
            int totalObjects = poolManager.poolsDict.Values.Sum(p => p.m_pooledObjects.Count);
            int activeObjects = poolManager.poolsDict.Values.Sum(p => p.m_pooledObjects.Count(o => o.GameObject != null && o.GameObject.activeSelf));
            int totalWaiters = poolManager.poolsDict.Values.Sum(p => p.Waiters.Count);

            EditorGUILayout.LabelField("Total Pools:", poolManager.poolsDict.Count.ToString());
            EditorGUILayout.LabelField("Total Objects (Active/Total):", $"{activeObjects} / {totalObjects}");
            EditorGUILayout.LabelField("Total Memory:", FormatBytes(totalMemory));
            EditorGUILayout.LabelField("Total Async Waiters:", totalWaiters.ToString());

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Pools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Memory usage includes the GameObject, its components, and referenced assets like meshes, materials, and textures. Shared assets are counted only once in the 'Total Memory' and per-pool summaries.", MessageType.Info);


            // --- Individual Pool Details ---
            // Create a copy of the keys to prevent modification errors during iteration.
            var keys = new List<PoolObjectHash>(poolManager.poolsDict.Keys);
            foreach (var hash in keys)
            {
                if (!poolManager.poolsDict.TryGetValue(hash, out PoolInfo info)) continue;

                if (!_foldoutStates.ContainsKey(hash))
                {
                    _foldoutStates[hash] = false;
                }

                int activeCount = info.m_pooledObjects.Count(o => o.GameObject != null && o.GameObject.activeSelf);
                string poolName = info.Object != null ? info.Object.name : "Unnamed Pool";
                long poolMemory = GetDetailedMemoryUsage(info.m_pooledObjects.Select(po => po.GameObject));
                
                string foldoutLabel = $"{poolName} - [{activeCount}/{info.m_pooledObjects.Count}] - {FormatBytes(poolMemory)}";
                
                // A foldout for each pool to keep the inspector clean.
                _foldoutStates[hash] = EditorGUILayout.Foldout(_foldoutStates[hash], foldoutLabel, true, EditorStyles.foldout);

                if (_foldoutStates[hash])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Hash:", ((int)hash).ToString());
                    EditorGUILayout.ObjectField("Source Asset:", info.Object, typeof(PoolObject), false);
                    EditorGUILayout.LabelField("Can Increase:", info.Object.allowIncrease.ToString());
                    EditorGUILayout.LabelField("Async Waiters:", info.Waiters.Count.ToString());
                    EditorGUILayout.LabelField("Memory Usage:", FormatBytes(poolMemory));
                    
                    // Display a list of the actual GameObjects in the pool.
                    if (info.m_pooledObjects.Count > 0)
                    {
                        EditorGUILayout.LabelField("Instances:", EditorStyles.boldLabel);
                        EditorGUI.indentLevel++;
                        foreach(var pooledObj in info.m_pooledObjects)
                        {
                            if (pooledObj.GameObject != null)
                            {
                                long objectMemory = GetDetailedMemoryUsage(new[] { pooledObj.GameObject });
                                // Allows you to click and highlight the object in the hierarchy.
                                EditorGUILayout.ObjectField($"{pooledObj.GameObject.name} ({FormatBytes(objectMemory)})", pooledObj.GameObject, typeof(GameObject), true);
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            // Force the inspector to repaint continuously to show live updates.
            Repaint();
        }

        /// <summary>
        /// Calculates the detailed memory usage for a collection of GameObjects, including their components and referenced assets like Meshes, Materials, and Textures.
        /// Shared assets are only counted once to provide an accurate total.
        /// </summary>
        /// <param name="gameObjects">The collection of GameObjects to analyze.</param>
        /// <returns>The total memory usage in bytes.</returns>
        private long GetDetailedMemoryUsage(IEnumerable<GameObject> gameObjects)
        {
            if (gameObjects == null || !gameObjects.Any())
            {
                return 0;
            }

            var trackedAssets = new HashSet<UnityEngine.Object>();
            long totalMemory = 0;

            foreach (var go in gameObjects.Where(g => g != null))
            {
                // The GameObject itself
                if (trackedAssets.Add(go))
                {
                    totalMemory += Profiler.GetRuntimeMemorySizeLong(go);
                }

                // Components and their assets
                foreach (var component in go.GetComponents<Component>())
                {
                    if (component == null) continue;

                    if (trackedAssets.Add(component))
                    {
                        totalMemory += Profiler.GetRuntimeMemorySizeLong(component);
                    }

                    // MeshFilter
                    if (component is MeshFilter meshFilter && meshFilter.sharedMesh != null)
                    {
                        if (trackedAssets.Add(meshFilter.sharedMesh))
                        {
                            totalMemory += Profiler.GetRuntimeMemorySizeLong(meshFilter.sharedMesh);
                        }
                    }
                    // Renderer
                    else if (component is Renderer renderer)
                    {
                        foreach (var material in renderer.sharedMaterials)
                        {
                            if (material == null) continue;
                            if (trackedAssets.Add(material))
                            {
                                totalMemory += Profiler.GetRuntimeMemorySizeLong(material);
                                
                                // Textures in material
                                string[] texturePropertyNames = material.GetTexturePropertyNames();
                                foreach(var propName in texturePropertyNames)
                                {
                                    var texture = material.GetTexture(propName);
                                    if (texture != null && trackedAssets.Add(texture))
                                    {
                                        totalMemory += Profiler.GetRuntimeMemorySizeLong(texture);
                                    }
                                }
                            }
                        }
                    }
                    // Particle System
                    else if (component is ParticleSystem ps)
                    {
                        var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
                        if (psRenderer != null)
                        {
                            // Material
                            if (psRenderer.sharedMaterial != null && trackedAssets.Add(psRenderer.sharedMaterial))
                            {
                                totalMemory += Profiler.GetRuntimeMemorySizeLong(psRenderer.sharedMaterial);
                                // And its textures
                                string[] texturePropertyNames = psRenderer.sharedMaterial.GetTexturePropertyNames();
                                foreach(var propName in texturePropertyNames)
                                {
                                    var texture = psRenderer.sharedMaterial.GetTexture(propName);
                                    if (texture != null && trackedAssets.Add(texture))
                                    {
                                        totalMemory += Profiler.GetRuntimeMemorySizeLong(texture);
                                    }
                                }
                            }
                            // Mesh
                            if (psRenderer.mesh != null && trackedAssets.Add(psRenderer.mesh))
                            {
                                totalMemory += Profiler.GetRuntimeMemorySizeLong(psRenderer.mesh);
                            }
                        }
                    }
                }
            }

            return totalMemory;
        }
        
        private string FormatBytes(long bytes)
        {
            if (bytes > 1024 * 1024) return $"{(bytes / (1024f * 1024f)):F2} MB";
            if (bytes > 1024) return $"{(bytes / 1024f):F2} KB";
            return $"{bytes} Bytes";
        }
    }
}