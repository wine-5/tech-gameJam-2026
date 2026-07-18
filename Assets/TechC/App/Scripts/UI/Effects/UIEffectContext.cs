using UnityEngine;
using UnityEngine.UI;

namespace TechC.UI.Effects
{
    /// <summary>
    /// エフェクトが操作する対象と初期値をまとめたコンテキスト。
    /// 初期値をここで一元的にキャッシュすることで、複数エフェクトを組み合わせても基準値がズレない。
    /// </summary>
    public class UIEffectContext
    {
        public RectTransform RectTransform { get; }
        public CanvasGroup CanvasGroup { get; }
        public Graphic Graphic { get; }

        /// <summary>起動時点のscale（拡大系エフェクトの基準値）</summary>
        public Vector3 InitialScale { get; }

        /// <summary>起動時点のアルファ（点滅・フェード系エフェクトの基準値）</summary>
        public float InitialAlpha { get; }

        public UIEffectContext(GameObject target)
        {
            RectTransform = target.GetComponent<RectTransform>();
            CanvasGroup = target.GetComponent<CanvasGroup>();
            Graphic = target.GetComponent<Graphic>();

            InitialScale = RectTransform != null ? RectTransform.localScale : Vector3.one;
            InitialAlpha = GetAlpha();
        }

        /// <summary>現在のアルファを取得（CanvasGroup優先、なければGraphic）</summary>
        public float GetAlpha()
        {
            if (CanvasGroup != null) return CanvasGroup.alpha;
            if (Graphic != null) return Graphic.color.a;
            return 1f;
        }

        /// <summary>アルファを設定（CanvasGroup優先、なければGraphic）</summary>
        public void SetAlpha(float alpha)
        {
            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = alpha;
            }
            else if (Graphic != null)
            {
                Color color = Graphic.color;
                color.a = alpha;
                Graphic.color = color;
            }
        }

        /// <summary>初期scaleに対する倍率でscaleを設定</summary>
        public void SetScale(float multiplier)
        {
            if (RectTransform != null)
                RectTransform.localScale = InitialScale * multiplier;
        }
    }
}
