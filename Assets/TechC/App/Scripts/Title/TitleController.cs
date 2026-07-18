using UnityEngine;
using UnityEngine.UI;
using ProjectName.Audio;
using ProjectName.Core.Log;
using ProjectName.Core.Manager;
using ProjectName.Scene.Manager;

namespace ProjectName.Title
{
    /// <summary>
    /// Titleシーンの最上位モジュール。
    /// シーン全体の初期化・進行はここを起点に書く
    /// </summary>
    public class TitleController : MonoBehaviour
    {
        [SerializeField] private Button _inGameButton;
        [SerializeField] private Button _exitButton;

        private void Start()
        {
            if (_inGameButton != null)
                _inGameButton.onClick.AddListener(() => SceneController.I.ChangeToInGameScene());
            else
                CusLog.Error("TitleController", "InGameボタンが設定されていません");

            if (_exitButton != null)
                _exitButton.onClick.AddListener(() => GameManager.I.QuitGame());
            else
                CusLog.Error("TitleController", "Exitボタンが設定されていません");

            // AudioDataSOにTitle用BGMを追加したらコメントを解除する
            // AudioManager.I.PlayBGM(BGMType.Title);
        }
    }
}
