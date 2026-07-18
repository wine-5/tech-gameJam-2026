using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectName.UI.Effects
{
    /// <summary>
    /// UIエフェクトの基底クラス。
    /// [SerializeReference] で UIEffectController の List に載せるため、具象クラスには [Serializable] を付ける。
    /// ポーズメニューのボタンにも使うため、時間は unscaledDeltaTime ベースで進める（Pause中も動く）。
    /// </summary>
    [Serializable]
    public abstract class UIEffectBase
    {
        [SerializeField] private UIEffectTrigger _trigger = UIEffectTrigger.Always;

        /// <summary>このエフェクトをいつ再生するか</summary>
        public UIEffectTrigger Trigger => _trigger;

        /// <summary>
        /// エフェクトを再生する。
        /// ループ系（パルス・点滅）はキャンセルされるまで戻らない
        /// </summary>
        public abstract UniTask PlayAsync(UIEffectContext context, CancellationToken token);

        /// <summary>
        /// トリガー解除時（ホバーが外れた等）の処理。
        /// デフォルトは即時に初期状態へ戻す。滑らかに戻したいエフェクトはoverrideする
        /// </summary>
        public virtual UniTask StopAsync(UIEffectContext context, CancellationToken token)
        {
            ResetState(context);
            return UniTask.CompletedTask;
        }

        /// <summary>対象を初期状態に戻す</summary>
        public abstract void ResetState(UIEffectContext context);

        /// <summary>
        /// duration かけて進捗 0→1 を毎フレーム通知する（unscaled時間・Pause中も進む）
        /// </summary>
        protected static async UniTask LerpAsync(
            float duration,
            Action<float> onProgress,
            CancellationToken token)
        {
            if (duration <= 0f)
            {
                onProgress?.Invoke(1f);
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;
                onProgress?.Invoke(Mathf.Clamp01(elapsed / duration));
                await UniTask.Yield(token);
            }
        }
    }
}
