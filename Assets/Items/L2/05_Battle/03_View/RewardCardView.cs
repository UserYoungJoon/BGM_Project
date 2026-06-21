using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YoungJoon.L2.Battle.Card;

namespace YoungJoon.L2.Battle.View
{
    public class RewardCardView : MonoBehaviour
    {
        [SerializeField] private Image _bg;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Button _button;

        public void Set(CardDataSO data, System.Action onPick)
        {
            _bg.color = CardColors.Of(data.Type);
            _nameText.text = !string.IsNullOrEmpty(data.CardName) ? data.CardName : data.Type.ToString();
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onPick());
        }
    }
}
