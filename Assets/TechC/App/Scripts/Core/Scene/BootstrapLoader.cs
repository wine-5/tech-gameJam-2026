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
        private const string ManagerSceneName = "Manager";
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
            
            bool managerSceneLoaded = IsSceneLoaded(ManagerSceneName);
            
            if (managerSceneLoaded)
            {
                Debug.Log($"[Bootstrap] {ManagerSceneName}シーンは既にロードされています");
                return;
            }
            
            try
            {
                SceneManager.LoadScene(ManagerSceneName, LoadSceneMode.Additive);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Bootstrap] {ManagerSceneName}シーンのロードに失敗しました: {e.Message}");
                Debug.LogError($"[Bootstrap] Build Settingsに'{ManagerSceneName}'シーンが追加されているか確認してください");
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