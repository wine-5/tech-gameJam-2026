using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.UI.Effects
{
    /// <summary>
    /// 指定アルファまでフェードするエフェクト。
    /// Hover / Pressトリガーと組み合わせて「押下中は少し暗くする」等に使うのが典型
    /// </summary>
    [Serializable]
    public class AlphaToEffect : UIEffectBase
    {
        [Range(0f, 1f)]
        [SerializeField] private float _targetAlpha = 0.6f;

        [SerializeField] private float _duration = 0.1f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public override async UniTask PlayAsync(UIEffectContext context, CancellationToken token)
        {
            float start = context.GetAlpha();
            await LerpAsync(
                _duration,
                t => context.SetAlpha(Mathf.Lerp(start, _targetAlpha, _curve.Evaluate(t))),
                token);
        }

        // 解除時はスナップせず、現在のアルファから初期値へ滑らかに戻す
        public override async UniTask StopAsync(UIEffectContext context, CancellationToken token)
        {
            float start = context.GetAlpha();
            await LerpAsync(
                _duration,
                t => context.SetAlpha(Mathf.Lerp(start, context.InitialAlpha, _curve.Evaluate(t))),
                token);
        }

        public override void ResetState(UIEffectContext context)
        {
            context.SetAlpha(context.InitialAlpha);
        }
    }
}
