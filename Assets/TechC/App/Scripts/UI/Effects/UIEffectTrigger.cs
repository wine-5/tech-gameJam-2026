namespace TechC.UI.Effects
{
    /// <summary>
    /// UIエフェクトをいつ再生するかの指定
    /// </summary>
    public enum UIEffectTrigger
    {
        /// <summary>有効中ずっと再生（常時パルスなど）</summary>
        Always,

        /// <summary>ホバー中だけ再生（離れたら元に戻る）</summary>
        Hover,

        /// <summary>押下中だけ再生</summary>
        Press,

        /// <summary>クリック時に1回再生</summary>
        Click
    }
}
