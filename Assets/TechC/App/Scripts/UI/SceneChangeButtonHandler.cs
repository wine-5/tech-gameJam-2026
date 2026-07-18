using Cysharp.Threading.Tasks;
using ProjectName.Scene.Manager;
using UnityEngine;

namespace ProjectName.UI
{
    /// <summary>
    /// ButtonのOnClickからシーンを変更するためのラッパークラス
    /// </summary>
    public class SceneChangeButtonHandler : MonoBehaviour
    {
        [SerializeField]
        private SceneName _targetScene = SceneName.Title;

        /// <summary>
        /// ButtonのOnClickから呼び出されるメソッド
        /// 指定されたシーンに切り替える
        /// </summary>
        public void ChangeScene()
        {
            if (SceneController.I == null) return;

            SceneController.I.LoadSceneAsync(_targetScene).Forget();
        }
    }
}