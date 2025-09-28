using UnityEngine;

namespace DonutPackage.BTween
{
    /// <summary>
    /// An interface for MonoBehaviour components that can have their animation previewed in the editor.
    /// The preview logic is handled by PreviewableAnimationEditor.
    /// </summary>
    public interface IPreviewableAnimation
    {
        /// <summary>
        /// Gets the ScriptableObject that defines the animation parameters.
        /// </summary>
        /// <returns>The animation profile, which should inherit from BaseTweenAnimationProfile.</returns>
        BaseTweenAnimationProfile GetAnimationProfile();

        /// <summary>
        /// Gets the Transform of the visual element that should be animated.
        /// </summary>
        /// <returns>The target transform for animation.</returns>
        Transform GetVisualsTransform();
    }
}
