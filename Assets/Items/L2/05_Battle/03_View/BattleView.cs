using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YoungJoon.L1.Sound;
using YoungJoon.L1.UI;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.L2.Battle.View
{
    public class BattleView : MonoBehaviour
    {
        [SerializeField] private RectTransform _playerArea;
        [SerializeField] private RectTransform _botArea;
        [SerializeField] private TextMeshProUGUI _turnText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Button _endBtn;
        [SerializeField] private DamageTextManager _dmgText;
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private TextMeshProUGUI _resultTitle;
        [SerializeField] private RectTransform _rewardContainer;
        [SerializeField] private Button _restartBtn;
        [SerializeField] private CardView _cardViewPrefab;
        [SerializeField] private CardInfoView _userDesc;
        [SerializeField] private CardInfoView _botDesc;
        [SerializeField] private TargetingArrow _arrow;
        [SerializeField] private TabGroup _rewardTabGroup;
        [SerializeField] private CardInfoView _resultDesc;
        [SerializeField] private Button _nextBtn;

        private CardPlayerBase _me;
        private CardPlayerBase _bot;
        private CardView _draggingCard;
        private CardView _glowingTarget;
        private CardType[] _rewardTypes;
        private CardView[] _rewardViews;
        private Shaker _playerShaker;
        private Shaker _botShaker;
        private CanvasGroup _resultGroup;

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

            if (BattleManager.Instance == null) { Debug.LogError("[BattleView] BattleManager.Instance 없음"); return; }

            _playerShaker = _playerArea.GetComponent<Shaker>();
            _botShaker = _botArea.GetComponent<Shaker>();
            _resultGroup = _resultPanel.GetComponent<CanvasGroup>();
            if (_resultGroup == null) _resultGroup = _resultPanel.AddComponent<CanvasGroup>();

            _endBtn.onClick.AddListener(OnEndTurn);
            _restartBtn.onClick.AddListener(() => { _resultPanel.SetActive(false); BattleManager.Instance.RestartRun(); });
            _nextBtn.onClick.AddListener(OnNextReward);
            _rewardTabGroup.EventBus.ConnectEvent<OnTabChanged>(OnRewardTabChanged);
            _resultPanel.SetActive(false);

            _me = new GameObject("Me").AddComponent<CardPlayerBase>();
            _bot = new GameObject("Bot").AddComponent<CardPlayerBase>();

            BattleManager.Instance.OnBattleStarted += BuildBoard;
            BattleManager.Instance.OnResolved += OnResolved;
            if (BattleManager.Instance.EventBus != null)
            {
                BattleManager.Instance.EventBus.ConnectEvent<BattleEndedEvent>(OnBattleEnded);
                BattleManager.Instance.EventBus.ConnectEvent<TurnChangedEvent>(OnTurnChanged);
            }

            BattleManager.Instance.StartRun(_me, _bot);
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
            SoundManager.Instance.Play(SoundKey.CardPlace);
            _queue.Clear();
            _animating = false;
            _pendingEnd = false;
            ResetTurnUI();
        }

        private CardView CreateCardView(CardBase model, Vector2[] slots, bool mine)
        {
            if (model == null) return null;
            var area = mine ? _playerArea : _botArea;
            var v = Instantiate(_cardViewPrefab, area);
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
            return BattleManager.Instance.State == BattleState.Game_MyTurn
                   && v.Model != null && v.Model.Owner == _me && !v.Model.IsDead
                   && BattleManager.Instance.Cost >= v.Model.Cost;
        }

        private void OnAttackDrop(CardView from, CardView target)
        {
            if (BattleManager.Instance.State != BattleState.Game_MyTurn) return;
            if (!IsValidTarget(from, target)) return;

            if (BattleManager.Instance.TryInteract(from.Model, target.Model))
                UpdateCost();
        }

        private void OnDragBegin(CardView src)
        {
            _draggingCard = src;
            src.SetGlow(true);
            _userDesc.Show(src.Model);
            _arrow.Begin(src.Rect);
        }

        private void OnDragHover(CardView src, CardView hovered, Vector2 screen)
        {
            bool valid = hovered != null && IsValidTarget(src, hovered);
            var newTarget = valid ? hovered : null;
            if (_glowingTarget != newTarget)
            {
                if (_glowingTarget != null) _glowingTarget.SetGlow(false);
                if (newTarget != null) newTarget.SetGlow(true);
                _glowingTarget = newTarget;
            }

            if (valid) _botDesc.Show(hovered.Model); else _botDesc.Clear();

            var kind = !valid ? TargetingArrow.TargetKind.None
                     : hovered.Model.Owner == _bot ? TargetingArrow.TargetKind.Enemy
                     : TargetingArrow.TargetKind.Ally;
            _arrow.Aim(screen, kind);
        }

        private void OnDragEnd()
        {
            if (_draggingCard != null) _draggingCard.SetGlow(false);
            if (_glowingTarget != null) _glowingTarget.SetGlow(false);
            _draggingCard = null;
            _glowingTarget = null;
            _arrow.Hide();
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
            if (_animating || BattleManager.Instance.State != BattleState.Game_MyTurn) return;
            SoundManager.Instance.Play(SoundKey.Click);
            _turnText.text = "상대 턴...";
            _endBtn.interactable = false;
            BattleManager.Instance.EndTurn();
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
            if (atkV != null && tgtV != null)
                yield return Wait(atkV.Lunge(tgtV.WorldCenter));

            foreach (var hit in ir.Hits)
            {
                var v = ViewOf(hit.Card);
                if (v == null) continue;
                SoundManager.Instance.Play(hit.Card == ir.Attacker ? SoundKey.Counter : SoundKey.Hit);
                _dmgText.Pop(hit.Amount, v.WorldCenter, false);
                v.Hit();
                _playerShaker.Shake();
                _botShaker.Shake();
                yield return Wait(v.SetHp(hit.HpAfter));
                if (hit.Died) { SoundManager.Instance.Play(SoundKey.Die); yield return Wait(v.Die()); RemoveView(hit.Card); }
            }
        }

        private IEnumerator AnimateSpawns(List<CardBase> spawned)
        {
            for (int i = 0; i < spawned.Count; i++)
            {
                var c = spawned[i];
                bool mine = c.Owner == _me;
                var v = CreateCardView(c, mine ? _meSlots : _botSlots, mine);
                if (v != null)
                {
                    SoundManager.Instance.Play(SoundKey.CardRespawn);
                    yield return Wait(v.Born());
                }
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
            _turnText.text = "내 턴   |   Stage " + (BattleManager.Instance.Stage + 1);
            _endBtn.interactable = BattleManager.Instance.State == BattleState.Game_MyTurn;
            UpdateCost();
        }

        private void UpdateCost() { _costText.text = "Cost " + BattleManager.Instance.Cost + " / " + BattleManager.Instance.CostPerTurn; }

        private void RefreshAllHp() { foreach (var kv in _views) if (kv.Value != null && kv.Key != null && !kv.Key.IsDead) kv.Value.RefreshHp(); }

        private void OnBattleEnded(BattleEndedEvent e) { _pendingEnd = true; _pendingWon = e.PlayerWon; }

        private void OnTurnChanged(TurnChangedEvent e)
        {
            if (e.State == BattleState.Game_MyTurn || e.State == BattleState.Game_BotTurn)
                SoundManager.Instance.Play(SoundKey.TurnChanged);
        }

        // 승/패 모두 보상 3장(탭 단일선택) + desc + '다음으로' 버튼.
        private void ShowResult(bool won)
        {
            SoundManager.Instance.Play(won ? SoundKey.Win : SoundKey.Lose);
            _resultPanel.SetActive(true);
            _resultPanel.transform.SetAsLastSibling();
            _resultGroup.DOKill();
            _resultGroup.alpha = 0f;
            _resultGroup.DOFade(1f, 0.25f);
            _resultTitle.text = won ? "승리!" : "패배";
            _restartBtn.gameObject.SetActive(false);
            BuildRewards();
        }

        private void HideResultPanel()
        {
            _resultGroup.DOKill();
            _resultGroup.DOFade(0f, 0.25f).OnComplete(() => _resultPanel.SetActive(false));
        }

        private void BuildRewards()
        {
            for (int i = _rewardContainer.childCount - 1; i >= 0; i--)
                Destroy(_rewardContainer.GetChild(i).gameObject);

            SoundManager.Instance.Play(SoundKey.RewardAppear);

            CardType[] pool = { CardType.Normal, CardType.Ranged, CardType.Mussang, CardType.Healer, CardType.Guard };
            _rewardTypes = new CardType[3];
            _rewardViews = new CardView[3];
            var tabs = new TabButton[3];
            for (int i = 0; i < 3; i++)
            {
                var type = pool[Random.Range(0, pool.Length)];
                _rewardTypes[i] = type;
                var v = Instantiate(_cardViewPrefab, _rewardContainer);
                v.Set(BattleManager.Instance.GetCardData(type));
                v.SetGlow(false);
                _rewardViews[i] = v;
                tabs[i] = v.gameObject.AddComponent<TabButton>();
            }
            _rewardTabGroup.SetTabs(tabs);
            _rewardTabGroup.Init();   // 첫 번째 선택 → OnTabChanged 발행
        }

        private void OnRewardTabChanged(OnTabChanged e)
        {
            if (_rewardViews == null) return;
            int sel = e.CurrTabButton.TabIndex;
            for (int i = 0; i < _rewardViews.Length; i++)
                if (_rewardViews[i] != null) _rewardViews[i].SetGlow(i == sel);

            var model = CardFactory.Create(_rewardTypes[sel]);
            model.Spawn(BattleManager.Instance.GetCardData(_rewardTypes[sel]));
            _resultDesc.Show(model);
        }

        private void OnNextReward()
        {
            var cur = _rewardTabGroup.CurrentSelectedTab;
            if (cur == null) return;
            SoundManager.Instance.Play(SoundKey.Click);
            var type = _rewardTypes[cur.TabIndex];
            BattleManager.Instance.PickRewardAndContinue(type);   // 새 보드는 불투명 패널 뒤에서 빌드
            HideResultPanel();                                    // 페이드아웃하며 새 보드 드러남
        }
    }
}
