using UnityEngine;

namespace TechC.Core.Manager
{
    /// <summary>
    /// ゲーム全体を管理するSingleton。
    /// アプリケーションの終了など、特定のシーンに属さない全体処理を持つ
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        protected override bool UseDontDestroyOnLoad => true;

        /// <summary>
        /// アプリケーションを終了する。
        /// エディタ再生中はPlayモードを停止する（Application.Quitはエディタでは効かないため）
        /// </summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
