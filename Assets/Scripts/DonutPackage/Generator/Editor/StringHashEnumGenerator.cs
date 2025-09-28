using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using DonutPackage.Utils; // For the .Hash() method
using UnityEditor.Build; // For NamedBuildTarget

namespace DonutPackage.Generator.Editor
{
    public class StringHashEnumGenerator : EditorWindow
    {
        private const string k_GenerationSymbol = "STRING_HASH_ENUMS_GENERATED";

        private StringHashCollection _collection;
        private SerializedObject _serializedObject;
        private SerializedProperty _timerTagsProp;
        private SerializedProperty _animationHashesProp;
        private Vector2 _scrollPosition;

        [MenuItem("DonutPackage/Generate/String Hash Enums")]
        public static void ShowWindow()
        {
            GetWindow<StringHashEnumGenerator>("String Hash Enums");
        }

        private void OnEnable()
        {
            FindOrCreateCollectionAsset();
        }

        private void OnGUI()
        {
            if (_collection == null)
            {
                EditorGUILayout.HelpBox("Could not find or create the StringHashCollection asset.", MessageType.Error);
                return;
            }

            _serializedObject.Update();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.LabelField("Enum Lists", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Add the string keys you want to turn into hashed enums. The generator will create separate enum files for each list.", MessageType.Info);

            EditorGUILayout.PropertyField(_timerTagsProp, true);
            EditorGUILayout.PropertyField(_animationHashesProp, true);
            
            EditorGUILayout.EndScrollView();

            _serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Generate Enums", GUILayout.Height(40)))
            {
                GenerateEnums();
            }

            if (GUILayout.Button("Force Clear Generation Symbol"))
            {
                RemoveGenerationSymbol();
            }
        }

        private void FindOrCreateCollectionAsset()
        {
            string[] guids = AssetDatabase.FindAssets("t:StringHashCollection");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _collection = AssetDatabase.LoadAssetAtPath<StringHashCollection>(path);
            }
            else
            {
                Debug.Log("[StringHashEnumGenerator] No StringHashCollection asset found. Creating a new one.");
                StringHashCollection asset = ScriptableObject.CreateInstance<StringHashCollection>();
                string path = "Assets/Scripts/DonutPackage/Generator";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/StringHashCollection.asset");
                AssetDatabase.CreateAsset(asset, assetPathAndName);
                AssetDatabase.SaveAssets();
                _collection = asset;
            }

            if (_collection != null)
            {
                _serializedObject = new SerializedObject(_collection);
                _timerTagsProp = _serializedObject.FindProperty("TimerTags");
                _animationHashesProp = _serializedObject.FindProperty("AnimationHashes");
            }
        }

        private void GenerateEnums()
        {
            EnsureAssemblyDefinitions();

            string genFolder = Path.Combine(Application.dataPath, "Scripts/Generated");
            Directory.CreateDirectory(genFolder);

            GenerateEnumFile(genFolder, "TimerTags", _collection.TimerTags, "uint");
            GenerateEnumFile(genFolder, "AnimationHashes", _collection.AnimationHashes, "int");

            AssetDatabase.Refresh();
            Debug.Log("[StringHashEnumGenerator] Enums generated successfully!");
            AddGenerationSymbol();
        }

        private void EnsureAssemblyDefinitions()
        {
            string genFolderPath = "Assets/Scripts/Generated";
            string generatedAsmdefPath = Path.Combine(genFolderPath, "DonutPackage.Generated.asmdef");
            string generatedAsmdefName = "DonutPackage.Generated";
            string timerAsmdefPath = "Assets/Scripts/DonutPackage/Timer/DonutPackage.Timer.asmdef";
            bool needsRefresh = false;

            if (!File.Exists(generatedAsmdefPath))
            {
                Debug.Log($"[StringHashEnumGenerator] Creating missing assembly definition at: {generatedAsmdefPath}");
                const string asmdefContent = @"{
    ""name"": ""DonutPackage.Generated"",
    ""rootNamespace"": """",
    ""references"": [],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
                File.WriteAllText(generatedAsmdefPath, asmdefContent);
                needsRefresh = true;
            }

            if (File.Exists(timerAsmdefPath))
            {
                string timerAsmdefJson = File.ReadAllText(timerAsmdefPath);
                if (!timerAsmdefJson.Contains("\"DonutPackage.Generated\""))
                {
                    Debug.Log($"[StringHashEnumGenerator] Adding missing reference to '{generatedAsmdefName}' in {timerAsmdefPath}");
                    const string updatedTimerAsmdefContent = @"{
    ""name"": ""DonutPackage.Timer"",
    ""rootNamespace"": ""DonutPackage.Timer"",
    ""references"": [
        ""GUID:f51ebe6a0ceec4240a699833d6309b23"",
        ""DonutPackage.Utils"",
        ""DonutPackage.Generated""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": true,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
                    File.WriteAllText(timerAsmdefPath, updatedTimerAsmdefContent);
                    needsRefresh = true;
                }
            }

            if (needsRefresh) AssetDatabase.Refresh();
        }

        private void GenerateEnumFile(string folderPath, string enumName, List<string> stringList, string baseType)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine($"public enum {enumName} : {baseType}");
            sb.AppendLine("{");
            var distinctNames = new HashSet<string>();
            foreach (var name in stringList.Where(n => !string.IsNullOrWhiteSpace(n)))
            {
                string safeName = MakeSafeIdentifier(name);
                if (distinctNames.Add(safeName))
                {
                    if (baseType == "int")
                    {
                        int hash = Animator.StringToHash(name);
                        sb.AppendLine($"    {safeName} = {hash},");
                    }
                    else // uint
                    {
                        uint hash = name.Hash();
                        sb.AppendLine($"    {safeName} = {hash},");
                    }
                }
            }
            sb.AppendLine("}");
            string fullPath = Path.Combine(folderPath, $"{enumName}.cs");
            File.WriteAllText(fullPath, sb.ToString());
        }

        private string MakeSafeIdentifier(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "_";
            var sb = new StringBuilder();
            if (!char.IsLetter(raw[0]) && raw[0] != '_') sb.Append('_');
            foreach (char c in raw) sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            return sb.ToString();
        }

        #region Symbol Management
        private static NamedBuildTarget CurrentNamedBuildTarget => NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

        private static void AddGenerationSymbol()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbols(CurrentNamedBuildTarget);
            List<string> allDefines = definesString.Split(';').ToList();
            if (!allDefines.Contains(k_GenerationSymbol))
            {
                allDefines.Add(k_GenerationSymbol);
                PlayerSettings.SetScriptingDefineSymbols(CurrentNamedBuildTarget, string.Join(";", allDefines.ToArray()));
                Debug.Log($"[StringHashEnumGenerator] Added '{k_GenerationSymbol}' to Scripting Define Symbols.");
            }
        }

        private static void RemoveGenerationSymbol()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbols(CurrentNamedBuildTarget);
            List<string> allDefines = definesString.Split(';').ToList();
            if (allDefines.Contains(k_GenerationSymbol))
            {
                allDefines.Remove(k_GenerationSymbol);
                PlayerSettings.SetScriptingDefineSymbols(CurrentNamedBuildTarget, string.Join(";", allDefines.ToArray()));
                Debug.Log($"[StringHashEnumGenerator] Removed '{k_GenerationSymbol}' from Scripting Define Symbols.");
            }
        }
        #endregion
    }
}
