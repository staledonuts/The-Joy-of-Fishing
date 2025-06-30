using UnityEngine;
using Cysharp.Threading.Tasks;

namespace DonutEngine
{
    /// <summary>
    /// Manages the animation for a UI element that notifies the player of a save operation.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SaveNotifier : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private float _stayDuration = 1.0f;

        private RectTransform _rectTransform;
        private Vector2 _onScreenPosition = new Vector2(-100, 100); // Anchored position from lower-right
        private Vector2 _offScreenPosition;
        private bool _isAnimating = false;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            // Start off-screen
            _offScreenPosition = new Vector2(100, -100); // This assumes the anchor is lower-right
            _rectTransform.anchoredPosition = _offScreenPosition;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the save notifier by animating it onto the screen, waiting, and animating it out.
        /// </summary>
        public async UniTask ShowNotifier()
        {
            if (_isAnimating) return;
            _isAnimating = true;

            gameObject.SetActive(true);

            // Animate in
            await _rectTransform.TweenAnchoredPosition(_onScreenPosition, _animationDuration, easeFunction: BTween.Ease.OutCubic);
            
            // Wait for a moment
            await UniTask.Delay((int)(_stayDuration * 1000));
            
            // Animate out
            await _rectTransform.TweenAnchoredPosition(_offScreenPosition, _animationDuration, easeFunction: BTween.Ease.InCubic);
            
            gameObject.SetActive(false);
            _isAnimating = false;
        }
    }
}
