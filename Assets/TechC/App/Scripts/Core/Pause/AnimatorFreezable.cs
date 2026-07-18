using System;
using UnityEngine;

namespace TechC.Core.Pause
{
    /// <summary>
    /// Animator の再生を停止/再開する凍結部品。
    /// </summary>
    [Serializable]
    public class AnimatorFreezable : IPauseFreezable
    {
        [SerializeField] private Animator _animator;

        public void OnFreeze()
        {
            if (_animator != null)
                _animator.speed = 0f;
        }

        public void OnUnfreeze()
        {
            if (_animator != null)
                _animator.speed = 1f;
        }
    }
}
