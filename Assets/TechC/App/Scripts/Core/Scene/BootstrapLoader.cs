using UnityEngine;
using UnityEngine.SceneManagement;

namespace TechC.Core.Scene
{
    /// <summary>
    /// どのシーンから開始しても必要なManagerシーンを自動的にロードするBootstrap
    /// InGameシーンから直接テストする場合でも、Managerが正しく初期化されます
    /// </summary>
    public class BootstrapLoader
    {
        private const string MANAGER_SCENE_NAME = "Manager";
        private static bool _isBootstrapped = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (_isBootstrapped)
            {
                Debug.Log("[Bootstrap] 既に初期化済みです");
                return;
            }
            
            _isBootstrapped = true;
            
            bool managerSceneLoaded = IsSceneLoaded(MANAGER_SCENE_NAME);
            
            if (managerSceneLoaded)
            {
                Debug.Log($"[Bootstrap] {MANAGER_SCENE_NAME}シーンは既にロードされています");
                return;
            }
            
            try
            {
                SceneManager.LoadScene(MANAGER_SCENE_NAME, LoadSceneMode.Additive);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Bootstrap] {MANAGER_SCENE_NAME}シーンのロードに失敗しました: {e.Message}");
                Debug.LogError($"[Bootstrap] Build Settingsに'{MANAGER_SCENE_NAME}'シーンが追加されているか確認してください");
            }
        }
        
        private static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName && scene.isLoaded)
                    return true;
            }
            return false;
        }
    }
}