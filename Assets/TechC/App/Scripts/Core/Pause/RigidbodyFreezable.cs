using System;
using UnityEngine;

namespace ProjectName.Core.Pause
{
    /// <summary>
    /// Rigidbody2D の物理シミュレーションを停止/再開する凍結部品。
    /// </summary>
    [Serializable]
    public class RigidbodyFreezable : IPauseFreezable
    {
        [SerializeField] private Rigidbody2D body;

        public void OnFreeze()
        {
            if (body != null)
                body.simulated = false;
        }

        public void OnUnfreeze()
        {
            if (body != null)
                body.simulated = true;
        }
    }
}
