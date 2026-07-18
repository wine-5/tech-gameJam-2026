using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectName.UI.Effects
{
    /// <summary>
    /// 登録された UIEffectBase をトリガー種別ごとにまとめて再生するコンポーネント。
    /// PauseFreezer と同様、インスペクターで必要なエフェクトだけを追加して組み合わせる。
    /// これ1つで「常時パルス＋ホバー拡大＋点滅」のような複合エフェクトを実現できる
    /// </summary>
    public class UIEffectController : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        IPointerClickHandler
    {
        [SerializeReference] private List<UIEffectBase> _effects = new();

        private UIEffectContext _context;

        // トリガー種別ごとに実行中タスクのCTSを持つ（同トリガーの再発火時は前のを打ち切る）
        private readonly Dictionary<UIEffectTrigger, CancellationTokenSource> _ctsByTrigger = new();

        private void Awake()
        {
            _context = new UIEffectContext(gameObject);
        }

        private void OnEnable()
        {
            Play(UIEffectTrigger.Always);
        }

        private void OnDisable()
        {
            foreach (var cts in _ctsByTrigger.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _ctsByTrigger.Clear();

            foreach (var effect in _effects)
                effect?.ResetState(_context);
        }

        public void OnPointerEnter(PointerEventData eventData) => Play(UIEffectTrigger.Hover);
        public void OnPointerExit(PointerEventData eventData) => Stop(UIEffectTrigger.Hover);
        public void OnPointerDown(PointerEventData eventData) => Play(UIEffectTrigger.Press);
        public void OnPointerUp(PointerEventData eventData) => Stop(UIEffectTrigger.Press);
        public void OnPointerClick(PointerEventData eventData) => Play(UIEffectTrigger.Click);

        private void Play(UIEffectTrigger trigger)
        {
            CancellationToken token = RenewToken(trigger);
            foreach (var effect in _effects)
            {
                if (effect != null && effect.Trigger == trigger)
                    effect.PlayAsync(_context, token).Forget();
            }
        }

        private void Stop(UIEffectTrigger trigger)
        {
            CancellationToken token = RenewToken(trigger);
            foreach (var effect in _effects)
            {
                if (effect != null && effect.Trigger == trigger)
                    effect.StopAsync(_context, token).Forget();
            }
        }

        /// <summary>
        /// 指定トリガーの実行中タスクを打ち切り、新しいトークンを発行する。
        /// 破棄時にも自動でキャンセルされるよう destroyCancellationToken とリンクする
        /// </summary>
        private CancellationToken RenewToken(UIEffectTrigger trigger)
        {
            if (_ctsByTrigger.TryGetValue(trigger, out var oldCts))
            {
                oldCts.Cancel();
                oldCts.Dispose();
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _ctsByTrigger[trigger] = cts;
            return cts.Token;
        }
    }
}
