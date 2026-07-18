using System.Collections.Generic;
using UnityEngine;

namespace TechC.Core.Pause
{
    using TechC;

    /// <summary>
    /// ポーズ状態に合わせて、登録された IPauseFreezable をまとめて停止・再開するコンポーネント。
    /// インスペクターで必要な凍結対象（Rigidbody / Animator / Tween など）だけを追加する。
    /// これを付けるだけで、各クラスに個別のポーズ購読・凍結処理を書かずに済む。
    /// </summary>
    public class PauseFreezer : MonoBehaviour
    {
        [SerializeReference] private List<IPauseFreezable> _freezables = new();

        private void OnEnable()
        {
            if (!PauseManager.IsValid())
                return;

            PauseManager.I.OnPaused += Freeze;
            PauseManager.I.OnResumed += Unfreeze;

            if (PauseManager.I.IsPaused)
                Freeze();
            else
                Unfreeze();
        }

        private void OnDisable()
        {
            if (!PauseManager.IsValid())
                return;

            PauseManager.I.OnPaused -= Freeze;
            PauseManager.I.OnResumed -= Unfreeze;
        }

        private void Freeze()
        {
            foreach (var freezable in _freezables)
                freezable.OnFreeze();
        }

        private void Unfreeze()
        {
            foreach (var freezable in _freezables)
                freezable.OnUnfreeze();
        }
    }
}
