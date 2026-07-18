using UnityEngine;
using TechC.Audio;

namespace TechC.InGame
{
    /// <summary>
    /// InGameシーンの最上位モジュール。
    /// シーン全体の初期化・進行はここを起点に書く
    /// </summary>
    public class InGameController : MonoBehaviour
    {
        private void Start()
        {
            // AudioDataSOにInGame用BGMを追加したらコメントを解除する
            // AudioManager.I.PlayBGM(BGMType.InGame);
        }
    }
}
