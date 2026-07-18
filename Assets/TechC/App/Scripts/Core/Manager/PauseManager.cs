using System;
using UnityEngine;
using ProjectName.Core.Manager;
using ProjectName.Core.Log;
using ProjectName.InGame.UI;
using ProjectName.Scene.Manager;

namespace ProjectName
{
    /// <summary>
    /// ゲーム全体のPause状態を管理するシングルトン
    /// 全シーン（Story, InGame, Result）で使用
    /// Pause状態管理、メニュー表示、ボタン処理を一元管理
    /// </summary>
    public class PauseManager : Singleton<PauseManager>
    {
        protected override bool UseDontDestroyOnLoad => true;

        public event Action OnPaused;
        public event Action OnResumed;

        [SerializeField] private PauseMenuView pauseMenuView;
        private bool _isPaused;

        public bool IsPaused => _isPaused;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (pauseMenuView != null)
            {
                pauseMenuView.Init();
                SetupMenuView();
            }
        }

        /// <summary>
        /// Pause状態を設定。Time.timeScale も自動制御
        /// </summary>
        public void SetPaused(bool paused)
        {
            if (_isPaused == paused)
                return;

            _isPaused = paused;

            if (paused)
            {
                Time.timeScale = 0f;
                OnPaused?.Invoke();
                CusLog.Log("PauseManager", "ゲームを一時停止しました");
            }
            else
            {
                Time.timeScale = 1f;
                OnResumed?.Invoke();
                CusLog.Log("PauseManager", "ゲームを再開しました");
            }
        }

        /// <summary>
        /// Pause状態をトグル
        /// </summary>
        public void TogglePause()
        {
            SetPaused(!_isPaused);
        }

        protected override void OnRelease()
        {
            if (pauseMenuView != null)
                pauseMenuView.OnButtonClicked -= HandleMenuButton;

            OnPaused = null;
            OnResumed = null;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// PauseMenuView のイベントをリッスン
        /// </summary>
        private void SetupMenuView()
        {
            pauseMenuView.OnButtonClicked += HandleMenuButton;
        }

        /// <summary>
        /// メニューボタン処理を一元管理
        /// </summary>
        private void HandleMenuButton(PauseMenuButton button)
        {
            SetPaused(false);  // Pause解除

            switch (button)
            {
                case PauseMenuButton.BackInGame:
                    CusLog.Log("PauseManager", "ゲームに戻ります");
                    break;

                case PauseMenuButton.ToTitle:
                    CusLog.Log("PauseManager", "タイトルに遷移します");
                    if (SceneController.I != null)
                        SceneController.I.ChangeToTitleScene();
                    break;
            }
        }
    }
}
