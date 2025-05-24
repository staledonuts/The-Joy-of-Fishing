using UnityEngine;
using UnityEditor; // Required for editor scripts

// This attribute tells Unity that this is a custom editor for the FishSpawner class
[CustomEditor(typeof(FishSpawner))]
public class FishSpawnerEditor : Editor
{
    private void OnSceneGUI()
    {
        // Get a reference to the FishSpawner instance being inspected
        FishSpawner fishSpawner = (FishSpawner)target;

        // Check if the spawnerPoint is assigned to avoid errors
        if (fishSpawner.spawnerPoint == null)
        {
            return;
        }

        // Store the spawnerPoint's transform for easier access
        Transform spawnerTransform = fishSpawner.spawnerPoint;

        // Get the spawnRadius from the FishSpawner
        float spawnRadius = 0f;

        // We need to access the serializedObject to get the spawnRadius value
        // correctly, especially if it's not public or has a [SerializeField] attribute.
        SerializedProperty spawnRadiusProp = serializedObject.FindProperty("spawnRadius");
        if (spawnRadiusProp != null)
        {
            spawnRadius = spawnRadiusProp.floatValue;
        }
        else
        {
            // Fallback if the property can't be found (should not happen with [SerializeField])
            // Or, if you made spawnRadius public, you could directly access it:
            // spawnRadius = fishSpawner.spawnRadius;
            Debug.LogWarning("Could not find SerializedProperty 'spawnRadius' on FishSpawner. Ensure it's serialized.");
            return;
        }


        // Set the color for the circle
        Handles.color = Color.cyan;

        // Store the current matrix
        Matrix4x4 oldMatrix = Handles.matrix;

        // Set the Handles matrix to the spawnerPoint's local space
        // This makes drawing easier if the spawnerPoint is rotated or scaled.
        // However, for a simple circle based on world position and a radius,
        // directly using world coordinates is often clearer.
        // For this case, we'll draw in world space using the spawnerPoint's position.

        // Draw the wire disc (circle)
        // Parameters:
        // 1. Center of the circle (world space)
        // 2. Normal of the plane on which to draw the circle (e.g., Vector3.forward for XY plane)
        // 3. Radius of the circle
        Handles.DrawWireDisc(spawnerTransform.position, Vector3.forward, spawnRadius);

        // --- Optional: Draw handles to adjust radius directly in Scene view ---
        Handles.color = Color.yellow;
        EditorGUI.BeginChangeCheck();
        // Create a radius handle. The '0.05f * HandleUtility.GetHandleSize(spawnerTransform.position)'
        // part is to make the handle size relative to its view in the scene.
        float newSpawnRadius = Handles.RadiusHandle(Quaternion.identity, spawnerTransform.position, spawnRadius, false);
        if (EditorGUI.EndChangeCheck())
        {
            // If the handle was changed, update the spawnRadius property
            Undo.RecordObject(fishSpawner, "Change Spawn Radius"); // For Undo functionality
            spawnRadiusProp.floatValue = newSpawnRadius;
            serializedObject.ApplyModifiedProperties(); // Apply the changes to the actual object
        }

        // Restore the old matrix (important if you changed Handles.matrix)
        // Handles.matrix = oldMatrix; // Not strictly necessary here as we didn't change it globally
    }
}