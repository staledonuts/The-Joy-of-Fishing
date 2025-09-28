using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using DonutPackage.BTween;
using System;

namespace DonutPackage.BTween.Editor
{
    [CustomEditor(typeof(BTweenManager))]
    public class BTweenManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            BTweenManager manager = (BTweenManager)target;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Debug info is only available in Play Mode.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Live BTween Debug", EditorStyles.boldLabel);

            var activeTweens = GetPrivateField<List<TweenBase>>(manager, "_activeTweens");
            var tweenPool = GetPrivateField<Dictionary<Type, Stack<TweenBase>>>(manager, "_tweenPool");

            if (activeTweens != null)
            {
                EditorGUILayout.LabelField("Active Tweens", activeTweens.Count.ToString());
            }

            if (tweenPool != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Object Pools", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;

                if (tweenPool.Count == 0)
                {
                    EditorGUILayout.LabelField("No pools created yet.");
                }
                else
                {
                    long totalMemory = 0;
                    foreach (var kvp in tweenPool)
                    {
                        int count = kvp.Value.Count;
                        long singleObjectSize = EstimateTweenObjectSize(kvp.Key);
                        long poolMemory = count * singleObjectSize;
                        totalMemory += poolMemory;
                        
                        string memoryString = EditorUtility.FormatBytes(poolMemory);
                        EditorGUILayout.LabelField($"Pool<{kvp.Key.Name}>", $"{count} available (~{memoryString})");
                    }
                    
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Total Estimated Pool Memory", $"~{EditorUtility.FormatBytes(totalMemory)}", EditorStyles.boldLabel);
                }
                EditorGUI.indentLevel--;
            }

            Repaint();
        }

        private T GetPrivateField<T>(object obj, string fieldName) where T : class
        {
            if (obj == null) return null;
            var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(obj) as T;
        }

        private int GetValueTypeSize(Type type)
        {
            if (type == typeof(float)) return sizeof(float);
            if (type == typeof(Vector2)) return sizeof(float) * 2;
            if (type == typeof(Vector3)) return sizeof(float) * 3;
            if (type == typeof(Color)) return sizeof(float) * 4;
            if (type == typeof(Quaternion)) return sizeof(float) * 4;
            if (type.IsValueType)
            {
                try { return System.Runtime.InteropServices.Marshal.SizeOf(type); }
                catch { return IntPtr.Size; } // Fallback for non-blittable value types
            }
            return IntPtr.Size; // Size for reference types
        }

        private int EstimateTweenObjectSize(Type tweenValueType)
        {
            int pointerSize = IntPtr.Size;
            int clrOverhead = 24; // A reasonable estimate for a 64-bit runtime with a sync block

            int fieldsSize = 0;
            // Tween<T> fields
            fieldsSize += 2 * GetValueTypeSize(tweenValueType); // StartValue, EndValue
            fieldsSize += 2 * pointerSize; // Setter, Interpolator

            // TweenBase fields
            fieldsSize += 6 * pointerSize; // Owner, EaseFunction, OnComplete, Cts, Registration, TaskCompletionSource
            fieldsSize += 3 * sizeof(float); // Duration, ElapsedTime, OnCompleteDelay
            fieldsSize += sizeof(uint); // TweenIdentifierHash
            fieldsSize += sizeof(bool); // IgnoreTimeScale

            return clrOverhead + fieldsSize;
        }
    }
}