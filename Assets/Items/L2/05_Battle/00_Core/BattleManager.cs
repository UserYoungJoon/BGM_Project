using System.Collections.Generic;
using UnityEngine;
using YoungJoon.L0.Events;
using YoungJoon.L1.App;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.L2.Battle
{
    public class BattleManager : MonoBehaviour, ISceneManager
    {
        public static BattleManager Instance { get; private set; }
        public GameObjectEventBus EventBus { get; private set; }

        public string SceneName => SceneNames.BATTLE;

        [SerializeField] private CardRegistry _registry;
        [SerializeField] private int _costPerTurn = 3;
        [SerializeField] private CardPlayerBase _me;
        [SerializeField] private CardPlayerBase _bot;

        private int _stage;
        private int _cost;
        private BattleState _state = BattleState.None;
        private BattleStep _current;

        public BattleState State => _state;
        public CardPlayerBase LocalPlayer => _me;
        public CardPlayerBase BotPlayer => _bot;
        public int Cost => _cost;
        public int CostPerTurn => _costPerTurn;
        public int Stage => _stage;

        public System.Action OnBattleStarted;
        public System.Action<BattleStep> OnResolved;

        public CardDataSO GetCardData(CardType type) => _registry.GetCardData(type);

        private void Awake()
        {
            Instance = this;
            EventBus = gameObject.AddComponent<GameObjectEventBus>();
            if (GameManager.Instance != null)
                GameManager.Instance.Register(this);
        }

        public void OnEnterScene(SceneContext context) { }
        public void OnExitScene() { }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Unregister(this);
            if (Instance == this)
                Instance = null;
        }

        public void StartRun()
        {
            _stage = 0;
            _me.SetDeck(MakeStartingDeck());
            StartGame();
        }

        private void StartGame()
        {
            _bot.SetDeck(MakeBotDeck(_stage));
            _registry.BuildAllCaches();
            _me.InitBattle(_registry);
            _bot.InitBattle(_registry);
            SetState(BattleState.Game_MyTurn);
            BeginTurn(_me);
            OnBattleStarted?.Invoke();
        }

        private void BeginTurn(CardPlayerBase player)
        {
            _cost = _costPerTurn;
            _current = new BattleStep();
            foreach (var card in player.AliveFieldCards())
                card.OnTurnStart();
            if (_current.Heals.Count > 0) OnResolved?.Invoke(_current);
            _current = null;
        }

        public bool TryInteract(CardBase attacker, CardBase target)
        {
            if (_state != BattleState.Game_MyTurn) return false;
            if (attacker == null || attacker.Owner != _me || attacker.IsDead) return false;
            if (_cost < attacker.Cost) return false;

            _cost -= attacker.Cost;
            ResolveInteraction(attacker, target);
            return true;
        }

        public void EndTurn()
        {
            if (_state != BattleState.Game_MyTurn) return;
            SetState(BattleState.Game_BotTurn);
            RunBotTurn();
        }

        private void RunBotTurn()
        {
            BeginTurn(_bot);

            int guard = 100;
            while (_state == BattleState.Game_BotTurn && guard-- > 0)
            {
                var attacker = AffordableBotAttacker();
                if (attacker == null) break;
                var target = RandomAliveCard(_me);
                if (target == null) break;

                _cost -= attacker.Cost;
                ResolveInteraction(attacker, target);
            }

            if (_state == BattleState.Game_BotTurn)
            {
                SetState(BattleState.Game_MyTurn);
                BeginTurn(_me);
            }
        }

        private void ResolveInteraction(CardBase attacker, CardBase target)
        {
            if (attacker == null || attacker.IsDead || target == null || target.IsDead) return;

            _current = new BattleStep();
            _current.Attacker = attacker;
            _current.Target = target;
            attacker.InteractWith(target);
            _current.Spawned.AddRange(_me.CleanupAndRefill());
            _current.Spawned.AddRange(_bot.CleanupAndRefill());
            OnResolved?.Invoke(_current);
            _current = null;

            CheckEnd();
        }

        public void Deal(CardBase from, CardBase victim, int amount)
        {
            victim.AttackedBy(new AttackSource(from, amount));
            _current.Hits.Add(new DamageResult { Card = victim, Amount = amount, HpAfter = victim.CurrentHp, Died = victim.IsDead });
        }

        public void Block(CardBase from, CardBase target, int amount)
        {
            target.AddBlock(amount);
            _current.Blocks.Add(new BlockResult { Card = target, Amount = amount });
        }

        public void Heal(CardBase from, CardBase target, int amount)
        {
            int before = target.CurrentHp;
            target.HealedBy(new HealSource(from, amount));
            if (target.CurrentHp > before)
                _current.Heals.Add(new HealResult { Card = target, Amount = target.CurrentHp - before, HpAfter = target.CurrentHp });
        }

        private void CheckEnd()
        {
            if (_bot.IsDefeated) EndBattle(true);
            else if (_me.IsDefeated) EndBattle(false);
        }

        private void EndBattle(bool playerWon)
        {
            SetState(BattleState.End);
            EventBus.SendEvent(new BattleEndedEvent { PlayerWon = playerWon, Stage = _stage });
        }

        public void PickRewardAndContinue(CardType reward)
        {
            if (_state != BattleState.End) return;
            _me.AddCard(reward);
            _stage++;
            StartGame();
        }

        public void RestartRun()
        {
            _stage = 0;
            _me.SetDeck(MakeStartingDeck());
            StartGame();
        }

        private void SetState(BattleState next)
        {
            _state = next;
            EventBus.SendEvent(new TurnChangedEvent { State = next });
        }

        private CardBase AffordableBotAttacker()
        {
            foreach (var card in _bot.AliveFieldCards())
                if (card.Cost <= _cost)
                    return card;
            return null;
        }

        private CardBase RandomAliveCard(CardPlayerBase player)
        {
            var alive = new List<CardBase>();
            foreach (var card in player.AliveFieldCards())
                alive.Add(card);
            if (alive.Count == 0) return null;
            return alive[Random.Range(0, alive.Count)];
        }

        private static List<CardType> MakeStartingDeck()
        {
            return new List<CardType>
            {
                CardType.Normal, CardType.Guard, CardType.Ranged,
                CardType.Mussang, CardType.Healer, CardType.Normal
            };
        }

        private static List<CardType> MakeBotDeck(int stage)
        {
            var deck = new List<CardType>();
            int count = 6 + stage;
            CardType[] pool = { CardType.Normal, CardType.Ranged, CardType.Mussang, CardType.Healer };
            for (int i = 0; i < count; i++)
                deck.Add(pool[i % pool.Length]);
            return deck;
        }
    }
}
