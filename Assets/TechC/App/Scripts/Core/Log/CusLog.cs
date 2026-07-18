using UnityEngine;
using System.Diagnostics;

namespace ProjectName.Core.Log
{
    /// <summary>
    /// カスタムロガーシステム
    /// ビルド時の出力制御とカテゴリ別の色分けに対応
    /// </summary>
    public static class CusLog
    {
        // ビルド時のログ出力制御フラグ
        public static bool isDebug = true;

        #region 基本ログメソッド

        /// <summary>
        /// 通常ログ（白色）
        /// </summary>
        [HideInCallstack]
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message)
        {
            if (!isDebug) return;
            UnityEngine.Debug.Log($"<color=white>[LOG]</color> {message}");
        }

        /// <summary>
        /// 警告ログ（黄色）
        /// </summary>
        [HideInCallstack]
        [Conditional("UNITY_EDITOR")]
        public static void Warning(string message)
        {
            if (!isDebug) return;
            UnityEngine.Debug.LogWarning($"<color=yellow>[WARNING]</color> {message}");
        }

        /// <summary>
        /// エラーログ（赤色）
        /// </summary>
        [HideInCallstack]
        [Conditional("UNITY_EDITOR")]
        public static void Error(string message)
        {
            if (!isDebug) return;
            UnityEngine.Debug.LogError($"<color=red>[ERROR]</color> {message}");
        }

        #endregion

        #region カスタムカテゴリログメソッド

        /// <summary>
        /// カテゴリ付き通常ログ
        /// </summary>
        [HideInCallstack]
        [Conditional("UNITY_EDITOR")]
        public static void Log(string category, string message)
        {
            if (!isDebug) return;
            
            string colorCode = LoggerSettings.Instance.GetCategoryColor(category);
            UnityEngine.Debug.Log($"<color={colorCode}>[{category}]</color> {message}");
        }

        /// <summary>
        /// カテゴリ付き警告ログ
        /// </summary>
        [HideInCallstack]
        [Conditional("UNITY_EDITOR")]
        public static void Warning(string category, string message)
        {
            if (!isDebug) return;
            
            string colorCode = LoggerSettings.Instance.GetCategoryColor(category);
            UnityEngine.Debug.LogWarning($"<color={colorCode}>[{category}]</color> <color=yellow>[WARNING]</color> {message}");
        }

        /// <summary>
        /// カテゴリ付きエラーログ
        /// </summary>
        [HideInCallstack]
        [Conditional("UNITY_EDITOR")]
        public static void Error(string category, string message)
        {
            if (!isDebug) return;
            
            string colorCode = LoggerSettings.Instance.GetCategoryColor(category);
            UnityEngine.Debug.LogError($"<color={colorCode}>[{category}]</color> <color=red>[ERROR]</color> {message}");
        }

        #endregion

        #region オブジェクト付きログメソッド

        /// <summary>
        /// UnityObjectを指定した通常ログ
        /// </summary>
        [HideInCallstack]
        [Conditional("UNITY_EDITOR")]
        public static void Log(string message, Object context)
        {
            if (!isDebug) return;
            UnityEngine.Debug.Log($"<color=white>[LOG]</color> {message}", context);
        }

        /// <summary>
        /// UnityObjectを指定したカテゴリ付きログ
        /// </summary>
        [HideInCallstack]
        [Conditional("UNITY_EDITOR")]
        public static void Log(string category, string message, Object context)
        {
            if (!isDebug) return;
            
            string colorCode = LoggerSettings.Instance.GetCategoryColor(category);
            UnityEngine.Debug.Log($"<color={colorCode}>[{category}]</color> {message}", context);
        }

        #endregion
    }
}