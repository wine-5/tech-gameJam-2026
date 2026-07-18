using UnityEngine;
using TechC.Audio;

namespace TechC.Result
{
    /// <summary>
    /// Resultシーンの最上位モジュール。
    /// シーン全体の初期化・進行はここを起点に書く
    /// </summary>
    public class ResultController : MonoBehaviour
    {
        private void Start()
        {
            // AudioDataSOにResult用BGMを追加したらコメントを解除する
            // AudioManager.I.PlayBGM(BGMType.Result);
        }
    }
}
