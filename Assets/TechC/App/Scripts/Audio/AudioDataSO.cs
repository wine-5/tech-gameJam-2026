using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TechC.Audio
{
    /// <summary>
    /// BGMオーディオデータ
    /// </summary>
    [System.Serializable]
    public class BGMAudioData
    {
        [Header("基本設定")]
        public BGMType BgmType;
        public AudioClip Clip;

        [Header("音量設定")]
        [Range(0f, 1f)]
        public float Volume = 1f;

        [Header("詳細設定")]
        [Range(-3f, 3f)]
        public float Pitch = 1f;
    }

    /// <summary>
    /// SEオーディオデータ
    /// </summary>
    [System.Serializable]
    public class SEAudioData
    {
        [Header("基本設定")]
        public SEType SeType;
        public AudioClip Clip;

        [Header("音量設定")]
        [Range(0f, 1f)]
        public float Volume = 1f;

        [Header("詳細設定")]
        public bool Loop = false;

        [Range(-3f, 3f)]
        public float Pitch = 1f;

        [Range(0f, 1f)]
        public float SpatialBlend = 0f; // 0 = 2D, 1 = 3D
    }

    /// <summary>
    /// オーディオデータを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "AudioDataSO", menuName = "Audio/AudioDataSO")]
    public class AudioDataSO : ScriptableObject
    {
        [Header("BGMリスト")]
        [SerializeField] private List<BGMAudioData> _bgmList = new();

        [Header("SEリスト")]
        [SerializeField] private List<SEAudioData> _seList = new();

        private Dictionary<BGMType, BGMAudioData> _bgmDictionary;
        private Dictionary<SEType, SEAudioData> _seDictionary;

        /// <summary>
        /// 初期化（最初の1回呼び出す）
        /// </summary>
        public void Initialize()
        {
            _bgmDictionary = new();
            _seDictionary = new();

            foreach (var bgmData in _bgmList)
            {
                if (bgmData == null)
                    continue;
                _bgmDictionary[bgmData.BgmType] = bgmData;
            }

            foreach (var seData in _seList)
            {
                if (seData == null)
                    continue;
                _seDictionary[seData.SeType] = seData;
            }
        }

        /// <summary>
        /// BGMデータを取得
        /// </summary>
        public BGMAudioData GetBGMData(BGMType bgmType)
        {
            if (_bgmDictionary == null)
                Initialize();

            if (!_bgmDictionary.TryGetValue(bgmType, out BGMAudioData bgmData))
                return null;
            return bgmData;
        }

        /// <summary>
        /// SEデータを取得
        /// </summary>
        public SEAudioData GetSEData(SEType seType)
        {
            if (_seDictionary == null)
                Initialize();

            if (!_seDictionary.TryGetValue(seType, out SEAudioData seData))
                return null;
            return seData;
        }

        public List<BGMAudioData> BGMList => _bgmList;
        public List<SEAudioData> SEList => _seList;

#if UNITY_EDITOR
        /// <summary>
        /// Editor用：重複チェック
        /// </summary>
        private void OnValidate()
        {
            var bgmDuplicates = _bgmList
                .Where(data => data != null)
                .GroupBy(data => data.BgmType)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (bgmDuplicates.Count > 0)
                Debug.LogWarning($"[AudioDatabase] 重複するBGMが見つかりました: {string.Join(", ", bgmDuplicates)}");

            var seDuplicates = _seList
                .Where(data => data != null)
                .GroupBy(data => data.SeType)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (seDuplicates.Count > 0)
                Debug.LogWarning($"[AudioDatabase] 重複するSEが見つかりました: {string.Join(", ", seDuplicates)}");

            var emptyBGM = _bgmList
                .Where(data => data != null && data.Clip == null)
                .Select(data => data.BgmType)
                .ToList();

            if (emptyBGM.Count > 0)
                Debug.LogWarning($"[AudioDatabase] AudioClipが設定されていないBGM: {string.Join(", ", emptyBGM)}");

            var emptySE = _seList
                .Where(data => data != null && data.Clip == null)
                .Select(data => data.SeType)
                .ToList();

            if (emptySE.Count > 0)
                Debug.LogWarning($"[AudioDatabase] AudioClipが設定されていないSE: {string.Join(", ", emptySE)}");
        }
#endif
    }
}