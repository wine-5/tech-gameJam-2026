#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace TechC.Editor
{
    [InitializeOnLoad]
    public static class SceneDropdownToolbar
    {
        private static VisualElement _rootVisualElement;
        private static VisualElement _leftZoneAlign;
        private static string[] _scenePaths;
        private static string[] _sceneNames;

        static SceneDropdownToolbar()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            if (_rootVisualElement != null) return;
            RebuildToolbar();
        }

        private static void RebuildToolbar()
        {
            try
            {
                var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
                var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
                var currentToolbar = toolbars.FirstOrDefault();
                if (currentToolbar == null) return;

                var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
                var fieldInfo = toolbarType.GetField("m_Root", bindingFlags);

                _rootVisualElement = fieldInfo.GetValue(currentToolbar) as VisualElement;
                if (_rootVisualElement == null) return;

                _leftZoneAlign = _rootVisualElement.Q("ToolbarZoneLeftAlign");
                if (_leftZoneAlign == null) return;

                LoadScenes();
                AddSceneIconButton();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SceneDropdownToolbar] エラー: {e.Message}");
            }
        }

        /// <summary>
        /// シーンアイコンを描画
        /// </summary>
        private static void AddSceneIconButton()
        {
            if (_sceneNames.Length == 0) return;

            // IMGUIContainer ボタンを作成
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandHeight(false));
                if (GUILayout.Button(EditorGUIUtility.IconContent("SceneAsset Icon"), GUILayout.Width(30), GUILayout.Height(22)))
                    ShowSceneMenu();

                GUILayout.EndHorizontal();
            });

            container.style.width = 30;
            container.style.height = 22;
            container.style.marginLeft = 2;
            container.style.marginRight = 2;

            // PlayModeボタンを探す
            VisualElement playModeTools = _rootVisualElement.Query<VisualElement>()
                .Where(e => e.name == "PlayMode")
                .First();

            if (playModeTools != null)
            {
                var parent = playModeTools.parent;
                int index = parent.IndexOf(playModeTools);
                parent.Insert(index, container);
            }
            else
            {
                // PlayModeが見つからない場合は左端に追加
                _leftZoneAlign.Add(container);
            }
        }

        /// <summary>
        /// 選択できるシーンの表示
        /// </summary>
        private static void ShowSceneMenu()
        {
            GenericMenu menu = new GenericMenu();
            var currentScene = SceneManager.GetActiveScene();
            
            // 現在のシーンを表示
            menu.AddDisabledItem(new GUIContent($"● {currentScene.name} (現在)"));
            menu.AddSeparator("");
            
            // フォルダ別にシーンを表示
            var productionScenes = _scenePaths.Where(p => p.Contains("/Production/")).ToArray();
            var workspaceScenes = _scenePaths.Where(p => p.Contains("/Workspace/")).ToArray();
            
            // Production フォルダ
            if (productionScenes.Length > 0)
            {
                menu.AddDisabledItem(new GUIContent("─── Production ───"));
                foreach (var path in productionScenes)
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (name == currentScene.name) continue;
                    
                    menu.AddItem(new GUIContent($"  {name}"), false, () =>
                    {
                        OpenScene(path);
                    });
                }
                menu.AddSeparator("");
            }
            
            // Workspace フォルダ
            if (workspaceScenes.Length > 0)
            {
                menu.AddDisabledItem(new GUIContent("─── Workspace ───"));
                foreach (var path in workspaceScenes)
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (name == currentScene.name) continue;
                    
                    menu.AddItem(new GUIContent($"  {name}"), false, () =>
                    {
                        OpenScene(path);
                    });
                }
                menu.AddSeparator("");
            }
            
            menu.AddItem(new GUIContent("Build Settings..."), false, () =>
            {
                EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            });
            
            menu.DropDown(new Rect(0, 20, 0, 0));
        }

        /// <summary>
        /// シーンを開く
        /// </summary>
        private static void OpenScene(string path)
        {
            if (Application.isPlaying)
            {
                SceneManager.LoadScene(path); // 再生中：普通にロード

            }
            else
            {
                // 編集モード：シーンを保存してから切り替え
                bool saved = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                if (!saved) return; // キャンセルされたら中断

                EditorSceneManager.OpenScene(path);

            }
        }

        /// <summary>
        /// TechCプロジェクトのシーンを取得
        /// </summary>
        private static void LoadScenes()
        {
            var productionGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/TechC/App/Scenes/Production" });
            var workspaceGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/TechC/App/Scenes/Workspace" });
            
            var allGuids = productionGuids.Concat(workspaceGuids).ToArray();
            _scenePaths = allGuids.Select(g => AssetDatabase.GUIDToAssetPath(g))
                                .Where(p => p.EndsWith(".unity"))
                                .OrderBy(p => p)
                                .ToArray();
            _sceneNames = _scenePaths.Select(p => System.IO.Path.GetFileNameWithoutExtension(p)).ToArray();
            

        }
    }
}
#endif