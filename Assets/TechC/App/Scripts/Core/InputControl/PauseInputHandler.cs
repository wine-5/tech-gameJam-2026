using UnityEngine;
using UnityEngine.InputSystem;
using ProjectName.Core.Manager;
using ProjectName.Core.Log;

namespace ProjectName.Core.InputControl
{
    /// <summary>
    /// Pause入力を受け取り、PauseManager のトグルを呼ぶだけのクラス
    /// 入力検知（ここ）と Pause 状態管理（PauseManager）を分離する
    /// PauseManager と同じく全シーン常駐させる想定
    /// </summary>
    public class PauseInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference pauseAction;

        private void OnEnable()
        {
            if (pauseAction == null)
            {
                CusLog.Warning("PauseInputHandler", "pauseAction が未設定です");
                return;
            }

            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }

        private void OnDisable()
        {
            if (pauseAction == null)
                return;

            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }

        /// <summary>
        /// Pause アクション発火時：PauseManager にトグルを依頼
        /// </summary>
        private void OnPausePerformed(InputAction.CallbackContext ctx)
        {
            if (PauseManager.IsValid())
                PauseManager.I.TogglePause();
        }
    }
}
