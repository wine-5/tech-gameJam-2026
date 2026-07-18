using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TechC.Core.Util
{
    /// <summary>
    /// UniTask を使った遅延・繰り返し処理。
    /// Pause 非対応と Pause 対応の2つのセクションに分けて提供。
    /// </summary>
    public static class DelayUtility
    {
        // ============================================================
        // === ポーズ非対応 ===
        // ポーズ状態になっても処理が計測され続ける
        // ============================================================

        /// <summary>
        /// 指定秒数だけ待つ（Pause中でも進む）
        /// 使い方: await DelayUtility.DelayAsync(3f, token);
        /// </summary>
        /// <param name="seconds">待機する秒数</param>
        /// <param name="token">中断用トークン。破棄時などに待機をキャンセルする</param>
        public static async UniTask DelayAsync(
            float seconds,
            CancellationToken token = default)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                token.ThrowIfCancellationRequested();
                // TODO: unscaledDeltaTime にする必要があるかもしれないが、ヒットストップの関係でいったん保留
                elapsed += Time.deltaTime;
                await UniTask.Yield(token);
            }
        }

        /// <summary>
        /// duration の間、interval ごとに action を実行する（Pause中でも進む）
        /// 使い方: await DelayUtility.RepeatAsync(10f, 2f, myAction, token);
        /// </summary>
        /// <param name="duration">繰り返しを続ける合計秒数</param>
        /// <param name="interval">action を実行する間隔（秒）</param>
        /// <param name="action">interval ごとに実行する処理</param>
        /// <param name="token">中断用トークン。破棄時などに繰り返しをキャンセルする</param>
        public static async UniTask RepeatAsync(
            float duration,
            float interval,
            Func<UniTask> action,
            CancellationToken token = default)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                if (action != null)
                    await action();

                elapsed += interval;
                await WaitIntervalAsync(interval, token);
            }
        }

        /// <summary>
        /// condition が true の間、interval ごとに action を実行する（Pause中でも進む）
        /// 使い方: await DelayUtility.RepeatWhileAsync(() => isAlive, 0.1f, myAction, token);
        /// </summary>
        /// <param name="condition">true を返す間だけ繰り返す継続条件</param>
        /// <param name="interval">action を実行する間隔（秒）</param>
        /// <param name="action">interval ごとに実行する処理</param>
        /// <param name="token">中断用トークン。破棄時などに繰り返しをキャンセルする</param>
        public static async UniTask RepeatWhileAsync(
            Func<bool> condition,
            float interval,
            Func<UniTask> action,
            CancellationToken token = default)
        {
            while (condition == null || condition())
            {
                token.ThrowIfCancellationRequested();

                if (action != null)
                    await action();

                await WaitIntervalAsync(interval, token);
            }
        }

        // ============================================================
        // === ポーズ対応 ===
        // ポーズ状態になったら処理が停止し、解除されたら再開する
        // ============================================================

        /// <summary>
        /// 指定秒数だけ待つ（Pause中はカウントしない）
        /// 使い方: await DelayUtility.DelayWithPauseAsync(3f, () => PauseManager.I.IsPaused, token);
        /// </summary>
        /// <param name="seconds">待機する秒数（Pause中の時間は含めない）</param>
        /// <param name="isPausedFunc">ポーズ判定。true を返す間は時間を進めない（例: () => PauseManager.I.IsPaused）</param>
        /// <param name="token">中断用トークン。破棄時などに待機をキャンセルする</param>
        public static async UniTask DelayWithPauseAsync(
            float seconds,
            Func<bool> isPausedFunc,
            CancellationToken token = default)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                token.ThrowIfCancellationRequested();

                if (isPausedFunc != null && isPausedFunc())
                {
                    await UniTask.Yield(token);
                    continue;
                }

                elapsed += Time.deltaTime;
                await UniTask.Yield(token);
            }
        }

        /// <summary>
        /// duration の間、interval ごとに action を実行する（Pause中はカウントしない）
        /// 使い方: await DelayUtility.RepeatWithPauseAsync(5f, 0.5f, myAction, () => PauseManager.I.IsPaused, token);
        /// </summary>
        /// <param name="duration">繰り返しを続ける合計秒数（Pause中の時間は含めない）</param>
        /// <param name="interval">action を実行する間隔（秒）</param>
        /// <param name="action">interval ごとに実行する処理</param>
        /// <param name="isPausedFunc">ポーズ判定。true を返す間は時間を進めない（例: () => PauseManager.I.IsPaused）</param>
        /// <param name="token">中断用トークン。破棄時などに繰り返しをキャンセルする</param>
        public static async UniTask RepeatWithPauseAsync(
            float duration,
            float interval,
            Func<UniTask> action,
            Func<bool> isPausedFunc,
            CancellationToken token = default)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                while (isPausedFunc?.Invoke() == true)
                {
                    token.ThrowIfCancellationRequested();
                    await UniTask.Yield(token);
                }

                if (action != null)
                    await action();

                elapsed += interval;
                await WaitIntervalWithPauseAsync(interval, isPausedFunc, token);
            }
        }

        /// <summary>
        /// condition が true の間、interval ごとに action を実行する（Pause中はカウントしない）
        /// 使い方: await DelayUtility.RepeatWhileWithPauseAsync(() => isAlive, 0.1f, myAction, () => PauseManager.I.IsPaused, token);
        /// </summary>
        /// <param name="condition">true を返す間だけ繰り返す継続条件</param>
        /// <param name="interval">action を実行する間隔（秒）</param>
        /// <param name="action">interval ごとに実行する処理</param>
        /// <param name="isPausedFunc">ポーズ判定。true を返す間は時間を進めない（例: () => PauseManager.I.IsPaused）</param>
        /// <param name="token">中断用トークン。破棄時などに繰り返しをキャンセルする</param>
        public static async UniTask RepeatWhileWithPauseAsync(
            Func<bool> condition,
            float interval,
            Func<UniTask> action,
            Func<bool> isPausedFunc,
            CancellationToken token = default)
        {
            while (condition == null || condition())
            {
                token.ThrowIfCancellationRequested();

                while (isPausedFunc?.Invoke() == true)
                {
                    token.ThrowIfCancellationRequested();
                    await UniTask.Yield(token);
                }

                if (action != null)
                    await action();

                await WaitIntervalWithPauseAsync(interval, isPausedFunc, token);
            }
        }

        /// <summary>
        /// duration の間、進捗 t（0→1）を毎フレーム通知する（Pause中は進めない）
        /// 移動・フェードなどの補間処理に使う
        /// 使い方: await DelayUtility.LerpWithPauseAsync(0.5f, t => cg.alpha = t, () => PauseManager.I.IsPaused, token);
        /// </summary>
        /// <param name="duration">補間にかける合計秒数（Pause中の時間は含めない）</param>
        /// <param name="onProgress">毎フレーム呼ばれる処理。引数 t は進捗 0→1（補間が不要なら _ で無視可）。完了時は必ず 1f が渡される</param>
        /// <param name="isPausedFunc">ポーズ判定。true を返す間は進捗を進めない（例: () => PauseManager.I.IsPaused）</param>
        /// <param name="token">中断用トークン。破棄時などに補間をキャンセルする</param>
        public static async UniTask LerpWithPauseAsync(
            float duration,
            Action<float> onProgress,
            Func<bool> isPausedFunc,
            CancellationToken token = default)
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

                if (isPausedFunc != null && isPausedFunc())
                {
                    await UniTask.Yield(token);
                    continue;
                }

                elapsed += Time.deltaTime;
                onProgress?.Invoke(Mathf.Clamp01(elapsed / duration));
                await UniTask.Yield(token);
            }

            onProgress?.Invoke(1f);
        }

        // ============================================================
        // === ポーズ非対応 === (ヘルパー)
        // ============================================================

        /// <summary>
        /// interval 秒待つ（Pause中でも進む）
        /// RepeatAsync / RepeatWhileAsync から内部的に呼ばれる
        /// </summary>
        private static async UniTask WaitIntervalAsync(
            float interval,
            CancellationToken token)
        {
            float t = 0f;
            while (t < interval)
            {
                token.ThrowIfCancellationRequested();
                t += Time.deltaTime;
                await UniTask.Yield(token);
            }
        }

        // ============================================================
        // === ポーズ対応 === (ヘルパー)
        // ============================================================

        /// <summary>
        /// interval 秒待つ（Pause中はカウントしない）
        /// RepeatWithPauseAsync / RepeatWhileWithPauseAsync から内部的に呼ばれる
        /// </summary>
        private static async UniTask WaitIntervalWithPauseAsync(
            float interval,
            Func<bool> isPausedFunc,
            CancellationToken token)
        {
            float t = 0f;
            while (t < interval)
            {
                token.ThrowIfCancellationRequested();

                while (isPausedFunc?.Invoke() == true)
                {
                    token.ThrowIfCancellationRequested();
                    await UniTask.Yield(token);
                }

                t += Time.deltaTime;
                await UniTask.Yield(token);
            }
        }
    }
}
