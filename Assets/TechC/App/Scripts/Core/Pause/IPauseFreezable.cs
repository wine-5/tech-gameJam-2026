namespace ProjectName.Core.Pause
{
    /// <summary>
    /// ポーズ状態に合わせて凍結する対象の共通契約。
    /// ポーズの際に停止、再開するコンポーネントに継承する。
    /// </summary>
    public interface IPauseFreezable
    {
        /// <summary>ポーズ時：対象を停止する</summary>
        void OnFreeze();

        /// <summary>解除時：対象を再開する</summary>
        void OnUnfreeze();
    }
}
