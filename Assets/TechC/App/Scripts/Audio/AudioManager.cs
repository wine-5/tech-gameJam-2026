using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectName.Core.Manager;
using ProjectName.Core.Log;

namespace ProjectName.Audio
{
    /// <summary>
    /// オーディオ管理システム
    /// BGM・SEの再生、停止、ボリューム制御などを行う
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("設定")]
        [SerializeField] private AudioDataSO _audioDatabase;
        [SerializeField] private int _seSourceCount = 10;

        [Header("初期ボリューム")]
        [Range(0f, 1f)]
        [SerializeField] private float _initialMasterVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float _initialBGMVolume = 0.8f;
        [Range(0f, 1f)]
        [SerializeField] private float _initialSEVolume = 1f;

        protected override bool UseDontDestroyOnLoad => true;

        private AudioSource _bgmSource;
        private BGMType? _currentBGMType;
        private CancellationTokenSource _bgmFadeCts;

        private List<AudioSource> _seSources = new();
        private int _seSourceIndex;

        private float _masterVolume = 1f;
        private float _bgmVolume = 0.8f;
        private float _seVolume = 1f;

        private bool _isBGMPaused;
        private readonly List<AudioSource> _pausedSESources = new();

        protected override void OnInitialize()
        {
            SetupAudioSources();

            _masterVolume = _initialMasterVolume;
            _bgmVolume = _initialBGMVolume;
            _seVolume = _initialSEVolume;

            if (_audioDatabase == null)
                CusLog.Error("AudioManager", "AudioDatabase が設定されていません！");
            else
                _audioDatabase.Initialize();
        }

        private void SetupAudioSources()
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;

            for (int i = 0; i < _seSourceCount; i++)
            {
                AudioSource seSource = gameObject.AddComponent<AudioSource>();
                seSource.loop = false;
                seSource.playOnAwake = false;
                _seSources.Add(seSource);
            }
        }

        #region BGM制御

        /// <summary>
        /// BGMを再生
        /// </summary>
        public void PlayBGM(BGMType bgmType, float fadeTime = 0f)
        {
            if (_audioDatabase == null)
            {
                CusLog.Error("AudioManager", "AudioDatabase が null です");
                return;
            }

            BGMAudioData bgmData = _audioDatabase.GetBGMData(bgmType);
            if (bgmData == null)
            {
                CusLog.Error("AudioManager", $"BGM '{bgmType}' が見つかりません");
                return;
            }

            if (_currentBGMType == bgmType && _bgmSource.isPlaying)
                return;

            if (fadeTime > 0f)
            {
                _bgmFadeCts?.Cancel();
                _bgmFadeCts = new CancellationTokenSource();
                FadeBGM(bgmData, fadeTime, _bgmFadeCts.Token).Forget();
            }
            else
                PlayBGMInternal(bgmData);
        }

        private void PlayBGMInternal(BGMAudioData bgmData)
        {
            _bgmSource.clip = bgmData.clip;
            _bgmSource.volume = bgmData.volume * _bgmVolume * _masterVolume;
            _bgmSource.pitch = bgmData.pitch;
            _bgmSource.loop = true;
            _bgmSource.Play();

            _currentBGMType = bgmData.bgmType;
            _isBGMPaused = false;

            CusLog.Log("AudioManager", $"BGM '{bgmData.bgmType}' を再生しました");
        }

        private async UniTask FadeBGM(BGMAudioData newBGM, float fadeTime, CancellationToken ct)
        {
            float startVolume = _bgmSource.volume;

            float elapsed = 0f;
            while (elapsed < fadeTime / 2f)
            {
                elapsed += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (fadeTime / 2f));
                await UniTask.Yield(ct);
            }

            _bgmSource.clip = newBGM.clip;
            _bgmSource.pitch = newBGM.pitch;
            _bgmSource.Play();
            _currentBGMType = newBGM.bgmType;

            elapsed = 0f;
            float targetVolume = newBGM.volume * _bgmVolume * _masterVolume;
            while (elapsed < fadeTime / 2f)
            {
                elapsed += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (fadeTime / 2f));
                await UniTask.Yield(ct);
            }

            _bgmSource.volume = targetVolume;

            CusLog.Log("AudioManager", $"BGM '{newBGM.bgmType}' をフェードイン再生しました");
        }

        /// <summary>
        /// BGMを停止
        /// </summary>
        public void StopBGM(float fadeTime = 0f)
        {
            if (!_bgmSource.isPlaying)
                return;

            if (fadeTime > 0f)
            {
                _bgmFadeCts?.Cancel();
                _bgmFadeCts = new CancellationTokenSource();
                FadeOutBGM(fadeTime, _bgmFadeCts.Token).Forget();
            }
            else
            {
                _bgmSource.Stop();
                _currentBGMType = null;
                CusLog.Log("AudioManager", "BGM を停止しました");
            }
        }

        private async UniTask FadeOutBGM(float fadeTime, CancellationToken ct)
        {
            float startVolume = _bgmSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
                await UniTask.Yield(ct);
            }

            _bgmSource.Stop();
            _bgmSource.volume = startVolume;
            _currentBGMType = null;

            CusLog.Log("AudioManager", "BGM をフェードアウト停止しました");
        }

        /// <summary>
        /// BGMを一時停止
        /// </summary>
        public void PauseBGM()
        {
            if (_bgmSource.isPlaying)
            {
                _bgmSource.Pause();
                _isBGMPaused = true;
                CusLog.Log("AudioManager", "BGM を一時停止しました");
            }
        }

        /// <summary>
        /// BGMを再開
        /// </summary>
        public void ResumeBGM()
        {
            if (_isBGMPaused)
            {
                _bgmSource.UnPause();
                _isBGMPaused = false;
                CusLog.Log("AudioManager", "BGM を再開しました");
            }
        }

        /// <summary>
        /// BGMが再生中かチェック
        /// </summary>
        public bool IsPlayingBGM() => _bgmSource.isPlaying;

        /// <summary>
        /// 現在のBGMタイプを取得
        /// </summary>
        public BGMType? GetCurrentBGMType() => _currentBGMType;

        #endregion

        #region SE制御

        /// <summary>
        /// SEを再生
        /// </summary>
        public void PlaySE(SEType seType, bool loop = false, float pitch = 1f)
        {
            if (_audioDatabase == null)
            {
                CusLog.Error("AudioManager", "AudioDatabase が null です");
                return;
            }

            SEAudioData seData = _audioDatabase.GetSEData(seType);
            if (seData == null)
            {
                CusLog.Error("AudioManager", $"SE '{seType}' が見つかりません");
                return;
            }

            PlaySEInternal(seData, loop, pitch);
        }

        private void PlaySEInternal(SEAudioData seData, bool loop, float pitch)
        {
            AudioSource seSource = GetAvailableSESource();

            seSource.clip = seData.clip;
            seSource.volume = seData.volume * _seVolume * _masterVolume;
            seSource.pitch = pitch != 1f ? pitch : seData.pitch;
            seSource.loop = loop || seData.loop;
            seSource.spatialBlend = seData.spatialBlend;
            seSource.Play();

            CusLog.Log("AudioManager", $"SE '{seData.seType}' を再生しました");
        }

        /// <summary>
        /// 3D位置指定でSEを再生
        /// </summary>
        public void PlaySE(SEType seType, Vector3 position, float pitch = 1f)
        {
            if (_audioDatabase == null)
                return;

            SEAudioData seData = _audioDatabase.GetSEData(seType);
            if (seData == null)
                return;

            GameObject tempGO = new($"TempSE_{seType}");
            tempGO.transform.position = position;
            AudioSource tempSource = tempGO.AddComponent<AudioSource>();

            tempSource.clip = seData.clip;
            tempSource.volume = seData.volume * _seVolume * _masterVolume;
            tempSource.pitch = pitch != 1f ? pitch : seData.pitch;
            tempSource.spatialBlend = 1f;
            tempSource.Play();

            Destroy(tempGO, seData.clip.length);

            CusLog.Log("AudioManager", $"SE '{seData.seType}' を3D再生しました");
        }

        private AudioSource GetAvailableSESource()
        {
            for (int i = 0; i < _seSources.Count; i++)
            {
                if (!_seSources[i].isPlaying)
                    return _seSources[i];
            }

            _seSourceIndex = (_seSourceIndex + 1) % _seSources.Count;
            return _seSources[_seSourceIndex];
        }

        /// <summary>
        /// 全てのSEを停止
        /// </summary>
        public void StopAllSE()
        {
            foreach (var seSource in _seSources)
                seSource.Stop();
        }

        /// <summary>
        /// 全てのSEを一時停止
        /// </summary>
        public void PauseAllSE()
        {
            _pausedSESources.Clear();
            foreach (var seSource in _seSources)
            {
                if (seSource.isPlaying)
                {
                    seSource.Pause();
                    _pausedSESources.Add(seSource);
                }
            }
            CusLog.Log("AudioManager", "全てのSEを一時停止しました");
        }

        /// <summary>
        /// 全てのSEを再開
        /// </summary>
        public void ResumeAllSE()
        {
            foreach (var seSource in _pausedSESources)
                seSource.UnPause();
            _pausedSESources.Clear();
            CusLog.Log("AudioManager", "全てのSEを再開しました");
        }

        #endregion

        #region ボリューム制御

        /// <summary>
        /// マスターボリュームを設定
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
            CusLog.Log("AudioManager", $"マスターボリューム: {_masterVolume:F2}");
        }

        /// <summary>
        /// BGMボリュームを設定
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            UpdateBGMVolume();
            CusLog.Log("AudioManager", $"BGMボリューム: {_bgmVolume:F2}");
        }

        /// <summary>
        /// SEボリュームを設定
        /// </summary>
        public void SetSEVolume(float volume)
        {
            _seVolume = Mathf.Clamp01(volume);
            CusLog.Log("AudioManager", $"SEボリューム: {_seVolume:F2}");
        }

        private void UpdateAllVolumes()
        {
            UpdateBGMVolume();
        }

        private void UpdateBGMVolume()
        {
            if (_bgmSource == null || _bgmSource.clip == null)
                return;

            if (_currentBGMType == null)
                return;

            BGMAudioData bgmData = _audioDatabase.GetBGMData(_currentBGMType.Value);
            if (bgmData != null)
                _bgmSource.volume = bgmData.volume * _bgmVolume * _masterVolume;
        }

        /// <summary>
        /// マスターボリュームを取得
        /// </summary>
        public float GetMasterVolume() => _masterVolume;

        /// <summary>
        /// BGMボリュームを取得
        /// </summary>
        public float GetBGMVolume() => _bgmVolume;

        /// <summary>
        /// SEボリュームを取得
        /// </summary>
        public float GetSEVolume() => _seVolume;

        #endregion

        #region ユーティリティ

        /// <summary>
        /// 全ての音を停止
        /// </summary>
        public void StopAll()
        {
            StopBGM();
            StopAllSE();
        }

        /// <summary>
        /// 全ての音を一時停止
        /// </summary>
        public void PauseAll()
        {
            PauseBGM();
            PauseAllSE();
            CusLog.Log("AudioManager", "全ての音を一時停止しました");
        }

        /// <summary>
        /// 全ての音を再開
        /// </summary>
        public void ResumeAll()
        {
            ResumeBGM();
            ResumeAllSE();
            CusLog.Log("AudioManager", "全ての音を再開しました");
        }

        #endregion

        protected override void OnRelease()
        {
            _bgmFadeCts?.Cancel();
            _bgmFadeCts?.Dispose();
            StopAll();
        }
    }
}
