using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectName.UI.Effects
{
    /// <summary>
    /// sin波でふわふわと拡大縮小を繰り返すエフェクト。
    /// Alwaysトリガーで「常に脈打つボタン」に使うのが典型
    /// </summary>
    [Serializable]
    public class ScalePulseEffect : UIEffectBase
    {
        [SerializeField] private float _minScale = 0.97f;
        [SerializeField] private float _maxScale = 1.05f;

        [Tooltip("1往復にかかる秒数")]
        [SerializeField] private float _period = 1.2f;

        public override async UniTask PlayAsync(UIEffectContext context, CancellationToken token)
        {
            if (context.RectTransform == null) return;

            float elapsed = 0f;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;

                // sin波を0〜1に正規化（開始時はちょうど中間なので繋ぎ目が滑らか）
                float t = (Mathf.Sin(elapsed / Mathf.Max(0.01f, _period) * Mathf.PI * 2f) + 1f) * 0.5f;
                context.SetScale(Mathf.Lerp(_minScale, _maxScale, t));

                await UniTask.Yield(token);
            }
        }

        public override void ResetState(UIEffectContext context)
        {
            if (context.RectTransform != null)
                context.RectTransform.localScale = context.InitialScale;
        }
    }
}
