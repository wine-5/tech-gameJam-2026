using System.Threading;
using Cysharp.Threading.Tasks;
using TechC.Core.Util;
using UnityEngine;

namespace TechC.Core.Extentions
{
    /// <summary>
    /// CanvasGroup に関する拡張メソッドを提供するクラス
    /// </summary>
    public static class CanvasGroupExtensions
    {
        public static void Show(this CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public static void Hide(this CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        // ポーズ中はフェードを止める（PauseManager 未配置シーンでは常に進む）
        public static UniTask FadeAsync(
            this CanvasGroup canvasGroup,
            float targetAlpha,
            float duration,
            CancellationToken token = default)
        {
            float startAlpha = canvasGroup.alpha;
            return DelayUtility.LerpWithPauseAsync(
                duration,
                t => canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t),
                () => PauseManager.IsValid() && PauseManager.I.IsPaused,
                token);
        }
    }
}