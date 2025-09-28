using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

namespace DonutPackage.BTween
{
    [CreateAssetMenu(fileName = "PlatformMovementProfile", menuName = "DonutPackage/BTween/Platform Movement Profile")]
    public class PlatformMovementProfile : BaseTweenAnimationProfile
    {
        [Header("Platform Movement")]
        [Tooltip("The target position to move to, relative to the platform's starting position.")]
        public Vector3 targetOffset = new Vector3(0, 5, 0);

        [Tooltip("The time it takes to move from the start to the target offset.")]
        public float moveDuration = 3.0f;

        [Tooltip("The easing function to use for the movement.")]
        public BTweenums easeType = BTweenums.InOutSine;

        public override void PlayAnimation(Transform target, Vector3 initialPosition, Quaternion initialRotation)
        {
            Vector3 startPos = initialPosition;
            Vector3 endPos = initialPosition + targetOffset;

            Func<float, float> easeFunc = BTween.GetEaseFunc(easeType);

            Action moveToEnd = null;
            Action moveToStart = null;

            moveToEnd = () => {
                if (target == null) return;
                target.TweenLocalPosition(endPos, moveDuration, moveToStart, easeFunc).Forget();
            };

            moveToStart = () => {
                if (target == null) return;
                target.TweenLocalPosition(startPos, moveDuration, moveToEnd, easeFunc).Forget();
            };

            // Start the animation loop
            moveToEnd();
        }

        public override void StopAnimation(Transform target)
        {
            if (target != null)
            {
                BTween.StopAllTweensForOwner(target);
            }
        }
    }
}
