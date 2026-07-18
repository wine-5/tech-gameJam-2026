using System;
using UnityEngine;

namespace TechC.Core.Pause
{
    /// <summary>
    /// Rigidbody2D の物理シミュレーションを停止/再開する凍結部品。
    /// </summary>
    [Serializable]
    public class RigidbodyFreezable : IPauseFreezable
    {
        [SerializeField] private Rigidbody2D _body;

        public void OnFreeze()
        {
            if (_body != null)
                _body.simulated = false;
        }

        public void OnUnfreeze()
        {
            if (_body != null)
                _body.simulated = true;
        }
    }
}
