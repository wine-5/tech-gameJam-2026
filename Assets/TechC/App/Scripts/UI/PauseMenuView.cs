using UnityEngine;
using UnityEngine.UI;
using TechC.Core.Manager;
using TechC.Core.Extentions;

namespace TechC.InGame.UI
{
    public enum PauseMenuButton
    {
        BackInGame,
        ToTitle
    }

    /// <summary>
    /// Pause中に表示されるメニューUI
    /// InGameへ戻るボタンとタイトルへ戻るボタンのみ実装
    /// </summary>
    public class PauseMenuView : MonoBehaviour
    {
        public event System.Action<PauseMenuButton> OnButtonClicked;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button backInGameButton;
        [SerializeField] private Button toTitleButton;

        private void Awake()
        {
            // 初期状態は非表示
            canvasGroup.Hide();
        }

        /// <summary>
        /// Awakeの後の初期化処理
        /// PauseManagerから参照される
        /// </summary>
        public void Init()
        {
            if (!PauseManager.IsValid())
                return;

            PauseManager.I.OnPaused += OnPauseStart;
            PauseManager.I.OnResumed += OnPauseEnd;

            // ボタンリスナー登録
            if (backInGameButton != null)
                backInGameButton.onClick.AddListener(() => OnButtonClicked?.Invoke(PauseMenuButton.BackInGame));

            if (toTitleButton != null)
                toTitleButton.onClick.AddListener(() => OnButtonClicked?.Invoke(PauseMenuButton.ToTitle));
        }

        private void OnDisable()
        {
            if (PauseManager.IsValid())
            {
                PauseManager.I.OnPaused -= OnPauseStart;
                PauseManager.I.OnResumed -= OnPauseEnd;
            }

            if (backInGameButton != null)
                backInGameButton.onClick.RemoveAllListeners();

            if (toTitleButton != null)
                toTitleButton.onClick.RemoveAllListeners();
        }

        private void OnPauseStart() => canvasGroup.Show();

        private void OnPauseEnd() => canvasGroup.Hide();
    }
}
