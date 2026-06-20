using System.Collections.Generic;
using UnityEngine;
using YoungJoon.L2.Battle.Card;

public class CardPlayerBase : MonoBehaviour
{
    [SerializeField] private int _playerName;
    [SerializeField] private bool _isBot;
    private List<CardBase> _masterDeck;   // 소유 전체
    private List<CardBase> _drawPile;     // 뽑을 더미
    private List<CardBase> _hand;         // 손패
    private List<CardBase> _exhaustPile;  // 소멸(=무덤)

    public void GetTurn()
    {
        
    }

    public void ReturnTurn()
    {
        
    }
}