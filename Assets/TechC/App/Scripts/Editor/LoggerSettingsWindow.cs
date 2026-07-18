using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using TechC.Core.Log;

namespace TechC.Editor
{
    /// <summary>
    /// CusLog設定を編集するためのEditorWindow
    /// </summary>
    public class LoggerSettingsWindow : EditorWindow
    {
        private const int MinWindowWidth = 400;
        private const int MinWindowHeight = 300;
        private const int ColorFieldWidth = 60;
        private const int ButtonWidth = 60;
        private const int DeleteButtonWidth = 30;
        private const int ScrollViewHeight = 300;
        private const int CategoryNameWidth = 150;
        private const int PreviewWidth = 150;

        private LoggerSettings _settings;
        private Vector2 _scrollPosition;
        private string _newCategoryName = "";
        private Color _newCategoryColor = Color.white;

        [MenuItem("Tools/Custom Logger/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<LoggerSettingsWindow>("CusLog Settings");
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
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
            
            bool currentDebugFlag = CusLog.IsDebug;
            bool newDebugFlag = EditorGUILayout.Toggle("Enable Debug Logs", currentDebugFlag);
            
            if (newDebugFlag != currentDebugFlag)
            {
                CusLog.IsDebug = newDebugFlag;
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
            _newCategoryColor = EditorGUILayout.ColorField(GUIContent.none, _newCategoryColor, false, false, false, GUILayout.Width(ColorFieldWidth));
            
            EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_newCategoryName));
            if (GUILayout.Button("Add", GUILayout.Width(ButtonWidth)))
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

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(ScrollViewHeight));
            
            List<LogCategory> categoriesToRemove = new List<LogCategory>();
            
            foreach (var category in _settings.Categories)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                // カテゴリ名
                EditorGUILayout.LabelField(category.CategoryName, GUILayout.Width(CategoryNameWidth));
                
                // プレビュー
                GUIStyle previewStyle = new GUIStyle(EditorStyles.label);
                previewStyle.normal.textColor = category.Color;
                EditorGUILayout.LabelField($"[{category.CategoryName}] Sample", previewStyle, GUILayout.Width(PreviewWidth));
                
                // 色変更
                Color newColor = EditorGUILayout.ColorField(GUIContent.none, category.Color, false, false, false, GUILayout.Width(ColorFieldWidth));
                if (newColor != category.Color)
                {
                    category.Color = newColor;
                    EditorUtility.SetDirty(_settings);
                }
                
                GUILayout.FlexibleSpace();
                
                // 削除ボタン
                if (GUILayout.Button("×", GUILayout.Width(DeleteButtonWidth)))
                {
                    if (EditorUtility.DisplayDialog("確認", 
                        $"カテゴリ '{category.CategoryName}' を削除しますか?", 
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
                _settings.RemoveCategory(category.CategoryName);
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