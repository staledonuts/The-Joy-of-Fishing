using UnityEngine;

namespace DonutPackage.BTween
{
    /// <summary>
    /// An abstract base ScriptableObject for defining custom, previewable animations.
    /// Inherit from this class to create a specific set of animation behaviors.
    /// </summary>
    public abstract class BaseTweenAnimationProfile : ScriptableObject
    {
        /// <summary>
        /// Defines and starts the animation logic. Can be called in-editor or at runtime.
        /// </summary>
        /// <param name="target">The Transform of the object to animate.</param>
        /// <param name="initialPosition">The starting local position of the target.</param>
        /// <param name="initialRotation">The starting local rotation of the target.</param>
        public abstract void PlayAnimation(Transform target, Vector3 initialPosition, Quaternion initialRotation);

        /// <summary>
        /// Stops all tweens associated with the target.
        /// </summary>
        /// <param name="target">The Transform of the object that was animated.</param>
        public abstract void StopAnimation(Transform target);
    }
}
