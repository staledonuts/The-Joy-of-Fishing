using UnityEngine;

namespace DonutPackage.BTween
{
    public class AnimatedPlatform : MonoBehaviour, IPreviewableAnimation
    {
        [Header("Animation")]
        [Tooltip("The animation profile that defines the platform's movement.")]
        [SerializeField] private PlatformMovementProfile animationProfile;

        [Tooltip("The specific transform to animate. If null, this GameObject's transform will be used.")]
        [SerializeField] private Transform visuals;

        private Vector3 _initialPosition;
        private Quaternion _initialRotation;

        // --- IPreviewableAnimation Implementation ---
        public BaseTweenAnimationProfile GetAnimationProfile() => animationProfile;

        public Transform GetVisualsTransform()
        {
            // Default to this object's transform if 'visuals' is not set
            return visuals != null ? visuals : this.transform;
        }
        // ------------------------------------------

        private void Awake()
        {
            // Store initial state for runtime animation
            Transform target = GetVisualsTransform();
            _initialPosition = target.localPosition;
            _initialRotation = target.localRotation;
        }

        private void Start()
        {
            // Start the animation at runtime if a profile is assigned
            if (Application.isPlaying && animationProfile != null)
            {
                animationProfile.PlayAnimation(GetVisualsTransform(), _initialPosition, _initialRotation);
            }
        }
    }
}
