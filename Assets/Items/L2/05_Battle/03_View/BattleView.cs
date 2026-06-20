using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.L2.Battle.View
{
    public class BattleView : MonoBehaviour
    {
        [SerializeField] private RectTransform _playerArea;
        [SerializeField] private RectTransform _botArea;
        [SerializeField] private TMP_Text _turnText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _endBtn;
        [SerializeField] private DamageTextManager _dmgText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TMP_Text _resultTitle;
        [SerializeField] private RectTransform _rewardContainer;
        [SerializeField] private Button _restartBtn;
        [SerializeField] private CardView _cardViewPrefab;
        [SerializeField] private RewardCardView _rewardCardPrefab;
        [SerializeField] private CardInfoView _userDesc;
        [SerializeField] private CardInfoView _botDesc;

        private BattleManager _bm;
        private CardPlayerBase _me;
        private CardPlayerBase _bot;

        private readonly Dictionary<CardBase, CardView> _views = new Dictionary<CardBase, CardView>();
        private readonly Queue<BattleStep> _queue = new Queue<BattleStep>();
        private bool _animating;
        private bool _pendingEnd;
        private bool _pendingWon;

        private readonly Vector2[] _meSlots = new Vector2[3];
        private readonly Vector2[] _botSlots = new Vector2[3];

        private void Start()
        {
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 300f;
                _botSlots[i] = new Vector2(x, 0f);
                _meSlots[i] = new Vector2(x, 0f);
            }

            _bm = BattleManager.Instance;
            if (_bm == null) { Debug.LogError("[BattleView] BattleManager.Instance 없음"); return; }

            _endBtn.onClick.AddListener(OnEndTurn);
            _restartBtn.onClick.AddListener(() => { _resultPanel.SetActive(false); _bm.RestartRun(); });
            _resultPanel.SetActive(false);

            _me = new GameObject("Me").AddComponent<CardPlayerBase>();
            _bot = new GameObject("Bot").AddComponent<CardPlayerBase>();

            _bm.OnBattleStarted += BuildBoard;
            _bm.OnResolved += OnResolved;
            if (_bm.EventBus != null)
                _bm.EventBus.ConnectEvent<BattleEndedEvent>(OnBattleEnded);

            _bm.StartRun(_me, _bot);
        }

        // ---------- 보드 ----------
        private void BuildBoard()
        {
            foreach (var kv in _views) if (kv.Value != null) Destroy(kv.Value.gameObject);
            _views.Clear();

            for (int i = 0; i < 3; i++)
            {
                CreateCardView(_me.FieldAt(i), _meSlots, true);
                CreateCardView(_bot.FieldAt(i), _botSlots, false);
            }
            _queue.Clear();
            _animating = false;
            _pendingEnd = false;
            _resultPanel.SetActive(false);
            ResetTurnUI();
        }

        private CardView CreateCardView(CardBase model, Vector2[] slots, bool mine)
        {
            if (model == null) return null;
            var area = mine ? _playerArea : _botArea;
            var v = Instantiate(_cardViewPrefab, area);
            v.Init(area);
            v.SetHome(slots[model.SlotIndex]);
            v.Bind(model);
            if (mine)
            {
                v.CanDrag = CanDrag;
                v.OnAttackDrop = OnAttackDrop;
                v.OnDragBegin = OnDragBegin;
                v.OnDragHover = OnDragHover;
                v.OnDragEnd = OnDragEnd;
            }
            _views[model] = v;
            return v;
        }

        private CardView ViewOf(CardBase c) { return (c != null && _views.TryGetValue(c, out var v)) ? v : null; }

        private void RemoveView(CardBase c)
        {
            if (c != null && _views.TryGetValue(c, out var v)) { if (v != null) Destroy(v.gameObject); _views.Remove(c); }
        }

        // ---------- 입력 ----------
        private bool CanDrag(CardView v)
        {
            return _bm.State == BattleState.Game_MyTurn
                   && v.Model != null && v.Model.Owner == _me && !v.Model.IsDead
                   && _bm.Cost >= v.Model.Cost;
        }

        private void OnAttackDrop(CardView from, CardView target)
        {
            if (_bm.State != BattleState.Game_MyTurn) return;
            if (!IsValidTarget(from, target)) return;

            if (_bm.TryInteract(from.Model, target.Model))
                UpdateCost();
        }

        private void OnDragBegin(CardView src)
        {
            _userDesc.Show(src.Model);
        }

        private void OnDragHover(CardView src, CardView hovered)
        {
            if (hovered != null && IsValidTarget(src, hovered))
                _botDesc.Show(hovered.Model);
            else
                _botDesc.Clear();
        }

        private void OnDragEnd()
        {
            _userDesc.Clear();
            _botDesc.Clear();
        }

        private bool IsValidTarget(CardView src, CardView target)
        {
            if (target == null || target == src || target.Model == null || target.Model.IsDead) return false;
            var type = src.Model.InteractType;
            if ((type & CardInteractType.EnemyCard) != 0 && target.Model.Owner == _bot) return true;
            if ((type & CardInteractType.AllyCard) != 0 && target.Model.Owner == _me) return true;
            return false;
        }

        private void OnEndTurn()
        {
            if (_animating || _bm.State != BattleState.Game_MyTurn) return;
            _turnText.text = "상대 턴...";
            _endBtn.interactable = false;
            _bm.EndTurn();
            if (!_animating) FinishTurnCycle();
        }

        // ---------- 연출 ----------
        private void OnResolved(BattleStep step)
        {
            _queue.Enqueue(step);
            if (!_animating) { _animating = true; StartCoroutine(Drain()); }
        }

        private IEnumerator Drain()
        {
            _endBtn.interactable = false;
            while (_queue.Count > 0)
            {
                var step = _queue.Dequeue();
                yield return AnimateInteraction(step.Interaction);
                yield return AnimateSpawns(step.Spawned);
            }
            _animating = false;
            FinishTurnCycle();
        }

        private IEnumerator AnimateInteraction(InteractResult ir)
        {
            if (ir == null || ir.Attacker == null) yield break;
            var atkV = ViewOf(ir.Attacker);
            var tgtV = ViewOf(ir.Target);
            if (atkV != null)
            {
                atkV.SetGlow(true);
                if (tgtV != null) yield return Wait(atkV.Lunge(tgtV.WorldCenter));
            }

            foreach (var hit in ir.Hits)
            {
                var v = ViewOf(hit.Card);
                if (v == null) continue;
                _dmgText.Pop(hit.Amount, v.WorldCenter, false);
                v.Hit();
                yield return Wait(v.SetHp(hit.HpAfter));
                if (hit.Died) { yield return Wait(v.Die()); RemoveView(hit.Card); }
            }

            var still = ViewOf(ir.Attacker);
            if (still != null) still.SetGlow(false);
        }

        private IEnumerator AnimateSpawns(List<CardBase> spawned)
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                var c = spawned[i];
                bool mine = c.Owner == _me;
                var v = CreateCardView(c, mine ? _meSlots : _botSlots, mine);
                if (v != null) yield return Wait(v.Born());
            }
        }

        private IEnumerator Wait(Tween t)
        {
            if (t == null) yield break;
            while (t.IsActive() && !t.IsComplete()) yield return null;
        }

        // ---------- 턴/결과 ----------
        private void FinishTurnCycle()
        {
            RefreshAllHp();
            if (_pendingEnd) { _pendingEnd = false; ShowResult(_pendingWon); return; }
            ResetTurnUI();
        }

        private void ResetTurnUI()
        {
            _turnText.text = "내 턴   |   Stage " + (_bm.Stage + 1);
            _endBtn.interactable = _bm.State == BattleState.Game_MyTurn;
            UpdateCost();
        }

        private void UpdateCost() { _costText.text = "Cost " + _bm.Cost + " / " + _bm.CostPerTurn; }

        private void RefreshAllHp() { foreach (var kv in _views) if (kv.Value != null && kv.Key != null && !kv.Key.IsDead) kv.Value.RefreshHp(); }

        private void OnBattleEnded(BattleEndedEvent e) { _pendingEnd = true; _pendingWon = e.PlayerWon; }

        private void ShowResult(bool won)
        {
            _resultPanel.SetActive(true);
            _resultPanel.transform.SetAsLastSibling();
            _resultTitle.text = won ? "승리!" : "패배";

            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
                Destroy(_rewardContainer.GetChild(i).gameObject);

            _rewardContainer.gameObject.SetActive(won);
            _restartBtn.gameObject.SetActive(!won);

            if (!won) return;

            CardType[] pool = { CardType.Normal, CardType.Ranged, CardType.Mussang, CardType.Healer, CardType.Guard };
            for (int i = 0; i < 3; i++)
            {
                var type = pool[Random.Range(0, pool.Length)];
                var card = Instantiate(_rewardCardPrefab, _rewardContainer);
                card.Set(_bm.GetCardData(type), () => { _resultPanel.SetActive(false); _bm.PickRewardAndContinue(type); });
            }
        }
    }
}
