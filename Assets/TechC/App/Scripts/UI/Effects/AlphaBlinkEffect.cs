using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectName.UI.Effects
{
    /// <summary>
    /// sin波でアルファの点滅を繰り返すエフェクト。
    /// 「Press Start」のようなテキスト点滅に使うのが典型。
    /// CanvasGroupがあればそちらを、なければGraphic（Text / Image / TMP）のアルファを操作する
    /// </summary>
    [Serializable]
    public class AlphaBlinkEffect : UIEffectBase
    {
        [Range(0f, 1f)]
        [SerializeField] private float _minAlpha = 0.2f;

        [Range(0f, 1f)]
        [SerializeField] private float _maxAlpha = 1f;

        [Tooltip("1往復にかかる秒数")]
        [SerializeField] private float _period = 1f;

        public override async UniTask PlayAsync(UIEffectContext context, CancellationToken token)
        {
            float elapsed = 0f;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;

                // cos波で開始時に_maxAlphaから始める（表示状態から自然に暗くなっていく）
                float t = (Mathf.Cos(elapsed / Mathf.Max(0.01f, _period) * Mathf.PI * 2f) + 1f) * 0.5f;
                context.SetAlpha(Mathf.Lerp(_minAlpha, _maxAlpha, t));

                await UniTask.Yield(token);
            }
        }

        public override void ResetState(UIEffectContext context)
        {
            context.SetAlpha(context.InitialAlpha);
        }
    }
}
