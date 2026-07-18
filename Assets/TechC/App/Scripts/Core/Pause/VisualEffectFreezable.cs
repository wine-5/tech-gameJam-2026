using System;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectName.Core.Pause
{
    /// <summary>
    /// VFX Graph の VisualEffect の再生を停止/再開する凍結部品。
    /// </summary>
    [Serializable]
    public class VisualEffectFreezable : IPauseFreezable
    {
        [SerializeField] private VisualEffect visualEffect;

        public void OnFreeze()
        {
            if (visualEffect != null)
                visualEffect.pause = true;
        }

        public void OnUnfreeze()
        {
            if (visualEffect != null)
                visualEffect.pause = false;
        }
    }
}
