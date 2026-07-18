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

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _backInGameButton;
        [SerializeField] private Button _toTitleButton;

        private void Awake()
        {
            // 初期状態は非表示
            _canvasGroup.Hide();
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
            if (_backInGameButton != null)
                _backInGameButton.onClick.AddListener(() => OnButtonClicked?.Invoke(PauseMenuButton.BackInGame));

            if (_toTitleButton != null)
                _toTitleButton.onClick.AddListener(() => OnButtonClicked?.Invoke(PauseMenuButton.ToTitle));
        }

        private void OnDisable()
        {
            if (PauseManager.IsValid())
            {
                PauseManager.I.OnPaused -= OnPauseStart;
                PauseManager.I.OnResumed -= OnPauseEnd;
            }

            if (_backInGameButton != null)
                _backInGameButton.onClick.RemoveAllListeners();

            if (_toTitleButton != null)
                _toTitleButton.onClick.RemoveAllListeners();
        }

        private void OnPauseStart() => _canvasGroup.Show();

        private void OnPauseEnd() => _canvasGroup.Hide();
    }
}
