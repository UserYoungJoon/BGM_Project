using System.Collections.Generic;
using UnityEngine;
using YoungJoon.L0.Events;
using YoungJoon.L2.Battle.Card;

public class CardPlayerBase : MonoBehaviour
{
    public const int FieldSize = 3;

    [SerializeField] private int _playerName;
    [SerializeField] private bool _isBot;
    [SerializeField] private GameObjectEventBus _eventBus;

    private readonly List<CardType> _deck = new List<CardType>();
    private readonly CardBase[] _field = new CardBase[FieldSize];
    private readonly Queue<CardBase> _waiting = new Queue<CardBase>();
    private readonly List<CardBase> _graveyard = new List<CardBase>();

    public GameObjectEventBus EventBus => _eventBus;
    public bool IsBot => _isBot;
    public IReadOnlyList<CardBase> Field => _field;

    public CardBase FieldAt(int slot) => (slot >= 0 && slot < FieldSize) ? _field[slot] : null;

    public int AliveOnField
    {
        get
        {
            int n = 0;
            for (int i = 0; i < FieldSize; i++)
                if (_field[i] != null && !_field[i].IsDead) n++;
            return n;
        }
    }

    public bool IsDefeated => AliveOnField == 0 && _waiting.Count == 0;

    public IReadOnlyList<CardType> Deck => _deck;

    public void SetDeck(IEnumerable<CardType> deck)
    {
        _deck.Clear();
        _deck.AddRange(deck);
    }

    public void AddCard(CardType type) => _deck.Add(type);

    public void InitBattle(CardRegistry registry)
    {
        Clear();

        var built = new List<CardBase>();
        foreach (var type in _deck)
        {
            var card = CardFactory.Create(type);
            if (card == null) continue;
            card.Spawn(registry.GetCardData(type));
            built.Add(card);
        }

        int i = 0;
        for (; i < built.Count && i < FieldSize; i++)
            PlaceOnField(built[i], i);
        for (; i < built.Count; i++)
            _waiting.Enqueue(built[i]);
    }

    public List<CardBase> CleanupAndRefill()
    {
        var spawned = new List<CardBase>();
        for (int i = 0; i < FieldSize; i++)
        {
            if (_field[i] != null && _field[i].IsDead)
            {
                _graveyard.Add(_field[i]);
                _field[i] = null;
            }
            if (_field[i] == null && _waiting.Count > 0)
            {
                var next = _waiting.Dequeue();
                PlaceOnField(next, i);
                spawned.Add(next);
            }
        }
        return spawned;
    }

    public IEnumerable<CardBase> AdjacentAliveCards(int slot)
    {
        var left = FieldAt(slot - 1);
        if (left != null && !left.IsDead) yield return left;
        var right = FieldAt(slot + 1);
        if (right != null && !right.IsDead) yield return right;
    }

    public IEnumerable<CardBase> AliveFieldCards()
    {
        for (int i = 0; i < FieldSize; i++)
            if (_field[i] != null && !_field[i].IsDead)
                yield return _field[i];
    }

    public IEnumerable<CardBase> AliveFieldCardsExcept(CardBase self)
    {
        for (int i = 0; i < FieldSize; i++)
            if (_field[i] != null && !_field[i].IsDead && _field[i] != self)
                yield return _field[i];
    }

    private void PlaceOnField(CardBase card, int slot)
    {
        _field[slot] = card;
        card.Owner = this;
        card.SlotIndex = slot;
    }

    private void Clear()
    {
        for (int i = 0; i < FieldSize; i++) _field[i] = null;
        _waiting.Clear();
        _graveyard.Clear();
    }
}
