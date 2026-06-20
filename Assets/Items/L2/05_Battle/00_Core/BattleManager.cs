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

        private CardPlayerBase _me;
        private CardPlayerBase _bot;
        private int _stage;
        private int _cost;
        private BattleState _state = BattleState.None;

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

        public void StartRun(CardPlayerBase me, CardPlayerBase bot)
        {
            _me = me;
            _bot = bot;
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
            foreach (var card in player.AliveFieldCards())
                card.OnTurnStart();
        }

        public bool TryInteract(CardBase attacker, CardBase target)
        {
            if (_state != BattleState.Game_MyTurn) return false;
            if (attacker == null || attacker.Owner != _me || attacker.IsDead) return false;
            if (_cost < attacker.Cost) return false;

            _cost -= attacker.Cost;
            Resolve(attacker, target);
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
                Resolve(attacker, target);
            }

            if (_state == BattleState.Game_BotTurn)
            {
                SetState(BattleState.Game_MyTurn);
                BeginTurn(_me);
            }
        }

        private void Resolve(CardBase attacker, CardBase target)
        {
            if (attacker == null || attacker.IsDead || target == null || target.IsDead) return;

            var step = new BattleStep();
            step.Interaction = attacker.InteractWith(target);
            step.Spawned.AddRange(_me.CleanupAndRefill());
            step.Spawned.AddRange(_bot.CleanupAndRefill());
            OnResolved?.Invoke(step);

            CheckEnd();
        }

        private void CheckEnd()
        {
            if (_bot.IsDefeated) EndBattle(true);
            else if (_me.IsDefeated) EndBattle(false);
        }

        private void EndBattle(bool playerWon)
        {
            SetState(BattleState.End);
            Send(new BattleEndedEvent { PlayerWon = playerWon, Stage = _stage });
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
            Send(new TurnChangedEvent { State = next });
        }

        private void Send<T>(T evt) where T : struct, IGameEvent
        {
            if (EventBus != null)
                EventBus.SendEvent(evt);
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
                CardType.Normal, CardType.Normal, CardType.Ranged,
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
