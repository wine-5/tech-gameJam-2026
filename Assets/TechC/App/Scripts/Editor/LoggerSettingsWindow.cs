using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectName.Core.Log;

namespace ProjectName.Editor
{
    /// <summary>
    /// CusLog設定を編集するためのEditorWindow
    /// </summary>
    public class LoggerSettingsWindow : EditorWindow
    {
        private const int MIN_WINDOW_WIDTH = 400;
        private const int MIN_WINDOW_HEIGHT = 300;
        private const int COLOR_FIELD_WIDTH = 60;
        private const int BUTTON_WIDTH = 60;
        private const int DELETE_BUTTON_WIDTH = 30;
        private const int SCROLL_VIEW_HEIGHT = 300;
        private const int CATEGORY_NAME_WIDTH = 150;
        private const int PREVIEW_WIDTH = 150;

        private LoggerSettings _settings;
        private Vector2 _scrollPosition;
        private string _newCategoryName = "";
        private Color _newCategoryColor = Color.white;

        [MenuItem("Tools/Custom Logger/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<LoggerSettingsWindow>("CusLog Settings");
            window.minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
            window.Show();
        }

        private void OnEnable()
        {
            _settings = LoggerSettings.Instance;
        }

        private void OnGUI()
        {
            if (_settings == null) return;

            EditorGUILayout.Space(10);

            // タイトル
            EditorGUILayout.LabelField("CusLog Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // デバッグフラグ設定
            DrawDebugFlagSection();
            
            EditorGUILayout.Space(10);
            DrawSeparator();
            EditorGUILayout.Space(10);

            // カテゴリ管理
            DrawCategoryManagement();
        }

        /// <summary>
        /// デバッグフラグセクションの描画
        /// </summary>
        private void DrawDebugFlagSection()
        {
            EditorGUILayout.LabelField("Debug Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            bool currentDebugFlag = CusLog.isDebug;
            bool newDebugFlag = EditorGUILayout.Toggle("Enable Debug Logs", currentDebugFlag);
            
            if (newDebugFlag != currentDebugFlag)
            {
                CusLog.isDebug = newDebugFlag;
                EditorUtility.SetDirty(_settings);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "このフラグがOFFの場合、ビルド時にログが出力されません。\n" +
                "※Editor上では [Conditional] 属性により常にログが表示されます。",
                MessageType.Info
            );
            
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// カテゴリ管理セクションの描画
        /// </summary>
        private void DrawCategoryManagement()
        {
            EditorGUILayout.LabelField("Log Categories", EditorStyles.boldLabel);
            
            // 新規カテゴリ追加
            DrawAddCategorySection();
            
            EditorGUILayout.Space(10);
            
            // カテゴリリスト
            DrawCategoryList();
        }

        /// <summary>
        /// 新規カテゴリ追加セクション
        /// </summary>
        private void DrawAddCategorySection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Add New Category", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            _newCategoryName = EditorGUILayout.TextField("Category Name", _newCategoryName);
            _newCategoryColor = EditorGUILayout.ColorField(GUIContent.none, _newCategoryColor, false, false, false, GUILayout.Width(COLOR_FIELD_WIDTH));
            
            EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_newCategoryName));
            if (GUILayout.Button("Add", GUILayout.Width(BUTTON_WIDTH)))
            {
                _settings.AddCategory(_newCategoryName.Trim(), _newCategoryColor);
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
                
                _newCategoryName = "";
                _newCategoryColor = Color.white;
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// カテゴリリストの描画
        /// </summary>
        private void DrawCategoryList()
        {
            EditorGUILayout.LabelField("Registered Categories", EditorStyles.miniBoldLabel);
            
            if (_settings.Categories.Count == 0) return;

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(SCROLL_VIEW_HEIGHT));
            
            List<LogCategory> categoriesToRemove = new List<LogCategory>();
            
            foreach (var category in _settings.Categories)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                // カテゴリ名
                EditorGUILayout.LabelField(category.categoryName, GUILayout.Width(CATEGORY_NAME_WIDTH));
                
                // プレビュー
                GUIStyle previewStyle = new GUIStyle(EditorStyles.label);
                previewStyle.normal.textColor = category.color;
                EditorGUILayout.LabelField($"[{category.categoryName}] Sample", previewStyle, GUILayout.Width(PREVIEW_WIDTH));
                
                // 色変更
                Color newColor = EditorGUILayout.ColorField(GUIContent.none, category.color, false, false, false, GUILayout.Width(COLOR_FIELD_WIDTH));
                if (newColor != category.color)
                {
                    category.color = newColor;
                    EditorUtility.SetDirty(_settings);
                }
                
                GUILayout.FlexibleSpace();
                
                // 削除ボタン
                if (GUILayout.Button("×", GUILayout.Width(DELETE_BUTTON_WIDTH)))
                {
                    if (EditorUtility.DisplayDialog("確認", 
                        $"カテゴリ '{category.categoryName}' を削除しますか?", 
                        "削除", "キャンセル"))
                    {
                        categoriesToRemove.Add(category);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // 削除処理
            foreach (var category in categoriesToRemove)
            {
                _settings.RemoveCategory(category.categoryName);
                EditorUtility.SetDirty(_settings);
            }
            
            if (categoriesToRemove.Count > 0)
            {
                AssetDatabase.SaveAssets();
            }
            
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// セパレーターを描画
        /// </summary>
        private void DrawSeparator()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }
}