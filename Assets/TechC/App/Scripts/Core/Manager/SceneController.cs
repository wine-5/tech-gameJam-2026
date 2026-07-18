using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectName.Core.Manager;
using ProjectName.Core.Log;

namespace ProjectName.Scene.Manager
{
    /// <summary>
    /// ゲーム内のシーン名を定義するenum
    /// </summary>
    public enum SceneName
    {
        Title,
        InGame,
        Result
    }

    /// <summary>
    /// シーン遷移を管理するSingletonクラス
    /// Titleシーンで一度生成されれば、他のシーンでも利用可能
    /// </summary>
    public class SceneController : Singleton<SceneController>
    {
        protected override bool UseDontDestroyOnLoad => true;

        [Tooltip("シーン遷移時のフェードにかける秒数（片道）")]
        [SerializeField] private float _fadeDuration = 0.3f;

        /// <summary>
        /// 現在のステージ情報
        /// </summary>
        public SceneName CurrentStage { get; private set; } = SceneName.Title;

        /// <summary>
        /// シーン遷移中かどうか（遷移中の多重ロード防止用）
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// enumで指定されたシーンに非同期で切り替え。
        /// SceneTransitionが初期化されていればフェードアウト→ロード→フェードインを行う
        /// </summary>
        /// <param name="sceneName">遷移先のシーン</param>
        /// <param name="useFade">フェード演出を使うかどうか</param>
        public async UniTask LoadSceneAsync(SceneName sceneName, bool useFade = true)
        {
            // 遷移中の二重呼び出しは無視する
            if (IsLoading) return;

            // Build Settings未登録などでロードを開始できなかった場合はnullが返る
            var operation = SceneManager.LoadSceneAsync(sceneName.ToString());
            if (operation == null)
            {
                CusLog.Error($"シーン '{sceneName}' をロードできません。Build Settingsに登録されているか確認してください");
                return;
            }

            IsLoading = true;
            CurrentStage = sceneName;

            bool fade = useFade && SceneTransition.IsValid();
            if (fade)
            {
                // フェードで画面を覆いきるまでシーンの切り替えを待たせる（ロード自体は裏で進む）
                operation.allowSceneActivation = false;
                await SceneTransition.I.FadeOutAsync(_fadeDuration);
                operation.allowSceneActivation = true;
            }

            await operation.ToUniTask();

            if (fade)
                await SceneTransition.I.FadeInAsync(_fadeDuration);

            IsLoading = false;
        }

        /// <summary>タイトルシーンに遷移</summary>
        public void ChangeToTitleScene() => LoadSceneAsync(SceneName.Title).Forget();

        /// <summary>インゲームシーンに遷移</summary>
        public void ChangeToInGameScene() => LoadSceneAsync(SceneName.InGame).Forget();

        /// <summary>リザルトシーンに遷移</summary>
        public void ChangeResultScene() => LoadSceneAsync(SceneName.Result).Forget();

    }
}
