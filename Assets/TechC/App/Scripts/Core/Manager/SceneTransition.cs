using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectName.Core.Manager
{
    /// <summary>
    /// シーン遷移用のフェードオーバーレイを管理するSingleton。
    /// 必要なCanvasは実行時に自前で生成するため、シーンへの配置は不要。
    /// SceneController.LoadSceneAsync から自動的に使われる
    /// </summary>
    public class SceneTransition : Singleton<SceneTransition>
    {
        protected override bool UseDontDestroyOnLoad => true;

        [SerializeField] private Color _fadeColor = Color.black;

        [Tooltip("ゲーム起動時に黒画面からフェードインして開始するか")]
        [SerializeField] private bool _fadeInOnBoot = true;

        [Tooltip("起動時フェードインにかける秒数")]
        [SerializeField] private float _bootFadeDuration = 0.5f;

        private CanvasGroup _canvasGroup;

        protected override void OnInitialize()
        {
            CreateOverlay();

            // 起動時は覆った状態から始めて、最初のシーンをフェードインで見せる
            if (_fadeInOnBoot)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
                FadeInAsync(_bootFadeDuration, destroyCancellationToken).Forget();
            }
        }

        /// <summary>
        /// フェード用のCanvas階層（Canvas → CanvasGroup → 全面Image）を生成する
        /// </summary>
        private void CreateOverlay()
        {
            var canvasObject = new GameObject("FadeCanvas");
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // 常に最前面に表示する

            _canvasGroup = canvasObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;

            var imageObject = new GameObject("FadeImage");
            imageObject.transform.SetParent(canvasObject.transform, false);

            var image = imageObject.AddComponent<Image>();
            image.color = _fadeColor;

            // 画面全体に引き伸ばす
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 画面を覆う（フェードアウト）。覆っている間は誤クリックもブロックする
        /// </summary>
        public async UniTask FadeOutAsync(float duration, CancellationToken token = default)
        {
            _canvasGroup.blocksRaycasts = true;
            await FadeToAsync(1f, duration, token);
        }

        /// <summary>
        /// 画面を開ける（フェードイン）
        /// </summary>
        public async UniTask FadeInAsync(float duration, CancellationToken token = default)
        {
            await FadeToAsync(0f, duration, token);
            _canvasGroup.blocksRaycasts = false;
        }

        private async UniTask FadeToAsync(float targetAlpha, float duration, CancellationToken token)
        {
            if (duration <= 0f)
            {
                _canvasGroup.alpha = targetAlpha;
                return;
            }

            float start = _canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();
                // Pause中（timeScale=0）でも遷移できるようunscaledで進める
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(elapsed / duration));
                await UniTask.Yield(token);
            }

            _canvasGroup.alpha = targetAlpha;
        }
    }
}
