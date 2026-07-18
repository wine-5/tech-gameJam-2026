using UnityEngine;
using TechC.Core.Log;

namespace TechC.Core.Manager
{
    /// <summary>
    /// シングルトンパターンの基底クラス
    /// </summary>
    /// <typeparam name="T">継承先のクラス型</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance = null;
        private static bool _isInitialized = false;
        private static bool _isApplicationQuitting = false;

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static T I
        {
            get
            {
                if (_isApplicationQuitting)
                {
                    CusLog.Warning($"[Singleton] {typeof(T).Name} はアプリケーション終了中のためアクセスできません。");
                    return null;
                }

                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();

                    if (_instance == null)
                        CusLog.Error($"[Singleton] {typeof(T).Name} のインスタンスが見つかりません。Init() を呼び出してください。");
                }

                return _instance;
            }
        }

        /// <summary>
        /// DontDestroyOnLoad を使用するかどうか
        /// </summary>
        protected virtual bool UseDontDestroyOnLoad => true;

        /// <summary>
        /// 重複時に GameObject ごと破壊するか（false だとコンポーネントだけ破壊）
        /// </summary>
        protected virtual bool DestroyTargetGameObject => false;

        /// <summary>
        /// 初期化済みかどうか
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// インスタンスが有効かチェック
        /// </summary>
        public static bool IsValid() => _instance != null && !_isApplicationQuitting;

        /// <summary>
        /// シングルトンの初期化（静的メソッド）
        /// </summary>
        public static T Init()
        {
            if (_instance != null && _isInitialized)
            {
                CusLog.Warning($"[Singleton] {typeof(T).Name} は既に初期化されています。");
                return _instance;
            }

            _instance = FindFirstObjectByType<T>();

            if (_instance == null)
            {
                GameObject singletonObject = new GameObject($"[Singleton] {typeof(T).Name}");
                _instance = singletonObject.AddComponent<T>();
            }

            // インスタンスメソッドで初期化
            _instance.InitializeSingleton();

            return _instance;
        }

        protected virtual void Awake()
        {
            // UseDontDestroyOnLoad が true の場合は自動初期化
            // ただし、既に同じオブジェクトで初期化済みの場合はスキップ
            if (UseDontDestroyOnLoad && (!_isInitialized || _instance != this))
            {
                InitializeSingleton();
            }
        }

        /// <summary>
        /// シングルトンの初期化（インスタンスメソッド）
        /// </summary>
        public void InitializeSingleton()
        {
            // 既に初期化済みの場合は重複として扱う
            if (_isInitialized || (_instance != null && _instance != this))
            {
                HandleDuplicateInstance();
                return;
            }

            _instance = this as T;
            _isInitialized = true;

            // DontDestroyOnLoad の設定
            if (UseDontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            // 派生クラスの初期化処理
            OnInitialize();
        }

        /// <summary>
        /// 重複インスタンスの処理
        /// </summary>
        private void HandleDuplicateInstance()
        {
            string existingInstanceInfo = GetInstanceInfo(I);
            string newInstanceInfo = GetInstanceInfo(this as T);
            
            CusLog.Warning($"[Singleton] {typeof(T).Name} の重複インスタンスを破棄しました");
            CusLog.Warning($"  既存インスタンス: {existingInstanceInfo}");
            CusLog.Warning($"  新規インスタンス: {newInstanceInfo} ← 破棄対象");

            if (DestroyTargetGameObject)
                Destroy(gameObject);
            else
                Destroy(this);
        }

        /// <summary>
        /// インスタンスの詳細情報を取得
        /// </summary>
        private string GetInstanceInfo(T _instance)
        {
            if (_instance == null)
                return "null";

            var component = _instance as Component;
            if (component == null)
                return "非MonoBehaviour";

            var gameObject = component.gameObject;
            var scene = gameObject.scene;
            var hierarchyPath = GetGameObjectPath(gameObject);

            return $"Scene[{scene.name}] Path[{hierarchyPath}]";
        }

        /// <summary>
        /// GameObjectの階層パスを取得
        /// </summary>
        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null)
                return "null";

            var path = obj.name;
            var parent = obj.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        /// <summary>
        /// 派生クラス用の初期化メソッド
        /// </summary>
        protected virtual void OnInitialize()
        {
            // 継承先でオーバーライド
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                OnRelease();

                // UseDontDestroyOnLoad = false の場合は、必ずインスタンス参照をクリア
                if (!UseDontDestroyOnLoad)
                {
                    _instance = null;
                    _isInitialized = false;
                }
                // DontDestroyOnLoad の場合は、このインスタンスが本当に破棄される時のみクリア
                else if (gameObject.scene.name == "DontDestroyOnLoad")
                {
                    _instance = null;
                    _isInitialized = false;
                }

    
            }
        }

        /// <summary>
        /// 派生クラス用の破棄処理
        /// </summary>
        protected virtual void OnRelease()
        {
            // 継承先でオーバーライド
        }

        protected virtual void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }
    }
}