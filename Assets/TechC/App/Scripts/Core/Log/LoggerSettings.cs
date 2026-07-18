using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TechC.Core.Log
{
    /// <summary>
    /// ログカテゴリの設定データ
    /// </summary>
    [System.Serializable]
    public class LogCategory
    {
        public string CategoryName;
        public Color Color = Color.white;
        public bool IsEnabled = true;  // カテゴリー別のオン/オフフラグ

        public LogCategory(string name, Color color, bool enabled = true)
        {
            this.CategoryName = name;
            this.Color = color;
            this.IsEnabled = enabled;
        }
    }

    /// <summary>
    /// CusLog設定を保存するScriptableObject
    /// </summary>
    public class LoggerSettings : ScriptableObject
    {
        private static LoggerSettings _instance;
        public static LoggerSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<LoggerSettings>("LoggerSettings");
#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        _instance = CreateInstance<LoggerSettings>();
                        _instance.InitializeDefaultCategories();
                        
                        // Resources フォルダに保存
                        string path = "Assets/Resources";
                        if (!UnityEditor.AssetDatabase.IsValidFolder(path))
                        {
                            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                        }
                        UnityEditor.AssetDatabase.CreateAsset(_instance, "Assets/Resources/LoggerSettings.asset");
                        UnityEditor.AssetDatabase.SaveAssets();
                    }
#endif
                }
                return _instance;
            }
        }

        [SerializeField]
        private List<LogCategory> _categories = new List<LogCategory>();

        public List<LogCategory> Categories => _categories;

        /// <summary>
        /// デフォルトカテゴリの初期化
        /// </summary>
        private const float DefaultPlayerColorR = 0.3f;
        private const float DefaultPlayerColorG = 0.7f;
        private const float DefaultPlayerColorB = 1f;
        
        private const float DefaultEnemyColorR = 1f;
        private const float DefaultEnemyColorG = 0.3f;
        private const float DefaultEnemyColorB = 0.3f;
        
        private const float DefaultUIColorR = 0.5f;
        private const float DefaultUIColorG = 1f;
        private const float DefaultUIColorB = 0.5f;
        
        private const float DefaultAudioColorR = 1f;
        private const float DefaultAudioColorG = 0.8f;
        private const float DefaultAudioColorB = 0.3f;
        
        private const float DefaultNetworkColorR = 1f;
        private const float DefaultNetworkColorG = 0.5f;
        private const float DefaultNetworkColorB = 1f;
        
        private const float DefaultSystemColorR = 0.8f;
        private const float DefaultSystemColorG = 0.8f;
        private const float DefaultSystemColorB = 0.8f;
        
        private const int HexColorMaxValue = 255;

        private void InitializeDefaultCategories()
        {
            _categories = new List<LogCategory>
            {
                new LogCategory("Player", new Color(DefaultPlayerColorR, DefaultPlayerColorG, DefaultPlayerColorB)),      // 水色
                new LogCategory("Enemy", new Color(DefaultEnemyColorR, DefaultEnemyColorG, DefaultEnemyColorB)),        // 赤
                new LogCategory("UI", new Color(DefaultUIColorR, DefaultUIColorG, DefaultUIColorB)),           // 緑
                new LogCategory("Audio", new Color(DefaultAudioColorR, DefaultAudioColorG, DefaultAudioColorB)),        // オレンジ
                new LogCategory("Network", new Color(DefaultNetworkColorR, DefaultNetworkColorG, DefaultNetworkColorB)),        // ピンク
                new LogCategory("System", new Color(DefaultSystemColorR, DefaultSystemColorG, DefaultSystemColorB)),     // グレー
            };
        }

        /// <summary>
        /// カテゴリの色を取得
        /// </summary>
        public string GetCategoryColor(string categoryName)
        {
            var category = _categories.FirstOrDefault(c => c.CategoryName == categoryName);
            if (category != null)
            {
                return ColorToHex(category.Color);
            }
            
            // 未登録のカテゴリは白色
            return "#FFFFFF";
        }

        /// <summary>
        /// カテゴリが有効かどうかを判定
        /// </summary>
        public bool IsCategoryEnabled(string categoryName)
        {
            var category = _categories.FirstOrDefault(c => c.CategoryName == categoryName);
            return category?.IsEnabled ?? true; // 未登録のカテゴリはデフォルトで有効
        }

        /// <summary>
        /// カテゴリの有効/無効を設定
        /// </summary>
        public void SetCategoryEnabled(string categoryName, bool enabled)
        {
            var category = _categories.FirstOrDefault(c => c.CategoryName == categoryName);
            if (category != null)
            {
                category.IsEnabled = enabled;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        /// <summary>
        /// カテゴリを追加
        /// </summary>
        public void AddCategory(string categoryName, Color color)
        {
            if (!_categories.Any(c => c.CategoryName == categoryName))
                _categories.Add(new LogCategory(categoryName, color));
        }

        /// <summary>
        /// カテゴリを削除
        /// </summary>
        public void RemoveCategory(string categoryName)
        {
            _categories.RemoveAll(c => c.CategoryName == categoryName);
        }

        /// <summary>
        /// ColorをHEXコードに変換
        /// </summary>
        private string ColorToHex(Color color)
        {
            int r = Mathf.RoundToInt(color.r * HexColorMaxValue);
            int g = Mathf.RoundToInt(color.g * HexColorMaxValue);
            int b = Mathf.RoundToInt(color.b * HexColorMaxValue);
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }
}