using System;
using UnityEngine;
using UnityEngine.VFX;

namespace TechC.Core.Pause
{
    /// <summary>
    /// VFX Graph の VisualEffect の再生を停止/再開する凍結部品。
    /// </summary>
    [Serializable]
    public class VisualEffectFreezable : IPauseFreezable
    {
        [SerializeField] private VisualEffect _visualEffect;

        public void OnFreeze()
        {
            if (_visualEffect != null)
                _visualEffect.pause = true;
        }

        public void OnUnfreeze()
        {
            if (_visualEffect != null)
                _visualEffect.pause = false;
        }
    }
}
