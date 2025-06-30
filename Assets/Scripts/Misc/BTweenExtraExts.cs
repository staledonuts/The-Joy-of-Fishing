using System;
using Ami.BroAudio;
using Cysharp.Threading.Tasks;

public static class BTweenExtraExts
{
    /// <summary>
    /// Tweens the volume of an IAudioPlayer (BroAudio) to a target value over a specified duration.
    /// </summary>
    /// <param name="audioPlayer">The IAudioPlayer instance to modify.</param>
    /// <param name="targetVolume">The target volume to tween to.</param>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="onComplete">An action to invoke when the tween completes.</param>
    /// <param name="easeFunction">The easing function to use for the tween.</param>
    /// <param name="onCompleteDelay">A delay in seconds before the onComplete action is invoked.</param>
    public static UniTask TweenVolume(this IAudioPlayer audioPlayer, float targetVolume, float duration, Action onComplete = null, Func<float, float> easeFunction = null, float onCompleteDelay = 0f)
    {
        if (audioPlayer == null) return UniTask.CompletedTask;
        return BTween.Float(audioPlayer, "Volume", vol => audioPlayer.SetVolume(vol), audioPlayer.CurrentPlayingClip.Volume, targetVolume, duration, onComplete, easeFunction, onCompleteDelay);
    }
}