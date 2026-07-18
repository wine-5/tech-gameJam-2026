using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using TechC.Core.Log;

namespace TechC.Editor
{

    public class ProjectSetupTool : EditorWindow
    {
        #region フィールド
        private string _newProjectName = "MyProject";
        private int _workerCount = 1;
        private string[] _workerNames = new string[1] { "Worker1" };
        private bool _showSuccess;
        private string _successMessage = "";
        private double _successTime;
        #endregion

        #region Unityメソッド
        [MenuItem("Tools/Project Setup Tool")]
        public static void ShowWindow()
        {
            GetWindow<ProjectSetupTool>("Project Setup Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("プロジェクトセットアップツール", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            // 使い方説明
            EditorGUILayout.HelpBox(
                "【使い方】\n" +
                "1. プロジェクト名を入力してください（ProjectNameから変更されます）\n" +
                "2. 作業者の人数を入力してください\n" +
                "3. 各作業者の名前を入力してください\n" +
                "4. 「プロジェクトをセットアップ」ボタンを押してください\n\n" +
                "※このツールは以下を自動実行します：\n" +
                "• ProjectNameフォルダの名前変更\n" +
                "• 作業者用フォルダ・シーンファイルの作成\n" +
                "• スクリプト内の名前空間更新\n" +
                "• 元のWorkName1ファイルの削除",
                MessageType.Info);

            EditorGUILayout.Space();

            // プロジェクト名の入力
            GUILayout.Label("プロジェクト名（ProjectNameから変更）:");
            _newProjectName = EditorGUILayout.TextField(_newProjectName);

            EditorGUILayout.Space();

            // 作業者人数の入力
            GUILayout.Label("作業者人数:");
            int newWorkerCount = EditorGUILayout.IntField(_workerCount);

            // 人数が変更された場合、配列を再構築
            if (newWorkerCount != _workerCount && newWorkerCount > 0)
            {
                _workerCount = newWorkerCount;
                System.Array.Resize(ref _workerNames, _workerCount);
                for (int i = 0; i < _workerCount; i++)
                {
                    if (string.IsNullOrEmpty(_workerNames[i]))
                    {
                        _workerNames[i] = $"Worker{i + 1}";
                    }
                }
            }

            EditorGUILayout.Space();

            // 作業者名の入力欄を人数分表示
            GUILayout.Label("作業者名:");
            for (int i = 0; i < _workerCount; i++)
            {
                _workerNames[i] = EditorGUILayout.TextField($"作業者 {i + 1}:", _workerNames[i]);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("プロジェクトをセットアップ", GUILayout.Height(30)))
            {
                string workerList = string.Join("\n", _workerNames);
                if (EditorUtility.DisplayDialog("確認",
                    $"以下の変更を実行しますか？\n\n" +
                    $"ProjectName → {_newProjectName}\n\n" +
                    $"作業者:\n{workerList}", "実行", "キャンセル"))
                {
                    SetupProject();
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Root Namespaceを設定"))
            {
                SetRootNamespace();
            }

            EditorGUILayout.Space();

            // 成功メッセージの表示
            if (_showSuccess && EditorApplication.timeSinceStartup - _successTime < 5.0)
            {
                var originalColor = GUI.color;
                GUI.color = Color.green;

                GUIStyle successStyle = new GUIStyle(EditorStyles.helpBox);
                successStyle.normal.textColor = Color.white;
                successStyle.fontStyle = FontStyle.Bold;

                EditorGUILayout.BeginHorizontal(successStyle);
                GUILayout.Label("✓ " + _successMessage, successStyle);
                EditorGUILayout.EndHorizontal();

                GUI.color = originalColor;

                // 自動で画面を更新
                Repaint();
            }
            else if (_showSuccess)
            {
                _showSuccess = false;
            }
        }
        #endregion

        #region セットアップメソッド
        private void SetupProject()
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                // 1. ProjectNameフォルダの名前変更
                RenameProjectFolder();

                // 2. スクリプトファイル内の名前空間変更
                UpdateNamespacesInScripts();

                // 3. 作業者フォルダとシーンの作成
                CreateWorkerFilesAndScenes();

                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();

                // 成功メッセージを表示
                ShowSuccessMessage("プロジェクトのセットアップが完了しました！");

                EditorUtility.DisplayDialog("完了", "プロジェクトのセットアップが完了しました！", "OK");
            }
            catch (System.Exception e)
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.DisplayDialog("エラー", $"セットアップ中にエラーが発生しました：\n{e.Message}", "OK");
            }
        }

        private void RenameProjectFolder()
        {
            string oldPath = "Assets/TechC";
            string newPath = $"Assets/{_newProjectName}";

            // 同じ名前の場合は処理をスキップ
            if (oldPath == newPath)
            {
                LogSuccess($"プロジェクト名は既に '{_newProjectName}' です（変更不要）");
                return;
            }

            if (AssetDatabase.IsValidFolder(oldPath))
            {
                string result = AssetDatabase.MoveAsset(oldPath, newPath);
                if (!string.IsNullOrEmpty(result))
                {
                    CusLog.Error($"フォルダ名変更エラー: {result}");
                }
                else
                {
                    LogSuccess($"フォルダ名を変更: {oldPath} → {newPath}");
                }
            }
            else
            {
                CusLog.Warning($"元のフォルダが見つかりません: {oldPath}（既に変更済みの可能性があります）");
            }
        }

        private void CreateWorkerFilesAndScenes()
        {
            string workspacePath = $"Assets/{_newProjectName}/App/Scenes/Workspace";

            if (!AssetDatabase.IsValidFolder(workspacePath))
            {
                CusLog.Error($"Workspaceフォルダが見つかりません: {workspacePath}");
                return;
            }

            // 元となるWorkName1フォルダとシーンファイルのパス
            string sourceFolder = $"{workspacePath}/WorkName1";
            string sourceScene = $"{workspacePath}/WorkName1.unity";

            // 元ファイルが存在するかチェック
            if (!AssetDatabase.IsValidFolder(sourceFolder))
            {
                CusLog.Error($"元となるフォルダが見つかりません: {sourceFolder}");
                return;
            }

            if (!File.Exists(sourceScene))
            {
                CusLog.Error($"元となるシーンファイルが見つかりません: {sourceScene}");
                return;
            }

            // 各作業者用のフォルダとシーンを作成
            for (int i = 0; i < _workerCount; i++)
            {
                string workerName = _workerNames[i];
                string newFolderPath = $"{workspacePath}/{workerName}";
                string newScenePath = $"{workspacePath}/{workerName}.unity";

                // フォルダの複製
                if (!AssetDatabase.IsValidFolder(newFolderPath))
                {
                    if (!AssetDatabase.CopyAsset(sourceFolder, newFolderPath))
                    {
                        CusLog.Error($"フォルダの複製に失敗: {sourceFolder} → {newFolderPath}");
                    }
                    else
                    {
                        LogSuccess($"フォルダを作成: {newFolderPath}");
                    }
                }

                // シーンファイルの複製
                if (!File.Exists(newScenePath))
                {
                    if (!AssetDatabase.CopyAsset(sourceScene, newScenePath))
                    {
                        CusLog.Error($"シーンファイルの複製に失敗: {sourceScene} → {newScenePath}");
                    }
                    else
                    {
                        LogSuccess($"シーンファイルを作成: {newScenePath}");
                    }
                }
            }

            // すべての複製が完了したら、元のWorkName1フォルダとシーンを削除
            CusLog.Log("元のファイルを削除中...");

            // 元のシーンファイルを削除
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(sourceScene))
            {
                if (AssetDatabase.DeleteAsset(sourceScene))
                {
                    LogSuccess($"元のシーンファイルを削除: {sourceScene}");
                }
                else
                {
                    CusLog.Error($"シーンファイルの削除に失敗: {sourceScene}");
                }
            }

            // 元のフォルダを削除
            if (AssetDatabase.IsValidFolder(sourceFolder))
            {
                if (AssetDatabase.DeleteAsset(sourceFolder))
                {
                    LogSuccess($"元のフォルダを削除: {sourceFolder}");
                }
                else
                {
                    CusLog.Error($"フォルダの削除に失敗: {sourceFolder}");
                }
            }

            LogSuccess("元のファイル削除完了");
        }
        #endregion

        #region ユーティリティメソッド
        private void UpdateNamespacesInScripts()
        {
            string[] scriptFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

            foreach (string scriptFile in scriptFiles)
            {
                string content = File.ReadAllText(scriptFile);
                string originalContent = content;

                // ProjectName名前空間の置換
                content = Regex.Replace(content, @"\bnamespace\s+ProjectName\b", $"namespace {_newProjectName}");
                content = Regex.Replace(content, @"\busing\s+ProjectName\b", $"using {_newProjectName}");

                if (content != originalContent)
                {
                    File.WriteAllText(scriptFile, content);
                    string relativePath = "Assets" + scriptFile.Substring(Application.dataPath.Length);
                    CusLog.Log($"スクリプト更新: {relativePath}");
                }
            }
        }

        private void SetRootNamespace()
        {
            // Root Namespaceの設定
            EditorSettings.projectGenerationRootNamespace = _newProjectName;

            ShowSuccessMessage($"Root Namespaceを '{_newProjectName}' に設定しました！");

            EditorUtility.DisplayDialog("完了", $"Root Namespaceを '{_newProjectName}' に設定しました。\n" +
                "Edit → Project Settings → Editor → Root Namespaceで確認できます。", "OK");
        }

        private void ShowSuccessMessage(string message)
        {
            _successMessage = message;
            _showSuccess = true;
            _successTime = EditorApplication.timeSinceStartup;
            Repaint();
        }

        private void LogSuccess(string message)
        {
            CusLog.Log($"<color=green>[成功] {message}</color>");
        }
        #endregion
    }
}