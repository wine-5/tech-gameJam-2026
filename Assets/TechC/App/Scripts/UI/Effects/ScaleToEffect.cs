using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.UI.Effects
{
    /// <summary>
    /// 指定倍率まで拡大（縮小）するエフェクト。
    /// Hoverトリガーと組み合わせて「ホバーで拡大→離れたら滑らかに戻る」に使うのが典型
    /// </summary>
    [Serializable]
    public class ScaleToEffect : UIEffectBase
    {
        [SerializeField] private float _targetScale = 1.1f;
        [SerializeField] private float _duration = 0.1f;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public override async UniTask PlayAsync(UIEffectContext context, CancellationToken token)
        {
            if (context.RectTransform == null) return;

            Vector3 start = context.RectTransform.localScale;
            Vector3 target = context.InitialScale * _targetScale;
            await LerpAsync(
                _duration,
                t => context.RectTransform.localScale = Vector3.LerpUnclamped(start, target, _curve.Evaluate(t)),
                token);
        }

        // 解除時はスナップせず、現在のscaleから初期値へ滑らかに戻す
        public override async UniTask StopAsync(UIEffectContext context, CancellationToken token)
        {
            if (context.RectTransform == null) return;

            Vector3 start = context.RectTransform.localScale;
            await LerpAsync(
                _duration,
                t => context.RectTransform.localScale = Vector3.LerpUnclamped(start, context.InitialScale, _curve.Evaluate(t)),
                token);
        }

        public override void ResetState(UIEffectContext context)
        {
            if (context.RectTransform != null)
                context.RectTransform.localScale = context.InitialScale;
        }
    }
}
