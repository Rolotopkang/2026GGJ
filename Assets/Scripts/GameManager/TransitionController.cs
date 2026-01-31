using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 过场控制器：直接播放变黑屏 / 从黑屏出来的两个 UI 动画。Singleton，任意处可调用 TransitionController.Inst。
/// 使用 Legacy Animation 组件（Animation）。
/// </summary>
public class TransitionController : SingletonMono<TransitionController>
{
    [Header("黑幕动画")]
    [Tooltip("带 Animation 组件的黑幕根物体（需有两个动画片段）")]
    public RectTransform blackOverlayRoot;

    [Tooltip("变黑屏的动画片段名")]
    public string toBlackClipName = "ToBlack";

    [Tooltip("从黑屏出来的动画片段名")]
    public string fromBlackClipName = "FromBlack";

    [Tooltip("黑屏保持时长（秒），用于 PlayBlackTransition")]
    public float blackScreenHoldDuration = 1f;

    private Animation _animation;
    private Coroutine _waitCoroutine;

    private void Start()
    {
        if (blackOverlayRoot != null)
        {
            _animation = blackOverlayRoot.GetComponent<Animation>();
            blackOverlayRoot.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 播放变黑屏动画。onComplete 可选，传入则在动画结束后回调。
    /// </summary>
    public void TransitionToBlack(Action onComplete = null)
    {
        if (_animation == null)
        {
            Debug.LogWarning("[TransitionController] Animation 未找到，请设置 blackOverlayRoot。");
            onComplete?.Invoke();
            return;
        }

        if (_waitCoroutine != null)
            StopCoroutine(_waitCoroutine);

        _animation.Play(toBlackClipName);

        if (onComplete != null)
            _waitCoroutine = StartCoroutine(WaitForAnimationAndInvoke(toBlackClipName, onComplete));
    }

    /// <summary>
    /// 播放在黑屏恢复动画。onComplete 可选，传入则在动画结束后回调。
    /// </summary>
    public void TransitionFromBlack(Action onComplete = null)
    {
        if (_animation == null)
        {
            Debug.LogWarning("[TransitionController] Animation 未找到，请设置 blackOverlayRoot。");
            onComplete?.Invoke();
            return;
        }

        if (_waitCoroutine != null)
            StopCoroutine(_waitCoroutine);

        _animation.Play(fromBlackClipName);

        if (onComplete != null)
            _waitCoroutine = StartCoroutine(WaitForAnimationAndInvoke(fromBlackClipName, onComplete));
    }

    /// <summary>
    /// 播放变黑屏 → 等待指定秒数 → 播放在黑屏恢复。
    /// </summary>
    /// <param name="holdDuration">黑屏保持秒数，null 则使用 blackScreenHoldDuration</param>
    /// <param name="onBlackComplete">黑屏动画播放完后回调（进入保持阶段前）</param>
    /// <param name="onComplete">整个流程结束后回调</param>
    public void PlayBlackTransition(float? holdDuration = null, Action onBlackComplete = null, Action onComplete = null)
    {
        if (_animation == null)
        {
            Debug.LogWarning("[TransitionController] Animation 未找到，请设置 blackOverlayRoot。");
            onBlackComplete?.Invoke();
            onComplete?.Invoke();
            return;
        }

        if (_waitCoroutine != null)
            StopCoroutine(_waitCoroutine);

        float duration = holdDuration ?? blackScreenHoldDuration;
        _waitCoroutine = StartCoroutine(BlackTransitionCoroutine(duration, onBlackComplete, onComplete));
    }

    private IEnumerator BlackTransitionCoroutine(float holdDuration, Action onBlackComplete, Action onComplete)
    {
        _animation.Play(toBlackClipName);

        var toBlackState = _animation[toBlackClipName];
        if (toBlackState != null && toBlackState.length > 0f)
            yield return new WaitForSecondsRealtime(toBlackState.length);
        else
            while (_animation.isPlaying)
                yield return null;

        onBlackComplete?.Invoke();

        yield return new WaitForSecondsRealtime(holdDuration);

        _animation.Play(fromBlackClipName);

        var fromBlackState = _animation[fromBlackClipName];
        if (fromBlackState != null && fromBlackState.length > 0f)
            yield return new WaitForSecondsRealtime(fromBlackState.length);
        else
            while (_animation.isPlaying)
                yield return null;

        _waitCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator WaitForAnimationAndInvoke(string clipName, Action onComplete)
    {
        var state = _animation[clipName];
        if (state != null && state.length > 0f)
            yield return new WaitForSecondsRealtime(state.length);
        else
            while (_animation.isPlaying)
                yield return null;
        _waitCoroutine = null;
        onComplete?.Invoke();
    }
}
